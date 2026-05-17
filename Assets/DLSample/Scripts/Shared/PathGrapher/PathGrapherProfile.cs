using UnityEngine;

namespace DLSample.Editor.PathGrapher
{
    /// <summary>
    /// 路径绘制器配置，包含模拟参数和绘制选项
    /// </summary>
    [System.Serializable]
    public class PathGrapherProfile
    {
        [Header("Simulation")]
        [Range(0.01f, 0.5f)]
        public float samplingInterval = 0.1f;

        [Header("DrawOptions")]
        public Color pathColor = Color.cyan;

        [Space(10)]
        public bool drawWaypointLabel = false;
        public Color labelTexColor = Color.white;
        public Color labelBgClor = new(0, 0, 0, 0.5f);

        [Space(10)]
        public bool drawEvents = true;
        public bool drawEventLabel = true;

        [Space(10)]
        public bool drawAlways = true;
        public bool zTest = true;
        public float pathDrawDistance = 128f;
        public float labelDrawDistance = 64f;
    }
}
