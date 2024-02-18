using Godot;

public partial class PauseScreen : Control
{
	[Export] private string m_MenuPath;
	[Export] private string m_PauseActionName;

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		if(@event.IsActionReleased(m_PauseActionName))
		{	
			if (Visible)
			{
				Continue();
			}
			else
			{
				Pause();				
			}
		}
	}
	
	public void Pause()
	{
		PauseSystem.Instance.Pause(this);
		Show();
		
	}

	public void Continue()
	{
		PauseSystem.Instance.Unpause(this);
		Hide();
	}
	
	public void ExitToMenu()
	{
		GetTree().ChangeSceneToFile(m_MenuPath);
	}
	
	public void ExitGame()
	{
		GetTree().Quit();
	}
}
