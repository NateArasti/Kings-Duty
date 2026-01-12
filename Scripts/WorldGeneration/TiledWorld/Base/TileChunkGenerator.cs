using Godot;

namespace WorldGeneration.Tiled
{
    public partial class TileChunkGenerator : ChunkGenerator
	{
		[ExportGroup("Tile Data")]
		[Export] public Vector2I TilesCountPerChunk { get; private set;}
		[Export] public float CellSize { get; private set; }

		public override void _EnterTree()
		{
			base._EnterTree();
			ChunkSize = (Vector2)TilesCountPerChunk * CellSize;
		}

		public override ChunkInstance GenerateChunk(Vector2I globalIndex, ChunkInstance leftNeighbour = null, ChunkInstance topNeighbour = null, ChunkInstance rightNeighbour = null, ChunkInstance bottomNeighbour = null)
		{
			var chunkInstance = base.GenerateChunk(
				globalIndex, 
				leftNeighbour, 
				topNeighbour, 
				rightNeighbour, 
				bottomNeighbour);
			
			var chunkTiles = new TileType[TilesCountPerChunk.X * TilesCountPerChunk.Y];
			
			for (var i = 0; i < chunkTiles.Length; ++i)
			{
				chunkTiles[i] = TileType.Ground;
			}
			
			foreach (var road in chunkInstance.Roads)
			{
				var start = chunkInstance.AllPoints[road.StartIndex];
				var end = chunkInstance.AllPoints[road.EndIndex];
				const int resoultion = 1000;
				for (var i = 0f; i < resoultion; ++i)
				{
					var point = start.Lerp(end, i / resoultion);
					var gridPosition = Utility.GetGridPosition(point, Vector2.Zero, CellSize);
					var index = Utility.GetFlatIndex(
						Mathf.Clamp(gridPosition.X, 0, TilesCountPerChunk.X - 1), 
						Mathf.Clamp(gridPosition.Y, 0, TilesCountPerChunk.Y - 1), 
						TilesCountPerChunk.X);
					chunkTiles[index] = TileType.Road;
				}
			}
			
			foreach (var area in chunkInstance.Areas)
			{
				var start = Utility.GetGridPosition(area.Center - 0.5f * area.Size, Vector2.Zero, CellSize);
				var end = Utility.GetGridPosition(area.Center + 0.5f * area.Size, Vector2.Zero, CellSize);
				for (var x = start.X; x <= end.X; ++x)
				{
					for (var y = start.Y; y <= end.Y; ++y)
					{
						var index = Utility.GetFlatIndex(x, y, TilesCountPerChunk.X);
						chunkTiles[index] = TileType.Area;
					}
				}
			}
			
			return new TiledChunkInstance(chunkInstance, chunkTiles);
		}
	}
}