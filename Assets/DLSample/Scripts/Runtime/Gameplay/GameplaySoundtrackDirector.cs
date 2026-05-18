using Cysharp.Threading.Tasks;
using DLSample.Facility.Events;
using DLSample.Framework;
using DLSample.Gameplay.Phase;
using DLSample.Shared;
using UnityEngine;

namespace DLSample.Gameplay.Stream
{
    public class GameplaySoundtrackDirector : IModule, ISyncable, IBacktrackable, IPrepareAsync
    {
        public int Priority => DLSampleConsts.Gameplay.PRIORITY_SOUNDTRACK_DIRECTOR;

        private readonly GameplaySoundtrackPlayer _soundtrackPlayer;
        private readonly BacktrackablesHandler _backtracksHandler;

        private readonly EventBus _evtBus;

        private bool _synced = false;
        private int _playRequestVersion = 0;
        private GameplayStateBase _currentState;

        public int BacktrackPriority => DLSampleConsts.Gameplay.BACKTRACK_PRIORITY_SOUNDTRACK_DIRECTOR;

        public GameplaySoundtrackDirector(EventBus eventBus, GameplaySoundtrackPlayer player, BacktrackablesHandler backtrackHandler)
        {
            _evtBus = eventBus;
            _soundtrackPlayer = player;
            _backtracksHandler = backtrackHandler;
        }

        public void OnInit()
        {
            _soundtrackPlayer.Init();

            SubscribeEvents();
            _backtracksHandler?.Register(this);
        }

        public void OnShutdown()
        {
            UnsubscribeEvents();
            _backtracksHandler?.Unregister(this);

            _soundtrackPlayer.Dispose();
        }
        private void SubscribeEvents()
        {
            _evtBus?.Subscribe<GameplayEventParams.GameplayStateChangeCtx>(OnStateChange);
        }
        private void UnsubscribeEvents()
        {
            _evtBus?.Unsubscribe<GameplayEventParams.GameplayStateChangeCtx>(OnStateChange);
        }
        private void OnStateChange(GameplayEventParams.GameplayStateChangeCtx ctx)
        {
            _currentState = ctx.CurrentState;

            switch (ctx.CurrentState)
            {
                case GameplayStates.PreparingState:
                    Prepare();
                    break;
                case GameplayStates.GamingState:
                    _ = Play();
                    break;
                case GameplayStates.PauseState:
                    Stop();
                    break;
                case GameplayStates.OverState:
                    Fadeout();
                    break;
            }
        }

        private void Prepare()
        {
            _soundtrackPlayer?.PrepareAsync().Forget();
        }

        UniTask IPrepareAsync.PrepareAsync()
        {
            return _soundtrackPlayer?.PrepareAsync() ?? UniTask.CompletedTask;
        }
        private async UniTaskVoid Play()
        {
            int requestVersion = ++_playRequestVersion;

            if (!_synced)
                await SyncDelay();

            if (requestVersion != _playRequestVersion) return;

            _soundtrackPlayer?.Play();
        }
        private void Stop()
        {
            _playRequestVersion++;
            _soundtrackPlayer?.Stop();
        }
        private void Fadeout()
        {
            _playRequestVersion++;
            _soundtrackPlayer?.Fadeout();
        }
        public async UniTask SyncDelay()
        {
            _synced = true;

            var delay = PlayerPrefs.GetFloat(DLSampleConsts.SaveAndLoad.ID_SYNC_DELAY, 0f);
            if (delay > 0f)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(delay));
            }
        }

        public void Backtrack()
        {
            _synced = false;
            _soundtrackPlayer.Seek(_backtracksHandler.CurrentBacktrackTime);
        }
    }
}
