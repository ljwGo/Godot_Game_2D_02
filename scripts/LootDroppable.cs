using Godot;

namespace Game
{
	public partial class LootDroppable : Node
	{
		[Export] public LootItemParam[] lootItems;
		[Export] public float minDropRadius = 10.0f;
		[Export] public float maxDropRadius = 20.0f;
		[Export] public float jumpHeight = 10.0f;
		[Export] public float duration = 3f;

		// 统一存放掉落物的节点
		private Node2D lootContainer;

		public override void _Ready()
		{
			lootContainer = GetNode<Node2D>("/root/Root/Game/LootsContainer");
		}

		public void DoDropLoot()
		{
			if (lootItems == null || lootItems.Length == 0)
			{
				GD.PrintErr("Loot scene is not assigned!");
				return;
			}

			Node2D parent = GetParent() as Node2D;

			foreach (var item in lootItems)
			{
				if (item.lootScene == null)
				{
					GD.PrintErr("LootItemParam has no lootScene assigned!");
					continue;
				}

				for (int i = 0; i < item.quantity; i++)
				{
					if (GD.Randf() <= item.dropChance)
					{
						Vector2 dropPosition = GetRandomDropPosition(parent.GlobalPosition, minDropRadius, maxDropRadius);
						Loot lootInstance = item.lootScene.Instantiate<Loot>();
						LootDrop lootDrop = lootInstance.GetNode<LootDrop>("LootDrop");
						Stackable stackable = lootInstance.GetNodeOrNull<Stackable>("Stackable");
						if (stackable != null)
						{
							stackable.SetStackSize(item.stackSize);
						}
						lootContainer.AddChild(lootInstance);
						lootDrop.StartDrop(parent.GlobalPosition, dropPosition, jumpHeight, duration);
					}
				}
			}
		}

		public Vector2 GetRandomDropPosition(Vector2 origin, float minRadius, float maxRadius)
		{
			float randomAngle = GD.Randf() * Mathf.Pi * 2;
			float randomRadius = minRadius + GD.Randf() * (maxRadius - minRadius);
			return origin + new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * randomRadius;
		}
	}
}
