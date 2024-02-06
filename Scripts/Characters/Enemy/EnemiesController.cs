using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public partial class EnemiesController : Node
{
	public static EnemiesController Instance { get; private set; }
	
	public event Action<EnemyNPC> OnEnemyDeath;
	
	[Export] private float m_SpawnRange;
	[Export] private int m_StartCurrency = 2;
	[Export] private float m_EnemySpawnCooldown = 1;
	[Export] private float m_WaveSpawnCooldown = 10;
	[Export] private float m_WaveAwaitAdd = 2;
	[Export] private PackedScene[] m_EnemyScenes;
	[Export] private int[] m_EnemyCosts;
	
	private int m_CurrentWaveMaxCost = 0;

	private readonly ConcurrentQueue<PackedScene> m_EnemySpawnRequests = new();
	private readonly HashSet<EnemyNPC> m_CurrentEnemies = new();

	public int KilledEnemeiesCount { get; private set; }
	public IReadOnlyCollection<EnemyNPC> CurrentEnemies => m_CurrentEnemies;

	public override void _Ready()
	{
		base._Ready();
		Instance = this;
		EnemyRequest();
		EnemySpawn();
	}
	
	private async void EnemySpawn()
	{
		while (true)
		{
			await Task.Delay((int)(m_EnemySpawnCooldown * 1000));
			if(m_EnemySpawnRequests.Count < 0) continue;
			
			if (m_EnemySpawnRequests.TryDequeue(out var enemyScene))
			{
				SpawnEnemyInstance(enemyScene);
			}
		}
	}
	
	private async void EnemyRequest()
	{
		while (true)
		{
			m_CurrentWaveMaxCost += m_StartCurrency;
			var awailableCurrency = m_CurrentWaveMaxCost;
			for (var i = m_EnemyCosts.Length - 1; i >= 0;)
			{
				if (awailableCurrency <= m_EnemyCosts[i])
				{
					i--;
					continue;
				}
				
				m_EnemySpawnRequests.Enqueue(m_EnemyScenes[i]);
				awailableCurrency -= m_EnemyCosts[i];
			}
			await Task.Delay((int)(m_WaveSpawnCooldown * 1000));
			m_WaveSpawnCooldown += m_WaveAwaitAdd;
		}
	}

	private void SpawnEnemyInstance(PackedScene enemyScene)
	{
		var enemy = enemyScene.Instantiate<EnemyNPC>();
		enemy.OnDeath += HandleEnemyDeath;
		var spawnOffset = RandomExtensions.RandomPointOnUnitCircle() * m_SpawnRange;
		enemy.Position = PlayerGlobalController.Instance.Player.GlobalPosition + new Vector3(spawnOffset.X, 0, spawnOffset.Y);
		AddChild(enemy);
		m_CurrentEnemies.Add(enemy);
	}
	
	private void HandleEnemyDeath(FightNPC instance)
	{
		if(instance is not EnemyNPC enemy) return;
		KilledEnemeiesCount++;
		m_CurrentEnemies.Remove(enemy);
		instance.QueueFree();
		
		OnEnemyDeath?.Invoke(enemy);
	}
}
