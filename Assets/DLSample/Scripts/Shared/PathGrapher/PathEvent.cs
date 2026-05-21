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

        /// <summary>
        /// 是否在时间线上作为路点边界（产生新的 Waypoint）
        /// </summary>
        bool IsWaypointBoundary { get; }

        /// <summary>
        /// 对模拟状态施加事件的物理效应
        /// </summary>
        void ApplyTo(ref SimulationStatus state, double time);
    }

    /// <summary>
    /// 点路径事件基类，发生在单个时间点
    /// </summary>
    [Serializable] public abstract class PointPathEvent : IPathEvent
    {
        [SerializeField] private double _globalTime;
        public double GlobalTime { get => _globalTime; set => _globalTime = value; }

        public virtual bool IsWaypointBoundary => false;
        public virtual void ApplyTo(ref SimulationStatus state, double time) { }
    }

    /// <summary>
    /// 段路径事件基类，发生在一个时间段内
    /// </summary>
    [Serializable] public abstract class SegmentPathEvent : IPathEvent
    {
        [SerializeField] private double _startTime;
        [SerializeField] private double _endTime;

        private const double EPSILON = 0.0001;

        public double GlobalTime { get => _startTime; set => _startTime = value; }
        public double StartTime { get => _startTime; set => _startTime = value; }
        public virtual double EndTime { get => _endTime; set => _endTime = value; }

        public virtual bool IsWaypointBoundary => false;

        public virtual void ApplyTo(ref SimulationStatus state, double time)
        {
            bool atStart = Math.Abs(time - StartTime) < EPSILON;
            bool atEnd = Math.Abs(time - EndTime) < EPSILON;

            if (atStart) OnApplyStart(ref state);
            if (atEnd) OnApplyEnd(ref state);
        }

        protected virtual void OnApplyStart(ref SimulationStatus state) { }
        protected virtual void OnApplyEnd(ref SimulationStatus state) { }
    }

    #region PointEvents

    /// <summary>
    /// 强制转向事件
    /// </summary>
    [Serializable]
    public class ForceTurnEvent : PointPathEvent
    {
        public override bool IsWaypointBoundary => true;
    }

    /// <summary>
    /// 速度变更事件
    /// </summary>
    [Serializable]
    public class SpeedChangeEvent : PointPathEvent
    {
        public float newSpeed;

        public override void ApplyTo(ref SimulationStatus state, double time)
        {
            state.currentSpeed = newSpeed;
        }
    }

    /// <summary>
    /// 重力变更事件
    /// </summary>
    [Serializable]
    public class GravityChangeEvent : PointPathEvent
    {
        public Vector3 newGravity;

        public override void ApplyTo(ref SimulationStatus state, double time)
        {
            state.currentGravity = newGravity;
        }
    }

    /// <summary>
    /// 方向变更事件
    /// </summary>
    [Serializable]
    public class DirectionChangeEvent : PointPathEvent
    {
        public PlayerDirections newDirections = new();

        public override void ApplyTo(ref SimulationStatus state, double time)
        {
            state.currentDirecion = newDirections.Clone();
            state.currentDirecion.Reset();
        }
    }
    #endregion

    #region SegmentEvents
    /// <summary>
    /// 瞬时传送事件，EndTime 恒等于 StartTime
    /// </summary>
    [Serializable]
    public class TeleportEvent : SegmentPathEvent
    {
        public Vector3 targetPosition;

        public override double EndTime { get => StartTime; set { } }

        protected override void OnApplyStart(ref SimulationStatus state)
        {
            state.isTeleporting = true;
        }

        protected override void OnApplyEnd(ref SimulationStatus state)
        {
            state.position = targetPosition;
            state.isTeleport = true;
            state.isTeleporting = false;
        }
    }

    /// <summary>
    /// 跳跃事件
    /// </summary>
    [Serializable]
    public class JumpEvent : SegmentPathEvent
    {
        public Vector3 velocity;

        protected override void OnApplyStart(ref SimulationStatus state)
        {
            state.isJumping = true;
            state.verticalVelocity = velocity;
        }

        protected override void OnApplyEnd(ref SimulationStatus state)
        {
            state.isJumping = false;
            state.verticalVelocity = Vector3.zero;
        }
    }
    #endregion
}
