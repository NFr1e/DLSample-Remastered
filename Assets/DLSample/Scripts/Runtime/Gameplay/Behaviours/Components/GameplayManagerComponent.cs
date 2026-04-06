using UnityEngine;
using DLSample.Shared;
using DLSample.Gameplay.Phase;
using DLSample.Gameplay.Stream;
using DLSample.Facility.Events;
using DLSample.Facility;
using DLSample.Framework;

namespace DLSample.Gameplay.Behaviours
{
    public class GameplayManagerComponent : GameplayObject
    {
        [SerializeField] private LevelDataScriptable levelData;

        [Space(10)]
        [SerializeField] private GameplayPlayerMove mainPlayer;

        [Space(10)]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip audioClip;

        private GameplayFSM _fsm;
        private GameplayStateHandler _stateHandler;

        private BacktrackablesHandler _backtrackHandler;
        private CheckpointHandler _checkpointHandler;

        private GameplayTimer _timer;
        private GameplayTimerDirector _timerDirector;

        private GameplayPlayerController _playerController;
        private GameplayInputHandler _inputHandler;

        private GameplaySoundtrackPlayer _soundtrackPlayer;
        private GameplaySoundtrackDirector _soundtrackDirector;

        private GameplayResulter _resulter;

        private GameplayInitPipeline _initializer;

        private EventBus eventBus;
        private ServiceLocator serviceLocator;
        private ModulesManager modulesManager;

        protected override void OnInit()
        {
            eventBus = GameplayEntry.Instance.EventBus;
            serviceLocator = GameplayEntry.Instance.ServiceLocator;
            modulesManager = GameplayEntry.Instance.ModulesManager;

            _fsm = new GameplayFSM();
            _stateHandler = new GameplayStateHandler(eventBus, _fsm);

            _backtrackHandler = new BacktrackablesHandler();
            _checkpointHandler = new CheckpointHandler(eventBus);

            _timer = new GameplayTimer();
            _timerDirector = new GameplayTimerDirector(eventBus, _timer, _backtrackHandler);

            _playerController = new GameplayPlayerController(eventBus, mainPlayer, _stateHandler, _checkpointHandler, _backtrackHandler);
            _inputHandler = new GameplayInputHandler(eventBus, _playerController);

            _soundtrackPlayer = new GameplaySoundtrackPlayer(audioClip, audioSource);
            _soundtrackDirector = new GameplaySoundtrackDirector(eventBus, _soundtrackPlayer, _backtrackHandler);

            _resulter = new GameplayResulter(eventBus, levelData, _timer);

            serviceLocator.Register<BacktrackablesHandler>(_backtrackHandler);
            serviceLocator.Register<CheckpointHandler>(_checkpointHandler);
            serviceLocator.Register<GameplayTimer>(_timer);
            serviceLocator.Register<GameplayPlayerController>(_playerController);
            serviceLocator.Register<GameplayResulter>(_resulter);
            serviceLocator.Register<LevelDataScriptable>(levelData);
        }
        protected override void OnStart()
        {
            modulesManager.Register(_stateHandler);
            modulesManager.Register(_backtrackHandler);
            modulesManager.Register(_checkpointHandler);
            modulesManager.Register(_timer);
            modulesManager.Register(_timerDirector);
            modulesManager.Register(_playerController);
            modulesManager.Register(_inputHandler);
            modulesManager.Register(_soundtrackDirector);
            modulesManager.Register(_resulter);

            CreateInitPipeline();
        }
        protected override void OnExit()
        {
            serviceLocator?.Unregister<BacktrackablesHandler>();
            serviceLocator?.Unregister<CheckpointHandler>();
            serviceLocator?.Unregister<GameplayTimer>();
            serviceLocator?.Unregister<GameplayPlayerController>();
            serviceLocator?.Unregister<GameplayResulter>();
            serviceLocator?.Unregister<LevelDataScriptable>();

            _fsm = null;
            _stateHandler = null;
            _backtrackHandler = null;
            _checkpointHandler = null;
            _playerController = null;
            _inputHandler = null;
            _soundtrackPlayer = null;
            _soundtrackDirector = null;

            _initializer = null;
        }

        private void CreateInitPipeline()
        {
            _initializer = new GameplayInitPipeline(
                eventBus, 
                _playerController, mainPlayer,
                levelData, _resulter);

            modulesManager.Register(_initializer);
        }
    }
}
