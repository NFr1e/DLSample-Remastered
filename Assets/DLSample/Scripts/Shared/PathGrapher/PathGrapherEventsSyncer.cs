using UnityEngine;
using DLSample.Gameplay;
using DLSample.Gameplay.Stream;
using DLSample.Gameplay.Behaviours;
using DLSample.Facility.Events;
using System.Collections.Generic;

namespace DLSample.Editor.PathGrapher
{
    /// <summary>
    /// 路径图事件同步器，将路径图资产中的事件注册到游戏计时器中
    /// </summary>
    public class PathGrapherEventsSyncer : GameplayObject
    {
        [SerializeField] private PathGrapherAsset pathGrapherAsset;

        private GameplayTimer _gameplayTimer;
        private readonly List<GameplayTimer.TickEvent> _tickEvents = new();

        protected override void OnStart()
        {
            _gameplayTimer = GameplayEntry.Instance.ServiceLocator.Get<GameplayTimer>();
            var evtBus = GameplayEntry.Instance.ServiceLocator.Get<EventBus>();

            foreach (var evt in pathGrapherAsset.pathData.globalEvents)
            {
                var gameplayEvent = evt.ResolveToGameplayEvent();
                _tickEvents.Add(new GameplayTimer.TickEvent(evt.GlobalTime, () =>
                {
                    evtBus.Invoke(this, gameplayEvent.ToEventArg());
                }));
            }
            foreach (var tEvt in _tickEvents)
            {
                _gameplayTimer.RegisterTickEvent(tEvt);
            }
        }
        protected override void OnExit()
        {
            foreach (var tEvt in _tickEvents)
            {
                _gameplayTimer?.UnregisterTickEvent(tEvt);
            }
        }
    }
}
