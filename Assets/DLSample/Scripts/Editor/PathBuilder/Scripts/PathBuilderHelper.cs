using UnityEngine;
using DLSample.Gameplay.Behaviours;
using DLSample.Editor.PathGrapher;
using System.Linq;

namespace DLSample.Editor.PathBuilder
{
    public static class PathBuilderHelper
    {
        #region Path
        public static bool GeneratePath(PathData pathData, PathGenerateType type, GameObject prefab, float width)
        {
            if (pathData == null || prefab == null) return false;

            Transform pathRoot = new GameObject("PathContainer").transform;

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
            Vector3 direction = end - start;
            float distance = direction.magnitude;

            Vector3 position = (start + end) * 0.5f;

            GameObject element = Object.Instantiate(prefab, parent);

            Quaternion rotation = Quaternion.LookRotation(direction, up);
            element.transform.SetLocalPositionAndRotation(position, rotation);

            Vector3 scale = element.transform.localScale;
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
        public static bool GenerateHintLine(PathData pathData, GameObject segmentPrefab, GameObject boxPrefab)
        {
            if (pathData == null || boxPrefab == null || segmentPrefab == null) return false;

            Transform guidanceContainer = new GameObject("HintLines").transform;

            foreach (var segment in pathData.generatedSegments)
            {
                if (!segment.IsValid) continue;

                var wp = segment.startWaypoint;

                GameObject hintBox = Object.Instantiate(boxPrefab, wp.position, wp.rotation, guidanceContainer);
                hintBox.name = $"HintBox_{wp.beatIndex}";

                var component = hintBox.GetComponent<HintBox>();

                if (component)
                {
                    component.StandardTime = (float)wp.time;

                    GameObject lineGroup = new($"HintLineGroup_{wp.beatIndex}");
                    lineGroup.transform.SetParent(hintBox.transform);
                    lineGroup.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                    component.segments = lineGroup.transform;

                    if (segment.IsSimpleStright || !segment.containedEvents.Any(e => e is SegmentPathEvent))
                    {
                        SpawnSegments(segment.startWaypoint.position, segment.endWaypoint.position, segment.sections[0].upDir, lineGroup.transform, segmentPrefab);
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
                                    SpawnSegments(section.points[i], section.points[^1], section.upDir, lineGroup.transform, segmentPrefab);
                                }
                            }
                        }
                    }
                }
            }

            return guidanceContainer;
        }
        private static void SpawnSegments(Vector3 start, Vector3 end, Vector3 upDir, Transform parent, GameObject linePrefab)
        {
            Vector3 dir = end - start;

            float dist = dir.magnitude;
            if (dist < 0.1f) return;

            Vector3 dirNormalized = dir.normalized;

            float offsetStart = 1.0f;
            float totalDistance = dist - offsetStart - 1f;

            if (totalDistance <= Mathf.Epsilon) return;

            Vector3 currentPos = start + dirNormalized * offsetStart;
            float remainingDistance = totalDistance;
            bool isLongSegment = true;

            while (remainingDistance > 0)
            {
                float currentLength = isLongSegment ? 2 : 0.3f;
                currentLength = Mathf.Min(currentLength, remainingDistance);

                Vector3 segmentEnd = currentPos + dirNormalized * currentLength;

                if (linePrefab != null)
                {
                    GameObject line = Object.Instantiate(linePrefab, parent);
                    line.transform.SetPositionAndRotation((currentPos + segmentEnd) / 2, Quaternion.LookRotation(dir, upDir));

                    Vector3 scale = linePrefab.transform.localScale;
                    line.transform.localScale = new Vector3(0.15f, scale.y, currentLength);
                }

                currentPos = segmentEnd;
                remainingDistance -= currentLength;
                isLongSegment = !isLongSegment;

                if (remainingDistance > 0)
                {
                    float actualGap = Mathf.Min(0.2f, remainingDistance);
                    currentPos += dirNormalized * actualGap;
                    remainingDistance -= actualGap;
                }
            }
        }
        #endregion
    }
}
