using Godot;
using System.Collections.Generic;

public partial class RetinueController : Node
{
	public static RetinueController Instance { get; private set; }
	
	[Export] private RandomCharactersGenerator m_CharacterGenerator;
	[Export] private PackedScene m_AllyScene;
	[Export] private float m_RetinueOffset = 1;
	[Export] private int m_StartRetinueCount = 4;
	
	private readonly List<AllyNPC> m_CurrentAllies = new();
	
	public IReadOnlyCollection<AllyNPC> CurrentAllies => m_CurrentAllies;

	public override void _Ready()
	{
		base._Ready();
		
		Instance = this;
		
		m_CharacterGenerator.OnCharactersCreated += AddCharactersToRetinue;
		GenerateAlly(m_StartRetinueCount);
	}
	
	public void GenerateAlly(int count = 1)
	{
		GetTree().Paused = true;
		
		m_CharacterGenerator.StartCharacterCreation(count);
	}

	private void AddCharactersToRetinue(IEnumerable<Character> allies)
	{
		foreach (var newAlly in allies)
		{
			SpawnAlly(newAlly);
		}
		UpdateRetinuePositioning();
		
		GetTree().Paused = false;
	}
	
	private void SpawnAlly(Character character)
	{
		var allyInstance = m_AllyScene.Instantiate<AllyNPC>();
		allyInstance.Visible = false;
		allyInstance.HealthSystem.OnDeath += () => m_CurrentAllies.Remove(allyInstance);
		allyInstance.SetCharacterData(character);
		m_CurrentAllies.Add(allyInstance);
		AddChild(allyInstance);
	}
	
	private void UpdateRetinuePositioning()
	{
		for (var i = 0; i < m_CurrentAllies.Count; ++i)
		{
			var direction = Vector2.Right;
			direction = direction.Rotated(2 * Mathf.Pi * i / m_CurrentAllies.Count);
			var offset = new Vector3(direction.X, 0, direction.Y).Normalized() * m_RetinueOffset;
			m_CurrentAllies[i].PlayerFollowOffset = offset;
			if (!m_CurrentAllies[i].Visible)
			{
				m_CurrentAllies[i].Visible = true;
				m_CurrentAllies[i].GlobalPosition = PlayerGlobalController.Instance.Player.GlobalPosition + offset;				
			}
		}
	}
}
