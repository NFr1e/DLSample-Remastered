using DLSample.Facility.EnityFramework;
using System.Collections.Generic;
using UnityEngine;

namespace DLSample.Gameplay.Behaviours.Skin
{
    /// <summary>
    /// 默认障碍物碰撞特效，包含可弹出碎片的物理效果。
    /// </summary>
    public class DefaultObstacledEffect : MonoBehaviour, IPoolabelEntity
    {
        [SerializeField] private List<Rigidbody> clips = new();

        public bool IsVaild { get; }

        /// <summary>
        /// 入池时调用，重置碎片并隐藏对象。
        /// </summary>
        public void OnEnpool()
        {
            ResetClips();
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 出池时调用，显示对象。
        /// </summary>
        public void OnDepool()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 设置可见性。
        /// </summary>
        /// <param name="visible">是否可见。</param>
        public void SetVisiblity(bool visible) { }

        private void ResetClips()
        {
            foreach (var item in clips)
            {
                item.isKinematic = true;
                item.useGravity = false;
                item.transform.localPosition = Vector3.zero;
            }
        }

        /// <summary>
        /// 触发碎片炸开物理效果。
        /// </summary>
        public void BoomClips()
        {
            foreach (var item in clips)
            {
                item.isKinematic = false;
                item.useGravity = true;
            }
        }
    }
}
