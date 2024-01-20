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
	[Export] private Label m_AttackSpeedLabel;
	[Export] private Label m_ResistanceLabel;
	[Export] private Label m_WeaponNameLabel;
	[Export] private Label m_WeaponDamageLabel;
	[Export] private Label m_WeaponAttackCooldownLabel;
	
	public Character Character { get; private set; }
	
	public void SetCharacter(Character character)
	{
		Character = character;
		m_NameLabel.Text = Character.Name;
		m_MainRect.Texture = Character.Sprite;
		
		m_HealthLabel.Text = $"Max HP: {character.MaxHP}";
		
		m_ResistanceLabel.Text = $"Resistance: {character.DefaultResistance}%";
		
		Weapon weapon = null;
		
		if(character is FightCharacter fightCharacter)
		{
			m_TypeLabel.Text = $"Type: {fightCharacter.Type}";
			m_AttackSpeedLabel.Text = $"Attack Speed: {fightCharacter.AttackSpeed:0.00}";
			weapon = fightCharacter.Weapon;
		}
		else
		{
			m_TypeLabel.Text = $"Type: None";			
		}
		
		if (weapon != null)
		{
			m_WeaponRect.Texture = weapon.Sprite;
			m_WeaponNameLabel.Text = $"Type: {weapon.ResourceName}";
			m_WeaponDamageLabel.Text = $"Damage: {weapon.AttackDamage}";
			m_WeaponAttackCooldownLabel.Text = $"Cooldown: {weapon.AttackCooldown:0.00}";
		}
	}
	
	public void ChooseCharacter()
	{
		OnCharacterChosen?.Invoke();
	}
}