using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using DLSample.Shared;
using DLSample.Gameplay.Stream;

namespace DLSample.Gameplay.Behaviours
{
    public class HintBox : GameplayObject, ICollectable, IBacktrackable
    {
        public float StandardTime;

        [Space(10)]
        [SerializeField] private GameObject triggerEffectPrefab;
        [SerializeField] private Renderer mRenderer;

        public Transform segments;

        public event Action OnCollect;
        public string TypeId => "Collectables.HintBox";
        public bool IsCollected { get; private set; } = false;

        public int BacktrackPriority => DLSampleConsts.Gameplay.BACKTRACK_PRIORITY_COLLECTABLE;

        private GameplayTimer _timer;
        private GameplayPlayerController _playerController;
        private BacktrackablesHandler _backtrack;

        private IPlayerMove _player;

        private float _minTime, _maxTime;

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

            _minTime = StandardTime - DLSampleConsts.Gameplay.HINT_BOX_TRIGGER_INTERVAL;
            _maxTime = StandardTime + DLSampleConsts.Gameplay.HINT_BOX_TRIGGER_INTERVAL;

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

        #region Judge&Collect
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
            return _timer.CurrentTime >= _minTime && _timer.CurrentTime <= _maxTime;
        }

        private void OnPlayerTurn(PlayerMovingArgs _)
        {
            if(_isTriggering && Judged())
            {
                Collect();
            }
        }

        public void Collect()
        {
            if (IsCollected) return;

            IsCollected = true;
            OnCollect?.Invoke();

            mRenderer.enabled = false;

            PlayEffect();
        }

        private void PlayEffect()
        {
            if (_currentEffect)
            {
                Destroy(_currentEffect);
            }

            _currentEffect = Instantiate(triggerEffectPrefab, mRenderer.transform.position, transform.rotation, transform);
            Destroy(_currentEffect, 1f);
        }
        #endregion

        #region HandleSegments
        private void CacheSegments()
        {
            _hintSegments.Clear();

            if (!segments) return;

            _hintSegments.AddRange(segments
                .GetComponentsInChildren<HintLineSegment>(true)
                .OrderBy(segment => segment.DisappearTime));
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

        private void RefreshSegmentsVisibility()
        {
            double currentTime = _timer.CurrentTime;

            foreach (var hintSegment in _hintSegments)
            {
                if (hintSegment == null) continue;

                hintSegment.RefreshVisibility(currentTime);
            }
        }
        #endregion

        public void Backtrack()
        {
            bool hide = StandardTime <= _timer.CurrentTime;
            IsCollected = hide;
            mRenderer.enabled = !hide;

            if (segments && !segments.gameObject.activeSelf)
            {
                segments.gameObject.SetActive(true);
            }

            RefreshSegmentsVisibility();

            if (_currentEffect)
            {
                Destroy(_currentEffect);
            }
        }
    }
}
