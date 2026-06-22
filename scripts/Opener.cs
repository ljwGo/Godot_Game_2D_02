using Godot;
using System;

public partial class Opener : Node
{
  public Func<Openable, bool> CanOpen = (openable) => true;

  [Signal] public delegate void OpenEventHandler(Opener opener, Openable openable);

  public void MayDoOpen(Openable openable)
  {
    if (CanOpen(openable) && openable.CanOpen(this) && !openable.HasOpened)
    {
      EmitSignal(SignalName.Open, this, openable);
      openable.EmitSignal(Openable.SignalName.BeOpened, this, openable);
      openable.HasOpened = true;
    }
  }
}
