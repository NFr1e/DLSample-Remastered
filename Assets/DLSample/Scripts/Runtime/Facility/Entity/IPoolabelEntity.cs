namespace DLSample.Facility.EnityFramework
{
    /// <summary>
    /// 可池化实体接口，提供入池和出池回调
    /// </summary>
    public interface IPoolabelEntity : IEntity
    {
        /// <summary>
        /// 实体放回对象池时调用
        /// </summary>
        void OnEnpool();

        /// <summary>
        /// 实体从对象池取出时调用
        /// </summary>
        void OnDepool();
    }
}
