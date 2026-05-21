#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DLSample.Editor.PathGrapher
{
    /// <summary>
    /// PathGrapherBehaviour 的自定义 Inspector 编辑器，提供路径事件创建、编辑与可视化功能。
    /// </summary>
    [CustomEditor(typeof(PathGrapherBehaviour))]
    public class PathGrapherBehaviourEditor : UnityEditor.Editor
    {
        public static PathGrapherBehaviourEditor ActiveEditor { get; private set; }

        private PathGrapherBehaviour _target;
        public PathGrapherBehaviour Target => _target;

        private static readonly PathGrapherDrawer _drawer = new();

        public IPathEvent SelectedEvent;
        public static bool EnableEventCreation;

        // 拖拽状态（实例级避免多 Behaviour 时互相干扰）
        public IPathEvent DraggingEvent;
        public bool IsDraggingTurn;
        public Vector3 TempTurnWorldPos;
        public bool IsDraggingJumpEnd;
        public Vector3 TempJumpEndWorldPos;
        public bool IsDraggingTeleportStart;
        public Vector3 TempTeleportStartWorldPos;

        #region Caches
        private readonly GUIContent _menuSpeedChangeLabel = new("Add SpeedChange");
        private readonly GUIContent _menuGravityChangeLabel = new("Add GravityChange");
        private readonly GUIContent _menuDirectionChangeLabel = new("Add DirectionChange");
        private readonly GUIContent _menuForceTurnLabel = new("Add ForceTurn");
        private readonly GUIContent _menuJumpLabel = new("Add Jump");
        private readonly GUIContent _menuTeleport = new("Add Teleport");

        public readonly Dictionary<IPathEvent, PropertyTree> PropertyTreeCache = new();
        #endregion

        private void OnEnable()
        {
            ActiveEditor = this;
            _target = (PathGrapherBehaviour)target;
        }

        private void OnDestroy()
        {
            _drawer?.Dispose();
        }

        private void OnDisable()
        {
            if (ActiveEditor == this) ActiveEditor = null;
            foreach (var tree in PropertyTreeCache.Values)
            {
                tree?.Dispose();
            }
            PropertyTreeCache.Clear();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                _target.RequestRebuild();
            }

            EditorGUILayout.Space(10);
            DrawOperations();
        }

        private void OnSceneGUI()
        {
            if (!_target.asset) return;
            HandleEventPlaceholder();
            HandleEvents();
        }

        [DrawGizmo(GizmoType.NonSelected)]
        public static void DrawPathOnDeselected(PathGrapherBehaviour beh, GizmoType gizmoType)
        {
            var asset = beh.asset;
            if (asset == null) return;

            if (beh.profile.drawAlways)
            {
                _drawer?.DrawPath(asset.pathData, beh.transform, beh.profile);
            }
        }

        [DrawGizmo(GizmoType.Selected)]
        public static void DrawPathOnSelected(PathGrapherBehaviour beh, GizmoType gizmoType)
        {
            if (beh.asset == null) return;
            _drawer?.DrawPath(beh.asset.pathData, beh.transform, beh.profile);
        }

        #region Inspector
        private void DrawOperations()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("ForceRebuild"))
            {
                _target.RequestRebuild();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawEventCreationToggle()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"EventsCreator: {(EnableEventCreation ? "ON" : "Off")}", EditorStyles.boldLabel);
            EnableEventCreation = EditorGUILayout.Toggle("Enable Event Creator", EnableEventCreation);
            EditorGUILayout.EndVertical();
        }

        private void DrawSelectedEventFields()
        {
            if (SelectedEvent == null) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Editing: {SelectedEvent.GetType().Name}", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            DrawEventsImgui();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_target.asset);
                _target.RequestRebuild();
            }

            if (GUILayout.Button("Delete Event"))
            {
                Undo.RecordObject(_target.asset, "Delete Path Event");

                _target.asset.pathData.globalEvents.Remove(SelectedEvent);

                if (PropertyTreeCache.TryGetValue(SelectedEvent, out var tree))
                {
                    tree.Dispose();
                    PropertyTreeCache.Remove(SelectedEvent);
                }

                SelectedEvent = null;
                _target.RequestRebuild();
            }

            EditorGUILayout.EndVertical();
        }

        public void DrawEventsImgui()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical();
            SelectedEvent.GlobalTime = EditorGUILayout.DoubleField("Event Time", SelectedEvent.GlobalTime);

            if (PathEventEditorMeta.Registry.TryGetValue(SelectedEvent.GetType(), out var meta))
            {
                meta.DrawInspector?.Invoke(SelectedEvent, this);
            }

            EditorGUILayout.EndVertical();
        }

        public void DrawDirectionChangeEventInspector(DirectionChangeEvent evt)
        {
            if (evt.newDirections == null)
            {
                EditorGUILayout.HelpBox("Directions field is null.", MessageType.Error);
                return;
            }

            if (!PropertyTreeCache.TryGetValue(evt, out var propertyTree) || propertyTree == null)
            {
                propertyTree = PropertyTree.Create(evt.newDirections);
                PropertyTreeCache[evt] = propertyTree;
            }

            propertyTree.Draw(false);

            propertyTree.ApplyChanges();
            EditorUtility.SetDirty(_target.asset);
        }
        #endregion

        #region CreateEvent
        private void HandleEventPlaceholder()
        {
            if (!EnableEventCreation) return;

            Event e = Event.current;

            var (worldPos, time) = PathMappingUtility.FindNearestPointByMouse(e.mousePosition, _target.asset.pathData, _target.transform, _target.profile.samplingInterval);

            float screenDist = Vector2.Distance(e.mousePosition, HandleUtility.WorldToGUIPoint(worldPos));

            if (screenDist < 32f)
            {
                Handles.color = new Color(1, 1, 1, 0.4f);
                Handles.SphereHandleCap(0, worldPos, Quaternion.identity,
                                 HandleUtility.GetHandleSize(worldPos) * 0.1f, EventType.Repaint);

                Handles.Label(worldPos + Vector3.up * 0.2f, $"{time:F2}s");
                if (e.type == EventType.MouseDown && e.button == 1)
                {
                    ShowContextMenu(time);
                    e.Use();
                }
            }
        }

        private void ShowContextMenu(double time)
        {
            var genericMenu = new GenericMenu();
            genericMenu.AddItem(_menuSpeedChangeLabel, false, () => CreatePointEvent<SpeedChangeEvent>(time));
            genericMenu.AddItem(_menuGravityChangeLabel, false, () => CreatePointEvent<GravityChangeEvent>(time));
            genericMenu.AddItem(_menuDirectionChangeLabel, false, () => CreatePointEvent<DirectionChangeEvent>(time));
            genericMenu.AddItem(_menuForceTurnLabel, false, () => CreatePointEvent<ForceTurnEvent>(time));
            genericMenu.AddSeparator("");
            genericMenu.AddItem(_menuJumpLabel, false, () => CreateSegmentEvent<JumpEvent>(time));
            genericMenu.AddItem(_menuTeleport, false, () => CreateSegmentEvent<TeleportEvent>(time));
            genericMenu.ShowAsContext();
        }

        private void CreatePointEvent<T>(double time) where T : PointPathEvent, new()
        {
            Undo.RecordObject(_target.asset, "Add Point Event");

            T evt = new()
            {
                GlobalTime = time
            };

            switch (evt)
            {
                case SpeedChangeEvent s:
                    s.newSpeed = 12;
                    break;
            }

            OnEventCreated(evt);
        }

        private void CreateSegmentEvent<T>(double startTime) where T : SegmentPathEvent, new()
        {
            Undo.RecordObject(_target.asset, "Add Segment Event");

            T evt = new()
            {
                StartTime = startTime
            };

            double endTime = startTime + 0.0001f;

            switch (evt)
            {
                case JumpEvent j:
                    j.velocity = Vector3.zero;

                    var segment = PathMappingUtility.GetSegmentAtTime(startTime, _target.asset.pathData);
                    double maxPossibleTime = segment.endWaypoint.time;
                    endTime = System.Math.Min(startTime + 1.0, maxPossibleTime);
                    break;
            }

            evt.EndTime = endTime;

            OnEventCreated(evt);
        }

        private void OnEventCreated(IPathEvent evt)
        {
            _target.asset.pathData.globalEvents.Add(evt);
            SelectedEvent = evt;
            _target.RequestRebuild();
        }
        #endregion

        #region HandleEvent
        private void HandleEvents()
        {
            foreach (var evt in _target.asset.pathData.globalEvents)
            {
                if (PathEventEditorMeta.Registry.TryGetValue(evt.GetType(), out var meta))
                {
                    meta.DrawSceneHandles?.Invoke(evt, ref SelectedEvent, _target, this);
                }
            }
        }
        #endregion
    }
}
#endif