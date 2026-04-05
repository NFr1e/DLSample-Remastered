using UnityEngine;

namespace DLSample.Gameplay.Behaviours
{
    public class HintLineSegment : MonoBehaviour
    {
        [field: SerializeField]
        public float DisappearTime { get; private set; }

        public void Initialize(float disappearTime)
        {
            DisappearTime = Mathf.Max(disappearTime - 0.3f, 0);
        }

        public void SetVisible(bool isVisible)
        {
            if (gameObject.activeSelf == isVisible) return;

            gameObject.SetActive(isVisible);
        }

        public void RefreshVisibility(double currentTime)
        {
            SetVisible(currentTime < DisappearTime);
        }
    }
}
