using Godot;
using System;
using System.Collections.Generic;

namespace Game
{
	public partial class Inventory : Node
	{
		[Export] public uint capacity = 3;

		[Signal] public delegate void ItemChangeEventHandler(int index, InventoryItem item);
		[Signal] public delegate void InventoryFullEventHandler(InventoryItem item);

		private InventoryItem[] items = [];

		public override void _Ready()
		{
			InitItems();
		}

		public void InitItems()
		{
			items = new InventoryItem[capacity];
		}

		#region 查询逻辑 (Query)

		public bool HasItem(InventoryItem item)
		{
			if (item == null) return false;
			return Array.IndexOf(items, item) >= 0;
		}

		public bool HasItem(string itemName)
		{
			if (string.IsNullOrEmpty(itemName)) return false;

			for (int i = 0; i < capacity; i++)
			{
				if (items[i] != null && items[i].itemName == itemName)
					return true;
			}
			return false;
		}

		public bool CanAddItem(InventoryItem item, out uint addQuantity)
		{
			addQuantity = 0;
			if (item == null) return false;

			Stackable stackable = GetStackableComponent(item);

			// 如果不可堆叠，只要有空位就能放1个
			if (stackable == null)
			{
				if (FindEmptyIx() != -1)
				{
					addQuantity = 1;
					return true;
				}
				return false;
			}

			// 如果可堆叠，计算当前背包中同类物品的剩余空间 + 空格数 * 最大堆叠量
			for (int i = 0; i < capacity; i++)
			{
				if (items[i] == null)
				{
					addQuantity = stackable.currentStackSize;
				}
				else if (items[i].itemName == item.itemName)
				{
					Stackable otherStack = GetStackableComponent(items[i]);
					if (otherStack != null && stackable.CanStackWith(otherStack))
					{
						addQuantity = Math.Min(otherStack.GetRemainderSpace(), stackable.currentStackSize);
					}
				}
			}

			return addQuantity > 0;
		}

		#endregion

		#region 删除逻辑 (Remove)

		public void RemoveItem(InventoryItem item)
		{
			if (item == null) return;
			int ix = Array.IndexOf(items, item);
			if (ix >= 0)
			{
				RemoveItemAt(ix);
			}
		}

		public void RemoveFirstItem(string itemName)
		{
			for (int i = 0; i < capacity; i++)
			{
				if (items[i] != null && items[i].itemName == itemName)
				{
					RemoveItemAt(i);
					return; // 只删第一个
				}
			}
		}

		public void RemoveAllItems(string itemName)
		{
			for (int i = 0; i < capacity; i++)
			{
				if (items[i] != null && items[i].itemName == itemName)
				{
					RemoveItemAt(i);
				}
			}
		}

		public void RemoveItemAt(int ix)
		{
			// 越界保护
			if (ix < 0 || ix >= capacity) return;

			if (items[ix] != null)
			{
				InventoryItem removedItem = items[ix];
				items[ix] = null;
				EmitSignal(SignalName.ItemChange, ix, removedItem);
			}
		}

		#endregion

		#region 添加逻辑 (Add)

		// 如果 ix == -1 则自动寻找位置，否则尝试放入指定位置
		public void AddItemRecursive(InventoryItem item, int ix = -1)
		{
			Stackable stackable = GetStackableComponent(item);
			// 不可堆叠
			if (stackable == null)
			{
				if (ix == -1)
				{
					int targetIx = FindEmptyIx();
					if (targetIx != -1)
					{
						PlaceItemAt(item, targetIx);
						return;
					}
					else
					{
						// 背包满了，无法添加
						EmitSignal(SignalName.InventoryFull, item);
						return;
					}
				}
				else if (items[ix] == null)
				{
					PlaceItemAt(item, ix);
					return;
				}
				else
				{
					// 指定位置上有物品
					GD.PrintErr($"Cannot place item at index {ix} because it's already occupied.");
					return;
				}
			}
			// 可堆叠
			else
			{
				if (ix == -1)
				{
					int targetIx = FindCanAddIx(item);
					if (targetIx != -1)
					{
						bool hasAdded = TryStackItemAt(item, targetIx, out uint remainder);
						if (remainder > 0 && hasAdded)
						{
							AddItemRecursive(item, -1); // 递归尝试添加剩余的物品
						}
					}
					else
					{
						// 背包满了，无法添加
						EmitSignal(SignalName.InventoryFull, item);
						return;
					}
				}
			}
		}

		#endregion

		#region 内部辅助方法 (Helpers)

		// 将物品直接放置在对应格子，并触发信号
		private void PlaceItemAt(InventoryItem item, int ix)
		{
			items[ix] = item;
			Node parent = item.GetParent();
			Node grandParent = parent.GetParent();
			if (IsInstanceValid(grandParent) && !grandParent.IsQueuedForDeletion())
			{
				grandParent.RemoveChild(parent);
			}
			EmitSignal(SignalName.ItemChange, ix, item);
		}

		// 尝试将物品堆叠到指定格子。返回是否堆叠成功，并通过 out 输出剩余数量
		private bool TryStackItemAt(InventoryItem item, int ix, out uint remainder)
		{
			remainder = 0;
			Stackable incomingStack = GetStackableComponent(item);
			Stackable existingStack = items[ix] != null ? GetStackableComponent(items[ix]) : null;

			if (incomingStack != null && existingStack != null && incomingStack.CanStackWith(existingStack))
			{
				// 假设 MayStackWith 方法会改变双方的内部数量，并返回未能堆叠进去的剩余量
				remainder = existingStack.MayStackWith(incomingStack);
				EmitSignal(SignalName.ItemChange, ix, items[ix]); // 堆叠数量变化，通知 UI 刷新
				return true;
			}
			else if (items[ix] == null && incomingStack != null)
			{
				PlaceItemAt(item, ix);
				return true;
			}
			return false;
		}

		// 仅寻找符合条件的位置索引，不产生修改数据的副作用
		private int FindCanAddIx(InventoryItem item)
		{
			Stackable incomingStack = GetStackableComponent(item);

			// 1. 如果可堆叠，优先找包里同类型且未满的格子
			if (incomingStack != null)
			{
				for (int i = 0; i < capacity; i++)
				{
					if (items[i] != null && items[i].itemName == item.itemName)
					{
						Stackable existingStack = GetStackableComponent(items[i]);
						if (existingStack != null && existingStack.CanStackWith(incomingStack))
						{
							return i; // 找到可堆叠的目标
						}
					}
				}
			}

			// 2. 如果不可堆叠，或者同类物品格子都满了，找一个空位
			return FindEmptyIx();
		}

		private int FindEmptyIx()
		{
			for (int i = 0; i < capacity; i++)
			{
				if (items[i] == null) return i;
			}
			return -1;
		}

		// 安全获取 Stackable 组件的辅助封装
		private Stackable GetStackableComponent(InventoryItem item)
		{
			if (item == null || item.GetParent() == null) return null;
			return item.GetParent().GetNodeOrNull<Stackable>("Stackable");
		}

		#endregion
	}
}