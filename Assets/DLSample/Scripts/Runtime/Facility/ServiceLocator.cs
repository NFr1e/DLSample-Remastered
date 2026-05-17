using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DLSample.Facility
{
    /// <summary>
    /// 服务定位器，提供服务的注册、获取和就绪回调机制
    /// </summary>
    public class ServiceLocator
    {
        readonly Dictionary<Type, object> _services = new();
        readonly Dictionary<Type, Action> _onServiceReady = new();

        /// <summary>
        /// 注册服务实例
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <param name="service">服务实例</param>
        public void Register<TService>(TService service) where TService : class
        {
            var type = typeof(TService);

            if (_services.ContainsKey(type))
            {
                return;
            }

            _services[type] = service;

            NotifyServiceReady<TService>();
        }

        /// <summary>
        /// 注销服务
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        public void Unregister<TService>()
        {
            _services.Remove(typeof(TService));
        }

        /// <summary>
        /// 获取服务实例，如果未注册则返回 null 并输出错误日志
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns>服务实例，未注册时返回 null</returns>
        public T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return service as T;
            }

            Debug.LogError($"Service {typeof(T)} 未注册");
            return null;
        }

        /// <summary>
        /// 尝试获取服务实例
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <param name="service">输出的服务实例</param>
        /// <returns>是否成功获取</returns>
        public bool TryGet<TService>(out TService service) where TService : class
        {
            var type = typeof(TService);
            if (_services.TryGetValue(type, out var s))
            {
                service = s as TService;
                return true;
            }

            service = null;
            return false;
        }

        /// <summary>
        /// 当指定类型的所有服务就绪后执行回调
        /// </summary>
        /// <param name="callback">就绪回调</param>
        /// <param name="types">等待就绪的服务类型列表</param>
        public void WhenServicesReady(Action callback, params Type[] types)
        {
            if (types == null || types.Length == 0)
            {
                callback?.Invoke();
                return;
            }

            var pending = new HashSet<Type>(types);

            void OnOneServiceReady()
            {
                if (pending.Count == 0)
                {
                    return;
                }

                pending.RemoveWhere(t => _services.ContainsKey(t));

                if (pending.Count == 0)
                {
                    callback?.Invoke();
                }
            }

            foreach (var type in pending.ToArray())
            {
                if (_services.ContainsKey(type))
                {
                    OnOneServiceReady();
                }
                else
                {
                    if (!_onServiceReady.ContainsKey(type))
                    {
                        _onServiceReady[type] = null;
                    }

                    _onServiceReady[type] += OnOneServiceReady;
                }
            }
        }

        /// <summary>
        /// 通知所有等待此服务就绪的回调
        /// </summary>
        /// <typeparam name="TService">已就绪的服务类型</typeparam>
        public void NotifyServiceReady<TService>() where TService : class
        {
            var type = typeof(TService);

            if (_onServiceReady.TryGetValue(type, out var callbacks))
            {
                callbacks?.Invoke();
                _onServiceReady.Remove(type);
            }
        }

        /// <summary>
        /// 释放服务定位器，清空所有注册
        /// </summary>
        public void Dispose()
        {
            _services.Clear();
            _onServiceReady.Clear();
        }
    }
}
