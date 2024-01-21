using System;
using System.Collections.Generic;
using Godot;

public partial class EnemiesController : Node
{
	public static EnemiesController Instance { get; private set; }
	
	public event Action OnEnemyDeath;
	
	[Export] private float m_SpawnRange;
	[Export] private int m_WaveBaseValue = 2;
	[Export] private float m_EnemySpawnCooldown = 1;
	[Export] private PackedScene m_EnemyScene;

	private float m_CurrentSpawnCooldown;
	
	private int m_WavesCount;
	private int m_CurrentWaveMaxEnemiesCount;

	private readonly HashSet<EnemyNPC> m_CurrentEnemies = new();

	public int KilledEnemeiesCount { get; private set; 
	}
	public IReadOnlyCollection<EnemyNPC> CurrentEnemies => m_CurrentEnemies;

	public override void _Ready()
	{
		base._Ready();
		Instance = this;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if(m_CurrentSpawnCooldown > 0) m_CurrentSpawnCooldown -= (float)delta;
		
		if(m_CurrentSpawnCooldown <= 0)
		{
			if (m_CurrentEnemies.Count == m_CurrentWaveMaxEnemiesCount)
			{
				m_CurrentWaveMaxEnemiesCount = GetWaveEnemyCount(m_WavesCount);
			}
			
			if(m_CurrentEnemies.Count < m_CurrentWaveMaxEnemiesCount)
			{
				SpawnEnemyInstance();
				m_CurrentSpawnCooldown = m_EnemySpawnCooldown;
				
				if (m_CurrentEnemies.Count == m_CurrentWaveMaxEnemiesCount)
				{
					m_WavesCount++;
					m_CurrentWaveMaxEnemiesCount = Mathf.RoundToInt(0.3f * m_CurrentWaveMaxEnemiesCount);
				}				
			}
		}
	}
	
	private int GetWaveEnemyCount(int waveIndex)
	{
		var waveOffset = waveIndex / 2;
		return m_WaveBaseValue + m_WaveBaseValue * waveIndex * (1 + GlobalTimer.CurrentTime.Minutes) + GD.RandRange(-waveOffset, waveOffset);
	}

	private void SpawnEnemyInstance()
	{
		var enemy = m_EnemyScene.Instantiate<EnemyNPC>();
		enemy.HealthSystem.OnDeath += () => KillEnemy(enemy);
		var spawnOffset = RandomExtensions.RandomPointOnUnitCircle() * m_SpawnRange;
		enemy.Position = PlayerGlobalController.Instance.Player.GlobalPosition + new Vector3(spawnOffset.X, 0, spawnOffset.Y);
		AddChild(enemy);
		m_CurrentEnemies.Add(enemy);
	}
	
	private void KillEnemy(EnemyNPC instance)
	{
		KilledEnemeiesCount++;
		m_CurrentEnemies.Remove(instance);
		instance.QueueFree();
		
		OnEnemyDeath?.Invoke();
	}
}
