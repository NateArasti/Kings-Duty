using Godot;

public partial class Statue : Node3D
{
	[Export] private Area3D m_TriggerArea;
	[Export] private Node3D m_Visuals;
	[Export] private AnimationPlayer m_AnimationPlayer;
	[Export] private string m_DestroyAnimationName;
	[Export] private string m_ResetAnimationName;

	private bool m_Enabled;

	public override void _Ready()
	{
		base._Ready();
		m_TriggerArea.AreaEntered += OnAreaEntered;
		Progression.Instance.OnLevelUpStart += Enable;
		Progression.Instance.OnLevelUpEnd += Disable;
	}

	public void Reset()
	{
		Disable();
		m_AnimationPlayer.Play(m_ResetAnimationName);
	}
	
	private void Enable()
	{
		m_Enabled = true;
		m_Visuals.Visible = true;
	}
	
	private void Disable()
	{
		m_Enabled = false;
		m_Visuals.Visible = false;
	}

	public void OnAreaEntered(Area3D checkArea)
	{
		if (!m_Enabled) return;
		
		if (checkArea == PlayerGlobalController.Instance.PlayerInteraction.InteractionArea)
		{
			PlayerGlobalController.Instance.PlayerInteraction.StartInteraction<LevelUpInteraction>();
			m_AnimationPlayer.Play(m_DestroyAnimationName);
			Disable();
			Progression.Instance.FinishLevelUp();
		}
	}
}
