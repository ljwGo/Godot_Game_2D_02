using Godot;
using System;
using System.Collections.Generic;

namespace Game
{
	public partial class Farmer : Node2D
	{
		[Export] public uint floorLayerMask = 1 << 6; // 默认只检测第6层（地面层）
		[Export] public int fieldSourceId = 0;
		[Export] public int terrainSetId = 0;
		[Export] public int terrainId = 0;

		AnimatedSprite2D _animatedSprite;

		public override void _Ready()
		{
			Node2D parent = GetParent<Node2D>();
			_animatedSprite = parent.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		}

		public override void _Process(double delta)
		{
		}

		public class TileHitInfo
		{
			public TileMapLayer TileMapLayer { get; set; }
			public Vector2I MapPos { get; set; }
		}

		public void DoPlow(TileMapLayer layer, Vector2I mapPos)
		{
			// 更换3x3格子的瓦片为耕地状态
			UpdateFieldTile(layer, mapPos);
		}

		public void MayDoPlow()
		{
			Node parent = GetParent();
			InteractPoint interactPoint = parent.GetNodeOrNull<InteractPoint>("InteractPoint");
			if (interactPoint == null)
			{
				GD.PrintErr("Cannot find InteractPoint node!");
				return;
			}
			List<TileHitInfo> hitInfos = ScanTileWithPoint(interactPoint.point.GlobalPosition, floorLayerMask);
			if (hitInfos == null || hitInfos.Count == 0)
			{
				GD.Print("No tile detected at interact point.");
				return;
			}

			if (CanDoPlow(hitInfos))
			{
				// Temp
				var floorInfo = hitInfos.Find(item => item.TileMapLayer.Name == "Floor");
				if (floorInfo != null)
				{
					var fieldInfo = floorInfo.TileMapLayer.GetNode<TileMapLayer>("Field");
					DoPlow(fieldInfo, floorInfo.MapPos);
				}
			}
			else
			{
				GD.Print($"Cannot plow at position: {interactPoint.point.GlobalPosition}");
			}
		}

		public void PlayPlowAnimation()
		{
			string currentAnimation = _animatedSprite.Animation;
			if (currentAnimation.EndsWith("Down"))
			{
				_animatedSprite.Play("plowDown");
			}
			else if (currentAnimation.EndsWith("Up"))
			{
				_animatedSprite.Play("plowUp");
			}
			else if (currentAnimation.EndsWith("Left"))
			{
				_animatedSprite.Play("plowLeft");
			}
			else if (currentAnimation.EndsWith("Right"))
			{
				_animatedSprite.Play("plowRight");
			}
		}

		public bool CanDoPlow(List<TileHitInfo> hitInfos)
		{
			return TileCanBePlowed(hitInfos);
		}

		public bool TileCanBePlowed(List<TileHitInfo> hitInfos)
		{
			foreach (var hitInfo in hitInfos)
			{
				var tileMapLayer = hitInfo.TileMapLayer;
				var mapPos = hitInfo.MapPos;

				if (tileMapLayer.Name == "Floor")
				{
					// 周围3x3格子都必须是可耕地
					for (int i = mapPos.X - 1; i <= mapPos.X + 1; i++)
					{
						for (int j = mapPos.Y - 1; j <= mapPos.Y + 1; j++)
						{
							TileData tileData = tileMapLayer.GetCellTileData(new Vector2I(i, j));
							if (tileData == null) return false;

							bool tileCanBePlowed = tileData.GetCustomData("Arable").AsBool();
							if (!tileCanBePlowed) return false;
						}
					}
				}
				else if (tileMapLayer.Name == "Field")
				{
					// 当前格子必须是可耕地
					TileData tileData = tileMapLayer.GetCellTileData(new Vector2I(mapPos.X, mapPos.Y));
					if (tileData != null)
					{
						bool tileCanBePlowed = tileData.GetCustomData("Arable").AsBool();
						if (!tileCanBePlowed) return false;
					}
				}
			}
			return true;
		}

		public List<TileHitInfo> ScanTileWithPoint(Vector2 pos, uint collisionMask)
		{
			List<TileHitInfo> hitInfos = [];
			var spaceState = GetWorld2D().DirectSpaceState;
			var query = new PhysicsPointQueryParameters2D
			{
				Position = pos,
				CollisionMask = collisionMask,
				CollideWithBodies = true
			};

			// 射线如果是从碰撞体内部发出的，是无法检测到碰撞的
			var result = spaceState.IntersectPoint(query);

			for (int i = 0; i < result.Count; i++)
			{
				var collider = result[i]["collider"].As<Node>();

				if (collider is TileMapLayer _tileMapLayer)
				{
					Vector2 localPos = _tileMapLayer.ToLocal(pos);
					Vector2I mapPos = _tileMapLayer.LocalToMap(localPos);
					hitInfos.Add(new TileHitInfo { TileMapLayer = _tileMapLayer, MapPos = mapPos });
				}
				else
				{
					GD.Print("Collider is not a TileMapLayer: " + collider.Name);
					return null;
				}
			}
			return hitInfos;
		}

	  public void OnPlowAnimationFinished()
		{
			string currentAnimation = _animatedSprite.Animation;
			if (currentAnimation.StartsWith("plow"))
			{
				MayDoPlow();
				// 更换回原来的动画
				if (currentAnimation.EndsWith("Down"))
				{
					_animatedSprite.Play("idleDown");
				}
				else if (currentAnimation.EndsWith("Up"))
				{
					_animatedSprite.Play("idleUp");
				}
				else if (currentAnimation.EndsWith("Left"))
				{
					_animatedSprite.Play("idleLeft");
				}
				else if (currentAnimation.EndsWith("Right"))
				{
					_animatedSprite.Play("idleRight");
				}
			}
		}

		void UpdateFieldTile(TileMapLayer tileMapLayer, Vector2I mapPos)
		{
			var needUpdateCellsPos = new Godot.Collections.Array<Vector2I> { mapPos };

			for (int i = mapPos.X - 1; i <= mapPos.X + 1; i++)
			{
				for (int j = mapPos.Y - 1; j <= mapPos.Y + 1; j++)
				{
					if (i == mapPos.X && j == mapPos.Y) continue;

					var existingTileData = tileMapLayer.GetCellTileData(new Vector2I(i, j));
					if (existingTileData == null) continue;

					bool tileCanBePlowed = existingTileData.GetCustomData("Arable").AsBool();
					if (!tileCanBePlowed) continue;

					needUpdateCellsPos.Add(new Vector2I(i, j));
				}
			}

			tileMapLayer.SetCellsTerrainConnect(needUpdateCellsPos, terrainSetId, terrainId);
		}
	}
}
