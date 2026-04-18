using UnityEngine;

namespace DLSample.Gameplay.Behaviours
{
    public class HintLineControllerComponent : GameplayObject
    {
        [SerializeField] private GameObject hintLineGroup;
        private HintLineController _controller;

        protected override void OnInit()
        {
            _controller = new HintLineController(hintLineGroup);

            GameplayEntry.Instance.ServiceLocator.Register<HintLineController>(_controller);
        }
        protected override void OnStart()
        {
            GameplayEntry.Instance.ModulesManager.Register(_controller);
        }
    }
}
