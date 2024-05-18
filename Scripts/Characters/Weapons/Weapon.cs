using GenericLayeredCharacters;
using Godot;

public abstract partial class Weapon : Resource
{
	[Export] public int AttackDamage { get; private set; } = 1;
	[Export] public float AttackCooldown { get; private set; } = 2;
	[Export] public float AttackRange { get; private set; } = 0.25f;
	[Export] public ElementVariation Visuals { get; private set; }
}