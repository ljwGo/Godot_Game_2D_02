using Godot;
using System;

namespace Game
{
	public partial class InventoryItem : Node
	{
		[Export] public string itemName;

		public override void _Ready()
		{
			if (string.IsNullOrEmpty(itemName))
			{
				GD.PrintErr($"InventoryItem {Name} has an empty itemName!");
			}
		}
	}
}
