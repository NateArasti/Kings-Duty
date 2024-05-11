using Godot;

namespace GenericLayeredCharacters.Demo
{
	public partial class Demo : Node
	{
		[Export] private MeshInstance3D m_Quad;
		[Export] private Button m_GenerateRandom;

		public override void _Ready()
		{
			base._Ready();
			m_GenerateRandom.Pressed += GenerateRandom;
			GenerateRandom();
		}

		private void GenerateRandom()
		{
			Utility.FillRandomLayers(m_Quad.MaterialOverride as ShaderMaterial);
		}
	}
}
