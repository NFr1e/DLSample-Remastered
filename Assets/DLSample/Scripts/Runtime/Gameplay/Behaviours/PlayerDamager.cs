using System;
using UnityEngine;
using DLSample.Facility.Events;
using DLSample.Shared;

namespace DLSample.Gameplay.Behaviours
{
    /// <summary>
    /// 玩家死亡原因枚举。
    /// </summary>
    public enum PlayerDiecause
    {
        None,
        Obstacle,
        Drown,
        Border
    }

    /// <summary>
    /// 玩家伤害处理器，检测碰撞和触发器并发送死亡事件。
    /// </summary>
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public class PlayerDamager : GameplayObject
    {
        public GameplayPlayerMove player;

        public LayerMask obstacleLayer;
        public LayerMask drownLayer;
        public LayerMask borderLayer;

        private EventBus _evtBus;
        private PlayerEventsParams.PlayerDieArg _dieArg = new();

        public event Action<PlayerEventsParams.PlayerDieArg> OnDie;

        protected override void OnInit()
        {
            _evtBus = GameplayEntry.Instance.ServiceLocator.Get<EventBus>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (LayerHelper.IsLayer(other.gameObject, drownLayer))
            {
                RequestDamage(PlayerDiecause.Drown);
            }
            if (LayerHelper.IsLayer(other.gameObject, borderLayer))
            {
                RequestDamage(PlayerDiecause.Border);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (LayerHelper.IsLayer(collision.gameObject, obstacleLayer))
            {
                RequestDamage(PlayerDiecause.Obstacle);
            }
        }

        /// <summary>
        /// 请求造成伤害，设置死亡原因并发送死亡事件。
        /// </summary>
        /// <param name="diecause">死亡原因。</param>
        public void RequestDamage(PlayerDiecause diecause)
        {
            _dieArg.DieCause = diecause;
            _dieArg.MovingArgs = player.MovingArgs;

            _evtBus.Invoke(this, _dieArg);
            OnDie?.Invoke(_dieArg);
        }
    }
}
