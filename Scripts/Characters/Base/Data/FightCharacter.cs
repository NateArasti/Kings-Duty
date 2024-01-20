using Godot;

public partial  class FightCharacter : Character
{
	[Export] private string m_Type = "None";
	[Export] private float m_AttackSpeed = 1;

	public string Type { get; private set; }
	public float AttackSpeed { get; private set; }
	
	public virtual Weapon Weapon => null;
	
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
		fightCharacter.Type = m_Type;
		fightCharacter.AttackSpeed = m_AttackSpeed;
	}
}
