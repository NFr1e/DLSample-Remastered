using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace DLSample.Facility.Input
{
    /// <summary>
    /// 输入任务池，管理同一 InputAction 绑定的多个 InputTask，按层级优先级分发
    /// </summary>
    public class InputTaskPool
    {
        readonly List<InputTask> _tasks = new();

        bool _isSorted;

        /// <summary>
        /// 添加输入任务
        /// </summary>
        /// <param name="task">输入任务</param>
        public void AddTask(InputTask task)
        {
            if (_tasks.Contains(task))
            {
                return;
            }

            _tasks.Add(task);
            _isSorted = false;
        }

        /// <summary>
        /// 移除输入任务
        /// </summary>
        /// <param name="task">要移除的输入任务</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveTask(InputTask task)
        {
            if (_tasks.Remove(task))
            {
                _isSorted = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 处理输入事件，按层级优先级顺序分发给各任务回调
        /// </summary>
        /// <param name="ctx">输入回调上下文</param>
        public void OnInputed(InputAction.CallbackContext ctx)
        {
            if (!_isSorted)
            {
                Sort();
            }

            foreach (var task in _tasks)
            {
                if (!ctx.started && !ctx.performed)
                {
                    continue;
                }

                task.Callback?.Invoke(ctx);

                if (task.Layer.BlockLowerLayers)
                {
                    break;
                }
            }
        }

        void Sort()
        {
            _tasks.Sort();
            _isSorted = true;
        }

        /// <summary>
        /// 清空所有任务
        /// </summary>
        public void Clear()
        {
            _tasks.Clear();
            _isSorted = false;
        }

        /// <summary>
        /// 任务池是否为空
        /// </summary>
        public bool IsEmpty() => _tasks.Count == 0;
    }
}
