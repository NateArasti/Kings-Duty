using System;
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
		m_BodySprite.Modulate = Colors.White.Lerp(Colors.Red, (MaxHealth - CurrentHealth) / (float)MaxHealth);
		if (CurrentHealth <= 0)
		{
			OnDeath?.Invoke();
		}
	}
}