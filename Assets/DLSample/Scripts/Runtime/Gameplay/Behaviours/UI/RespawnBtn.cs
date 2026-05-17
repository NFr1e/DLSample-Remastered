using UnityEngine;
using UnityEngine.UI;
using DLSample.Facility.Events;

namespace DLSample.Gameplay.Behaviours.UI
{
    /// <summary>
    /// 重生按钮，点击后向事件总线发送重生请求。
    /// </summary>
    public class RespawnBtn : MonoBehaviour
    {
        [SerializeField] private Button button;

        private EventBus _eventBus;
        private readonly GameplayEventParams.RespawnGameRequest _respawnRequest = new();

        private void Awake()
        {
            _eventBus = GameplayEntry.Instance.EventBus;
        }

        private void OnEnable()
        {
            button.onClick.AddListener(Respawn);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(Respawn);
        }

        private void Respawn()
        {
            _eventBus.Invoke(this, _respawnRequest);
        }
    }
}
