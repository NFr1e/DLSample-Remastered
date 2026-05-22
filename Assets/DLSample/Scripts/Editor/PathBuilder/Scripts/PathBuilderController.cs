using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using DLSample.Editor.PathGrapher;

namespace DLSample.Editor.PathBuilder
{
    /// <summary>
    /// 路径生成方式：Connected 为连接模式，Disconnected 为断开模式。
    /// </summary>
    public enum PathGenerateType
    {
        Connected,
        Disconnected
    }
    /// <summary>
    /// 路径构建控制器，负责管理 UI 元素、处理用户输入并调用路径生成逻辑。
    /// </summary>
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

        /// <summary>
        /// 初始化路径构建控制器并克隆 UI 模板。
        /// </summary>
        /// <param name="visualTree">UI 布局模板</param>
        /// <param name="root">UI 根元素</param>
        public PathBuilderController(VisualTreeAsset visualTree, VisualElement root)
        {
            _visualTree = visualTree;
            _root = root;

            _visualTree.CloneTree(_root);
        }

        /// <summary>
        /// 初始化 UI 元素并绑定事件回调。
        /// </summary>
        public void Init()
        {
            GetElements();
            SubscribeEvents();
        }
        /// <summary>
        /// 释放控制器资源，取消所有事件订阅。
        /// </summary>
        public void Dispose()
        {
            UnsubscribeEvents();
        }

        /// <summary>
        /// 将已保存的设置值回填到 UI 控件。
        /// </summary>
        public void LoadSettings(PathBuilderSettings settings)
        {
            _pathPrefabField.value = settings.PathPrefab;
            _pathWidthField.value = settings.PathWidth;
            _pathTypeEnum.value = settings.GenerateType;
            _hintBoxPrefabField.value = settings.HintBoxPrefab;
            _hintSegPrefabField.value = settings.HintSegmentPrefab;
        }

        /// <summary>
        /// 从 UI 控件读取当前值并写入设置对象。
        /// </summary>
        public void SaveSettings(PathBuilderSettings settings)
        {
            settings.PathPrefab = _pathPrefabField.value as GameObject;
            settings.PathWidth = _pathWidthField.value;
            settings.GenerateType = (PathGenerateType)_pathTypeEnum.value;
            settings.HintBoxPrefab = _hintBoxPrefabField.value as GameObject;
            settings.HintSegmentPrefab = _hintSegPrefabField.value as GameObject;
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
            var asset = _pathGrapherAssetField.value as PathGrapherAsset;
            var pathPrefab = _pathPrefabField.value as GameObject;
            var pathWidth = _pathWidthField.value;
            var type = (PathGenerateType)_pathTypeEnum.value;

            if (asset == null) return;

            PathBuilderHelper.GeneratePath(asset.pathData, type, pathPrefab, pathWidth);
        }

        private void OnGenerateHintLineClicked(ClickEvent _)
        {
            var asset = _pathGrapherAssetField.value as PathGrapherAsset;
            var hintSeg = _hintSegPrefabField.value as GameObject;
            var hintBox = _hintBoxPrefabField.value as GameObject;

            if (asset == null) return;

            PathBuilderHelper.GenerateHintLine(asset.pathData, hintSeg, hintBox);
        }
    }
}
