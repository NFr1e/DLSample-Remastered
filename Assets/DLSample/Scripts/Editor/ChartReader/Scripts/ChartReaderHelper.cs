using DLSample.Shared;
using UnityEngine;

namespace DLSample.Editor.ChartReader
{
    public enum ChartType
    {
        Osu
    }
    public static class ChartReaderHelper
    {
        public static void ReadAndApply(BeatmapDataScriptable target, TextAsset chartFile, float offset = 0, ChartType type = ChartType.Osu)
        {
            if (target == null)
            {
                return;
            }

            if (chartFile == null)
            {
                return;
            }

            IChartReader reader = new OsuChartReader();
            Beat[] beats = reader.Read(chartFile.text, offset);
            target.SetBeats(beats);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(target);
#endif
        }
    }
}
