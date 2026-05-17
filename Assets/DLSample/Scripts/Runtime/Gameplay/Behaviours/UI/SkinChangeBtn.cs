using Cysharp.Threading.Tasks;
using DLSample.App;
using DLSample.Facility;
using DLSample.Facility.Events;
using DLSample.Gameplay.Skin;
using UnityEngine;
using UnityEngine.UI;

namespace DLSample.Gameplay.Behaviours.UI
{
    public class SkinChangeBtn : GameplayObject
    {
        [SerializeField] private Button button;
        [SerializeField] private string skinId = "skin.default";

        [SerializeField] private GameObject indicator;

        private EventBus _evtBus;
        private ServiceLocator _serviceLocator;
        private ChangeSkinRequest _request = new();

        private SkinsHandler _skinsHandler;

        protected override void OnInit()
        {
            _evtBus = AppEntry.EventBus;
            _serviceLocator = GameplayEntry.Instance.ServiceLocator;

            _request.SkinId = skinId;
        }
        protected override void OnStart()
        {
            _skinsHandler = _serviceLocator.Get<SkinsHandler>();
            _evtBus.Subscribe<ChangeSkinRequest>(OnSkinChanged);

            indicator.SetActive(false);
            _ = RefreshIndicator();
        }
        protected override void OnExit()
        {
            _evtBus.Unsubscribe<ChangeSkinRequest>(OnSkinChanged);
        }

        private void OnEnable()
        {
            button.onClick.AddListener(OnClick);
        }
        private void OnDisable()
        {
            button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            _evtBus.Invoke(this, _request);
        }

        private async UniTaskVoid RefreshIndicator()
        {
            await UniTask.Yield();

            if (_skinsHandler is null) return;

            var @bool = _skinsHandler.CurrentSkinId == skinId;
            indicator.SetActive(@bool);
        }
        private void OnSkinChanged(ChangeSkinRequest request)
        {
            _ = RefreshIndicator();
        }
    }
}
