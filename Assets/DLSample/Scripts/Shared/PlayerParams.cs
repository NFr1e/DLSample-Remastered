using System;
using UnityEngine;

namespace DLSample.Shared
{
    /// <summary>
    /// 玩家参数配置，包含移动速度、重力、方向等参数
    /// </summary>
    [Serializable]
    public class PlayerParams
    {
        [SerializeField] private float moveSpeed = 12;
        [SerializeField] private bool forceGrounded = false;
        [SerializeField] private bool useGravity = true;
        [SerializeField] private Vector3 localGravity = new(0, -9.81f, 0);
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float checkGroundDist = 0.75f;
        [SerializeField] private PlayerDirections directions = new();

        public float MoveSpeed => moveSpeed;
        public bool ForceGrounded => forceGrounded;
        public bool UseGravity => useGravity;
        public Vector3 LocalGravity => localGravity;
        public LayerMask GroundLayer => groundLayer;
        public float CheckGroundDist => checkGroundDist;
        public PlayerDirections Directions => directions;

        /// <summary>
        /// 设置移动速度
        /// </summary>
        public void SetSpeed(float speed)
        {
            moveSpeed = speed;
        }

        /// <summary>
        /// 设置是否强制着地
        /// </summary>
        public void SetForceGrounded(bool force)
        {
            forceGrounded = force;
        }

        /// <summary>
        /// 设置是否使用重力
        /// </summary>
        public void SetUseGravity(bool use)
        {
            useGravity = use;
        }

        /// <summary>
        /// 设置本地重力向量
        /// </summary>
        public void SetLocalGravity(Vector3 gravity)
        {
            localGravity = gravity;
        }

        /// <summary>
        /// 设置方向序列并重置当前索引
        /// </summary>
        public void SetDirection(PlayerDirections direction)
        {
            directions = direction;
            directions.Reset();
        }
    }
}
