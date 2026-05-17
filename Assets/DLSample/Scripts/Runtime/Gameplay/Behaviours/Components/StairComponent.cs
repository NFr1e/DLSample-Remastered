using UnityEngine;
using DG.Tweening;
using DLSample.Facility.Events;

namespace DLSample.Gameplay.Behaviours
{
    /// <summary>
    /// 阶梯状态枚举。
    /// </summary>
    public enum StairStatus
    {
        Landing,
        Landed,
        Rising,
        Rised
    }

    /// <summary>
    /// 阶梯事件参数。
    /// </summary>
    public struct StairEventArgs : IEventArg
    {
        /// <summary>
        /// 当前阶梯状态。
        /// </summary>
        public StairStatus Status { get; set; }
    }

    /// <summary>
    /// 阶梯组件，控制阶梯的下降与上升动画及其状态转换。
    /// </summary>
    public class StairComponent : GameplayObject
    {
        [SerializeField] private Transform stairTransform;
        [SerializeField] private GameplayPlayerMove player;
        [SerializeField] private Vector3 landPosition = new(0, -2, 0);
        [SerializeField] private GameObject landEffect;

        private StairStatus _status = StairStatus.Rised;

        private StairController _controller;

        private EventBus _eventBus;
        private readonly GameplayEventParams.PrepareGameRequest _prepareGameRequest = new();
        private readonly GameplayEventParams.WaitingGameRequest _waitingGameRequest = new();

        private StairEventArgs _eventArgs = new();

        private Transform _playerOriginalParent;
        private Vector3 _originalPosition;
        private Vector3 _playerOriginEuler;

        private Tween _stairTween;

        protected override void OnInit()
        {
            GetOriginalArgs();

            _controller = new StairController(player.transform, _playerOriginEuler);
            _eventBus = GameplayEntry.Instance.EventBus;

            _eventBus.Subscribe<StairRequests.LandRequest>(Land);
            _eventBus.Subscribe<StairRequests.RiseRequest>(Rise);
        }

        protected override void OnStart()
        {
            GameplayEntry.Instance.ModulesManager.Register(_controller);
        }

        /// <summary>
        /// 执行阶梯下降动画。
        /// </summary>
        /// <param name="_">下降请求（忽略）。</param>
        public void Land(StairRequests.LandRequest _)
        {
            if (_status is StairStatus.Landing) return;
            _status = StairStatus.Landing;

            player.transform.SetParent(stairTransform);

            _stairTween?.Kill();

            Destroy(Instantiate(landEffect, player.transform.position, Quaternion.identity), 1f);

            _stairTween = DOTween.Sequence()
                .Join(stairTransform.DOMove(landPosition, 0.8f).SetEase(Ease.InOutQuad))
                .Join(player.transform.DORotateQuaternion(player.PlayerParams.Directions.StartRotation(), 0.8f))
                .OnComplete(OnLanded);
        }

        /// <summary>
        /// 执行阶梯上升动画。
        /// </summary>
        /// <param name="_">上升请求（忽略）。</param>
        public void Rise(StairRequests.RiseRequest _)
        {
            if (_status is StairStatus.Rising) return;
            _status = StairStatus.Rising;

            _eventBus.Invoke(this, _waitingGameRequest);

            player.transform.SetParent(stairTransform);

            _stairTween?.Kill();

            _stairTween = DOTween.Sequence()
                .Join(stairTransform.DOMove(_originalPosition, 0.8f))
                .Join(player.transform.DORotate(_playerOriginEuler, 0.8f))
                .OnComplete(OnRised);
        }

        private void OnLanded()
        {
            player.transform.SetParent(_playerOriginalParent);
            player.transform.rotation = player.PlayerParams.Directions.StartRotation();

            _eventBus.Invoke(this, _prepareGameRequest);

            SetStatus(StairStatus.Landed);
        }

        private void OnRised()
        {
            SetStatus(StairStatus.Rised);
        }

        private void GetOriginalArgs()
        {
            _originalPosition = stairTransform.transform.position;
            _playerOriginEuler = player.PlayerParams.Directions.StartRotation().eulerAngles - new Vector3(0, -90, 0);
            _playerOriginalParent = player.transform.parent;
        }

        private void SetStatus(StairStatus status)
        {
            _status = status;
            _eventArgs.Status = _status;

            _eventBus.Invoke(this, _eventArgs);
        }
    }
}
