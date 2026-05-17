namespace DLSample.Gameplay.Behaviours
{
    /// <summary>
    /// 可收集物品接口，定义收集行为与状态。
    /// </summary>
    public interface ICollectable
    {
        /// <summary>
        /// 物品类型标识符。
        /// </summary>
        string TypeId { get; }

        /// <summary>
        /// 是否已被收集。
        /// </summary>
        bool IsCollected { get; }

        /// <summary>
        /// 执行收集操作。
        /// </summary>
        void Collect();

        /// <summary>
        /// 收集完成时触发的事件。
        /// </summary>
        event System.Action OnCollect;
    }
}
