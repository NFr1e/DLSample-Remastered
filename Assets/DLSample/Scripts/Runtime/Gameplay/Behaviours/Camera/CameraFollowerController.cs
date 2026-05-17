using DLSample.Gameplay.Phase;
using DLSample.Facility.Events;
using DLSample.Framework;
using DLSample.Shared;

namespace DLSample.Gameplay.Behaviours
{
    /// <summary>
    /// 相机跟随控制器，根据游戏状态控制相机的跟随行为，并支持回溯。
    /// </summary>
    public class CameraFollowerController : IModule, IBacktrackable, IModuleRequire<BacktrackablesHandler>
    {
        public int Priority { get; set; }

        private readonly EventBus _evtBus;

        private CameraFollower _follower;
        private BacktrackablesHandler _backtrackHandler;

        private bool _follow = false;

        /// <summary>
        /// 构造相机跟随控制器。
        /// </summary>
        /// <param name="eventBus">事件总线。</param>
        public CameraFollowerController(EventBus eventBus)
        {
            _evtBus = eventBus;
        }

        public void OnInit()
        {
            RegisterEvents();

            _backtrackHandler?.Register(this);
        }

        public void OnShutdown()
        {
            UnregisterEvents();

            _backtrackHandler?.Unregister(this);
        }

        public void OnUpdate(float deltaTime)
        {
            if (_follower && _follow)
            {
                _follower.Follow();
            }
        }

        private void RegisterEvents()
        {
            _evtBus?.Subscribe<GameplayEventParams.GameplayStateChangeCtx>(OnStateChange);
        }

        private void UnregisterEvents()
        {
            _evtBus?.Unsubscribe<GameplayEventParams.GameplayStateChangeCtx>(OnStateChange);
        }

        private void OnStateChange(GameplayEventParams.GameplayStateChangeCtx ctx)
        {
            _follow = ctx.CurrentState is GameplayStates.GamingState;
        }

        /// <summary>
        /// 切换相机跟随目标。
        /// </summary>
        /// <param name="follower">新的相机跟随组件。</param>
        public void ChangeFollower(CameraFollower follower)
        {
            if (follower != null)
                _follower = follower;
        }

        public int BacktrackPriority => DLSampleConsts.Gameplay.BACKTRACK_PRIORITY_CAMERA_FOLLOWER;

        /// <summary>
        /// 回溯时聚焦当前目标。
        /// </summary>
        public void Backtrack()
        {
            _ = _follower.FocusTarget();
        }

        /// <summary>
        /// 设置回溯处理器模块。
        /// </summary>
        /// <param name="backtrackableHandler">回溯处理器。</param>
        public void SetModule(BacktrackablesHandler backtrackableHandler)
        {
            _backtrackHandler = backtrackableHandler;
        }
    }
}
