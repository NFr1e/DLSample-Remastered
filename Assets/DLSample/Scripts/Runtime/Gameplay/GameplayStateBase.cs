namespace DLSample.Gameplay.Phase
{
    public abstract class GameplayStateBase
    {
        protected readonly GameplayFSM _fsm;

        public GameplayStateBase(GameplayFSM fsm)
        {
            _fsm = fsm;
        }

        public abstract void Enter();
        public abstract void Exit();

        public virtual void Init()
        {

        }

        public virtual void Update(float deltaTime)
        {

        }
    }
}
