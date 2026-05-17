using DLSample.Gameplay.Phase;
using DLSample.Facility.Events;
using DLSample.Framework;
using DLSample.Shared;

namespace DLSample.Gameplay.Stream
{
    public class GameplayTimerDirector : IModule, IBacktrackable
    {
        public int Priority => DLSampleConsts.Gameplay.PRIORITY_TIMER_DIRECTOR;

        private readonly GameplayTimer _timer;
        private readonly BacktrackablesHandler _backtrack;

        private readonly EventBus _evtBus;

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

        private void OnStateChange(GameplayEventParams.GameplayStateChangeCtx ctx)
        {
            switch (ctx.CurrentState)
            {
                case GameplayStates.GamingState:
                    _timer.Play();
                    break;
                default:
                    _timer.Stop();
                    break;
            }
        }

        public void Backtrack()
        {
            _timer.Seek(_backtrack.CurrentBacktrackTime);
        }
    }
}
