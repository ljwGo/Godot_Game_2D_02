using Godot;
using System;
using System.Collections.Generic;

namespace Game
{
	public partial class Absorber : Area2D
	{
		[Export(PropertyHint.Range, "0,1024,1")] public float absorbRadius = 32.0f;

		readonly List<Absorbable> absorbablesInRange = [];
		Timer canAbsorbCheckTimer;

		[Signal] public delegate void MayStartAbsorbEventHandler(Absorber absorber, MayStartAbsorbEventArgs args);
		[Signal] public delegate void DoAbsorbEventHandler(Absorber absorber, Absorbable absorbable);

		public partial class MayStartAbsorbEventArgs : RefCounted
		{
			public List<Absorbable> AbsorbablesInRange { get; set; }
		}

		public override void _Ready()
		{
			// 绑定事件
			AreaEntered += OnAreaEntered;
			AreaExited += OnAreaExited;

			// 设置半径
			CollisionShape2D collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
			Shape2D shape2D = collisionShape.Shape;
			if (shape2D is CircleShape2D circleShape)
			{
				circleShape.Radius = absorbRadius;
			}
		}

		public void OnAreaEntered(Area2D area)
		{
			if (area is Absorbable absorbable)
			{
				if (!absorbablesInRange.Contains(absorbable))
				{
					absorbablesInRange.Add(absorbable);
					StartCanAbsorbCheck();
				}
			}
		}

		public void OnAreaExited(Area2D area)
		{
			if (area is Absorbable absorbable)
			{
				absorbablesInRange.Remove(absorbable);
				MayStopAbsorbCheck();
			}
		}

		public void Absorb(Absorbable absorbable)
		{
			EmitSignal(SignalName.DoAbsorb, this, absorbable);
		}

		public void StartCanAbsorbCheck()
		{
			// 单例
			if (canAbsorbCheckTimer != null && IsInstanceValid(canAbsorbCheckTimer))
				return;

			MayStartAbsorbEventArgs args = new()
			{
				AbsorbablesInRange = absorbablesInRange
			};

			canAbsorbCheckTimer = new Timer
			{
				WaitTime = 0.5f,
				OneShot = false
			};
			canAbsorbCheckTimer.Timeout += () =>
			{
				EmitSignal(SignalName.MayStartAbsorb, this, args);
				// 信号处理是同步的
				MayStopAbsorbCheck();
			};
			AddChild(canAbsorbCheckTimer);
			canAbsorbCheckTimer.Start();
		}

		public void MayStopAbsorbCheck()
		{
			if (absorbablesInRange.Count <= 0)
			{
				if (canAbsorbCheckTimer != null && IsInstanceValid(canAbsorbCheckTimer))
				{
					canAbsorbCheckTimer.Stop();
					canAbsorbCheckTimer.QueueFree();
					canAbsorbCheckTimer = null;
				}
			}
		}
	}
}
