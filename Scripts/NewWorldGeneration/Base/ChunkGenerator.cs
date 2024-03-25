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
			var areas = new List<Rect2>();
			for (var i = 0; i < areaCount; ++i)
			{
				var size = new Vector2((float)GD.RandRange(m_AreaWidthRange.X, m_AreaWidthRange.Y), (float)GD.RandRange(m_AreaHeightRange.X, m_AreaHeightRange.Y));
				areas.Add(new Rect2(Vector2.Zero, size));
			}
			var edgePoints = GetStartPositions(null, null, null, null);
			var points = PoissonSampler.SamplePositions(chunkRect, m_PointsMinDistance, areas, edgePoints);
			var edges = DelaunayTriangulator.TriangulateToDistinctEdges(points);
			
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