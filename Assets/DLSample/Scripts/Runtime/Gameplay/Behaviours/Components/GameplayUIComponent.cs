using UnityEngine;
using DLSample.Shared.UI;
using DLSample.App;

namespace DLSample.Gameplay.Behaviours
{
    public class GameplayUIComponent : GameplayObject
    {
        [SerializeField] private UIPanelsDataScriptable panelsConfig;
        [SerializeField] private Camera uiCamera;

        [SerializeField] private GameplayUIMapper gameplayUIMapper;

        private GameplayUIHandler _handler;

        protected override void OnInit()
        {
            var uiManager = AppEntry.UIManager;
            _handler = new(GameplayEntry.Instance.EventBus, uiManager, gameplayUIMapper);

            uiManager.SetupConfigs(panelsConfig);
            uiManager.SetupCamera(uiCamera);
        }
        protected override void OnStart()
        {
            GameplayEntry.Instance.ModulesManager.Register(_handler);
        }
    }
}
