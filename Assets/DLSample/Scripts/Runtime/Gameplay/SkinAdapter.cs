using DLSample.Gameplay.Behaviours;
using DLSample.Gameplay.Behaviours.Skin;
using DLSample.Shared;
using UnityEngine;

namespace DLSample.Gameplay.Skin
{
    /// <summary>
    /// 持有当前SkinBehaviour实例（通过SkinChanger注入），通过委托实现应用皮肤效果。
    /// 每个adapter对应一个独立的SkinBehaviour实例，避免多Player间的皮肤状态竞争。
    /// </summary>
    public class SkinAdapter : IBacktrackable
    {
        public int BacktrackPriority => DLSampleConsts.Gameplay.BACKTRACK_PRIORITY_SKIN_ADAPTER;

        private readonly IPlayerMove _player;
        private readonly Transform _headContainer;
        private readonly BacktrackablesHandler _backtrackHandler;

        protected SkinBehaviourBase _currentSkinBehaviour;

        /// <summary>
        /// 头部容器Transform，供SkinChanger在实例化皮肤时直接设置。
        /// </summary>
        public Transform HeadContainer => _headContainer;

        public SkinAdapter(IPlayerMove player, Transform headContainer, BacktrackablesHandler backtrackHandler)
        {
            _player = player;
            _headContainer = headContainer;
            _backtrackHandler = backtrackHandler;
        }

        public virtual void Init()
        {
            _player.OnStartMove += OnStartMove;
            _player.OnStopMove += OnStopMove;
            _player.OnMoving += OnPlayerMoving;
            _player.OnTurn += OnPlayerTurn;
            _player.OnLand += OnPlayerLand;

            _backtrackHandler.Register(this);
        }
        public virtual void Dispose()
        {
            _player.OnStartMove -= OnStartMove;
            _player.OnStopMove -= OnStopMove;
            _player.OnMoving -= OnPlayerMoving;
            _player.OnTurn -= OnPlayerTurn;
            _player.OnLand -= OnPlayerLand;

            _backtrackHandler.Unregister(this);
        }

        /// <summary>
        /// 设置当前皮肤行为实例。HeadContainer由SkinChanger在实例化时直接设置。
        /// </summary>
        public void SetCurrentSkin(SkinBehaviourBase skinBehaviour)
        {
            _currentSkinBehaviour = skinBehaviour;
        }

        private void OnStartMove(PlayerMovingArgs arg)
        {
            if (_currentSkinBehaviour != null)
                _currentSkinBehaviour.OnStartMove(arg);
        }

        private void OnStopMove(PlayerMovingArgs arg)
        {
            if (_currentSkinBehaviour != null)
                _currentSkinBehaviour.OnStopMove(arg);
        }

        private void OnPlayerMoving(PlayerMovingArgs arg)
        {
            if (_currentSkinBehaviour != null)
                _currentSkinBehaviour.OnPlayerMoving(arg);
        }

        private void OnPlayerLand(PlayerMovingArgs arg)
        {
            if (_currentSkinBehaviour != null)
                _currentSkinBehaviour.OnPlayerLand(arg);
        }

        private void OnPlayerTurn(PlayerMovingArgs arg)
        {
            if (_currentSkinBehaviour != null)
                _currentSkinBehaviour.OnPlayerTurn(arg);
        }
        public void Backtrack()
        {
            if (_currentSkinBehaviour != null)
                _currentSkinBehaviour.OnReset();
        }
    }
}
