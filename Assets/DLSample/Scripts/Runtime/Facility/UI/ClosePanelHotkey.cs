using UnityEngine;
using UnityEngine.InputSystem;
using DLSample.App;
using DLSample.Facility.Input;

namespace DLSample.Facility.UI
{
    [RequireComponent(typeof(Panel))]
    public class ClosePanelHotkey : MonoBehaviour
    {
        Panel _panel;

        UIElementManager _uiManager;

        GameInput _gameInput;
        InputManager _inputManager;

        InputTask _closePanelInputTask;

        void Awake()
        {
            _uiManager = AppEntry.UIManager;
            _gameInput = AppEntry.GameInput;
            _inputManager = AppEntry.InputManager;

            _panel = GetComponent<Panel>();

            _closePanelInputTask = new InputTask(ClosePanel, _inputManager.GetInputLayer<InputLayers.UIInputLayer>());
        }

        void OnEnable()
        {
            _panel.Callbacks.onLoaded.AddListener(Register);
            _panel.Callbacks.onUnload.AddListener(Unregister);
        }

        void OnDisable()
        {
            _panel.Callbacks.onLoaded.RemoveListener(Register);
            _panel.Callbacks.onUnload.RemoveListener(Unregister);

            Unregister();
        }

        void Register() => _inputManager.RegisterInputTask(_gameInput.App.Cancel, _closePanelInputTask);

        void Unregister() => _inputManager.UnregisterInputTask(_gameInput.App.Cancel, _closePanelInputTask);

        async void ClosePanel(InputAction.CallbackContext _)
        {
            await _uiManager.CloseCurrentFullScreenPanel();
            Unregister();
        }
    }
}
