using DLSample.App;
using DLSample.Shared;
using DLSample.Framework;
using DLSample.Facility.Events;
using DLSample.Facility.Input;
using DLSample.Gameplay.Phase;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace DLSample.Gameplay
{
    public class GameplayInputHandler : IModule
    {
        int IModule.Priority => DLSampleConsts.Gameplay.PRIORITY_INPUT_HANDLER;

        private readonly GameplayPlayerController _playerController;

        private readonly EventBus _evtBus;
        private GameInput _input;
        private InputManager _inputManager;

        private GameplayStateBase _currentState;
        private readonly GameplayEventParams.PrepareGameplayStartRequest _prepareStartRequest = new();
        private readonly GameplayEventParams.PauseGameRequest _pauseRequest = new();

        private InputTask _pauseInputTask = new();

        public GameplayInputHandler(EventBus eventBus, GameplayPlayerController playerCtrl)
        {
            _playerController = playerCtrl;
            _evtBus = eventBus;
        }

        public void OnInit()
        {
            _input = AppEntry.GameInput;
            _inputManager = AppEntry.InputManager;

            _input.Gameplay.Enable();
            _pauseInputTask = new(OnPauseInputed, _inputManager.GetInputLayer<InputLayers.GameplayInputLayer>());

            SubscribeInput();
            SubscribeEvents();
        }

        public void OnShutdown()
        {
            _input.Gameplay.Disable();

            UnsubscribeInput();
            UnsubscribeEvents();
        }
        private void SubscribeEvents()
        {
            _evtBus.Subscribe<GameplayEventParams.GameplayStateChangeCtx>(OnStateChange);
        }
        private void UnsubscribeEvents()
        {
            _evtBus.Unsubscribe<GameplayEventParams.GameplayStateChangeCtx>(OnStateChange);
        }
        private void SubscribeInput()
        {
            _input.Gameplay.PlayerInput.performed += OnPlayerInputed;
            _inputManager.RegisterInputTask(_input.Gameplay.PauseInput, _pauseInputTask);
        }
        private void UnsubscribeInput()
        {
            _input.Gameplay.PlayerInput.performed -= OnPlayerInputed;
            _inputManager.UnregisterInputTask(_input.Gameplay.PauseInput, _pauseInputTask);
        }
        private void OnStateChange(GameplayEventParams.GameplayStateChangeCtx ctx)
        {
            _currentState = ctx.CurrentState;
        }
        private async void OnPlayerInputed(InputAction.CallbackContext ctx)
        {
            try
            {
                await UniTask.Yield();
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

                if (_currentState is GameplayStates.GamingState)
                {
                    _playerController?.PlayerInput();
                }
                if (_currentState is GameplayStates.PreparingState || _currentState is GameplayStates.PauseState)
                {
                    _evtBus.Invoke<GameplayEventParams.PrepareGameplayStartRequest>(this, _prepareStartRequest);
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }
        private void OnPauseInputed(InputAction.CallbackContext ctx)
        {
            if (_currentState is GameplayStates.GamingState)
            {
                _evtBus?.Invoke<GameplayEventParams.PauseGameRequest>(this, _pauseRequest);
            }
        }
    }
}
