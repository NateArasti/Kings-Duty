using Godot;

public partial class PlayerInteraction : Node
{
	public enum InteractionType
	{
		LevelUp,
	}
	
	[Export] private BaseInteraction[] m_Interactions;
	[Export] public Area3D InteractionArea { get; private set; }
	
	public void StartInteraction<T>() where T : BaseInteraction
	{
		foreach (var interaction in m_Interactions)
		{
			if(interaction is T)
			{
				interaction.StartInteraction();
				return;
			}
		}
	}
}
