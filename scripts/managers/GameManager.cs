using Godot;

namespace Game
{
  public partial class GameManager : Node
  {
    public override void _Ready()
    {
      // 这里可以放一些全局初始化逻辑，比如加载资源、设置单例等
      GD.Print("GameManager is ready!");
    }
  }
}