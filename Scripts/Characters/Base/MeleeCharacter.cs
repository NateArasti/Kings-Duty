using Godot;

public partial class MeleeCharacter : FightCharacter
{
	[Export] private Resource[] m_PossibleWeapons;
	
	protected MeleeWeapon ChosenWeapon { get; private set; }

    public override Weapon Weapon => ChosenWeapon;

    public override Character GetInstance()
	{
		var instance = new MeleeCharacter();
		FillCharacterInstance(instance);
		return instance;
	}

	protected override void FillCharacterInstance(Character character)
	{
		base.FillCharacterInstance(character);
		var meleeCharacter = character as MeleeCharacter;
		if(m_PossibleWeapons.Length > 0)
			meleeCharacter.ChosenWeapon = m_PossibleWeapons[GD.RandRange(0, m_PossibleWeapons.Length - 1)] as MeleeWeapon;
	}
}
