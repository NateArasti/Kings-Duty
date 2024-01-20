using Godot;
using RandomFriendlyNameGenerator;

public partial class Character : Resource
{
	[Export] private NameGender m_Gender;
	[Export] private Texture2D[] m_PossibleSprites;
	[Export] private int m_MaxHP = 10;
	[Export(PropertyHint.Range, "0, 100")] private float m_DefaultResistance = 0;
	
	public string Name { get; private set; }
	public Texture2D Sprite { get; private set; }
	public int MaxHP { get; private set; }
	public float DefaultResistance { get; private set; }
	
	public virtual Character GetInstance()
	{
		var instance = new Character();
		FillCharacterInstance(instance);
		return instance;
	}
	
	protected virtual void FillCharacterInstance(Character character)
	{
		character.Name = NameGenerator.PersonNames.Get(m_Gender, NameComponents.FirstNameOnly);
		character.MaxHP = m_MaxHP;
		character.DefaultResistance = m_DefaultResistance;
		character.Sprite = m_PossibleSprites[GD.RandRange(0, m_PossibleSprites.Length - 1)];
	}
}
