using Godot;
using System;

public partial class Openable : Node
{
  public bool HasOpened { get; set; } = false;
  public Func<Opener, bool> CanOpen = (opener) => true;

  [Signal] public delegate void BeOpenedEventHandler(Opener opener, Openable openable);
}
