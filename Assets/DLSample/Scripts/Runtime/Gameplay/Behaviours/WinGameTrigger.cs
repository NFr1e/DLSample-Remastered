using DLSample.Facility.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DLSample.Gameplay.Behaviours
{
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
