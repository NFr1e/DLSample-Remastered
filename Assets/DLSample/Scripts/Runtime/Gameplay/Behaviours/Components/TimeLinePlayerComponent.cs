using UnityEngine;
using UnityEngine.Playables;
using DLSample.Gameplay.Stream;

namespace DLSample.Gameplay.Behaviours
{
    /// <summary>
    /// 时间轴播放器组件，负责创建并注册时间轴播放器与导演模块。
    /// </summary>
    public class TimeLinePlayerComponent : GameplayObject
    {
        [SerializeField]
        private PlayableDirector playableDirector;

        private GameplayTimeLinePlayer _player;
        private GameplayTimeLineDirector _director;

        protected override void OnInit()
        {
            _player = new GameplayTimeLinePlayer(playableDirector);
            _director = new GameplayTimeLineDirector(_player);

            GameplayEntry.Instance.ServiceLocator.Register(_player);
            GameplayEntry.Instance.ServiceLocator.Register(_director);
        }

        protected override void OnStart()
        {
            GameplayEntry.Instance.ModulesManager.Register(_director);
        }
    }
}
