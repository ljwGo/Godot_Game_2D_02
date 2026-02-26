using Godot;
using System;

namespace Game
{
	public partial class Absorber : Area2D
	{
		[Export(PropertyHint.Range, "0,1024,1")] public float absorbRadius = 32.0f;

		public override void _Ready()
		{
			// 设置半径
			CollisionShape2D collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
			Shape2D shape2D = collisionShape.Shape;
			if (shape2D is CircleShape2D circleShape)
			{
				circleShape.Radius = absorbRadius;
			}
		}
	}
}
