using Godot;

public partial class AllyCharacter : CharacterFollower
{
	[Export] private CharacterVisuals m_Visuals;
	
	[ExportGroup("Animations")]
	[Export] private AnimationPlayer m_AnimationPlayer;
	[Export] private string m_IDLEAnimationName = "Idle";
	[Export] private string m_WalkAnimationName = "Walk";
	
	private bool m_Walking;
	private bool m_LookRight = true;
	
	public void SetCharacterData(Character character)
	{
		var weapon = (character as FightCharacter)?.Weapon;
		m_Visuals.SetVisuals(character.Sprite, weapon.Sprite);
	}

	public override void _Process(double delta)
	{
		if ((Velocity.Z > 0 && !m_LookRight) || (Velocity.Z < 0 && m_LookRight))
		{
			ToggleLook();
		}
		
		HandleAnimation();
	}
	
	private void ToggleLook()
	{
		m_LookRight = !m_LookRight;
		toggleLoop(m_Visuals);
		
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
		if (!m_Walking && Velocity.LengthSquared() > 0.05f)
		{
			m_AnimationPlayer.Stop();
			m_AnimationPlayer.Play(m_WalkAnimationName);
			m_Walking = true;
		}
		else if(m_Walking && Velocity.LengthSquared() < 0.05f)
		{
			m_AnimationPlayer.Stop();
			m_AnimationPlayer.Play(m_IDLEAnimationName);
			m_Walking = false;
		}
	}
}