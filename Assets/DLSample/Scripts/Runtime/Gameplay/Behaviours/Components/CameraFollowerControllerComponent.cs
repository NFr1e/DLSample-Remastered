using UnityEngine;
using DLSample.Facility.Events;

namespace DLSample.Gameplay.Behaviours
{
    /// <summary>
    /// 相机跟随控制器组件，负责创建并注册相机跟随控制器。
    /// </summary>
    public class CameraFollowerControllerComponent : GameplayObject
    {
        [SerializeField] private CameraFollower follower;

        private CameraFollowerController _controller;

        protected override void OnInit()
        {
            _controller = new CameraFollowerController(GameplayEntry.Instance.ServiceLocator.Get<EventBus>());
            _controller.ChangeFollower(follower);

            GameplayEntry.Instance.ModulesManager.Register<CameraFollowerController>(_controller);
        }

        protected override void OnExit()
        {
            _controller = null;
        }
    }
}
