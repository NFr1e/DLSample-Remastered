using System;
using UnityEngine;
using DG.Tweening;

namespace DLSample.Gameplay.Behaviours
{
    /// <summary>
    /// 皇冠可收集物品，包含收集与消费的动画表现。
    /// </summary>
    public class Crown : MonoBehaviour, ICollectable
    {
        [SerializeField] private Renderer mRenderer;

        [SerializeField] private GameObject collectEffect;
        [SerializeField] private SpriteRenderer crownIcon;

        public event Action OnCollect;
        public string TypeId => "Collectables.Crown";
        public bool IsCollected { get; private set; } = false;

        private Tweener _iconTween;

        private void Awake()
        {
            IsCollected = false;
            crownIcon.DOFade(0, 0);
        }

        /// <summary>
        /// 收集皇冠，触发收集动画与事件。
        /// </summary>
        public void Collect()
        {
            if (IsCollected) return;

            OnCollected();

            OnCollect?.Invoke();
            IsCollected = true;
        }

        private void OnCollected()
        {
            mRenderer.enabled = false;
            AnimateCollectEffect();
        }

        /// <summary>
        /// 消费皇冠，播放消费动画。
        /// </summary>
        public void Consume()
        {
            AnimateConsume();
        }

        private void AnimateCollectEffect()
        {
            var effect = Instantiate(collectEffect, mRenderer.transform.position, Quaternion.identity, transform);

            var dist = Vector3.Distance(crownIcon.transform.position, mRenderer.transform.position);
            var duration = 1f;

            Vector3[] path = new[]
            {
                mRenderer.transform.position,
                (crownIcon.transform.position + mRenderer.transform.position) / 2 + Vector3.up * Mathf.Clamp(dist, 2, 10),
                crownIcon.transform.position,
            };

            effect.transform
                .DOPath(path, duration, PathType.Linear)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    _iconTween?.Kill();
                    _iconTween = crownIcon.DOFade(1, 1);

                    Destroy(effect, 0.1f);
                });
        }

        private void AnimateConsume()
        {
            var effect = Instantiate(collectEffect, crownIcon.transform.position, Quaternion.identity, transform);

            _iconTween?.Kill();
            _iconTween = crownIcon.DOFade(0, 1);

            effect.transform.DOLocalMoveY(effect.transform.position.y + 10, 1).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                Destroy(effect, 0.1f);
            });
        }
    }
}
