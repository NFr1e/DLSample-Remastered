using UnityEngine;

namespace DLSample.Gameplay.Behaviours
{
    /// <summary>
    /// 提示线分段，控制单个分段的可见时间与显隐状态。
    /// </summary>
    public class HintLineSegment : MonoBehaviour
    {
        [field: SerializeField]
        public float DisappearTime { get; private set; }

        /// <summary>
        /// 初始化分段，设置消失时间（减0.3秒缓冲）。
        /// </summary>
        /// <param name="disappearTime">原始消失时间。</param>
        public void Initialize(float disappearTime)
        {
            DisappearTime = Mathf.Max(disappearTime - 0.3f, 0);
        }

        /// <summary>
        /// 设置分段的可见性。
        /// </summary>
        /// <param name="isVisible">是否可见。</param>
        public void SetVisible(bool isVisible)
        {
            if (gameObject.activeSelf == isVisible) return;

            gameObject.SetActive(isVisible);
        }

        /// <summary>
        /// 根据当前时间刷新可见性。
        /// </summary>
        /// <param name="currentTime">当前游戏时间。</param>
        public void RefreshVisibility(double currentTime)
        {
            SetVisible(currentTime < DisappearTime);
        }
    }
}
