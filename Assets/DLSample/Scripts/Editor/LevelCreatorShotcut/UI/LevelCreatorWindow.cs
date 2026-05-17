using DLSample.Shared;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DLSample.Editor.LevelCreator
{
    /// <summary>
    /// 关卡创建编辑器窗口，为关卡创建流程提供图形化界面。
    /// </summary>
    public class LevelCreatorWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;

        private LevelCreatorController _view;

        /// <summary>
        /// 打开关卡创建窗口。
        /// </summary>
        [MenuItem(
            itemName: DLSampleConsts.Editor.MENU_ITEM_CREATE_LEVEL,
            priority = DLSampleConsts.Editor.MENU_ITEM_CREATE_LEVEL_PRIORITY)]
        public static void OpenWindow()
        {
            var window = GetWindow<LevelCreatorWindow>();
            window.titleContent = new GUIContent("Level Creator");
            window.minSize = new(600, 300);
            window.Show();
        }

        public void CreateGUI()
        {
            if (m_VisualTreeAsset == null)
            {
                Debug.LogError("[LevelCreator] VisualTreeAsset is not assigned!");
                return;
            }

            Initialize();
        }

        private void OnDestroy()
        {
            Dispose();
        }

        private void Initialize()
        {
            _view = new LevelCreatorController(m_VisualTreeAsset, this);
            _view.Init(rootVisualElement);
        }

        private void Dispose()
        {
            _view?.Dispose();
        }
    }
}
