using Godot;

public partial class Statue : Node3D
{
	[Export] private Area3D m_TriggerArea;

	public override void _Ready()
	{
		base._Ready();
		m_TriggerArea.AreaEntered += OnAreaEntered;
	}

	public void OnAreaEntered(Area3D checkArea)
	{
		if (checkArea == PlayerGlobalController.Instance.PlayerInteraction.InteractionArea)
		{
			StartInteraction();
			m_TriggerArea.AreaEntered -= OnAreaEntered;
		}
	}
	
	public virtual void StartInteraction()
	{
		PlayerGlobalController.Instance.PlayerInteraction.StartInteraction<LevelUpInteraction>();
	}
}
