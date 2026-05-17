using UnityEngine;
using DLSample.Facility.EnityFramework;

namespace DLSample.Gameplay.Behaviours.Skin
{
    /// <summary>
    /// 射击粒子特效，可入池回收。
    /// </summary>
    public class ShotParticleEffect : MonoBehaviour, IPoolabelEntity
    {
        [SerializeField] private ParticleSystem particle;

        public ParticleSystem Particle => particle;

        /// <summary>
        /// 入池时调用，停止粒子并隐藏对象。
        /// </summary>
        public void OnEnpool()
        {
            particle.Stop();
            particle.gameObject.SetActive(false);
        }

        /// <summary>
        /// 出池时调用，显示对象。
        /// </summary>
        public void OnDepool()
        {
            particle.gameObject.SetActive(true);
        }

        /// <summary>
        /// 设置可见性。
        /// </summary>
        /// <param name="visible">是否可见。</param>
        public void SetVisiblity(bool visible)
        {
        }

        public bool IsVaild { get; set; }
    }
}
