using DLSample.Shared;
using UnityEditor;
using UnityEngine;

namespace DLSample.Editor.ChartReader
{
    public enum ChartType
    {
        Osu
    }
    public static class ChartReaderHelper
    {
        public static int ReadAndApply(BeatmapDataScriptable target, TextAsset chartFile, float offset = 0, ChartType type = ChartType.Osu)
        {
            if (target == null)
            {
                return 0;
            }

            if (chartFile == null)
            {
                return 0;
            }

            IChartReader reader = new OsuChartReader();
            Beat[] beats = reader.Read(chartFile.text, offset);
            Undo.RecordObject(target, "Read Chart");
            target.SetBeats(beats);

            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
            return beats.Length;
        }
    }
}
