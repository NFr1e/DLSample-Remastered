using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace DLSample.Gameplay.Behaviours
{
    public class CameraFollower : MonoBehaviour
    {
        public Transform target;
        public Transform animator;

        public Vector3 defaultOffset = Vector3.zero;
        public Vector3 defaultRotation = new(60f, 45f, 0f);
        public Vector3 defaultScale = Vector3.one;
        public Vector3 followSpeed = new(1.2f, 3f, 6f);

        public bool follow = true;
        public bool smooth = true;

        private Transform origin;

        private readonly Quaternion rotation = Quaternion.Euler(0, -45, 0);

        private Tween _offsetTween, _rotatTween, _scaleTween, _followSpeedTween;

        private Vector3 Translation
        {
            get
            {
                var targetPosition = rotation * target.position;
                var selfPosition = rotation * transform.position;
                return targetPosition - selfPosition;
            }
        }

        public void Init()
        {
            SetDefaultTransform();
            origin = new GameObject("CameraMovementOrigin")
            {
                transform =
                {
                    position = Vector3.zero,
                    rotation = Quaternion.Euler(0, 45, 0),
                    localScale = Vector3.one
                }
            }.transform;
        }

        public void Follow()
        {
            if (!follow) return;

            transform.Translate(smooth ? GetFollowPosition() : Translation, origin);
        }

        public Vector3 GetFollowPosition()
        {
            if (!follow) return Translation;

            var translation = new Vector3(Translation.x * Time.smoothDeltaTime * followSpeed.x,
                Translation.y * Time.smoothDeltaTime * followSpeed.y,
                Translation.z * Time.smoothDeltaTime * followSpeed.z);

            return translation;
        }

        public async UniTaskVoid FocusTarget()
        {
            await UniTask.Yield(PlayerLoopTiming.PreLateUpdate);
            transform.position = target.position;
        }
        public void SetFollow(bool follow)
        {
            this.follow = follow;
        }

        public void SetSmooth(bool smooth)
        {
            this.smooth = smooth;
        }

        public void DoOffset(Vector3 offset, float duration, Ease ease)
        {
            _offsetTween?.Kill();

            _offsetTween = animator
                .DOLocalMove(offset, duration)
                .SetEase(ease);
        }

        public void DoRotate(Vector3 euler, float duration, RotateMode mode, Ease ease)
        {
            _rotatTween?.Kill();

            _rotatTween = animator
                .DOLocalRotate(euler, duration, mode)
                .SetEase(ease);
        }
        public void DoScale(Vector3 scale, float duration, Ease ease)
        {
            _scaleTween?.Kill();

            _scaleTween = animator
                .DOScale(scale, duration)
                .SetEase(ease);
        }

        public void DoFollowSpeed(Vector3 speed, float duration, Ease ease)
        {
            _followSpeedTween?.Kill();

            _followSpeedTween = DOTween
                .To(() => followSpeed,
                    (value) => followSpeed = value,
                    speed,
                    duration)
                .SetEase(ease);
        }

        public void ClearTweens()
        {
            _offsetTween?.Kill();
            _rotatTween?.Kill();
            _scaleTween?.Kill();
            _followSpeedTween?.Kill();
        }
        private void SetDefaultTransform()
        {
            if (animator)
            {
                animator.localPosition = defaultOffset;
                animator.eulerAngles = defaultRotation;
                animator.localScale = defaultScale;
            }
        }

        private void Awake()
        {
            Init();
        }
        private void OnValidate()
        {
            if (!Application.isPlaying)
                SetDefaultTransform();
        }
    }
}
