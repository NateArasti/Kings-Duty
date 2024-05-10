using Godot;
using Godot.Collections;

namespace GenericLayeredCharacters
{
	[Tool]
	public partial class ElementVariation : Resource
	{
		[Export] public Layer Layer { get; private set; }
		[Export] public string Label { get; private set; }
		[Export] public Texture2D Main { get; private set; }
		[Export] public Array<ElementVariation> AvailableDependents { get; private set; }
	}
}