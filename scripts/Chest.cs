using Godot;
using System;

namespace Game
{
	public partial class Chest : StaticBody2D
	{
		[Export(PropertyHint.Enum, "Up,Right,Down,Left")]
		public string FacingDirection { get; set; } = "Down";

		AnimatedSprite2D _animatedSprite2D;
		LootDroppable lootDroppable;

		public override void _Ready()
		{
			_animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
			lootDroppable = GetNode<LootDroppable>("LootDroppable");
		}

		public void PlayOpenAnimation()
		{
			_animatedSprite2D.Play($"open{FacingDirection}");
		}

		public void DropLoot()
		{
			lootDroppable.DoDropLoot();
		}

		public void OnAnimationFinished()
		{
			string animationName = _animatedSprite2D.Animation;
			if (animationName.StartsWith("open"))
			{
				DropLoot();
			}
		}
	}
}