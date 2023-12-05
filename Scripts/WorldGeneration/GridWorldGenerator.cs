using System.Collections.Generic;
using Godot;

/// <summary>
/// Generates 9 chunks around player at runtime
/// Chunks are set like that
/// 	 <2>
///   <1>	<5>
/// <0>  <4>   <8>
///   <3>   <7>
///      <6>
///      
/// Player is always at center(4) chunk
/// </summary>
public partial class GridWorldGenerator : Node3D
{
	private const int k_RuntimeChunksCount = 9;
	
	[Export] private Node3D m_Player;
	[Export] private float m_MinTimeToGenerateChunk = 2;
	
	[ExportGroup("Grid Generation")]
	[Export] private PackedScene m_GroundTile;
	[Export] private PackedScene m_RoadTile;
	[Export] private Vector2 m_StepValue = Vector2.One;
	[Export] private Vector2 m_XGridDirection = new Vector2(1, -1);
	[Export] private Vector2 m_YGridDirection = new Vector2(1, 1);
	
	[ExportGroup("World Filling")]
	[Export] private PackedScene[] m_Trees;
	[Export] private float m_TreeSpawnMinDistance = 5;
	
	[ExportGroup("World Generation")]
	[Export] private Vector2 m_GridSize = new(20, 20);
	[Export] private float m_CellSize = 1f;
	[Export] private float m_MinDistance = 5;
	
	private WorldGenerator m_WorldGenerator;
	
	private readonly ChunkInstance[] m_CurrentChunks = new ChunkInstance[k_RuntimeChunksCount];
	
	private readonly List<Vector2> m_LeftBorderPositionsBuffer = new();
	private readonly List<Vector2> m_TopBorderPositionsBuffer = new();
	private readonly List<Vector2> m_RightBorderPositionsBuffer = new();
	private readonly List<Vector2> m_BottomBorderPositionsBuffer = new();
	
	private Vector2 m_GlobalOffset;
	
	private Vector2[] m_ChunkOffsets;
	
	private int m_CurrentChunkIndex;
	private float m_CurrentTimeInChunk = 0;
	
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
	
	private Vector2 RectSize => 2 * m_GridSize * m_StepValue;
	
	public override void _Ready()
	{
		m_ChunkOffsets = new Vector2[k_RuntimeChunksCount]
		{
			new Vector2(-1.5f, 0) * RectSize,
			new Vector2(-1f, 0.5f) * RectSize,
			new Vector2(-0.5f, 1) * RectSize,
			new Vector2(-1f, -0.5f) * RectSize,
			new Vector2(-0.5f, 0) * RectSize,
			new Vector2(0, 0.5f) * RectSize,
			new Vector2(-0.5f, -1f) * RectSize,
			new Vector2(0, -0.5f) * RectSize,
			new Vector2(0.5f, 0) * RectSize,
		};
		m_WorldGenerator = new WorldGenerator(m_GridSize, m_CellSize, m_MinDistance);
		for (var i = 0; i < k_RuntimeChunksCount; ++i)
		{
			GenerateChunk(i);			
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
				m_CurrentChunks[i]?.Dispose();
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
				m_CurrentChunks[i]?.Dispose();
				m_CurrentChunks[i] = null;
				GenerateChunk(i);
			}
		}
	}
	
	private int GetPlayerChunk()
	{
		var gridCoords = GetGridCoords(new Vector2(m_Player.GlobalPosition.Z, m_Player.GlobalPosition.X) - m_ChunkOffsets[4] - m_GlobalOffset + Vector2.Right * m_CellSize * m_StepValue.X);
		var chunkCoordinates = (gridCoords / m_GridSize).Floor();
		var chunkInddex = ((int)chunkCoordinates.X + 1) * 3 + (int)chunkCoordinates.Y + 1;
		return Mathf.Clamp(chunkInddex, 0, k_RuntimeChunksCount);
	}

	private void GenerateChunk(int chunkIndex)
	{
		var chunkNode = new Node3D();
		var chunkOffset = m_ChunkOffsets[chunkIndex] + m_GlobalOffset;
		chunkNode.Position = new Vector3(chunkOffset.Y, 0, chunkOffset.X);
		AddChild(chunkNode);
		
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
			chunkNode,
			chunk,
			m_LeftBorderPositionsBuffer.ToArray(),
			m_TopBorderPositionsBuffer.ToArray(),
			m_RightBorderPositionsBuffer.ToArray(),
			m_BottomBorderPositionsBuffer.ToArray()
		);
		
		for (var i = 0; i < chunk.ChunkCells.Count; ++i)
		{
			var coords = Utility.Get2DIndex(i, m_WorldGenerator.Width);
			var spawnCoordinates = GetWorldCoords(coords);
			chunkNode.AddChild(SpawnTile(spawnCoordinates, chunk.ChunkCells[i]));
		}
		
		var possibleTreesPositions = PoissonSampler.SamplePositions(new Rect2(0, -0.5f * RectSize.Y, RectSize), m_TreeSpawnMinDistance);
		foreach (var position in possibleTreesPositions)
		{
			var gridPosition = GetGridCoords(position + Vector2.Right * m_CellSize * m_StepValue.X);
			if(gridPosition.X.InRange(0, m_WorldGenerator.Width - 1)
				&& gridPosition.Y.InRange(0, m_WorldGenerator.Height - 1)
				&& chunk.ChunkCells[Utility.GetFlatIndex(gridPosition.X, gridPosition.Y, m_WorldGenerator.Width)] == WorldGenerator.CellType.Ground)
			{
				chunkNode.AddChild(SpawnTree(position));
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
	
	private Node SpawnTree(Vector2 position)
	{
		var tree = m_Trees[GD.RandRange(0, m_Trees.Length - 1)];
		var instance = tree.Instantiate() as Node3D;
		instance.Position = new Vector3(position.Y, 0, position.X);
		return instance;
	}
	
	private Node SpawnTile(Vector2 position, WorldGenerator.CellType tileType)
	{
		var tile = tileType == WorldGenerator.CellType.Ground ? m_GroundTile : m_RoadTile;
		var instance = tile.Instantiate() as Node3D;
		instance.Position = new Vector3(position.Y, 0, position.X);
		instance.Scale = Vector3.One * m_CellSize;
		return instance;
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
	
	private class ChunkInstance : System.IDisposable
	{
		public Node ChunkPivot { get; }
		
		public WorldGenerator.Chunk Chunk { get; }
		public IReadOnlyList<Vector2> InvertedLeftBorderPositions { get; }
		public IReadOnlyList<Vector2> InvertedTopBorderPositions { get; }
		public IReadOnlyList<Vector2> InvertedRightBorderPositions { get; }
		public IReadOnlyList<Vector2> InvertedBottomBorderPositions { get; }
		
		public ChunkInstance(
			Node pivot,
			WorldGenerator.Chunk chunk,
			IReadOnlyList<Vector2> invertedLeftBorderPositions,
			IReadOnlyList<Vector2> invertedTopBorderPositions,
			IReadOnlyList<Vector2> invertedRightBorderPositions,
			IReadOnlyList<Vector2> invertedBottomBorderPositions
		)
		{
			ChunkPivot = pivot;
			Chunk = chunk;
			InvertedLeftBorderPositions = invertedLeftBorderPositions;
			InvertedTopBorderPositions = invertedTopBorderPositions;
			InvertedRightBorderPositions = invertedRightBorderPositions;
			InvertedBottomBorderPositions = invertedBottomBorderPositions;
		}

		public void Dispose()
		{
			ChunkPivot.QueueFree();
		}
	}
}