using Godot;

public partial class CharacterVisuals : Node3D
{
	public enum State
	{
		None,
		IDLE,
		Walk,
	}
	
	[Export] private AnimationPlayer m_AnimationPlayer;
	[Export] private Sprite3D[] m_CharacterSprites;
	
	[ExportGroup("Animation names")]
	[Export] private string m_ResetAnimationName;
	[Export] private string m_IDLEAnimationName;
	[Export] private string m_WalkAnimationName;
	
	public State CurrentState { get; private set; }

	public override void _Ready()
	{
		base._Ready();
		SetState(State.IDLE);
	}

	public void SetState(State state)
	{
		if (CurrentState == state) return;
		CurrentState = state;
		m_AnimationPlayer.Stop();
		switch (state)
		{
			case State.IDLE:
				m_AnimationPlayer.Play(m_IDLEAnimationName, customSpeed: (float)GD.RandRange(0.95f, 1.05f));
				break;
			case State.Walk:
				m_AnimationPlayer.Play(m_WalkAnimationName);
				break;
			default:
				m_AnimationPlayer.Play(m_ResetAnimationName);
				break;
		}
	}
	
	public void ToggleLook()
	{
		foreach (var sprite in m_CharacterSprites)
		{
			sprite.FlipH = !sprite.FlipH;
		}
	}
}