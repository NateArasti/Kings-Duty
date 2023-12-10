using Godot;

public partial class Tree : Node3D
{
	[Export] private Sprite3D m_MainTree;
	[Export] private Sprite3D m_SubTree;
	[Export] private Texture2D[] m_MainTreeSprites;
	[Export] private Texture2D[] m_SubTreeSprites;
	
	public void SetRandom()
	{
		var index = GD.RandRange(0, m_MainTreeSprites.Length - 1);
		m_MainTree.Texture = m_MainTreeSprites[index];
		m_SubTree.Texture = m_SubTreeSprites[index];
	}
}