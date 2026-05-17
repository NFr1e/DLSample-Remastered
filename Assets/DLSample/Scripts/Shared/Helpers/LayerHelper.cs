using UnityEngine;

namespace DLSample.Shared
{
    /// <summary>
    /// 图层辅助工具类
    /// </summary>
    public static class LayerHelper
    {
        /// <summary>
        /// 判断GameObject是否属于指定的LayerMask
        /// </summary>
        /// <param name="go">目标GameObject</param>
        /// <param name="mask">图层遮罩</param>
        /// <returns>是否属于指定图层</returns>
        public static bool IsLayer(GameObject go, LayerMask mask)
        {
            return (mask.value & (1 << go.layer)) != 0;
        }
    }
}
