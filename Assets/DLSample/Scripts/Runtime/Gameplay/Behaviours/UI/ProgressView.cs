using UnityEngine;
using UnityEngine.UI;
using DLSample.Facility.UI;
using DG.Tweening;

namespace DLSample.Gameplay.Behaviours.UI
{
    /// <summary>
    /// 进度视图，显示关卡完成百分比与收集宝石数量。
    /// </summary>
    public class ProgressView : MonoBehaviour
    {
        [SerializeField] private Slider percentageSlider;
        [SerializeField] private LabelDisplayer percentageLabel;
        [SerializeField] private LabelDisplayer gemLabel;

        private GameplayResulter _resulter;
        private Tween _sliderTween;

        private void Awake()
        {
            percentageSlider.minValue = 0;
            percentageSlider.maxValue = 100;
        }

        private void Start()
        {
            _resulter = GameplayEntry.Instance.ServiceLocator.Get<GameplayResulter>();
            Display();
        }

        private void OnDestroy()
        {
            _resulter = null;
            _sliderTween = null;
        }

        private void Display()
        {
            if (_resulter is null) return;

            if (percentageSlider)
            {
                _sliderTween?.Kill();
                _sliderTween = percentageSlider.DOValue(_resulter.GetPercentage(), 1f).SetEase(Ease.OutExpo);
            }

            if (percentageLabel.label)
                percentageLabel.SetText($"{_resulter.GetPercentage()}%");

            if (gemLabel.label)
                gemLabel.SetText($"{_resulter.GetGemsCount()}/{_resulter.LevelData.GemCount}");
        }
    }
}
