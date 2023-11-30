using System.Collections.Generic;
using Godot;

public static class PoissonSampler
{
	private const int k_MaxSearchIterionsCount = 50;
	
	public static List<Vector2> SamplePositions(
		Rect2 rect, float minimumDistance, 
		List<Vector2> startPoints = null,
		int maxSearchIterionsCount = k_MaxSearchIterionsCount)
	{
		var cellSize = minimumDistance / MathExtensions.SquareRootOfTwo;
		var sqrMinDistance = minimumDistance * minimumDistance;
		
		var width = Mathf.CeilToInt(rect.Size.X / cellSize);
		var height = Mathf.CeilToInt(rect.Size.Y / cellSize);
		var grid = new Vector2?[width * height];
		
		var currentPoints = new List<Vector2>();
		var resultPoints = new List<Vector2>();
		
		if(startPoints != null)
		{
			foreach	(var point in startPoints)
			{
				currentPoints.Add(point);
				resultPoints.Add(point);
				var gridPosition = Utility.GetGridPosition(point, rect.Position, cellSize);
				gridPosition = gridPosition.Clamp(Vector2I.Zero, new Vector2I(width - 1, height - 1));
				var index = Utility.GetFlatIndex(gridPosition.X, gridPosition.Y, width);
				grid[index] = point;
			}
		}
		else
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
}
