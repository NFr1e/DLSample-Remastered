using UnityEngine;
using DLSample.Facility.Events;
using DLSample.Shared;

namespace DLSample.Gameplay.Phase
{
    public class GameplayStates
    {
        #region GameTime
        public class WaitingState : GameplayStateBase
        {
            public WaitingState(GameplayFSM fsm) : base(fsm) { }

            public override void Enter() { }
            public override void Exit() { }

            public override string ToString() => $"\'<color=grey>{GetType().Name}</color>\'";
        }
        public class PreparingState : GameplayStateBase
        {
            public PreparingState(GameplayFSM fsm) : base(fsm) { }

            public override void Enter() { }
            public override void Exit() { }

            public override string ToString() => $"\'<color=#FF6347>{GetType().Name}</color>\'";

        }

        public class GamingState : GameplayStateBase
        {
            public GamingState(GameplayFSM fsm) : base(fsm) { }

            public override void Enter() { }
            public override void Exit() { }

            public override string ToString() => $"\'<color=#32CD32>{GetType().Name}</color>\'";
        }

        public class PauseState : GameplayStateBase
        {
            private float _prevTimeScale = 1;

            public PauseState(GameplayFSM fsm) : base(fsm) { }

            public override void Init()
            {
                _prevTimeScale = Time.timeScale;
            }
            public override void Enter()
            {
                Time.timeScale = 0;
            }
            public override void Exit()
            {
                Time.timeScale = _prevTimeScale;
            }

            public override string ToString() => $"\'<color=#5F9EA0>{GetType().Name}</color>\'";
        }

        public class OverState : GameplayStateBase
        {
            public OverState(GameplayFSM fsm) : base(fsm) { }

            public override void Enter() { }
            public override void Exit() { }

            public override string ToString() => $"\'<color=red>{GetType().Name}</color>\'";
        }

        public class RespawnState : GameplayStateBase
        {
            private readonly float _maskInDuration = 0.2f;
            private readonly float _maskOutDuration = 0.4f;

            private readonly EventBus _evtBus;
            private readonly GameplayEventParams.BacktrackGameRequest _backtrackRequest = new();

            public RespawnState(GameplayFSM fsm) : base(fsm)
            {
                _evtBus = GameplayEntry.Instance.EventBus;
            }

            public override void Enter()
            {
                ScreenHelper.FullScreenMaskAction(RequestBacktrack, _maskInDuration, _maskOutDuration);
            }
            public override void Exit() { }

            public override string ToString() => $"\'<color=magenta>{GetType().Name}</color>\'";

            private void RequestBacktrack()
            {
                _evtBus.Invoke(this, _backtrackRequest);
                _fsm.SetCurrentState<PreparingState>();
            }
        }
        public class Exiting : GameplayStateBase
        {
            public Exiting(GameplayFSM fsm) : base(fsm) { }

            public override void Enter() { }
            public override void Exit() { }
            public override string ToString() => $"\'<color=white>{GetType().Name}</color>\'";
        }
        #endregion
    }
}
