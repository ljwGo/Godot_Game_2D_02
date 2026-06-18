using Game.Utils;
using Godot;
using System;
using System.Collections.Generic;

public class DialogNode : AbstractGraphics<DialogNode>
{
	public string text;
}

public class DialogGraphics
{
	// 当前对话节点
	public DialogNode CurrentNode { get; private set; }
	// 初始对话节点
	public DialogNode InitNode { get; private set; }

	public DialogGraphics(DialogNode initNode)
	{
		this.InitNode = initNode;
		this.CurrentNode = initNode;
	}

	public bool HasNeighbour(int ix)
	{
		return CurrentNode.vectorNeighbours != null && 0 <= ix && ix < CurrentNode.vectorNeighbours.Count;
	}

	public void Next(int ix = 0)
	{
		if (CurrentNode.vectorNeighbours == null || ix >= CurrentNode.vectorNeighbours.Count) return;
		CurrentNode = CurrentNode.vectorNeighbours[ix];
	}
}

public partial class DialogManager : Node
{
	[Export] public float ShowSpeed = 0.1f;

	DialogGraphics dialogGraphics;
	DialogContainer dialogContainer;

	public override void _Ready()
	{
		dialogContainer = GetNode<DialogContainer>("%DialogContainer");

		Init();

		// 测试用
		Test();
	}

	public void Test()
	{
		DialogNode node1 = new DialogNode() { text = "这是第一句对话话话话话话话话话话话话话话话话话话话话话话话话话话话。" };
		DialogNode node2 = new DialogNode() { text = "这是第二句对话话话话话话话话话话话话话[color=#ff0000]话话话话话话话话话话[/color]话话话话话话话话话话话话话话话话话话话话话话话话话话话话话话话。" };
		DialogNode node3 = new DialogNode() { text = "这是第三句对话话话话话话话话话话话话话话话话话话话话话话话话话话话。" };

		node1.vectorNeighbours = new List<DialogNode>() { node2 };
		node2.vectorNeighbours = new List<DialogNode>() { node3 };

		Start(new DialogGraphics(node1));
	}

	public void Init()
	{
		dialogContainer.Connect("NextIndicateClicked", new Callable(this, "OnNextIndicateClicked"));
		dialogContainer.Connect("TextShowStart", new Callable(this, "OnTextShowStart"));
		dialogContainer.Connect("TextAllDisplayed", new Callable(this, "OnTextAllDisplayed"));
		dialogContainer.Connect("TextAreaClick", new Callable(this, "OnTextAreaClicked"));
	}

	// 开启一个新的对话图
	public void Start(DialogGraphics dialogGraphics)
	{
		this.dialogGraphics = dialogGraphics;
		dialogContainer.Show(dialogGraphics.CurrentNode.text, ShowSpeed);
	}

	// 下一个节点
	public void Next(int ix = 0)
	{
		if (!dialogContainer.IsShowing() && dialogGraphics.HasNeighbour(ix))
		{
			dialogGraphics.Next(ix);
			dialogContainer.Show(dialogGraphics.CurrentNode.text, ShowSpeed);
		}
	}

	public void OnNextIndicateClicked()
	{
		Next(0);
	}

	public void OnTextShowStart()
	{
		dialogContainer.HideNextIndicate();
	}

	public void OnTextAllDisplayed()
	{
		if (dialogGraphics.HasNeighbour(0))
		{
			dialogContainer.ShowNextIndicate();
		}
	}

	public void OnTextAreaClicked() {
		if (dialogContainer.IsShowing())
		{
			dialogContainer.ShowInstance(dialogGraphics.CurrentNode.text);
		}
		else if (!dialogGraphics.HasNeighbour(0))
		{
			dialogContainer.Close();
		}
	}
}
