using DLSample.Facility.Events;
using DLSample.Framework;
using DLSample.Shared;
using System.Collections.Generic;
using System.Linq;

namespace DLSample.Gameplay
{
    public class BacktrackablesHandler : IModule
    {
        public int Priority => DLSampleConsts.Gameplay.PRIORITY_BACKTRACKABLES_HANDLER;

        private readonly List<IBacktrackable> instances = new();

        public double CurrentBacktrackTime { get; private set; }

        private readonly EventBus _evtBus;

        public BacktrackablesHandler(EventBus evtBus)
        {
            _evtBus = evtBus;
        }

        public void OnInit()
        {
            _evtBus?.Subscribe<CheckpointEventParams.OnCheckpointed>(OnCheckpointed);
            _evtBus?.Subscribe<GameplayEventParams.BacktrackGameRequest>(OnBacktrack);
        }
        public void OnShutdown()
        {
            _evtBus?.Unsubscribe<CheckpointEventParams.OnCheckpointed>(OnCheckpointed);
            _evtBus?.Unsubscribe<GameplayEventParams.BacktrackGameRequest>(OnBacktrack);

            instances.Clear();
        }

        private void OnCheckpointed(CheckpointEventParams.OnCheckpointed ctx)
        {
            CurrentBacktrackTime = ctx.CheckTime;

            foreach (var instance in instances.ToArray())
            {
                instance.GetBacktrackState();
            }
        }
        private void OnBacktrack(GameplayEventParams.BacktrackGameRequest _)
        {
            var sorted = instances.ToArray().OrderBy(p => p.BacktrackPriority);

            foreach (var instance in sorted)
            {
                instance.Backtrack();
            }
        }

        /// <summary>
        /// 注册IBacktrackable对象
        /// </summary>
        public void Register(IBacktrackable backtrackable)
        {
            if (!instances.Contains(backtrackable))
            {
                instances.Add(backtrackable);
            }
        }

        /// <summary>
        /// 注销Backtrackable对象
        /// </summary>
        public void Unregister(IBacktrackable backtrackable)
        {
            instances.Remove(backtrackable);
        }
    }
}
