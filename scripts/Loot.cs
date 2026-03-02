using System.Threading.Tasks;
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
			if (inventory != null && item != null)
			{
				if (inventory.CanAddItem(item, out uint canAddCount))
				{
					inventory.AddItemRecursive(item);
				}
				else
				{
					// Todo: 重新开启是否可吸收判定
					// await ToSignal(GetTree().CreateTimer(2f), "timeout");
					// absorbable.StartCanAbsorbCheck(absorber);
				}
			}
			else
			{
				GD.PrintErr("Failed to get inventory or item reference when absorb finished!");
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