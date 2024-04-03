using System;
using System.Collections.Generic;
using Godot;

namespace WorldGeneration
{
	public partial class ChunkGenerator : Node
	{
		[Export] private float m_PointsMinDistance = 50;
		[Export] private Vector2I m_AreaCountRange;
		[Export] private Vector2 m_AreaWidthRange;
		[Export] private Vector2 m_AreaHeightRange;
		
		[Export] public Vector2 ChunkSize { get; private set; } = new Vector2(800, 500);
		
		public ChunkInstance GenerateChunk(int globalIndex)
		{
			var chunkRect = new Rect2(Vector2.Zero, ChunkSize);
			var areaCount = GD.RandRange(m_AreaCountRange.X, m_AreaCountRange.Y);
			var areaRects = new List<Rect2>();
			for (var i = 0; i < areaCount; ++i)
			{
				var size = new Vector2((float)GD.RandRange(m_AreaWidthRange.X, m_AreaWidthRange.Y), (float)GD.RandRange(m_AreaHeightRange.X, m_AreaHeightRange.Y));
				areaRects.Add(new Rect2(Vector2.Zero, size));
			}
			var edgePoints = GetStartPositions(null, null, null, null);
			var points = PoissonSampler.SamplePositions(chunkRect, m_PointsMinDistance, areaRects, edgePoints);
			var edgeGraph = DelaunayTriangulator.TriangulateToEdgeGraph(points);
			var areas = new List<Area>();
			foreach (var areaRect in areaRects)
			{
				areas.Add(new Area()
				{
					AreaRect = areaRect,
				});
			}
			
			// removing border edges
			var edgesToRemove = new HashSet<DelaunayTriangulator.Edge>();
			foreach (var pointEdges in edgeGraph)
			{
				if (!pointEdges.Key.InRange(areas.Count, areas.Count + edgePoints.Count - 1))
				{
					continue;
				}
				
				edgesToRemove.Clear();
				foreach (var edge in pointEdges.Value)
				{
					if(edge.EndIndex.InRange(areas.Count, areas.Count + edgePoints.Count - 1))
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

			var edges = new HashSet<DelaunayTriangulator.Edge>();
			
			foreach (var pointEdges in edgeGraph)
			{
				foreach (var edge in pointEdges.Value)
				{
					edges.Add(edge);
				}
			}
			
			return new ChunkInstance(globalIndex, areas, points, edges);
		}

		private List<Vector2> GetStartPositions(IReadOnlyList<Vector2> leftBorderStartPositions, IReadOnlyList<Vector2> topBorderStartPositions, IReadOnlyList<Vector2> rightBorderStartPositions, IReadOnlyList<Vector2> bottomBorderStartPositions)
		{
			var startPositions = new List<Vector2>();
			if (leftBorderStartPositions != null)
			{
				startPositions.AddRange(leftBorderStartPositions);
			}
			else
			{
				startPositions.AddRange(PoissonSampler.SamplePositions(Vector2.Zero, Vector2.Down * ChunkSize.Y, m_PointsMinDistance));
			}
			if (topBorderStartPositions != null)
			{
				startPositions.AddRange(topBorderStartPositions);
			}
			else
			{
				startPositions.AddRange(PoissonSampler.SamplePositions(Vector2.Zero, Vector2.Right * ChunkSize.X, m_PointsMinDistance));
			}
			if (rightBorderStartPositions != null)
			{
				startPositions.AddRange(rightBorderStartPositions);
			}
			else
			{
				startPositions.AddRange(PoissonSampler.SamplePositions(Vector2.Right * ChunkSize.X, ChunkSize, m_PointsMinDistance));
			}
			if (bottomBorderStartPositions != null)
			{
				startPositions.AddRange(bottomBorderStartPositions);
			}
			else
			{
				startPositions.AddRange(PoissonSampler.SamplePositions(Vector2.Down * ChunkSize.Y, ChunkSize, m_PointsMinDistance));
			}

			return startPositions;
		}
		
		public int GetCorrespondingChunkIndex(Vector3 globalPosition)
		{
			throw new NotImplementedException();
		}
	}
}