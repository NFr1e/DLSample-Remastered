using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using DLSample.Shared;

namespace DLSample.Editor.ChartReader
{
    /// <summary>
    /// Osu 格式谱面读取器，解析 osu 谱面文件中的 [HitObjects] 节拍数据。
    /// </summary>
    public class OsuChartReader : IChartReader
    {
        /// <summary>
        /// 从 osu 谱面文本中读取节拍数组。
        /// </summary>
        /// <param name="content">谱面文件文本内容</param>
        /// <param name="offset">时间偏移量（秒）</param>
        /// <returns>解析出的节拍数组</returns>
        public Beat[] Read(string content, float offset)
        {
            if (string.IsNullOrEmpty(content))
            {
                Debug.LogError("Chart content is empty.");
                return Array.Empty<Beat>();
            }

            var lines = content.Split('\n')
                .Select(l => l.Trim())
                .ToList();

            int hitObjectsIndex = lines.IndexOf("[HitObjects]");
            if (hitObjectsIndex < 0)
            {
                Debug.LogError("Unable to find \"[HitObjects]\" section in chart file.");
                return Array.Empty<Beat>();
            }

            lines.RemoveRange(0, hitObjectsIndex + 1);
            lines.RemoveAll(string.IsNullOrEmpty);

            var beats = new List<Beat> { new(0) };

            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length < 3)
                    continue;

                if (int.TryParse(parts[2], out int timeMs))
                {
                    double timeSec = timeMs / 1000.0 + offset;
                    beats.Add(new Beat(timeSec));
                }
            }

            return beats.ToArray();
        }
    }
}
