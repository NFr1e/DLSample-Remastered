using Cysharp.Threading.Tasks;

namespace DLSample.Framework
{
    /// <summary>
    /// 异步准备接口，实现此接口的模块可在游戏开始前执行异步准备工作
    /// </summary>
    public interface IPrepareAsync
    {
        UniTask PrepareAsync();
    }
}
