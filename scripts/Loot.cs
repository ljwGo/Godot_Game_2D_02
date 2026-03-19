using Godot;

namespace Game
{
	public partial class Loot : Node2D
	{
		Absorbable absorbable;

    public override void _Ready()
		{
			absorbable = GetNode<Absorbable>("Absorbable");
		}

		public void OnLootDropFinished()
		{
			GD.Print($"{Name} received drop finished signal, starting absorb!");
			absorbable.ProcessMode = ProcessModeEnum.Inherit;
		}

		public void OnLootAbsorbCloseToTarget(Absorber absorber, Absorbable absorbable)
		{
			absorbable.StopAbsorb();
			absorber.Absorb(absorbable);
		}
	}
}