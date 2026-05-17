using DLSample.Shared;
using System;
using UnityEngine;

namespace DLSample.Gameplay.Behaviours
{
    /// <summary>
    /// 玩家移动参数结构体，包含移动状态与位置信息。
    /// </summary>
    public struct PlayerMovingArgs
    {
        /// <summary>
        /// 玩家参数配置。
        /// </summary>
        public PlayerParams Params;

        /// <summary>
        /// 当前位置。
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// 当前旋转。
        /// </summary>
        public Quaternion Rotation;

        /// <summary>
        /// 当前速度。
        /// </summary>
        public Vector3 Velocity;

        /// <summary>
        /// 是否着地。
        /// </summary>
        public bool IsGrounded;

        /// <summary>
        /// 是否正在移动。
        /// </summary>
        public bool IsMoving;
    }

    /// <summary>
    /// 玩家移动接口，控制移动的启动、停止与输入响应。
    /// </summary>
    public interface IPlayerMove
    {
        /// <summary>
        /// 开始移动。
        /// </summary>
        void StartMove();

        /// <summary>
        /// 停止移动。
        /// </summary>
        void StopMove();

        /// <summary>
        /// 响应玩家输入。
        /// </summary>
        void Inputed();

        /// <summary>
        /// 是否正在移动。
        /// </summary>
        bool IsMoving { get; }

        /// <summary>
        /// 移动开始时触发。
        /// </summary>
        event Action<PlayerMovingArgs> OnStartMove;

        /// <summary>
        /// 移动停止时触发。
        /// </summary>
        event Action<PlayerMovingArgs> OnStopMove;

        /// <summary>
        /// 移动过程中持续触发。
        /// </summary>
        event Action<PlayerMovingArgs> OnMoving;

        /// <summary>
        /// 转向时触发。
        /// </summary>
        event Action<PlayerMovingArgs> OnTurn;

        /// <summary>
        /// 着地时触发。
        /// </summary>
        event Action<PlayerMovingArgs> OnLand;
    }
}
