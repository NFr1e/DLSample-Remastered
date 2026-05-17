namespace DLSample.Gameplay
{
    public interface IBacktrackable
    {
        int BacktrackPriority { get; }
        /// <summary>
        /// 在此保存回溯数据
        /// </summary>
        virtual void GetBacktrackState() { }

        /// <summary>
        /// 回溯时调用，恢复到上一次保存的数据
        /// </summary>
        void Backtrack();
    }
}
