using System.Collections.Generic;
using Godot;

namespace WorldGeneration.Test
{
	public partial class NodesCreationTest : Node2D
	{
		private Texture2D m_IconSprite = ResourceLoader.Load<Texture2D>("icon.svg");
		
		private Rect2 borderRect;
		private List<Rect2> rectAreas;

		public override void _Input(InputEvent @event)
		{
			base._Input(@event);
			if (@event is InputEventMouseButton mouseButton && mouseButton.IsReleased())
			{
				_Ready();
			}
		}

		public override void _Ready()
		{
			base._Ready();
			
			foreach (var child in GetChildren())
			{
				child.QueueFree();
			}
			
			Position = new Vector2(640, 360);
			var size = new Vector2(500, 350);
			borderRect = new Rect2(-0.5f * size, size);
			
			rectAreas = new List<Rect2>()
			{
				new Rect2(Vector2.Zero, new Vector2(150, 250)),
				new Rect2(Vector2.Zero, new Vector2(50, 50)),
				new Rect2(Vector2.Zero, new Vector2(50, 50)),
			};
			
			var points = PoissonSampler.SamplePositions(borderRect, 50, rectAreas);
			foreach (var point in points)
			{
				var sprite = new Sprite2D();
				AddChild(sprite);
				sprite.Texture = m_IconSprite;
				sprite.Position = point;
				sprite.Scale = Vector2.One * 0.1f;
			}
			
			QueueRedraw();
		}

		public override void _Draw()
		{
			base._Draw();
			
			DrawRect(borderRect, Colors.White, false, 5);
			foreach (var rect in rectAreas)
			{
				DrawRect(rect, Colors.Yellow);				
			}
		}
	}
}