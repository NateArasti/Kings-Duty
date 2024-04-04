using System.Collections.Generic;
using Godot;

namespace WorldGeneration
{
	public partial class ChunkGenerator : Node
	{
		[ExportGroup("Area params")]
		[Export] private Vector2I m_AreaCountRange;
		[Export] private Vector2 m_AreaWidthRange;
		[Export] private Vector2 m_AreaHeightRange;
		
		[ExportGroup("Generation Params")]
		[Export] private float m_PointsMinDistance = 50;
		[Export] public Vector2 ChunkSize { get; protected set; } = new Vector2(800, 500);
		
		public virtual ChunkInstance GenerateChunk(
			Vector2I globalIndex, 
			ChunkInstance leftNeighbour = null, 
			ChunkInstance topNeighbour = null,
			ChunkInstance rightNeighbour = null,
			ChunkInstance bottomNeighbour = null)
		{
			var chunkRect = new Rect2(Vector2.Zero, ChunkSize);
			var areaCount = GD.RandRange(m_AreaCountRange.X, m_AreaCountRange.Y);
			var areaRects = new List<Rect2>();
			for (var i = 0; i < areaCount; ++i)
			{
				var size = new Vector2((float)GD.RandRange(m_AreaWidthRange.X, m_AreaWidthRange.Y), (float)GD.RandRange(m_AreaHeightRange.X, m_AreaHeightRange.Y));
				areaRects.Add(new Rect2(Vector2.Zero, size));
			}
			var edgePoints = GetStartPositions(leftNeighbour, topNeighbour, rightNeighbour, bottomNeighbour);
			var points = PoissonSampler.SamplePositions(chunkRect, m_PointsMinDistance, areaRects, edgePoints);
			var edgeGraph = DelaunayTriangulator.TriangulateToEdgeGraph(points);
			var areas = new List<Area>();
			foreach (var areaRect in areaRects)
			{
				areas.Add(new Area()
				{
					Center = areaRect.GetCenter(),
					AreaRect = areaRect,
				});
			}
			
			RemoveBorderEdges(edgeGraph, new Vector2I(areas.Count, areas.Count + edgePoints.Count - 1));

			var edges = new HashSet<DelaunayTriangulator.Edge>();
			
			foreach (var pointEdges in edgeGraph)
			{
				foreach (var edge in pointEdges.Value)
				{
					edges.Add(edge);
				}
			}
			
			return new ChunkInstance(globalIndex, areas, edgePoints, points, edges);
		}
		
		private static void RemoveBorderEdges(IReadOnlyDictionary<int, List<DelaunayTriangulator.Edge>> edgeGraph, Vector2I borderPointsRange)
		{
			var edgesToRemove = new HashSet<DelaunayTriangulator.Edge>();
			foreach (var pointEdges in edgeGraph)
			{
				if (!pointEdges.Key.InRange(borderPointsRange.X, borderPointsRange.Y))
				{
					continue;
				}
				
				edgesToRemove.Clear();
				foreach (var edge in pointEdges.Value)
				{
					if(edge.EndIndex.InRange(borderPointsRange.X, borderPointsRange.Y))
					{
						edgesToRemove.Add(edge);
					}
				}
				
				foreach (var edge in edgesToRemove)
				{
					edgeGraph[edge.StartIndex].Remove(edge);
					edgeGraph[edge.EndIndex].Remove(edge);
				}
			}
		}

		private List<Vector2> GetStartPositions(
			ChunkInstance leftNeighbour = null, 
			ChunkInstance topNeighbour = null,
			ChunkInstance rightNeighbour = null,
			ChunkInstance bottomNeighbour = null)
		{
			var startPositions = new List<Vector2>();
			
			if (leftNeighbour != null)
			{
				foreach (var edgePoint in leftNeighbour.EdgePoints)
				{
					if (edgePoint.X == ChunkSize.X)
					{
						startPositions.Add(new Vector2(0, edgePoint.Y));
					}
				}
			}
			else
			{
				startPositions.AddRange(PoissonSampler.SamplePositions(Vector2.Zero, Vector2.Down * ChunkSize.Y, m_PointsMinDistance));
			}
			
			if (topNeighbour != null)
			{
				foreach (var edgePoint in topNeighbour.EdgePoints)
				{
					if (edgePoint.Y == ChunkSize.Y)
					{
						startPositions.Add(new Vector2(edgePoint.X, 0));
					}
				}
			}
			else
			{
				startPositions.AddRange(PoissonSampler.SamplePositions(Vector2.Zero, Vector2.Right * ChunkSize.X, m_PointsMinDistance));
			}
			
			if (rightNeighbour != null)
			{
				foreach (var edgePoint in rightNeighbour.EdgePoints)
				{
					if (edgePoint.X == 0)
					{
						startPositions.Add(new Vector2(ChunkSize.X, edgePoint.Y));
					}
				}
			}
			else
			{
				startPositions.AddRange(PoissonSampler.SamplePositions(Vector2.Right * ChunkSize.X, ChunkSize, m_PointsMinDistance));
			}
			
			if (bottomNeighbour != null)
			{
				foreach (var edgePoint in bottomNeighbour.EdgePoints)
				{
					if (edgePoint.Y == 0)
					{
						startPositions.Add(new Vector2(edgePoint.X, ChunkSize.Y));
					}
				}
			}
			else
			{
				startPositions.AddRange(PoissonSampler.SamplePositions(Vector2.Down * ChunkSize.Y, ChunkSize, m_PointsMinDistance));
			}

			return startPositions;
		}
	}
}