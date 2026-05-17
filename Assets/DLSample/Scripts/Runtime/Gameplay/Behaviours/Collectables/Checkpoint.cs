using UnityEngine;
using DLSample.Gameplay.Stream;
using DLSample.Facility.Events;

namespace DLSample.Gameplay.Behaviours
{
    /// <summary>
    /// 消费检查点事件参数。
    /// </summary>
    public struct OnConsumeCheckpoint : IEventArg
    {
        /// <summary>
        /// 被消费的检查点。
        /// </summary>
        public Checkpoint checkpoint;
    }

    /// <summary>
    /// 检查点基类，管理基于时间的触发检查与消费逻辑。
    /// </summary>
    public class Checkpoint : GameplayObject
    {
        [SerializeField] protected double checkTime = 0;

        [SerializeField] private Transform mainPlayerRespawnTransform;
        [SerializeField] private bool visulize = false;

        protected bool _consumed = false;

        private GameplayTimer _timer;
        private GameplayTimer.TickEvent _checkTickEvent;

        private OnConsumeCheckpoint _consumedEvent = new();

        /// <summary>
        /// 玩家重生位置。
        /// </summary>
        public Transform MainPlayerTransform => mainPlayerRespawnTransform;

        /// <summary>
        /// 检查时间点。
        /// </summary>
        public double CheckTime => checkTime;

        protected override void OnStart()
        {
            _checkTickEvent = new(checkTime, Check);

            _timer = GameplayEntry.Instance.ServiceLocator.Get<GameplayTimer>();
            _timer.RegisterTickEvent(_checkTickEvent);
        }

        protected override void OnExit()
        {
            _timer?.UnregisterTickEvent(_checkTickEvent);
        }

        /// <summary>
        /// 执行检查逻辑，将自身注册到 CheckpointHandler。
        /// </summary>
        protected virtual void Check()
        {
            if (GameplayEntry.Instance.ServiceLocator.TryGet<CheckpointHandler>(out var cpHnadler))
            {
                cpHnadler.Check(this);
            }
        }

        /// <summary>
        /// 消费检查点，标记已消费并发送事件。
        /// </summary>
        public virtual void Consume()
        {
            _consumed = true;

            _consumedEvent.checkpoint = this;
            GameplayEntry.Instance.ServiceLocator.Get<EventBus>().Invoke<OnConsumeCheckpoint>(this, _consumedEvent);
        }

        private void OnDrawGizmos()
        {
            if (mainPlayerRespawnTransform && visulize)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(mainPlayerRespawnTransform.position + Vector3.up * 2, 0.2f);
                Gizmos.DrawLine(mainPlayerRespawnTransform.position + Vector3.up * 2,
                    mainPlayerRespawnTransform.position + mainPlayerRespawnTransform.rotation * Vector3.forward + Vector3.up * 2);
                Gizmos.DrawCube(mainPlayerRespawnTransform.position, Vector3.one);
            }
        }
    }
}
