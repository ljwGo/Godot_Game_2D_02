using Godot;
using System;

public partial class Picker : Node
{
  public Func<Pickable, bool> CanPick = (pickable) => true;
  public Func<Pickable, string> GetResult;

  private string DEFAULT_NOT_PICKED_RESULT = "背包已满,无法拾取";

  [Signal] public delegate void PickEventHandler(Picker picker, Pickable pickable);
  [Signal] public delegate void NotPickEventHandler(Picker picker, Pickable pickable, string result);

  public void MayDoPick(Pickable pickable)
  {
    if (CanPick(pickable) && pickable.CanPick(this)) {
      EmitSignal(SignalName.Pick, this, pickable);
      pickable.EmitSignal(Pickable.SignalName.BePicked, this, pickable);
    }
    else {
      string result = GetResult != null ? GetResult(pickable) : DEFAULT_NOT_PICKED_RESULT;
      EmitSignal(SignalName.NotPick, this, pickable, result);
      pickable.EmitSignal(Pickable.SignalName.NotBePicked, this, pickable, result);
    }
  }
}
