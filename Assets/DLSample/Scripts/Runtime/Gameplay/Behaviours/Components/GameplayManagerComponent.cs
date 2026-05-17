using UnityEngine;
using System.Collections.Generic;
using DLSample.Shared;
using DLSample.Gameplay.Phase;
using DLSample.Gameplay.Stream;
using DLSample.Facility.Events;
using DLSample.Facility;
using DLSample.Framework;

namespace DLSample.Gameplay.Behaviours
{
    /// <summary>
    /// 游戏管理器组件，负责创建并注册所有核心游戏系统（FSM、计时器、玩家控制、输入、音轨、结果器等）。
    /// </summary>
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

        private GameplayReadinessCoordinator _readinessCoordinator;

        private GameplayResulter _resulter;

        private GameplayInitPipeline _initializer;

        private EventBus _eventBus;
        private ServiceLocator _serviceLocator;
        private ModulesManager _modulesManager;

        protected override void OnInit()
        {
            _eventBus = GameplayEntry.Instance.EventBus;
            _serviceLocator = GameplayEntry.Instance.ServiceLocator;
            _modulesManager = GameplayEntry.Instance.ModulesManager;

            _fsm = new GameplayFSM(_eventBus);
            _stateHandler = new GameplayStateHandler(_eventBus, _fsm);

            _backtrackHandler = new BacktrackablesHandler(_eventBus);
            _checkpointHandler = new CheckpointHandler(_eventBus);

            _timer = new GameplayTimer();
            _timerDirector = new GameplayTimerDirector(_eventBus, _timer, _backtrackHandler);

            _playerController = new GameplayPlayerController(_eventBus, mainPlayer, _stateHandler, _checkpointHandler, _backtrackHandler);
            _inputHandler = new GameplayInputHandler(_eventBus, _playerController);

            _soundtrackPlayer = new GameplaySoundtrackPlayer(audioClip, audioSource);
            _soundtrackDirector = new GameplaySoundtrackDirector(_eventBus, _soundtrackPlayer, _backtrackHandler);
            _readinessCoordinator = new GameplayReadinessCoordinator(_eventBus, new List<IPrepareAsync> { _soundtrackDirector });

            _resulter = new GameplayResulter(_eventBus, levelData, _timer);

            _serviceLocator.Register<EventBus>(_eventBus);
            _serviceLocator.Register<BacktrackablesHandler>(_backtrackHandler);
            _serviceLocator.Register<CheckpointHandler>(_checkpointHandler);
            _serviceLocator.Register<GameplayTimer>(_timer);
            _serviceLocator.Register<GameplayPlayerController>(_playerController);
            _serviceLocator.Register<GameplayResulter>(_resulter);
            _serviceLocator.Register<LevelDataScriptable>(levelData);
        }

        protected override void OnStart()
        {
            _modulesManager.Register(_stateHandler);
            _modulesManager.Register(_backtrackHandler);
            _modulesManager.Register(_checkpointHandler);
            _modulesManager.Register(_timer);
            _modulesManager.Register(_timerDirector);
            _modulesManager.Register(_playerController);
            _modulesManager.Register(_inputHandler);
            _modulesManager.Register(_soundtrackDirector);
            _modulesManager.Register(_readinessCoordinator);
            _modulesManager.Register(_resulter);

            CreateInitPipeline();
        }

        protected override void OnExit()
        {
            _serviceLocator?.Unregister<EventBus>();
            _serviceLocator?.Unregister<BacktrackablesHandler>();
            _serviceLocator?.Unregister<CheckpointHandler>();
            _serviceLocator?.Unregister<GameplayTimer>();
            _serviceLocator?.Unregister<GameplayPlayerController>();
            _serviceLocator?.Unregister<GameplayResulter>();
            _serviceLocator?.Unregister<LevelDataScriptable>();

            _fsm = null;
            _stateHandler = null;
            _backtrackHandler = null;
            _checkpointHandler = null;
            _playerController = null;
            _inputHandler = null;
            _soundtrackPlayer = null;
            _soundtrackDirector = null;
            _readinessCoordinator = null;

            _initializer = null;
        }

        private void CreateInitPipeline()
        {
            _initializer = new GameplayInitPipeline(
                _eventBus,
                _playerController, mainPlayer,
                levelData, _resulter);

            _modulesManager.Register(_initializer);
        }
    }
}
