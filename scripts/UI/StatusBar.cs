using Godot;
using System;

public partial class StatusBar : VBoxContainer
{
  // 一颗心等于两滴血
  [Export]
  public int HeartCount { get; set; } = 3;
  [Export]
  public int CurrentHealth { get; set; } = 6;

  [Export]
  public int CurrentStamina { get; set; } = 100;
  [Export]
  public int MaxStamina { get; set; } = 100;
  [Export]
  public int StaminaBarWidthPerStamina { get; set; } = 2;

  [Export]
  public int HeartUISize { get; set; } = 24;

  [Export]
  public Resource FullHeartResource { get; set; }
  [Export]
  public Resource HalfHeartResource { get; set; }
  [Export]
  public Resource EmptyHeartResource { get; set; }

  HBoxContainer heartBox;
  ProgressBar progressBar;

  public override void _Ready()
  {
    VBoxContainer HeartAProgress = GetNode<VBoxContainer>("%HeartAProgress");
    heartBox = HeartAProgress.GetNode<HBoxContainer>("HeartBox");
    progressBar = HeartAProgress.GetNode<ProgressBar>("ProgressBar");

    Init();
  }

  private void Init()
  {
    InitHeartBox();
    InitProgressBar();
  }

  private void InitHeartBox()
  {
    int fullHearts = CurrentHealth / 2;
    int halfHearts = CurrentHealth % 2;
    // int emptyHearts = HeartCount - fullHearts - halfHearts;

    int f2hHearts = fullHearts + halfHearts;

    for (int i = 0; i < HeartCount; i++)
    {
      // 检测heartBox下对应索引的子节点是否存在
      bool heartExists = i < heartBox.GetChildCount();
      TextureRect heart;

      if (heartExists)
      {
        // 存在则获取对应索引的TextureRect节点
        heart = heartBox.GetChild<TextureRect>(i);
      }
      else
      {
        // 不存在则添加TextureRect节点
        heart = new TextureRect();
        heart.CustomMinimumSize = new Vector2(HeartUISize, HeartUISize);
        heartBox.AddChild(heart);
      }

      if (i < fullHearts)
      {
        heart.Texture = FullHeartResource as Texture2D;
      }
      else if (i < f2hHearts)
      {
        heart.Texture = HalfHeartResource as Texture2D;
      }
      else
      {
        heart.Texture = EmptyHeartResource as Texture2D;
      }
    }
  }

  private void InitProgressBar()
  {
    progressBar.MaxValue = MaxStamina;
    progressBar.Value = CurrentStamina;
    progressBar.CustomMinimumSize = new Vector2(MaxStamina * StaminaBarWidthPerStamina, progressBar.CustomMinimumSize.Y);

    // Todo: 添加体力条的状态颜色
    Label progressLabel = progressBar.GetNode<Label>("Label");
    progressLabel.Text = $"{CurrentStamina} / {MaxStamina}";
  }
}
