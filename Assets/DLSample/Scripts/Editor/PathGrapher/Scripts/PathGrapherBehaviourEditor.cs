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
        private PathGrapherBehaviour _target;

        private static readonly PathGrapherDrawer _drawer = new();

        public IPathEvent SelectedEvent;
        private static bool _enableEventCreation;

        #region Caches
        private readonly GUIContent _menuSpeedChangeLabel = new("Add SpeedChange");
        private readonly GUIContent _menuGravityChangeLabel = new("Add GravityChange");
        private readonly GUIContent _menuDirectionChangeLabel = new("Add DirectionChange");
        private readonly GUIContent _menuForceTurnLabel = new("Add ForceTurn");
        private readonly GUIContent _menuJumpLabel = new("Add Jump");
        private readonly GUIContent _menuTeleport = new("Add Teleport");

        private readonly Dictionary<IPathEvent, PropertyTree> _evtPropertyTreesCache = new();
        #endregion

        private void OnEnable()
        {
            _target = (PathGrapherBehaviour)target;
        }

        private void OnDestroy()
        {
            _drawer?.Dispose();
        }

        private void OnDisable()
        {
            foreach (var tree in _evtPropertyTreesCache.Values)
            {
                tree?.Dispose();
            }
            _evtPropertyTreesCache.Clear();
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
            DrawEventCreationToggle();
            DrawSelectedEventFields();
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
            EditorGUILayout.LabelField($"EventsCreator: {(_enableEventCreation ? "ON" : "Off")}", EditorStyles.boldLabel);
            _enableEventCreation = EditorGUILayout.Toggle("Enable Event Creator", _enableEventCreation);
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

                if (_evtPropertyTreesCache.TryGetValue(SelectedEvent, out var tree))
                {
                    tree.Dispose();
                    _evtPropertyTreesCache.Remove(SelectedEvent);
                }

                SelectedEvent = null;
                _target.RequestRebuild();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawEventsImgui()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical();
            SelectedEvent.GlobalTime = EditorGUILayout.DoubleField("Event Time", SelectedEvent.GlobalTime);

            switch (SelectedEvent)
            {
                case PointPathEvent ptE:

                    EditorGUILayout.Space(10);

                    switch (ptE)
                    {
                        case SpeedChangeEvent s:
                            s.newSpeed = EditorGUILayout.FloatField("New Speed", s.newSpeed);
                            break;

                        case GravityChangeEvent g:
                            g.newGravity = EditorGUILayout.Vector3Field("New Gravity", g.newGravity);
                            break;

                        case DirectionChangeEvent d:
                            DrawDirectionChangeEvent(d);
                            break;
                    }
                    break;

                case SegmentPathEvent segE:

                    switch (segE)
                    {
                        case JumpEvent j:
                            EditorGUILayout.BeginHorizontal();
                            segE.EndTime = EditorGUILayout.DoubleField("End Time", segE.EndTime);
                            if (GUILayout.Button("+ 0.1s"))
                            {
                                segE.EndTime += 0.1f;
                            }
                            if (GUILayout.Button("- 0.1s"))
                            {
                                segE.EndTime -= 0.1f;
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.Space(10);

                            j.velocity = EditorGUILayout.Vector3Field("Velocity", j.velocity);
                            break;

                        case TeleportEvent t:
                            t.targetPosition = EditorGUILayout.Vector3Field("Target Pos", t.targetPosition);
                            break;
                    }
                    break;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawDirectionChangeEvent(DirectionChangeEvent evt)
        {
            if (evt.newDirections == null)
            {
                EditorGUILayout.HelpBox("Directions field is null.", MessageType.Error);
                return;
            }

            if (!_evtPropertyTreesCache.TryGetValue(evt, out var propertyTree) || propertyTree == null)
            {
                propertyTree = PropertyTree.Create(evt.newDirections);
                _evtPropertyTreesCache[evt] = propertyTree;
            }

            propertyTree.Draw(false);

            propertyTree.ApplyChanges();
            EditorUtility.SetDirty(_target.asset);
        }
        #endregion

        #region CreateEvent
        private void HandleEventPlaceholder()
        {
            if (!_enableEventCreation) return;

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

                case TeleportEvent t:
                    t.targetPosition = Vector3.zero;
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
            var asset = _target.asset;

            foreach (var evt in asset.pathData.globalEvents)
            {
                switch (evt)
                {
                    case ForceTurnEvent t:
                        t.Handle(ref SelectedEvent, _target, this);
                        break;
                    case JumpEvent j:
                        j.Handle(ref SelectedEvent, _target, this);
                        break;
                    case TeleportEvent t:
                        t.Handle(ref SelectedEvent, _target, this);
                        break;

                    default:
                        evt.Handle(ref SelectedEvent, _target, this);
                        break;
                }
            }
        }
        #endregion
    }
}
#endif