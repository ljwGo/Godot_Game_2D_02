using Data;
using Game;
using Godot;
using System;

public partial class Axe : Node2D
{
  DialogManager dialogManager;

  public override void _Ready()
  {
    dialogManager = GetNode<DialogManager>("%ManagerContainer/DialogManager");
  }

  public void OnBePicked(Picker picker, Pickable pickable)
  {
    InventoryItem inventoryItem = pickable.GetParent().GetNode<InventoryItem>("InventoryItem");
    var node = new DialogNode() { text = $"你获得了[color=#FF0000] **{inventoryItem.itemName}** [/color], 可以使用它来砍伐木头了." };
    var dialogGraphics = new DialogGraphics(node);

    dialogManager.OpenDialog();
    // dialogManager.AutoPlayMode = true;
    dialogManager.NewDialog(dialogGraphics);
  }

  public void OnNotBePicked(Picker picker, Pickable pickable, string result)
  {
    var node = new DialogNode() { text = $"由于{result}, 无法拾取." };
    var dialogGraphics = new DialogGraphics(node);

    dialogManager.OpenDialog();
    // dialogManager.AutoPlayMode = true;
    dialogManager.NewDialog(dialogGraphics);
  }
}
