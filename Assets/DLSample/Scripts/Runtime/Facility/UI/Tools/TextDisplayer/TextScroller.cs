using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace DLSample.Facility.UI
{
    public class TextScroller : Label
    {
        public Vector2 startPosition;
        [Range(10f, 100f)] public float speed = 10f;
        [Range(0.5f, 3f)] public float delay = 1f;
        public Ease easeType = Ease.Linear;
        public bool autoPlay = true;

        Text _contentText;
        RectTransform _contentTransform;
        RectTransform _containerTransform;
        Sequence _scrollSequence;

        void Awake()
        {
            SetupComponents();
        }

        void SetupComponents()
        {
            _containerTransform = GetComponent<RectTransform>();
            _contentText = GetComponentInChildren<Text>();

            _contentTransform = _contentText.GetComponent<RectTransform>();
        }

        public async override void SetText(string text)
        {
            _scrollSequence?.Kill();
            _contentText.text = text;

            await UniTask.Yield();

            RefreshLayout();
        }

        public void RefreshLayout()
        {
            _scrollSequence?.Kill();

            LayoutRebuilder.ForceRebuildLayoutImmediate(_contentTransform);

            float textWidth = _contentText.preferredWidth;
            float containerWidth = _containerTransform.rect.width;

            if (textWidth <= containerWidth)
            {
                SetStaticPosition(textWidth, containerWidth);
            }
            else
            {
                CreateScrollSequence(textWidth, containerWidth);
            }
        }

        void SetStaticPosition(float textWidth, float containerWidth)
        {
            _contentTransform.DOAnchorPosX(startPosition.x, 0.3f).SetEase(Ease.OutQuad);
        }

        void CreateScrollSequence(float textWidth, float containerWidth)
        {
            _scrollSequence = DOTween.Sequence();

            SetupCenterAlignedScroll(textWidth, containerWidth);
            _scrollSequence.SetLoops(-1, LoopType.Restart);
        }

        void SetupCenterAlignedScroll(float textWidth, float containerWidth)
        {
            float offset = textWidth - containerWidth;
            float scrollDuration = offset / speed;

            _contentTransform.anchoredPosition = startPosition;

            _scrollSequence.Append(_contentTransform.DOAnchorPosX(-offset, scrollDuration).SetEase(easeType));
            _scrollSequence.AppendInterval(delay);

            _scrollSequence.Append(_contentTransform.DOAnchorPosX(startPosition.x, scrollDuration).SetEase(easeType));
            _scrollSequence.AppendInterval(delay);
        }

        void OnDestroy()
        {
            _scrollSequence?.Kill();
        }
    }
}
