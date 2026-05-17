using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DLSample.Facility.Events
{
    /// <summary>
    /// 异步事件池，管理同一事件类型的所有异步订阅者，支持并行执行
    /// </summary>
    public class AsyncEventPool
    {
        /// <summary>存储异步委托</summary>
        readonly List<object> _subscribers = new();
        readonly object _lock = new();

        /// <summary>
        /// 添加异步订阅者
        /// </summary>
        /// <typeparam name="TArg">事件参数类型</typeparam>
        /// <param name="action">异步事件处理回调，返回 UniTask</param>
        /// <exception cref="ArgumentNullException">action 为 null 时抛出</exception>
        public void AddSubscriber<TArg>(Func<TArg, UniTask> action) where TArg : IEventArg
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock (_lock)
            {
                if (!_subscribers.Contains(action))
                {
                    _subscribers.Add(action);
                }
            }
        }

        /// <summary>
        /// 移除异步订阅者
        /// </summary>
        /// <typeparam name="TArg">事件参数类型</typeparam>
        /// <param name="action">要移除的异步事件处理回调</param>
        /// <exception cref="ArgumentNullException">action 为 null 时抛出</exception>
        public void RemoveSubscriber<TArg>(Func<TArg, UniTask> action) where TArg : IEventArg
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock (_lock)
            {
                if (_subscribers.Contains(action))
                {
                    _subscribers.Remove(action);
                }
            }
        }

        /// <summary>
        /// 触发异步事件，并行执行所有订阅者，并等待其全部完成
        /// </summary>
        /// <typeparam name="TArg">事件参数类型</typeparam>
        /// <param name="sender">事件发送者</param>
        /// <param name="args">事件参数</param>
        /// <exception cref="ArgumentNullException">args 为 null 时抛出</exception>
        public async UniTask TriggerAsync<TArg>(object sender, TArg args) where TArg : IEventArg
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            // 快照复制，防止在遍历过程中集合被修改
            object[] copySubscribers;
            lock (_lock)
            {
                copySubscribers = _subscribers.ToArray();
            }

            if (copySubscribers.Length == 0)
            {
                return;
            }

            var tasks = new List<UniTask>(copySubscribers.Length);

            foreach (var subscriber in copySubscribers)
            {
                try
                {
                    if (subscriber is Func<TArg, UniTask> asyncAction)
                    {
                        // 启动任务但不使用 await，以便并行执行
                        tasks.Add(asyncAction(args));
                    }
                }
                catch (Exception ex)
                {
                    // 捕获同步委托抛出的异常，发生在创建 Task 时
                    Debug.LogError($"[AsyncEventPool] 订阅者抛出异常 ({typeof(TArg).Name}): {ex.Message}");
                }
            }

            if (tasks.Count > 0)
            {
                try
                {
                    // 等待所有任务完成
                    await UniTask.WhenAll(tasks);
                }
                catch (Exception ex)
                {
                    // Task.WhenAll 只会抛出聚合异常中的第一个，其他异常会丢失
                    // 这里统一捕获，防止未观察到的异常导致程序崩溃
                    Debug.LogError($"[AsyncEventPool] 事件执行异常 ({typeof(TArg).Name}): {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 清除所有订阅者
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _subscribers.Clear();
            }
        }
    }
}
