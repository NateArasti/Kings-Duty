using Godot;

namespace GenericLayeredCharacters
{
	[Tool]
	public partial class CharacterElement : Resource
	{
		[Export] public Layer Layer { get; private set; }
		[Export] public ElementVariation[] Variations { get; private set; }
	}
}
