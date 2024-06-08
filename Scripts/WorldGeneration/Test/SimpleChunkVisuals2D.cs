using Godot;

namespace WorldGeneration.Test
{
	public partial class SimpleChunkVisuals2D : Node2D
	{
		[Export] private Vector2 m_Offset;
		[Export] private ChunkGenerator m_ChunkGenerator;
		
		private readonly Rect2[] borderRects = new Rect2[9];
		private readonly ChunkInstance[] m_ChunkInstances = new ChunkInstance[9];
		
		private readonly Texture2D m_IconSprite = ResourceLoader.Load<Texture2D>("icon.svg");
		
		private int currentStep = 0;
		
		public override void _Ready()
		{
			base._Ready();
			
			for (var i = 0; i < m_ChunkInstances.Length; ++i)
			{
				borderRects[i] = new Rect2(m_Offset + Utility.Get2DIndex(i, 3) * m_ChunkGenerator.ChunkSize, m_ChunkGenerator.ChunkSize);
				
				m_ChunkInstances[i] = i switch
				{
					0 => m_ChunkGenerator.GenerateChunk(new Vector2I(0, 0), null, null, null, null),
					1 => m_ChunkGenerator.GenerateChunk(new Vector2I(1, 0), m_ChunkInstances[0], null, null, null),
					2 => m_ChunkGenerator.GenerateChunk(new Vector2I(2, 0), m_ChunkInstances[1], null, null, null),
					3 => m_ChunkGenerator.GenerateChunk(new Vector2I(0, 1), null, m_ChunkInstances[0], null, null),
					4 => m_ChunkGenerator.GenerateChunk(new Vector2I(1, 1), m_ChunkInstances[3], m_ChunkInstances[1], null, null),
					5 => m_ChunkGenerator.GenerateChunk(new Vector2I(2, 1), m_ChunkInstances[4], m_ChunkInstances[2], null, null),
					6 => m_ChunkGenerator.GenerateChunk(new Vector2I(0, 2), null, m_ChunkInstances[3], null, null),
					7 => m_ChunkGenerator.GenerateChunk(new Vector2I(1, 2), m_ChunkInstances[6], m_ChunkInstances[4], null, null),
					8 => m_ChunkGenerator.GenerateChunk(new Vector2I(2, 2), m_ChunkInstances[7], m_ChunkInstances[5], null, null),
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
				currentStep++;
				QueueRedraw();
			}
		}

		public override void _Draw()
		{
			base._Draw();
			
			foreach (var child in GetChildren())
			{
				child.QueueFree();
			}
			
			for (var i = 0; i < m_ChunkInstances.Length; ++i)
			{
				DrawRect(borderRects[i], Colors.White, false, 5);
				
				if (currentStep == 0) continue;
				
				foreach (var area in m_ChunkInstances[i].Areas)
				{
					DrawRect(new Rect2(area.Center + borderRects[i].Position - 0.5f * area.Size, area.Size), Colors.Red);
					
					var sprite = new Sprite2D();
					AddChild(sprite);
					sprite.Texture = m_IconSprite;
					sprite.Position = area.Center + borderRects[i].Position;
					sprite.Scale = Vector2.One * 0.1f;
				}
				
				if (currentStep == 1) continue;
				
				foreach (var point in m_ChunkInstances[i].AllPoints)
				{
					var sprite = new Sprite2D();
					AddChild(sprite);
					sprite.Texture = m_IconSprite;
					sprite.Position = point + borderRects[i].Position;
					sprite.Scale = Vector2.One * 0.1f;
				}
				
				if (currentStep == 2) continue;
				
				foreach (var road in m_ChunkInstances[i].Roads)
				{
					DrawLine(m_ChunkInstances[i].AllPoints[road.StartIndex] + borderRects[i].Position, m_ChunkInstances[i].AllPoints[road.EndIndex] + borderRects[i].Position, Colors.Red);
				}
			}
		}
	}
}