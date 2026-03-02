using Godot;
using System;

namespace Game
{
	public partial class MainInventory : PanelContainer
	{
		[Export] public PackedScene slotScene;
		[Export] public uint size;
		HBoxContainer slotsContainer;

		public override void _Ready()
		{
			slotsContainer = GetNode<HBoxContainer>("Slots");
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
			tile.SetQuantity(stackable != null ? stackable.currentStackSize : 2);
		}

		void GenerateSlots()
		{
			for (uint i = 0; i < size; i++)
			{
				var slot = slotScene.Instantiate();
				slotsContainer.AddChild(slot);
			}
		}
	}
}
