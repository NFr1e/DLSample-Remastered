using DLSample.Gameplay.Stream;
using DLSample.Shared;
using System;
using UnityEngine;

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

        protected override void OnStart()
        {
            _timer = GameplayEntry.Instance.ServiceLocator.Get<GameplayTimer>();
            _playerController = GameplayEntry.Instance.ServiceLocator.Get<GameplayPlayerController>();
            _backtrack = GameplayEntry.Instance.ServiceLocator.Get<BacktrackablesHandler>();
            _player = _playerController.MainPlayer;

            _player.OnTurn += OnPlayerTurn;

            _minTime = StandardTime - DLSampleConsts.Gameplay.HINT_BOX_TRIGGER_INTERVAL;
            _maxTime = StandardTime + DLSampleConsts.Gameplay.HINT_BOX_TRIGGER_INTERVAL;
            _backtrack.Register(this);
        }
        protected override void OnExit()
        {
            _player.OnTurn -= OnPlayerTurn;
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
            if(segments) segments.gameObject.SetActive(false);

            if (_currentEffect)
            {
                Destroy(_currentEffect);
            }
            _currentEffect = Instantiate(triggerEffectPrefab, mRenderer.transform.position, Quaternion.identity, transform);
            Destroy(_currentEffect, 1f);
        }
        public void Backtrack()
        {
            if (StandardTime > _timer.CurrentTime)
            {
                IsCollected = false;
                mRenderer.enabled = true;
                if (segments) segments.gameObject.SetActive(true);

                if (_currentEffect)
                {
                    Destroy(_currentEffect);
                }
            }
            else
            {
                IsCollected = true;
                mRenderer.enabled = false;
                if (segments) segments.gameObject.SetActive(false);
            }
        }
    }
}
