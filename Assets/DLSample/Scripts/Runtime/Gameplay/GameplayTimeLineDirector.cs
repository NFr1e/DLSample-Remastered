using Cysharp.Threading.Tasks;
using DLSample.Gameplay.Phase;
using DLSample.Facility.Events;
using DLSample.Framework;
using DLSample.Shared;

namespace DLSample.Gameplay.Stream
{
    public class GameplayTimeLineDirector : IModule, ISyncable, IBacktrackable, IModuleRequire<BacktrackablesHandler>
    {
        int IModule.Priority => DLSampleConsts.Gameplay.PRIORITY_TIMER_DIRECTOR;
        int IBacktrackable.BacktrackPriority => DLSampleConsts.Gameplay.BACKTRACK_PRIORITY_TIMER_DIRECTOR;

        private readonly IStreamPlayer _timelinePlayer;

        private EventBus _evtBus;
        private BacktrackablesHandler _backtrackablesHandler;

        private bool _synced = false;

        public GameplayTimeLineDirector(IStreamPlayer player, EventBus evtBus)
        {
            _timelinePlayer = player;
            _evtBus = evtBus;
        }

        public void OnInit()
        {
            Subscribe();

            _timelinePlayer.Seek(_timelinePlayer.CurrentTime);
        }

        public void OnShutdown()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            _evtBus?.Subscribe<GameplayEventParams.GameplayStateChangeCtx>(OnStateChange);
            _backtrackablesHandler.Register(this);
        }
        private void Unsubscribe()
        {
            _evtBus?.Unsubscribe<GameplayEventParams.GameplayStateChangeCtx>(OnStateChange);
            _backtrackablesHandler.Unregister(this);
        }
        private void OnStateChange(GameplayEventParams.GameplayStateChangeCtx ctx)
        {
            switch (ctx.CurrentState)
            {
                case GameplayStates.GamingState:
                    Play();
                    break;
                case GameplayStates.PauseState or GameplayStates.OverState:
                    Stop();
                    break;
            }
        }

        private async void Play()
        {
            if (!_synced)
                await SyncDelay();

            _timelinePlayer?.Play();
        }
        private void Stop()
        {
            _timelinePlayer?.Stop();
        }

        public async UniTask SyncDelay()
        {
            _synced = true;
            await UniTask.Delay(0);
        }

        public void Backtrack()
        {
            _timelinePlayer.Seek(_backtrackablesHandler.CurrentBacktrackTime);
        }
        public void SetModule(BacktrackablesHandler b)
        {
            _backtrackablesHandler = b;
        }
    }
}
