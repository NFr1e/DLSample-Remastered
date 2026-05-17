using UnityEngine;
using UnityEngine.UI;
using DLSample.Shared;

namespace DLSample.Gameplay.Behaviours.UI
{
    /// <summary>
    /// 重新开始按钮，点击后重新加载当前关卡场景。
    /// </summary>
    public class RestartBtn : MonoBehaviour
    {
        [SerializeField] private Button button;

        private LevelRestarter _levelRestarter;

        private void Awake()
        {
            _levelRestarter = new(GameplayEntry.Instance.ServiceLocator.Get<LevelDataScriptable>().SceneName);
        }

        private void OnEnable()
        {
            button.onClick.AddListener(RestartLevel);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(RestartLevel);
        }

        private void RestartLevel() => _levelRestarter.RestartLevel();
    }
}
