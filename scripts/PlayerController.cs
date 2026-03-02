using System;
using Godot;

namespace Game
{
	public partial class PlayerController : CharacterBody2D
	{
		// 增加配置参数，方便在编辑器中微调手感
		[Export] public float MaxSpeed = 120.0f;     // 最大速度
		[Export] public float Acceleration = 800.0f; // 加速度 (数值越大起步越快)
		[Export] public float Friction = 1000.0f;    // 摩擦力 (数值越大停止越快)

		AnimatedSprite2D _animatedSprite;
		MainInventory mainInventory;

		public override void _Ready()
		{
			_animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
			mainInventory = GetNodeOrNull<MainInventory>("%MainInventory");
		}

		public override void _PhysicsProcess(double delta)
		{
			// 将 delta 转换为 float 以匹配 Vector2 运算
			float fDelta = (float)delta;

			HandleMovement(fDelta);
			ApplyAnimation();

			// 执行物理移动
			MoveAndSlide();
		}

		public override void _Process(double delta)
		{
			// 这里可以处理一些非物理相关的逻辑，比如交互提示、状态更新等
		}

		public void OnInteractStart(InteractEventHandlerParams @params, Interactor interactor)
		{
			foreach (var interactive in @params.interactivesInRange)
			{
				var interactiveParent = interactive.GetParent();
				var interactorParent = interactor.GetParent();
				if (interactiveParent != null && interactorParent != null)
				{
					MayDoChop(@params, interactor);
				}
			}
		}

		private void HandleMovement(float delta)
		{
			string currentAnimation = _animatedSprite.Animation;
			if (currentAnimation.StartsWith("chop"))
			{
				Velocity = Vector2.Zero; // 砍伐动画期间禁止移动
				return;
			}

			// 1. 获取输入向量 (左, 右, 上, 下)
			// GetVector 会自动归一化，且支持手柄压感
			Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

			if (inputDir != Vector2.Zero)
			{
				// 2. 有输入：朝目标速度加速
				Vector2 targetVelocity = inputDir * MaxSpeed;
				Velocity = Velocity.MoveToward(targetVelocity, Acceleration * delta);
			}
			else
			{
				// 3. 无输入：朝零速度减速（模拟摩擦力）
				Velocity = Velocity.MoveToward(Vector2.Zero, Friction * delta);
			}
		}

		private void ApplyAnimation()
		{
			// 动画逻辑基于当前实际 Velocity
			// 增加一个小阈值判断，防止物理微调导致的动画抖动
			string currentAnimation = _animatedSprite.Animation;
			if (Velocity.Length() < 1.0f)
			{
				if (currentAnimation.StartsWith("walk"))
				{
					_animatedSprite.Stop();
				}
				return;
			}

			// 优先判断水平还是垂直，或者根据你的美术资源决定优先级
			if (Mathf.Abs(Velocity.X) > Mathf.Abs(Velocity.Y))
			{
				_animatedSprite.Play(Velocity.X > 0 ? "walkRight" : "walkLeft");
			}
			else
			{
				_animatedSprite.Play(Velocity.Y > 0 ? "walkDown" : "walkUp");
			}
		}

		private void MayDoChop(InteractEventHandlerParams @params, Interactor interactor)
		{
			var parent = interactor.GetParent();
			if (parent == null) return;

			Faller faller = parent.GetNodeOrNull<Faller>("Faller");
			if (faller == null) return;

			foreach (var interactive in @params.interactivesInRange)
			{
				var interactiveParent = interactive.GetParent();
				if (interactiveParent == null) continue;
				Cuttable cuttable = interactiveParent.GetNodeOrNull<Cuttable>("Cuttable");
				if (cuttable != null)
				{
					faller.MayPlayChopAnimation(cuttable);
				}
			}
		}

		// UI更新部分
		public void OnInventoryItemChange(int index, InventoryItem item)
		{
			if (mainInventory == null) return;

			mainInventory.UpdateSlot(index, item);
		}

		public void OnInventoryFull(InventoryItem item)
		{
			// 这里可以显示一个提示，告诉玩家背包满了
			GD.Print("Cannot add " + item.itemName + " to inventory: Inventory is full!");
		}
	}
}