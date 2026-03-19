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
				Node2D parent = GetParent<Node2D>();
				itemName = parent.Name; // 默认使用父节点的名称作为物品名称
			}
		}
	}
}
