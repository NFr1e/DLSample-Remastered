using System.Collections.Generic;
using UnityEngine;
using DLSample.Shared;

namespace DLSample.Editor.PathGrapher
{
    /// <summary>
    /// 模拟器错误码
    /// </summary>
    public enum SimulationError
    {
        None = 0,
        NullBeatmap,
        NullDirections,
        EmptyBeatmap,
        InvalidDirectionConfig,
    }

    /// <summary>
    /// 模拟器输入，纯数据，无 ScriptableObject 依赖
    /// </summary>
    public class SimulationInput
    {
        public IReadOnlyList<Beat> BeatTimes;
        public Vector3 StartPosition;
        public float InitialSpeed;
        public Vector3 InitialGravity;
        public PlayerDirections InitialDirections;
        public List<IPathEvent> Events;

        public bool IsValid => BeatTimes != null && BeatTimes.Count > 0
                            && InitialDirections != null && InitialDirections.IsValid;
    }

    /// <summary>
    /// 模拟器输出，包含生成的路点、路径段和错误信息
    /// </summary>
    public class SimulationResult
    {
        public bool Success;
        public SimulationError Error;
        public string ErrorMessage;
        public List<Waypoint> Waypoints;
        public List<PathSegment> Segments;

        public static SimulationResult Ok(List<Waypoint> waypoints, List<PathSegment> segments)
        {
            return new SimulationResult
            {
                Success = true,
                Waypoints = waypoints,
                Segments = segments
            };
        }

        public static SimulationResult Fail(SimulationError error, string message)
        {
            return new SimulationResult
            {
                Success = false,
                Error = error,
                ErrorMessage = message
            };
        }
    }
}
