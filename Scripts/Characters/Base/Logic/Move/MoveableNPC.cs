using Godot;

public partial class MoveableNPC : CharacterFollower
{
	[Export] protected CharacterVisuals Visuals { get; private set; }
	private bool m_Walking;
	private Node3D m_VisualsPivot;
	
	protected bool LookRight { get; private set; } = true;
		
	public bool LookAtTarget { get; set; }

	public override void _Ready()
	{
		base._Ready();
		m_VisualsPivot = GetNode("CharacterVisuals") as Node3D;
		ResetWalkAnimationState();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (LookAtTarget)
		{
			var targetLook = FollowTarget.GlobalPosition.Z > GlobalPosition.Z;
			if (LookRight != targetLook) Visuals.ToggleLook();
		}
		else if((Velocity.Z > 0 && !LookRight) || (Velocity.Z < 0 && LookRight))
		{
			Visuals.ToggleLook();
		}
		
		HandleAnimation();
	}
	
	protected virtual void HandleAnimation()
	{
		if (!m_Walking && Velocity.LengthSquared() > 0.05f)
		{
			Visuals.SetState(CharacterVisuals.State.Walk);
			m_Walking = true;
		}
		else if(m_Walking && Velocity.LengthSquared() < 0.05f)
		{
			Visuals.SetState(CharacterVisuals.State.IDLE);
			m_Walking = false;
		}
	}
	
	protected void ResetWalkAnimationState()
	{
		Visuals.SetState(CharacterVisuals.State.None);
		m_Walking = false;
	}
}