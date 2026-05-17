namespace DLSample.Framework
{
    /// <summary>
    /// 游戏模块接口，提供 Init、Update、Shutdown 生命周期方法
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// 模块优先级，通过 ModulesManager 排序，优先创建小优先级模块（值越小优先级越高）
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 模块初始化，在模块注册并排序后调用
        /// </summary>
        virtual void OnInit() { }

        /// <summary>
        /// 模块每帧更新
        /// </summary>
        /// <param name="deltaTime">帧间隔时间（秒）</param>
        virtual void OnUpdate(float deltaTime) { }

        /// <summary>
        /// 模块关闭，释放资源
        /// </summary>
        virtual void OnShutdown() { }
    }
}
