using Cysharp.Threading.Tasks;
using DLSample.Facility.EnityFramework;
using DLSample.Shared;
using UnityEngine;

namespace DLSample.Gameplay.Behaviours.Skin
{
    public abstract class StretchTailSkinBehaviour : SkinBehaviourBase
    {
        [SerializeField] private GameObject headPrefab;
        [SerializeField] private GameObject tailPrefab;
        [SerializeField] private ShotParticleEffect landEffectPrefab;
        [SerializeField] private DefaultObstacledEffect obstacledEffectPrefab;
        [SerializeField] private AudioClip obstacledAudioClip;
        [SerializeField] private AudioClip drownAudioClip;

        private GameObject _headInstance;

        private Transform _tailContainer;
        private Transform _tailInstance;
        private Vector3 _tailStartPos;

        private EntityPool<ShotParticleEffect> _landEffectPool;
        private EntityPool<DefaultObstacledEffect> _obstacledEffectPool;

        protected Transform _effectsContainer;

        public override void OnApply()
        {
            if (_headInstance == null)
            {
                _headInstance = Instantiate(headPrefab, _headContainer);
                _headInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(Vector3.zero));
            }

            if (_tailContainer == null)
            {
                _tailContainer = new GameObject("Tail").transform;
            }
            _tailContainer.SetParent(transform);

            if (_effectsContainer == null)
            {
                _effectsContainer = new GameObject("Effects").transform;
            }
            _effectsContainer.SetParent(transform);

            _landEffectPool = new(landEffectPrefab.gameObject, 10, _effectsContainer);
            _landEffectPool.Prewarm(3);

            _obstacledEffectPool = new(obstacledEffectPrefab.gameObject, 2, _effectsContainer);
            _obstacledEffectPool.Prewarm(2);
        }
        public override void OnDetach()
        {
            _landEffectPool?.Dispose();
            _obstacledEffectPool?.Dispose();

            if (_headInstance)
                Destroy(_headInstance);

            if (_tailContainer)
                Destroy(_tailContainer.gameObject);

            if (_effectsContainer)
                Destroy(_effectsContainer.gameObject);
        }

        public override void OnStartMove(PlayerMovingArgs arg)
        {
            if (arg.IsGrounded)
                _tailInstance = CreateTail(arg.Position, arg.Rotation);
        }
        public override void OnPlayerMoving(PlayerMovingArgs arg)
        {
            if (arg.IsGrounded)
            {
                StretchTail(arg.Position);
            }
        }
        public override void OnPlayerLand(PlayerMovingArgs arg)
        {
            if (!arg.IsMoving) return;

            _tailInstance = CreateTail(arg.Position, arg.Rotation);

            if (arg.Velocity.magnitude >= 2 && landEffectPrefab != null)
            {
                _ = PlayShotParticle(_landEffectPool, arg.Position, arg.Rotation, 1.5f);
            }
        }

        public override void OnPlayerTurn(PlayerMovingArgs arg)
        {
            _tailInstance = CreateTail(arg.Position, arg.Rotation);
        }

        public override void OnPlayerDie(PlayerEventsParams.PlayerDieArg arg)
        {
            switch (arg.DieCause)
            {
                case PlayerDiecause.Obstacle:
                    var effect = _obstacledEffectPool.Get();

                    effect.gameObject.transform.position = arg.MovingArgs.Position;
                    effect.BoomClips();
                    AudioHelper.PlayAudioClip(obstacledAudioClip);
                    break;

                case PlayerDiecause.Drown:
                    AudioHelper.PlayAudioClip(drownAudioClip);
                    break;
            }
        }

        #region HelperMethods
        private Transform CreateTail(Vector3 position, Quaternion rotation)
        {
            _tailStartPos = position;

            if (_tailContainer == null)
            {
                _tailContainer = new GameObject("Tail").transform;
                _tailContainer.SetParent(transform);
            }

            var instance = Instantiate(tailPrefab, position, rotation, _tailContainer).transform;

            return instance;
        }
        private void StretchTail(Vector3 headPosition)
        {
            if (_tailInstance == null) return;

            var tailSize = tailPrefab.transform.localScale.z;

            _tailInstance.localScale =
                new(_tailInstance.localScale.x,
                    _tailInstance.localScale.y,
                    Vector3.Distance(_tailStartPos, headPosition) + tailSize);

            _tailInstance.position = (_tailStartPos + headPosition) / 2;
        }
        protected async UniTaskVoid PlayShotParticle(EntityPool<ShotParticleEffect> pool, Vector3 pos, Quaternion rotation, float lifeTime)
        {
            var effect = pool.Get();

            effect.transform.SetPositionAndRotation(pos, rotation);
            effect.Particle.Play();

            await UniTask.Delay((int)(lifeTime * 1000));
            _landEffectPool.Return(effect);
        }
        #endregion

        #region Backtrack
        public override void OnReset()
        {
            Destroy(_tailContainer.gameObject);

            _landEffectPool?.ReturnAll();
            _obstacledEffectPool?.ReturnAll();
        }

        #endregion
    }
}
