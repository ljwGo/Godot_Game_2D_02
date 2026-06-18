using System.Collections.Generic;

namespace Game.Utils {
  public abstract class AbstractGraphics<NodeT>
  {
    // 有向图, 通道是单向的
    public List<NodeT> vectorNeighbours;
  }
}