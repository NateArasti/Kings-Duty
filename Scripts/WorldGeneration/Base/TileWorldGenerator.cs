using System.Collections.Generic;
using Godot;

/// <summary>
/// Generates 9 chunks around player at runtime
/// Chunks are set like that
/// 	 <6>
///   <3>	<7>
/// <0>  <4>   <8>
///   <1>   <5>
///      <2>
///      
/// Player is always at center(4) chunk
/// </summary>
public partial class TileWorldGenerator : Node3D
{
	public const int k_RuntimeChunksCount = 9;
	public const int k_RuntimeChunksSideCount = 3;
	
	[ExportGroup("Grid Generation")]
	[Export] private Vector2 m_StepValue = Vector2.One;
	[Export] private Vector2 m_XGridDirection = new Vector2(1, -1);
	[Export] private Vector2 m_YGridDirection = new Vector2(1, 1);
	
	[ExportGroup("World Tiles")]
	[Export] private MultiMeshInstance3D m_TilesMultimesh;
	[Export] private Basis m_DefaultTileBasis;
	[Export] private Vector2I m_GroundTilesIndexRange;
	[Export] private Vector2I m_RoadTilesIndexRange;
	
	[ExportGroup("World Generation")]
	[Export] private Vector2I m_ChunkGridSize = new(20, 20);
	[Export] private float m_CellSize = 1f;
	[Export] private float m_MinRoadCellDistance = 5;
	[Export] private float m_MinTimeToGenerateChunk = 2;
	
	private Vector2 m_ChunkSize;
	private Vector2 m_RectSize;
	private WorldGenerator m_WorldGenerator;
	
	private Vector2 m_GlobalOffset;
	private Vector2 m_StartOffset;
	
	private Vector2[] m_ChunkOffsets;
	
	private int m_CurrentChunkIndex;
	private float m_CurrentTimeInChunk = 0;
	
	private readonly List<IWorldGenerationSubscriber> m_WorldGenerationSubscribers = new();
	
	private readonly ChunkInstance[] m_CurrentChunks = new ChunkInstance[k_RuntimeChunksCount];
	
	private readonly List<Vector2> m_LeftBorderPositionsBuffer = new();
	private readonly List<Vector2> m_TopBorderPositionsBuffer = new();
	private readonly List<Vector2> m_RightBorderPositionsBuffer = new();
	private readonly List<Vector2> m_BottomBorderPositionsBuffer = new();
	
	private readonly int[][] m_ChunkSubgroups = new int[k_RuntimeChunksCount][]
	{
		new[] { 0, 1, 3, 4 },
		new[] { 0, 1, 2, 3, 4, 5 },
		new[] { 1, 2, 4, 5 },
		new[] { 0, 1, 3, 4, 6, 7 },
		null,
		new[] { 1, 2, 4, 5, 7, 8 },
		new[] { 3, 4, 6, 7 },
		new[] { 3, 4, 5, 6, 7, 8 },
		new[] { 4, 5, 7, 8 },
	};

    public override void _EnterTree()
    {
		m_ChunkSize = (Vector2)m_ChunkGridSize * m_CellSize;
		m_RectSize = 2 * m_ChunkSize * m_StepValue;
		
		var maxRuntimeTilesCount = k_RuntimeChunksCount * m_ChunkGridSize.X * m_ChunkGridSize.Y;
		
		var multimesh = m_TilesMultimesh.Multimesh;
		multimesh.InstanceCount = 0;
		multimesh.InstanceCount = maxRuntimeTilesCount;
		multimesh.Mesh.Set("size", Vector2.One * m_CellSize);	
		
		m_ChunkOffsets = new Vector2[k_RuntimeChunksCount]
		{
			new Vector2(-1.5f, 0) * m_RectSize,
			new Vector2(-1f, -0.5f) * m_RectSize,
			new Vector2(-0.5f, -1f) * m_RectSize,
			new Vector2(-1f, 0.5f) * m_RectSize,
			new Vector2(-0.5f, 0) * m_RectSize,
			new Vector2(0, -0.5f) * m_RectSize,
			new Vector2(-0.5f, 1) * m_RectSize,
			new Vector2(0, 0.5f) * m_RectSize,
			new Vector2(0.5f, 0) * m_RectSize,
		};
		
		m_StartOffset = m_ChunkOffsets[0];
		m_GlobalOffset = m_StartOffset;
		
		foreach (var child in GetChildren())
		{
			if (child is IWorldGenerationSubscriber worldGenerationSubscriber)
			{
				worldGenerationSubscriber.Initialize(m_ChunkGridSize, m_RectSize, GetGridCoords, GetWorldCoords);
				m_WorldGenerationSubscribers.Add(worldGenerationSubscriber);
			}
		}
    }

