using Godot;

public abstract partial class Weapon : Resource
{
	[Export] public Texture2D Sprite { get; set; }
}