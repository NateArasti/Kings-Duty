using Godot;

namespace WorldGeneration.Tiled
{
	public interface IWorldGenerationSubscriber
	{
		bool Enabled { get; }
		
		void Init(
			int maxRuntimeChunksCount, 
			Vector2 rectSize,
			Vector2I chunkGridSize, 
			float cellSize, 
			System.Func<Vector3, Vector2I> getGridCoords, 
			System.Func<Vector2I, Vector3> getWorldCoords);
		
		void OnChunkGenerated(ChunkInstance chunkInstance, Vector3 chunkOffset);
		
		void UpdateAllChunks(ChunkInstance[] allChunksInstances, Vector3 globalOffset);
	}	
}
