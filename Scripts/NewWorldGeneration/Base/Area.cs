using Godot;

namespace WorldGeneration
{	
	public partial class Area : Resource
	{
		[Export] public Vector2 WidthRange { get; private set; }
		[Export] public Vector2 HeightRange { get; private set; }
		
		public Vector2 Center { get; set; }
		public Vector2 Size { get; private set; }
		
		public virtual Area GetInstance() 
		{
			var instance = new Area();
			instance.SetInstanceValues(this);
			return instance;
		}
		
		protected virtual void SetInstanceValues(Area reference)
		{
			Center = reference.Center;
			Size = new Vector2(reference.WidthRange.RandomInRange(), reference.HeightRange.RandomInRange());
		}
	}
}