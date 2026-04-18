using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using DLSample.Shared;

namespace DLSample.Editor.ChartReader
{
    public class ChartReaderWindow : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        // UI 元素引用
        private ObjectField beatmapDataField;
        private ObjectField chartFileField;
        private Button readButton;

        [MenuItem(DLSampleConsts.Editor.MENU_ITEM_CHART_READER,
            priority = DLSampleConsts.Editor.MENU_ITEM_CHART_READER_PRIORITY)]
        public static void Open()
        {
            ChartReaderWindow wnd = GetWindow<ChartReaderWindow>();
            wnd.titleContent = new GUIContent("Chart Reader");
            wnd.minSize = new Vector2(350, 150);
        }

        public void CreateGUI()
        {
            m_VisualTreeAsset.CloneTree(rootVisualElement);

            beatmapDataField = rootVisualElement.Q<ObjectField>(className: "output-beatmapdata-field");
            chartFileField = rootVisualElement.Q<ObjectField>(className: "source-chartfile-field");
            readButton = rootVisualElement.Q<Button>(className: "output-read-btn");

            if (beatmapDataField != null)
                beatmapDataField.objectType = typeof(BeatmapDataScriptable);

            if (chartFileField != null)
                chartFileField.objectType = typeof(TextAsset);

            if (readButton != null)
                readButton.clicked += OnReadButtonClicked;
        }

        private void OnReadButtonClicked()
        {
            var beatmapData = beatmapDataField?.value as BeatmapDataScriptable;
            var chartFile = chartFileField?.value as TextAsset;

            if (beatmapData == null)
            {
                EditorUtility.DisplayDialog("错误", "请先选择一个 BeatmapDataScriptable 资源。", "确定");
                return;
            }

            if (chartFile == null)
            {
                EditorUtility.DisplayDialog("错误", "请先选择一个谱面文本文件。", "确定");
                return;
            }
            ChartReaderHelper.ReadAndApply(beatmapData, chartFile);

            EditorUtility.SetDirty(beatmapData);
            AssetDatabase.SaveAssets();
        }
    }
}
