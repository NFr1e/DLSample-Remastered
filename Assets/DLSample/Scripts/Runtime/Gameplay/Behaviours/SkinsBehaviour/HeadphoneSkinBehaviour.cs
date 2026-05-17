using DLSample.Facility.EnityFramework;
using UnityEngine;

namespace DLSample.Gameplay.Behaviours.Skin
{
    /// <summary>
    /// 耳机皮肤行为，在转向时播放射击粒子特效。
    /// </summary>
    public class HeadphoneSkinBehaviour : StretchTailSkinBehaviour
    {
        [SerializeField] private GameObject turnEffectPrefab;

        private EntityPool<ShotParticleEffect> _turnEffectPool;

        public override void OnApply()
        {
            base.OnApply();

            _turnEffectPool = new(turnEffectPrefab, 10, _effectsContainer);
            _turnEffectPool.Prewarm(5);
        }

        public override void OnDetach()
        {
            _turnEffectPool?.Dispose();

            base.OnDetach();
        }

        public override void OnPlayerTurn(PlayerMovingArgs arg)
        {
            base.OnPlayerTurn(arg);

            _ = PlayShotParticle(_turnEffectPool, arg.Position, Quaternion.identity, 1);
        }
    }
}
