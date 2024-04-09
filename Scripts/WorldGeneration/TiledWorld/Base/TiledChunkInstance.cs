namespace WorldGeneration.Tiled
{
	public class TiledChunkInstance : ChunkInstance
	{
		public TileType[] ChunkTiles { get; }
		public int[] TilesCustomData { get; private set; }
		
		public TiledChunkInstance(ChunkInstance chunkInstance, TileType[] chunkTiles) 
			: base(chunkInstance.Index, chunkInstance.Areas, chunkInstance.EdgePoints, chunkInstance.AllPoints, chunkInstance.Roads)
		{
			ChunkTiles = chunkTiles;
		}
		
		public void SetupCustomData()
		{
			TilesCustomData = new int[ChunkTiles.Length];
		}
	}
}