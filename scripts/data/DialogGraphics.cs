using Godot;
using Utils;

namespace Data
{
	public enum DialogNodeType
	{
		Normal,
		Choice,
	}

	public enum AvatarShowPosition {
		Left,
		Right,
	}

	public class DialogEdge: AbstractEdge<DialogNode>
	{
		public string choiceText;
		// public Resource iconResource;
	}

	public class DialogNode : AbstractGraphics<DialogNode>
	{
		public string text;
		public DialogNodeType type = DialogNodeType.Normal;

		public AvatarShowPosition avatarShowPos;
		public string name;
		public Resource avatarResource;
		public Vector2 avatarPos;
		public Vector2 avatarSize;
		public bool isAvatarFlip;
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
			return CurrentNode.edges != null && 0 <= ix && ix < CurrentNode.edges.Count;
		}

		public void Next(int ix = 0)
		{
			if (CurrentNode.edges == null || ix >= CurrentNode.edges.Count) return;
			CurrentNode = CurrentNode.edges[ix].to;
		}
	}
}