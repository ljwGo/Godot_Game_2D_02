using Godot;
using System.Collections.Generic;

namespace Game.Utils
{
  // 1. 定义处理者的接口
  public interface IHandler<T>
  {
    IHandler<T> SetNext(IHandler<T> next);
    void Handle(T request);
  }

  // 2. 抽象基类，处理“传递”逻辑
  public abstract class BaseHandler<T> : IHandler<T>
  {
    private IHandler<T> _nextHandler;

    public IHandler<T> SetNext(IHandler<T> next)
    {
      _nextHandler = next;
      // 返回 next 以便支持流式链式调用: .SetNext(h1).SetNext(h2)
      return next;
    }

    public virtual void Handle(T request)
    {
      // 如果自己没处理完，或者需要继续传递，则交给下一个
      _nextHandler?.Handle(request);
    }
  }
}