using System.Collections.Generic;
using Godot;

namespace WorldGeneration.Tiled
{
	public partial class PropsGenerator : Node, IWorldGenerationSubscriber
	{
		[Export] public bool Enabled { get; private set; } = true;
		
		[ExportGroup("Generation Data")]
		[Export] private float m_TreeSpawnMinDistance = 1;
		[Export] private int m_MaxChunkTreeCount = 100;
		[Export] private Vector2I m_TreesTextureRange;
		[Export] private Mesh m_TreeMeshData;
		[Export] private Basis m_DefaultTreeBasis;
		[Export] private PackedScene m_ObstacleScene;
		
		private float m_CellSize;
		private Vector2 m_ChunkRectSize;
		private Vector2I m_ChunkGridSize;
		private Vector2I m_GlobalSize;
		private System.Func<Vector3, Vector2I> GetGridCoords;
		private System.Func<Vector2I, Vector3> GetWorldCoords;
		
		private NodePool<Node3D> m_ObstaclesPool;
		private readonly Queue<MultiMeshInstance3D> m_FreeNatureMultimeshInstances = new();

		public override void _Ready()
		{
			GD.Print(m_DefaultTreeBasis);
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
		
		public void Init(int maxRuntimeChunksCount, Vector2 rectSize, Vector2I chunkGridSize, float cellSize, System.Func<Vector3, Vector2I> getGridCoords, System.Func<Vector2I, Vector3> getWorldCoords)
		{
			GetGridCoords = getGridCoords;
			GetWorldCoords = getWorldCoords;
			
			m_ChunkRectSize = rectSize;
			m_ChunkGridSize = chunkGridSize;
			m_CellSize = cellSize;
			
			m_GlobalSize = (int)Mathf.Sqrt(maxRuntimeChunksCount) * chunkGridSize;
		}

		public void OnChunkGenerated(ChunkInstance chunkInstance, Vector3 chunkOffset)
		{
			var obstacles = new HashSet<Node3D>();
			var natureMultimeshInstance = m_FreeNatureMultimeshInstances.Dequeue();
			
			SpawnNature(chunkInstance as TileChunkGenerator.TiledChunkInstance, obstacles, chunkOffset, natureMultimeshInstance.Multimesh);
			chunkInstance.OnChunkDiscard += () => 
			{
				m_FreeNatureMultimeshInstances.Enqueue(natureMultimeshInstance);
				foreach (var obstacle in obstacles)
				{
					m_ObstaclesPool.Return(obstacle);
				}
			};
		}

		public void UpdateAllChunks(ChunkInstance[] allChunksInstances, Vector3 globalOffset) { }
		
		private void SpawnNature(TileChunkGenerator.TiledChunkInstance tiledChunkInstance, HashSet<Node3D> obstacles, Vector3 chunkOffset, MultiMesh multiMesh)
		{
			var possibleTreesPositions = PoissonSampler.SamplePositions(
				new Rect2(0, -0.5f * m_ChunkRectSize.Y, m_ChunkRectSize),
				m_TreeSpawnMinDistance,
				maxSearchIterionsCount: 5
			);
			
			var index = 0;
			foreach (var position in possibleTreesPositions)
			{
				var gridPosition = GetGridCoords(WorldController.Convert2DTo3D(position));
				if(gridPosition.X.InRange(0, m_ChunkGridSize.X - 1)
					&& gridPosition.Y.InRange(0, m_ChunkGridSize.Y - 1)
					&& tiledChunkInstance.ChunkTiles[Utility.GetFlatIndex(gridPosition.X, gridPosition.Y, m_ChunkGridSize.X)] == TileChunkGenerator.TileType.Ground)
				{
					var obstacle = SpawnObstacle(position);
					obstacle.Position += chunkOffset;
					multiMesh.SetInstanceTransform(index, new Transform3D(m_DefaultTreeBasis, obstacle.Position));
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
				instance.Position = WorldController.Convert2DTo3D(position);
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
}
