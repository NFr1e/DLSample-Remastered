using UnityEngine;
using DLSample.Facility.Events;

namespace DLSample.Gameplay.Behaviours
{
    /// <summary>
    /// 提示线控制器组件，负责创建并注册提示线控制器。
    /// </summary>
    public class HintLineControllerComponent : GameplayObject
    {
        [SerializeField] private GameObject hintLineGroup;
        private HintLineController _controller;

        protected override void OnInit()
        {
            _controller = new HintLineController(hintLineGroup, GameplayEntry.Instance.ServiceLocator.Get<EventBus>());

            GameplayEntry.Instance.ServiceLocator.Register<HintLineController>(_controller);
        }

        protected override void OnStart()
        {
            GameplayEntry.Instance.ModulesManager.Register(_controller);
        }
    }
}
