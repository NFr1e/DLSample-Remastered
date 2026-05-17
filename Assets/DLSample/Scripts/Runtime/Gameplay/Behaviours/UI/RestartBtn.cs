using UnityEngine;
using UnityEngine.UI;
using DLSample.Shared;
using DLSample.App;
using DLSample.Facility.Events;
using DLSample.Facility.SceneManage;

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
            _levelRestarter = new(
                GameplayEntry.Instance.ServiceLocator.Get<LevelDataScriptable>().SceneName,
                AppEntry.SceneManager,
                GameplayEntry.Instance.ServiceLocator.Get<EventBus>());
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
