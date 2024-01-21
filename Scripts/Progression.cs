using Godot;

public partial class Progression : Node
{
	[Export] private ProgressBar m_CurrentProgress;
	[Export] private int m_BaseEnemyKillsPerLevel = 5;

	private int m_CurrentLevel;

	public override void _Ready()
	{
		base._Ready();
		// EnemiesController.Instance.OnEnemyDeath += UpdateProgress;
	}

	public override void _Process(double delta)
	{
		UpdateProgress();
	}

	private void UpdateProgress()
	{
		var currentEnemyKillCount = EnemiesController.Instance.KilledEnemeiesCount;
		var shouldBeLevel = GetCorrespondingLevel(currentEnemyKillCount);
		if (shouldBeLevel > m_CurrentLevel)
		{
			LevelUp();
		}
		
		m_CurrentProgress.Value = Mathf.Remap(currentEnemyKillCount, GetCorrespondingEnemyKillCount(m_CurrentLevel), GetCorrespondingEnemyKillCount(m_CurrentLevel + 1), 0f, 1f);
	}

	private void LevelUp()
	{
		GD.Print("LevelUp");
		RetinueController.Instance.GenerateAlly();
		m_CurrentLevel++;
	}
	
	private int GetCorrespondingLevel(int enemyKillCount)
	{
		return enemyKillCount / m_BaseEnemyKillsPerLevel;
	}
	
	private int GetCorrespondingEnemyKillCount(int level)
	{
		return level * m_BaseEnemyKillsPerLevel;	
	}
}
