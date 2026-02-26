using Godot;
using System;

namespace Game
{
	public partial class Faller : Node
	{
		// 砍伐力
		[Export] public float ChopPerPower = 1.0f;

		// 砍伐范围
		[Export] public float ChopRange = 200.0f;

		AnimatedSprite2D animatedSprite2D;
		Cuttable currentTarget;

		public override void _Ready()
		{
			animatedSprite2D = GetNode<AnimatedSprite2D>("../AnimatedSprite2D");
		}

		public override void _Process(double delta)
		{
		}

		public bool CanChop(Cuttable target)
		{
			string currentAnimation = animatedSprite2D.Animation;

			if (currentAnimation.StartsWith("chop"))
			{
				return false;
			}

			Node2D cuttableParent = target.GetParent() as Node2D;
			Node2D fallerParent = GetParent() as Node2D;
			if (cuttableParent == null || fallerParent == null)
				return false;

			// 计算两者之间的距离
			float distance = cuttableParent.GlobalPosition.DistanceTo(fallerParent.GlobalPosition);
			return distance <= ChopRange;
		}

		public void MayPlayChopAnimation(Cuttable cuttable)
		{
			if (CanChop(cuttable))
			{
				currentTarget = cuttable;
				PlayChopAnimation(cuttable);
			}
		}

		public void MayDoChop(Cuttable cuttable)
		{
			if (CanChop(cuttable))
			{
				currentTarget = cuttable;
				DoChop(cuttable);
			}
		}

		public void DoChop(Cuttable target)
		{
			target.DoChop(ChopPerPower);
			GD.Print($"Chopping target: {target.Name}, Current Chop Power: {target.CurChopPower}/{target.TotalChopPower}");
		}

		private void PlayChopAnimation(Cuttable cuttable)
		{
			// 根据目标位置和当前动画状态选择合适的砍伐动画
			Node2D cuttableParent = cuttable.GetParent() as Node2D;
			Node2D fallerParent = GetParent() as Node2D;
			if (cuttableParent == null || fallerParent == null)
				return;

			Vector2 direction = cuttableParent.GlobalPosition - fallerParent.GlobalPosition;
			string animationName = "chopDown"; // 默认向下砍

			if (Math.Abs(direction.X) > Math.Abs(direction.Y))
			{
				animationName = direction.X > 0 ? "chopRight" : "chopLeft";
			}
			else
			{
				animationName = direction.Y > 0 ? "chopDown" : "chopUp";
			}

			animatedSprite2D.Play(animationName);
			GD.Print($"Playing chop animation: {animationName}");
		}

		// 无法连接异步的事件处理者
		public void OnAnimationFinished()
		{
			Change2IdleAnim();
			MayDoChop(currentTarget);
		}

		private void Change2IdleAnim()
		{
			if (animatedSprite2D.Animation == "chopLeft")
			{
				animatedSprite2D.Animation = "idleLeft";
			}
			else if (animatedSprite2D.Animation == "chopRight")
			{
				animatedSprite2D.Animation = "idleRight";
			}
			else if (animatedSprite2D.Animation == "chopUp")
			{
				animatedSprite2D.Animation = "idleUp";
			}
			else if (animatedSprite2D.Animation == "chopDown")
			{
				animatedSprite2D.Animation = "idleDown";
			}
		}
	}
}
