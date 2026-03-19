using Godot;
using System;

namespace Game
{
	public partial class Absorbable : Area2D
	{
		[Export] public float absorbSpeed = 40.0f;

		Node2D parentNode;
		Absorber target;
		bool canAbsorb = false;

		[Signal] public delegate void AbsorbFinishedEventHandler(Absorber absorber, Absorbable absorbable);
		[Signal] public delegate void CloseToTargetEventHandler(Absorber absorber, Absorbable absorbable);

		public override void _Ready()
		{
			parentNode = GetParent<Node2D>();
		}

		public override void _PhysicsProcess(double delta)
		{
			if (canAbsorb && target != null)
			{
				Absorb2Target(target);
			}
		}

		public void SetTarget(Absorber absorber)
		{
			target = absorber;
		}

		public void StartAbsorb()
		{
			canAbsorb = true;
		}

		public void StopAbsorb()
		{
			canAbsorb = false;
			EmitSignal(SignalName.AbsorbFinished, target, this);
		}

		public void Absorb2Target(Absorber absorber)
		{
			Node2D targetParent = absorber.GetParent<Node2D>();
			Vector2 targetPos = targetParent.GlobalPosition;
			Vector2 currentPos = parentNode.GlobalPosition;
			Vector2 offset = targetPos - currentPos;

			if (offset.Length() > absorbSpeed)
			{
				parentNode.GlobalPosition += offset.Normalized() * absorbSpeed;
			}
			else
			{
				parentNode.GlobalPosition = targetPos;
				EmitSignal(SignalName.CloseToTarget, target, this);
			}
		}
	}
}