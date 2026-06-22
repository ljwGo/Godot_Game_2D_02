using Godot;
using System;

public partial class Pickable : Node
{
  public Func<Picker, bool> CanPick = (picker) => true;

  [Signal] public delegate void BePickedEventHandler(Picker picker, Pickable pickable);
  [Signal] public delegate void NotBePickedEventHandler(Picker picker, Pickable pickable, string result);
}
