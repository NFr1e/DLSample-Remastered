using System;
using System.Collections.Generic;

namespace DLSample.Facility.Events
{
    /// <summary>
    /// 事件池，管理同一事件类型的所有订阅者，线程安全
    /// </summary>
    public class EventPool
    {
        readonly List<Delegate> _subscribers = new();
        readonly object _lock = new();

        /// <summary>
        /// 添加事件订阅者
        /// </summary>
        /// <typeparam name="TArg">事件参数类型</typeparam>
        /// <param name="action">事件处理回调</param>
        /// <exception cref="ArgumentNullException">action 为 null 时抛出</exception>
        public void AddSubscriber<TArg>(Action<TArg> action) where TArg : IEventArg
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
        /// 移除事件订阅者
        /// </summary>
        /// <typeparam name="TArg">事件参数类型</typeparam>
        /// <param name="action">要移除的事件处理回调</param>
        /// <exception cref="ArgumentNullException">action 为 null 时抛出</exception>
        public void RemoveSubscriber<TArg>(Action<TArg> action) where TArg : IEventArg
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
        /// 触发事件，执行所有订阅者回调
        /// </summary>
        /// <typeparam name="TArg">事件参数类型</typeparam>
        /// <param name="sender">事件发送者</param>
        /// <param name="args">事件参数</param>
        /// <exception cref="ArgumentNullException">args 为 null 时抛出</exception>
        public void Trigger<TArg>(object sender, TArg args) where TArg : IEventArg
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            Delegate[] copySubscribers;
            lock (_lock)
            {
                copySubscribers = _subscribers.ToArray();
            }

            foreach (var subscriber in copySubscribers)
            {
                try
                {
                    (subscriber as Action<TArg>)?.Invoke(args);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"[EventBus] 触发 {typeof(TArg).Name} 事件时订阅者执行异常: {ex.Message}\n{ex.StackTrace}");
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
