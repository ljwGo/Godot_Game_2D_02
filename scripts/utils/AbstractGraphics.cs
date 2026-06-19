using System.Collections.Generic;

namespace Utils {
  public abstract class AbstractEdge<NodeT>
  {
    public NodeT to;
  }

  public abstract class AbstractGraphics<NodeT>
  {
    // 有向图, 通道是单向的
    public List<AbstractEdge<NodeT>> edges;
  }
}