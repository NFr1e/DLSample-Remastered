using System;
using System.Collections.Generic;
using UnityEngine;

namespace DLSample.Facility.EnityFramework
{
    /// <summary>
    /// 泛型实体对象池，管理 IPoolabelEntity 实体的出池、归还和销毁
    /// </summary>
    /// <typeparam name="T">实体组件类型，必须实现 IPoolabelEntity</typeparam>
    public class EntityPool<T> where T : Component, IPoolabelEntity
    {
        readonly GameObject _prefab;
        readonly Transform _container;

        readonly Queue<T> _pooledObjects = new();
        readonly List<T> _unpooledObjects = new();

        readonly int _maxCapacity;

        bool _prewarmed;
        int _currentSize;

        /// <summary>
        /// 初始化实体对象池
        /// </summary>
        /// <param name="prefab">实体预制体</param>
        /// <param name="maxCapacity">最大容量，0 或负数表示无限制</param>
        /// <param name="container">实例化父节点</param>
        /// <exception cref="ArgumentNullException">prefab 缺少所需组件时抛出</exception>
        public EntityPool(GameObject prefab, int maxCapacity = 0, Transform container = default)
        {
            _prefab = prefab;
            _container = container;

            _maxCapacity = maxCapacity < 0 ? 0 : maxCapacity;
            _currentSize = 0;
            _prewarmed = false;

            if (!_prefab.TryGetComponent<T>(out _))
            {
                throw new ArgumentNullException(nameof(prefab));
            }
        }

        /// <summary>
        /// 预热对象池，提前创建指定数量的实例
        /// </summary>
        /// <param name="count">预热数量，会被限制在 [0, maxCapacity] 范围内</param>
        public void Prewarm(int count)
        {
            if (_prewarmed)
            {
                return;
            }

            count = Mathf.Clamp(count, 0, _maxCapacity);

            for (int i = 0; i < count; ++i)
            {
                var instance = CreateInstance();
                _pooledObjects.Enqueue(instance);
                instance.OnEnpool();
            }

            _prewarmed = true;
        }

        /// <summary>
        /// 从对象池获取一个实体实例
        /// </summary>
        /// <returns>可用实体实例</returns>
        public virtual T Get()
        {
            if (!_prewarmed)
            {
                Debug.LogWarning($"[EntityPool<{typeof(T).Name}>] 尚未预热，自动预热中");
                Prewarm(_maxCapacity);
            }

            T instance;

            if (_pooledObjects.Count > 0)
            {
                instance = _pooledObjects.Dequeue();
            }
            else if (_maxCapacity <= 0 || _currentSize < _maxCapacity)
            {
                instance = CreateInstance();
            }
            else
            {
                Return(_unpooledObjects[0]);
                instance = _pooledObjects.Dequeue();
            }

            instance.OnDepool();
            _unpooledObjects.Add(instance);

            return instance;
        }

        /// <summary>
        /// 将实体归还到对象池
        /// </summary>
        /// <param name="instance">要归还的实体实例</param>
        public virtual void Return(T instance)
        {
            if (instance == null)
            {
                return;
            }

            _unpooledObjects.Remove(instance);
            _pooledObjects.Enqueue(instance);
            instance.OnEnpool();
        }

        /// <summary>
        /// 归还所有已取出的实体
        /// </summary>
        public virtual void ReturnAll()
        {
            foreach (var instance in _unpooledObjects.ToArray())
            {
                Return(instance);
            }
        }

        /// <summary>
        /// 销毁对象池，释放所有实例
        /// </summary>
        public virtual void Dispose()
        {
            foreach (var instance in _pooledObjects)
            {
                if (instance)
                {
                    GameObject.Destroy(instance.gameObject);
                }
            }

            _pooledObjects.Clear();
            _unpooledObjects.Clear();

            _currentSize = 0;
            _prewarmed = false;
        }

        T CreateInstance()
        {
            var go = GameObject.Instantiate(_prefab, _container);

            if (!go.TryGetComponent<T>(out var component))
            {
                GameObject.Destroy(go);
                throw new InvalidOperationException($"Prefab missing required component {typeof(T)}");
            }

            _currentSize++;
            return component;
        }
    }
}
