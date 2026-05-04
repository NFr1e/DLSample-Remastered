using DLSample.Shared;
using DLSample.Framework;
using DLSample.Facility.Events;

namespace DLSample.Gameplay.Phase
{
    public class GameplayStateHandler : IModule
    {
        private readonly GameplayFSM _fsm;

        private readonly EventBus _evtBus;

        public bool IsGameStarted { get; private set; } = false;
        public bool IsGameWin { get; private set; } = false;

        public int Priority => DLSampleConsts.Gameplay.PRIORITY_STATE_HANDLER;

        public GameplayStateHandler(EventBus eventBus, GameplayFSM stateMachine)
        {
            _evtBus = eventBus;
            _fsm = stateMachine;
        }
        public void OnInit()
        {
            RegisterEvents();
        }
        public void OnShutdown()
        {
            UnregisterEvents();
        }
        public void OnUpdate(float deltaTime) 
        { 
            _fsm.Update(deltaTime);
        }

        private void RegisterEvents()
        {
            _evtBus.Subscribe<GameplayEventParams.WaitingGameRequest>(OnRequestGameWaiting);
            _evtBus.Subscribe<GameplayEventParams.PrepareGameRequest>(OnRequestGameReady);
            _evtBus.Subscribe<GameplayEventParams.StartGameRequest>(OnRequestGameStart);
            _evtBus.Subscribe<GameplayEventParams.PauseGameRequest>(OnRequestGamePause);
            _evtBus.Subscribe<GameplayEventParams.RespawnGameRequest>(OnRequestRespawn);
            _evtBus.Subscribe<GameplayEventParams.ExitGameRequest>(OnRequestExit);
            _evtBus.Subscribe<GameplayEventParams.WinGameRequest>(OnRequestWin);
            _evtBus.Subscribe<PlayerEventsParams.PlayerDieArg>(OnPlayerDie);
        }
        private void UnregisterEvents()
        {
            _evtBus.Unsubscribe<GameplayEventParams.WaitingGameRequest>(OnRequestGameWaiting);
            _evtBus.Unsubscribe<GameplayEventParams.PrepareGameRequest>(OnRequestGameReady);
            _evtBus.Unsubscribe<GameplayEventParams.StartGameRequest>(OnRequestGameStart);
            _evtBus.Unsubscribe<GameplayEventParams.PauseGameRequest>(OnRequestGamePause);
            _evtBus.Unsubscribe<GameplayEventParams.RespawnGameRequest>(OnRequestRespawn);
            _evtBus.Unsubscribe<GameplayEventParams.ExitGameRequest>(OnRequestExit);
            _evtBus.Unsubscribe<GameplayEventParams.WinGameRequest>(OnRequestWin);
            _evtBus.Unsubscribe<PlayerEventsParams.PlayerDieArg>(OnPlayerDie);
        }

        private void OnRequestGameWaiting(GameplayEventParams.WaitingGameRequest request)
        {
            _fsm.SetCurrentState<GameplayStates.WaitingState>();
        }
        private void OnRequestGameReady(GameplayEventParams.PrepareGameRequest request)
        {
            _fsm.SetCurrentState<GameplayStates.PreparingState>();
        }
        private void OnRequestGameStart(GameplayEventParams.StartGameRequest request)
        {
            _fsm.SetCurrentState<GameplayStates.GamingState>();
            IsGameStarted = true;
        }
        private void OnRequestGamePause(GameplayEventParams.PauseGameRequest request)
        {
            if (_fsm.CurrentState is not GameplayStates.GamingState) return;

            _fsm.SetCurrentState<GameplayStates.PauseState>();
        }
        private void OnRequestRespawn(GameplayEventParams.RespawnGameRequest request)
        {
            _fsm.SetCurrentState<GameplayStates.RespawnState>();
        }
        private void OnRequestExit(GameplayEventParams.ExitGameRequest request)
        {
            _fsm.SetCurrentState<GameplayStates.Exiting>();
        }
        private void OnRequestWin(GameplayEventParams.WinGameRequest request)
        {
            IsGameWin = true;
            _fsm.SetCurrentState<GameplayStates.OverState>();
        }
        private void OnPlayerDie(PlayerEventsParams.PlayerDieArg arg)
        {
            _fsm.SetCurrentState<GameplayStates.OverState>();
        }
    }
}
