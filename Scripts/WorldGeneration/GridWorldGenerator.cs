using System.Collections.Generic;
using System.Diagnostics;
using Godot;

public partial class GridWorldGenerator : Node3D
{
	private const int k_RuntimeChunksCount = 9;
	
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
	
	public override void _Ready()
	{
		m_WorldGenerator = new WorldGenerator(m_GridSize, m_CellSize, m_MinDistance);
	}

	public override void _Input(InputEvent @event)
	{
		if(@event is InputEventMouseButton inputEventMouse &&
			inputEventMouse.ButtonIndex == MouseButton.Left && inputEventMouse.IsPressed())
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			Generate();
			stopwatch.Stop();
			GD.Print(stopwatch.ElapsedMilliseconds);
		}
	}
	
	private void Generate()
	{
		ClearAllChildren();
		var rectSize = 2 * m_GridSize * m_StepValue;
		var chunkOffsets = new Vector2[k_RuntimeChunksCount]
		{
			new Vector2(-1.5f, 0) * rectSize,
			new Vector2(-1f, 0.5f) * rectSize,
			new Vector2(-0.5f, 1) * rectSize,
			new Vector2(-1f, -0.5f) * rectSize,
			new Vector2(-0.5f, 0) * rectSize,
			new Vector2(0, 0.5f) * rectSize,
			new Vector2(-0.5f, -1f) * rectSize,
			new Vector2(0, -0.5f) * rectSize,
			new Vector2(0.5f, 0) * rectSize,
		};
		for (var k = 0; k < k_RuntimeChunksCount; ++k)
		{
			var chunkNode = new Node3D();
			var chunkOffset = chunkOffsets[k];
			chunkNode.Position = new Vector3(chunkOffset.Y, 0, chunkOffset.X);
			AddChild(chunkNode);
			
			var chunk = k switch
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
			
			m_CurrentChunks[k] = new(
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
			
			var possibleTreesPositions = PoissonSampler.SamplePositions(new Rect2(0, -0.5f * rectSize.Y, rectSize), m_TreeSpawnMinDistance);
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

	private void ClearAllChildren()
	{
		foreach(var child in GetChildren())
		{
			child.QueueFree();
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
	
	private class ChunkInstance
	{
		public WorldGenerator.Chunk Chunk { get; }
		public IReadOnlyList<Vector2> InvertedLeftBorderPositions { get; }
		public IReadOnlyList<Vector2> InvertedTopBorderPositions { get; }
		public IReadOnlyList<Vector2> InvertedRightBorderPositions { get; }
		public IReadOnlyList<Vector2> InvertedBottomBorderPositions { get; }
		
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
	}
}