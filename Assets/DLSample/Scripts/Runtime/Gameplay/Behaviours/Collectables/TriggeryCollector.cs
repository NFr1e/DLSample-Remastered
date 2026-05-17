using DLSample.Facility.Events;
using DLSample.Shared;
using UnityEngine;

namespace DLSample.Gameplay.Behaviours
{
    /// <summary>
    /// 触发器收集器，玩家进入触发器时自动收集可收集物品。
    /// </summary>
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public class TriggeryCollector : GameplayObject, ICollector
    {
        [SerializeField] private LayerMask excludeLayers;

        private EventBus _evtBus;
        private OnCollectEventArgs _onCollectEventArgs;

        protected override void OnStart()
        {
            _evtBus = GameplayEntry.Instance.EventBus;

            _onCollectEventArgs = new OnCollectEventArgs()
            {
                collector = this,
            };
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<ICollectable>(out var collectable) && !LayerHelper.IsLayer(other.gameObject, excludeLayers))
            {
                Collect(collectable);
            }
        }

        /// <summary>
        /// 收集指定的可收集物品，并发送收集事件。
        /// </summary>
        /// <param name="collectable">待收集的物品。</param>
        public void Collect(ICollectable collectable)
        {
            collectable.Collect();

            _onCollectEventArgs.collectable = collectable;
            _evtBus?.Invoke(this, _onCollectEventArgs);
        }
    }
}
