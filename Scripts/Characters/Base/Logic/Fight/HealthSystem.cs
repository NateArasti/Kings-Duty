using System;
using Godot;

public partial class HealthSystem : Node
{
	public event Action OnDeath;
	
	[Export] protected AnimationPlayer m_HitboxAnimationPlayer;
	[Export] protected GpuParticles3D m_HealEffect;
	[Export] protected string m_HitAnimation;
	[Export] protected string m_DeathAnimation;
	[Export] protected string m_ResetAnimation;
	
	[Export] public Area3D HitboxArea { get; private set; }
	
	public int MaxHealth { get; set; }
	public int CurrentHealth { get; set; }

	public override void _Ready()
	{
		base._Ready();
		m_HitboxAnimationPlayer.Play(m_ResetAnimation);
	}

	public virtual void TakeDamage(int damageCount)
	{
		CurrentHealth -= damageCount;
		OnHPChanged();
		if (CurrentHealth <= 0)
		{
			ShowDeathEffect();
			OnDeath?.Invoke();
		}
		else
		{
			ShowHitEffect();			
		}
	}
	
	protected virtual void OnHPChanged() { }
	
	public void HealFull()
	{
		CurrentHealth = MaxHealth;
		OnHPChanged();
		ShowHealEffect();
	}
	
	protected virtual void ShowDeathEffect()
	{
		if(string.IsNullOrEmpty(m_DeathAnimation)) return;
		m_HitboxAnimationPlayer.Stop();
		m_HitboxAnimationPlayer.Play(m_DeathAnimation);
	}
	
	protected virtual void ShowHitEffect()
	{
		if(string.IsNullOrEmpty(m_HitAnimation)) return;
		m_HitboxAnimationPlayer.Stop();
		m_HitboxAnimationPlayer.Play(m_HitAnimation);
	}
	
	protected virtual void ShowHealEffect()
	{
		m_HealEffect.Emitting = true;
	}
}