using UnityEditor;
using UnityEngine;

namespace DLSample.Editor.PathGrapher
{
    /// <summary>
    /// 路径绘制器，负责在 Scene 视图中渲染路径段、路径点以及事件标记。
    /// </summary>
    public class PathGrapherDrawer
    {
        #region Configs
        private static readonly Color SPEED_CHANGE_EVT_COLOR = Color.green;
        private static readonly Color GRAVITY_CHANGE_EVT_COLOR = Color.magenta;
        private static readonly Color DIRECTION_CHANGE_EVT_COLOR = Color.blue;
        private static readonly Color FORCE_TURN_EVT_COLOR = Color.gray;
        private static readonly Color JUMP_EVT_COLOR = Color.yellow;
        private static readonly Color TP_EVT_COLOR = Color.red;
        #endregion

        private GUIStyle _labelStyle = new();
        private Texture2D _labelBgTex;
        private Camera _sceneCamera;

        /// <summary>
        /// 在 Scene 视图中绘制完整的路径，包括路径段、路径点和事件标记。
        /// </summary>
        /// <param name="pathData">路径数据</param>
        /// <param name="origin">路径的 Transform 原点</param>
        /// <param name="profile">路径绘制配置</param>
        public void DrawPath(PathData pathData, Transform origin, PathGrapherProfile profile = default)
        {
            var localToWorld = origin.localToWorldMatrix;

            if (profile.zTest)
            {
                var prevZTest = Handles.zTest;

                Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                Draw();
                Handles.zTest = prevZTest;
                return;
            }

            Draw();

            void Draw()
            {
                DrawSegments(pathData, localToWorld, profile);
                DrawWaypoints(pathData, localToWorld, profile);
                DrawEventHandles(pathData, origin, profile);
            }
        }

        /// <summary>
        /// 释放绘制器持有的资源（相机引用、标签样式和背景纹理）。
        /// </summary>
        public void Dispose()
        {
            _sceneCamera = null;
            _labelStyle = null;

            GameObject.DestroyImmediate(_labelBgTex);
        }

        private void DrawSegments(PathData pathData, Matrix4x4 matrix, PathGrapherProfile profile)
        {
            foreach (var segment in pathData.generatedSegments)
            {
                DrawSegment(segment, matrix, profile);
            }
        }

        private void DrawSegment(PathSegment segment, Matrix4x4 matrix, PathGrapherProfile profile)
        {
            if (!segment.IsValid) return;

            var segmentStartPos = matrix.MultiplyPoint(segment.startWaypoint.position);
            if (!IsWithinDrawDistance(profile.pathDrawDistance, segmentStartPos)) return;

            if (segment.IsSimpleStright)
            {
                DrawStraightLine();
                return;
            }

            DrawSegmentDetailed();

            void DrawStraightLine()
            {
                var endPos = matrix.MultiplyPoint(segment.endWaypoint.position);

                Handles.color = profile.pathColor;
                Handles.DrawLine(segmentStartPos, endPos);
            }
            void DrawSegmentDetailed()
            {
                Handles.color = profile.pathColor;
                for (int i = 0; i < segment.sections.Count; i++)
                {
                    if (segment.sections[i].isTeleport)
                    {
                        DrawTeleport(segment.sections[i]);
                    }
                    else
                    {
                        DrawCurve(segment.sections[i]);
                    }
                }

                void DrawCurve(PathSection section)
                {
                    var len = section.points.Length;
                    var points = new Vector3[len];

                    for (int i = 0; i < len; i++)
                        points[i] = matrix.MultiplyPoint(section.points[i]);

                    Handles.color = section.isJump ? Color.red : profile.pathColor;

                    Handles.DrawAAPolyLine(4f, points);
                }
                void DrawTeleport(PathSection section)
                {
                    var len = section.points.Length;
                    var points = new Vector3[len];

                    for (int i = 0; i < len; i++)
                        points[i] = matrix.MultiplyPoint(section.points[i]);

                    Handles.color = Color.red;
                    Handles.DrawDottedLine(points[0], points[^1], 4f);
                }
            }
        }

