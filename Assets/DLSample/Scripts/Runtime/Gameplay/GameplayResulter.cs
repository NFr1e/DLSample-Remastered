using DLSample.Shared;
using DLSample.Framework;
using DLSample.Facility.Events;
using DLSample.Gameplay.Stream;
using DLSample.Gameplay.Behaviours;
using System.Collections.Generic;
using UnityEngine;

namespace DLSample.Gameplay
{
    public class GameplayResulter : IModule
    {
        public int Priority => DLSampleConsts.Gameplay.PRIORITY_RESULTER;

        private readonly LevelDataScriptable _levelData;

        private float _levelLengthSecond;
        public float LevelLengthSecond
        {
            get
            {
                return Mathf.Max(0, _levelLengthSecond);
            }
            set
            {
                _levelLengthSecond = value;
            }
        }

        public LevelDataScriptable LevelData => _levelData;

        private readonly EventBus _eventBus;
        private readonly GameplayTimer _timer;

        private readonly List<Gem> _collectedGems = new();
        private readonly List<Crown> _collectedCrowns = new();

        public GameplayResulter(EventBus eventBus, LevelDataScriptable levelData, GameplayTimer timer)
        {
            _eventBus = eventBus;
            _levelData = levelData;
            _timer = timer;

            LevelLengthSecond = levelData.LevelLength;
        }

        public void OnInit()
        {
            _eventBus.Subscribe<OnCollectEventArgs>(OnCollectCollectable);
            _eventBus.Subscribe<CheckpointEventParams.OnCheckpointed>(OnCheckpointed);
            _eventBus.Subscribe<OnConsumeCheckpoint>(OnConsumeCheckpoint);
            _eventBus.Subscribe<GameplayEventParams.BacktrackGameRequest>(OnBacktrack);
        }
        public void OnShutdown()
        {
            _eventBus.Unsubscribe<OnCollectEventArgs>(OnCollectCollectable);
            _eventBus.Unsubscribe<CheckpointEventParams.OnCheckpointed>(OnCheckpointed);
            _eventBus.Unsubscribe<OnConsumeCheckpoint>(OnConsumeCheckpoint);
            _eventBus.Unsubscribe<GameplayEventParams.BacktrackGameRequest>(OnBacktrack);
        }
        private void OnCollectCollectable(OnCollectEventArgs args)
        {
            switch(args.collectable)
            {
                case Gem gem:
                    if(!_collectedGems.Contains(gem))
                        _collectedGems.Add(gem);
                    break;
            }
        }
        private void OnCheckpointed(CheckpointEventParams.OnCheckpointed args)
        {
            if(args.Checkpoint is CrownCheckpoint crownCp)
            {
                var crown = crownCp.Crown;

                if (!_collectedCrowns.Contains(crown))
                    _collectedCrowns.Add(crown);
            }
        }
        private void OnConsumeCheckpoint(OnConsumeCheckpoint args)
        {
            if (args.checkpoint is CrownCheckpoint crownCheckpoint)
            {
                _collectedCrowns.Remove(crownCheckpoint.Crown);
            }
        }
        private void OnBacktrack(GameplayEventParams.BacktrackGameRequest _)
        {
            _collectedGems.Clear();
        }

        public int GetPercentage()
        {
            if (_timer is null) return 0;
            if (_levelLengthSecond == 0) return 0;

            return Mathf.Clamp((int)(100 * _timer.CurrentTime / LevelLengthSecond), 0, 100); 
        }

        public int GetGemsCount() => _collectedGems.Count;
        public int GetCrownsCount() => _collectedCrowns.Count;
    }
}
