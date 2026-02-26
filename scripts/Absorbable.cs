using Godot;
using System;

namespace Game
{
	public partial class Absorbable : Area2D
	{
		[Export] public float absorbSpeed = 40.0f;
		[Signal] public delegate void AbsorbFinishedEventHandler(Absorber absorber, Absorbable absorbable);
		[Signal] public delegate void MayStartAbsorbEventHandler(Absorber absorber, Absorbable absorbable);

		public bool isAbsorbing = false;

		Timer canAbsorbCheckTimer;
		Node2D parentNode;
		Absorber target;
		bool canAbsorb = false;

		public override void _Ready()
		{
			AreaEntered += OnAreaEntered;
			AreaExited += OnAreaExited;
		}

		public override void _Process(double delta)
		{
			if (canAbsorb && target != null)
			{
				Absorb2Target(target);
			}
		}

		public void SetTarget(Absorber absorber)
		{
			target = absorber;
			parentNode = GetParent<Node2D>();
		}

		public void StartAbsorb()
		{
			canAbsorb = true;
			isAbsorbing = true;
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
				isAbsorbing = false;
				canAbsorb = false;
				EmitSignal(SignalName.AbsorbFinished, target, this);
			}
		}

		public void OnAreaEntered(Area2D area)
		{
			if (area is Absorber absorber)
			{
				SetTarget(absorber);
				canAbsorbCheckTimer = new Timer
				{
					WaitTime = 0.1f,
					OneShot = true
				};
				canAbsorbCheckTimer.Timeout += () =>
				{
					if (IsInstanceValid(absorber) && IsInstanceValid(this))
					{
						EmitSignal(SignalName.MayStartAbsorb, absorber, this);
						// 通过isAbsorbing来回传是否已经开始吸收
						if (isAbsorbing)
						{
							if (IsInstanceValid(canAbsorbCheckTimer))
							{
								canAbsorbCheckTimer.Stop();
								canAbsorbCheckTimer.QueueFree();
							}
						}
					}
				};
				AddChild(canAbsorbCheckTimer);
				canAbsorbCheckTimer.Start();
			}
		}

		public void OnAreaExited(Area2D area)
		{
			if (area is Absorber)
			{
				SetTarget(null);
				if (IsInstanceValid(canAbsorbCheckTimer))
				{
					canAbsorbCheckTimer.Stop();
					canAbsorbCheckTimer.QueueFree();
				}
			}
		}
	}
}