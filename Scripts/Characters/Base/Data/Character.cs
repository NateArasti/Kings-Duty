using Godot;
using RandomFriendlyNameGenerator;

public partial class Character : Resource
{
	[Export] private NameGender m_Gender;
	[Export] private Texture2D[] m_PossibleSprites;
	[Export] private Vector2I m_MaxHPRange = new Vector2I(5, 15);
	[Export] private Vector2 m_DefaultResistanceRange;
	
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
		character.MaxHP = GD.RandRange(m_MaxHPRange.X, m_MaxHPRange.Y);
		character.DefaultResistance = (float)GD.RandRange(m_DefaultResistanceRange.X, m_DefaultResistanceRange.Y);
		character.Sprite = m_PossibleSprites[GD.RandRange(0, m_PossibleSprites.Length - 1)];
	}
}
