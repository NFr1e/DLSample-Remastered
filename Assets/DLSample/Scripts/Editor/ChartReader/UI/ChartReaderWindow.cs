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

            beatmapDataField = rootVisualElement.Q<ObjectField>("BeatmapdataField");
            chartFileField = rootVisualElement.Q<ObjectField>("SourceField");
            readButton = rootVisualElement.Q<Button>("GenerateHintLineBtn");

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
                EditorUtility.DisplayDialog("Error", "Please select a BeatmapDataScriptable asset first.", "OK");
                return;
            }

            if (chartFile == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a chart text file first.", "OK");
                return;
            }

            int beatCount = ChartReaderHelper.ReadAndApply(beatmapData, chartFile);
            EditorUtility.DisplayDialog("Read Complete", $"Wrote {beatCount} beats.", "OK");
        }
    }
}
