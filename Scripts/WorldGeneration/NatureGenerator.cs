using Godot;
using System.Collections.Generic;

public partial class NatureGenerator : Node, IWorldGenerationSubscriber
{
	[Export] public bool Enabled { get; private set; } = true;
	
	[ExportGroup("Generation Data")]
	[Export] private float m_TreeSpawnMinDistance = 1;
	[Export] private int m_MaxChunkTreeCount = 100;
	[Export] private Vector2I m_TreesTextureRange;
	[Export] private Mesh m_TreeMeshData;
	[Export] private Basis m_DefaultTreeBasis;
	[Export] private PackedScene m_ObstacleScene;
	
	private Vector2I m_ChunkGridSize;
	private Vector2 m_ChunkRectSize;
	private System.Func<Vector2, Vector2I> GetGridCoords;
	private System.Func<Vector2I, Vector2> GetWorldCoords;
	
	private NodePool<Node3D> m_ObstaclesPool;
	private readonly Queue<MultiMeshInstance3D> m_FreeNatureMultimeshInstances = new();

	public override void _Ready()
	{
		m_ObstaclesPool = new NodePool<Node3D>(CreateObstacle, 5000, PoolGetCallback, PoolReturnCallback);
		
		for (var i = 0; i < TileWorldGenerator.k_RuntimeChunksCount; ++i)
		{
			var instance = new MultiMeshInstance3D();
			var multiMesh = new MultiMesh
			{
				Mesh = m_TreeMeshData,
				UseCustomData = true,
				VisibleInstanceCount = 0,
				TransformFormat = MultiMesh.TransformFormatEnum.Transform3D
			};
			instance.Multimesh = multiMesh;
			m_FreeNatureMultimeshInstances.Enqueue(instance);
			multiMesh.InstanceCount = m_MaxChunkTreeCount;
			AddChild(instance);
		}
	}

	public void Initialize(Vector2I chunkGridSize, Vector2 chunkRectSize, 
		System.Func<Vector2, Vector2I> getGridCoords, System.Func<Vector2I, Vector2> getWorldCoords)
	{
		m_ChunkGridSize = chunkGridSize;
		m_ChunkRectSize = chunkRectSize;
		
		GetGridCoords = getGridCoords;
		GetWorldCoords = getWorldCoords;
	}

	public void OnChunkGenerated(TileWorldGenerator.ChunkInstance chunkInstance, Vector3 worldOffset)
	{
		var obstacles = new HashSet<Node3D>();
		var natureMultimeshInstance = m_FreeNatureMultimeshInstances.Dequeue();
		
		SpawnNature(chunkInstance.Chunk, obstacles, worldOffset, natureMultimeshInstance.Multimesh);
		chunkInstance.OnChunkDiscard += () => 
		{
			m_FreeNatureMultimeshInstances.Enqueue(natureMultimeshInstance);
			foreach (var obstacle in obstacles)
			{
				m_ObstaclesPool.Return(obstacle);
			}
		};
	}
	
	private void SpawnNature(WorldGenerator.Chunk chunk, HashSet<Node3D> obstacles, Vector3 worldOffset, MultiMesh multiMesh)
	{
		var possibleTreesPositions = PoissonSampler.SamplePositions(
			new Rect2(0, -0.5f * m_ChunkRectSize.Y, m_ChunkRectSize),
			m_TreeSpawnMinDistance,
			maxSearchIterionsCount: 5
		);
		
		var index = 0;
		foreach (var position in possibleTreesPositions)
		{
			var gridPosition = GetGridCoords(position);
			if(gridPosition.X.InRange(0, m_ChunkGridSize.X - 1)
				&& gridPosition.Y.InRange(0, m_ChunkGridSize.Y - 1)
				&& chunk.ChunkCells[Utility.GetFlatIndex(gridPosition.X, gridPosition.Y, m_ChunkGridSize.X)] == WorldGenerator.CellType.Ground)
			{
				var obstacle = SpawnObstacle(position);
				obstacle.Position += worldOffset;
				multiMesh.SetInstanceTransform(index, new Transform3D(m_DefaultTreeBasis, obstacle.GlobalPosition));
				multiMesh.SetInstanceCustomData(index, new Color(GD.RandRange(m_TreesTextureRange.X, m_TreesTextureRange.Y), 0, 0, 0));
				index++;
				obstacles.Add(obstacle);
				
				if(index >= m_MaxChunkTreeCount) break;
			}
		}
		
		multiMesh.VisibleInstanceCount = index;
	}
	
	private Node3D SpawnObstacle(Vector2 position)
	{
		if(m_ObstaclesPool.TryGet(out var instance))
		{
			instance.Position = new Vector3(position.Y, 0, position.X);
			instance.Show();
		}
		return instance;
	}
	
	private Node3D CreateObstacle()
	{
		var instance = m_ObstacleScene.Instantiate<Node3D>();
		AddChild(instance);
		return instance;
	}
	
	private void PoolGetCallback(Node3D instance)
	{
		instance.Show();
		instance.ProcessMode = ProcessModeEnum.Inherit;
	}
	
	private void PoolReturnCallback(Node3D instance)
	{
		instance.Hide();
		instance.ProcessMode = ProcessModeEnum.Disabled;
	}
}
