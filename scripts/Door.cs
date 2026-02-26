using Godot;
using System;
using System.Collections.Generic;

namespace Game
{
  // RefCounted 允许我们在发出信号时传递这些数据
  public partial class DirectionInfo : RefCounted
  {
    public string horizontalDirectionInEntered;
    public string verticalDirectionInEntered;
    public string horizontalDirectionInExited;
    public string verticalDirectionInExited;
  }

  // 小心高速移动和瞬移
  public partial class Door : Area2D
  {
    [Signal]
    public delegate void ExitedEventHandler(Node2D curNode, Node2D exitedNode, DirectionInfo directionInfo);

    private readonly Dictionary<ulong, DirectionInfo> directionInfos = [];

    public override void _Ready()
    {
      // Area2D 建议使用 AreaEntered/AreaExited (如果你检测的是 Area2D)
      AreaEntered += OnAreaEntered;
      AreaExited += OnAreaExited;
      BodyEntered += OnBodyEntered;
      BodyExited += OnBodyExited;
    }

    public void OnAreaEntered(Area2D area)
    {
      Vector2 collisionDirection = (area.GlobalPosition - GlobalPosition).Normalized();
      string[] result = GetDirection(collisionDirection);

      ulong instanceId = area.GetInstanceId();
      // 修复：存入初始方向
      directionInfos[instanceId] = new DirectionInfo
      {
        horizontalDirectionInEntered = result[1],
        verticalDirectionInEntered = result[0]
      };
    }

    public void OnAreaExited(Area2D area)
    {
      ulong instanceId = area.GetInstanceId();
      if (!directionInfos.ContainsKey(instanceId)) return;

      Vector2 collisionDirection = (area.GlobalPosition - GlobalPosition).Normalized();
      string[] result = GetDirection(collisionDirection);

      // 核心修复：更新 struct 的部分字段，必须先取出再存入
      DirectionInfo info = directionInfos[instanceId];
      info.horizontalDirectionInExited = result[1];
      info.verticalDirectionInExited = result[0];

      directionInfos[instanceId] = info;

      EmitSignal(SignalName.Exited, this, area, info);

      // 发出信号后移除记录，防止内存泄露
      directionInfos.Remove(instanceId);
    }

    public void OnBodyExited(Node2D body)
    {
      ulong instanceId = body.GetInstanceId();
      if (!directionInfos.ContainsKey(instanceId)) return;

      Vector2 collisionDirection = (body.GlobalPosition - GlobalPosition).Normalized();
      string[] result = GetDirection(collisionDirection);

      // 核心修复：更新 struct 的部分字段，必须先取出再存入
      DirectionInfo info = directionInfos[instanceId];
      info.horizontalDirectionInExited = result[1];
      info.verticalDirectionInExited = result[0];

      directionInfos[instanceId] = info;

      EmitSignal(SignalName.Exited, this, body, info);

      // 发出信号后移除记录，防止内存泄露
      directionInfos.Remove(instanceId);
    }

    public void OnBodyEntered(Node2D body)
    {
      Vector2 collisionDirection = (body.GlobalPosition - GlobalPosition).Normalized();
      string[] result = GetDirection(collisionDirection);

      ulong instanceId = body.GetInstanceId();
      // 修复：存入初始方向
      directionInfos[instanceId] = new DirectionInfo
      {
        horizontalDirectionInEntered = result[1],
        verticalDirectionInEntered = result[0]
      };
    }

    private string[] GetDirection(Vector2 vec)
    {
      string[] result = new string[2];
      // 优化：使用阈值而不是硬等 0
      float threshold = 0.1f;

      // 纵向判断
      float downDot = vec.Dot(Vector2.Down);
      if (downDot > threshold) result[0] = "below";
      else if (downDot < -threshold) result[0] = "above";
      else result[0] = "side";

      // 横向判断
      float rightDot = vec.Dot(Vector2.Right);
      if (rightDot > threshold) result[1] = "right";
      else if (rightDot < -threshold) result[1] = "left";
      else result[1] = "front";

      return result;
    }
  }
}