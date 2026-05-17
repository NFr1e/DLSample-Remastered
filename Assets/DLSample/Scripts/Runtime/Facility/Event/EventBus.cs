using System;
using System.Collections.Generic;

namespace DLSample.Facility.Events
{
    /// <summary>
    /// 同步事件总线，提供线程安全的事件订阅、取消订阅和触发机制
    /// </summary>
    public class EventBus
    {
        readonly Dictionary<Type, EventPool> _eventsDic = new();
        readonly object _lock = new();

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <typeparam name="TArg">事件参数类型，必须实现 IEventArg</typeparam>
        /// <param name="action">事件处理回调</param>
        /// <exception cref="ArgumentNullException">action 为 null 时抛出</exception>
        public void Subscribe<TArg>(Action<TArg> action) where TArg : IEventArg
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
                    eventPool = new EventPool();
                    _eventsDic.Add(eventType, eventPool);
                }

                eventPool.AddSubscriber(action);
            }
        }

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        /// <typeparam name="TArg">事件参数类型</typeparam>
        /// <param name="action">要移除的事件处理回调</param>
        /// <exception cref="ArgumentNullException">action 为 null 时抛出</exception>
        public void Unsubscribe<TArg>(Action<TArg> action) where TArg : IEventArg
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
        /// 触发事件，执行所有已订阅的回调
        /// </summary>
        /// <typeparam name="TArg">事件参数类型</typeparam>
        /// <param name="sender">事件发送者</param>
        /// <param name="args">事件参数</param>
        /// <exception cref="ArgumentNullException">args 为 null 时抛出</exception>
        public void Invoke<TArg>(object sender, TArg args) where TArg : IEventArg
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            lock (_lock)
            {
                var eventType = typeof(TArg);

                if (!_eventsDic.TryGetValue(eventType, out var eventPool))
                {
                    return;
                }

                eventPool.Trigger(sender, args);
            }
        }

        /// <summary>
        /// 清除所有已注册的事件和订阅者
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
        /// 释放事件总线，清除所有事件
        /// </summary>
        public void Dispose()
        {
            ClearAllEvents();
        }
    }
}
