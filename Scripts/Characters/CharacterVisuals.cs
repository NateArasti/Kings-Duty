using Godot;

public partial class CharacterVisuals : Node3D
{
	[Export] private Sprite3D m_BodySprite;
	[Export] private Sprite3D m_MainHandObjectSprite;
	
	public void SetVisuals(Texture2D bodyTexture, Texture2D mainHandObjectTexture = null)
	{
		m_BodySprite.Texture = bodyTexture;
		m_MainHandObjectSprite.Texture = mainHandObjectTexture;
	}
}