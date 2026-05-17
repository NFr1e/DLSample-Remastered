using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DLSample.Facility.Input
{
    /// <summary>
    /// 输入管理器，负责绑定 InputAction 到 InputTask 并分发输入事件
    /// </summary>
    public class InputManager
    {
        readonly List<IInputLayer> _layersCache = new();
        readonly Dictionary<InputAction, InputTaskPool> _inputMapping = new();

        /// <summary>
        /// 释放输入管理器，解绑所有 InputAction 并清空映射
        /// </summary>
        public void Dispose()
        {
            foreach (var action in _inputMapping.Keys)
            {
                action.performed -= OnInputed;
            }

            _layersCache.Clear();
            _inputMapping.Clear();
        }

        /// <summary>
        /// 注册输入任务，将 InputAction 绑定到指定的 InputTask
        /// </summary>
        /// <param name="inputAction">输入动作</param>
        /// <param name="task">输入任务</param>
        public void RegisterInputTask(InputAction inputAction, InputTask task)
        {
            if (!_inputMapping.ContainsKey(inputAction))
            {
                _inputMapping[inputAction] = new InputTaskPool();

                inputAction.performed += OnInputed;
            }

            _inputMapping[inputAction].AddTask(task);
        }

        /// <summary>
        /// 注销输入任务，解绑 InputAction 与 InputTask 的关联
        /// </summary>
        /// <param name="inputAction">输入动作</param>
        /// <param name="task">输入任务</param>
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

        /// <summary>
        /// 获取或创建指定类型的输入层实例
        /// </summary>
        /// <typeparam name="T">输入层类型，必须实现 IInputLayer 且有无参构造函数</typeparam>
        /// <returns>输入层实例</returns>
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

        void OnInputed(InputAction.CallbackContext ctx)
        {
            if (_inputMapping.TryGetValue(ctx.action, out var pool))
            {
                pool.OnInputed(ctx);
            }
        }
    }
}
