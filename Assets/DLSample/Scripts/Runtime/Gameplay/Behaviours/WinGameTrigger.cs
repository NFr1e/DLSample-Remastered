using DLSample.Facility.Events;
using UnityEngine;

namespace DLSample.Gameplay.Behaviours
{
    /// <summary>
    /// 胜利触发器，玩家进入触发区域时发送胜利请求。
    /// </summary>
    public class WinGameTrigger : MonoBehaviour
    {
        private EventBus _eventBus;
        private readonly GameplayEventParams.WinGameRequest _winRequest = new();

        private void Start()
        {
            _eventBus = GameplayEntry.Instance.EventBus;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _eventBus.Invoke(this, _winRequest);
            }
        }
    }
}
