using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace DLSample.Editor.PathGrapher
{
    /// <summary>
    /// Scene 视图悬浮面板：Event Creator 开关和事件参数编辑。
    /// 仅在选中 PathGrapherBehaviour 时显示。
    /// </summary>
    [Overlay(typeof(SceneView), "PathGrapher", false)]
    public class PathGrapherOverlay : Overlay
    {
        public override void OnCreated()
        {
            base.OnCreated();
            displayed = false;
            Selection.selectionChanged += OnSelectionChanged;
        }

        public override void OnWillBeDestroyed()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            base.OnWillBeDestroyed();
        }

        private void OnSelectionChanged()
        {
            var go = Selection.activeGameObject;
            displayed = go != null && go.GetComponent<PathGrapherBehaviour>() != null;
        }

        public override VisualElement CreatePanelContent()
        {
            var container = new IMGUIContainer(DrawGUI);
            container.style.minWidth = 200;
            return container;
        }

        private void DrawGUI()
        {
            var editor = PathGrapherBehaviourEditor.ActiveEditor;
            if (editor == null) return;

            DrawEventCreationToggle();
            DrawSelectedEventFields(editor);
        }

        private static void DrawEventCreationToggle()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"EventsCreator: {(PathGrapherBehaviourEditor.EnableEventCreation ? "ON" : "Off")}", EditorStyles.boldLabel);
            PathGrapherBehaviourEditor.EnableEventCreation = EditorGUILayout.Toggle("Enable Event Creator", PathGrapherBehaviourEditor.EnableEventCreation);
            EditorGUILayout.EndVertical();
        }

        private static void DrawSelectedEventFields(PathGrapherBehaviourEditor editor)
        {
            if (editor.SelectedEvent == null) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Editing: {editor.SelectedEvent.GetType().Name}", EditorStyles.boldLabel);

            var asset = editor.Target.asset;
            Undo.RecordObject(asset, "Edit Path Event");
            EditorGUI.BeginChangeCheck();

            editor.DrawEventsImgui();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(asset);
                editor.Target.RequestRebuild();
            }

            if (GUILayout.Button("Delete Event"))
            {
                Undo.RecordObject(asset, "Delete Path Event");

                asset.pathData.globalEvents.Remove(editor.SelectedEvent);

                if (editor.PropertyTreeCache.TryGetValue(editor.SelectedEvent, out var tree))
                {
                    tree.Dispose();
                    editor.PropertyTreeCache.Remove(editor.SelectedEvent);
                }

                editor.SelectedEvent = null;
                editor.Target.RequestRebuild();
            }

            EditorGUILayout.EndVertical();
        }
    }
}
