using Godot;

public partial class AllyNPC : FightNPC
{
	[Export] private GeometryInstance3D m_GenericVisuals;
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
		var shaderMaterial = m_GenericVisuals.MaterialOverride as ShaderMaterial;
		GenericLayeredCharacters.Utility.ClearAllLayers(shaderMaterial);
		GenericLayeredCharacters.Utility.SetElementVariationsOnMaterial(shaderMaterial, character.Visuals.Values);
		if (character is FightCharacter fightCharacter)
		{
			var weapon = fightCharacter.Weapon;
			GenericLayeredCharacters.Utility.SetElementVariationOnMaterial(shaderMaterial, weapon.Visuals);
			
			// HealthSystem.MaxHealth = fightCharacter.MaxHP;
			// HealthSystem.CurrentHealth = fightCharacter.MaxHP;
			AttackCooldown = Mathf.Max(0.1f, weapon.AttackCooldown / fightCharacter.AttackSpeed);
			AttackRange = weapon.AttackRange;
			AttackDamage = weapon.AttackDamage;
		}
	}
}