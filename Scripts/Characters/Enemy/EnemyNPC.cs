using Godot;

public partial class EnemyNPC : FightNPC
{
	[Export] private int MaxHP = 5;

	public override void _Ready()
	{
		base._Ready();
		HealthSystem.MaxHealth = MaxHP;
		HealthSystem.CurrentHealth = MaxHP;
	}
	public override void _Process(double delta)
	{
		base._Process(delta);
		HandleTarget();
		if (AttackFollowTarget) HandleAttackOffset();
	}

    protected override void OnDeath()
    {
    }

    private void HandleAttackOffset()
	{
		var direction = (GlobalPosition - FollowTarget.GlobalPosition).Normalized();
		
		FollowOffset = direction * (CanAttack ? AttackRange : AttackStayInRange);
	}

	private void HandleTarget()
	{
		if (!AttackFollowTarget || FollowTarget == PlayerGlobalController.Instance.Player)
		{
			var possibleTarget = GetNextTarget();
			SubscribeToAttackTarget(possibleTarget ?? PlayerGlobalController.Instance.Player);
		}
	}
	
	private AllyNPC GetNextTarget()
	{
		if (RetinueController.Instance == null || RetinueController.Instance.CurrentAllies.Count == 0) return null;
		
		(float sqrDistance, AllyNPC chosenEnemy) nextTarget = (float.PositiveInfinity, null);
		foreach (var ally in RetinueController.Instance.CurrentAllies)
		{
			var sqrDistance = GlobalPosition.DistanceSquaredTo(ally.GlobalPosition);
			
			if (sqrDistance < nextTarget.sqrDistance && 
				sqrDistance <= AttackVisionRange * AttackVisionRange)
			{
				nextTarget = (sqrDistance, ally);
			}
		}		
		
		return nextTarget.chosenEnemy;
	}
}