using UnityEngine;
using DLSample.Gameplay.Behaviours;

namespace DLSample.Gameplay.Skin
{
    /// <summary>
    /// 皮肤适配器组件，创建并初始化 GameplaySkinAdapter。
    /// </summary>
    public class SkinAdapterComponent : GameplayObject
    {
        [SerializeField] private GameplayPlayerMove playerMove;
        [SerializeField] private PlayerDamager playerDamager;
        [SerializeField] private Transform headContainer;

        private GameplaySkinAdapter _adapter;
        public GameplaySkinAdapter Adapter => _adapter;

        protected override void OnStart()
        {
            var backtrackHandler = GameplayEntry.Instance.ServiceLocator.Get<BacktrackablesHandler>();

            _adapter = new GameplaySkinAdapter(playerMove, headContainer, backtrackHandler, playerDamager);
            _adapter.Init();

            GameplayEntry.Instance.ServiceLocator.Get<SkinChanger>().AddAdapter(_adapter);
        }

        protected override void OnExit()
        {
            _adapter.Dispose();
            _adapter = null;
        }
    }
}