    public override void _Ready()
	{
		m_WorldGenerator = new WorldGenerator(m_ChunkGridSize, m_CellSize, m_MinRoadCellDistance);
		GenerateWorld();
	}
	
	private void GenerateWorld()
	{
		for (var i = 0; i < k_RuntimeChunksCount; ++i)
		{
			if(m_CurrentChunks[i] != null) continue;
			GenerateChunk(i);
		}
		RegenerateMultiMesh();
	}
	
	private void RegenerateMultiMesh()
	{
		// need to look from upside down (y = height - y) to set instances of multimesh in correct draw order
		var globalSize = k_RuntimeChunksSideCount * m_ChunkGridSize;

		var worldOffset = new Vector3(m_GlobalOffset.Y, 0, m_GlobalOffset.X);
		for (var i = 0; i < k_RuntimeChunksCount; i++)
		{
			var chunk = m_CurrentChunks[i].Chunk;
			var cells = chunk.ChunkCells;
			var chunkCoordinates = Utility.Get2DIndex(i, k_RuntimeChunksSideCount);
			var newChunk = false;
			if (m_CurrentChunks[i].TilesTextures == null)
			{
				newChunk = true;
				m_CurrentChunks[i].TilesTextures = new();
			}
			for (var j = 0; j < cells.Count; ++j)
			{
				var coords = Utility.Get2DIndex(j, m_WorldGenerator.Width);
				var globalCoords = coords + chunkCoordinates * m_ChunkGridSize;
				var spawnCoordinates = GetWorldCoords(globalCoords);
				var index = Utility.GetFlatIndex(globalCoords.X, globalSize.Y - 1 - globalCoords.Y, globalSize.X);

				int texture_index;
				if (newChunk)
				{
					texture_index = cells[j] == WorldGenerator.CellType.Ground ? 
						GD.RandRange(m_GroundTilesIndexRange.X, m_GroundTilesIndexRange.Y) : 
						GD.RandRange(m_RoadTilesIndexRange.X, m_RoadTilesIndexRange.Y);
						
					m_CurrentChunks[i].TilesTextures.Add(texture_index);
				}
				else
				{
					texture_index = m_CurrentChunks[i].TilesTextures[j];
				}
					
				m_TilesMultimesh.Multimesh.SetInstanceCustomData(index, new Color(texture_index, texture_index, texture_index, texture_index));
				m_TilesMultimesh.Multimesh.SetInstanceTransform(index, new Transform3D(m_DefaultTileBasis, worldOffset + new Vector3(spawnCoordinates.Y, 0, spawnCoordinates.X) - m_StepValue.X * Vector3.Forward));
			}
		}
	}

	public override void _Process(double delta)
	{
		var chunkIndex = GetPlayerChunk();
		
		if(m_CurrentChunkIndex != chunkIndex)
		{
			m_CurrentTimeInChunk = 0;			
		}
		
		m_CurrentChunkIndex = chunkIndex;
		
		if(m_CurrentChunkIndex != 4)
		{
			m_CurrentTimeInChunk += (float)delta;
		}
		
		if(m_CurrentTimeInChunk > m_MinTimeToGenerateChunk)
		{
			m_CurrentTimeInChunk = 0;
			
			RegenerateChunksAround(m_CurrentChunkIndex);
		}
	}
	
