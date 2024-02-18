using Godot;

public partial class DeathScreen : Control
{
	public void Restart()
	{
		PauseSystem.Instance.Unpause(PlayerGlobalController.Instance.Player);
		GetTree().ReloadCurrentScene();
	}
}
