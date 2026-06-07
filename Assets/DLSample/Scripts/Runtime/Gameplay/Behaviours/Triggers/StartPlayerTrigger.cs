using UnityEngine;

namespace DLSample.Gameplay.Behaviours.Triggers
{
    public class StartPlayerTrigger : GameplayObject
    {
        [SerializeField] private GameplayPlayerMove player;
        [SerializeField] private bool addToController;

        private GameplayPlayerController _controller;

        protected override void OnStart()
        {
            _controller = GameplayEntry.Instance.ServiceLocator.Get<GameplayPlayerController>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if(addToController)
                    _controller.AddPlayer(player);
                player.StartMove();
            }
        }
    }
}
