using DLSample.Gameplay;

namespace DLSample.Editor.PathGrapher
{
    /// <summary>
    /// 路径事件解析器，将路径事件转换为游戏玩法事件
    /// </summary>
    public static class PathEventResolver
    {
        /// <summary>
        /// 将路径事件解析为游戏玩法事件
        /// </summary>
        /// <param name="evt">要解析的路径事件</param>
        /// <returns>对应的游戏玩法事件</returns>
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