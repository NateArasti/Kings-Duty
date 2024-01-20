using Godot;

public partial class PlayerGlobalController : Node
{	
	public static PlayerGlobalController Instance { get; private set; }
	
	[Export] public Node3D Player { get; private set; }
	[Export] private float m_EnemyFightRadius = 7;

	public override void _Ready()
	{
		base._Ready();
		Instance = this;
	}

    public bool IsTooFarFromPlayer(Node3D target)
    {
        return target.GlobalPosition.DistanceSquaredTo(Player.GlobalPosition) > m_EnemyFightRadius * m_EnemyFightRadius;
    }

}
