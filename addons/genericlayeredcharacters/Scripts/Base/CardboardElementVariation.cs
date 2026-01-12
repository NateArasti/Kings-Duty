using Godot;

namespace GenericLayeredCharacters
{
	[Tool]
	public partial class CardboardElementVariation : ElementVariation
	{
		[Export] public Texture2D Cardboard { get; private set; }
		[Export] public Texture2D Outline { get; private set; }
	}
}