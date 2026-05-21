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
        private EventBus _evtBus;
        private readonly List<GameplayTimer.TickEvent> _tickEvents = new();

        protected override void OnStart()
        {
            _gameplayTimer = GameplayEntry.Instance.ServiceLocator.Get<GameplayTimer>();
            _evtBus = GameplayEntry.Instance.ServiceLocator.Get<EventBus>();
            RegisterEvents();
        }

        protected override void OnExit()
        {
            UnregisterEvents();
        }

        /// <summary>
        /// 运行时切换 PathGrapherAsset，立即注销旧事件并注册新事件
        /// </summary>
        public void SwitchAsset(PathGrapherAsset newAsset)
        {
            if (newAsset == null || newAsset == pathGrapherAsset) return;

            UnregisterEvents();
            _tickEvents.Clear();

            pathGrapherAsset = newAsset;

            if (_gameplayTimer != null)
                RegisterEvents();
        }

        private void RegisterEvents()
        {
            foreach (var evt in pathGrapherAsset.pathData.globalEvents)
            {
                var gameplayEvent = PathEventResolver.ToGameplayEvent(evt);
                _tickEvents.Add(new GameplayTimer.TickEvent(evt.GlobalTime, () =>
                {
                    gameplayEvent.Trigger(_evtBus);
                }));
            }
            foreach (var tEvt in _tickEvents)
                _gameplayTimer.RegisterTickEvent(tEvt);
        }

        private void UnregisterEvents()
        {
            foreach (var tEvt in _tickEvents)
                _gameplayTimer?.UnregisterTickEvent(tEvt);
        }
    }
}
