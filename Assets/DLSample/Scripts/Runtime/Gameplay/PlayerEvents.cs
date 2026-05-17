using UnityEngine;
using DLSample.Shared;
using DLSample.Facility.Events;
using DLSample.Gameplay.Behaviours;

namespace DLSample.Gameplay
{
    public struct PlayerEventsParams
    {
        public struct PlayerDieArg : IEventArg
        {
            public PlayerDiecause DieCause { get; set; }
            public PlayerMovingArgs MovingArgs { get; set; }
        }
        public struct SpeedChangeRequest : IEventArg
        {
            public float Speed { get; set; }
        }

        public struct GravityChangeRequest : IEventArg
        {
            public Vector3 Gravity { get; set; }
        }

        public struct DirectionChangeRequest : IEventArg
        {
            public PlayerDirections Directions { get; set; }
        }
        public struct ForceTurnRequest : IEventArg
        {

        }

        public struct TeleportRequest : IEventArg
        {
            public Vector3 Position { get; set; }
        }

        public struct VelocityChangeRequest : IEventArg
        {
            public Vector3 Velocity { get; set; }
        }
    }
    public struct PlayerEvents
    {

        public class SpeedChangeEvent : IGameplayEvent
        {
            public float Speed { get; set; } = 12;
            public double InvokeTime { get; set; } = 0;

            private PlayerEventsParams.SpeedChangeRequest _request = new();

            public void Trigger()
            {
                _request.Speed = Speed;
                GameplayEntry.Instance.EventBus.Invoke(this, _request);
            }
        }

        public class GravityChangeEvent : IGameplayEvent
        {
            public Vector3 Gravity { get; set; }
            public double InvokeTime { get; set; } = 0;

            private PlayerEventsParams.GravityChangeRequest _request = new();

            public void Trigger()
            {
                _request.Gravity = Gravity;
                GameplayEntry.Instance.EventBus.Invoke(this, _request);
            }
        }

        public class DirectionChangeEvent : IGameplayEvent
        {
            public PlayerDirections Directions { get; set; }
            public double InvokeTime { get; set; } = 0;

            private PlayerEventsParams.DirectionChangeRequest _request = new();

            public void Trigger()
            {
                _request.Directions = Directions;
                GameplayEntry.Instance.EventBus.Invoke(this, _request);
            }
        }

        public class ForceTurnEvent : IGameplayEvent
        {
            public double InvokeTime{ get; set; } = 0;

            private PlayerEventsParams.ForceTurnRequest _request = new();

            public void Trigger()
            {
                GameplayEntry.Instance.EventBus.Invoke(this, _request);
            }
        }

        public class TeleportEvent : IGameplayEvent
        {
            public Vector3 Position { get; set; }
            public double InvokeTime { get; set; } = 0;

            private PlayerEventsParams.TeleportRequest _request = new();

            public void Trigger()
            {
                _request.Position = Position;
                GameplayEntry.Instance.EventBus.Invoke(this, _request);
            }
        }

        public class JumpEvent : IGameplayEvent
        {
            public Vector3 Velocity { get; set; }
            public double InvokeTime { get; set; } = 0;

            private PlayerEventsParams.VelocityChangeRequest _request = new();

            public void Trigger()
            {
                _request.Velocity = Velocity;
                GameplayEntry.Instance.EventBus.Invoke(this, _request);
            }
        }
    }
}
