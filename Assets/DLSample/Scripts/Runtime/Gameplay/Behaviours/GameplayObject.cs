using UnityEngine;

namespace DLSample.Gameplay.Behaviours
{
    /// <summary>
    /// Gameplay 对象抽象基类，管理在 GameplayEntry 中的注册与注销生命周期。
    /// </summary>
    public abstract class GameplayObject : MonoBehaviour
    {
        private bool _isDestroyed = false;
        private GameplayEntry _entry;

        private void Awake()
        {
            OnInit();

            _entry = GameplayEntry.Instance;
            _entry.RegisterGameplayObject(this);
        }

        private void OnDestroy()
        {
            if (!_isDestroyed)
            {
                if (_entry)
                {
                    _entry.UnregisterGameplayObject(this);
                }

                OnExit();
            }

            _isDestroyed = true;
        }

        /// <summary>
        /// 由 GameplayEntry 调用的启动方法。
        /// </summary>
        public void DoStart() => OnStart();

        /// <summary>
        /// Awake 时调用，用于初始化操作。
        /// </summary>
        protected virtual void OnInit() { }

        /// <summary>
        /// 当 GameplayEntry 准备就绪时调用，所有模块已完成创建和注册，此时可以安全执行模块间注册和访问其他模块等工作。
        /// </summary>
        protected virtual void OnStart() { }

        /// <summary>
        /// 对象销毁时调用，用于执行清理操作。
        /// </summary>
        protected virtual void OnExit() { }
    }
}