	private void RegenerateChunksAround(int newCenterChunkIndex)
	{
		m_GlobalOffset += m_ChunkOffsets[newCenterChunkIndex] - m_ChunkOffsets[4];
		
		var indexOffset = 4 - newCenterChunkIndex;
		var generatedChunks = new bool[k_RuntimeChunksCount];
		var subGroup = m_ChunkSubgroups[newCenterChunkIndex];
		
		var groupIndex = 0;
		for (var i = 0; i < k_RuntimeChunksCount; ++i)
		{
			if (groupIndex < subGroup.Length && subGroup[groupIndex] == i)
			{
				groupIndex++;
			}
			else
			{
				m_CurrentChunks[i]?.DiscardChunk();
				m_CurrentChunks[i] = null;
			}
		}
		
		if (Mathf.Sign(indexOffset) > 0)
		{
			for (int i = subGroup.Length - 1; i >= 0; i--)
			{
				m_CurrentChunks[subGroup[i] + indexOffset] = m_CurrentChunks[subGroup[i]];
				generatedChunks[subGroup[i] + indexOffset] = true;
				m_CurrentChunks[subGroup[i]] = null;
			}
		}
		else
		{
			for (int i = 0; i < subGroup.Length; i++)
			{
				m_CurrentChunks[subGroup[i] + indexOffset] = m_CurrentChunks[subGroup[i]];
				generatedChunks[subGroup[i] + indexOffset] = true;
				m_CurrentChunks[subGroup[i]] = null;
			}
		}
		
		for (var i = 0; i < k_RuntimeChunksCount; ++i)
		{
			if(!generatedChunks[i])
			{		
				m_CurrentChunks[i]?.DiscardChunk();
				m_CurrentChunks[i] = null;
				GenerateChunk(i);
			}
		}
		
		RegenerateMultiMesh();
	}
	
	private int GetPlayerChunk()
	{
		var player = PlayerGlobalController.Instance.Player;
		var gridCoords = GetGridCoords(new Vector2(player.GlobalPosition.Z, player.GlobalPosition.X) - m_ChunkOffsets[4] - (m_GlobalOffset - m_StartOffset));
		var chunkCoordinates = (gridCoords / (Vector2)m_ChunkGridSize).Floor();
		var chunkIndex = (int)(chunkCoordinates.Y + 1) * k_RuntimeChunksSideCount + (int)(chunkCoordinates.X + 1);
		return Mathf.Clamp(chunkIndex, 0, k_RuntimeChunksCount);
	}

	private void GenerateChunk(int chunkIndex)
	{
		var chunkOffset = m_ChunkOffsets[chunkIndex] + m_GlobalOffset - m_StartOffset;
		var worldOffset = new Vector3(chunkOffset.Y, 0, chunkOffset.X);
		
		var chunk = chunkIndex switch
		{
			0 => m_WorldGenerator.GenerateChunk(null, m_CurrentChunks[1]?.InvertedBottomBorderPositions, m_CurrentChunks[3]?.InvertedLeftBorderPositions, null),
			1 => m_WorldGenerator.GenerateChunk(null, m_CurrentChunks[2]?.InvertedBottomBorderPositions, m_CurrentChunks[4]?.InvertedLeftBorderPositions, m_CurrentChunks[0]?.InvertedTopBorderPositions),
			2 => m_WorldGenerator.GenerateChunk(null, null, m_CurrentChunks[5]?.InvertedLeftBorderPositions, m_CurrentChunks[1]?.InvertedTopBorderPositions),
			3 => m_WorldGenerator.GenerateChunk(m_CurrentChunks[0]?.InvertedRightBorderPositions, m_CurrentChunks[4]?.InvertedBottomBorderPositions, m_CurrentChunks[6]?.InvertedLeftBorderPositions, null),
			4 => m_WorldGenerator.GenerateChunk(m_CurrentChunks[1]?.InvertedRightBorderPositions, m_CurrentChunks[5]?.InvertedBottomBorderPositions, m_CurrentChunks[7]?.InvertedLeftBorderPositions, m_CurrentChunks[3]?.InvertedTopBorderPositions),
			5 => m_WorldGenerator.GenerateChunk(m_CurrentChunks[2]?.InvertedRightBorderPositions, null, m_CurrentChunks[8]?.InvertedLeftBorderPositions, m_CurrentChunks[4]?.InvertedTopBorderPositions),
			6 => m_WorldGenerator.GenerateChunk(m_CurrentChunks[3]?.InvertedRightBorderPositions, m_CurrentChunks[7]?.InvertedBottomBorderPositions, null, null),
			7 => m_WorldGenerator.GenerateChunk(m_CurrentChunks[4]?.InvertedRightBorderPositions, m_CurrentChunks[8]?.InvertedBottomBorderPositions, null, m_CurrentChunks[6]?.InvertedTopBorderPositions),
			8 => m_WorldGenerator.GenerateChunk(m_CurrentChunks[5]?.InvertedRightBorderPositions, null, null, m_CurrentChunks[7]?.InvertedTopBorderPositions),
			_ => throw new System.NotImplementedException(),
		};
		FillEdgeBuffers(chunk);
		
		m_CurrentChunks[chunkIndex] = new(
			chunk,
			m_LeftBorderPositionsBuffer.ToArray(),
			m_TopBorderPositionsBuffer.ToArray(),
			m_RightBorderPositionsBuffer.ToArray(),
			m_BottomBorderPositionsBuffer.ToArray()
		);
		
		foreach (var subsciber in m_WorldGenerationSubscribers)
		{
			if(subsciber.Enabled)
			{
				subsciber.OnChunkGenerated(m_CurrentChunks[chunkIndex], worldOffset);
			}
		}
	}
	
