using System.Threading.Tasks;
using Godot;

public partial class PlayerHealthSystem : HealthSystem
{
	[Export] private int m_MaxHealth = 5;
	[Export] private ShaderMaterial m_PlayerVignette;
	[Export] private float m_HitValueChangeSpeed = 1;
	[Export] private string m_PlayerResetAnimation;
	
	private float m_CurrentHitValue = 0;
	private float m_TargetHitValue = 0;

	public override void _Ready()
	{
		MaxHealth = m_MaxHealth;
		CurrentHealth = m_MaxHealth;
		m_PlayerVignette.SetShaderParameter("value", m_CurrentHitValue);
		base._Ready();		
	}

	public override void TakeDamage(int damageCount)
	{
		base.TakeDamage(1);
		m_TargetHitValue = 1 - (float)CurrentHealth / MaxHealth;
	}

	protected override void ShowDeathEffect()
	{
		GetTree().Paused = true;
		DelayedDeathEffect();
	}
	
	private async void DelayedDeathEffect()
	{
		ShowHitEffect();
		await Task.Delay((int)(m_HitboxAnimationPlayer.GetAnimation(m_HitAnimation).Length * 1000));
		base.ShowDeathEffect();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		
		if (m_CurrentHitValue != m_TargetHitValue)
		{
			m_CurrentHitValue = (float) Mathf.MoveToward(m_CurrentHitValue, m_TargetHitValue, delta * m_HitValueChangeSpeed);
			m_PlayerVignette.SetShaderParameter("value", m_CurrentHitValue);
		}
	}
}
