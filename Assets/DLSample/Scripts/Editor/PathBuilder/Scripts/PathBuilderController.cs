using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using DLSample.Editor.PathGrapher;

namespace DLSample.Editor.PathBuilder
{
    public enum PathGenerateType
    {
        Connected,
        Disconnected
    }
    public class PathBuilderController
    {
        private readonly VisualTreeAsset _visualTree;
        private readonly VisualElement _root;

        private ObjectField _pathGrapherAssetField;

        private ObjectField _pathPrefabField;
        private FloatField _pathWidthField;
        private EnumField _pathTypeEnum;
        private Button _generatePathBtn;

        private ObjectField _hintBoxPrefabField;
        private ObjectField _hintSegPrefabField;
        private Button _generateHintLineBtn;

        private readonly string _class_pathGrapherAssetField = "source-pathgrapherasset-field";

        private readonly string _class_pathPrefabField = "path-pathprefab-field";
        private readonly string _class_pathWidthField = "path-pathwidth-field";
        private readonly string _class_pathTypeEnum = "path-generatetype-enum";
        private readonly string _class_generatePathBtn = "path-generate-btn";

        private readonly string _class_hintBoxPrefabField = "hintline-hintboxprefab-field";
        private readonly string _class_hintSegPrefabField = "hintline-hintsegprefab-field";
        private readonly string _class_generateHintLineBtn = "hintline-generate-btn";

        public PathBuilderController(VisualTreeAsset visualTree, VisualElement root)
        {
            _visualTree = visualTree;
            _root = root;

            _visualTree.CloneTree(_root);
        }

        public void Init()
        {
            GetElements();
            SubscribeEvents();
        }
        public void Dispose()
        {
            UnsubscribeEvents();
        }

        private void GetElements()
        {
            _pathGrapherAssetField = _root.Q<ObjectField>(className: _class_pathGrapherAssetField);

            _pathPrefabField = _root.Q<ObjectField>(className: _class_pathPrefabField);
            _pathWidthField = _root.Q<FloatField>(className: _class_pathWidthField);
            _pathTypeEnum = _root.Q<EnumField>(className: _class_pathTypeEnum);
            _generatePathBtn = _root.Q<Button>(className: _class_generatePathBtn);

            _hintBoxPrefabField = _root.Q<ObjectField>(className: _class_hintBoxPrefabField);
            _hintSegPrefabField = _root.Q<ObjectField>(className: _class_hintSegPrefabField);
            _generateHintLineBtn = _root.Q<Button>(className: _class_generateHintLineBtn);

            _pathGrapherAssetField.objectType = typeof(PathGrapherAsset);
            _pathPrefabField.objectType = typeof(GameObject);
            _hintBoxPrefabField.objectType = typeof(GameObject);
        }
        private void SubscribeEvents()
        {
            _generatePathBtn.RegisterCallback<ClickEvent>(OnGeneratePathBtnClicked);
            _generateHintLineBtn.RegisterCallback<ClickEvent>(OnGenerateHintLineClicked);
        }
        private void UnsubscribeEvents()
        {
            _generatePathBtn.UnregisterCallback<ClickEvent>(OnGeneratePathBtnClicked);
            _generateHintLineBtn.UnregisterCallback<ClickEvent>(OnGenerateHintLineClicked);
        }

        private void OnGeneratePathBtnClicked(ClickEvent _)
        {
            PathGrapherAsset asset = _pathGrapherAssetField.value as PathGrapherAsset;
            GameObject pathPrefab = _pathPrefabField.value as GameObject;
            float pathWidth = _pathWidthField.value;
            PathGenerateType type = (PathGenerateType)_pathTypeEnum.value;

            PathBuilderHelper.GeneratePath(asset.pathData, type, pathPrefab, pathWidth);
        }

        private void OnGenerateHintLineClicked(ClickEvent evt)
        {
            PathGrapherAsset asset = _pathGrapherAssetField.value as PathGrapherAsset;
            GameObject hintSeg = _hintSegPrefabField.value as GameObject;
            GameObject hintBox = _hintBoxPrefabField.value as GameObject;

            PathBuilderHelper.GenerateHintLine(asset.pathData, hintSeg, hintBox);
        }
    }
}
