using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace DLSample.Facility.UI
{
    /// <summary>
    /// 检测内容尺寸并执行补间适配动画
    /// </summary>
    public class ContentSizeFitterTweener : MonoBehaviour
    {
        public ContentSizeFitter targetSizeFitter;

        public float tweenDuration = 0.6f;
        public Ease easeType = Ease.OutExpo;

        [HorizontalGroup] public bool horizentalFit = true, verticalFit = true;

        public UnityEvent onFit, onFitted;

        protected RectTransform _rectTrans;

        Tween _sizeFitterTweener;
        List<ContentSizeFitter> _childSizeFitters = new();

        public void DoFit()
        {
            StartCoroutine(FitContentSize());
        }

        protected void RebuildLayouts()
        {
            foreach (var fitter in _childSizeFitters)
            {
                if (fitter != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(fitter.GetComponent<RectTransform>());
            }
        }

        protected IEnumerator FitContentSize()
        {
            targetSizeFitter.enabled = false;

            RebuildLayouts();

            yield return new WaitForEndOfFrame();

            _sizeFitterTweener?.Kill();

            onFit?.Invoke();

            Vector2 targetSize = new Vector2
                (horizentalFit ? ContentSize(_rectTrans).x : _rectTrans.sizeDelta.x,
                verticalFit ? ContentSize(_rectTrans).y : _rectTrans.sizeDelta.y);

            _sizeFitterTweener = _rectTrans
                .DOSizeDelta(targetSize, tweenDuration)
                .SetEase(easeType)
                .SetUpdate(true)
                .OnComplete(OnTweenComplete);
        }

        protected virtual void OnTweenComplete()
        {
            targetSizeFitter.enabled = true;

            onFitted?.Invoke();
        }

        public Vector2 ContentSize(RectTransform rectTransform)
        {
            return new(LayoutUtility.GetPreferredWidth(rectTransform), LayoutUtility.GetPreferredHeight(rectTransform));
        }

        protected virtual void Start()
        {
            if (targetSizeFitter == null)
            {
                Debug.LogError($"{gameObject.name}: targetSizeFitter is null");
                return;
            }

            _childSizeFitters.Clear();

            _rectTrans = targetSizeFitter.GetComponent<RectTransform>();
            _childSizeFitters = targetSizeFitter.GetComponentsInChildren<ContentSizeFitter>(true).ToList();
        }
    }
}
