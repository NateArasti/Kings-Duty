using Godot;
using System;
using System.Collections.Generic;

public partial class RetinueController : Node
{
	public static RetinueController Instance { get; private set; }
	
	[Export] private RandomCharactersGenerator m_CharacterGenerator;
	[Export] private PackedScene m_AllyScene;
	[Export] private int m_RetinueStartCount = 2;
	[Export] private float m_RetinueOffset = 1;
	[Export] private float m_AttackAreaDistance = 3;
	
	private readonly List<AllyNPC> m_CurrentAllies = new();
	
	private readonly Dictionary<EnemyNPC, HashSet<AllyNPC>> m_EnemyTargetingData = new();
	private readonly Dictionary<AllyNPC, EnemyNPC> m_AlliesTargets = new();
	
	public IReadOnlyCollection<AllyNPC> CurrentAllies => m_CurrentAllies;

	public override void _Ready()
	{
		base._Ready();
		
		Instance = this;
		
		m_CharacterGenerator.OnCharactersCreated += AddCharactersToRetinue;
		EnemiesController.Instance.OnEnemyDeath += HandleEnemyDeath;
		GenerateAlly(m_RetinueStartCount);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		AdjustRetinueTargets();
	}
	
	private void AdjustRetinueTargets()
	{
		var enemies = EnemiesController.Instance.CurrentEnemies;
		if(m_CurrentAllies.Count != m_AlliesTargets.Count &&
			m_EnemyTargetingData.Count != enemies.Count)
		{
			foreach (var enemy in enemies)
			{
				if (m_EnemyTargetingData.ContainsKey(enemy) || IsTooFarFromPlayer(enemy)) continue;
				var bestDistance = float.PositiveInfinity;
				AllyNPC chosenAlly = null;
				foreach (var ally in m_CurrentAllies)
				{
					if (m_AlliesTargets.ContainsKey(ally)) continue;
					var sqrDistance = ally.GlobalPosition.DistanceSquaredTo(enemy.GlobalPosition);
					if (sqrDistance < bestDistance)
					{
						chosenAlly = ally;
						bestDistance = sqrDistance;
					}
				}
				
				if (chosenAlly != null)
				{
					chosenAlly.SubscribeToAttackTarget(enemy);
					m_EnemyTargetingData[enemy] = new() { chosenAlly };
					m_AlliesTargets[chosenAlly] = enemy;
				}
			}
		}
		if(m_CurrentAllies.Count != m_AlliesTargets.Count)
		{
			foreach (var ally in m_CurrentAllies)
			{
				if (m_AlliesTargets.ContainsKey(ally)) continue;
				var bestDistance = float.PositiveInfinity;
				EnemyNPC chosenEnemy = null;
				foreach (var enemy in enemies)
				{
					if (IsTooFarFromPlayer(enemy)) continue;
					var sqrDistance = ally.GlobalPosition.DistanceSquaredTo(enemy.GlobalPosition);
					if (sqrDistance < bestDistance)
					{
						chosenEnemy = enemy;
						bestDistance = sqrDistance;
					}
				}
				if(chosenEnemy != null)
				{
					ally.SubscribeToAttackTarget(chosenEnemy);
					m_EnemyTargetingData[chosenEnemy].Add(ally);
					m_AlliesTargets[ally] = chosenEnemy;
				}
			}
		}
		
		foreach (var ally in m_CurrentAllies)
		{
			if (IsTooFarFromPlayer(ally))
			{
				ally.UnsubscribeFromAttackTarget();
				RemoveAllyFightDependecies(ally);
			}
		}
	}

	public void GenerateAlly(int count = 1)
	{
		GetTree().Paused = true;
		
		m_CharacterGenerator.StartCharacterCreation(count);
	}

	private void AddCharactersToRetinue(IEnumerable<Character> allies)
	{
		foreach (var newAlly in allies)
		{
			SpawnAlly(newAlly);
		}
		UpdateRetinuePositioning();
		
		GetTree().Paused = false;
	}
	
	private void SpawnAlly(Character character)
	{
		var allyInstance = m_AllyScene.Instantiate<AllyNPC>();
		allyInstance.Visible = false;
		allyInstance.OnDeath += HandleAllyDeath;
		allyInstance.SetCharacterData(character);
		m_CurrentAllies.Add(allyInstance);
		AddChild(allyInstance);
	}
	
	private void UpdateRetinuePositioning()
	{
		for (var i = 0; i < m_CurrentAllies.Count; ++i)
		{
			var direction = Vector2.Right;
			direction = direction.Rotated(2 * Mathf.Pi * i / m_CurrentAllies.Count);
			var offset = new Vector3(direction.X, 0, direction.Y).Normalized() * m_RetinueOffset;
			m_CurrentAllies[i].PlayerFollowOffset = offset;
			if (!m_CurrentAllies[i].Visible)
			{
				m_CurrentAllies[i].Visible = true;
				m_CurrentAllies[i].GlobalPosition = PlayerGlobalController.Instance.Player.GlobalPosition + offset;
				m_CurrentAllies[i].UnsubscribeFromAttackTarget();			
			}
		}
	}

	public bool IsTooFarFromPlayer(Node3D target)
	{
		return target.GlobalPosition
			.DistanceSquaredTo(PlayerGlobalController.Instance.Player.GlobalPosition) > m_AttackAreaDistance * m_AttackAreaDistance;
	}
	
	public void HandleAllyDeath(FightNPC fightNPC)
	{
		if(fightNPC is not AllyNPC ally) return;
		m_CurrentAllies.Remove(ally);
		RemoveAllyFightDependecies(ally);
	}
	
	private void RemoveAllyFightDependecies(AllyNPC ally)
	{
		if (m_AlliesTargets.TryGetValue(ally, out var enemyTarget))
		{
			m_AlliesTargets.Remove(ally);
			if (m_EnemyTargetingData.TryGetValue(enemyTarget, out var allies))
			{
				if (allies.Contains(ally)) allies.Remove(ally);

				if (m_EnemyTargetingData[enemyTarget].Count == 0) m_EnemyTargetingData.Remove(enemyTarget);
			}
		}
	}

	private void HandleEnemyDeath(EnemyNPC enemy)
	{
		if (!m_EnemyTargetingData.TryGetValue(enemy, out var allies)) return;
		foreach (var ally in allies)
		{
			if(m_AlliesTargets.ContainsKey(ally))
			{
				m_AlliesTargets.Remove(ally);
			}
		}
		m_EnemyTargetingData.Remove(enemy);
	}

	public void HealAllies()
	{
		foreach (var ally in m_CurrentAllies)
		{
			ally.HealthSystem.HealFull();
		}
	}
}
