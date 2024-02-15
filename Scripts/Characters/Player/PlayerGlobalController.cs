using Godot;

public partial class PlayerGlobalController : Node
{	
	public static PlayerGlobalController Instance { get; private set; }
	
	[Export] public Node3D Player { get; private set; }
	[Export] public PlayerHealthSystem PlayerHealthSystem { get; private set; }
	[Export] public PlayerInteraction PlayerInteraction { get; private set; }

	public override void _Ready()
	{
		base._Ready();
		Instance = this;
	}
}
