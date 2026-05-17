using UnityEngine;
using DLSample.Gameplay;
using DLSample.Gameplay.Behaviours;
using DLSample.Editor.PathGrapher;
using System.Linq;

namespace DLSample.Editor.PathBuilder
{
    /// <summary>
    /// 路径构建辅助类，根据 PathData 在场景中生成路径网格和提示线。
    /// </summary>
    public static class PathBuilderHelper
    {
        #region Path
        /// <summary>
        /// 根据路径数据在场景中生成路径网格对象。
        /// </summary>
        /// <param name="pathData">路径数据</param>
        /// <param name="type">路径生成方式（连接或断开）</param>
        /// <param name="prefab">路径段预制体</param>
        /// <param name="width">路径宽度</param>
        /// <returns>是否成功生成路径</returns>
        public static bool GeneratePath(PathData pathData, PathGenerateType type, GameObject prefab, float width)
        {
            if (pathData == null || prefab == null) return false;

            var pathRoot = new GameObject("PathContainer").transform;

            foreach (var segment in pathData.generatedSegments)
            {
                if (!segment.IsValid) continue;

                foreach (var section in segment.sections)
                {
                    if (section.isTeleport || section.isJump) continue;

                    if (section.points.Length == 2)
                    {
                        CreatePathElement(section.points[0], section.points[1], section.upDir, width, type, prefab, pathRoot);
                    }
                    else
                    {
                        for (int i = 0; i < section.points.Length - 1; i++)
                        {
                            CreatePathElement(section.points[i], section.points[i + 1], section.upDir, width, type, prefab, pathRoot);
                        }
                    }
                }
            }
            return pathRoot;
        }

        private static void CreatePathElement(Vector3 start, Vector3 end, Vector3 up, float width, PathGenerateType type, GameObject prefab, Transform parent)
        {
            var direction = end - start;
            var distance = direction.magnitude;

            var position = (start + end) * 0.5f;

            var element = Object.Instantiate(prefab, parent);

            var rotation = Quaternion.LookRotation(direction, up);
            element.transform.SetLocalPositionAndRotation(position, rotation);

            var scale = element.transform.localScale;
            scale.x = width;

            switch (type)
            {
                case PathGenerateType.Connected:
                    scale.z = distance + width;
                    break;
                case PathGenerateType.Disconnected:
                    scale.z = distance;
                    break;
            }

            element.transform.localScale = scale;
            element.transform.Translate(-element.transform.up * (0.5f * scale.y + 0.5f), Space.World);

            if (type is PathGenerateType.Disconnected)
            {
                element.transform.Translate(Vector3.back * (width / 2), Space.Self);
            }
        }
        #endregion

        #region Hint
        /// <summary>
        /// 根据路径数据在场景中生成提示线对象（长线段和短线段交替排列）。
        /// </summary>
        /// <param name="pathData">路径数据</param>
        /// <param name="segmentPrefab">提示线段预制体</param>
        /// <param name="boxPrefab">提示盒预制体</param>
        /// <returns>是否成功生成提示线</returns>
        public static bool GenerateHintLine(PathData pathData, GameObject segmentPrefab, GameObject boxPrefab)
        {
            if (pathData == null || boxPrefab == null || segmentPrefab == null) return false;

            var guidanceContainer = new GameObject("HintLines").transform;

            foreach (var segment in pathData.generatedSegments)
            {
                if (!segment.IsValid) continue;

                var wp = segment.startWaypoint;

                var hintBox = Object.Instantiate(boxPrefab, wp.position, wp.rotation, guidanceContainer);
                hintBox.name = $"HintBox_{wp.beatIndex}";

                var component = hintBox.GetComponent<HintBox>();

                if (component)
                {
                    component.StandardTime = (float)wp.time;

                    var lineGroup = new GameObject($"HintLineGroup_{wp.beatIndex}");
                    lineGroup.transform.SetParent(hintBox.transform);
                    lineGroup.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                    component.segments = lineGroup.transform;

                    if (segment.IsSimpleStright || !segment.containedEvents.Any(e => e is SegmentPathEvent))
                    {
                        SpawnSegments(segment.startWaypoint.position,
                                      segment.endWaypoint.position,
                                      segment.sections[0].upDir,
                                      (float)segment.startWaypoint.time,
                                      (float)segment.endWaypoint.time,
                                      lineGroup.transform,
                                      segmentPrefab);
                    }
                    else
                    {
                        foreach (var section in segment.sections)
                        {
                            if (section.isJump || section.isTeleport) continue;

                            if (section.points.Length >= 2)
                            {
                                for (int i = 0; i < section.points.Length - 1; i++)
                                {
                                    SpawnSegments(section.points[i],
                                                  section.points[i + 1],
                                                  section.upDir,
                                                  GetPointTime(section, i),
                                                  GetPointTime(section, i + 1),
                                                  lineGroup.transform,
                                                  segmentPrefab);
                                }
                            }
                        }
                    }
                }
            }

            return guidanceContainer;
        }
        private static void SpawnSegments(Vector3 start, Vector3 end, Vector3 upDir, float startTime, float endTime, Transform parent, GameObject linePrefab)
        {
            var dir = end - start;

            var dist = dir.magnitude;
            if (dist < 0.1f) return;

            var dirNormalized = dir.normalized;

            var offsetStart = 1.0f;
            var totalDistance = dist - offsetStart - 1f;

            if (totalDistance <= Mathf.Epsilon) return;

            var currentPos = start + dirNormalized * offsetStart;
            var remainingDistance = totalDistance;
            var isLongSegment = true;

            while (remainingDistance > 0)
            {
                var currentLength = Mathf.Min(isLongSegment ? 2f : 0.3f, remainingDistance);

                var segmentEnd = currentPos + dirNormalized * currentLength;

                if (linePrefab != null)
                {
                    var line = Object.Instantiate(linePrefab, parent);
                    line.transform.SetPositionAndRotation((currentPos + segmentEnd) / 2, Quaternion.LookRotation(dir, upDir));

                    var scale = linePrefab.transform.localScale;
                    line.transform.localScale = new Vector3(0.15f, scale.y, currentLength);

                    var distanceToMidPoint = Vector3.Distance(start, (currentPos + segmentEnd) / 2f);
                    var timeFactor = dist <= Mathf.Epsilon ? 0f : distanceToMidPoint / dist;
                    var disappearTime = Mathf.Lerp(startTime, endTime, Mathf.Clamp01(timeFactor));

                    if (!line.TryGetComponent<HintLineSegment>(out var hintSegment))
                    {
                        hintSegment = line.AddComponent<HintLineSegment>();
                    }

                    if (hintSegment != null)
                    {
                        hintSegment.Initialize(disappearTime);
                    }
                }

                currentPos = segmentEnd;
                remainingDistance -= currentLength;
                isLongSegment = !isLongSegment;

                if (remainingDistance > 0)
                {
                    var actualGap = Mathf.Min(0.2f, remainingDistance);
                    currentPos += dirNormalized * actualGap;
                    remainingDistance -= actualGap;
                }
            }
        }

        private static float GetPointTime(PathSection section, int pointIndex, float samplingInterval = 0.1f)
        {
            if (section.points == null || section.points.Length == 0) return (float)section.startTime;
            if (section.points.Length == 1) return (float)section.startTime;
            if (pointIndex <= 0) return (float)section.startTime;
            if (pointIndex >= section.points.Length - 1) return (float)section.endTime;
            if (section.points.Length == 2) return Mathf.Lerp((float)section.startTime, (float)section.endTime, pointIndex);

            return (float)section.startTime + pointIndex * samplingInterval;
        }
        #endregion
    }
}
