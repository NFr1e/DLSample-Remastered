using DLSample.Shared;
using UnityEditor;
using UnityEngine;

namespace DLSample.Editor.ChartReader
{
    /// <summary>
    /// 支持的谱面格式类型。
    /// </summary>
    public enum ChartType
    {
        Osu
    }
    /// <summary>
    /// 谱面读取辅助类，负责解析外部谱面文件并将节拍数据写入 BeatmapDataScriptable。
    /// </summary>
    public static class ChartReaderHelper
    {
        /// <summary>
        /// 读取谱面文件并将节拍数据应用到目标 BeatmapDataScriptable 资源。
        /// </summary>
        /// <param name="target">目标 BeatmapDataScriptable 资源</param>
        /// <param name="chartFile">谱面文本文件</param>
        /// <param name="offset">时间偏移量（秒）</param>
        /// <param name="type">谱面格式类型</param>
        /// <returns>成功读取的节拍数量</returns>
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

            var reader = new OsuChartReader();
            var beats = reader.Read(chartFile.text, offset);
            Undo.RecordObject(target, "Read Chart");
            target.SetBeats(beats);

            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
            return beats.Length;
        }
    }
}
