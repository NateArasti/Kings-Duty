using System;
using System.Threading.Tasks;
using Godot;

public partial class HealthSystem : Node
{
	public event Action OnDeath;
	
	[Export] private Sprite3D m_BodySprite;
	
	public int MaxHealth { get; set; }
	public int CurrentHealth { get; set; }
	
	public void TakeDamage(int damageCount)
	{
		CurrentHealth -= damageCount;
		if (CurrentHealth <= 0)
		{
			OnDeath?.Invoke();
		}
		else
		{
			ShowHitEffect();			
		}
	}
	
	private async void ShowHitEffect()
	{
		(m_BodySprite.MaterialOverlay as ShaderMaterial).SetShaderParameter("active", true);
		await Task.Delay(100);
		(m_BodySprite.MaterialOverlay as ShaderMaterial).SetShaderParameter("active", false);
	}
}