	private void FillEdgeBuffers(WorldGenerator.Chunk chunk)
	{
		m_LeftBorderPositionsBuffer.Clear();
		m_TopBorderPositionsBuffer.Clear();
		m_RightBorderPositionsBuffer.Clear();
		m_BottomBorderPositionsBuffer.Clear();
		
		foreach (var cell in chunk.EdgeCells)
		{
			var cellCoords = cell;
			if(cell.X == 0)
			{
				cellCoords.X = m_WorldGenerator.Width - 1;
				var position = (Vector2)cellCoords * m_CellSize;
				m_LeftBorderPositionsBuffer.Add(position);
			}
			else if (cell.Y == 0)
			{
				cellCoords.Y = m_WorldGenerator.Height - 1;
				var position = (Vector2)cellCoords * m_CellSize;
				m_TopBorderPositionsBuffer.Add(position);			
			}
			else if (cell.X == m_WorldGenerator.Width - 1)
			{
				cellCoords.X = 0;
				var position = (Vector2)cellCoords * m_CellSize;
				m_RightBorderPositionsBuffer.Add(position);			
			}
			else if (cell.Y == m_WorldGenerator.Height - 1)
			{
				cellCoords.Y = 0;
				var position = (Vector2)cellCoords * m_CellSize;
				m_BottomBorderPositionsBuffer.Add(position);
			}
			else
			{
				GD.PrintErr($"WTF? {cell} is not edge cell");
			}
		}
	}
	
	private Vector2 GetWorldCoords(Vector2I gridCoords)
	{
		return m_CellSize * m_StepValue * (gridCoords.X * m_XGridDirection + gridCoords.Y * m_YGridDirection);
	}
	
	private Vector2I GetGridCoords(Vector2 worldCoords)
	{
		var c = worldCoords / (m_CellSize * m_StepValue);
		var (x, y) = MathExtensions.Solve(m_XGridDirection.X, m_YGridDirection.X, c.X, m_XGridDirection.Y, m_YGridDirection.Y, c.Y);
		return new Vector2I(Mathf.FloorToInt(x), Mathf.FloorToInt(y));
	}
	
	public class ChunkInstance
	{
		public event System.Action OnChunkDiscard;
		
		public WorldGenerator.Chunk Chunk { get; }
		public IReadOnlyList<Vector2> InvertedLeftBorderPositions { get; }
		public IReadOnlyList<Vector2> InvertedTopBorderPositions { get; }
		public IReadOnlyList<Vector2> InvertedRightBorderPositions { get; }
		public IReadOnlyList<Vector2> InvertedBottomBorderPositions { get; }
		
		public List<int> TilesTextures;
		
		public ChunkInstance(
			WorldGenerator.Chunk chunk,
			IReadOnlyList<Vector2> invertedLeftBorderPositions,
			IReadOnlyList<Vector2> invertedTopBorderPositions,
			IReadOnlyList<Vector2> invertedRightBorderPositions,
			IReadOnlyList<Vector2> invertedBottomBorderPositions
		)
		{
			Chunk = chunk;
			InvertedLeftBorderPositions = invertedLeftBorderPositions;
			InvertedTopBorderPositions = invertedTopBorderPositions;
			InvertedRightBorderPositions = invertedRightBorderPositions;
			InvertedBottomBorderPositions = invertedBottomBorderPositions;
		}
		
		public void DiscardChunk()
		{
			OnChunkDiscard?.Invoke();
		}
	}
}