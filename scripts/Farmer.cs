using Godot;
using System;

namespace Game
{
	public partial class Farmer : Node2D
	{
		public override void _Ready()
		{
		}

		public override void _Process(double delta)
		{
		}

		public void Plow()
		{

		}

		public void ScanTileWithRay(Vector2 from, Vector2 to)
		{
			var spaceState = GetWorld2D().DirectSpaceState;
			var query = PhysicsRayQueryParameters2D.Create(from, to);
			query.CollideWithBodies = true;

			var result = spaceState.IntersectRay(query);

			if (result.Count > 0)
			{
				// 1. 获取撞到的节点
				var collider = result["collider"].As<Node>();

				// 2. 确认撞到的是不是 TileMapLayer
				if (collider is TileMapLayer tileLayer)
				{
					// 3. 获取世界坐标和法线
					Vector2 hitPoint = result["position"].As<Vector2>();
					Vector2 hitNormal = result["normal"].As<Vector2>();

					// 资深避坑技巧：
					// 射线撞击点往往正好在两个图块的像素边缘。
					// 浮点数精度问题可能导致坐标转换时“偏移”到相邻的错误格子里。
					// 解决办法：沿着射线的方向，或者沿着法线的反方向，往图块内部推进一点点（比如 1 个像素）。
					Vector2 insideTilePoint = hitPoint - (hitNormal * 1.0f);

					// 4. 将世界坐标转换为 TileMapLayer 的局部坐标
					Vector2 localPos = tileLayer.ToLocal(insideTilePoint);

					// 5. 将局部坐标转换为网格（Map）坐标 (例如 X:5, Y:10)
					Vector2I mapPos = tileLayer.LocalToMap(localPos);

					// 6. 获取该坐标上的图块数据 (TileData)
					TileData tileData = tileLayer.GetCellTileData(mapPos);

					if (tileData != null)
					{
						// 7. 读取自定义数据！
						// 注意：这里的名称必须和你 TileSet 里设置的 Name 完全一致
						bool isDestructible = tileData.GetCustomData("destructible").AsBool();

						GD.Print($"击中了图块坐标: {mapPos}, 是否可破坏: {isDestructible}");

						if (isDestructible)
						{
							// 比如：执行破坏逻辑，将该坐标的图块替换为空（-1）
							tileLayer.SetCell(mapPos, -1);
						}
					}
				}
			}
		}
	}
}
