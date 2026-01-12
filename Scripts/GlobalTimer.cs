using System;
using Godot;

public partial class GlobalTimer : Node
{
	public static TimeSpan CurrentTime { get; private set; }
	
	[Export] private Label m_TimerLabel;

	public override void _EnterTree()
	{
		base._EnterTree();
		CurrentTime = new TimeSpan();
	}

	public override void _Process(double delta)
	{
		CurrentTime += new TimeSpan(0, 0, 0, 0, (int)(delta * 1000));
		
		m_TimerLabel.Text = CurrentTime.ToString("mm':'ss':'ff");
	}
}
