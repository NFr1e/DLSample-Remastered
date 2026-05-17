#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// AI辅助生成代码，仅供参考

namespace DLSample.Editor.PathGrapher
{
    /// <summary>
    /// 路径映射工具类，提供时间、世界坐标、屏幕坐标之间的转换功能
    /// </summary>
    public static class PathMappingUtility
    {
        /// <summary>
        /// 在世界空间中找到路径上距离给定位置最近的时间点
        /// </summary>
        /// <param name="worldPos">世界空间中的位置</param>
        /// <param name="pathData">路径数据</param>
        /// <param name="origin">原点的Transform，用于世界坐标与本地坐标的转换</param>
        /// <param name="samplingInterval">采样间隔</param>
        /// <returns>最近的时间点</returns>
        public static double FindNearestTimeOnPath(Vector3 worldPos, PathData pathData, Transform origin, float samplingInterval = 0.1f)
        {
            if (pathData.generatedSegments.Count == 0) return 0;

            var segments = pathData.generatedSegments;

            Matrix4x4 worldToLocal = origin.worldToLocalMatrix;
            Vector3 localPos = worldToLocal.MultiplyPoint(worldPos);

            double bestTime = 0;
            float minSqrDist = float.MaxValue;

            foreach (var segment in segments)
            {
                if (!segment.IsValid) continue;

                foreach (var section in segment.sections)
                {
                    if (section.points == null || section.points.Length < 2) continue;

                    for (int i = 0; i < section.points.Length - 1; i++)
                    {
                        Vector3 p1 = section.points[i];
                        Vector3 p2 = section.points[i + 1];

                        // 1. 投影点到线段 p1-p2 上
                        Vector3 nearestPointOnLine = ClosestPointOnSegment(p1, p2, localPos);
                        float sqrDist = (localPos - nearestPointOnLine).sqrMagnitude;

                        if (sqrDist < minSqrDist)
                        {
                            minSqrDist = sqrDist;

                            // 2. 计算投影点在线段中的比例 (0-1)
                            float tFactor = GetProjectionFactor(p1, p2, nearestPointOnLine);

                            // 3. 获取最小线段 [p1, p2] 对应的开始和结束时间
                            double timeAtP1, timeAtP2;

                            if (section.points.Length == 2)
                            {
                                // 直线段：直接映射 Section 的时间范围
                                timeAtP1 = section.startTime;
                                timeAtP2 = section.endTime;
                            }
                            else
                            {
                                // 曲线采样段：根据采样率计算时间
                                timeAtP1 = section.startTime + (i * samplingInterval);

                                // 最后一个采样点的时间强制对齐到 Section 的 endTime
                                if (i == section.points.Length - 2)
                                    timeAtP2 = section.endTime;
                                else
                                    timeAtP2 = section.startTime + ((i + 1) * samplingInterval);
                            }

                            // 4. 插值得到最终精确时间
                            bestTime = timeAtP1 + (timeAtP2 - timeAtP1) * tFactor;
                        }
                    }
                }
            }
            return bestTime;
        }

        /// <summary>
        /// 计算点在线段上的投影点
        /// </summary>
        private static Vector3 ClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 p)
        {
            Vector3 ap = p - a;
            Vector3 ab = b - a;
            float magnitudeAB = ab.sqrMagnitude;
            if (magnitudeAB == 0) return a;
            float distance = Vector3.Dot(ap, ab) / magnitudeAB;
            return (distance < 0) ? a : (distance > 1) ? b : a + ab * distance;
        }

        /// <summary>
        /// 获取投影点在线段上的比例 (0-1)
        /// </summary>
        private static float GetProjectionFactor(Vector3 a, Vector3 b, Vector3 p)
        {
            Vector3 ab = b - a;
            Vector3 ap = p - a;
            float magSq = ab.sqrMagnitude;
            if (magSq == 0) return 0;
            return Mathf.Clamp01(Vector3.Dot(ap, ab) / magSq);
        }

