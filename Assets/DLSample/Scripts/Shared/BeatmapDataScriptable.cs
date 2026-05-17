using System;
using System.Collections.Generic;
using UnityEngine;

namespace DLSample.Shared
{
    /// <summary>
    /// 节拍数据结构，包含单个节拍的时间信息
    /// </summary>
    [Serializable]
    public struct Beat
    {
        [SerializeField] private double timeSecond;

        public readonly double TimeSecond => timeSecond;

        public Beat(double time)
        {
            timeSecond = time;
        }
    }

    /// <summary>
    /// 节拍数据配置，保存关卡中所有节拍的时间信息
    /// </summary>
    [CreateAssetMenu(
        menuName = DLSampleConsts.Editor.CREATE_MENU_BEATMAPDATA_MENU_NAME,
        fileName = DLSampleConsts.Editor.CREATE_MENU_BEATMAPDATA_FILE_NAME,
        order = DLSampleConsts.Editor.CREATE_MENU_BEATMAPDATA_ORDER)]
    public class BeatmapDataScriptable : ScriptableObject
    {
        [SerializeField] private List<Beat> beats = new();

        public IReadOnlyList<Beat> Beats => beats.AsReadOnly();

        /// <summary>
        /// 设置节拍列表
        /// </summary>
        /// <param name="beats">新的节拍数据列表</param>
        public void SetBeats(IReadOnlyList<Beat> beats)
        {
            this.beats ??= new();
            this.beats.Clear();
            this.beats.AddRange(beats);
        }
    }
}
