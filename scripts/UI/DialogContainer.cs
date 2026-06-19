using Data;
using Godot;
using System;
using UI;

public partial class DialogContainer : Control
{
  RichTextLabel contentLabel;
  RichTextLabel indicateLabel;
  Button indicateBtn;
  Button textAreaBtn;
  PanelContainer choiceContainer;
  VBoxContainer vChoicesBox;
  PanelContainer leftAvatarContainer;
  PanelContainer rightAvatarContainer;

  Tween tween;

  [Signal] public delegate void NextIndicateClickedEventHandler();
  [Signal] public delegate void TextAllDisplayedEventHandler();
  [Signal] public delegate void TextUpdatedEventHandler(string text, int visibleCharacters);
  [Signal] public delegate void TextShowStartEventHandler();
  [Signal] public delegate void TextAreaClickEventHandler();

  public override void _Ready()
  {
    VBoxContainer vBoxContainer = GetNode<VBoxContainer>("TextContainer/VBoxContainer");
    contentLabel = vBoxContainer.GetNode<RichTextLabel>("ContentArea");
    indicateLabel = vBoxContainer.GetNode<RichTextLabel>("NextIndicate");

    indicateBtn = indicateLabel.GetNode<Button>("Button");
    textAreaBtn = contentLabel.GetNode<Button>("Button");

    choiceContainer = GetNode<PanelContainer>("ChoiceContainer");
    vChoicesBox = choiceContainer.GetNode<VBoxContainer>("VBoxContainer");

    indicateBtn.Connect("button_down", new Callable(this, "OnNextIndicateClicked"));
    textAreaBtn.Connect("button_down", new Callable(this, "OnTextAreaClicked"));

    leftAvatarContainer = GetNode<PanelContainer>("LeftAvatar");
    rightAvatarContainer = GetNode<PanelContainer>("RightAvatar");
  }

  public void Open()
  {
    Visible = true;
  }

  public void Close()
  {
    Visible = false;
  }

  public bool IsShowing()
  {
    return tween != null && tween.IsRunning();
  }

  public void Show(string text, float speed)
  {
    if (text == null || IsShowing()) return;

    contentLabel.Text = text;
    contentLabel.VisibleCharacters = 0;

    // Debug: duration的时间不准确, 要移除 [] 标签内的字符数
    float duration = text.Length * speed;

    tween = CreateTween();
    tween.TweenProperty(contentLabel, "visible_characters", text.Length, duration).SetTrans(Tween.TransitionType.Linear);

    tween.StepFinished += (ix) =>
    {
      EmitSignal(SignalName.TextUpdated, contentLabel.Text, contentLabel.VisibleCharacters);
    };

    tween.Finished += () =>
    {
      EmitSignal(SignalName.TextAllDisplayed);
      tween = null;
    };

    EmitSignal(SignalName.TextShowStart);
  }

  public void ShowInstance(string text)
  {
    if (text == null) return;

    if (IsShowing())
    {
      tween.Stop();
      tween.Dispose();
      tween = null;
    }

    contentLabel.Text = text;
    contentLabel.VisibleCharacters = text.Length;

    EmitSignal(SignalName.TextAllDisplayed);
  }

  public void ShowAvatar(
    AvatarShowPosition showPos,
    Resource resource,
    Vector2 avatarPos,
    Vector2 avatarSize,
    string name,
    bool isFlipH = false
  )
  {
    ShowAvatar(showPos);

    PanelContainer targetContainer = null;
    switch (showPos)
    {
      case AvatarShowPosition.Left:
        targetContainer = leftAvatarContainer;
        break;
      case AvatarShowPosition.Right:
        targetContainer = rightAvatarContainer;
        break;
    }

    if (targetContainer == null) return;

    TextureRect targetRect = targetContainer.GetNode<TextureRect>("VBoxContainer/Avatar/TextureRect");
    Label nameLabel = targetContainer.GetNode<Label>("VBoxContainer/Name");

    nameLabel.Text = name;
    AtlasTexture atlasTexture = targetRect.Texture as AtlasTexture;
    atlasTexture.Atlas = resource as Texture2D;
    atlasTexture.Region = new Rect2(avatarPos, avatarSize);

    targetRect.FlipH = isFlipH;
  }

  public void OnNextIndicateClicked()
  {
    EmitSignal(SignalName.NextIndicateClicked);
  }

  public void OnTextAreaClicked()
  {
    EmitSignal(SignalName.TextAreaClick);
  }

  public void ShowAvatar(AvatarShowPosition showPos)
  {
    switch (showPos)
    {
      case AvatarShowPosition.Left:
        leftAvatarContainer.Visible = true;
        break;
      case AvatarShowPosition.Right:
        rightAvatarContainer.Visible = true;
        break;
    }
  }

  public void HideAvatar(AvatarShowPosition showPos)
  {
    switch (showPos)
    {
      case AvatarShowPosition.Left:
        leftAvatarContainer.Visible = false;
        break;
      case AvatarShowPosition.Right:
        rightAvatarContainer.Visible = false;
        break;
    }
  }

  public void HideAllAvatar()
  {
    leftAvatarContainer.Visible = false;
    rightAvatarContainer.Visible = false;
  }

  public void ShowChoices()
  {
    choiceContainer.Visible = true;
  }

  public void HideChoices()
  {
    choiceContainer.Visible = false;
  }

  public void ShowNextIndicate()
  {
    indicateLabel.Visible = true;
  }

  public void HideNextIndicate()
  {
    indicateLabel.Visible = false;
  }

  public void ClearChoices()
  {
    foreach (Button button in vChoicesBox.GetChildren())
    {
      button.QueueFree();
    }
  }

  public void AddChoice(string choiceText, Action onClick)
  {
    Button choiceButton = new Button();
    choiceButton.Text = choiceText;
    choiceButton.Connect("button_down", Callable.From(onClick));

    vChoicesBox.AddChild(choiceButton);
  }
}
