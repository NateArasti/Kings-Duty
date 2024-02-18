using System;
using Godot;

public partial class Progression : Node
{
	public event Action OnLevelUpStart;
	public event Action OnLevelUpEnd;
	
	public static Progression Instance { get; private set; }
	
	[Export] private ProgressBar m_CurrentProgress;
	[Export] private ProgressBar m_ShouldBeProgress;
	[Export] private float m_VisualsChangeSpeed = 1f;
	[Export] private int m_BaseExperiencePerLevel = 5;
	[Export] private float m_TimeExperienceModifier = 1;

	private int m_CurrentLevel;
	private float m_CurrentExperience;
	private bool m_IsWaitingForLevelUp;
	
	private int m_LastCheckedSeconds;

    public override void _EnterTree()
    {
        base._EnterTree();
		Instance = this;
    }

    public override void _Ready()
	{
		base._Ready();
		m_CurrentProgress.Value = 0;
		m_ShouldBeProgress.Value = 0;
		EnemiesController.Instance.OnEnemyDeath += AddProgressForEnemyKill;
	}

	private void AddProgressForEnemyKill(EnemyNPC enemy)
	{
		UpdateProgress(1);
	}

	public override void _Process(double delta)
	{
		AddTimeProgress();
		UpdateVisuals(delta);
	}

	private void UpdateProgress(float addExp)
	{
		if (m_IsWaitingForLevelUp == true) return;
		
		m_CurrentExperience += addExp;
		if(GetCorrespondingLevel(m_CurrentExperience) > m_CurrentLevel)
		{
			StartLevelUp();
		}
		
		m_ShouldBeProgress.Value = Mathf.Remap(
			m_CurrentExperience, 
			GetCorrespondingExperience(m_CurrentLevel), 
			GetCorrespondingExperience(m_CurrentLevel + 1), 
			0f, 1f);
	}
	
	private void AddTimeProgress()
	{
		var currentSeconds = GlobalTimer.CurrentTime.Seconds;
		if(currentSeconds == m_LastCheckedSeconds) return;
		var delta = currentSeconds - m_LastCheckedSeconds;
		m_LastCheckedSeconds = currentSeconds;
		UpdateProgress(delta * m_TimeExperienceModifier);
	}
	
	private void UpdateVisuals(double delta)
	{
		m_CurrentProgress.Value = Mathf.MoveToward(m_CurrentProgress.Value, m_ShouldBeProgress.Value, Mathf.Min(m_CurrentProgress.Step, m_VisualsChangeSpeed * delta));
	}

	public void StartLevelUp()
	{
		OnLevelUpStart?.Invoke();
		m_IsWaitingForLevelUp = true;
	}
	
	public void FinishLevelUp()
	{
		OnLevelUpEnd?.Invoke();	
		m_IsWaitingForLevelUp = false;
		m_CurrentLevel++;
		m_CurrentProgress.Value = 0;
		m_ShouldBeProgress.Value = 0;
	}
	
	private int GetCorrespondingLevel(float experience)
	{
		return (int) (experience / m_BaseExperiencePerLevel);
	}
	
	private float GetCorrespondingExperience(int level)
	{
		return level * m_BaseExperiencePerLevel;
	}
}
