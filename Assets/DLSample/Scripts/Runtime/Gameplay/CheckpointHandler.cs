using DLSample.Gameplay.Phase;
using DLSample.Facility.Events;
using DLSample.Gameplay.Behaviours;
using DLSample.Framework;
using DLSample.Shared;

namespace DLSample.Gameplay
{
    public struct CheckpointEventParams
    {
        public struct OnCheckpointed : IEventArg
        {
            public Checkpoint Checkpoint { get; set; }
            public double CheckTime { get; set; }
        }
        public struct OnConsumeCheckpoint : IEventArg
        {
            public Checkpoint Checkpoint { get; set; }
        }
    }

    public class CheckpointHandler : IModule
    {
        public int Priority => DLSampleConsts.Gameplay.PRIORITY_CHECKPOINT_HANDLER;

        private Checkpoint _currentCheckpoint;

        private readonly EventBus _evBus;
        private CheckpointEventParams.OnCheckpointed _onCheckpointedCtx = new();
        private CheckpointEventParams.OnConsumeCheckpoint _onConsumeCheckpointCtx = new();

        private bool isRespawned = false;

        public bool IsCheckpointed { get; private set; } = false;
        public Checkpoint CurrentCheckpoint => _currentCheckpoint;

        public CheckpointHandler(EventBus eventBus)
        {
            _evBus = eventBus;
        }

        public void OnInit()
        {
            _evBus.Subscribe<GameplayEventParams.GameplayStateChangeCtx>(OnStateChange);
            _evBus.Subscribe<GameplayEventParams.RespawnGameRequest>(OnRespawn);
        }
        public void OnShutdown()
        {
            _currentCheckpoint = null;
            _evBus.Unsubscribe<GameplayEventParams.GameplayStateChangeCtx>(OnStateChange);
            _evBus.Unsubscribe<GameplayEventParams.RespawnGameRequest>(OnRespawn);
        }
        public void Check(Checkpoint checkpoint)
        {
            if (checkpoint != null)
            {
                IsCheckpointed = true;

                if (_currentCheckpoint == checkpoint) return;

                _currentCheckpoint = checkpoint;

                _onCheckpointedCtx.CheckTime = checkpoint.CheckTime;
                _onCheckpointedCtx.Checkpoint = checkpoint;
                _evBus?.Invoke(this, _onCheckpointedCtx);
            }
        }
        public void Consume()
        {
            if (IsCheckpointed is false) return;

            _currentCheckpoint.Consume();

            _onConsumeCheckpointCtx.Checkpoint = _currentCheckpoint;
            _evBus?.Invoke(this, _onConsumeCheckpointCtx);
        }

        private void OnRespawn(GameplayEventParams.RespawnGameRequest _)
        {
            isRespawned = true;
        }
        private void OnStateChange(GameplayEventParams.GameplayStateChangeCtx ctx)
        {
            switch (ctx.CurrentState)
            {
                case GameplayStates.GamingState:
                    if (IsCheckpointed && isRespawned)
                    {
                        Consume();
                        isRespawned = false;
                    }
                    break;
            }
        }
    }
}
