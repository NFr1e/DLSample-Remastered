using Cysharp.Threading.Tasks;
using DLSample.Gameplay.Phase;
using DLSample.Facility.Events;
using DLSample.Framework;
using DLSample.Shared;
using UnityEngine;

namespace DLSample.Gameplay.Stream
{
    public class GameplayTimerDirector : IModule, IBacktrackable, ISyncable
    {
        public int Priority => DLSampleConsts.Gameplay.PRIORITY_TIMER_DIRECTOR;

        private readonly GameplayTimer _timer;
        private readonly BacktrackablesHandler _backtrack;

        private readonly EventBus _evtBus;

        private bool _synced;

        public int BacktrackPriority => DLSampleConsts.Gameplay.BACKTRACK_PRIORITY_TIMER;

        public GameplayTimerDirector(EventBus eventBus, GameplayTimer timer, BacktrackablesHandler backtrack)
        {
            _evtBus = eventBus;
            _timer = timer;
            _backtrack = backtrack;
        }

        public void OnInit()
        {
            _evtBus.Subscribe<GameplayEventParams.GameplayStateChangeCtx>(OnStateChange);

            _backtrack?.Register(this);
        }
        public void OnShutdown()
        {
            _evtBus.Unsubscribe<GameplayEventParams.GameplayStateChangeCtx>(OnStateChange);

            _backtrack?.Unregister(this);
        }
        public void OnUpdate(float deltaTime)
        {
            _timer.Tick(deltaTime);
        }

        private async void OnStateChange(GameplayEventParams.GameplayStateChangeCtx ctx)
        {
            try
            {
                switch (ctx.CurrentState)
                {
                    case GameplayStates.GamingState:
                        if (!_synced)
                            await SyncDelay();
                        _timer.Play();
                        break;
                    default:
                        _timer.Stop();
                        break;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        public async UniTask SyncDelay()
        {
            _synced = true;

            var delay = PlayerPrefs.GetFloat(DLSampleConsts.SaveAndLoad.ID_SYNC_DELAY, 0f);
            if (delay < 0f)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(-delay));
            }
        }

        public void Backtrack()
        {
            _synced = false;
            _timer.Seek(_backtrack.CurrentBacktrackTime);
        }
    }
}
