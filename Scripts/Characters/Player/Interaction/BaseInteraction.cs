using Godot;

public partial class BaseInteraction : Control
{
	public override void _Ready()
	{
		base._Ready();
		Hide();
	}

	public virtual void StartInteraction()
	{
		Show();
		GetTree().Paused = true;
	}
	
	public virtual void EndInteraction()
	{
		Hide();
		GetTree().Paused = false;
	}
}
