using System;
using System.Collections.Generic;
using Godot;

public partial class RandomCharactersGenerator : Control
{
	public event Action<IEnumerable<Character>> OnCharactersCreated;
	
	[Export] private Character[] m_CharactersDatas;
	[Export] private CharacterPanel[] m_CharacterPanels;
	[Export] private Label m_CharactersCoundown;
	
	private int m_CharactersToCreateCount;
	
	private readonly Queue<Character> m_GeneratedCharacters = new();

	public override void _Ready()
	{
		for (var i = 0; i < m_CharacterPanels.Length; ++i)
		{
			var j = i;
			m_CharacterPanels[i].OnCharacterChosen += () => ChooseCharacter(j);
		}
		SetRandomCharacters();
	}
	
	public void StartCharacterCreation(int charactersToCreate)
	{
		m_GeneratedCharacters.Clear();
		m_CharactersToCreateCount = charactersToCreate;
		m_CharactersCoundown.Text = charactersToCreate < 2 ? string.Empty : $"1/{charactersToCreate}";
		SetRandomCharacters();
		Show();
	}

	private void ChooseCharacter(int i)
	{
		m_GeneratedCharacters.Enqueue(m_CharacterPanels[i].Character);
		if (m_GeneratedCharacters.Count == m_CharactersToCreateCount)
		{
			OnCharactersCreated?.Invoke(m_GeneratedCharacters);
			Hide();
		}
		else
		{
			m_CharactersCoundown.Text = $"{m_GeneratedCharacters.Count}/{m_CharactersToCreateCount}";
			SetRandomCharacters();			
		}
	}
	
	private void SetRandomCharacters()
	{
		for (var i = 0; i < m_CharacterPanels.Length; ++i)
		{
			m_CharacterPanels[i].SetCharacter(m_CharactersDatas[GD.RandRange(0, m_CharactersDatas.Length - 1)].GetInstance());
		}
	}
}
