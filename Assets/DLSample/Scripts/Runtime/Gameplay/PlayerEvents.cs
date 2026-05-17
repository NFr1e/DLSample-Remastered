using UnityEngine;
using DLSample.Shared;
using DLSample.Facility.Events;

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

            public IEventArg ToEventArg()
            {
                return new PlayerEventsParams.SpeedChangeRequest { Speed = Speed };
            }
        }

        public class GravityChangeEvent : IGameplayEvent
        {
            public Vector3 Gravity { get; set; }
            public double InvokeTime { get; set; } = 0;

            public IEventArg ToEventArg()
            {
                return new PlayerEventsParams.GravityChangeRequest { Gravity = Gravity };
            }
        }

        public class DirectionChangeEvent : IGameplayEvent
        {
            public PlayerDirections Directions { get; set; }
            public double InvokeTime { get; set; } = 0;

            public IEventArg ToEventArg()
            {
                return new PlayerEventsParams.DirectionChangeRequest { Directions = Directions };
            }
        }

        public class ForceTurnEvent : IGameplayEvent
        {
            public double InvokeTime { get; set; } = 0;

            public IEventArg ToEventArg()
            {
                return new PlayerEventsParams.ForceTurnRequest();
            }
        }

        public class TeleportEvent : IGameplayEvent
        {
            public Vector3 Position { get; set; }
            public double InvokeTime { get; set; } = 0;

            public IEventArg ToEventArg()
            {
                return new PlayerEventsParams.TeleportRequest { Position = Position };
            }
        }

        public class JumpEvent : IGameplayEvent
        {
            public Vector3 Velocity { get; set; }
            public double InvokeTime { get; set; } = 0;

            public IEventArg ToEventArg()
            {
                return new PlayerEventsParams.VelocityChangeRequest { Velocity = Velocity };
            }
        }
    }
}
