using Godot;

public partial class Statue : Node3D
{
	[Export] private Area3D m_TriggerArea;
	[Export] private AnimationPlayer m_AnimationPlayer;
	[Export] private string m_DestroyAnimationName;
	[Export] private string m_ResetAnimationName;

	private bool m_Enabled;

	public override void _Ready()
	{
		base._Ready();
		m_Enabled = true;
		m_TriggerArea.AreaEntered += OnAreaEntered;
	}

	public void Reset()
	{
		m_Enabled = true;
		m_AnimationPlayer.Play(m_ResetAnimationName);
	}

	public void OnAreaEntered(Area3D checkArea)
	{
		if (m_Enabled) return;
		
		if (checkArea == PlayerGlobalController.Instance.PlayerInteraction.InteractionArea)
		{
			PlayerGlobalController.Instance.PlayerInteraction.StartInteraction<LevelUpInteraction>();
			m_AnimationPlayer.Play(m_DestroyAnimationName);
			m_Enabled = false;
		}
	}
}
