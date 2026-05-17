namespace DLSample.Facility.Input
{
    /// <summary>
    /// 输入层接口，定义输入优先级和层级阻断行为
    /// </summary>
    public interface IInputLayer
    {
        /// <summary>层级名称</summary>
        string Name { get; }

        /// <summary>优先级，值越小优先级越高</summary>
        int Priority { get; }

        /// <summary>是否阻断低于此层级的输入</summary>
        bool BlockLowerLayers { get; }
    }
}
