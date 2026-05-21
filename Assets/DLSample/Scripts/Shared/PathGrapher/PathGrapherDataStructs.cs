using System;
using System.Collections.Generic;
using UnityEngine;
using DLSample.Shared;

namespace DLSample.Editor.PathGrapher
{
    /// <summary>
    /// 路径点数据结构
    /// </summary>
    [Serializable]
    public struct Waypoint
    {
        public Vector3 position;
        public Quaternion rotation;
        public double time;

        public int beatIndex;
    }

    /// <summary>
    /// 路径段数据结构，连接两个路径点
    /// </summary>
    [Serializable]
    public struct PathSegment
    {
        public Waypoint startWaypoint;
        public Waypoint endWaypoint;

        [SerializeReference]
        public List<IPathEvent> containedEvents;
        public List<PathSection> sections;

        public readonly bool IsValid => sections != null && sections.Count > 0 && sections[0].points.Length >= 2;
        public readonly bool IsSimpleStright => IsValid && sections.Count == 1 && sections[0].points.Length == 2 && !sections[0].isJump && !sections[0].isTeleport;
    }

    /// <summary>
    /// 路径区间数据结构，表示路径段中的一段连续采样区间
    /// </summary>
    [Serializable]
    public struct PathSection
    {
        public double startTime;
        public double endTime;

        public Vector3[] points;
        public Vector3 upDir;
        public bool isJump;
        public bool isTeleport;
    }

    /// <summary>
    /// 模拟状态，在路径模拟过程中逐时间点推进
    /// </summary>
    public struct SimulationStatus
    {
        public Vector3 position;
        public Quaternion rotation;
        public float currentSpeed;

        public PlayerDirections currentDirecion;

        public Vector3 currentGravity;
        public Vector3 verticalVelocity;
        public double currentTime;

        public bool isJumping;
        public bool isTeleporting;
        public bool isTeleport;
    }
}
