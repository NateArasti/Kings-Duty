using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Godot;

public partial class EnemiesController : Node
{
	public static EnemiesController Instance { get; private set; }
	
	public event Action<EnemyNPC> OnEnemyDeath;
	
	[Export] private float m_SpawnRange;
	[Export] private int m_StartCurrency = 2;
	[Export] private int m_BaseCurrency = 2;
	[Export] private float m_EnemySpawnCooldown = 1;
	[Export] private float m_WaveSpawnCooldown = 10;
	[Export] private float m_WaveAwaitAdd = 2;
	[Export] private PackedScene[] m_EnemyScenes;
	[Export] private int[] m_EnemyCosts;
	
	private int m_CurrentWaveMaxCost = 0;
	private float m_CurrentEnemySpawnCooldown;
	private float m_CurrentWaveSpawnCooldown;

	private readonly ConcurrentQueue<PackedScene> m_EnemySpawnRequests = new();
	private readonly HashSet<EnemyNPC> m_CurrentEnemies = new();

	public int KilledEnemeiesCount { get; private set; }
	public IReadOnlyCollection<EnemyNPC> CurrentEnemies => m_CurrentEnemies;

	public override void _EnterTree()
	{
		base._EnterTree();
		
		Instance = this;
	}

	public override void _Ready()
	{
		base._Ready();
		m_CurrentWaveMaxCost = m_StartCurrency;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		
		EnemyRequest((float)delta);
		EnemySpawn((float)delta);
	}

	private void EnemySpawn(float delta)
	{
		if (m_CurrentEnemySpawnCooldown > 0)
		{
			m_CurrentEnemySpawnCooldown -= delta;
			return;
		}
		
		if(m_EnemySpawnRequests.Count < 0) return;
		
		if (m_EnemySpawnRequests.TryDequeue(out var enemyScene))
		{
			SpawnEnemyInstance(enemyScene);
			m_CurrentEnemySpawnCooldown = m_EnemySpawnCooldown;
		}
	}
	
	private void EnemyRequest(float delta)
	{
		if (m_CurrentWaveSpawnCooldown > 0)
		{
			m_CurrentWaveSpawnCooldown -= delta;
			return;
		}
		
		m_CurrentWaveMaxCost += m_BaseCurrency;
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
		
		m_WaveSpawnCooldown += m_WaveAwaitAdd;
		m_CurrentWaveSpawnCooldown = m_WaveSpawnCooldown;
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
		
		OnEnemyDeath?.Invoke(enemy);
	}
}
