using Godot;
using System;

namespace Debug
{
	public partial class InteractDraw : CollisionShape2D
	{
		public override void _Draw()
		{
			base._Draw();
			DrawCircle(Vector2.Zero, 4, new Color(1, 0, 0, 0.5f));
		}
	}
}