        /// <summary>
        /// 根据时间获取路径上的世界空间位置
        /// </summary>
        /// <param name="time">目标时间</param>
        /// <param name="pathData">路径数据</param>
        /// <param name="origin">原点的Transform</param>
        /// <param name="samplingInterval">采样间隔</param>
        /// <returns>世界空间中的位置</returns>
        public static Vector3 GetWorldPosFromTime(double time, PathData pathData, Transform origin, float samplingInterval = 0.1f)
        {
            if (pathData.generatedSegments.Count == 0) return origin.transform.position;

            var segments = pathData.generatedSegments;
            var waypoints = pathData.generatedWaypoints;

            foreach (var segment in segments)
            {
                // 使用微小容差处理边界情况
                if (time >= segment.startWaypoint.time && time <= segment.endWaypoint.time)
                {
                    if (!segment.IsValid)
                        return origin.transform.TransformPoint(segment.startWaypoint.position);

                    // 定位 PathSection
                    foreach (var section in segment.sections)
                    {
                        if (time >= section.startTime && time <= section.endTime)
                        {
                            if (section.points == null || section.points.Length < 2) continue;

                            Vector3 localPos;
                            double duration = section.endTime - section.startTime;

                            // 根据采样点数选择插值逻辑
                            if (section.points.Length == 2)
                            {
                                // --- 直线/坍缩片段 ---
                                float t = duration > 0 ? (float)((time - section.startTime) / duration) : 0;
                                localPos = Vector3.Lerp(section.points[0], section.points[1], t);
                            }
                            else
                            {
                                // --- 曲线采样片段 ---
                                double relativeTime = time - section.startTime;

                                // 定位采样区间
                                int index = Mathf.FloorToInt((float)(relativeTime / samplingInterval));
                                index = Mathf.Clamp(index, 0, section.points.Length - 2);

                                // 计算小区间的插值参数
                                double tAtP1 = index * samplingInterval;
                                double tAtP2 = (index == section.points.Length - 2)
                                               ? duration
                                               : (index + 1) * samplingInterval;

                                double subDuration = tAtP2 - tAtP1;
                                float factor = subDuration > 0 ? (float)((relativeTime - tAtP1) / subDuration) : 0;

                                localPos = Vector3.Lerp(section.points[index], section.points[index + 1], Mathf.Clamp01(factor));
                            }

                            return origin.transform.TransformPoint(localPos);
                        }
                    }
                }
            }

            // 如果时间超出范围，返回最近的路点位置
            if (waypoints.Count > 0)
            {
                var wp = time < waypoints[0].time ? waypoints[0] : waypoints[^1];
                return origin.transform.TransformPoint(wp.position);
            }

            return origin.transform.position;
        }

        /// <summary>
        /// 根据时间获取路径段
        /// </summary>
        /// <param name="time">目标时间</param>
        /// <param name="pathData">路径数据</param>
        /// <returns>包含该时间的路径段</returns>
        public static PathSegment GetSegmentAtTime(double time, PathData pathData)
        {
            foreach (var seg in pathData.generatedSegments)
            {
                if (time >= seg.startWaypoint.time && time <= seg.endWaypoint.time)
                    return seg;
            }
            return default;
        }

