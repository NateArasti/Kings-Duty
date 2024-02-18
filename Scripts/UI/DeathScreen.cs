using Godot;

public partial class DeathScreen : Control
{
	public void Restart()
	{
		GetTree().Paused = false;
		GetTree().ReloadCurrentScene();
	}
}
