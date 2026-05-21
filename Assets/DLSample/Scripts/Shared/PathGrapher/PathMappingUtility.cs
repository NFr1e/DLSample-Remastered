#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
                            var (timeAtP1, timeAtP2) = GetSectionPointTimeRange(section, i, samplingInterval);

                            // 4. 插值得到最终精确时间
                            bestTime = timeAtP1 + (timeAtP2 - timeAtP1) * tFactor;
                        }
                    }
                }
            }
            return bestTime;
        }

        /// <summary>
        /// 在给定 Section 中获取第 pointIndex 个相邻采样点对所对应的时间范围。
        /// 2 点直线段返回 (startTime, endTime)，多点曲线段按采样间隔计算并强制末点对齐 endTime。
        /// </summary>
        private static (double timeAtP1, double timeAtP2) GetSectionPointTimeRange(PathSection section, int pointIndex, float samplingInterval)
        {
            if (section.points.Length == 2)
                return (section.startTime, section.endTime);

            double p1 = section.startTime + (pointIndex * samplingInterval);
            double p2 = (pointIndex == section.points.Length - 2)
                        ? section.endTime
                        : section.startTime + ((pointIndex + 1) * samplingInterval);
            return (p1, p2);
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
            var waypoints = pathData.generatedWaypoints;
            if (waypoints.Count == 0) return origin.transform.position;

            if (TryGetSegmentAtTime(time, pathData, out var segment))
            {
                if (!segment.IsValid)
                    return origin.transform.TransformPoint(segment.startWaypoint.position);

                foreach (var section in segment.sections)
                {
                    if (time >= section.startTime && time <= section.endTime)
                    {
                        if (section.points == null || section.points.Length < 2) continue;

                        Vector3 localPos;
                        double duration = section.endTime - section.startTime;

                        if (section.points.Length == 2)
                        {
                            float t = duration > 0 ? (float)((time - section.startTime) / duration) : 0;
                            localPos = Vector3.Lerp(section.points[0], section.points[1], t);
                        }
                        else
                        {
                            double relativeTime = time - section.startTime;
                            int index = Mathf.FloorToInt((float)(relativeTime / samplingInterval));
                            index = Mathf.Clamp(index, 0, section.points.Length - 2);

                            var (timeAtP1, timeAtP2) = GetSectionPointTimeRange(section, index, samplingInterval);
                            double subDuration = timeAtP2 - timeAtP1;
                            float factor = subDuration > 0 ? (float)((time - timeAtP1) / subDuration) : 0;

                            localPos = Vector3.Lerp(section.points[index], section.points[index + 1], Mathf.Clamp01(factor));
                        }

                        return origin.transform.TransformPoint(localPos);
                    }
                }
            }

            // 时间超出范围，返回最近的路点位置
            if (waypoints.Count > 0)
            {
                var wp = time < waypoints[0].time ? waypoints[0] : waypoints[^1];
                return origin.transform.TransformPoint(wp.position);
            }

            return origin.transform.position;
        }

        /// <summary>
        /// 二分查找包含指定时间的路径段
        /// </summary>
        private static bool TryGetSegmentAtTime(double time, PathData pathData, out PathSegment result)
        {
            var segs = pathData.generatedSegments;
            int lo = 0, hi = segs.Count - 1;
            while (lo <= hi)
            {
                int mid = lo + (hi - lo) / 2;
                var seg = segs[mid];
                if (time > seg.endWaypoint.time)
                    lo = mid + 1;
                else if (time < seg.startWaypoint.time)
                    hi = mid - 1;
                else
                {
                    result = seg;
                    return true;
                }
            }
            result = default;
            return false;
        }

        /// <summary>
        /// 根据时间获取路径段
        /// </summary>
        public static PathSegment GetSegmentAtTime(double time, PathData pathData)
        {
            TryGetSegmentAtTime(time, pathData, out var seg);
            return seg;
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

            const float BROAD_PHASE_THRESHOLD = 80f;
            const float NARROW_PHASE_THRESHOLD = 40f;

            // 1. Broad Phase: 按点对计算线段距离，而非仅检查端点
            List<PathSegment> candidates = new();

            foreach (var segment in pathData.generatedSegments)
            {
                if (!segment.IsValid) continue;

                bool isCandidate = false;
                foreach (var section in segment.sections)
                {
                    if (section.points.Length < 2) continue;

                    for (int i = 0; i < section.points.Length - 1; i++)
                    {
                        Vector3 w1 = localToWorld.MultiplyPoint(section.points[i]);
                        Vector3 w2 = localToWorld.MultiplyPoint(section.points[i + 1]);
                        Vector2 s1 = HandleUtility.WorldToGUIPoint(w1);
                        Vector2 s2 = HandleUtility.WorldToGUIPoint(w2);

                        if (DistancePointToSegmentSq(mousePos, s1, s2) < BROAD_PHASE_THRESHOLD * BROAD_PHASE_THRESHOLD)
                        {
                            isCandidate = true;
                            break;
                        }
                    }
                    if (isCandidate) break;
                }

                if (isCandidate) candidates.Add(segment);
            }

            // 2. Narrow Phase: 基于 3D 射线-线段求交，避免透视投影非线性误差
            Ray mouseRay = HandleUtility.GUIPointToWorldRay(mousePos);

            foreach (var segment in candidates)
            {
                foreach (var section in segment.sections)
                {
                    if (section.points.Length < 2) continue;

                    for (int i = 0; i < section.points.Length - 1; i++)
                    {
                        Vector3 p1 = localToWorld.MultiplyPoint(section.points[i]);
                        Vector3 p2 = localToWorld.MultiplyPoint(section.points[i + 1]);

                        var (closestWorldPos, t3D) = ClosestPointOnSegmentToRay(p1, p2, mouseRay);

                        // 将 3D 最近点投影回屏幕以判断距离门限
                        Vector2 screenPt = HandleUtility.WorldToGUIPoint(closestWorldPos);
                        float screenDist = Vector2.Distance(mousePos, screenPt);

                        if (screenDist < minScreenDist && screenDist < NARROW_PHASE_THRESHOLD)
                        {
                            minScreenDist = screenDist;
                            bestWorldPos = closestWorldPos;

                            var (timeAtP1, timeAtP2) = GetSectionPointTimeRange(section, i, samplingInterval);
                            bestTime = timeAtP1 + (timeAtP2 - timeAtP1) * (double)t3D;
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
        /// 计算屏幕空间点到线段的平方距离（非无限直线）
        /// </summary>
        private static float DistancePointToSegmentSq(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            Vector2 ap = p - a;
            float abSqMag = ab.sqrMagnitude;
            if (abSqMag < float.Epsilon) return ap.sqrMagnitude;
            float t = Mathf.Clamp01(Vector2.Dot(ap, ab) / abSqMag);
            Vector2 closest = a + t * ab;
            return (p - closest).sqrMagnitude;
        }

        /// <summary>
        /// 计算 3D 线段到鼠标射线的最近点及其在线段上的投影比例
        /// </summary>
        private static (Vector3 closestPoint, float t) ClosestPointOnSegmentToRay(Vector3 a, Vector3 b, Ray ray)
        {
            Vector3 ab = b - a;
            Vector3 ao = ray.origin - a;
            Vector3 d = ray.direction;

            float dd = Vector3.Dot(d, d);
            float abSq = Vector3.Dot(ab, ab);
            float dDotAb = Vector3.Dot(d, ab);

            float det = dd * abSq - dDotAb * dDotAb;

            // 平行或近平行时退化为点到线段的投影
            if (det < 1e-6f)
            {
                float tmSeg = Mathf.Clamp01(Vector3.Dot(-ao, ab) / Mathf.Max(abSq, float.Epsilon));
                return (a + tmSeg * ab, tmSeg);
            }

            float aod = Vector3.Dot(ao, d);
            float aoAb = Vector3.Dot(ao, ab);

            float tRay = (-aod * abSq + dDotAb * aoAb) / det;
            float tSeg = (dd * aoAb - dDotAb * aod) / det;

            tSeg = Mathf.Clamp01(tSeg);

            // 沿射线推进 tRay 到达最近点
            Vector3 rayPoint = ray.origin + tRay * d;
            return (a + tSeg * ab, tSeg);
        }

        /// <summary>
        /// 根据时间获取路径上的旋转角度
        /// </summary>
        /// <param name="time">目标时间</param>
        /// <param name="data">路径数据</param>
        /// <returns>该时间点的旋转量</returns>
        public static Quaternion GetRotationAtTime(double time, PathData data)
        {
            var wps = data.generatedWaypoints;
            if (wps.Count == 0) return Quaternion.identity;
            if (wps.Count == 1) return wps[0].rotation;

            // 二分查找：找到满足 wps[i].time <= time <= wps[i+1].time 的 i
            int lo = 0, hi = wps.Count - 2;
            while (lo <= hi)
            {
                int mid = lo + (hi - lo) / 2;
                if (time > wps[mid + 1].time)
                    lo = mid + 1;
                else if (time < wps[mid].time)
                    hi = mid - 1;
                else
                    return wps[mid].rotation;
            }
            return time < wps[0].time ? wps[0].rotation : wps[wps.Count - 2].rotation;
        }
    }
}
#endif