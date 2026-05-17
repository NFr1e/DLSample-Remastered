using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace DLSample.Facility.Events
{
    /// <summary>
    /// 异步事件总线，提供线程安全的异步事件订阅、取消订阅和触发机制
    /// </summary>
    public class AsyncEventBus
    {
        readonly Dictionary<Type, AsyncEventPool> _eventsDic = new();
        readonly object _lock = new();

        /// <summary>
        /// 订阅异步事件
        /// 注意：action 必须是 async 或返回 UniTask 的方法
        /// </summary>
        /// <typeparam name="TArg">事件参数类型，必须实现 IEventArg</typeparam>
        /// <param name="action">异步事件处理回调，返回 UniTask</param>
        /// <exception cref="ArgumentNullException">action 为 null 时抛出</exception>
        public void Subscribe<TArg>(Func<TArg, UniTask> action) where TArg : IEventArg
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock (_lock)
            {
                var eventType = typeof(TArg);

                if (!_eventsDic.TryGetValue(eventType, out var eventPool))
                {
                    eventPool = new AsyncEventPool();
                    _eventsDic.Add(eventType, eventPool);
                }

                eventPool.AddSubscriber(action);
            }
        }

        /// <summary>
        /// 取消订阅异步事件
        /// </summary>
        /// <typeparam name="TArg">事件参数类型</typeparam>
        /// <param name="action">要移除的异步事件处理回调</param>
        /// <exception cref="ArgumentNullException">action 为 null 时抛出</exception>
        public void Unsubscribe<TArg>(Func<TArg, UniTask> action) where TArg : IEventArg
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock (_lock)
            {
                var eventType = typeof(TArg);
                if (_eventsDic.TryGetValue(eventType, out var eventPool))
                {
                    eventPool.RemoveSubscriber(action);
                }
            }
        }

        /// <summary>
        /// 触发异步事件，调用者需要使用 await 等待所有订阅者执行完成
        /// </summary>
        /// <typeparam name="TArg">事件参数类型</typeparam>
        /// <param name="sender">事件发送者</param>
        /// <param name="args">事件参数</param>
        /// <exception cref="ArgumentNullException">args 为 null 时抛出</exception>
        public async UniTask InvokeAsync<TArg>(object sender, TArg args) where TArg : IEventArg
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            AsyncEventPool eventPool;
            lock (_lock)
            {
                var eventType = typeof(TArg);
                if (!_eventsDic.TryGetValue(eventType, out eventPool))
                {
                    return;
                }
            }

            await eventPool.TriggerAsync(sender, args);
        }

        /// <summary>
        /// 清除所有已注册的异步事件和订阅者
        /// </summary>
        public void ClearAllEvents()
        {
            lock (_lock)
            {
                foreach (var pool in _eventsDic.Values)
                {
                    pool.Clear();
                }

                _eventsDic.Clear();
            }
        }

        /// <summary>
        /// 释放异步事件总线，清除所有事件
        /// </summary>
        public void Dispose()
        {
            ClearAllEvents();
        }
    }
}
