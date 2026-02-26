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

		public void OnAbsorbFinished(Absorber absorber, Absorbable absorbable)
		{
			Inventory inventory = absorber.GetParent().GetNodeOrNull<Inventory>("Inventory");
			InventoryItem item = absorbable.GetParent().GetNodeOrNull<InventoryItem>("InventoryItem");
			if (inventory != null && item != null && inventory.CanAddItem(item, out uint canAddCount))
			{
				inventory.AddItemRecursive(item);
			}
		}

		public void OnMayStartAbsorb(Absorber absorber, Absorbable absorbable)
		{
			Inventory inventory = absorber.GetParent().GetNodeOrNull<Inventory>("Inventory");
			InventoryItem item = absorbable.GetParent().GetNodeOrNull<InventoryItem>("InventoryItem");
			if (inventory != null && item != null && inventory.CanAddItem(item, out uint canAddCount))
			{
				absorbable.StartAbsorb();
			}
		}
	}
}