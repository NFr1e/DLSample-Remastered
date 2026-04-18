using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using DLSample.Shared;

namespace DLSample.Editor.ChartReader
{
    public class OsuChartReader : IChartReader
    {
        public Beat[] Read(string content, float offset)
        {
            if (string.IsNullOrEmpty(content))
            {
                Debug.LogError("EmptyChart");
                return Array.Empty<Beat>();
            }

            var lines = content.Split('\n')
                .Select(l => l.Trim())
                .ToList();

            int hitObjectsIndex = lines.IndexOf("[HitObjects]");
            if (hitObjectsIndex < 0)
            {
                Debug.LogError("Unable find \"HitObjects\" ¡£");
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
