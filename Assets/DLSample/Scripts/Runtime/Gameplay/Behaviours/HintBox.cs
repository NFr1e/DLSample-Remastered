using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using DLSample.Shared;
using DLSample.Gameplay.Stream;

namespace DLSample.Gameplay.Behaviours
{
    /// <summary>
    /// 提示盒，在玩家处于触发区域且节拍判定正确时收集，管理提示线分段的可见性，支持回溯。
    /// </summary>
    public class HintBox : GameplayObject, IBacktrackable
    {
        public float StandardTime;
        public float RelevantEndTime { get; private set; }
        public bool IsRuntimeLoaded { get; private set; } = true;

        [Space(10)]
        [SerializeField] private GameObject triggerEffectPrefab;
        [SerializeField] private Renderer mRenderer;

        public Transform segments;

        public bool IsCollected { get; private set; } = false;

        public int BacktrackPriority => DLSampleConsts.Gameplay.BACKTRACK_PRIORITY_COLLECTABLE;

        private GameplayTimer _timer;
        private GameplayPlayerController _playerController;
        private BacktrackablesHandler _backtrack;

        private IPlayerMove _player;

        private float _minJudgeTime, _maxJudgeTime;

        private bool _isTriggering = false;

        private GameObject _currentEffect;
        private readonly List<HintLineSegment> _hintSegments = new();
        private readonly List<GameplayTimer.TickEvent> _segmentTickEvents = new();

        protected override void OnStart()
        {
            _timer = GameplayEntry.Instance.ServiceLocator.Get<GameplayTimer>();
            _playerController = GameplayEntry.Instance.ServiceLocator.Get<GameplayPlayerController>();
            _backtrack = GameplayEntry.Instance.ServiceLocator.Get<BacktrackablesHandler>();
            _player = _playerController.MainPlayer;

            _player.OnTurn += OnPlayerTurn;

            _minJudgeTime = StandardTime - DLSampleConsts.Gameplay.HINT_BOX_TRIGGER_INTERVAL;
            _maxJudgeTime = StandardTime + DLSampleConsts.Gameplay.HINT_BOX_TRIGGER_INTERVAL;

            CacheSegments();
            RegisterSegmentTickEvents();

            _backtrack.Register(this);
        }

        protected override void OnExit()
        {
            _player.OnTurn -= OnPlayerTurn;

            UnregisterSegmentTickEvents();

            _backtrack?.Unregister(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            _isTriggering = true;
        }

        private void OnTriggerStay(Collider other)
        {
            _isTriggering = true;
        }

        private void OnTriggerExit(Collider other)
        {
            _isTriggering = false;
        }

        private bool Judged()
        {
            return _timer.CurrentTime >= _minJudgeTime && _timer.CurrentTime <= _maxJudgeTime;
        }

        private void OnPlayerTurn(PlayerMovingArgs _)
        {
            if (_isTriggering && Judged())
            {
                Collect();
            }
        }

        /// <summary>
        /// 收集提示盒，标记已收集并更新渲染与特效。
        /// </summary>
        public void Collect()
        {
            if (IsCollected) return;

            IsCollected = true;
            RefreshRendererVisibility();

            PlayEffect();
        }

        private void PlayEffect()
        {
            if (triggerEffectPrefab == null || mRenderer == null) return;

            if (_currentEffect)
            {
                Destroy(_currentEffect);
            }

            _currentEffect = Instantiate(triggerEffectPrefab, mRenderer.transform.position, transform.rotation, transform);
            Destroy(_currentEffect, 1f);
        }

        private void CacheSegments()
        {
            _hintSegments.Clear();
            RelevantEndTime = StandardTime;

            if (!segments) return;

            _hintSegments.AddRange(segments
                .GetComponentsInChildren<HintLineSegment>(true)
                .OrderBy(segment => segment.DisappearTime));

            RelevantEndTime = _hintSegments.Count > 0
                ? Mathf.Max(StandardTime, _hintSegments[^1].DisappearTime)
                : StandardTime;
        }

        private void RegisterSegmentTickEvents()
        {
            UnregisterSegmentTickEvents();

            foreach (var hintSegment in _hintSegments)
            {
                var tickEvent = new GameplayTimer.TickEvent(hintSegment.DisappearTime, () => OnSegmentTicked(hintSegment));
                _segmentTickEvents.Add(_timer.RegisterTickEvent(tickEvent));
            }
        }

        private void UnregisterSegmentTickEvents()
        {
            foreach (var tickEvent in _segmentTickEvents)
            {
                _timer?.UnregisterTickEvent(tickEvent);
            }

            _segmentTickEvents.Clear();
        }

        private void OnSegmentTicked(HintLineSegment hintSegment)
        {
            if (hintSegment == null) return;

            hintSegment.SetVisible(false);
        }

        private void RefreshSegmentsVisibility(double currentTime)
        {
            foreach (var hintSegment in _hintSegments)
            {
                if (hintSegment == null) continue;

                hintSegment.RefreshVisibility(currentTime);
            }
        }

        private void RefreshRendererVisibility()
        {
            if (mRenderer == null) return;

            mRenderer.enabled = IsRuntimeLoaded && !IsCollected;
        }

        /// <summary>
        /// 回溯恢复，根据当前时间刷新所有状态。
        /// </summary>
        public void Backtrack()
        {
            _isTriggering = false;
            RefreshByTime(_timer.CurrentTime);

            if (_currentEffect)
            {
                Destroy(_currentEffect);
            }
        }

        /// <summary>
        /// 设置运行时加载状态，控制对象的激活与刷新。
        /// </summary>
        /// <param name="loaded">是否加载。</param>
        public void SetRuntimeLoaded(bool loaded)
        {
            if (IsRuntimeLoaded == loaded)
            {
                if (gameObject.activeSelf != loaded)
                {
                    gameObject.SetActive(loaded);
                }
                return;
            }

            IsRuntimeLoaded = loaded;

            if (!loaded && _currentEffect)
            {
                Destroy(_currentEffect);
            }

            if (!loaded)
            {
                _isTriggering = false;
            }

            gameObject.SetActive(loaded);

            if (loaded)
            {
                RefreshByTime(_timer != null ? _timer.CurrentTime : 0);
            }
        }

        /// <summary>
        /// 根据给定时间刷新收集状态与分段可见性。
        /// </summary>
        /// <param name="currentTime">当前游戏时间。</param>
        public void RefreshByTime(double currentTime)
        {
            IsCollected = StandardTime <= currentTime;
            RefreshRendererVisibility();

            if (segments && !segments.gameObject.activeSelf)
            {
                segments.gameObject.SetActive(true);
            }

            RefreshSegmentsVisibility(currentTime);
        }
    }
}
