using Godot;
using System;

namespace Game
{
	public partial class House : Node2D
	{
		TileMapLayer wall;
		TileMapLayer roof;
		[Export] CharacterBody2D character;

		const float fadeDuration = 0.4f;
		// 定义墙体物理层的索引 (对应 TileSet 中的 Element Index)
		private const int OuterWallPhysicsIndex = 0;
		private const int InnerWallPhysicsIndex = 1;

		// 定义碰撞层级 (BitMask)
		private const uint CollisionLayerOuterActive = 1u << 3;
		private const uint CollisionLayerInnerActive = 1u << 4;
		private const uint CollisionLayerInactive = 0u;

		public override void _Ready()
		{
			wall = GetNode<TileMapLayer>("Wall");
			roof = GetNode<TileMapLayer>("Roof");

			// wall.TileSet = wall.TileSet.Duplicate() as TileSet;
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}

		public void OnDoorExited(Node2D curNode, Node2D exitedNode, DirectionInfo directionInfo)
		{
			if (exitedNode != character)
			{
				GD.Print($"Exited node {exitedNode.Name} is not the character, ignoring.");
				return;
			}

			GD.Print($"Player exited door from {directionInfo.horizontalDirectionInEntered} {directionInfo.verticalDirectionInEntered} to {directionInfo.horizontalDirectionInExited} {directionInfo.verticalDirectionInExited}");
			if (directionInfo.verticalDirectionInEntered == "above" && directionInfo.verticalDirectionInExited == "below")
			{
				wall.TileSet.SetPhysicsLayerCollisionLayer(OuterWallPhysicsIndex, CollisionLayerOuterActive);
        wall.TileSet.SetPhysicsLayerCollisionLayer(InnerWallPhysicsIndex, CollisionLayerInactive);
				FadeIn(roof, fadeDuration);
			}
			else if (directionInfo.verticalDirectionInEntered == "below" && directionInfo.verticalDirectionInExited == "above")
			{
				GD.Print("Player exited upwards, reactivating roof and inner walls.");
				wall.TileSet.SetPhysicsLayerCollisionLayer(OuterWallPhysicsIndex, CollisionLayerInactive);
				wall.TileSet.SetPhysicsLayerCollisionLayer(InnerWallPhysicsIndex, CollisionLayerInnerActive);
				FadeOut(roof, fadeDuration);
			}
		}

		public Tween FadeIn(CanvasItem node, float duration = 1.0f)
		{
			// 创建一个补间动画器
			Tween tween = GetTree().CreateTween();

			// 在 1.0 秒内，将透明度从当前值变为 1
			// "modulate:a" 表示修改 modulate 属性的 alpha 通道
			tween.TweenProperty(node, "modulate:a", 1.0f, duration);

			// 动画结束后的回调（类似协程执行完）
			tween.Finished += () => GD.Print("淡入完成！");
			return tween;
		}

		public Tween FadeOut(CanvasItem node, float duration = 1.0f)
		{
			// 创建一个补间动画器
			Tween tween = GetTree().CreateTween();

			// 在 1.0 秒内，将透明度从当前值变为 0
			// "modulate:a" 表示修改 modulate 属性的 alpha 通道
			tween.TweenProperty(node, "modulate:a", 0.0f, duration);

			// 动画结束后的回调（类似协程执行完）
			tween.Finished += () => GD.Print("淡出完成！");
			return tween;
		}
	}
}
