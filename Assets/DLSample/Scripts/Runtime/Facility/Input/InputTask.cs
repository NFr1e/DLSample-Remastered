using System;
using UnityEngine.InputSystem;

namespace DLSample.Facility.Input
{
    /// <summary>
    /// 输入任务结构体，封装输入回调及其所属输入层，按层级优先级排序
    /// </summary>
    public struct InputTask : IComparable<InputTask>
    {
        /// <summary>输入回调</summary>
        public Action<InputAction.CallbackContext> Callback { get; private set; }

        /// <summary>所属输入层</summary>
        public IInputLayer Layer { get; private set; }

        /// <summary>
        /// 创建输入任务
        /// </summary>
        /// <param name="callback">输入回调</param>
        /// <param name="layer">所属输入层</param>
        public InputTask(Action<InputAction.CallbackContext> callback, IInputLayer layer)
        {
            Callback = callback;
            Layer = layer;
        }

        /// <summary>
        /// 按层级优先级比较，优先级高的排前面
        /// </summary>
        public readonly int CompareTo(InputTask other)
        {
            return other.Layer.Priority.CompareTo(Layer.Priority);
        }
    }
}

