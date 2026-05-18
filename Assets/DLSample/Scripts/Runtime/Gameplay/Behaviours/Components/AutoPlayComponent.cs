using UnityEngine;
using DLSample.Facility.Events;
using DLSample.Shared;
using DLSample.Editor.PathGrapher;

namespace DLSample.Gameplay.Behaviours
{
    public class AutoPlayComponent : GameplayObject
    {
        [SerializeField] private BeatmapDataScriptable _beatmapData;
        [SerializeField] private PathGrapherAsset _pathGrapherAsset;
        [SerializeField] private bool _isAutoPlayEnabled;

        private AutoPlayController _controller;

        protected override void OnStart()
        {
            if (!_isAutoPlayEnabled) return;

            if (_beatmapData == null)
            {
                Debug.LogWarning("AutoPlayComponent: BeatmapData is not assigned.");
                return;
            }

            var eventBus = GameplayEntry.Instance.ServiceLocator.Get<EventBus>();
            _controller = new AutoPlayController(eventBus);
            _controller.SetBeatmapData(_beatmapData);

            if (_pathGrapherAsset != null)
                _controller.SetPathData(_pathGrapherAsset.pathData);

            _controller.SetEnabled(true);

            GameplayEntry.Instance.ModulesManager.Register(_controller);
        }

        protected override void OnExit()
        {
            if (_controller != null)
            {
                _controller.SetEnabled(false);
                _controller = null;
            }
        }
    }
}
