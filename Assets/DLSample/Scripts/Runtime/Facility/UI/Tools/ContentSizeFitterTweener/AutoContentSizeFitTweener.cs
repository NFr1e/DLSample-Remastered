using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DLSample.Facility.UI
{
    /// <summary>
    /// 自动检测内容尺寸变化并执行补间适配动画
    /// </summary>
    public class AutoContentSizeFitTweener : ContentSizeFitterTweener
    {
        public float sizeChangeThreshold = 0.5f;

        Vector2 _lastRecordedSize, _currentSize;

        protected override void Start()
        {
            base.Start();

            RebuildLayouts();

            _lastRecordedSize = ContentSize(_rectTrans);
            _currentSize = ContentSize(_rectTrans);

            targetSizeFitter.enabled = false;
        }

        void LateUpdate()
        {
            _currentSize = ContentSize(_rectTrans);

            CheckSizeChange();
        }
        protected override void OnTweenComplete()
        {
            onFitted?.Invoke();
        }

        void CheckSizeChange()
        {
            float sizeDiff = Vector2.Distance(_lastRecordedSize, _currentSize);

            if (sizeDiff > sizeChangeThreshold)
            {
                _lastRecordedSize = _currentSize;

                DoFit();
            }
        }
    }
}
