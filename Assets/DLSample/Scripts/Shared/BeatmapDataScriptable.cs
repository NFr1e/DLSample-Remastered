using System;
using System.Collections.Generic;
using UnityEngine;

namespace DLSample.Shared
{
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

    [CreateAssetMenu(
        menuName = DLSampleConsts.Editor.CREATE_MENU_BEATMAPDATA_MENU_NAME,
        fileName = DLSampleConsts.Editor.CREATE_MENU_BEATMAPDATA_FILE_NAME,
        order = DLSampleConsts.Editor.CREATE_MENU_BEATMAPDATA_ORDER)]
    public class BeatmapDataScriptable : ScriptableObject
    {
        [SerializeField] private List<Beat> beats = new();

        public IReadOnlyList<Beat> Beats => beats.AsReadOnly();

        public void SetBeats(IReadOnlyList<Beat> beats)
        {
            this.beats ??= new();
            this.beats.Clear();
            this.beats.AddRange(beats);
        }
    }
}
