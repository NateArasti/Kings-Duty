using Godot;
using System.Collections.Generic;

public partial class PauseSystem : Node
{
	public static PauseSystem Instance { get; private set; }
	
	private static readonly HashSet<Node> m_NodePauseRequests = new();
	
	public bool IsPaused => m_NodePauseRequests.Count > 0;

	public override void _EnterTree()
	{
		base._EnterTree();
		Instance = this;
	}

    public override void _ExitTree()
    {
        base._ExitTree();
		GetTree().Paused = false;
    }

    public void Pause(Node node)
	{
		m_NodePauseRequests.Add(node);
		HandlePause();
	}
	
	public void Unpause(Node node)
	{
		m_NodePauseRequests.Remove(node);
		HandlePause();
	}
	
	public void HandlePause()
	{
		GetTree().Paused = IsPaused;
	}
}
