using Godot;

public partial class MainMenu : Node
{
	[Export] private PackedScene m_GameScene;
	[Export] private string m_ReviewURL;
	
	public void StartGame()
	{
		GetTree().ChangeSceneToPacked(m_GameScene);
	}
	
	public void LeaveReview()
	{
		OS.ShellOpen(m_ReviewURL);
	}
	
	public void ExitGame()
	{
		GetTree().Quit();
	}
}
