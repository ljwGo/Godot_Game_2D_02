using Godot;
using System;
using UI;

namespace Game
{
	public partial class MainInventory : PanelContainer
	{
		[Export] public PackedScene slotScene;
		[Export] public uint size;
		HBoxContainer slotsContainer;
		PlayerController playerController;

		public override void _Ready()
		{
			slotsContainer = GetNode<HBoxContainer>("Slots");
			playerController = GetNode<PlayerController>("%Player");
			GenerateSlots();
		}

		public override void _Process(double delta)
		{
		}

		public void UpdateSlot(int index, InventoryItem item)
		{
			TextureRect slot = slotsContainer.GetChild<TextureRect>(index);
			Tile tile = slot.GetNode<Tile>("Tile");
			TextureRect sprite = tile.GetNode<TextureRect>("%Sprite");
			Sprite2D sprite2D = item.GetParent().GetNode<Sprite2D>("Sprite2D");
			Stackable stackable = item.GetParent().GetNodeOrNull<Stackable>("Stackable");

			sprite.Texture = sprite2D.Texture;
			tile.SetQuantity(stackable != null ? stackable.currentStackSize : 1);
		}

		void GenerateSlots()
		{
			for (int i = 0; i < size; i++)
			{
				var slot = slotScene.Instantiate();
				Slot slotScript = slot as Slot;
				slotScript.ix = i;
				slotScript.SlotClicked += HandleSlotClicked;
				slotsContainer.AddChild(slot);
			}
		}

		void HandleSlotClicked(int index)
		{
			GD.Print($"Slot {index} clicked in MainInventory");
			Inventory inventory = playerController.GetNode<Inventory>("Inventory");
			inventory.ActiveItemIx = index;

			var children = slotsContainer.GetChildren();
			for (int i = 0; i < children.Count; i++)
			{
				if (children[i] is Slot slot)
				{
					slot.SetBeSelected(i == index);
				}
			}
		}
	}
}
