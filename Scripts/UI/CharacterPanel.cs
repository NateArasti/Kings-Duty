using Godot;

public partial class CharacterPanel : Node
{
	public event System.Action OnCharacterChosen;
	
	[Export] private Label m_NameLabel;
	[Export] private TextureRect m_MainRect;
	[Export] private TextureRect m_WeaponRect;
	
	[ExportGroup("Data labels")]
	[Export] private Label m_TypeLabel;
	[Export] private Label m_HealthLabel;
	[Export] private Label m_ResistanceLabel;
	[Export] private Label m_WeaponLabel;
	
	public Character Character { get; private set; }
	
	public void SetCharacter(Character character)
	{
		Character = character;
		m_NameLabel.Text = Character.Name;
		m_MainRect.Texture = Character.Sprite;
		
		m_HealthLabel.Text = $"Max HP: {character.MaxHP}";
		m_ResistanceLabel.Text = $"Resistance: {character.DefaultResistance}%";
		
		if(character is FightCharacter fightCharacter)
		{
			m_TypeLabel.Text = $"Type: {fightCharacter.Type}";
		}
		else
		{
			m_TypeLabel.Text = $"Type: None";			
		}
		
		Weapon weapon = null;
		if(character is MeleeCharacter meleeCharacter)
		{
			weapon = meleeCharacter.Weapon;
		}
		else if (character is RangeCharacter rangeCharacter)
		{
			weapon = rangeCharacter.Weapon;
		}
		
		if (weapon != null)
		{
			m_WeaponRect.Texture = weapon.Sprite;
			m_WeaponLabel.Text = $"Type: {weapon.ResourceName}";
		}
	}
	
	public void ChooseCharacter()
	{
		OnCharacterChosen?.Invoke();
	}
}