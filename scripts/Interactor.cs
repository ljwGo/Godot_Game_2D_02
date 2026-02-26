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

		public override void _Process(double delta)
		{
		}

		public override void _PhysicsProcess(double delta)
		{
			// 这里可以添加一些逻辑，比如优先交互最近的对象，或者根据玩家朝向选择交互对象
			// 例如：如果按下交互键，触发最近的交互对象的交互事件
			if (Input.IsActionJustPressed("interact"))
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