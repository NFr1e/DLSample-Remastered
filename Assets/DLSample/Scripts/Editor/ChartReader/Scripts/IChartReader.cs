using DLSample.Shared;

namespace DLSample.Editor.ChartReader
{
    public interface IChartReader
    {
        Beat[] Read(string content, float offset);
    }
}
