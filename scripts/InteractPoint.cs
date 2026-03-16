using Godot;
using System;

namespace Game
{
	public partial class InteractPoint : Area2D
	{
		public CollisionShape2D point = null;

		public override void _Ready()
		{
			point = GetNode<CollisionShape2D>("CollisionShape2D");
		}

		public override void _Process(double delta)
		{
		}

    public override void _Draw()
		{
		}
	}
}
