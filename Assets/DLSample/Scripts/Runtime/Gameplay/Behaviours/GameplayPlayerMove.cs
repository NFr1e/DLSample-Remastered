using System;
using UnityEngine;
using DLSample.Shared;

namespace DLSample.Gameplay.Behaviours
{
    /// <summary>
    /// 玩家移动控制器，处理移动、重力、着地与转向逻辑，并广播移动事件。
    /// </summary>
    public class GameplayPlayerMove : GameplayObject, IPlayerMove
    {
        [SerializeField] private PlayerParams playerParams;

        public event Action<PlayerMovingArgs> OnStartMove;
        public event Action<PlayerMovingArgs> OnStopMove;
        public event Action<PlayerMovingArgs> OnMoving;
        public event Action<PlayerMovingArgs> OnTurn;
        public event Action<PlayerMovingArgs> OnLand;

        private bool _isMoving = false;
        private bool _isColliding = false;
        private bool _isGrounded = false;
        private Vector3 _dropVelocity = Vector3.zero;

        private PlayerMovingArgs _movingArgs = new();

        public bool IsMoving => _isMoving;
        public PlayerMovingArgs MovingArgs => _movingArgs;
        public PlayerParams PlayerParams => playerParams;

        private void Update()
        {
            if (_isMoving)
            {
                Move();
            }
            CheckGround();
        }

        private void FixedUpdate()
        {
            Drop();
        }

        private void OnCollisionEnter(Collision collision)
        {
            _isColliding = true;
        }

        private void OnCollisionStay(Collision collision)
        {
            _isColliding = true;
        }

        private void OnCollisionExit(Collision collision)
        {
            _isColliding = false;
        }

        /// <summary>
        /// 准备阶段，冻结玩家并将朝向重置为起始方向。
        /// </summary>
        public void Ready()
        {
            Freeze();
            transform.rotation = playerParams.Directions.StartRotation();
        }

        /// <summary>
        /// 开始移动。
        /// </summary>
        public void StartMove()
        {
            if (_isMoving) return;

            _isMoving = true;

            UpdateMovingCtx();
            OnStartMove?.Invoke(_movingArgs);
        }

        /// <summary>
        /// 停止移动。
        /// </summary>
        public void StopMove()
        {
            if (!_isMoving) return;

            _isMoving = false;

            UpdateMovingCtx();
            OnStopMove?.Invoke(_movingArgs);
        }

        /// <summary>
        /// 冻结玩家，清除移动与着地状态。
        /// </summary>
        public void Freeze()
        {
            _isMoving = false;
            _isGrounded = false;

            _dropVelocity = Vector3.zero;
        }

        private void Move()
        {
            transform.Translate(playerParams.MoveSpeed * Time.deltaTime * Vector3.forward, Space.Self);

            UpdateMovingCtx();
            OnMoving?.Invoke(_movingArgs);
        }

        private void Drop()
        {
            if (!IsMoving) return;

            if (!_isGrounded && playerParams.UseGravity)
            {
                _dropVelocity += playerParams.LocalGravity * Time.deltaTime;
            }

            transform.Translate(_dropVelocity * Time.deltaTime);
        }

        private void Land()
        {
            OnLand?.Invoke(_movingArgs);

            _dropVelocity = Vector3.zero;
            UpdateMovingCtx();
        }

        /// <summary>
        /// 转向，按玩家参数定义的下一方向旋转。
        /// </summary>
        public void Turn()
        {
            transform.rotation = playerParams.Directions.MoveNext();

            UpdateMovingCtx();
            OnTurn?.Invoke(_movingArgs);
        }

        private void UpdateMovingCtx()
        {
            _movingArgs.Params = playerParams;
            _movingArgs.Position = transform.position;
            _movingArgs.Rotation = transform.rotation;
            _movingArgs.Velocity = _dropVelocity;
            _movingArgs.IsGrounded = _isGrounded;
            _movingArgs.IsMoving = _isMoving;
        }

        /// <summary>
        /// 响应输入，着地时执行转向。
        /// </summary>
        public void Inputed()
        {
            if (IsMoving && _isGrounded)
            {
                Turn();
            }
        }

        private void CheckGround()
        {
            var wasGround = _isGrounded;

            _isGrounded = playerParams.ForceGrounded
                        || (_isColliding && Physics.Raycast(transform.position, -transform.up, playerParams.CheckGroundDist, playerParams.GroundLayer));

            if (!wasGround && _isGrounded) Land();
        }

        /// <summary>
        /// 设置下落速度。
        /// </summary>
        /// <param name="velocity">速度向量。</param>
        public void SetVelocity(Vector3 velocity)
        {
            _dropVelocity = velocity;
        }

        /// <summary>
        /// 设置着地状态。
        /// </summary>
        /// <param name="grounded">是否着地。</param>
        public void SetGrounded(bool grounded)
        {
            _isGrounded = grounded;
        }
    }
}
