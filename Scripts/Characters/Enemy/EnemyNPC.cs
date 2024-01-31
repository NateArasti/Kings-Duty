using Godot;

public partial class EnemyNPC : FightNPC
{
	[Export] private int m_MaxHP = 10;
	[Export] private float m_AttackPlayerTimeout = 1;
	private float m_CurrentPlayerReachTimeout;
	
	public override void _Ready()
	{
		base._Ready();
		HealthSystem.MaxHealth = m_MaxHP;
		HealthSystem.CurrentHealth = m_MaxHP;
	}
	
	public override void _Process(double delta)
	{
		base._Process(delta);
		HandleTarget((float)delta);
		if (AttackFollowTarget) HandleAttackOffset();
	}

	private void HandleAttackOffset()
	{
		var direction = (GlobalPosition - FollowTarget.GlobalPosition).Normalized();
		
		FollowOffset = direction * (CanAttack ? AttackRange : AttackStayInRange);
	}

	private void HandleTarget(float delta)
	{
		var canReachPlayer = CanReachPlayer();
		
		if (canReachPlayer)
		{
			m_CurrentPlayerReachTimeout += delta;
			m_CurrentPlayerReachTimeout = Mathf.Min(m_CurrentPlayerReachTimeout, m_AttackPlayerTimeout + 1);
		}
		else
		{
			m_CurrentPlayerReachTimeout = 0;
		}
		
		var shouldAttackPlayer = m_CurrentPlayerReachTimeout >= m_AttackPlayerTimeout;
		
		if (shouldAttackPlayer)
		{
			if (FollowTarget != PlayerGlobalController.Instance.Player)
			{
				SubscribeToAttackTarget(PlayerGlobalController.Instance.Player);
			}
			
			return;
		}
		
		if (!AttackFollowTarget)
		{
			var possibleTarget = GetNonPlayerAttackTarget();
			SubscribeToAttackTarget(possibleTarget ?? PlayerGlobalController.Instance.Player);
		}
	}
	
	private AllyNPC GetNonPlayerAttackTarget()
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
	
	private bool CanReachPlayer()
	{
		if (RetinueController.Instance == null || RetinueController.Instance.CurrentAllies.Count == 0) return true;
		
		var playerDirection = PlayerGlobalController.Instance.Player.GlobalPosition - GlobalPosition;
		var distanceToPlayer = playerDirection.LengthSquared();
		
		foreach (var ally in RetinueController.Instance.CurrentAllies)
		{
			var directionToAlly = ally.GlobalPosition - GlobalPosition;
			var dot = playerDirection.Dot(directionToAlly);
			var distanceToAlly = directionToAlly.LengthSquared();
			if(distanceToAlly <= AttackRange || (dot > 0 && distanceToAlly < distanceToPlayer)) return false;
		}
		
		return true;
	}
}