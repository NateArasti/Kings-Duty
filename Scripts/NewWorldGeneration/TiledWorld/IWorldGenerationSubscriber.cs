using Godot;

namespace WorldGeneration.Tiled
{
	public interface IWorldGenerationSubscriber
	{
		void Init(
			int maxRuntimeChunksCount, 
			Vector2I chunkGridSize, 
			float cellSize, 
			System.Func<Vector3, Vector2I> getGridCoords, 
			System.Func<Vector2I, Vector3> getWorldCoords);
		
		void OnChunkGenerated(ChunkInstance chunkInstance, Vector3 globalOffset);
		
		void UpdateAllChunks(ChunkInstance[] allChunksInstances, Vector3 globalOffset);
	}	
}
