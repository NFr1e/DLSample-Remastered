using Cysharp.Threading.Tasks;
using DLSample.Facility.Events;
using DLSample.Framework;
using DLSample.Gameplay.Phase;
using DLSample.Shared;

namespace DLSample.Gameplay.Stream
{
    public class GameplaySoundtrackDirector : IModule, ISyncable, IBacktrackable
    {
        public int Priority => DLSampleConsts.Gameplay.PRIORITY_SOUNDTRACK_DIRECTOR;

        private readonly GameplaySoundtrackPlayer _soundtrackPlayer;
        private readonly BacktrackablesHandler _backtracksHandler;

        private readonly EventBus _evtBus;

        private bool _synced = false;
        private int _playRequestVersion = 0;
        private bool _isPreparingStart = false;
        private GameplayStateBase _currentState;
        private readonly GameplayEventParams.StartGameRequest _startGameRequest = new();

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
            _evtBus?.Subscribe<GameplayEventParams.PrepareGameplayStartRequest>(OnPrepareGameplayStart);
        }
        private void UnsubscribeEvents()
        {
            _evtBus?.Unsubscribe<GameplayEventParams.GameplayStateChangeCtx>(OnStateChange);
            _evtBus?.Unsubscribe<GameplayEventParams.PrepareGameplayStartRequest>(OnPrepareGameplayStart);
        }
        private void OnStateChange(GameplayEventParams.GameplayStateChangeCtx ctx)
        {
            _currentState = ctx.CurrentState;

            switch(ctx.CurrentState)
            {
                case GameplayStates.PreparingState:
                    Prepare();
                    break;
                case GameplayStates.GamingState:
                    Play();
                    break;
                case GameplayStates.PauseState:
                    Stop();
                    break;
                case GameplayStates.OverState:
                    Fadeout();
                    break;
            }
        }

        private async void OnPrepareGameplayStart(GameplayEventParams.PrepareGameplayStartRequest request)
        {
            if (_isPreparingStart) return;
            if (_currentState is not (GameplayStates.PreparingState or GameplayStates.PauseState)) return;

            _isPreparingStart = true;
            await PrepareAsync();
            _isPreparingStart = false;

            if (_currentState is not (GameplayStates.PreparingState or GameplayStates.PauseState)) return;

            _evtBus?.Invoke(this, _startGameRequest);
        }

        private void Prepare()
        {
            _soundtrackPlayer?.PrepareAsync().Forget();
        }
        private UniTask PrepareAsync()
        {
            return _soundtrackPlayer?.PrepareAsync() ?? UniTask.CompletedTask;
        }
        private async void Play()
        {
            int requestVersion = ++_playRequestVersion;

            if(!_synced)
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
            await UniTask.Delay(0);
        }

        public void Backtrack()
        {
            _soundtrackPlayer.Seek(_backtracksHandler.CurrentBacktrackTime);
        }
    }
}
