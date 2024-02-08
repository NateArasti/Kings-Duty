using Godot;

public partial class MoveableNPC : CharacterFollower
{
	
	[ExportGroup("Animations")]
	[Export] protected AnimationPlayer m_AnimationPlayer;
	[Export] private string m_IDLEAnimationName = "Character/Idle";
	[Export] private string m_WalkAnimationName = "Character/Walk";
	
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
			if (LookRight != targetLook) ToggleLook();
		}
		else if((Velocity.Z > 0 && !LookRight) || (Velocity.Z < 0 && LookRight))
		{
			ToggleLook();			
		}
		
		HandleAnimation();
	}
	
	private void ToggleLook()
	{
		LookRight = !LookRight;
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
	
	protected virtual void HandleAnimation()
	{
		if (!m_Walking && Velocity.LengthSquared() > 0.05f)
		{
			m_AnimationPlayer.Stop();
			var directionSuffix = LookRight ? 'R' : 'L';
			m_AnimationPlayer.Play($"{m_WalkAnimationName}_{directionSuffix}");
			m_Walking = true;
		}
		else if(m_Walking && Velocity.LengthSquared() < 0.05f)
		{
			m_AnimationPlayer.Stop();
			m_AnimationPlayer.Play(m_IDLEAnimationName);
			m_Walking = false;
		}
	}
	
	protected void ResetWalkAnimationState()
	{
		m_AnimationPlayer.Stop();
		m_Walking = false;
	}
}