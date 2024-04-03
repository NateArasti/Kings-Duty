using Godot;

namespace WorldGeneration.Test
{
	public partial class SimpleChunkVisuals2D : Node2D
	{
		[Export] private Vector2 m_Offset;
		[Export] private ChunkGenerator m_ChunkGenerator;
		
		private Rect2 borderRect;
		private ChunkInstance m_ChunkInstance;
		
		private readonly Texture2D m_IconSprite = ResourceLoader.Load<Texture2D>("icon.svg");
		
		public override void _Ready()
		{
			base._Ready();
			
			foreach (var child in GetChildren())
			{
				child.QueueFree();
			}
			
			m_ChunkInstance = m_ChunkGenerator.GenerateChunk(0);
			borderRect = new Rect2(m_Offset, m_ChunkGenerator.ChunkSize);
			foreach (var point in m_ChunkInstance.Points)
			{
				var sprite = new Sprite2D();
				AddChild(sprite);
				sprite.Texture = m_IconSprite;
				sprite.Position = point + m_Offset;
				sprite.Scale = Vector2.One * 0.1f;
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
			
			DrawRect(borderRect, Colors.White, false, 5);
			foreach (var area in m_ChunkInstance.Areas)
			{
				var shiftArea = area.AreaRect;
				shiftArea.Position += m_Offset;
				DrawRect(shiftArea, Colors.Yellow);
			}
			foreach (var road in m_ChunkInstance.Roads)
			{
				DrawLine(m_ChunkInstance.Points[road.StartIndex] + m_Offset, m_ChunkInstance.Points[road.EndIndex] + m_Offset, Colors.Red);
			}
		}
	}
}