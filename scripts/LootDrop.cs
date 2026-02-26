using Godot;
using System;

namespace Game
{
	public partial class LootDrop : Node
	{
		[Export] public float dropDelay = 0.5f;
		[Signal] public delegate void DropFinishedEventHandler();

		public override void _Ready()
		{
		}

		/// <summary>
		/// 执行掉落动画
		/// </summary>
		/// <param name="startPos">树干位置</param>
		/// <param name="targetPos">掉落终点位置</param>
		/// <param name="jumpHeight">抛物线最高点的高度 (像素)</param>
		/// <param name="duration">动画总时长 (秒)</param>
		public void StartDrop(Vector2 startPos, Vector2 targetPos, float jumpHeight, float duration)
		{
			Node2D parent = GetParent() as Node2D;
			if (parent == null)
			{
				GD.PrintErr("WoodDrop must be a child of a Loot!");
				return;
			}
			parent.GlobalPosition = startPos;

			// 1. 创建水平移动 Tween (根节点匀速向目标点移动)
			Tween moveTween = CreateTween();
			moveTween.TweenProperty(parent, "position:x", targetPos[0], duration);

			// 2. 创建垂直跳跃 Tween (Sprite2D 模拟高度变化)
			Tween jumpTween = CreateTween();

			// 前半程：起跳向上，速度越来越慢 (EaseOut)
			jumpTween.TweenProperty(parent, "position:y", targetPos[1] - jumpHeight, duration * 0.5f)
					 .SetTrans(Tween.TransitionType.Quad)
					 .SetEase(Tween.EaseType.Out);

			// 后半程：下落回地面，速度越来越快 (EaseIn)
			jumpTween.TweenProperty(parent, "position:y", targetPos[1], duration * 0.5f)
					 .SetTrans(Tween.TransitionType.Quad)
					 .SetEase(Tween.EaseType.In);

			jumpTween.Finished += async () =>
			{
				await ToSignal(GetTree().CreateTimer(dropDelay), "timeout");
				GD.Print("木头落地了！");
				EmitSignal(SignalName.DropFinished);
			};
		}
	}
}
