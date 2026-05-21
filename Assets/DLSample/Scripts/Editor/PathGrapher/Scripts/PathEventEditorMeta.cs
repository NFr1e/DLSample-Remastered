using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DLSample.Editor.PathGrapher
{
    /// <summary>
    /// Scene 视图事件手柄绘制委托
    /// </summary>
    public delegate void SceneHandleDrawer(IPathEvent evt, ref IPathEvent selected, PathGrapherBehaviour behaviour, PathGrapherBehaviourEditor editor);

    /// <summary>
    /// 路径事件的编辑器元数据注册表，消除各处 switch 分发。
    /// 新增事件类型时，在此注册一行即可。
    /// </summary>
    public class PathEventEditorMeta
    {
        public Color GizmoColor;
        public Action<IPathEvent, PathGrapherBehaviourEditor> DrawInspector;
        public SceneHandleDrawer DrawSceneHandles;

        private static readonly Dictionary<Type, PathEventEditorMeta> _registry = new();
        public static IReadOnlyDictionary<Type, PathEventEditorMeta> Registry => _registry;

        static PathEventEditorMeta()
        {
            _registry[typeof(SpeedChangeEvent)] = new PathEventEditorMeta
            {
                GizmoColor = Color.green,
                DrawInspector = (evt, editor) =>
                {
                    var s = (SpeedChangeEvent)evt;
                    EditorGUILayout.Space(10);
                    s.newSpeed = EditorGUILayout.FloatField("New Speed", s.newSpeed);
                },
                DrawSceneHandles = PathEventHandler.HandleDefault
            };

            _registry[typeof(GravityChangeEvent)] = new PathEventEditorMeta
            {
                GizmoColor = Color.magenta,
                DrawInspector = (evt, editor) =>
                {
                    var g = (GravityChangeEvent)evt;
                    EditorGUILayout.Space(10);
                    g.newGravity = EditorGUILayout.Vector3Field("New Gravity", g.newGravity);
                },
                DrawSceneHandles = PathEventHandler.HandleDefault
            };

            _registry[typeof(DirectionChangeEvent)] = new PathEventEditorMeta
            {
                GizmoColor = Color.blue,
                DrawInspector = (evt, editor) =>
                {
                    var d = (DirectionChangeEvent)evt;
                    EditorGUILayout.Space(10);
                    editor.DrawDirectionChangeEventInspector(d);
                },
                DrawSceneHandles = PathEventHandler.HandleDefault
            };

            _registry[typeof(ForceTurnEvent)] = new PathEventEditorMeta
            {
                GizmoColor = Color.gray,
                DrawInspector = (evt, editor) => { },
                DrawSceneHandles = PathEventHandler.HandleForceTurn
            };

            _registry[typeof(JumpEvent)] = new PathEventEditorMeta
            {
                GizmoColor = Color.yellow,
                DrawInspector = (evt, editor) =>
                {
                    var j = (JumpEvent)evt;
                    EditorGUILayout.BeginHorizontal();
                    j.EndTime = EditorGUILayout.DoubleField("End Time", j.EndTime);
                    if (GUILayout.Button("+ 0.1s")) j.EndTime += 0.1f;
                    if (GUILayout.Button("- 0.1s")) j.EndTime -= 0.1f;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(10);
                    j.velocity = EditorGUILayout.Vector3Field("Velocity", j.velocity);
                },
                DrawSceneHandles = PathEventHandler.HandleJump
            };

            _registry[typeof(TeleportEvent)] = new PathEventEditorMeta
            {
                GizmoColor = Color.red,
                DrawInspector = (evt, editor) =>
                {
                    var t = (TeleportEvent)evt;
                    t.targetPosition = EditorGUILayout.Vector3Field("Target Pos", t.targetPosition);
                },
                DrawSceneHandles = PathEventHandler.HandleTeleport
            };
        }
    }
}
