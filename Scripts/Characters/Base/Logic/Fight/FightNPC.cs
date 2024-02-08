using System;
using Godot;

public partial class FightNPC : MoveableNPC, IHittable
{	
	public event Action<FightNPC> OnDeath;
	
	[Export] private Area3D m_MeleeAttackArea;
	[Export] private string m_MeleeAttackAnimaitonName = "FightCharacter/MeleeAttack";
	
	private float m_CurrentAttackCooldown;
	
	private IHittable m_CurrentAttackTarget;
	
	protected bool CanAttack => AttackFollowTarget && FollowTarget != null && m_CurrentAttackCooldown <= 0;
	
	protected bool AttackFollowTarget { get; set; }	
	
	[Export] public float AttackStayInRange { get; private set; } = 1.5f;
	[Export] public int AttackDamage { get; protected set; } = 10;
	[Export] public float AttackRange { get; protected set; } = 0.5f;
	[Export] public float AttackCooldown { get; protected set; } = 1.5f;
	
	[Export] public HealthSystem HealthSystem { get; private set; }
	
	public bool InAttack { get; private set; }
	
	protected FollowSettings AttackFollowSettings { get; } = new()
	{
		FollowSpaceDistance = 0f,
		MoveSpeedGradient = new Vector2(1.25f, 1.75f),
		RotationSpeed = 2,
	};

	public override void _Ready()
	{
		base._Ready();
		HealthSystem.OnDeath += Death;
	}
	
	public override void _Process(double delta)
	{		
		if(m_CurrentAttackCooldown > 0) m_CurrentAttackCooldown -= (float)delta;
		
		if (!InAttack && CanAttack && FollowTarget.GlobalPosition.DistanceSquaredTo(GlobalPosition) < AttackRange * AttackRange)
		{
			Attack();
		}
			
		base._Process(delta);		
	}

	protected void Death()
	{
		OnDeath?.Invoke(this);
	}
	
	public virtual void SubscribeToAttackTarget(Node3D attackTarget)
	{
		if(attackTarget == null) throw new NullReferenceException("Provided attack target was null");
		FollowTarget = attackTarget;
		
		if (FollowTarget is IHittable hittable)
		{
			hittable.HealthSystem.OnDeath += UnsubscribeFromAttackTarget;
		}
		
		LookAtTarget = true;
		AttackFollowTarget = true;
		StrictOffsetFollow = false;	
		Settings = AttackFollowSettings;	
	}
	
	public virtual void UnsubscribeFromAttackTarget()
	{
		FollowTarget = null;
		m_CurrentAttackTarget = null;
		
		if (FollowTarget is IHittable hittable)
		{
			hittable.HealthSystem.OnDeath -= UnsubscribeFromAttackTarget;
		}
		
		LookAtTarget = false;
		AttackFollowTarget = false;
		StrictOffsetFollow = true;	
		Settings = SimpleFollowSettings;
	}

	protected override void HandleAnimation()
	{
		if (InAttack) return;
		base.HandleAnimation();	
	}

	protected virtual void Attack()
	{
		InAttack = true;
		m_CurrentAttackTarget = FollowTarget as IHittable;
		m_AnimationPlayer.Stop();
		var directionSuffix = LookRight ? 'R' : 'L';
		m_AnimationPlayer.Play($"{m_MeleeAttackAnimaitonName}_{directionSuffix}");
		m_AnimationPlayer.AnimationFinished += FinishAttack;
	}

	private void FinishAttack(StringName animName)
	{
		InAttack = false;
		m_CurrentAttackCooldown = AttackCooldown;
		m_AnimationPlayer.AnimationFinished -= FinishAttack;
		ResetWalkAnimationState();
	}
	
	public virtual void PlaceAttackArea()
	{
		if (m_CurrentAttackTarget == null) return;
		var attackDirection = FollowTarget.GlobalPosition - GlobalPosition;
		attackDirection.Y = 0;
		m_MeleeAttackArea.GlobalPosition = GlobalPosition + attackDirection.Normalized() * AttackRange;
		TryProvideAttack();
	}
	
	public virtual void TryProvideAttack()
	{
		if (m_CurrentAttackTarget != null && m_MeleeAttackArea.OverlapsArea(m_CurrentAttackTarget.HealthSystem.HitboxArea))
		{
			m_CurrentAttackTarget.HealthSystem.TakeDamage(AttackDamage);
		}
	}
}