using Godot;

namespace WorldGeneration.Tiled
{
	public partial class WorldTilesGenerator : Node, IWorldGenerationSubscriber
	{
		[Export] public bool Enabled { get; private set; } = true;
		
		[Export] private MultiMesh m_WorldTilesMultiMesh;
		[Export] private Basis m_DefaultTileBasis;
		[Export] private Vector2I m_GroundTilesIndexRange;
		[Export] private Vector2I m_RoadTilesIndexRange;
		
		private MultiMeshInstance3D m_TilesMultimesh;
		
		private float m_CellSize;
		private Vector2 m_ChunkRectSize;
		private Vector2I m_ChunkGridSize;
		private Vector2I m_GlobalSize;
		private System.Func<Vector3, Vector2I> GetGridCoords;
		private System.Func<Vector2I, Vector3> GetWorldCoords;

		public void Init(int maxRuntimeChunksCount, Vector2 rectSize, Vector2I chunkGridSize, float cellSize, System.Func<Vector3, Vector2I> getGridCoords, System.Func<Vector2I, Vector3> getWorldCoords)
		{
			GetGridCoords = getGridCoords;
			GetWorldCoords = getWorldCoords;
			
			m_ChunkRectSize = rectSize;
			m_ChunkGridSize = chunkGridSize;
			m_CellSize = cellSize;
			
			m_GlobalSize = (int)Mathf.Sqrt(maxRuntimeChunksCount) * chunkGridSize;
			
			m_TilesMultimesh = new MultiMeshInstance3D
			{
				Multimesh = m_WorldTilesMultiMesh
			};
			AddChild(m_TilesMultimesh);
			var maxRuntimeTilesCount = maxRuntimeChunksCount * m_ChunkGridSize.X * m_ChunkGridSize.Y;			
			var multimesh = m_TilesMultimesh.Multimesh;
			multimesh.InstanceCount = 0;
			multimesh.InstanceCount = maxRuntimeTilesCount;
			multimesh.Mesh.Set("size", Vector2.One * m_CellSize);
		}

		public void OnChunkGenerated(ChunkInstance chunkInstance, Vector3 globalOffset) { }

		public void UpdateAllChunks(ChunkInstance[] allChunksInstances, Vector3 globalOffset)
		{
			var sideChunkCount = (int)Mathf.Sqrt(allChunksInstances.Length);
			for (var i = 0; i < allChunksInstances.Length; i++)
			{
				var tileChunkInstance = allChunksInstances[i] as TileChunkGenerator.TiledChunkInstance;
				var cells = tileChunkInstance.ChunkTiles;
				var chunkCoordinates = Utility.Get2DIndex(i, sideChunkCount);
				var newChunk = false;
				if (tileChunkInstance.TilesCustomData == null)
				{
					newChunk = true;
					tileChunkInstance.SetupCustomData();
				}
				for (var j = 0; j < cells.Length; ++j)
				{
					var coords = Utility.Get2DIndex(j, m_ChunkGridSize.X);
					var globalCoords = coords + chunkCoordinates * m_ChunkGridSize;
					var spawnCoordinates = GetWorldCoords(globalCoords);
					// need to set from top to bottom (y = height - y) to set instances of multimesh in correct draw order
					var index = Utility.GetFlatIndex(globalCoords.X, m_GlobalSize.Y - 1 - globalCoords.Y, m_GlobalSize.X);
					
					int texture_index;
					if (newChunk)
					{
						texture_index = cells[j] == TileChunkGenerator.TileType.Ground ? 
							GD.RandRange(m_GroundTilesIndexRange.X, m_GroundTilesIndexRange.Y) :
							GD.RandRange(m_RoadTilesIndexRange.X, m_RoadTilesIndexRange.Y);
							
						tileChunkInstance.TilesCustomData[j] = texture_index;
					}
					else
					{
						texture_index = tileChunkInstance.TilesCustomData[j];
					}
						
					m_TilesMultimesh.Multimesh.SetInstanceCustomData(index, new Color(texture_index, texture_index, texture_index, texture_index));
					m_TilesMultimesh.Multimesh.SetInstanceTransform(index, new Transform3D(m_DefaultTileBasis, globalOffset + spawnCoordinates));
				}
			}
		}
	}
}
