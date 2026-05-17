using Cysharp.Threading.Tasks;
using DLSample.Facility.Events;
using DLSample.Framework;
using DLSample.Gameplay.Phase;
using DLSample.Shared;
using System.Collections.Generic;

namespace DLSample.Gameplay
{
    /// <summary>
    /// 游戏准备就绪协调器，收集所有 IPrepareAsync 系统的准备任务，
    /// 在玩家触发开始后统一等待全部就绪，再发出 StartGameRequest 驱动状态转换
    /// </summary>
    public class GameplayReadinessCoordinator : IModule
    {
        public int Priority => DLSampleConsts.Gameplay.PRIORITY_READINESS_COORDINATOR;

        private readonly EventBus _evtBus;
        private readonly List<IPrepareAsync> _prepSystems;

        private bool _isPreparing;
        private GameplayStateBase _currentState;
        private readonly GameplayEventParams.StartGameRequest _startGameRequest = new();

        public GameplayReadinessCoordinator(EventBus eventBus, List<IPrepareAsync> prepSystems)
        {
            _evtBus = eventBus;
            _prepSystems = prepSystems ?? new();
        }

        public void OnInit()
        {
            _evtBus.Subscribe<GameplayEventParams.GameplayStateChangeCtx>(OnStateChange);
            _evtBus.Subscribe<GameplayEventParams.PrepareGameplayStartRequest>(OnPrepareGameplayStart);
        }

        public void OnShutdown()
        {
            _evtBus.Unsubscribe<GameplayEventParams.GameplayStateChangeCtx>(OnStateChange);
            _evtBus.Unsubscribe<GameplayEventParams.PrepareGameplayStartRequest>(OnPrepareGameplayStart);
        }

        private void OnStateChange(GameplayEventParams.GameplayStateChangeCtx ctx)
        {
            _currentState = ctx.CurrentState;
        }

        private async void OnPrepareGameplayStart(GameplayEventParams.PrepareGameplayStartRequest request)
        {
            if (_isPreparing) return;
            if (_currentState is not (GameplayStates.PreparingState or GameplayStates.PauseState)) return;

            _isPreparing = true;

            var tasks = new UniTask[_prepSystems.Count];
            for (int i = 0; i < _prepSystems.Count; i++)
            {
                tasks[i] = _prepSystems[i].PrepareAsync();
            }

            await UniTask.WhenAll(tasks);

            _isPreparing = false;

            if (_currentState is not (GameplayStates.PreparingState or GameplayStates.PauseState)) return;

            _evtBus.Invoke(this, _startGameRequest);
        }
    }
}
