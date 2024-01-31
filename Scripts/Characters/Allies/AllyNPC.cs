using Godot;

public partial class AllyNPC : FightNPC
{
	[Export] private CharacterVisuals m_Visuals;
	
	public Vector3 PlayerFollowOffset { get; set; }

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (AttackFollowTarget) HandleAttackOffset();
	}

	private void HandleAttackOffset()
	{
		var direction = (PlayerGlobalController.Instance.Player.GlobalPosition - FollowTarget.GlobalPosition).Normalized();
		
		FollowOffset = direction * (CanAttack ? AttackRange : AttackStayInRange);
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
			AttackCooldown = Mathf.Max(0.1f, weapon.AttackCooldown / fightCharacter.AttackSpeed);
			AttackRange = weapon.AttackRange;
			AttackDamage = weapon.AttackDamage;
		}
		m_Visuals.SetVisuals(character.Sprite, weapon.Sprite);
	}
}