using DLSample.Gameplay;

namespace DLSample.Editor.PathGrapher
{
    public static class PathEventResolver
    {
        public static IGameplayEvent ResolveToGameplayEvent(this IPathEvent evt)
        {
            IGameplayEvent gameplayEvt;

            switch (evt)
            {
                case SpeedChangeEvent spd:
                    var speedEvt = new PlayerEvents.SpeedChangeEvent
                    {
                        InvokeTime = spd.GlobalTime,
                        Speed = spd.newSpeed
                    };

                    gameplayEvt = speedEvt;
                    break;

                case GravityChangeEvent g:
                    var gravityEvt = new PlayerEvents.GravityChangeEvent
                    {
                        InvokeTime = g.GlobalTime,
                        Gravity = g.newGravity
                    };

                    gameplayEvt = gravityEvt;
                    break;

                case ForceTurnEvent tn:
                    var turnEvent = new PlayerEvents.ForceTurnEvent
                    {
                        InvokeTime = tn.GlobalTime,
                    };

                    gameplayEvt = turnEvent;
                    break;

                case DirectionChangeEvent dir:
                    var directionEvt = new PlayerEvents.DirectionChangeEvent
                    {
                        InvokeTime = dir.GlobalTime,
                        Directions = dir.newDirections
                    };

                    gameplayEvt = directionEvt;
                    break;

                case TeleportEvent tp:
                    var tpEvt = new PlayerEvents.TeleportEvent
                    {
                        InvokeTime = tp.GlobalTime,
                        Position = tp.targetPosition,
                    };

                    gameplayEvt = tpEvt;
                    break;

                case JumpEvent j:
                    var jpEvt = new PlayerEvents.JumpEvent
                    {
                        InvokeTime = j.GlobalTime,
                        Velocity = j.velocity
                    };
                    gameplayEvt = jpEvt;
                    break;

                default:
                    gameplayEvt = null;
                    break;
            }

            return gameplayEvt;
        }
    }
}