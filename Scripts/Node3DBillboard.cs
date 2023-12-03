using Godot;

public partial class Node3DBillboard : Node3D
{
	public override void _Ready()
	{
		Rotation = GetViewport().GetCamera3D().Rotation;
	}
}
