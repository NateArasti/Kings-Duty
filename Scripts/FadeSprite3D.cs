using Godot;

public partial class FadeSprite3D : Sprite3D
{
	[Export] private float m_FadeSpeed = 5f;
	[Export] private float m_FadeTargetTransparency = 0.9f;
	[Export(PropertyHint.Range, "-1000, 1000")] private Vector2 m_FadeCenterRangeX = new Vector2(-300, 300);
	[Export(PropertyHint.Range, "-1000, 1000")] private Vector2 m_FadeCenterRangeY = new Vector2(50, 200);

	private Viewport m_Viewport;
	private Camera3D m_Camera;
	private Vector2 m_ScreenSize;

	public override void _Ready()
	{
		base._Ready();
		m_Viewport = GetViewport();
		UpdateScreenSize();
		m_Viewport.SizeChanged += UpdateScreenSize;
		m_Camera = m_Viewport.GetCamera3D();
	}

	private void UpdateScreenSize()
	{
		m_ScreenSize = m_Viewport.GetVisibleRect().Size;
	}

	public override void _Process(double delta)
	{
		var screenPosition = m_Camera.UnprojectPosition(Position);
		var targetFadeValue = 0f;
		if((screenPosition.X - 0.5f * m_ScreenSize.X).InRange(m_FadeCenterRangeX.X, m_FadeCenterRangeX.Y) && 
			(screenPosition.Y - 0.5f * m_ScreenSize.Y).InRange(m_FadeCenterRangeY.X, m_FadeCenterRangeY.Y))
		{
			targetFadeValue = m_FadeTargetTransparency;
		}
		
		Transparency = (float)Mathf.Lerp(Transparency, targetFadeValue, delta * m_FadeSpeed);
	}
}
