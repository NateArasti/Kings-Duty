using Godot;

public static class MathExtensions
{
	public const float TwoPI = Mathf.Pi * 2;
	public static readonly float SquareRootOfTwo = Mathf.Sqrt(2);
	
	public static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
	{
		Vector2 q0 = p0.Lerp(p1, t);
		Vector2 q1 = p1.Lerp(p2, t);
		Vector2 q2 = p2.Lerp(p3, t);

		Vector2 r0 = q0.Lerp(q1, t);
		Vector2 r1 = q1.Lerp(q2, t);

		Vector2 s = r0.Lerp(r1, t);
		return s;
	}
	
	public static Circle GetCircumcircle(Vector2 v1, Vector2 v2, Vector2 v3)
	{
		var center = GetCircumcenter(v1, v2, v3);
		return new Circle()
		{
			Center = center,
			Radius = center.DistanceTo(v1)
		};
	}
	
	public static (float x, float y) Solve(float a1, float b1, float c1, float a2, float b2, float c2)
	{
		var x1 = Determinant(c1, b1, c2, b2) / Determinant(a1, b1, a2, b2);
		var x2 = Determinant(a1, c1, a2, c2) / Determinant(a1, b1, a2, b2);
		return (x1, x2);
	}
	
	public static float Determinant(float x1, float y1, float x2, float y2)
	{
		return x1 * y2 - y1 * x2;		
	}
	
	public static bool InRange(this float value, float min, float max)
	{
		return min <= value && value <= max;
	}
	
	public static bool InRange(this int value, int min, int max)
	{
		return min <= value && value <= max;
	}
	
	public static bool PointOnSegment(Vector2 point, Vector2 start, Vector2 end)
	{
		var xa = start - point;
		var xb = end - point;
		var angleBetween = xa.AngleTo(xb);
		return Mathf.Cos(angleBetween) == -1;
	}
	
	public static Vector2 GetCircumcenter(Vector2 pointA, Vector2 pointB, Vector2 pointC)
	{
		var lineAB = new LinearEquation(pointA, pointB);
		var lineBC = new LinearEquation(pointB, pointC);
 
		var midPointAB = pointA.Lerp(pointB, .5f);
		var midPointBC = pointB.Lerp(pointC, .5f);
 
		var perpendicularAB = lineAB.PerpendicularLineAt(midPointAB);
		var perpendicularBC = lineBC.PerpendicularLineAt(midPointBC);
 
		var circumcircle = GetCrossingPoint(perpendicularAB, perpendicularBC);
 
		var circumRadius = circumcircle.DistanceTo(pointA);
 
		return circumcircle;
	}
 
	private static Vector2 GetCrossingPoint(LinearEquation line1, LinearEquation line2)
	{
		var A1 = line1.A;
		var A2 = line2.A;
		var B1 = line1.B;
		var B2 = line2.B;
		var C1 = line1.C;
		var C2 = line2.C;
 
		//Cramer's rule
		var Determinant = A1 * B2 - A2 * B1;
		var DeterminantX = C1 * B2 - C2 * B1;
		var DeterminantY = A1 * C2 - A2 * C1;
 
		var x = DeterminantX / Determinant;
		var y = DeterminantY / Determinant;
 
		return new Vector2(x, y);
	}
	
	private class LinearEquation
	{
		public float A;
		public float B;
		public float C;
	
		public LinearEquation() { }
	
		//Ax+By=C
		public LinearEquation(Vector2 pointA, Vector2 pointB)
		{
			var deltaX = pointB.X - pointA.X;
			var deltaY = pointB.Y - pointA.Y;
			A = deltaY; //y2-y1
			B = -deltaX; //x1-x2
			C = A * pointA.X + B * pointA.Y;
		}
	
		public LinearEquation PerpendicularLineAt(Vector2 point)
		{
			var newLine = new LinearEquation();
	
			newLine.A = -B;
			newLine.B = A;
			newLine.C = newLine.A * point.X + newLine.B * point.Y;
	
			return newLine;
		}
	}

	public struct Circle
	{
		public Vector2 Center;
		public float Radius;
		
		public bool Contains(Vector2 point) => point.DistanceTo(Center) <= Radius;
	}
}