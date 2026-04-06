using UnityEngine;
using UnityEngine.InputSystem;
using DLSample.App;
using DLSample.Facility.Input;

namespace DLSample.Facility.UI 
{
    [RequireComponent(typeof(Panel))]
    public class ClosePanelHotkey : MonoBehaviour
    {
        private Panel _panel;

        private UIElementManager _uiManager;

        private GameInput _gameInput;
        private InputManager _inputManager;

        private InputTask _closePanelInputTask;

        private void Awake()
        {
            _uiManager = AppEntry.UIManager;
            _gameInput = AppEntry.GameInput;
            _inputManager = AppEntry.InputManager;

            _panel = GetComponent<Panel>();

            _closePanelInputTask = new InputTask(ClosePanel, _inputManager.GetInputLayer<InputLayers.UIInputLayer>());
        }

        private void OnEnable()
        {
            _panel.Callbacks.onLoaded.AddListener(Register);
            _panel.Callbacks.onUnload.AddListener(Unregister);
        }
        private void OnDisable()
        {
            _panel.Callbacks.onLoaded.RemoveListener(Register);
            _panel.Callbacks.onUnload.RemoveListener(Unregister);
        }

        private void Register() => _inputManager.RegisterInputTask(_gameInput.App.Cancel, _closePanelInputTask);
        private void Unregister() => _inputManager.UnregisterInputTask(_gameInput.App.Cancel, _closePanelInputTask);

        private async void ClosePanel(InputAction.CallbackContext _)
        {
            await _uiManager.CloseCurrentFullScreenPanel();
        }
    }
}
