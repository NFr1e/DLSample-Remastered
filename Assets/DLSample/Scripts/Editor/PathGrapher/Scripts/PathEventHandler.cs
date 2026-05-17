using UnityEditor;
using UnityEngine;

namespace DLSample.Editor.PathGrapher
{
    /// <summary>
    /// 路径事件手柄绘制类，提供各种路径事件在 Scene 视图中的交互手柄渲染与拖拽功能。
    /// </summary>
    public static class PathEventHandler
    {
        private static readonly Color _eventSelectHandleColor = new(1f, 1f, 1f, 0.5f);

        private static IPathEvent _draggingEvent;

        #region Default
        /// <summary>
        /// 绘制事件手柄，处理场景视图中的事件选择和位置拖拽。
        /// </summary>
        /// <param name="evt">当前路径事件</param>
        /// <param name="selected">当前选中的事件引用</param>
        /// <param name="behaviour">PathGrapherBehaviour 实例</param>
        /// <param name="editor">PathGrapherBehaviourEditor 实例</param>
        public static void Handle(this IPathEvent evt, ref IPathEvent selected, PathGrapherBehaviour behaviour, PathGrapherBehaviourEditor editor)
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

        private static bool _isDraggingTurn;
        private static Vector3 _tempTurnWorldPos;

        /// <summary>
        /// 绘制强制转向事件手柄，支持拖拽调整转向时间点。
        /// </summary>
        /// <param name="evt">当前强制转向事件</param>
        /// <param name="selected">当前选中的事件引用</param>
        /// <param name="behaviour">PathGrapherBehaviour 实例</param>
        /// <param name="editor">PathGrapherBehaviourEditor 实例</param>
        public static void Handle(this ForceTurnEvent evt, ref IPathEvent selected, PathGrapherBehaviour behaviour, PathGrapherBehaviourEditor editor)
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
            var displayPos = (_isDraggingTurn && _draggingEvent == evt) ? _tempTurnWorldPos : realWorldPos;

            EditorGUI.BeginChangeCheck();
            var nextPos = Handles.DoPositionHandle(displayPos, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                _isDraggingTurn = true;
                _draggingEvent = evt;
                _tempTurnWorldPos = nextPos;
            }

            if (_isDraggingTurn && _draggingEvent == evt && GUIUtility.hotControl == 0)
            {
                _isDraggingTurn = false;
                _draggingEvent = null;

                Undo.RecordObject(behaviour.asset, "Move Turn");

                var newTime = PathMappingUtility.FindNearestTimeOnPath(_tempTurnWorldPos, behaviour.asset.pathData, behaviour.transform, behaviour.profile.samplingInterval);

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

        private static bool _isDraggingJumpEnd;
        private static Vector3 _tempJumpEndWorldPos;

        /// <summary>
        /// 绘制跳跃事件手柄，支持拖拽调整跳跃起点和终点。
        /// </summary>
        /// <param name="evt">当前跳跃事件</param>
        /// <param name="selected">当前选中的事件引用</param>
        /// <param name="behaviour">PathGrapherBehaviour 实例</param>
        /// <param name="editor">PathGrapherBehaviourEditor 实例</param>
        public static void Handle(this JumpEvent evt, ref IPathEvent selected, PathGrapherBehaviour behaviour, PathGrapherBehaviourEditor editor)
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

            // 为了简化计算，此处使用了拖拽更新的方法

            var handleID = GUIUtility.GetControlID("JumpEndHandle".GetHashCode(), FocusType.Passive);

            var realWorldPos = PathMappingUtility.GetWorldPosFromTime(evt.EndTime, behaviour.asset.pathData, behaviour.transform, behaviour.profile.samplingInterval);
            var displayPos = (_isDraggingJumpEnd && _draggingEvent == evt) ? _tempJumpEndWorldPos : realWorldPos;

            EditorGUI.BeginChangeCheck();
            var nextPos = Handles.DoPositionHandle(displayPos, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                _isDraggingJumpEnd = true;
                _draggingEvent = evt;
                _tempJumpEndWorldPos = nextPos;
            }

            if (_isDraggingJumpEnd && _draggingEvent == evt && GUIUtility.hotControl == 0)
            {
                _isDraggingJumpEnd = false;
                _draggingEvent = null;

                Undo.RecordObject(behaviour.asset, "Move Jump End");

                var newTime = PathMappingUtility.FindNearestTimeOnPath(_tempJumpEndWorldPos, behaviour.asset.pathData, behaviour.transform, behaviour.profile.samplingInterval);

                var segment = PathMappingUtility.GetSegmentAtTime(evt.GlobalTime, behaviour.asset.pathData);

                var nextWpTime = segment.endWaypoint.time;
                evt.EndTime = System.Math.Clamp(newTime, evt.StartTime + 0.01, nextWpTime);

                behaviour.RequestRebuild();
            }

            #endregion
        }
        #endregion

        #region TeleportEvent
        /// <summary>
        /// 绘制传送事件手柄，支持拖拽调整传送目标位置。
        /// </summary>
        /// <param name="evt">当前传送事件</param>
        /// <param name="selected">当前选中的事件引用</param>
        /// <param name="behaviour">PathGrapherBehaviour 实例</param>
        /// <param name="editor">PathGrapherBehaviourEditor 实例</param>
        public static void Handle(this TeleportEvent evt, ref IPathEvent selected, PathGrapherBehaviour behaviour, PathGrapherBehaviourEditor editor)
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

            #region StartPoint
            // TeleportEvent 的起始点位置由场景中的跳板决定，此处不做位置调整。
            // 受限于当前架构，TeleportEvent 会在路径中引入一段空的 PathSection，
            // 导致无法通过 PositionHandle 的返回值来提取时间信息，故此功能已移除。
            #endregion

            #region EndPoint
            EditorGUI.BeginChangeCheck();

            var newTargetPos = Handles.PositionHandle(evt.targetPosition, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(behaviour.asset, "Move Teleport Target");

                evt.targetPosition = newTargetPos;
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
