using System.Linq;
using Godot;

public partial class PlayerMovementController : CharacterBody3D
{
	[Export] private float m_MoveSpeed = 2;
	[Export] private Node3D m_VisualsPivot;
	[Export] private bool m_LookRight = true;
	
	[ExportGroup("Animations")]
	[Export] private AnimationPlayer m_AnimationPlayer;
	[Export] private string m_IDLEAnimationName = "Idle";
	[Export] private string m_WalkAnimationName = "Walk";
	
	[ExportGroup("Move Actions Names")]
	[Export] private string m_MoveUpActionName = "MoveUp";
	[Export] private string m_MoveDownActionName = "MoveDown";
	[Export] private string m_MoveRightActionName = "MoveRight";
	[Export] private string m_MoveLeftActionName = "MoveLeft";
	
	private bool m_Walking;

	public override void _Ready()
	{
		m_AnimationPlayer.Stop();
		m_AnimationPlayer.Play(m_IDLEAnimationName);
		m_Walking = false;
	}

	public override void _Process(double delta)
	{
		GlobalPosition = new Vector3(GlobalPosition.X, 0, GlobalPosition.Z);
		
		var moveVector = Vector2.Zero;
		if (Input.IsActionPressed(m_MoveUpActionName))
		{
			moveVector += Vector2.Down;
		}
		if (Input.IsActionPressed(m_MoveDownActionName))
		{
			moveVector += Vector2.Up;
		}
		if (Input.IsActionPressed(m_MoveRightActionName))
		{
			moveVector += Vector2.Right;
		}
		if (Input.IsActionPressed(m_MoveLeftActionName))
		{
			moveVector += Vector2.Left;
		}
		Velocity = new Vector3(moveVector.Y, 0, moveVector.X).Normalized() * m_MoveSpeed;
		
		if (moveVector.X != 0 && 
			((Input.IsActionPressed(m_MoveRightActionName) && !m_LookRight)
			|| (Input.IsActionPressed(m_MoveLeftActionName) && m_LookRight)))
		{
			ToggleLook();
		}
		
		HandleAnimation();
	}
	
	private void ToggleLook()
	{
		m_LookRight = !m_LookRight;
		toggleLoop(m_VisualsPivot);
		
		void toggleLoop(Node3D pivot)
		{
			foreach (Node3D node in pivot.GetChildren())
			{
				var position = node.Position;
				position.X *= -1;
				node.Position = position;
				
				var rotation = node.Rotation;
				rotation.Z *= -1;
				node.Rotation = rotation;
				
				if(node is Sprite3D sprite)
				{
					sprite.FlipH = !sprite.FlipH;
				}
				
				toggleLoop(node);
			}
		}
	}
	
	private void HandleAnimation()
	{
		if (!m_Walking && Velocity.LengthSquared() > 0)
		{
			m_AnimationPlayer.Stop();
			m_AnimationPlayer.Play(m_WalkAnimationName);
			m_Walking = true;
		}
		else if(m_Walking && Velocity.LengthSquared() == 0)
		{
			m_AnimationPlayer.Stop();
			m_AnimationPlayer.Play(m_IDLEAnimationName);
			m_Walking = false;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		MoveAndSlide();
	}
}
