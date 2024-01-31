using Godot;

public partial class PlayerGlobalController : Node
{	
	public static PlayerGlobalController Instance { get; private set; }
	
	[Export] public Node3D Player { get; private set; }

	public override void _Ready()
	{
		base._Ready();
		Instance = this;
	}
}
