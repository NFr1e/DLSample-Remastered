using Cysharp.Threading.Tasks;
using DLSample.App;
using DLSample.Shared;
using DLSample.Framework;
using DLSample.Facility.Events;
using DLSample.Facility.Input;
using DLSample.Gameplay.Phase;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace DLSample.Gameplay
{
    public struct StairRequests
    {
        public struct RiseRequest : IEventArg { }
        public struct LandRequest : IEventArg { }
    }
    public class StairController : IModule, IModuleRequire<GameplayStateHandler>
    {
        public int Priority => DLSampleConsts.Gameplay.PRIORITY_STAIR_CONTROLLER;

        private readonly Transform _player;
        private readonly Vector3 _playerOriginRotation;

        private EventBus _evtBus;
        private GameInput _gameInput;
        private InputManager _inputManager;

        private GameplayStateBase _currentState;
        private GameplayStateHandler _stateHandler;

        private readonly GameplayEventParams.WaitingGameRequest _waitingGameRequest = new();

        private readonly StairRequests.RiseRequest _riseRequest = new();
        private readonly StairRequests.LandRequest _landRequest = new();

        private InputTask _playerInputTask, _cancelInputTask;

        public StairController(Transform player, Vector3 playerOriginalRotation, EventBus evtBus)
        {
            _player = player;
            _playerOriginRotation = playerOriginalRotation;
            _evtBus = evtBus;
        }

        public void OnInit()
        {
            _gameInput = AppEntry.GameInput;
            _inputManager = AppEntry.InputManager;

            _evtBus.Subscribe<GameplayEventParams.GameplayStateChangeCtx>(OnStateChange);

            _playerInputTask = new(OnPlayerInputed, _inputManager.GetInputLayer<InputLayers.GameplayInputLayer>());
            _cancelInputTask = new(OnCancelInputed, _inputManager.GetInputLayer<InputLayers.GameplayInputLayer>());

            _inputManager.RegisterInputTask(_gameInput.Gameplay.PlayerInput, _playerInputTask);
            _inputManager.RegisterInputTask(_gameInput.App.Cancel, _cancelInputTask);

            _evtBus.Invoke(this, _waitingGameRequest);
            _player.eulerAngles = _playerOriginRotation;
        }
        public void OnShutdown()
        {
            _evtBus.Unsubscribe<GameplayEventParams.GameplayStateChangeCtx>(OnStateChange);

            _inputManager.UnregisterInputTask(_gameInput.Gameplay.PlayerInput, _playerInputTask);
            _inputManager.UnregisterInputTask(_gameInput.App.Cancel, _cancelInputTask);
        }
        private void OnStateChange(GameplayEventParams.GameplayStateChangeCtx ctx)
        {
            _currentState = ctx.CurrentState;
        }

        private async void OnPlayerInputed(InputAction.CallbackContext ctx)
        {
            await UniTask.Yield();

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            if (_currentState is GameplayStates.WaitingState)
            {
                _evtBus.Invoke<StairRequests.LandRequest>(this, _landRequest);
            }
        }
        private async void OnCancelInputed(InputAction.CallbackContext ctx)
        {
            await UniTask.Yield();
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            if (_currentState is GameplayStates.PreparingState or GameplayStates.WaitingState)
            {
                if (!_stateHandler.IsGameStarted)
                    _evtBus.Invoke(this, _riseRequest);
            }
        }

        void IModuleRequire<GameplayStateHandler>.SetModule(GameplayStateHandler module)
        {
            _stateHandler = module;
        }
    }
}

