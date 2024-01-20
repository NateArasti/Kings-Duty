using System.Collections.Generic;
using Godot;

public partial class EnemiesController : Node
{
	public static EnemiesController Instance { get; private set; }
	
	[Export] private Vector3 m_SpawnOffset;
	[Export] private PackedScene m_EnemyScene;

	private readonly HashSet<EnemyNPC> m_CurrentEnemies = new();

	public IReadOnlyCollection<EnemyNPC> CurrentEnemies => m_CurrentEnemies;

	public override void _Ready()
	{
		base._Ready();
		Instance = this;
		SpawnEnemyInstance();
	}
	
	private void SpawnEnemyInstance()
	{
		var enemy = m_EnemyScene.Instantiate<EnemyNPC>();
		enemy.HealthSystem.OnDeath += () => m_CurrentEnemies.Remove(enemy);
		enemy.Position = PlayerGlobalController.Instance.Player.GlobalPosition + m_SpawnOffset;
		AddChild(enemy);
		m_CurrentEnemies.Add(enemy);
	}
	
	private void KillEnemy(EnemyNPC instance)
	{
		m_CurrentEnemies.Remove(instance);
		instance.QueueFree();
	}
}
