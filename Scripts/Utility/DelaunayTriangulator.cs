using System;
using System.Collections.Generic;
using Godot;

public static class DelaunayTriangulator
{	
	public static IEnumerable<Triangle> Triangulate(IReadOnlyList<Vector2> points)
	{
		var triangles = new HashSet<Triangle>();
		
		var superTrianglePoints = CreateSuperTriangle(points);
		var superTriangle = new Triangle(-1, -2, -3);
		triangles.Add(superTriangle);
		
		var edges = new HashSet<Edge>();
		var wrongEdges = new HashSet<Edge>();
		var wrongTriangles = new List<Triangle>();
		for (var i = 0; i < points.Count; ++i)
		{
			edges.Clear();
			wrongTriangles.Clear();
			foreach	(var triangle in triangles)
			{
				var v1 = getPointFromIndex(triangle.FirstIndex);
				var v2 = getPointFromIndex(triangle.SecondIndex);
				var v3 = getPointFromIndex(triangle.ThirdIndex);
				if(MathExtensions.GetCircumcircle(v1, v2, v3).Contains(points[i]))
				{
					wrongTriangles.Add(triangle);
					var edge1 = new Edge(triangle.FirstIndex, triangle.SecondIndex);
					var edge2 = new Edge(triangle.FirstIndex, triangle.ThirdIndex);
					var edge3 = new Edge(triangle.SecondIndex, triangle.ThirdIndex);
					
					if(edges.Contains(edge1))
					{
						wrongEdges.Add(edge1);
					}
					else
					{
						edges.Add(edge1);
					}
					if(edges.Contains(edge2))
					{
						wrongEdges.Add(edge2);
					}
					else
					{
						edges.Add(edge2);
					}
					if(edges.Contains(edge3))
					{
						wrongEdges.Add(edge3);
					}
					else
					{
						edges.Add(edge3);
					}
				}				
			}
			
			//removing wrong triangles
			foreach	(var triangle in wrongTriangles)
			{
				triangles.Remove(triangle);
			}
			
			//removing shared edges
			foreach (var edge in wrongEdges)
			{
				edges.Remove(edge);
			}
			
			foreach	(var edge in edges)
			{
				triangles.Add(new(edge.StartIndex, edge.EndIndex, i));
			}
		}
		
		foreach	(var triangle in triangles)
		{
			if(triangle.FirstIndex < 0 || triangle.SecondIndex < 0 || triangle.ThirdIndex < 0)
			{
				continue;
			}
			
			yield return triangle;
		}
		
		Vector2 getPointFromIndex(int index)
		{
			if(index < 0)
			{
				return index switch
				{
					-1 => superTrianglePoints.v1,
					-2 => superTrianglePoints.v2,
					-3 => superTrianglePoints.v3,
					_ => throw new System.NotImplementedException(),
				};
			}
			
			return points[index];
		}
	}
	
	public static HashSet<Edge> GetEdges(List<Triangle> triangles)
	{
		var edges = new HashSet<Edge>();
		foreach (var triangle in triangles)
		{
			edges.Add(new(triangle.FirstIndex, triangle.SecondIndex));
			edges.Add(new(triangle.FirstIndex, triangle.ThirdIndex));
			edges.Add(new(triangle.SecondIndex, triangle.ThirdIndex));
		}
		return edges;
	}
	
	private static (Vector2 v1, Vector2 v2, Vector2 v3) CreateSuperTriangle(IEnumerable<Vector2> points)
	{
		var minx = float.PositiveInfinity;
		var miny = float.PositiveInfinity;
		var maxx = float.NegativeInfinity;
		var maxy = float.NegativeInfinity;
		foreach(var point in points)
		{
			minx = Mathf.Min(minx, point.X);
			miny = Mathf.Min(miny, point.Y);
			maxx = Mathf.Max(maxx, point.X);
			maxy = Mathf.Max(maxy, point.Y);
		}
		const float offset = 100;
		minx -= offset;
		miny -= offset;
		maxx += offset;
		maxy += offset;
		
		var dx = maxx - minx;
		var dy = maxy - miny;
		
		var v0 = new Vector2(minx, miny);
		var v1 = v0 + new Vector2(2 * dx, 0);
		var v2 = v0 + new Vector2(0, 2 * dy);
		
		return new(v0, v1, v2);
	}

	public struct Edge : IEquatable<Edge>
	{
		public int StartIndex;
		public int EndIndex;
		
		public Edge(int start, int end)
		{
			StartIndex = start;
			EndIndex = end;
		}

		public bool Equals(Edge other)
		{
			return (StartIndex.Equals(other.StartIndex) && EndIndex.Equals(other.EndIndex))
				|| (StartIndex.Equals(other.EndIndex) && EndIndex.Equals(other.StartIndex));
		}
	}
	
	public struct Triangle
	{
		public int FirstIndex;
		public int SecondIndex;
		public int ThirdIndex;
		
		public Triangle(int i1, int i2, int i3)
		{
			FirstIndex = i1;
			SecondIndex = i2;
			ThirdIndex = i3;
		}
	}
}