        private void DrawWaypoints(PathData pathData, Matrix4x4 matrix, PathGrapherProfile profile)
        {
            Handles.color = profile.pathColor;

            foreach (var wp in pathData.generatedWaypoints)
            {
                var worldPos = matrix.MultiplyPoint(wp.position);

                if (!IsWithinDrawDistance(profile.pathDrawDistance, worldPos)) continue;

                var size = 0.5f;
                Handles.CubeHandleCap(0, worldPos, wp.rotation, size, EventType.Repaint);

                if (!IsWithinDrawDistance(profile.labelDrawDistance, worldPos)) continue;
                if (profile.drawWaypointLabel)
                {
                    var style = GetLabelStyle(profile);
                    Handles.Label(worldPos + 2 * size * Vector3.up, $"Beat: {wp.beatIndex} Time: {wp.time:F2}s", style);
                }
            }
        }

        private void DrawEventHandles(PathData pathData, Transform origin, PathGrapherProfile profile)
        {
            if (!profile.drawEvents) return;

            foreach (var ev in pathData.globalEvents)
            {
                var worldPos = PathMappingUtility.GetWorldPosFromTime(ev.GlobalTime, pathData, origin, profile.samplingInterval);

                if (!IsWithinDrawDistance(profile.pathDrawDistance, worldPos)) continue;

                var size = 1f;
                Handles.color = GetEventColor(ev);
                Handles.CubeHandleCap(0, worldPos, Quaternion.identity, size, EventType.Repaint);

                if (ev is SegmentPathEvent segEv)
                {
                    var endWorldPos = PathMappingUtility.GetWorldPosFromTime(segEv.EndTime, pathData, origin, profile.samplingInterval);
                    Handles.CubeHandleCap(0, endWorldPos, Quaternion.identity, size, EventType.Repaint);
                }

                var info = ev.GetType().Name.Replace("Event", "");

                if (!IsWithinDrawDistance(profile.labelDrawDistance, worldPos)) continue;

                if (profile.drawEventLabel)
                {
                    var style = GetLabelStyle(profile);
                    Handles.Label(worldPos + Vector3.down * size, info, style);
                }
            }
        }

        private bool IsWithinDrawDistance(float dist, Vector3 worldPos)
        {
            if (SceneView.lastActiveSceneView)
                _sceneCamera = SceneView.lastActiveSceneView.camera;

            if (_sceneCamera == null)
                return true;

            var sqrDist = (_sceneCamera.transform.position - worldPos).sqrMagnitude;
            return sqrDist <= dist * dist;
        }

        private Color GetEventColor(IPathEvent ev)
        {
            return ev switch
            {
                SpeedChangeEvent => SPEED_CHANGE_EVT_COLOR,
                GravityChangeEvent => GRAVITY_CHANGE_EVT_COLOR,
                DirectionChangeEvent => DIRECTION_CHANGE_EVT_COLOR,
                ForceTurnEvent => FORCE_TURN_EVT_COLOR,
                JumpEvent => JUMP_EVT_COLOR,
                TeleportEvent => TP_EVT_COLOR,
                _ => Color.white
            };
        }

        private GUIStyle GetLabelStyle(PathGrapherProfile profile)
        {
            _labelStyle ??= new GUIStyle();
            _labelStyle.normal.textColor = profile.labelTexColor;

            if (profile.labelBgClor.a > 0)
            {
                if (_labelBgTex == null)
                {
                    _labelBgTex = new(1, 1);
                    _labelBgTex.SetPixel(0, 0, profile.labelBgClor);
                    _labelBgTex.Apply();
                }
                _labelStyle.normal.background = _labelBgTex;
            }
            return _labelStyle;
        }
    }
}