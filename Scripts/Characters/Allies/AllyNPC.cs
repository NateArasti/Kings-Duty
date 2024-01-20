using Godot;

public partial class AllyNPC : FightNPC
{
	[Export] private CharacterVisuals m_Visuals;
	
	public Vector3 PlayerFollowOffset { get; set; }

	public override void _Process(double delta)
	{
		base._Process(delta);
		HandleTarget();
		if (AttackFollowTarget) HandleAttackOffset();
	}

	private void HandleAttackOffset()
	{
		var direction = (PlayerGlobalController.Instance.Player.GlobalPosition - FollowTarget.GlobalPosition).Normalized();
		
		FollowOffset = direction * (CanAttack ? AttackRange : AttackStayInRange);
	}

	private void HandleTarget()
	{
		if (!AttackFollowTarget)
		{
			var possibleTarget = GetNextTarget();
			if (possibleTarget != null)
			{
				SubscribeToAttackTarget(possibleTarget);
			}
		}
		
		if (!AttackFollowTarget || FollowTarget == null
			|| PlayerGlobalController.Instance.IsTooFarFromPlayer(FollowTarget))
		{
			UnsubscribeFromAttackTarget();
		}
	}

	public override void UnsubscribeFromAttackTarget()
	{
		base.UnsubscribeFromAttackTarget();
		
		FollowOffset = PlayerFollowOffset;
		FollowTarget = PlayerGlobalController.Instance.Player;
	}

	public void SetCharacterData(Character character)
	{
		var fightCharacter = character as FightCharacter;
		var weapon = fightCharacter?.Weapon;
		if (fightCharacter != null)
		{
			HealthSystem.MaxHealth = fightCharacter.MaxHP;
			HealthSystem.CurrentHealth = fightCharacter.MaxHP;
			AttackCooldown = weapon.AttackCooldown * fightCharacter.AttackSpeed;
			AttackRange = weapon.AttackRange;
			AttackDamage = weapon.AttackDamage;
		}
		m_Visuals.SetVisuals(character.Sprite, weapon.Sprite);
	}
	
	private EnemyNPC GetNextTarget()
	{
		if (EnemiesController.Instance == null || EnemiesController.Instance.CurrentEnemies.Count == 0) return null;
		
		(float sqrDistance, EnemyNPC chosenEnemy) nextTarget = (float.PositiveInfinity, null);
		foreach (var enemy in EnemiesController.Instance.CurrentEnemies)
		{
			var sqrDistance = GlobalPosition.DistanceSquaredTo(enemy.GlobalPosition);
			
			if (sqrDistance < nextTarget.sqrDistance && 
				sqrDistance <= AttackVisionRange * AttackVisionRange)
			{
				nextTarget = (sqrDistance, enemy);
			}
		}		
		
		return nextTarget.chosenEnemy;
	}
}