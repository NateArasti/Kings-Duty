using System.Diagnostics;
using Godot;

public partial class GridWorldGenerator : Node3D
{
	[ExportGroup("Grid Generation")]
	[Export] private PackedScene m_GroundTile;
	[Export] private PackedScene m_RoadTile;
	[Export] private Vector2 m_StepValue = Vector2.One;
	[Export] private Vector2 m_XGridDirection = new Vector2(1, -1);
	[Export] private Vector2 m_YGridDirection = new Vector2(1, 1);
	
	[ExportGroup("World Filling")]
	[Export] private PackedScene[] m_Trees;
	[Export] private float m_TreeSpawnMinDistance = 5;
	
	[ExportGroup("World Generation")]
	[Export] private Vector2 m_GridSize = new(20, 20);
	[Export] private float m_CellSize = 1f;
	[Export] private float m_MinDistance = 5;
	
	private WorldGenerator m_WorldGenerator;
	
	public override void _Ready()
	{
		m_WorldGenerator = new WorldGenerator(m_GridSize, m_CellSize, m_MinDistance);
	}

	public override void _Input(InputEvent @event)
	{
		if(@event is InputEventMouseButton inputEventMouse &&
			inputEventMouse.ButtonIndex == MouseButton.Left && inputEventMouse.IsPressed())
		{
			Generate();
		}
	}
	
	private void Generate()
	{
		ClearAllChildren();
		var chunk = m_WorldGenerator.GenerateChunkNonAllocation();
		for (var i = 0; i < chunk.Length; ++i)
		{
			var coords = Utility.Get2DIndex(i, m_WorldGenerator.Width);
			SpawnTile(coords, chunk[i]);
		}
		var rectSize = 2 * m_GridSize * m_StepValue;
		var possibleTreesPositions = PoissonSampler.SamplePositions(new Rect2(0, -0.5f * rectSize.Y, rectSize), m_TreeSpawnMinDistance);
		foreach (var position in possibleTreesPositions)
		{
			var gridPosition = GetGridCoords(position + Vector2.Right * m_CellSize * m_StepValue.X);
			if(gridPosition.X.InRange(0, m_WorldGenerator.Width - 1)
				&& gridPosition.Y.InRange(0, m_WorldGenerator.Height - 1)
				&& chunk[Utility.GetFlatIndex(gridPosition.X, gridPosition.Y, m_WorldGenerator.Width)] == WorldGenerator.CellType.Ground)
				{
					SpawnTree(position);
				}
		}
	}

	private void ClearAllChildren()
	{
		foreach(var child in GetChildren())
		{
			child.QueueFree();
		}
	}
	
	private void SpawnTree(Vector2 position)
	{
		var tree = m_Trees[GD.RandRange(0, m_Trees.Length - 1)];
		var instance = tree.Instantiate() as Node3D;
		instance.Position = new Vector3(position.Y, 0, position.X);
		AddChild(instance);
	}
	
	private void SpawnTile(Vector2I gridCoords, WorldGenerator.CellType tileType)
	{
		var tile = tileType == WorldGenerator.CellType.Ground ? m_GroundTile : m_RoadTile;
		var spawnCoordinate = GetWorldCoords(gridCoords);
		var instance = tile.Instantiate() as Node3D;
		instance.Position = new Vector3(spawnCoordinate.Y, 0, spawnCoordinate.X);
		instance.Scale = Vector3.One * m_CellSize;
		AddChild(instance);
	}
	
	private Vector2 GetWorldCoords(Vector2I gridCoords)
	{
		return m_CellSize * m_StepValue * (gridCoords.X * m_XGridDirection + gridCoords.Y * m_YGridDirection);
	}
	
	private Vector2I GetGridCoords(Vector2 worldCoords)
	{
		var c = worldCoords / (m_CellSize * m_StepValue);
		var (x, y) = MathExtensions.Solve(m_XGridDirection.X, m_YGridDirection.X, c.X, m_XGridDirection.Y, m_YGridDirection.Y, c.Y);
		return new Vector2I(Mathf.FloorToInt(x), Mathf.FloorToInt(y));
	}
}