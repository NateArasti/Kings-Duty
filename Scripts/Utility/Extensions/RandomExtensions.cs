using System.Collections.Generic;
using Godot;

public static class RandomExtensions
{
	public static Vector2 GetRandomPointInArea(Rect2 rect)
	{
		return rect.Position + 
			new Vector2(GD.Randf() * rect.Size.X, GD.Randf() * rect.Size.Y);
	}
	
	public static Vector2 GetRandomPointInArea(Rect2 rect, Vector2 margin)
	{
		return rect.Position + 0.5f * margin +
			new Vector2(GD.Randf() * (rect.Size.X - margin.X), GD.Randf() * (rect.Size.Y - margin.Y));
	}
	
	public static Vector2 RandomPointOnUnitCircle()
	{
		var angle = GD.Randf() * MathExtensions.TwoPI;
		
		var x = Mathf.Cos(angle);
		var y = Mathf.Sin(angle);
		
		return new Vector2(x, y);
	}
	
	public static float RandomInRange(this Vector2 range)
	{
		return (float)GD.RandRange(range.X, range.Y);
	}
	
	public static int RandomInRange(this Vector2I range)
	{
		return GD.RandRange(range.X, range.Y);
	}
	
	public static T Random<T>(this IReadOnlyList<T> values)
	{
		return values[GD.RandRange(0, values.Count - 1)];
	}
}