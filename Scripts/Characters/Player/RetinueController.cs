using Godot;
using System.Collections.Generic;

public partial class RetinueController : Node
{
	[Export] private Node3D m_Player;
	[Export] private RandomCharactersGenerator m_CharacterGenerator;
	[Export] private PackedScene m_AllyScene;
	[Export] private float m_RetinueOffset = 1;
	[Export] private int m_StartRetinueCount = 4;
	
	private readonly List<AllyCharacter> m_CurrentAllies = new();

	public override void _Ready()
	{
		m_CharacterGenerator.OnCharactersCreated += AddCharactersToRetinue;
		
		m_CharacterGenerator.StartCharacterCreation(m_StartRetinueCount);
	}

	private void AddCharactersToRetinue(IEnumerable<Character> allies)
	{
		foreach (var newAlly in allies)
		{
			var allyInstance = m_AllyScene.Instantiate<AllyCharacter>();
			allyInstance.SetCharacterData(newAlly);
			allyInstance.Target = m_Player;
			m_CurrentAllies.Add(allyInstance);
			AddChild(allyInstance);
		}
		UpdateRetinuePositioning();
	}
	
	private void UpdateRetinuePositioning()
	{
		for (var i = 0; i < m_CurrentAllies.Count; ++i)
		{
			var direction = Vector2.Right;
			direction = direction.Rotated(2 * Mathf.Pi * i / m_CurrentAllies.Count);
			var offset = new Vector3(direction.X, 0, direction.Y).Normalized() * m_RetinueOffset;
			m_CurrentAllies[i].FollowOffset = offset;
			m_CurrentAllies[i].GlobalPosition = m_Player.GlobalPosition + offset;
		}
	}
}
