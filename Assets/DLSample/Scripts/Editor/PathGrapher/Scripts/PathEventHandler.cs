using UnityEditor;
using UnityEngine;

namespace DLSample.Editor.PathGrapher
{
    /// <summary>
    /// 路径事件交互类，提供各种路径事件在 Scene 视图中的交互手柄渲染与拖拽交互功能。
    /// </summary>
    public static class PathEventHandler
    {
        private static readonly Color _eventSelectHandleColor = new(1f, 1f, 1f, 0.5f);

        #region Default
        /// <summary>
        /// 绘制事件手柄，处理场景视图中的事件选择和位置拖拽（默认实现）。
        /// </summary>
        public static void HandleDefault(IPathEvent evt, ref IPathEvent selected, PathGrapherBehaviour behaviour, PathGrapherBehaviourEditor editor)
        {
            var pos = PathMappingUtility.GetWorldPosFromTime(evt.GlobalTime, behaviour.asset.pathData, behaviour.transform, behaviour.profile.samplingInterval);
            var size = 0.5f;

            Handles.color = _eventSelectHandleColor;

            if (Handles.Button(pos, Quaternion.identity, size, size, Handles.SphereHandleCap))
            {
                selected = evt;
                RefreshEditorSelectEvent(selected, editor);
            }

            if (selected != evt)
            {
                return;
            }

            EditorGUI.BeginChangeCheck();

            var newPos = Handles.PositionHandle(pos, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(behaviour.asset, "Edit Event");

                evt.GlobalTime = PathMappingUtility.FindNearestTimeOnPath(newPos, behaviour.asset.pathData, behaviour.transform, behaviour.profile.samplingInterval);
                behaviour.RequestRebuild();

                editor.Repaint();
            }
        }
        #endregion

        #region ForceTurn

        /// <summary>
        /// 绘制强制转向事件手柄，支持拖拽调整转向时间点。
        /// </summary>
        public static void HandleForceTurn(IPathEvent evt, ref IPathEvent selected, PathGrapherBehaviour behaviour, PathGrapherBehaviourEditor editor)
        {
            var pos = PathMappingUtility.GetWorldPosFromTime(evt.GlobalTime, behaviour.asset.pathData, behaviour.transform, behaviour.profile.samplingInterval);
            var size = 0.5f;

            Handles.color = _eventSelectHandleColor;

            if (Handles.Button(pos, Quaternion.identity, size, size, Handles.SphereHandleCap))
            {
                selected = evt;
                RefreshEditorSelectEvent(selected, editor);
            }

            if (selected != evt)
            {
                return;
            }

            var handleID = GUIUtility.GetControlID("TurnHandle".GetHashCode(), FocusType.Passive);

            var realWorldPos = PathMappingUtility.GetWorldPosFromTime(evt.GlobalTime, behaviour.asset.pathData, behaviour.transform, behaviour.profile.samplingInterval);
            var displayPos = (editor.IsDraggingTurn && editor.DraggingEvent == evt) ? editor.TempTurnWorldPos : realWorldPos;

            EditorGUI.BeginChangeCheck();
            var nextPos = Handles.DoPositionHandle(displayPos, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                editor.IsDraggingTurn = true;
                editor.DraggingEvent = evt;
                editor.TempTurnWorldPos = nextPos;
            }

            if (editor.IsDraggingTurn && editor.DraggingEvent == evt && GUIUtility.hotControl == 0)
            {
                editor.IsDraggingTurn = false;
                editor.DraggingEvent = null;

                Undo.RecordObject(behaviour.asset, "Move Turn");

                var newTime = PathMappingUtility.FindNearestTimeOnPath(editor.TempTurnWorldPos, behaviour.asset.pathData, behaviour.transform, behaviour.profile.samplingInterval);

                var segment = PathMappingUtility.GetSegmentAtTime(evt.GlobalTime, behaviour.asset.pathData);

                var prevWpTime = segment.startWaypoint.time;
                var newWpTime = PathMappingUtility.GetSegmentAtTime(evt.GlobalTime + 0.01f, behaviour.asset.pathData).endWaypoint.time;
                evt.GlobalTime = System.Math.Clamp(newTime, prevWpTime, newWpTime);

                behaviour.RequestRebuild();
                editor.Repaint();
            }
        }
        #endregion

        #region JumpEvent

