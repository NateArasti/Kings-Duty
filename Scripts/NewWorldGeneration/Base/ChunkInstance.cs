using System.Collections.Generic;
using Godot;

namespace WorldGeneration
{
	public class ChunkInstance
	{
		public int Index { get; }
		public IReadOnlyList<Area> Areas { get; }
		public IReadOnlyList<Vector2> EdgePoints { get; }
		public IReadOnlyList<Vector2> AllPoints { get; }
		public IReadOnlyCollection<DelaunayTriangulator.Edge> Roads { get; }
		
		public ChunkInstance(
			int index, 
			IReadOnlyList<Area> areas, 
			IReadOnlyList<Vector2> edgePoints,
			IReadOnlyList<Vector2> allPoints, 
			IReadOnlyCollection<DelaunayTriangulator.Edge> roads)
		{
			Index = index;
			Areas = areas;
			EdgePoints = edgePoints;
			AllPoints = allPoints;
			Roads = roads;
		}
	}
}