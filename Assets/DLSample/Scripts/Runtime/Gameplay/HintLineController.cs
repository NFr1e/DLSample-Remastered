using UnityEngine;
using DLSample.Shared;
using DLSample.Framework;
using DLSample.Facility.Events;

namespace DLSample.Gameplay
{
    public struct HintLineEventsParams
    {
        public struct HintLineStateChangeRequest : IEventArg { }

        public struct HintLineStateChanged : IEventArg
        {
            public bool IsOn { get; set; }
        }
    }

    public class HintLineController : IModule
    {
        int IModule.Priority => DLSampleConsts.Gameplay.PRIORITY_HINT_LINE_CONTROLLER;

        private readonly GameObject _hintlineGroup;
        private readonly EventBus _eventBus;

        private bool _isOn;
        public bool IsOn => _isOn;

        public HintLineController(GameObject hintlineGroup)
        {
            _eventBus = GameplayEntry.Instance.EventBus;

            _hintlineGroup = hintlineGroup;
        }

        public void OnInit()
        {
            SubscribeEvents();

            LoadHintLineState();
        }
        public void OnExit()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _eventBus.Subscribe<HintLineEventsParams.HintLineStateChangeRequest>(OnHintLineStateChangeRequested);
        }
        private void UnsubscribeEvents()
        {
            _eventBus.Unsubscribe<HintLineEventsParams.HintLineStateChangeRequest>(OnHintLineStateChangeRequested);
        }

        private void OnHintLineStateChangeRequested(HintLineEventsParams.HintLineStateChangeRequest arg)
        {
            Toggle();
        }

        public void Toggle()
        {
            SetState(!_isOn);
        }

        public void SetState(bool isOn)
        {
            if (_isOn == isOn && _hintlineGroup.activeSelf == isOn)
            {
                return;
            }

            _isOn = isOn;
            ApplyState(isOn);
            SaveState(isOn);
            NotifyStateChanged(isOn);
        }

        private void LoadHintLineState()
        {
            var savedState = PlayerPrefs.GetInt(DLSampleConsts.SaveAndLoad.ID_HINTLINE_STATE, 1) == 1;
            _isOn = savedState;

            ApplyState(savedState);
            NotifyStateChanged(savedState);
        }

        private void ApplyState(bool isOn)
        {
            _hintlineGroup.SetActive(isOn);
        }

        private void SaveState(bool isOn)
        {
            PlayerPrefs.SetInt(DLSampleConsts.SaveAndLoad.ID_HINTLINE_STATE, isOn ? 1 : 0);
        }

        private void NotifyStateChanged(bool isOn)
        {
            _eventBus.Invoke(this, new HintLineEventsParams.HintLineStateChanged
            {
                IsOn = isOn
            });
        }
    }
}
