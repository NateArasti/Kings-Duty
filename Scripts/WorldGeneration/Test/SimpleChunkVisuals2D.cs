using Godot;

namespace WorldGeneration.Test
{
	public partial class SimpleChunkVisuals2D : Node2D
	{
		[Export] private Vector2 m_Offset;
		[Export] private ChunkGenerator m_ChunkGenerator;
		
		private readonly Rect2[] borderRects = new Rect2[4];
		private readonly ChunkInstance[] m_ChunkInstances = new ChunkInstance[4];
		
		private readonly Texture2D m_IconSprite = ResourceLoader.Load<Texture2D>("icon.svg");
		
		public override void _Ready()
		{
			base._Ready();
			
			foreach (var child in GetChildren())
			{
				child.QueueFree();
			}
			
			for (var i = 0; i < 4; ++i)
			{
				borderRects[i] = new Rect2(m_Offset + Utility.Get2DIndex(i, 2) * m_ChunkGenerator.ChunkSize, m_ChunkGenerator.ChunkSize);
				
				m_ChunkInstances[i] = i switch
                {
                    0 => m_ChunkGenerator.GenerateChunk(new Vector2I(0, 0)),
                    1 => m_ChunkGenerator.GenerateChunk(new Vector2I(0, 1), m_ChunkInstances[0]),
                    2 => m_ChunkGenerator.GenerateChunk(new Vector2I(1, 0), null, m_ChunkInstances[0]),
                    3 => m_ChunkGenerator.GenerateChunk(new Vector2I(1, 1), m_ChunkInstances[2], m_ChunkInstances[1]),
                    _ => throw new System.NotImplementedException(),
                };
				foreach (var point in m_ChunkInstances[i].AllPoints)
				{
					var sprite = new Sprite2D();
					AddChild(sprite);
					sprite.Texture = m_IconSprite;
					sprite.Position = point + borderRects[i].Position;
					sprite.Scale = Vector2.One * 0.1f;
				}
			}
			QueueRedraw();
		}

		public override void _Input(InputEvent @event)
		{
			base._Input(@event);
			if (@event is InputEventMouseButton mouseButton && mouseButton.IsReleased())
			{
				_Ready();
			}
		}

		public override void _Draw()
		{
			base._Draw();
			
			for (var i = 0; i < 4; ++i)
			{
				DrawRect(borderRects[i], Colors.White, false, 5);
				foreach (var area in m_ChunkInstances[i].Areas)
				{
					var shiftArea = new Rect2(area.Center - 0.5f * area.Size, area.Size);
					shiftArea.Position += borderRects[i].Position;
					DrawRect(shiftArea, Colors.Yellow);
				}
				foreach (var road in m_ChunkInstances[i].Roads)
				{
					DrawLine(m_ChunkInstances[i].AllPoints[road.StartIndex] + borderRects[i].Position, m_ChunkInstances[i].AllPoints[road.EndIndex] + borderRects[i].Position, Colors.Red);
				}
			}
		}
	}
}