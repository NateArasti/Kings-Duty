using System.Collections.Generic;
using Godot;

namespace WorldGeneration
{
	public class ChunkInstance
	{
		public int Index { get; }
		public IReadOnlyList<Area> Areas { get; }
		public IReadOnlyList<Vector2> Points { get; }
		public IReadOnlyCollection<DelaunayTriangulator.Edge> Roads { get; }
		
		public ChunkInstance(
			int index, 
			IReadOnlyList<Area> areas, 
			IReadOnlyList<Vector2> points, 
			IReadOnlyCollection<DelaunayTriangulator.Edge> roads)
		{
			Index = index;
			Areas = areas;
			Points = points;
			Roads = roads;
		}
	}
}