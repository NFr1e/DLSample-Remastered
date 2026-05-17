using DLSample.Facility.Events;
using DLSample.Framework;
using DLSample.Gameplay.Behaviours;
using DLSample.Shared;

namespace DLSample.Gameplay
{
    public class GameplayInitPipeline : IModule
    {
        public int Priority => DLSampleConsts.Gameplay.PRIORITY_INITIALIZER;

        private readonly EventBus _evtBus;
        private readonly GameplayEventParams.PrepareGameRequest _prepareGameRequest = new();

        private readonly GameplayPlayerMove _mainPlayer;
        private readonly GameplayPlayerController _playerController;

        private readonly LevelDataScriptable _levelData;
        private readonly GameplayResulter _progressCounter;

        public GameplayInitPipeline(
            EventBus evtBus,
            GameplayPlayerController playerController, GameplayPlayerMove mainPlayer,
            LevelDataScriptable levelData, GameplayResulter progressCounter)
        {
            _evtBus = evtBus;
            _playerController = playerController;
            _mainPlayer = mainPlayer;
            _levelData = levelData;
            _progressCounter = progressCounter;
        }

        public void OnInit()
        {
            _playerController.AddPlayer(_mainPlayer);
            _progressCounter.LevelLengthSecond = _levelData.LevelLength;

            _evtBus.Invoke(this, _prepareGameRequest);
        }
    }
}
