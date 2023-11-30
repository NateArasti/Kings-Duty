using Godot;

public static class Utility
{
	public static int GetFlatIndex(int x, int y, int width) => y * width + x;
	public static Vector2I Get2DIndex(int index, int width) => new Vector2I(index % width, index / width);

	public static Vector2I GetGridPosition(Vector2 point, Vector2 start, float cellSize)
	{
		var delta = point - start;
		return new Vector2I((int)(delta.X / cellSize), (int)(delta.Y / cellSize));
	}
}