        /// <summary>
        /// 通过鼠标位置找到路径上最近的点
        /// </summary>
        /// <param name="mousePos">鼠标屏幕坐标</param>
        /// <param name="pathData">路径数据</param>
        /// <param name="origin">原点的Transform</param>
        /// <param name="samplingInterval">采样间隔</param>
        /// <returns>包含世界坐标和时间的元组</returns>
        public static (Vector3 worldPos, double time) FindNearestPointByMouse(Vector2 mousePos, PathData pathData, Transform origin, float samplingInterval = 0.1f)
        {
            if (pathData == null || pathData.generatedSegments.Count == 0)
                return (Vector3.zero, 0);

            Matrix4x4 localToWorld = origin.localToWorldMatrix;

            Vector3 bestWorldPos = Vector3.zero;
            double bestTime = 0;
            float minScreenDist = float.MaxValue;

            // 距离阈值（屏幕坐标）
            const float BROAD_PHASE_THRESHOLD = 80f;
            const float NARROW_PHASE_THRESHOLD = 20f;

            // 1. Broad Phase: 粗略筛选 Segment
            List<PathSegment> candidates = new();

            foreach (var segment in pathData.generatedSegments)
            {
                if (!segment.IsValid) continue;

                bool isCandidate = false;
                // 遍历所有 Section 的所有点进行粗筛
                foreach (var section in segment.sections)
                {
                    for (int i = 0; i < section.points.Length; i++)
                    {
                        Vector3 worldP = localToWorld.MultiplyPoint(section.points[i]);
                        Vector2 screenP = HandleUtility.WorldToGUIPoint(worldP);
                        if (Vector2.Distance(screenP, mousePos) < BROAD_PHASE_THRESHOLD)
                        {
                            isCandidate = true;
                            break;
                        }
                    }
                    if (isCandidate) break;
                }

                if (isCandidate) candidates.Add(segment);
            }

            // 2. Narrow Phase: 精确投影
            foreach (var segment in candidates)
            {
                foreach (var section in segment.sections)
                {
                    if (section.points.Length < 2) continue;

                    for (int i = 0; i < section.points.Length - 1; i++)
                    {
                        Vector3 p1 = localToWorld.MultiplyPoint(section.points[i]);
                        Vector3 p2 = localToWorld.MultiplyPoint(section.points[i + 1]);

                        Vector2 s1 = HandleUtility.WorldToGUIPoint(p1);
                        Vector2 s2 = HandleUtility.WorldToGUIPoint(p2);

                        // 计算屏幕空间中鼠标到线段的距离
                        float screenDist = HandleUtility.DistancePointLine(mousePos, s1, s2);

                        if (screenDist < minScreenDist && screenDist < NARROW_PHASE_THRESHOLD)
                        {
                            minScreenDist = screenDist;

                            // 计算投影比例 (0-1)
                            float t2D = GetProjectionFactor(s1, s2, mousePos);
                            bestWorldPos = Vector3.Lerp(p1, p2, t2D);

                            // 获取小段的时间范围
                            double timeAtP1, timeAtP2;
                            if (section.points.Length == 2)
                            {
                                // 直线段
                                timeAtP1 = section.startTime;
                                timeAtP2 = section.endTime;
                            }
                            else
                            {
                                // 曲线采样段
                                timeAtP1 = section.startTime + (i * samplingInterval);
                                timeAtP2 = (i == section.points.Length - 2)
                                           ? section.endTime
                                           : section.startTime + ((i + 1) * samplingInterval);
                            }

                            // 最终插值得到时间
                            bestTime = timeAtP1 + (timeAtP2 - timeAtP1) * (double)t2D;
                        }
                    }
                }
            }

            if (minScreenDist < NARROW_PHASE_THRESHOLD)
            {
                return (bestWorldPos, bestTime);
            }

            return (Vector3.zero, 0);
        }

        /// <summary>
        /// 根据时间获取路径上的旋转角度
        /// </summary>
        /// <param name="time">目标时间</param>
        /// <param name="data">路径数据</param>
        /// <returns>该时间点的旋转量</returns>
        public static Quaternion GetRotationAtTime(double time, PathData data)
        {
            for (int i = 0; i < data.generatedWaypoints.Count - 1; i++)
            {
                if (time >= data.generatedWaypoints[i].time && time <= data.generatedWaypoints[i + 1].time)
                {
                    return data.generatedWaypoints[i].rotation;
                }
            }
            return data.generatedWaypoints.Count > 0 ? data.generatedWaypoints[0].rotation : Quaternion.identity;
        }
    }
}
#endif