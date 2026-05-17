using DLSample.Shared;

namespace DLSample.Editor.ChartReader
{
    /// <summary>
    /// 谱面读取器接口，定义从文本内容解析节拍数据的方法。
    /// </summary>
    public interface IChartReader
    {
        /// <summary>
        /// 从谱面文本中读取节拍数组。
        /// </summary>
        /// <param name="content">谱面文件文本内容</param>
        /// <param name="offset">时间偏移量（秒）</param>
        /// <returns>解析出的节拍数组</returns>
        Beat[] Read(string content, float offset);
    }
}
