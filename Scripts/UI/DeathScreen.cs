using Godot;

public partial class DeathScreen : Control
{
	public void Restart()
	{
		GD.Print("HELLO");
		GetTree().Paused = false;
		GetTree().ReloadCurrentScene();
	}
}
