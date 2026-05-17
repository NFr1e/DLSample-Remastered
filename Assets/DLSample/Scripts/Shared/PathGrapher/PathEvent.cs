using System;
using UnityEngine;
using DLSample.Shared;

namespace DLSample.Editor.PathGrapher
{
    /// <summary>
    /// 路径事件接口，所有路径事件必须实现此接口
    /// </summary>
    public interface IPathEvent
    {
        double GlobalTime { get; set; }
    }

    /// <summary>
    /// 点路径事件基类，发生在单个时间点
    /// </summary>
    [Serializable] public abstract class PointPathEvent : IPathEvent
    {
        [SerializeField] private double _globalTime;
        public double GlobalTime { get => _globalTime; set => _globalTime = value; }
    }

    /// <summary>
    /// 段路径事件基类，发生在一个时间段内
    /// </summary>
    [Serializable] public abstract class SegmentPathEvent : IPathEvent
    {
        [SerializeField] private double _startTime;
        [SerializeField] private double _endTime;

        public double GlobalTime { get => _startTime; set => _startTime = value; }
        public double StartTime { get => _startTime; set => _startTime = value; }
        public virtual double EndTime { get => _endTime; set => _endTime = value; }
    }

    #region PointEvents

    /// <summary>
    /// 强制转向事件
    /// </summary>
    [Serializable]
    public class ForceTurnEvent : PointPathEvent
    {
    }

    /// <summary>
    /// 速度变更事件
    /// </summary>
    [Serializable]
    public class SpeedChangeEvent : PointPathEvent
    {
        public float newSpeed;
    }

    /// <summary>
    /// 重力变更事件
    /// </summary>
    [Serializable]
    public class GravityChangeEvent : PointPathEvent
    {
        public Vector3 newGravity;
    }

    /// <summary>
    /// 方向变更事件
    /// </summary>
    [Serializable]
    public class DirectionChangeEvent : PointPathEvent
    {
        public PlayerDirections newDirections = new();
    }
    #endregion

    #region SegmentEvents
    /// <summary>
    /// 传送事件
    /// </summary>
    [Serializable]
    public class TeleportEvent : SegmentPathEvent
    {
        public Vector3 targetPosition;

        public override double EndTime { get => base.StartTime + 0.0001f; }
    }

    /// <summary>
    /// 跳跃事件
    /// </summary>
    [Serializable]
    public class JumpEvent : SegmentPathEvent
    {
        public Vector3 velocity;
    }
    #endregion
}