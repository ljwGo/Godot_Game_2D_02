using Godot;
using System;
using System.Collections.Generic;
using Game.Utils;

namespace Game
{
	public partial class InteractEventHandlerParams : RefCounted
	{
		public HashSet<Interactive> interactivesInRange;
	}

	public partial class Interactor : Area2D
	{
		[Signal] public delegate void InteractEventHandler(InteractEventHandlerParams @params, Interactor interactor);
		// 存储所有在交互范围内的交互对象
		private readonly HashSet<Interactive> interactivesInRange = [];

		public override void _Ready()
		{
			AreaExited += OnAreaExited;
			AreaEntered += OnAreaEntered;
		}

		public override void _UnhandledInput(InputEvent @event)
		{
			// 只有当鼠标点击没被 UI 拦截时，这里才会运行
			if (@event.IsActionPressed("interact"))
			{
				EmitSignal(SignalName.Interact, new InteractEventHandlerParams { interactivesInRange = interactivesInRange }, this);
			}
		}

		private void OnAreaEntered(Area2D area)
		{
			Interactive interactive = area.GetNodeOrNull<Interactive>(".");
			if (interactive != null)
			{
				interactivesInRange.Add(interactive);
				GD.Print($"Entered interactive: {interactive.Name}");
			}
		}

		private void OnAreaExited(Area2D area)
		{
			Interactive interactive = area.GetNodeOrNull<Interactive>(".");
			if (interactive != null)
			{
				interactivesInRange.Remove(interactive);
				GD.Print($"Exited interactive: {interactive.Name}");
			}
		}
	}
}