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
		PauseSystem.Instance.Pause(this);
	}
	
	public virtual void EndInteraction()
	{
		Hide();
		PauseSystem.Instance.Unpause(this);
	}
}
