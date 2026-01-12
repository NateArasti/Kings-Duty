using Godot;

public partial  class FightCharacter : Character
{
	[Export] public string Type { get; private set; } = "None";
	[Export] private Vector2 m_AttackSpeedRange = new Vector2(0.9f, 1.1f);
	[Export] private Weapon[] m_PossibleWeapons;
	
	public float AttackSpeed { get; private set; }
	
	public virtual Weapon Weapon { get; private set; }
	
	public override Character GetInstance()
	{
		var instance = new FightCharacter();
		FillCharacterInstance(instance);
		return instance;
	}

	protected override void FillCharacterInstance(Character character)
	{
		base.FillCharacterInstance(character);
		var fightCharacter = character as FightCharacter;
		fightCharacter.Type = Type;
		fightCharacter.AttackSpeed = (float)GD.RandRange(m_AttackSpeedRange.X, m_AttackSpeedRange.Y);
		fightCharacter.Weapon = m_PossibleWeapons.Random();
	}
}
