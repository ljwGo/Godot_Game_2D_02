using Godot;
using System;

public partial class Cuttable : Node
{
	// 砍倒需要的砍刀力总和
	[Export] public float TotalChopPower = 3.0f;

	[Signal] public delegate void ChoppedEventHandler(Cuttable cuttable);
	[Signal] public delegate void ChopPowerChangedEventHandler(Cuttable cuttable, float currentChopPower);

	// 当前砍刀力
	public float CurChopPower = 0.0f;

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

	public void DoChop(float chopPower)
	{
		CurChopPower += chopPower;
		if (chopPower > 0)
		{
			EmitSignal(SignalName.ChopPowerChanged, this, CurChopPower);
		}

		if (CurChopPower >= TotalChopPower)
		{
			// 这里可以添加一些砍倒后的逻辑，比如播放倒下的动画、掉落物品等
			EmitSignal(SignalName.Chopped, this);
		}
	}
}
