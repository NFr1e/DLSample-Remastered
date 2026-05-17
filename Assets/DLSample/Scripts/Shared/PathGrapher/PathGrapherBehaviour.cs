#if UNITY_EDITOR
using UnityEngine;

namespace DLSample.Editor.PathGrapher
{
    /// <summary>
    /// 路径绘制器行为组件，用于在编辑器中触发路径模拟重建
    /// </summary>
    [ExecuteInEditMode]
    public class PathGrapherBehaviour : MonoBehaviour
    {
        public PathGrapherAsset asset;
        public PathGrapherProfile profile;

        private void OnValidate()
        {
            RequestRebuild();
        }

        /// <summary>
        /// 请求重新构建路径数据
        /// </summary>
        public void RequestRebuild()
        {
            if (asset != null)
            {
                PathSimulator.Simulate(asset, profile.samplingInterval);
            }
        }
    }
}
#endif