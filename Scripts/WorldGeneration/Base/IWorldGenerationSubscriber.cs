using Godot;

public interface IWorldGenerationSubscriber
{
	bool Enabled { get; }
	void Initialize(Vector2I chunkGridSize, Vector2 chunkRectSize, System.Func<Vector2, Vector2I> getGridCoords, System.Func<Vector2I, Vector2> getWorldCoords);	
	void OnChunkGenerated(TileWorldGenerator.ChunkInstance chunkInstance, Vector3 worldOffset);
}
