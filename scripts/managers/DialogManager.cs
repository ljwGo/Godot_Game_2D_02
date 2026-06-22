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

		public bool AutoPlayMode = false;

		DialogGraphics dialogGraphics;
		DialogContainer dialogContainer;

		[Signal] public delegate void DialogChoiceClickEventHandler(DialogChoiceClickArgs args);

		public override void _Ready()
		{
			dialogContainer = GetNode<DialogContainer>("%DialogContainer");

			Init();

			// 测试用
			// Test();
		}

		public void Test()
		{
			Resource cat = ResourceLoader.Load<Resource>("res://tres/UI/avatar/cat.tres");
			Resource eggBoy = ResourceLoader.Load<Resource>("res://tres/UI/avatar/egg_boy.tres");

			DialogNode node1 = new DialogNode()
			{
				text = "你好呀, 我是猫猫",
				name = "猫猫",
				avatarShowPos = AvatarShowPosition.Left,
				avatarResource = cat,
				avatarPos = new Vector2(0, 0),
				avatarSize = new Vector2(32, 32),
			};

			DialogNode node2 = new DialogNode()
			{
				text = "你好呀, 我是蛋蛋男孩",
				name = "蛋蛋男孩",
				avatarShowPos = AvatarShowPosition.Right,
				avatarResource = eggBoy,
				avatarPos = new Vector2(0, 0),
				avatarSize = new Vector2(40, 40)
			};

			DialogNode node3 = new DialogNode()
			{
				text = "你有看到一个头戴帽子的女孩吗?",
				name = "猫猫",
				avatarShowPos = AvatarShowPosition.Left,
				avatarResource = cat,
				avatarPos = new Vector2(0, 0),
				avatarSize = new Vector2(32, 32),
			};

			DialogNode node4 = new DialogNode() { text = "两人相视而笑" };

			node1.edges = new List<AbstractEdge<DialogNode>>() { new DialogEdge { to = node2 } };
			node2.edges = new List<AbstractEdge<DialogNode>>() { new DialogEdge { to = node3 } };
			node3.edges = new List<AbstractEdge<DialogNode>>() { new DialogEdge { to = node4 } };

			OpenDialog();
			NewDialog(new DialogGraphics(node1));
		}

		public void Init()
		{
			dialogContainer.Connect("NextIndicateClicked", new Callable(this, "OnNextIndicateClicked"));
			dialogContainer.Connect("TextShowStart", new Callable(this, "OnTextShowStart"));
			dialogContainer.Connect("TextAllDisplayed", new Callable(this, "OnTextAllDisplayed"));
			dialogContainer.Connect("TextAreaClick", new Callable(this, "OnTextAreaClicked"));
		}

		// 开启一个新的对话图
		public void NewDialog(DialogGraphics dialogGraphics)
		{
			this.dialogGraphics = dialogGraphics;
			dialogContainer.Show(dialogGraphics.CurrentNode.text, ShowSpeed);
			if (dialogGraphics.CurrentNode.avatarResource != null)
			{
				dialogContainer.ShowAvatar(
					dialogGraphics.CurrentNode.avatarShowPos,
					dialogGraphics.CurrentNode.avatarResource,
					dialogGraphics.CurrentNode.avatarPos,
					dialogGraphics.CurrentNode.avatarSize,
					dialogGraphics.CurrentNode.name,
					dialogGraphics.CurrentNode.isAvatarFlip
				);
			}
		}

		// 下一个节点
		public void Next(int ix = 0)
		{
			if (!dialogContainer.IsShowing() && dialogGraphics.HasNeighbour(ix))
			{
				dialogGraphics.Next(ix);
				dialogContainer.Show(dialogGraphics.CurrentNode.text, ShowSpeed);

				dialogContainer.HideAllAvatar();
				if (dialogGraphics.CurrentNode.avatarResource != null)
				{
					dialogContainer.ShowAvatar(
						dialogGraphics.CurrentNode.avatarShowPos,
						dialogGraphics.CurrentNode.avatarResource,
						dialogGraphics.CurrentNode.avatarPos,
						dialogGraphics.CurrentNode.avatarSize,
						dialogGraphics.CurrentNode.name,
						dialogGraphics.CurrentNode.isAvatarFlip
					);
				}
			}
		}

		public void OpenDialog()
		{
			dialogContainer.Open();
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
			if (!dialogGraphics.HasNeighbour(0)) {
				if (AutoPlayMode) {
					dialogContainer.Close();
				}
				return;
			}

			DialogNodeType nodeType = dialogGraphics.CurrentNode.type;
			switch (nodeType)
			{
				case DialogNodeType.Normal:
					if (AutoPlayMode)
					{
						Next(0);
					}
					else
					{
						dialogContainer.HideChoices();
						dialogContainer.ShowNextIndicate();
					}
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
