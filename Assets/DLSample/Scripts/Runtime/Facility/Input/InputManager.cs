using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DLSample.Facility.Input
{
    public class InputManager
    {
        private readonly List<IInputLayer> _layersCache = new();
        private readonly Dictionary<InputAction, InputTaskPool> _inputMapping = new();

        public void Dispose()
        {
            foreach (var action in _inputMapping.Keys)
            {
                action.performed -= OnInputed;
            }

            _layersCache.Clear();
            _inputMapping.Clear();
        }

        public void RegisterInputTask(InputAction inputAction, InputTask task)
        {
            if (!_inputMapping.ContainsKey(inputAction))
            {
                _inputMapping[inputAction] = new InputTaskPool();

                inputAction.performed += OnInputed;
            }

            _inputMapping[inputAction].AddTask(task);
        }

        public void UnregisterInputTask(InputAction inputAction, InputTask task)
        {
            if (_inputMapping.TryGetValue(inputAction, out var pool))
            {
                if (pool.RemoveTask(task))
                {
                    if (pool.IsEmpty()) 
                    {
                        inputAction.performed -= OnInputed;
                        _inputMapping.Remove(inputAction);
                    }
                }
            }
        }

        public T GetInputLayer<T>() where T : IInputLayer, new()
        {
            IInputLayer result = _layersCache.OfType<T>().FirstOrDefault();

            if (result == null)
            {
                result = new T();
                if (result != null)
                {
                    _layersCache.Add(result);
                }
            }
            return (T)result;
        }

        private void OnInputed(InputAction.CallbackContext ctx)
        {
            if (_inputMapping.TryGetValue(ctx.action, out var pool))
            {
                pool.OnInputed(ctx);
            }
        }
    }
}