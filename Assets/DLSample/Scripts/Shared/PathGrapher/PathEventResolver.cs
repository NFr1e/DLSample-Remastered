using DLSample.Gameplay;

namespace DLSample.Editor.PathGrapher
{
    /// <summary>
    /// 将 PathGrapher 路径事件映射为 Gameplay 运行时事件。
    /// 新增 PathEvent 类型时在此添加一个分支即可。
    /// </summary>
    public static class PathEventResolver
    {
        public static IGameplayEvent ToGameplayEvent(IPathEvent evt)
        {
            return evt switch
            {
                SpeedChangeEvent s => new PlayerEvents.SpeedChangeEvent
                {
                    InvokeTime = s.GlobalTime,
                    Speed = s.newSpeed
                },
                GravityChangeEvent g => new PlayerEvents.GravityChangeEvent
                {
                    InvokeTime = g.GlobalTime,
                    Gravity = g.newGravity
                },
                ForceTurnEvent tn => new PlayerEvents.ForceTurnEvent
                {
                    InvokeTime = tn.GlobalTime
                },
                DirectionChangeEvent d => new PlayerEvents.DirectionChangeEvent
                {
                    InvokeTime = d.GlobalTime,
                    Directions = d.newDirections
                },
                TeleportEvent tp => new PlayerEvents.TeleportEvent
                {
                    InvokeTime = tp.EndTime,
                    Position = tp.targetPosition
                },
                JumpEvent j => new PlayerEvents.JumpEvent
                {
                    InvokeTime = j.GlobalTime,
                    Velocity = j.velocity
                },
                _ => null,
            };
        }
    }
}
