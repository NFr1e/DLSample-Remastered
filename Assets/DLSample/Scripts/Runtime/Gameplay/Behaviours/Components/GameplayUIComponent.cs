using UnityEngine;
using DLSample.Shared.UI;
using DLSample.App;
using DLSample.Facility.Events;

namespace DLSample.Gameplay.Behaviours
{
    /// <summary>
    /// 游戏UI组件，初始化UI管理器并注册UI处理器模块。
    /// </summary>
    public class GameplayUIComponent : GameplayObject
    {
        [SerializeField] private UIPanelsDataScriptable panelsConfig;
        [SerializeField] private Camera uiCamera;
        [SerializeField] private GameplayUIMapper gameplayUIMapper;

        private GameplayUIHandler _handler;

        protected override void OnInit()
        {
            var uiManager = AppEntry.UIManager;
            _handler = new(GameplayEntry.Instance.ServiceLocator.Get<EventBus>(), uiManager, gameplayUIMapper);

            uiManager.SetupConfigs(panelsConfig);
            uiManager.SetupCamera(uiCamera);
        }

        protected override void OnStart()
        {
            GameplayEntry.Instance.ModulesManager.Register(_handler);
        }
    }
}
