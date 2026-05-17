namespace DLSample.Facility.Persist
{
    /// <summary>
    /// 恢复数据
    /// </summary>
    public interface IRestorer
    {
        int Order { get; }
        void Restore();
    }
}
