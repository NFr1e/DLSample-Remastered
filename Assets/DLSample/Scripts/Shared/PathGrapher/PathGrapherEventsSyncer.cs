using UnityEngine;
using DLSample.Gameplay;
using DLSample.Gameplay.Stream;
using DLSample.Gameplay.Behaviours;
using System.Collections.Generic;

namespace DLSample.Editor.PathGrapher
{
    public class PathGrapherEventsSyncer : GameplayObject
    {
        [SerializeField] private PathGrapherAsset pathGrapherAsset;

        private GameplayTimer _gameplayTimer;
        private readonly List<GameplayTimer.TickEvent> _tickEvents = new();

        protected override void OnStart()
        {
            _gameplayTimer = GameplayEntry.Instance.ServiceLocator.Get<GameplayTimer>();

            foreach (var evt in pathGrapherAsset.pathData.globalEvents)
            {
                _tickEvents.Add(new GameplayTimer.TickEvent(evt.GlobalTime, evt.ResolveToGameplayEvent().Trigger));
            }
            foreach (var tEvt in _tickEvents)
            {
                _gameplayTimer.RegisterTickEvent(tEvt);
            }
        }
        protected override void OnExit()
        {
            foreach(var tEvt in _tickEvents)
            {
                _gameplayTimer?.UnregisterTickEvent(tEvt);
            }
        }
    }
}
