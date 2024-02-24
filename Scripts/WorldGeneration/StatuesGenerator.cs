using Godot;
using System.Collections.Generic;

public partial class StatuesGenerator : Node, IWorldGenerationSubscriber
{
	[Export] public bool Enabled { get; private set; } = true;
	
	[ExportGroup("Generation Data")]
	[Export] private PackedScene m_PropScene;
	[Export(PropertyHint.Range, "0, 1")] private float m_PropSpawnChance = 0.25f;
	
	private Vector2I m_ChunkGridSize;
	private Vector2 m_ChunkRectSize;
	private System.Func<Vector2, Vector2I> GetGridCoords;
	private System.Func<Vector2I, Vector2> GetWorldCoords;

	public void Initialize(Vector2I chunkGridSize, Vector2 chunkRectSize, 
		System.Func<Vector2, Vector2I> getGridCoords, System.Func<Vector2I, Vector2> getWorldCoords)
	{
		m_ChunkGridSize = chunkGridSize;
		m_ChunkRectSize = chunkRectSize;
		
		GetGridCoords = getGridCoords;
		GetWorldCoords = getWorldCoords;
	}

	public void OnChunkGenerated(TileWorldGenerator.ChunkInstance chunkInstance, Vector3 worldOffset)
	{
		var props = new HashSet<Statue>();
		
		foreach (var gridPosition in chunkInstance.Chunk.PointOfInterestCells)
		{
			if (GD.Randf() > m_PropSpawnChance) continue;
			var position = GetWorldCoords(gridPosition);
			var prop = CreateProp(position);
			prop.Position += worldOffset;
			props.Add(prop);
		}
	}
	
	private Statue CreateProp(Vector2 position)
	{
		var instance = m_PropScene.Instantiate<Statue>();
		instance.Reset();
		instance.Position = new Vector3(position.Y, 0, position.X);
		AddChild(instance);
		return instance;
	}
}
