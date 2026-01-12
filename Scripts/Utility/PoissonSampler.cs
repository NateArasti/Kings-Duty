using System;
using System.Collections.Generic;
using Godot;

public static partial class PoissonSampler
{
	private const int k_MaxSearchIterionsCount = 50;
	
	public static List<Vector2> SamplePositions(Vector2 start, Vector2 end, float minimumDistance, bool includeCorners = true)
	{
		var result = new List<Vector2>();
		var direction = (end - start).Normalized();
		var corners = includeCorners ? 
			(start + 0.5f * minimumDistance * direction, end - 0.5f * minimumDistance * direction) :
			(start, end);
		var step = minimumDistance + minimumDistance * GD.Randf();
		var currentPoint = corners.Item1 + step * direction;
		while(MathExtensions.PointOnSegment(currentPoint, corners.Item1, corners.Item2))
		{
			result.Add(currentPoint);
			step = minimumDistance + minimumDistance * GD.Randf();
			currentPoint += step * direction;
		}
		return result;
	}
	
	public static List<Vector2> SamplePositions(
		Rect2 rect, 
		float minimumDistance, 
		int maxSearchIterionsCount = k_MaxSearchIterionsCount)
	{
		return SamplePositions(rect, minimumDistance, samplePreparation: null, maxSearchIterionsCount);
	}
	
	public static List<Vector2> SamplePositions(
		Rect2 rect, 
		float minimumDistance, 
		IReadOnlyList<Vector2> startPoints,
		int maxSearchIterionsCount = k_MaxSearchIterionsCount)
	{
		void samplePreparation(SampleData sampleData)
		{
			foreach	(var point in startPoints)
			{
				sampleData.CurrentPoints.Add(point);
				sampleData.ResultPoints.Add(point);
				var gridPosition = Utility.GetGridPosition(point, rect.Position, sampleData.CellSize);
				gridPosition = gridPosition.Clamp(Vector2I.Zero, new Vector2I(sampleData.Width - 1, sampleData.Height - 1));
				var index = Utility.GetFlatIndex(gridPosition.X, gridPosition.Y, sampleData.Width);
				sampleData.Grid[index] = point;
			}
		}
		
		return SamplePositions(rect, minimumDistance, samplePreparation, maxSearchIterionsCount);
	}
	
	public static List<Vector2> SamplePositions(
		Rect2 rect,
		float minimumDistance, 
		List<Rect2> predefinedAreas,
		IReadOnlyList<Vector2> startPoints = null,
		int maxSearchIterionsCount = k_MaxSearchIterionsCount)
	{
		return SamplePositions(rect, minimumDistance, (data) =>
		{
			PreparePredefinedAreas(data, predefinedAreas);
			if (startPoints != null)
				PrepareStartPoints(data, startPoints);
		}, maxSearchIterionsCount);;
	}
	
	private static List<Vector2> SamplePositions(
		Rect2 rect, 
		float minimumDistance, 
		Action<SampleData> samplePreparation = null,
		int maxSearchIterionsCount = k_MaxSearchIterionsCount)
	{
		var cellSize = minimumDistance / MathExtensions.SquareRootOfTwo;
		var sqrMinDistance = minimumDistance * minimumDistance;
		
		var width = Mathf.CeilToInt(rect.Size.X / cellSize);
		var height = Mathf.CeilToInt(rect.Size.Y / cellSize);
		
		var sampleData = new SampleData(rect, cellSize, width, height);
		
		var grid = sampleData.Grid;
		
		var currentPoints = sampleData.CurrentPoints;
		var resultPoints = sampleData.ResultPoints;
		
		samplePreparation?.Invoke(sampleData);
		
		if (currentPoints.Count == 0)
		{
			var startPoint = RandomExtensions.GetRandomPointInArea(rect);
			var gridPosition = Utility.GetGridPosition(startPoint, rect.Position, cellSize);
			var index = Utility.GetFlatIndex(gridPosition.X, gridPosition.Y, width);
			grid[index] = startPoint;
			currentPoints.Add(startPoint);
			resultPoints.Add(startPoint);
		}
		
		while(currentPoints.Count > 0)
		{
			var checkPointIndex = GD.RandRange(0, currentPoints.Count - 1);
			var checkPoint = currentPoints[checkPointIndex];
			
			var addedPoints = false;
			
			for	(var i = 0; i < maxSearchIterionsCount; ++i)
			{
				var radius = minimumDistance + minimumDistance * GD.Randf();
				var candidate = checkPoint + RandomExtensions.RandomPointOnUnitCircle() * radius;
				if (!rect.HasPoint(candidate)) continue;
				var gridPosition = Utility.GetGridPosition(candidate, rect.Position, cellSize);
				var tooClose = false;
				
				var searchXRange = new Vector2I(Mathf.Max(0, gridPosition.X - 2), Mathf.Min(width - 1, gridPosition.X + 2));
				var searchYRange = new Vector2I(Mathf.Max(0, gridPosition.Y - 2), Mathf.Min(height - 1, gridPosition.Y + 2));
				
				for	(var x = searchXRange.X; x <= searchXRange.Y; ++x)
				{
					for	(var y = searchYRange.X; y <= searchYRange.Y; ++y)
					{
						var index = Utility.GetFlatIndex(x, y, width);
						if (grid[index].HasValue && grid[index].Value.DistanceSquaredTo(candidate) < sqrMinDistance)
						{
							tooClose = true;
							break;
						}
					}
					if(tooClose) break;
				}
				
				if(tooClose) continue;
				
				var candidateIndex = Utility.GetFlatIndex(gridPosition.X, gridPosition.Y, width);				
				grid[candidateIndex] = candidate;
				currentPoints.Add(candidate);
				resultPoints.Add(candidate);
				
				addedPoints = true;
			}
			
			if(!addedPoints)
			{
				currentPoints.RemoveAt(checkPointIndex);
			}
		}
		
		return resultPoints;
	}
	
