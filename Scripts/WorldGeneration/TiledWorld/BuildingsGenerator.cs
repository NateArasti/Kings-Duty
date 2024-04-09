using System.Collections.Generic;
using Godot;

namespace WorldGeneration.Tiled
{
	public partial class BuildingsGenerator : Node, IWorldGenerationSubscriber
	{
		[Export] public bool Enabled { get; private set; } = true;
		[ExportGroup("Generation Data")]
		[Export] private PackedScene m_StatueScene;
		
		private float m_CellSize;
		private Vector2 m_ChunkRectSize;
		private Vector2I m_ChunkGridSize;
		private Vector2I m_GlobalSize;
		private System.Func<Vector3, Vector2I> GetGridCoords;
		private System.Func<Vector2I, Vector3> GetWorldCoords;

		public void Init(
			int maxRuntimeChunksCount, 
			Vector2 rectSize, 
			Vector2I chunkGridSize, 
			float cellSize, 
			System.Func<Vector3, Vector2I> getGridCoords, 
			System.Func<Vector2I, Vector3> getWorldCoords)
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
			var statues = new HashSet<Node3D>();
			
			foreach (var area in chunkInstance.Areas)
			{
				if (area is StatueArea statueArea)
				{
					var gridPosition = Utility.GetGridPosition(area.Center, Vector2.Zero, m_CellSize);
					var position = GetWorldCoords(gridPosition);
					var prop = CreateStatue(position);
					prop.Position += chunkOffset;
					statues.Add(prop);
				}
			}
			
			chunkInstance.OnChunkDiscard += () => 
			{
				foreach (var statue in statues)
				{
					statue.QueueFree();
				}
			};
		}
	
		private Node3D CreateStatue(Vector3 position)
		{
			var instance = m_StatueScene.Instantiate<Node3D>();
			instance.Position = position;
			AddChild(instance);
			return instance;
		}

		public void UpdateAllChunks(ChunkInstance[] allChunksInstances, Vector3 globalOffset) { }
	}
}
