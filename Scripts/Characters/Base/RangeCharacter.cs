using Godot;

public partial class RangeCharacter : FightCharacter
{
	[Export] private RangeWeapon[] m_PossibleWeapons;
	
	public RangeWeapon ChosenWeapon { get; private set; }
    public override Weapon Weapon => ChosenWeapon;

	public override Character GetInstance()
	{
		var instance = new RangeCharacter();
		FillCharacterInstance(instance);
		return instance;
	}

	protected override void FillCharacterInstance(Character character)
	{
		base.FillCharacterInstance(character);
		var rangeCharacter = character as RangeCharacter;
		if(m_PossibleWeapons.Length > 0)
			rangeCharacter.ChosenWeapon = m_PossibleWeapons[GD.RandRange(0, m_PossibleWeapons.Length - 1)];
	}
}
