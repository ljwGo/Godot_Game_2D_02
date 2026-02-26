using Game;
using Godot;
using System;

namespace Game
{
	public partial class Tree : TileMapLayer
	{
		private LootDroppable _loot;

		public override void _Ready()
		{
			_loot = GetNode<LootDroppable>("LootDroppable");
		}

		public void OnTreeChopped(Cuttable cuttable)
		{
			// 这里可以添加一些树被砍倒后的逻辑，比如播放倒下的动画、掉落物品等
			GD.Print($"Tree {Name} has been chopped down!");
			_loot.DoDropLoot();
			// 销毁自身
			QueueFree();
		}
	}
}