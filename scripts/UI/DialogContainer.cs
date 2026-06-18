using Godot;
using System;
using UI;

public partial class DialogContainer : Control
{
  RichTextLabel contentLabel;
  Button indicate;
  Button textArea;

  Tween tween;

  [Signal] public delegate void NextIndicateClickedEventHandler();
  [Signal] public delegate void TextAllDisplayedEventHandler();
  [Signal] public delegate void TextUpdatedEventHandler(string text, int visibleCharacters);
  [Signal] public delegate void TextShowStartEventHandler();
  [Signal] public delegate void TextAreaClickEventHandler();

  public override void _Ready()
  {
    contentLabel = GetNode<RichTextLabel>("TextArea/RichTextLabel");
    indicate = GetNode<Button>("NextIndicate");
    textArea = GetNode<Button>("TextArea");

    indicate.Connect("button_down", new Callable(this, "OnNextIndicateClicked"));
    textArea.Connect("button_down", new Callable(this, "OnTextAreaClicked"));
  }

  public void Open() {
    Visible = true;
  }

  public void Close() {
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


  public void OnNextIndicateClicked()
  {
    EmitSignal(SignalName.NextIndicateClicked);
  }

  public void OnTextAreaClicked() {
    EmitSignal(SignalName.TextAreaClick);
  }

  public void ShowNextIndicate()
  {
    indicate.Visible = true;
  }

  public void HideNextIndicate() {
    indicate.Visible = false;
  }
}