	private static void PrepareStartPoints(SampleData sampleData, IReadOnlyList<Vector2> startPoints)
	{
		foreach	(var point in startPoints)
		{
			sampleData.CurrentPoints.Add(point);
			sampleData.ResultPoints.Add(point);
			var gridPosition = Utility.GetGridPosition(point, sampleData.Rect.Position, sampleData.CellSize);
			gridPosition = gridPosition.Clamp(Vector2I.Zero, new Vector2I(sampleData.Width - 1, sampleData.Height - 1));
			var index = Utility.GetFlatIndex(gridPosition.X, gridPosition.Y, sampleData.Width);
			sampleData.Grid[index] = point;
		}
	}
	
	private static void PreparePredefinedAreas(SampleData sampleData, List<Rect2> predefinedAreas)
	{
		for (var i = 0; i < predefinedAreas.Count; i++)
		{
			var area = predefinedAreas[i];
			var center = Vector2.Zero;
			for (var k = 0; k < k_MaxSearchIterionsCount; ++k)
			{
				center = RandomExtensions.GetRandomPointInArea(sampleData.Rect, area.Size);
				area.Position = center - 0.5f * area.Size;
				var success = true;
				for (var j = 0; j < i; ++j)
				{
					if (area.Intersects(predefinedAreas[j]))
					{
						success = false;
						break;
					}
				}
				if (success) break;
			}
			
			predefinedAreas[i] = area;
			
			sampleData.CurrentPoints.Add(center);
			sampleData.ResultPoints.Add(center);
			
			var start = Utility.GetGridPosition(area.Position, sampleData.Rect.Position, sampleData.CellSize);
			var end = Utility.GetGridPosition(area.End, sampleData.Rect.Position, sampleData.CellSize);
			
			for	(var x = start.X; x <= end.X; ++x)
			{
				for	(var y = start.Y; y <= end.Y; ++y)
				{
					var gridPosition = new Vector2I(x, y);
					gridPosition = gridPosition.Clamp(Vector2I.Zero, new Vector2I(sampleData.Width - 1, sampleData.Height - 1));
					var index = Utility.GetFlatIndex(gridPosition.X, gridPosition.Y, sampleData.Width);
					sampleData.Grid[index] = Utility.GetWorldPosition(gridPosition, sampleData.Rect.Position, sampleData.CellSize);
				}
			}
		}
	}
	
	private readonly struct SampleData
	{
		public readonly Rect2 Rect;
		public readonly float CellSize;
		public readonly int Width;
		public readonly int Height;
		
		public readonly Vector2?[] Grid;		
		public readonly List<Vector2> CurrentPoints = new();
		public readonly List<Vector2> ResultPoints = new();

		public SampleData(
			Rect2 rect,
			float cellSize,
			int width,
			int height
		)
		{
			Rect = rect;
			
			CellSize = cellSize;
			Width = width;
			Height = height;
			
			Grid = new Vector2?[width * height];
		}
	}
}
