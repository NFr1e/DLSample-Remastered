using DLSample.Facility.Events;
using UnityEngine;
using UnityEngine.UI;

namespace DLSample.Gameplay.Behaviours.UI
{
    /// <summary>
    /// 提示线开关视图，控制提示线的显示与隐藏。
    /// </summary>
    public class HintLineToggleView : GameplayObject
    {
        [SerializeField] private Button button;
        [SerializeField] private GameObject mark;

        private EventBus _eventBus;
        private HintLineController _controller;

        private bool _isAvailable = false;

        protected override void OnInit()
        {
            _eventBus = GameplayEntry.Instance.ServiceLocator.Get<EventBus>();

            button.onClick.AddListener(OnButtonClicked);
            _eventBus.Subscribe<HintLineEventsParams.HintLineStateChanged>(OnHintLineStateChanged);
        }

        protected override void OnStart()
        {
            _isAvailable = GameplayEntry.Instance.ServiceLocator.TryGet(out _controller);

            if (!_isAvailable)
            {
                gameObject.SetActive(false);
                return;
            }

            UpdateView(_controller.IsOn);
        }

        protected override void OnExit()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClicked);
            }

            _eventBus?.Unsubscribe<HintLineEventsParams.HintLineStateChanged>(OnHintLineStateChanged);
        }

        private void OnButtonClicked()
        {
            _eventBus.Invoke(this, new HintLineEventsParams.HintLineStateChangeRequest());
        }

        private void OnHintLineStateChanged(HintLineEventsParams.HintLineStateChanged arg)
        {
            UpdateView(arg.IsOn);
        }

        private void UpdateView(bool isOn)
        {
            mark.SetActive(isOn);
        }
    }
}
