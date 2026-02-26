using Godot;
using System;

namespace Game
{
	[GlobalClass] // GlobalClass 使得这个 Resource 可以在编辑器中被创建和编辑
	public partial class LootItemParam : Resource
	{
		[Export] public PackedScene lootScene;
		[Export] public uint quantity = 1;
		[Export] public uint stackSize = 1;
		[Export] public float dropChance = 1.0f; // 0.0 - 1.0
	}
}