        /// <summary>
        /// 绘制跳跃事件手柄，支持拖拽调整跳跃起点和终点。
        /// </summary>
        public static void HandleJump(IPathEvent evt, ref IPathEvent selected, PathGrapherBehaviour behaviour, PathGrapherBehaviourEditor editor)
        {
            var jumpEvt = (JumpEvent)evt;
            var pos = PathMappingUtility.GetWorldPosFromTime(evt.GlobalTime, behaviour.asset.pathData, behaviour.transform, behaviour.profile.samplingInterval);
            var size = 0.5f;

            Handles.color = _eventSelectHandleColor;

            if (Handles.Button(pos, Quaternion.identity, size, size, Handles.SphereHandleCap))
            {
                selected = evt;
                RefreshEditorSelectEvent(selected, editor);
            }

            if (selected != evt)
            {
                return;
            }

            #region StartPoint
            EditorGUI.BeginChangeCheck();

            var newStartPos = Handles.PositionHandle(pos, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(behaviour.asset, "Move Jump Start");

                evt.GlobalTime = PathMappingUtility.FindNearestTimeOnPath(newStartPos, behaviour.asset.pathData, behaviour.transform, behaviour.profile.samplingInterval);
                behaviour.RequestRebuild();

                editor.Repaint();
            }
            #endregion

            #region EndPoint
            var handleID = GUIUtility.GetControlID("JumpEndHandle".GetHashCode(), FocusType.Passive);

            var realWorldPos = PathMappingUtility.GetWorldPosFromTime(jumpEvt.EndTime, behaviour.asset.pathData, behaviour.transform, behaviour.profile.samplingInterval);
            var displayPos = (editor.IsDraggingJumpEnd && editor.DraggingEvent == evt) ? editor.TempJumpEndWorldPos : realWorldPos;

            EditorGUI.BeginChangeCheck();
            var nextPos = Handles.DoPositionHandle(displayPos, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                editor.IsDraggingJumpEnd = true;
                editor.DraggingEvent = evt;
                editor.TempJumpEndWorldPos = nextPos;
            }

            if (editor.IsDraggingJumpEnd && editor.DraggingEvent == evt && GUIUtility.hotControl == 0)
            {
                editor.IsDraggingJumpEnd = false;
                editor.DraggingEvent = null;

                Undo.RecordObject(behaviour.asset, "Move Jump End");

                var newTime = PathMappingUtility.FindNearestTimeOnPath(editor.TempJumpEndWorldPos, behaviour.asset.pathData, behaviour.transform, behaviour.profile.samplingInterval);

                var segment = PathMappingUtility.GetSegmentAtTime(evt.GlobalTime, behaviour.asset.pathData);

                var nextWpTime = segment.endWaypoint.time;
                jumpEvt.EndTime = System.Math.Clamp(newTime, jumpEvt.StartTime + 0.01, nextWpTime);

                behaviour.RequestRebuild();
            }
            #endregion
        }
        #endregion

        #region TeleportEvent
        /// <summary>
        /// 绘制传送事件手柄，支持拖拽调整传送目标位置。
        /// </summary>
        public static void HandleTeleport(IPathEvent evt, ref IPathEvent selected, PathGrapherBehaviour behaviour, PathGrapherBehaviourEditor editor)
        {
            var teleportEvt = (TeleportEvent)evt;
            var pos = PathMappingUtility.GetWorldPosFromTime(evt.GlobalTime, behaviour.asset.pathData, behaviour.transform, behaviour.profile.samplingInterval);
            var size = 0.5f;

            Handles.color = _eventSelectHandleColor;

            if (Handles.Button(pos, Quaternion.identity, size, size, Handles.SphereHandleCap))
            {
                selected = evt;
                RefreshEditorSelectEvent(selected, editor);
            }

            if (selected != evt)
            {
                return;
            }

            #region StartPoint
            var handleID = GUIUtility.GetControlID("TeleportStartHandle".GetHashCode(), FocusType.Passive);

            var realWorldPos = PathMappingUtility.GetWorldPosFromTime(evt.GlobalTime, behaviour.asset.pathData, behaviour.transform, behaviour.profile.samplingInterval);
            var displayPos = (editor.IsDraggingTeleportStart && editor.DraggingEvent == evt) ? editor.TempTeleportStartWorldPos : realWorldPos;

            EditorGUI.BeginChangeCheck();
            var nextPos = Handles.DoPositionHandle(displayPos, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                editor.IsDraggingTeleportStart = true;
                editor.DraggingEvent = evt;
                editor.TempTeleportStartWorldPos = nextPos;
            }

            if (editor.IsDraggingTeleportStart && editor.DraggingEvent == evt && GUIUtility.hotControl == 0)
            {
                editor.IsDraggingTeleportStart = false;
                editor.DraggingEvent = null;

                Undo.RecordObject(behaviour.asset, "Move Teleport Start");

                double newGlobalTime = PathMappingUtility.FindNearestTimeOnPath(editor.TempTeleportStartWorldPos, behaviour.asset.pathData, behaviour.transform, behaviour.profile.samplingInterval);
                evt.GlobalTime = newGlobalTime;

                behaviour.RequestRebuild();
                editor.Repaint();
            }
            #endregion

            #region EndPoint
            EditorGUI.BeginChangeCheck();

            var newTargetPos = Handles.PositionHandle(teleportEvt.targetPosition, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(behaviour.asset, "Move Teleport Target");

                teleportEvt.targetPosition = newTargetPos;
                behaviour.RequestRebuild();
            }
            #endregion
        }
        #endregion

        private static void RefreshEditorSelectEvent(IPathEvent evt, PathGrapherBehaviourEditor editor)
        {
            editor.SelectedEvent = evt;
            editor.Repaint();
        }
    }
}
