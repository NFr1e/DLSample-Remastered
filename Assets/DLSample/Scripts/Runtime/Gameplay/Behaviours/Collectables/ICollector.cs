using DLSample.Facility.Events;

namespace DLSample.Gameplay.Behaviours
{
    public struct OnCollectEventArgs : IEventArg
    {
        public ICollectable collectable;
        public ICollector collector;
    }

    public interface ICollector
    {
        public interface ICollector
        {
            void Collect(ICollectable collectable);
        }
    }
}
