using Godot;
using System;

namespace Game
{
	public partial class Stackable : Node
	{
		[Export] public uint maxStackSize = 40;

		private uint _currentStackSize = 1;
		[Export]
		public uint currentStackSize
		{
			get => _currentStackSize;
			set
			{
				// 防止溢出和非法赋值
				_currentStackSize = Math.Clamp(value, 0, maxStackSize);
				EmitSignal(SignalName.StackSizeChanged, _currentStackSize);
			}
		}

		// 数量改变的信号
		[Signal] public delegate void StackSizeChangedEventHandler(uint newSize);

		public InventoryItem InventoryItem { get; private set; }

		public override void _Ready()
		{
			InventoryItem = GetParent().GetNodeOrNull<InventoryItem>("InventoryItem");
			Check();
		}

		public void SetStackSize(uint newSize)
		{
			currentStackSize = newSize;
		}

		public bool IsFull()
		{
			return currentStackSize >= maxStackSize;
		}

		public uint GetRemainderSpace()
		{
			return maxStackSize - currentStackSize;
		}

		public bool CanStackWith(Stackable other)
		{
			if (other == null || this == other) return false;
			if (InventoryItem == null || other.InventoryItem == null) return false;

			// 不是同一个物品
			if (other.InventoryItem.itemName != InventoryItem.itemName) return false;

			// 没有空余位置
			if (IsFull()) return false;

			return true;
		}

		public uint StackWith(Stackable other)
		{
			if (!CanStackWith(other)) return other.currentStackSize;

			uint spaceLeft = GetRemainderSpace();
			uint amountToStack = Math.Min(spaceLeft, other.currentStackSize);

			this.currentStackSize += amountToStack;
			other.currentStackSize -= amountToStack;

			return other.currentStackSize; // 对方剩余的数量
		}

		/// <summary>
		/// 尝试进行堆叠。
		/// </summary>
		/// <param name="autoFreeIfEmpty">如果物品被吸干，是否自动销毁节点实体</param>
		/// <returns>未被堆叠进去的剩余数量</returns>
		public uint MayStackWith(Stackable other, bool autoFreeIfEmpty = true)
		{
			if (!CanStackWith(other)) return other.currentStackSize;

			uint remainder = StackWith(other);

			if (remainder == 0 && autoFreeIfEmpty)
			{
				Node parentEntity = other.GetParent();
				if (IsInstanceValid(parentEntity) && !parentEntity.IsQueuedForDeletion())
				{
					parentEntity.QueueFree();
				}
			}

			return remainder;
		}

		public Stackable Split(uint splitAmount)
		{
			if (splitAmount < 1 || splitAmount >= currentStackSize)
			{
				GD.Print("[Stackable] 无法拆分：数量必须在 1 和当前堆叠数量之间");
			}
			uint actualSplit = Math.Clamp(splitAmount, 1, currentStackSize - 1);
			Node clone = GetParent().Duplicate();
			Stackable cloneStackable = clone.GetNode<Stackable>("Stackable");
			cloneStackable.currentStackSize = actualSplit;
			this.currentStackSize -= actualSplit;
			return cloneStackable;
		}

		private bool Check()
		{
			if (InventoryItem == null)
			{
				GD.PrintErr($"[Stackable] {Name} 找不到同级的 InventoryItem！它和 InventoryItem 必须同属一个父节点。");
				return false;
			}
			if (currentStackSize > maxStackSize || currentStackSize == 0)
			{
				GD.PrintErr($"[Stackable] {Name} 的初始数量异常: {currentStackSize}/{maxStackSize}");
			}
			return true;
		}
	}
}