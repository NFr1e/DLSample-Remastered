using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace DLSample.Framework
{
    /// <summary>
    /// 模块管理器，负责模块的注册、初始化、更新和销毁
    /// </summary>
    public class ModulesManager
    {
        readonly List<IModule> _modules = new();
        readonly Dictionary<Type, IModule> _typeMap = new();

        bool _isInitialized;

        /// <summary>
        /// 注册模块，必须在 Init 之前调用
        /// </summary>
        /// <typeparam name="T">模块类型，必须实现 IModule</typeparam>
        /// <param name="module">模块实例</param>
        public void Register<T>(T module) where T : IModule
        {
            if (_isInitialized)
            {
                Debug.LogWarning($"{module.GetType().Name} 尝试在 ModulesManager 初始化后注册模块");
            }

            if (_typeMap.ContainsKey(typeof(T)))
            {
                Debug.LogWarning($"{module} 已被注册");
                return;
            }

            _modules.Add(module);
            _typeMap[typeof(T)] = module;
        }

        /// <summary>
        /// 初始化所有模块，按优先级排序并注入依赖
        /// </summary>
        public void Init()
        {
            _modules.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            HandleModuleRequires();

            _isInitialized = true;
        }

        /// <summary>
        /// 启动所有模块，调用各模块的 OnInit
        /// </summary>
        public void Start()
        {
            if (!_isInitialized)
            {
                return;
            }

            foreach (var module in _modules)
            {
                module.OnInit();
            }
        }

        /// <summary>
        /// 每帧更新所有模块
        /// </summary>
        /// <param name="deltaTime">帧间隔时间（秒）</param>
        public void Update(float deltaTime)
        {
            if (!_isInitialized)
            {
                return;
            }

            for (int i = 0; i < _modules.Count; i++)
            {
                _modules[i].OnUpdate(deltaTime);
            }
        }

        /// <summary>
        /// 销毁所有模块并释放资源
        /// </summary>
        public void Dispose()
        {
            foreach (var module in _modules)
            {
                module.OnShutdown();
            }

            _modules.Clear();
        }

        /// <summary>
        /// 处理模块依赖注入，扫描各模块实现的 IModuleRequire 接口并注入对应模块
        /// </summary>
        void HandleModuleRequires()
        {
            foreach (var module in _modules)
            {
                var interfaces = module.GetType().GetInterfaces();

                foreach (var interfaceType in interfaces)
                {
                    if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IModuleRequire<>))
                    {
                        Type targetType = interfaceType.GetGenericArguments()[0];

                        if (_typeMap.TryGetValue(targetType, out var dependency))
                        {
                            MethodInfo setMethod = interfaceType.GetMethod("SetModule");
                            setMethod.Invoke(module, new object[] { dependency });
                        }
                        else
                        {
                            Debug.LogWarning($"<color=red>[ModuleManager]</color> {module.GetType().Name} 需要 {targetType.Name}，但该模块未注册");
                        }
                    }
                }
            }
        }
    }
}
