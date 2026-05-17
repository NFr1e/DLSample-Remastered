using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using DLSample.Shared;

namespace DLSample.Editor.ChartReader
{
    /// <summary>
    /// 谱面读取器编辑器窗口，用于从外部谱面文件（如 osu 格式）读取节拍数据并写入 BeatmapDataScriptable。
    /// </summary>
    public class ChartReaderWindow : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        private ObjectField _beatmapDataField;
        private ObjectField _chartFileField;
        private Button _readButton;

        /// <summary>
        /// 打开谱面读取器窗口。
        /// </summary>
        [MenuItem(DLSampleConsts.Editor.MENU_ITEM_CHART_READER,
            priority = DLSampleConsts.Editor.MENU_ITEM_CHART_READER_PRIORITY)]
        public static void Open()
        {
            var wnd = GetWindow<ChartReaderWindow>();
            wnd.titleContent = new GUIContent("Chart Reader");
            wnd.minSize = new Vector2(350, 150);
        }

        public void CreateGUI()
        {
            m_VisualTreeAsset.CloneTree(rootVisualElement);

            _beatmapDataField = rootVisualElement.Q<ObjectField>("BeatmapdataField");
            _chartFileField = rootVisualElement.Q<ObjectField>("SourceField");
            _readButton = rootVisualElement.Q<Button>("GenerateHintLineBtn");

            if (_beatmapDataField != null)
                _beatmapDataField.objectType = typeof(BeatmapDataScriptable);

            if (_chartFileField != null)
                _chartFileField.objectType = typeof(TextAsset);

            if (_readButton != null)
                _readButton.clicked += OnReadButtonClicked;
        }

        private void OnReadButtonClicked()
        {
            var beatmapData = _beatmapDataField?.value as BeatmapDataScriptable;
            var chartFile = _chartFileField?.value as TextAsset;

            if (beatmapData == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a BeatmapDataScriptable asset first.", "OK");
                return;
            }

            if (chartFile == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a chart text file first.", "OK");
                return;
            }

            var beatCount = ChartReaderHelper.ReadAndApply(beatmapData, chartFile);
            EditorUtility.DisplayDialog("Read Complete", $"Wrote {beatCount} beats.", "OK");
        }
    }
}
