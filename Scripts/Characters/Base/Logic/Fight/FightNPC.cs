using System;
using Godot;

public abstract partial class FightNPC : MoveableNPC, IHittable
{	
	[Export] private string m_MeleeAttackAnimaitonName = "MeleeAttack";
	[Export] private string m_RangeAttackAnimaitonName = "RangeAttack";
	
	private float m_CurrentAttackCooldown;
	
	protected bool CanAttack => AttackFollowTarget && FollowTarget != null && m_CurrentAttackCooldown <= 0;
	
	protected bool AttackFollowTarget { get; set; }	
	
	[Export] public float AttackRange { get; private set; } = 0.5f;
	[Export] public float AttackStayInRange { get; private set; } = 1.5f;
	[Export] public float AttackCooldown { get; private set; } = 1.5f;
	[Export] public float AttackVisionRange { get; private set; } = 5f;
	
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
		HealthSystem.OnDeath += OnDeath;
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

	protected virtual void OnDeath()
	{
		QueueFree();
	}
	
	public virtual void SubscribeToAttackTarget(Node3D attackTarget)
	{
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
		m_AnimationPlayer.Stop();
		m_AnimationPlayer.Play(m_MeleeAttackAnimaitonName);
		m_AnimationPlayer.AnimationFinished += FinishAttack;
	}

	private void FinishAttack(StringName animName)
	{
		if (FollowTarget is IHittable hittable)
		{
			hittable.HealthSystem.TakeDamage(2);
		}
		InAttack = false;
		m_CurrentAttackCooldown = AttackCooldown;
		m_AnimationPlayer.AnimationFinished -= FinishAttack;
		ResetWalkAnimationState();
	}
}