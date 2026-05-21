using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using DLSample.Shared;

namespace DLSample.Editor.PathGrapher
{
    /// <summary>
    /// 路径模拟器，根据路径图资产和节拍数据生成路径点及路径段
    /// </summary>
    public static class PathSimulator
    {
        private struct TimePointInfo
        {
            public enum TimePointType
            {
                Beat,
                Event,
            }

            public TimePointType type;
            public double time;
            public int beatIndex;
            public IPathEvent evt;
        }

        public static void Simulate(PathGrapherAsset asset, float samplingInterval)
        {
            if (asset.beatMapData == null || asset.initialDirections == null) return;

            SimulationStatus state = new()
            {
                position = asset.startPosition,
                rotation = asset.initialDirections.StartRotation(),
                currentSpeed = asset.initialSpeed,
                currentDirecion = asset.initialDirections.Clone(),
                currentGravity = asset.initialGravity,
                verticalVelocity = Vector3.zero,
                currentTime = 0,
                isJumping = false,
                isTeleport = false,
            };

            state.currentDirecion.Reset();

            asset.pathData.generatedWaypoints.Clear();
            asset.pathData.generatedSegments.Clear();

            var timePoints = CollectTimePoints(asset);

            Waypoint prevWaypoint = CreateWaypoint(state, -1);
            asset.pathData.generatedWaypoints.Add(prevWaypoint);

            List<PathSection> currentSections = new();
            List<IPathEvent> accumulatedEvents = new();

            for (int i = 0; i < timePoints.Count - 1; i++)
            {
                var currTimePoint = timePoints[i];
                var nextTimePoint = timePoints[i + 1];

                double timeStart = currTimePoint.time;
                double timeEnd = nextTimePoint.time;

                float deltaTime = (float)(timeEnd - timeStart);

                List<Vector3> sectionPoints = new()
                {
                    state.position
                };

                if (deltaTime > 0)
                {
                    bool isJump = state.isJumping;

                    if (isJump)
                    {
                        double tempTime = timeStart;
                        while (tempTime + samplingInterval < timeEnd)
                        {
                            state = StepSimulateStatus(state, samplingInterval);
                            sectionPoints.Add(state.position);

                            tempTime += samplingInterval;
                        }

                        float remainingDt = (float)(timeEnd - tempTime); // 剩余的delta
                        if (remainingDt > 0)
                            state = StepSimulateStatus(state, remainingDt);

                        sectionPoints.Add(state.position);
                    }
                    else
                    {
                        state = StepSimulateStatus(state, deltaTime);
                        sectionPoints.Add(state.position);
                    }

                    currentSections.Add(new PathSection
                    {
                        startTime = timeStart,
                        endTime = timeEnd,
                        points = sectionPoints.ToArray(),
                        upDir = state.rotation * Vector3.up,
                        isJump = isJump,
                    });

                    state.isTeleport = false;
                }

                Vector3 tempPos = state.position;

                ApplyEvents(nextTimePoint, ref state);

                // 仅在事件的 GlobalTime（非 EndTime）将事件累积到当前段
                if (nextTimePoint.evt != null && Math.Abs(nextTimePoint.time - nextTimePoint.evt.GlobalTime) < 0.0001)
                    accumulatedEvents.Add(nextTimePoint.evt);

                switch (nextTimePoint.type)
                {
                    case TimePointInfo.TimePointType.Beat:
                        state.rotation = state.currentDirecion.MoveNext();
                        state.currentTime = nextTimePoint.time;
                        Waypoint nextWaypoint = CreateWaypoint(state, nextTimePoint.beatIndex);

                        PathSegment segment = CreateSegment(prevWaypoint, nextWaypoint, accumulatedEvents);
                        accumulatedEvents = new();
                        segment.sections = new List<PathSection>(currentSections);
                        currentSections.Clear();
                        sectionPoints.Clear();

                        asset.pathData.generatedSegments.Add(segment);
                        asset.pathData.generatedWaypoints.Add(nextWaypoint);

                        prevWaypoint = nextWaypoint;
                    break;

                    default:
                        if (state.isTeleport)
                        {
                            currentSections.Add(new PathSection
                            {
                                startTime = nextTimePoint.time,
                                endTime = nextTimePoint.time,
                                points = new Vector3[] { tempPos, state.position },
                                upDir = state.rotation * Vector3.up,
                                isJump = false,
                                isTeleport = true
                            });
                            state.isTeleport = false;
                        }
                    break;
                }
            }
        }

        private static SimulationStatus StepSimulateStatus(SimulationStatus state, float dt)
        {
            Vector3 localMove = Vector3.zero;
            localMove += Vector3.forward * (state.currentSpeed * dt);

            if (state.isJumping)
            {
                Vector3 dropStepLocal = (state.verticalVelocity * dt) + (0.5f * dt * dt * state.currentGravity);
                localMove += dropStepLocal;

                state.verticalVelocity += state.currentGravity * dt;
            }

            state.position += state.rotation * localMove;

            return state;
        }

        private static List<TimePointInfo> CollectTimePoints(PathGrapherAsset asset)
        {
            var points = new List<TimePointInfo>();

            for (int i = 0; i < asset.beatMapData.Beats.Count; i++)
            {
                points.Add(new TimePointInfo
                {
                    type = TimePointInfo.TimePointType.Beat,
                    time = asset.beatMapData.Beats[i].TimeSecond,
                    beatIndex = i
                });
            }

            foreach (var ev in asset.pathData.globalEvents)
            {
                if (ev.IsWaypointBoundary)
                {
                    points.Add(
                        new TimePointInfo
                        {
                            type = TimePointInfo.TimePointType.Beat,
                            time = ev.GlobalTime,
                            evt = ev
                        });
                }
                else
                {
                    points.Add(
                        new TimePointInfo
                        {
                            type = TimePointInfo.TimePointType.Event,
                            time = ev.GlobalTime,
                            evt = ev
                        });
                }

                if (ev is SegmentPathEvent segEv && Math.Abs(segEv.EndTime - ev.GlobalTime) > 0.0001)
                {
                    points.Add(
                        new TimePointInfo
                        {
                            type = TimePointInfo.TimePointType.Event,
                            time = segEv.EndTime,
                            evt = ev
                        });
                }
            }

            return points.OrderBy(p => p.time).ToList();
        }

        private static void ApplyEvents(TimePointInfo timePoint, ref SimulationStatus state)
        {
            timePoint.evt?.ApplyTo(ref state, timePoint.time);
        }

        private static Waypoint CreateWaypoint(SimulationStatus state, int index)
        {
            return new Waypoint
            {
                position = state.position,
                rotation = state.rotation,
                time = state.currentTime,
                beatIndex = index
            };
        }

        private static PathSegment CreateSegment(Waypoint start, Waypoint end, List<IPathEvent> events)
        {
            return new PathSegment
            {
                startWaypoint = start,
                endWaypoint = end,
                containedEvents = events
            };
        }

    }
}