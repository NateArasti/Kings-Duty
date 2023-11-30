using Godot;

public static class RandomExtensions
{
	public static Vector2 GetRandomPointInArea(Rect2 rect)
	{
		return rect.Position + 
			new Vector2(GD.Randf() * rect.Size.X, GD.Randf() * rect.Size.Y);
	}
	
	public static Vector2 RandomPointOnUnitCircle()
	{
		var angle = GD.Randf() * MathExtensions.TwoPI;
		
		var x = Mathf.Cos(angle);
		var y = Mathf.Sin(angle);
		
		return new Vector2(x, y);
	}
}