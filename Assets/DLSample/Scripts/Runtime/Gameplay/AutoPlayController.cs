using System.Collections.Generic;
using DLSample.Framework;
using DLSample.Facility.Events;
using DLSample.Gameplay.Phase;
using DLSample.Gameplay.Stream;
using DLSample.Shared;
using DLSample.Editor.PathGrapher;

namespace DLSample.Gameplay
{
    public class AutoPlayController : IModule, IModuleRequire<GameplayTimer>, IModuleRequire<GameplayPlayerController>
    {
        public int Priority => DLSampleConsts.Gameplay.PRIORITY_AUTO_PLAY;

        private GameplayTimer _timer;
        private GameplayPlayerController _playerController;
        private readonly EventBus _eventBus;
        private BeatmapDataScriptable _beatmapData;
        private bool _isEnabled;
        private bool _isInGamingState;

        private readonly List<GameplayTimer.TickEvent> _registeredTickEvents = new();
        private Dictionary<int, Waypoint> _waypointMap;

        public AutoPlayController(EventBus eventBus)
        {
            _eventBus = eventBus;
        }

        void IModuleRequire<GameplayTimer>.SetModule(GameplayTimer module) => _timer = module;
        void IModuleRequire<GameplayPlayerController>.SetModule(GameplayPlayerController module) => _playerController = module;

        public void SetBeatmapData(BeatmapDataScriptable beatmapData) => _beatmapData = beatmapData;

        public void SetPathData(PathData pathData)
        {
            if (pathData == null || pathData.generatedWaypoints == null) return;

            _waypointMap = new Dictionary<int, Waypoint>(pathData.generatedWaypoints.Count);
            foreach (var wp in pathData.generatedWaypoints)
            {
                if (!_waypointMap.ContainsKey(wp.beatIndex))
                    _waypointMap.Add(wp.beatIndex, wp);
            }
        }

        public void SetEnabled(bool enabled)
        {
            if (_isEnabled == enabled) return;
            _isEnabled = enabled;

            if (enabled)
            {
                if (_isInGamingState)
                    RegisterBeatTickEvents();
            }
            else
            {
                UnregisterAllTickEvents();
            }
        }

        public void OnInit()
        {
            _eventBus.Subscribe<GameplayEventParams.GameplayStateChangeCtx>(OnStateChange);
        }

        public void OnShutdown()
        {
            _eventBus.Unsubscribe<GameplayEventParams.GameplayStateChangeCtx>(OnStateChange);
            UnregisterAllTickEvents();
        }

        private void OnStateChange(GameplayEventParams.GameplayStateChangeCtx ctx)
        {
            if (ctx.CurrentState is GameplayStates.GamingState)
            {
                if (!_isInGamingState)
                {
                    _isInGamingState = true;
                    if (_isEnabled)
                        RegisterBeatTickEvents();
                }
            }
            else
            {
                if (_isInGamingState)
                {
                    _isInGamingState = false;
                    UnregisterAllTickEvents();
                }
            }
        }

        private void RegisterBeatTickEvents()
        {
            if (_timer == null || _playerController == null || _beatmapData == null) return;

            UnregisterAllTickEvents();

            var beats = _beatmapData.Beats;
            for (int i = 0; i < beats.Count; i++)
            {
                var beat = beats[i];
                if (beat.TimeSecond <= 0.001) continue;

                int beatIndex = i;
                Waypoint? waypoint = _waypointMap != null && _waypointMap.TryGetValue(beatIndex, out var wp) ? wp : null;

                var tickEvent = new GameplayTimer.TickEvent(beat.TimeSecond, () =>
                {
                    if (waypoint.HasValue)
                    {
                        var wp = waypoint.Value;
                        _playerController.MainPlayer.transform.SetPositionAndRotation(wp.position, wp.rotation);
                    }
                    _playerController.PlayerInput();
                });
                _registeredTickEvents.Add(tickEvent);
                _timer.RegisterTickEvent(tickEvent);
            }
        }

        private void UnregisterAllTickEvents()
        {
            foreach (var evt in _registeredTickEvents)
            {
                _timer?.UnregisterTickEvent(evt);
            }
            _registeredTickEvents.Clear();
        }
    }
}
