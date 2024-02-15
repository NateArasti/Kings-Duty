using Godot;

public partial class LevelUpInteraction : BaseInteraction
{
	[Export] private Button m_AddAllyButton;
	[Export] private Button m_HealAlliesButton;
	[Export] private Button m_HealSelfButton;

	public override void _Ready()
	{
		base._Ready();
		m_AddAllyButton.Pressed += AddAlly;
		m_HealAlliesButton.Pressed += HealAllies;
		m_HealSelfButton.Pressed += HealSelf;
	}

	public void AddAlly()
	{
		EndInteraction();
		RetinueController.Instance.GenerateAlly();
	}
	
	public void HealAllies()
	{
		EndInteraction();
		RetinueController.Instance.HealAllies();
	}
	
	public void HealSelf()
	{
		EndInteraction();
		PlayerGlobalController.Instance.PlayerHealthSystem.HealFull();
	}
}
