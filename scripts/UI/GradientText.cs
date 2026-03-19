using Godot;

namespace UI
{
	public partial class GradientText : Label
	{
		[Export] public float GradientSpeed = 0.05f;

		Timer timer;

		[Signal] public delegate void TextFullyDisplayedEventHandler();
		[Signal] public delegate void TextUpdatedEventHandler(string currentText);

		public override async void _Ready()
		{
			await ToSignal(GetTree().CreateTimer(1), "timeout");
			PushText("这是一个渐变显示的文本示例。 你可以在编辑器中调整 GradientSpeed 来改变显示速度。 当文本完全显示后，计时器会自动停止并释放。");
		}

		public void PushText(string text)
		{
			this.Text = "";
			if (timer != null && IsInstanceValid(timer))
			{
				timer.Stop();
				timer.QueueFree();
			}

			timer = new Timer
			{
				WaitTime = GradientSpeed,
				OneShot = false,
				Autostart = true,
			};
			timer.Timeout += () =>
			{
				if (Text.Length < text.Length)
				{
					Text += text[Text.Length];
					EmitSignal(SignalName.TextUpdated, Text);
				}
				else
				{
					timer.Stop();
					timer.QueueFree();
					EmitSignal(SignalName.TextFullyDisplayed);
				}
			};

			AddChild(timer);
		}
	}
}
