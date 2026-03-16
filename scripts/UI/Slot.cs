using Godot;
using System;

namespace UI
{
	public partial class Slot : TextureRect
	{
		[Signal] public delegate void SlotClickedEventHandler(int ix);

		public int ix;

		public override void _GuiInput(InputEvent @event)
		{
			if (@event is InputEventMouseButton mouseEvent)
			{
				if (mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
				{
					EmitSignal(SignalName.SlotClicked, ix);
				}
			}
		}

		public void SetBeSelected(bool selected)
		{
			// 这里可以添加一些视觉效果来表示选中状态，例如改变背景颜色或添加边框
			if (selected)
			{
				SelfModulate = new Color(1, 1, 0.3f); // 选中时变亮
			}
			else
			{
				SelfModulate = new Color(1, 1, 1); // 非选中时恢复正常
			}
		}
	}
}