using UnityEngine;
using UnityEngine.UI;
using DLSample.Shared;

namespace DLSample.Gameplay.Behaviours.UI
{
    /// <summary>
    /// 音画同步延迟调节视图
    /// </summary>
    public class SyncDelayView : MonoBehaviour
    {
        private const float SNAP_INTERVAL = 0.05f;
        private const float MIN_DELAY = -1f;
        private const float MAX_DELAY = 1f;

        [SerializeField] private Slider _slider;
        [SerializeField] private Text _valueText;

        private bool _isUpdating;

        private void Start()
        {
            var saved = PlayerPrefs.GetFloat(DLSampleConsts.SaveAndLoad.ID_SYNC_DELAY, 0f);
            var snapped = Snap(saved);

            _slider.minValue = MIN_DELAY;
            _slider.maxValue = MAX_DELAY;
            _slider.value = snapped;
            _slider.onValueChanged.AddListener(OnSliderChanged);

            UpdateText(snapped);
        }

        private void OnDestroy()
        {
            _slider?.onValueChanged.RemoveListener(OnSliderChanged);
        }

        private void OnSliderChanged(float rawValue)
        {
            if (_isUpdating) return;

            var snapped = Snap(rawValue);
            if (Mathf.Approximately(snapped, rawValue)) return;

            _isUpdating = true;
            _slider.value = snapped;
            _isUpdating = false;

            PlayerPrefs.SetFloat(DLSampleConsts.SaveAndLoad.ID_SYNC_DELAY, snapped);
            PlayerPrefs.Save();
            UpdateText(snapped);
        }

        private static float Snap(float value)
        {
            return Mathf.Round(value / SNAP_INTERVAL) * SNAP_INTERVAL;
        }

        private void UpdateText(float value)
        {
            var sign = value >= 0f ? "+" : "";
            _valueText.text = $"{sign}{value:F2}s";
        }
    }
}
