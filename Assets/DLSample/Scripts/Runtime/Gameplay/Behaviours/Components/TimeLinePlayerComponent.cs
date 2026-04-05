using UnityEngine;
using UnityEngine.Playables;
using DLSample.Gameplay.Stream;

namespace DLSample.Gameplay.Behaviours
{
    public class TimeLinePlayerComponent : GameplayObject
    {
        [SerializeField]
        private PlayableDirector playableDirector;

        private GameplayTimeLinePlayer player;
        private GameplayTimeLineDirector director;

        protected override void OnInit()
        {
            player = new GameplayTimeLinePlayer(playableDirector);
            director = new GameplayTimeLineDirector(player);

            GameplayEntry.Instance.ServiceLocator.Register(player);
            GameplayEntry.Instance.ServiceLocator.Register(director);
        }
        protected override void OnStart()
        {
            GameplayEntry.Instance.ModulesManager.Register(director);
        }
    }
}
