using System;
using UnityEngine;
using DLSample.Facility;
using DLSample.Shared;

namespace DLSample.Gameplay.Behaviours
{
    /// <summary>
    /// 宝石可收集物品，支持收集动画与回溯恢复。
    /// </summary>
    public class Gem : GameplayObject, ICollectable, IBacktrackable
    {
        [SerializeField] private Renderer mRenderer;
        [SerializeField] private GameObject collectEffect;
        [SerializeField] private float effectLifetime = 10;

        public event Action OnCollect;
        public string TypeId => "Collectables.Gem";
        public bool IsCollected { get; private set; } = false;

        private GameObject _currentEffectInstance;
        private BacktrackablesHandler _backtrackablesHandler;

        protected override void OnInit()
        {
            IsCollected = false;
        }

        protected override void OnStart()
        {
            _backtrackablesHandler = GameplayEntry.Instance.ServiceLocator.Get<BacktrackablesHandler>();
            RegisterBacktrack();
        }

        protected override void OnExit()
        {
            UnregisterBacktrack();
        }

        /// <summary>
        /// 收集宝石，隐藏渲染并播放收集特效。
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

            if (_currentEffectInstance)
            {
                Destroy(_currentEffectInstance);
            }
            _currentEffectInstance = Instantiate(collectEffect, transform.position, Quaternion.identity, transform);
            Destroy(_currentEffectInstance, effectLifetime);
        }

        public int BacktrackPriority => DLSampleConsts.Gameplay.BACKTRACK_PRIORITY_COLLECTABLE;

        /// <summary>
        /// 回溯恢复，重置收集状态与渲染可见性。
        /// </summary>
        public void Backtrack()
        {
            IsCollected = false;
            mRenderer.enabled = true;

            Destroy(_currentEffectInstance);
        }

        private void RegisterBacktrack()
        {
            _backtrackablesHandler.Register(this);
        }

        private void UnregisterBacktrack()
        {
            _backtrackablesHandler.Unregister(this);
        }
    }
}
