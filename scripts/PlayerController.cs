using System;
using System.Collections.Generic;
using System.Linq;
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
		Area2D interactPoint;
		Inventory inventory;
		Picker picker;
		Interactor interactor;

		public override void _Ready()
		{
			_animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
			mainInventory = GetNodeOrNull<MainInventory>("%MainInventory");
			interactPoint = GetNode<Area2D>("InteractPoint");
			inventory = GetNode<Inventory>("Inventory");
			picker = GetNodeOrNull<Picker>("Picker");
			interactor = GetNodeOrNull<Interactor>("Interactor");

			Init();
		}

		public override void _PhysicsProcess(double delta)
		{
			// 将 delta 转换为 float 以匹配 Vector2 运算
			float fDelta = (float)delta;

			if (CanMove()) HandleMovement(fDelta);
			ApplyAnimation();
			MakeInteractPointDirection();

			// 执行物理移动
			MoveAndSlide();
		}

		public override void _Process(double delta)
		{
			// 这里可以处理一些非物理相关的逻辑，比如交互提示、状态更新等
		}

		public override void _UnhandledInput(InputEvent @event)
		{
			// 只有当鼠标点击没被 UI 拦截时，这里才会运行
			if (@event.IsActionPressed("interact"))
			{
				Interact();
			}
		}

		public void Init()
		{
			picker.CanPick = CanPick;
		}

		public void Interact()
		{
			List<Interactive> sortedResults = GetNearInteractives();

			if (MayDoPick(sortedResults, interactor)) return;
			else if (MayDoChop(sortedResults, interactor)) return;
			else if (MayDoOpen(sortedResults, interactor)) return;
			// else if (MayDoPlow(sortedResults, interactor)) return;
		}

		public List<Interactive> GetNearInteractives() {
			var shape = interactor.GetNode<CollisionShape2D>("InteractorScope").Shape as CircleShape2D;
			var circle = new CircleShape2D
			{
				Radius = shape.Radius,
			};

			var query = new PhysicsShapeQueryParameters2D
			{
				Shape = circle,
				Transform = Transform,
				CollideWithAreas = true,
				CollideWithBodies = false,
				CollisionMask = 4
			};

			var spaceState = GetWorld2D().DirectSpaceState;
			Vector2 centerPosition = interactor.Position;

			// 1. 获取无序的原始物理检测结果
			var rawResults = spaceState.IntersectShape(query, maxResults: 32);

			var selectInteractorsFn = (Godot.Collections.Dictionary result) =>
			{
				var obj = result["collider"].AsGodotObject();
				if (obj is Node2D node2D && node2D.GetParent() != null)
				{
					Node2D parentNode = node2D.GetParent() as Node2D;
					return new {
						Interactive = parentNode.GetNodeOrNull<Interactive>("Interactive"),
						DistanceSq = parentNode.GlobalPosition.DistanceSquaredTo(centerPosition)
					};
				}
				return null;
			};

			// 2. 使用 LINQ 按照与中心点的距离进行升序排序 (OrderBy)
			return rawResults
					.Select(selectInteractorsFn)
					.Where((result) => result != null)
					.OrderBy(item => item.DistanceSq) // 按距离平方升序排序（最近的在最前）
					.Select((result) => result.Interactive)
					.ToList();
		}

		public void OnInteractStart(InteractEventHandlerParams @params, Interactor interactor)
		{
		}

		// 是否可吸收
		public void OnMayStartAbsorb(Absorber absorber, Absorber.MayStartAbsorbEventArgs args)
		{
			List<Absorbable> absorbablesInRange = args.AbsorbablesInRange;
			for (int i = absorbablesInRange.Count - 1; i >= 0; i--)
			{
				Absorbable absorbable = absorbablesInRange[i];
				Inventory inventory = absorber.GetParent().GetNodeOrNull<Inventory>("Inventory");
				InventoryItem item = absorbable.GetParent().GetNodeOrNull<InventoryItem>("InventoryItem");
				if (inventory != null && item != null && inventory.CanAddItem(item, out uint canAddCount))
				{
					absorbable.SetTarget(absorber);
					absorbable.StartAbsorb();
					// 移除物品
					absorbablesInRange.RemoveAt(i);
				}
			}
		}

		// 决定吸收
		public void OnAbsorbFinished(Absorber absorber, Absorbable absorbable)
		{
			Inventory inventory = absorber.GetParent().GetNodeOrNull<Inventory>("Inventory");
			InventoryItem item = absorbable.GetParent().GetNodeOrNull<InventoryItem>("InventoryItem");
			if (inventory != null && item != null)
			{
				if (inventory.CanAddItem(item, out uint canAddCount))
				{
					inventory.AddItemRecursive(item);
				}
				else
				{
					// Todo: 重新开启是否可吸收判定
					// await ToSignal(GetTree().CreateTimer(2f), "timeout");
					// absorbable.StartCanAbsorbCheck(absorber);
				}
			}
			else
			{
				GD.PrintErr("Failed to get inventory or item reference when absorb finished!");
			}
		}

		private void MakeInteractPointDirection()
		{
			string currentAnimation = _animatedSprite.Animation;
			if (currentAnimation.EndsWith("Down"))
			{
				interactPoint.Rotation = 0;
			}
			else if (currentAnimation.EndsWith("Up"))
			{
				interactPoint.Rotation = Mathf.Pi;
			}
			else if (currentAnimation.EndsWith("Left"))
			{
				interactPoint.Rotation = Mathf.Pi / 2;
			}
			else if (currentAnimation.EndsWith("Right"))
			{
				interactPoint.Rotation = -Mathf.Pi / 2;
			}
		}

		private void HandleMovement(float delta)
		{
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

		private bool CanMove()
		{
			string currentAnimation = _animatedSprite.Animation;
			if (currentAnimation.StartsWith("chop") || currentAnimation.StartsWith("plow"))
			{
				Velocity = Vector2.Zero; // 砍伐动画期间禁止移动
				return false;
			}
			return true;
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

		public bool CanPick(Pickable pickable)
		{
			InventoryItem inventoryItem = pickable.GetParent().GetNode<InventoryItem>("InventoryItem");
			return inventory.CanAddItem(inventoryItem, out _);
		}

		private bool MayDoOpen(List<Interactive> interactives, Interactor interactor)
		{
			var interactorParent = interactor.GetParent();
			if (interactorParent == null) return false;

			foreach (Interactive interactive in interactives)
			{
				var interactiveParent = interactive.GetParent();
				if (interactiveParent == null) continue;

				Openable openable = interactiveParent.GetNodeOrNull<Openable>("Openable");
				Opener opener = interactorParent.GetNodeOrNull<Opener>("Opener");

				if (openable != null && opener != null)
				{
					return opener.MayDoOpen(openable);
				}
			}
			return false;
		}

		private bool MayDoPick(List<Interactive> interactives, Interactor interactor)
		{
			var interactorParent = interactor.GetParent();
			if (interactorParent == null) return false;

			foreach (Interactive interactive in interactives)
			{
				var interactiveParent = interactive.GetParent();
				if (interactiveParent == null) continue;

				Picker picker = interactorParent.GetNodeOrNull<Picker>("Picker");
				Pickable pickable = interactiveParent.GetNodeOrNull<Pickable>("Pickable");

				if (picker != null && pickable != null)
				{
					return picker.MayDoPick(pickable);
				}
			}
			return false;
		}

		private bool MayDoChop(List<Interactive> interactives, Interactor interactor)
		{
			var parent = interactor.GetParent();
			if (parent == null) return false;

			Faller faller = parent.GetNodeOrNull<Faller>("Faller");
			if (faller == null) return false;

			foreach (var interactive in interactives)
			{
				var interactiveParent = interactive.GetParent();
				if (interactiveParent == null) continue;

				Cuttable cuttable = interactiveParent.GetNodeOrNull<Cuttable>("Cuttable");
				if (cuttable != null)
				{
					return faller.MayPlayChopAnimation(cuttable);
				}
			}

			return false;
		}

		private bool MayDoPlow(List<Interactive> interactives, Interactor interactor)
		{
			var parent = interactor.GetParent();
			if (parent == null) return false;

			Farmer farmer = parent.GetNodeOrNull<Farmer>("Farmer");
			if (farmer == null) return false;

			farmer.PlayPlowAnimation();

			return true;
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

		public void OnPickUp(Picker picker, Pickable pickable)
		{
			InventoryItem inventoryItem = pickable.GetParent().GetNode<InventoryItem>("InventoryItem");
			inventory.AddItemRecursive(inventoryItem);
		}
	}
}