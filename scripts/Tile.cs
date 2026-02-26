using Godot;
using System;

namespace Game
{
	public partial class Tile : PanelContainer
	{
		public Label quantityLabel;

		public override void _Ready()
		{
			quantityLabel = GetNode<Label>("QuantityLabel");
		}

		public override void _Process(double delta)
		{
		}

		public void SetQuantity(uint quantity)
		{
			if (quantity == 0)
			{
				quantityLabel.Text = "";
				quantityLabel.Visible = false;
			}
			else
			{
				quantityLabel.Text = quantity.ToString();
				quantityLabel.Visible = true;
			}
		}
	}
}
