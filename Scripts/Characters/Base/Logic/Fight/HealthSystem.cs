using System;
using System.Threading.Tasks;
using Godot;

public partial class HealthSystem : Node
{
	public event Action OnDeath;
	
	[Export] private AnimationPlayer m_HitboxAnimationPlayer;
	[Export] private string m_HitAnimation;
	[Export] private string m_DeathAnimation;
	
	[Export] public Area3D HitboxArea { get; private set; }
	
	public int MaxHealth { get; set; }
	public int CurrentHealth { get; set; }
	
	public void TakeDamage(int damageCount)
	{
		CurrentHealth -= damageCount;
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
	
	private void ShowDeathEffect()
	{
		if(string.IsNullOrEmpty(m_DeathAnimation)) return;
		m_HitboxAnimationPlayer.Stop();
		m_HitboxAnimationPlayer.Play(m_DeathAnimation);
	}
	
	private void ShowHitEffect()
	{
		if(string.IsNullOrEmpty(m_HitAnimation)) return;
		m_HitboxAnimationPlayer.Stop();
		m_HitboxAnimationPlayer.Play(m_HitAnimation);
	}
}