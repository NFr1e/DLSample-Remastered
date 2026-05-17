#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace DLSample.Editor.LevelCreator
{
    /// <summary>
    /// 关卡创建窗口的控制器，负责管理 UI 元素生命周期、事件订阅与数据绑定。
    /// </summary>
    public class LevelCreatorController
    {
        private readonly VisualTreeAsset _visualTree;
        private readonly LevelCreatorWindow _window;

        private string _selectedBasePath = "";

        #region UIElements
        private Label _pathDisplayLabel;
        private Button _pathSelectBtn;

        private TextField _levelNameField;
        private TextField _soundtrackInfoField;
        private ObjectField _soundtrackField;
        private IntegerField _gemCountField;

        private Button _confirmBtn;
        private Button _cancelBtn;

        #region Mapper
        private readonly string _class_pathDisplayLabel = "path-display-label";
        private readonly string _class_pathSelectBtn = "path-select-btn";

        private readonly string _class_levelNameField = "config-levelname-field";
        private readonly string _class_soundtrackInfoField = "config-soundtrackinfo-field";
        private readonly string _class_sountrackField = "config-soundtrack-field";
        private readonly string _class_gemCountField = "config-gemcount-field";

        private readonly string _class_confirmBtn = "btn-confirm";
        private readonly string _class_cancelBtn = "btn-cancel";
        #endregion
        #endregion

        /// <summary>
        /// 初始化关卡创建控制器。
        /// </summary>
        /// <param name="visualTree">UI 布局模板</param>
        /// <param name="window">所属的编辑器窗口实例</param>
        public LevelCreatorController(VisualTreeAsset visualTree, LevelCreatorWindow window)
        {
            _visualTree = visualTree;
            _window = window;
        }

        /// <summary>
        /// 初始化 UI 元素并绑定事件回调。
        /// </summary>
        /// <param name="root">UI 根元素</param>
        public void Init(VisualElement root)
        {
            if (_visualTree == null) return;

            _visualTree.CloneTree(root);

            GetElements(root);
            InitElements();
            SubscribeEvents();

            SetDefaultPath();
        }

        /// <summary>
        /// 释放控制器资源，取消所有事件订阅。
        /// </summary>
        public void Dispose()
        {
            UnsubscribeEvents();
        }

        private void GetElements(VisualElement root)
        {
            _pathDisplayLabel = root.Q<Label>(className: _class_pathDisplayLabel);
            _pathSelectBtn = root.Q<Button>(className: _class_pathSelectBtn);

            _levelNameField = root.Q<TextField>(className: _class_levelNameField);
            _soundtrackInfoField = root.Q<TextField>(className: _class_soundtrackInfoField);
            _soundtrackField = root.Q<ObjectField>(className: _class_sountrackField);
            _gemCountField = root.Q<IntegerField>(className: _class_gemCountField);

            _confirmBtn = root.Q<Button>(className: _class_confirmBtn);
            _cancelBtn = root.Q<Button>(className: _class_cancelBtn);
        }

        private void InitElements()
        {
            _soundtrackField.objectType = typeof(AudioClip);
            _gemCountField.value = 10;
        }

        private void SubscribeEvents()
        {
            _pathSelectBtn.RegisterCallback<ClickEvent>(OnPathSelectClicked);
            _confirmBtn.RegisterCallback<ClickEvent>(OnConfirmClicked);
            _cancelBtn.RegisterCallback<ClickEvent>(OnCancelClicked);
        }

        private void UnsubscribeEvents()
        {
            _pathSelectBtn.UnregisterCallback<ClickEvent>(OnPathSelectClicked);
            _confirmBtn.UnregisterCallback<ClickEvent>(OnConfirmClicked);
            _cancelBtn.UnregisterCallback<ClickEvent>(OnCancelClicked);
        }

        private void SetDefaultPath()
        {
            var defaultPath = "Assets/DLSample/Levels";

            var fullPath = Path.Combine(Application.dataPath, "DLSample/Levels").Replace('\\', '/');
            if (Directory.Exists(fullPath))
            {
                _selectedBasePath = defaultPath;
                _pathDisplayLabel.text = _selectedBasePath;
            }
            else
            {
                _selectedBasePath = "Assets";
                _pathDisplayLabel.text = _selectedBasePath;
            }
        }

        private void OnPathSelectClicked(ClickEvent _)
        {
            var initialPath = Application.dataPath;
            if (!string.IsNullOrEmpty(_selectedBasePath) && _selectedBasePath.StartsWith("Assets"))
            {
                var relative = _selectedBasePath.Substring("Assets".Length);
                var testPath = Application.dataPath + relative;
                if (Directory.Exists(testPath))
                {
                    initialPath = testPath;
                }
            }

            var selectedFolder = EditorUtility.OpenFolderPanel("选择关卡保存位置", initialPath, "");
            if (string.IsNullOrEmpty(selectedFolder))
                return;

            var dataPath = Application.dataPath;
            if (!selectedFolder.StartsWith(dataPath))
            {
                EditorUtility.DisplayDialog("错误", "请选择 Assets 目录下的文件夹", "确定");
                return;
            }

            var relativePath = "Assets" + selectedFolder.Substring(dataPath.Length).Replace('\\', '/');
            _selectedBasePath = relativePath;
            _pathDisplayLabel.text = _selectedBasePath;
        }

        private void OnConfirmClicked(ClickEvent _)
        {
            var levelName = _levelNameField.value?.Trim();
            var soundtrackInfo = _soundtrackInfoField.value;
            var gemCount = _gemCountField.value;
            var soundtrackClip = _soundtrackField.value as AudioClip;

            if (string.IsNullOrEmpty(_selectedBasePath))
            {
                EditorUtility.DisplayDialog("错误", "请选择保存路径", "确定");
                return;
            }

            if (string.IsNullOrEmpty(levelName))
            {
                EditorUtility.DisplayDialog("错误", "请输入关卡名称", "确定");
                return;
            }

            var invalidChars = Path.GetInvalidFileNameChars();
            if (levelName.IndexOfAny(invalidChars) >= 0)
            {
                EditorUtility.DisplayDialog("错误", "关卡名称包含非法字符", "确定");
                return;
            }

            bool success = LevelCreatorHelper.CreateLevel(_selectedBasePath, levelName, soundtrackInfo, gemCount, soundtrackClip);
            if (success)
                _window.Close();
        }

        private void OnCancelClicked(ClickEvent _)
        {
            _window.Close();
        }
    }
}
#endif