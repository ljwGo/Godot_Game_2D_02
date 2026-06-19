using Data;
using Godot;
using System.Collections.Generic;
using Utils;

namespace Game
{

	public partial class DialogChoiceClickArgs : RefCounted
	{
		public DialogEdge edge;
	}

	public partial class DialogManager : Node
	{
		[Export] public float ShowSpeed = 0.02f;

		DialogGraphics dialogGraphics;
		DialogContainer dialogContainer;

		[Signal] public delegate void DialogChoiceClickEventHandler(DialogChoiceClickArgs args);

		public override void _Ready()
		{
			dialogContainer = GetNode<DialogContainer>("%DialogContainer");

			Init();

			// 测试用
			Test();
		}

		public void Test()
		{
			DialogNode node1 = new DialogNode() { text = "这是第一句对话话话话话话话话话话话话话话话话话话话话话话话话话话话。", type = DialogNodeType.Choice };
			DialogNode node2 = new DialogNode() { text = "这是第二句" };
			DialogNode node3 = new DialogNode() { text = "这是第三句" };
			DialogNode node4 = new DialogNode() { text = "这是第四句" };
			DialogNode node5 = new DialogNode() { text = "这是第五句" };

			node1.edges = new List<AbstractEdge<DialogNode>>() {
				new DialogEdge() { to = node2, choiceText = "正确" },
				new DialogEdge() { to = node3, choiceText = "错误" },
			};

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
			dialogContainer.HideChoices();
		}

		public void OnTextAllDisplayed()
		{
			if (!dialogGraphics.HasNeighbour(0)) return;

			DialogNodeType nodeType = dialogGraphics.CurrentNode.type;
			switch (nodeType)
			{
				case DialogNodeType.Normal:
					dialogContainer.HideChoices();
					dialogContainer.ShowNextIndicate();
					break;
				case DialogNodeType.Choice:
					dialogContainer.ShowChoices();
					dialogContainer.HideNextIndicate();
					VBoxContainer vBox = dialogContainer.GetNode<VBoxContainer>("ChoiceContainer/VBoxContainer");

					// 生成选项按钮
					for (int i = 0; i < dialogGraphics.CurrentNode.edges.Count; i++)
					{
						AbstractEdge<DialogNode> edge = dialogGraphics.CurrentNode.edges[i];
						string text = (edge as DialogEdge).choiceText;
						int ix = i; // 注意闭包问题, 需要复制一份 ix

						dialogContainer.AddChoice(text, () =>
						{
							dialogContainer.HideChoices();
							Next(ix);

							EmitSignal(SignalName.DialogChoiceClick, new DialogChoiceClickArgs() { edge = edge as DialogEdge });
						});
					}
					break;
			}
		}

		public void OnTextAreaClicked()
		{
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
}
