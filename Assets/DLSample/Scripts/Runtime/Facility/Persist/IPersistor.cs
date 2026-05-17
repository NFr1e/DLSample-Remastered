namespace DLSample.Facility.Persist
{
    /// <summary>
    /// 由存储层读，写数据
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public interface IPersistor<TData> where TData : class
    {
        TData Load();
        void Save(TData data);
    }
}
