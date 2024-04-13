using Godot;

public partial class PlayerMovementController : CharacterBody3D, IHittable
{
	[Export] private float m_MoveSpeed = 2;
	[Export] private CharacterVisuals m_Visuals;
	
	[ExportGroup("Move Actions Names")]
	[Export] private string m_MoveUpActionName = "MoveUp";
	[Export] private string m_MoveDownActionName = "MoveDown";
	[Export] private string m_MoveRightActionName = "MoveRight";
	[Export] private string m_MoveLeftActionName = "MoveLeft";
	
	private bool m_LookRight = true;

	[Export] public HealthSystem HealthSystem { get; private set; }

	public override void _Process(double delta)
	{
		GlobalPosition = new Vector3(GlobalPosition.X, 0, GlobalPosition.Z);
		
		var moveVector = Vector2.Zero;
		if (Input.IsActionPressed(m_MoveUpActionName))
		{
			moveVector += Vector2.Up;
		}
		if (Input.IsActionPressed(m_MoveDownActionName))
		{
			moveVector += Vector2.Down;
		}
		if (Input.IsActionPressed(m_MoveRightActionName))
		{
			moveVector += Vector2.Right;
		}
		if (Input.IsActionPressed(m_MoveLeftActionName))
		{
			moveVector += Vector2.Left;
		}
		Velocity = new Vector3(moveVector.X, 0, moveVector.Y).Normalized() * m_MoveSpeed;
		
		if (moveVector.X != 0 && 
			((Input.IsActionPressed(m_MoveRightActionName) && !m_LookRight)
			|| (Input.IsActionPressed(m_MoveLeftActionName) && m_LookRight)))
		{
			m_LookRight = !m_LookRight;
			m_Visuals.ToggleLook();
		}
		
		m_Visuals.SetState(Velocity.LengthSquared() > 0 ? CharacterVisuals.State.Walk : CharacterVisuals.State.IDLE);
	}

	public override void _PhysicsProcess(double delta)
	{
		MoveAndSlide();
	}
}
