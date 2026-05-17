using UnityEngine;

namespace DLSample.Gameplay.Behaviours.Skin
{
    /// <summary>
    /// 皮肤行为基类，提供所有皮肤共用的虚方法生命周期钩子。
    /// </summary>
    public abstract class SkinBehaviourBase : MonoBehaviour
    {
        protected Transform _headContainer;

        /// <summary>
        /// 应用皮肤时调用。
        /// </summary>
        public abstract void OnApply();

        /// <summary>
        /// 卸下皮肤时调用。
        /// </summary>
        public abstract void OnDetach();

        /// <summary>
        /// 重置皮肤状态（用于回溯功能）。
        /// </summary>
        public virtual void OnReset()
        {
            Debug.Log($"{gameObject.name} : I'm Resetting!");
        }

        /// <summary>
        /// 玩家开始移动时调用。
        /// </summary>
        /// <param name="arg">移动参数。</param>
        public virtual void OnStartMove(PlayerMovingArgs arg)
        {
        }

        /// <summary>
        /// 玩家停止移动时调用。
        /// </summary>
        /// <param name="arg">移动参数。</param>
        public virtual void OnStopMove(PlayerMovingArgs arg)
        {
        }

        /// <summary>
        /// 玩家移动中持续调用。
        /// </summary>
        /// <param name="arg">移动参数。</param>
        public virtual void OnPlayerMoving(PlayerMovingArgs arg)
        {
        }

        /// <summary>
        /// 玩家着地时调用。
        /// </summary>
        /// <param name="arg">移动参数。</param>
        public virtual void OnPlayerLand(PlayerMovingArgs arg)
        {
        }

        /// <summary>
        /// 玩家转向时调用。
        /// </summary>
        /// <param name="arg">移动参数。</param>
        public virtual void OnPlayerTurn(PlayerMovingArgs arg)
        {
        }

        /// <summary>
        /// 玩家死亡时调用。
        /// </summary>
        /// <param name="arg">死亡事件参数。</param>
        public virtual void OnPlayerDie(PlayerEventsParams.PlayerDieArg arg)
        {
        }

        /// <summary>
        /// 设置头部容器Transform。
        /// </summary>
        /// <param name="headContainer">头部容器。</param>
        public void SetHeadContainer(Transform headContainer) => _headContainer = headContainer;
    }
}
