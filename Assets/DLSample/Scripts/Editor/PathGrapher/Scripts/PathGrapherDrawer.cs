using UnityEditor;
using UnityEngine;

namespace DLSample.Editor.PathGrapher
{
    public class PathGrapherDrawer
    {
        #region Configs
        private static readonly Color speedChangeEvtColor = Color.green;
        private static readonly Color gravityChangeEvtColor = Color.magenta;
        private static readonly Color directionChangeEvtColor = Color.blue;
        private static readonly Color forceTurnEvtColor = Color.gray;
        private static readonly Color jumpEvtColor = Color.yellow;
        private static readonly Color tpEvtColor = Color.red;
        #endregion

        private GUIStyle _labelStyle = new();
        private Texture2D _labelBgTex;
        private Camera _sceneCamera;

        public void DrawPath(PathData pathData, Transform origin, PathGrapherProfile profile = default)
        {
            Matrix4x4 localToWorld = origin.localToWorldMatrix;

            if (profile.zTest)
            {
                var prevZtest = Handles.zTest;

                Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                Draw();
                Handles.zTest = prevZtest;
            }
            else
                Draw();

            void Draw()
            {
                DrawSegments(pathData, localToWorld, profile);
                DrawWaypoints(pathData, localToWorld, profile);
                DrawEventHandles(pathData, origin, profile);
            }
        }
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
        private void DrawSegment( PathSegment segment, Matrix4x4 matrix, PathGrapherProfile profile)
        {
            if (!segment.IsValid) return;

            Vector3 segmentStartPos = matrix.MultiplyPoint(segment.startWaypoint.position);
            if (!IsWithinDrawDistance(profile.pathDrawDistance, segmentStartPos)) return;

            if (segment.IsSimpleStright)
            {
                DrawStrightLine();
            }
            else
            {
                DrawSegmentDetailed();
            }

            #region ...
            void DrawStrightLine()
            {
                Vector3 endPos = matrix.MultiplyPoint(segment.endWaypoint.position);

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
                    int len = section.points.Length;
                    Vector3[] points = new Vector3[len];

                    for (int i = 0; i < len; i++)
                        points[i] = matrix.MultiplyPoint(section.points[i]);

                    if (section.isJump)
                        Handles.color = Color.red;
                    else Handles.color = profile.pathColor;

                    Handles.DrawAAPolyLine(4f, points);
                    //Handles.Label(points[0], "ThisSegmentIsDrawnAsCurve");
                }
                void DrawTeleport(PathSection section)
                {
                    int len = section.points.Length;
                    Vector3[] points = new Vector3[len];

                    for (int i = 0; i < len; i++)
                        points[i] = matrix.MultiplyPoint(section.points[i]);

                    Handles.color = Color.red;
                    Handles.DrawDottedLine(points[0], points[^1], 4f);
                }
            }
            #endregion
        }

        private void DrawWaypoints(PathData pathData, Matrix4x4 matrix, PathGrapherProfile profile)
        {
            Handles.color = profile.pathColor;

            foreach (var wp in pathData.generatedWaypoints)
            {
                Vector3 worldPos = matrix.MultiplyPoint(wp.position);

                if (!IsWithinDrawDistance(profile.pathDrawDistance, worldPos)) continue;

                float size = 0.5f;
                Handles.CubeHandleCap(0, worldPos, wp.rotation, size, EventType.Repaint);


                if (!IsWithinDrawDistance(profile.labelDrawDistance, worldPos)) continue;
                if (profile.drawWaypointLabel)
                {
                    GUIStyle style = GetLabelStyle(profile);
                    Handles.Label(worldPos + 2 * size * Vector3.up, $"Beat: {wp.beatIndex} Time: {wp.time:F2}s", style);
                }
            }
        }

        private void DrawEventHandles(PathData pathData, Transform origin, PathGrapherProfile profile)
        {
            if (!profile.drawEvents) return;

            foreach (var ev in pathData.globalEvents)
            {
                Vector3 worldPos = PathMappingUtility.GetWorldPosFromTime(ev.GlobalTime, pathData, origin, profile.samplingInterval);

                if (!IsWithinDrawDistance(profile.pathDrawDistance, worldPos)) continue;

                float size = 1f;
                Handles.color = GetEventColor(ev);
                Handles.CubeHandleCap(0, worldPos, Quaternion.identity, size, EventType.Repaint);

                if (ev is SegmentPathEvent segEv)
                {
                    Vector3 endWorldPos = PathMappingUtility.GetWorldPosFromTime(segEv.EndTime, pathData, origin, profile.samplingInterval);
                    Handles.CubeHandleCap(0, endWorldPos, Quaternion.identity, size, EventType.Repaint);
                }


                string info = ev.GetType().Name.Replace("Event", "");

                if (!IsWithinDrawDistance(profile.labelDrawDistance, worldPos)) continue;

                if (profile.drawEventLabel)
                {
                    GUIStyle style = GetLabelStyle(profile);
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

            float sqrDist = (_sceneCamera.transform.position - worldPos).sqrMagnitude;
            return sqrDist <= dist * dist;
        }

        private Color GetEventColor(IPathEvent ev)
        {
            return ev switch
            {
                SpeedChangeEvent => speedChangeEvtColor,
                GravityChangeEvent => gravityChangeEvtColor,
                DirectionChangeEvent => directionChangeEvtColor,
                ForceTurnEvent => forceTurnEvtColor,
                JumpEvent => jumpEvtColor,
                TeleportEvent => tpEvtColor,
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