using DLSample.Facility.UI;
using DLSample.Facility.Events;
using DLSample.Gameplay.Phase;
using DLSample.Framework;
using DLSample.Shared;
using DLSample.Shared.UI;

namespace DLSample.Gameplay
{
    public class GameplayUIHandler : IModule, IModuleRequire<CheckpointHandler>, IModuleRequire<GameplayStateHandler>
    {
        public int Priority => DLSampleConsts.Gameplay.PRIORITY_UI_HANDLER;

        private readonly EventBus _evtBus;
        private readonly UIElementManager _uiManager;
        private readonly GameplayUIMapper _mapper;

        private CheckpointHandler _checkpointHandler;
        private GameplayStateHandler _stateHandler;

        private Panel _preparingPanel;

        public GameplayUIHandler(EventBus eventBus, UIElementManager uiManager, GameplayUIMapper mapper)
        {
            _evtBus = eventBus;
            _uiManager = uiManager;
            _mapper = mapper;
        }
        public void OnInit()
        {
            _evtBus.Subscribe<GameplayEventParams.GameplayStateChangeCtx>(OnStateChange);
        }
        public void OnShutdown()
        {
            _evtBus.Unsubscribe<GameplayEventParams.GameplayStateChangeCtx>(OnStateChange);
        }
        private async void OnStateChange(GameplayEventParams.GameplayStateChangeCtx ctx)
        {
            switch (ctx.CurrentState)
            {
                case GameplayStates.PreparingState or GameplayStates.WaitingState:
                    if (_preparingPanel == null)
                    {
                        _preparingPanel = await _uiManager.OpenPanel(_mapper.PreparePanelId);
                    }
                    break;

                case GameplayStates.OverState:

                    if (_checkpointHandler is not null)
                    {
                        if (_checkpointHandler.IsCheckpointed && !_stateHandler.IsGameWin)
                        {
                            _ = await _uiManager.OpenPanel(_mapper.RespawnPanelId);
                            return;
                        }
                    }

                    _ = await _uiManager.OpenPanel(_mapper.OverPanelId);
                    break;

                case GameplayStates.PauseState:
                    _ = await _uiManager.OpenPanel(_mapper.PausePanelId);
                    break;

                default:
                    await _uiManager.CloseAllFullscreenPanel();
                    break;
            }
        }

        #region DI
        public void SetModule(CheckpointHandler cpHandler)
        {
            _checkpointHandler = cpHandler;
        }
        public void SetModule(GameplayStateHandler stateHandler)
        {
            _stateHandler = stateHandler;
        }
        #endregion
    }
}
