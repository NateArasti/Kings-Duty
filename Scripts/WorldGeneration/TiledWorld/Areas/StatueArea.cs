using Godot;

namespace WorldGeneration.Tiled
{
	public partial class StatueArea : Area
	{
		public override Area GetInstance()
		{
			var instance = new StatueArea();
			instance.SetInstanceValues(this);
			return instance;
		}
	}
}