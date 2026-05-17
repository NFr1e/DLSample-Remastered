using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace DLSample.Facility.UI
{
    public class TextOmitter : Label
    {
        public bool manualSetUpComponent = false;

        [SerializeField, ShowIf(nameof(manualSetUpComponent))] Text _childText;
        [SerializeField, ShowIf(nameof(manualSetUpComponent))] RectTransform _container;

        public string ellipsis = "...";

        void Awake()
        {
            SetupComponents();
        }

        void SetupComponents()
        {
            if (manualSetUpComponent)
                return;

            _container = GetComponent<RectTransform>();
            _childText = GetComponentInChildren<Text>();
        }

        public override void SetText(string content)
        {
            if (_childText == null || _container == null)
                return;

            _childText.text = content;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_childText.rectTransform);

            var containerWidth = _container.rect.width;

            if (_childText.preferredWidth <= containerWidth)
                return;

            int left = 0;
            int right = content.Length;
            int bestFit = 0;

            while (left <= right)
            {
                int mid = (left + right) / 2;
                string test = content.Substring(0, mid) + ellipsis;

                _childText.text = test;
                LayoutRebuilder.ForceRebuildLayoutImmediate(_childText.rectTransform);

                if (_childText.preferredWidth <= containerWidth)
                {
                    bestFit = mid;
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }

            _childText.text = content.Substring(0, bestFit) + ellipsis;
        }
    }
}
