using System;
using System.Collections.Generic;
using UnityEngine;

namespace DLSample.Shared
{
    /// <summary>
    /// 玩家方向序列管理，控制玩家的朝向切换
    /// </summary>
    [Serializable]
    public class PlayerDirections
    {
        [SerializeField] private Vector3 upwards = Vector3.up;
        [SerializeField] private List<Vector3> directionsSequence;

        [SerializeField, HideInInspector] private int _currentIndex = -1;

        public bool IsValid => directionsSequence.Count >= 2;
        public int CurrentIndex => _currentIndex;
        public Vector3 Upwards => upwards;

        public PlayerDirections()
        {
            directionsSequence = new()
            {
                new Vector3(0, 0, 1),
                new Vector3(1, 0, 0)
            };
        }

        /// <summary>
        /// 获取起始旋转（使用序列中最后一个方向）
        /// </summary>
        public Quaternion StartRotation()
        {
            Quaternion result;

            if (directionsSequence.Count > 0)
                result = Resolve(directionsSequence[^1]);
            else
                throw new ArgumentOutOfRangeException();

            return result;
        }

        /// <summary>
        /// 获取指定索引处的旋转
        /// </summary>
        public Quaternion RotationAtIndex(int index)
        {
            if (index >= directionsSequence.Count || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range.");
            }

            index = Mathf.Clamp(index, 0, directionsSequence.Count - 1);
            return Resolve(directionsSequence[index]);
        }

        /// <summary>
        /// 移动到下一个方向并返回对应的旋转
        /// </summary>
        public Quaternion MoveNext()
        {
            if (directionsSequence.Count <= 0)
                throw new ArgumentOutOfRangeException();

            _currentIndex++;

            if (_currentIndex > directionsSequence.Count - 1)
                _currentIndex = 0;

            return Resolve(directionsSequence[_currentIndex]);
        }

        /// <summary>
        /// 设置当前方向索引
        /// </summary>
        public void SetCurrentIndex(int index)
        {
            index = Mathf.Clamp(index, -1, directionsSequence.Count - 1);
            _currentIndex = index;
        }

        /// <summary>
        /// 重置当前方向索引为初始状态
        /// </summary>
        public void Reset()
        {
            _currentIndex = -1;
        }

        /// <summary>
        /// 深度克隆当前方向序列
        /// </summary>
        public PlayerDirections Clone()
        {
            return DeepCopyHelper.Clone(this);
        }

        private Quaternion Resolve(Vector3 dir)
        {
            return Quaternion.LookRotation(dir, upwards);
        }
    }
}
