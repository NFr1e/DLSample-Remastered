using DLSample.Facility.Events;

namespace DLSample.Gameplay
{
    public interface IGameplayEvent
    {
        double InvokeTime { get; }

        void Trigger(EventBus evtBus);
    }
}
