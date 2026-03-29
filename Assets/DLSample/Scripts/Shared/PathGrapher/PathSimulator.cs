using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using DLSample.Shared;

namespace DLSample.Editor.PathGrapher
{
    public static class PathSimulator
    {
        private struct SimulationStatus
        {
            public Vector3 position;
            public Quaternion rotation;
            public float currentSpeed;

            public PlayerDirections currentDirecion;

            public Vector3 currentGravity;
            public Vector3 verticalVelocity;
            public double currentTime;

            public bool isJumping;
            public bool isTeleport;
        }
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
                currentDirecion = asset.initialDirections,
                currentGravity = asset.initialGravity,
                verticalVelocity = Vector3.zero,
                currentTime = 0,
                isJumping = false,
                isTeleport = false,
            };

            ResetDirections(asset.initialDirections);

            asset.pathData.generatedWaypoints.Clear();
            asset.pathData.generatedSegments.Clear();

            var timePoints = CollectTimePoints(asset);

            Waypoint prevWaypoint = CreateWaypoint(state, -1);
            asset.pathData.generatedWaypoints.Add(prevWaypoint);

            List<PathSection> currentSections = new();

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

                        float remainingDt = (float)(timeEnd - tempTime);//Ê£ÓàµÄ¶Î
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

                switch (nextTimePoint.type)
                {
                    case TimePointInfo.TimePointType.Beat:
                        state.rotation = state.currentDirecion.MoveNext();
                        state.currentTime = nextTimePoint.time;
                        Waypoint nextWaypoint = CreateWaypoint(state, nextTimePoint.beatIndex);

                        PathSegment segment = CreateSegment(prevWaypoint, nextWaypoint, asset);
                        segment.sections = new List<PathSection>(currentSections);
                        currentSections.Clear();
                        sectionPoints.Clear();

                        asset.pathData.generatedSegments.Add(segment);
                        asset.pathData.generatedWaypoints.Add(nextWaypoint);

                        currentSections.Clear();
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
                if (ev is ForceTurnEvent)
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

                if (ev is SegmentPathEvent segEv)
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
            if (timePoint.type is not TimePointInfo.TimePointType.Event) return;

            switch(timePoint.evt)
            {
                case SpeedChangeEvent s:
                    state.currentSpeed = s.newSpeed;
                    break;

                case GravityChangeEvent g:
                    state.currentGravity = g.newGravity;
                    break;

                case DirectionChangeEvent d:
                    state.currentDirecion = d.newDirections;
                    state.currentDirecion.Reset();
                    break;

                case TeleportEvent t:
                    if (DoubleEquals(timePoint.time, t.StartTime))
                    {
                        state.position = t.targetPosition;

                        state.isTeleport = true;
                    }
                    break;

                case JumpEvent j:
                    if (DoubleEquals(timePoint.time, j.StartTime))
                    {
                        state.isJumping = true;
                        state.verticalVelocity = state.rotation * j.velocity;
                    }
                    else if (DoubleEquals(timePoint.time, j.EndTime))
                    {
                        state.isJumping = false;
                        state.verticalVelocity = Vector3.zero;
                    }
                    break;
                default:
                    break;
            }
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

        private static PathSegment CreateSegment(Waypoint start, Waypoint end, PathGrapherAsset asset)
        {
            var segment = new PathSegment
            {
                startWaypoint = start,
                endWaypoint = end,
                containedEvents = GetEventsOfInterval(asset, start.time, end.time)
            };
            return segment;
        }

        private static List<IPathEvent> GetEventsOfInterval(PathGrapherAsset asset, double start, double end)
        {
            return asset.pathData.globalEvents
                    .Where(e => e.GlobalTime >= start && e.GlobalTime < end)
                    .ToList();
        }

        private static void ResetDirections(PlayerDirections directions)
        {
            directions.Reset();
        }
        private static bool DoubleEquals(double a, double b, double epsilon = 0.0001f)
        {
            return Math.Abs(a - b) < epsilon;
        }
    }
}