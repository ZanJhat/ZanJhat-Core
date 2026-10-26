using Engine;
using System;
using Game;

namespace ZanJhat.Core
{
    public static class TimeUtils
    {
        public enum TimeFormat
        {
            Short,
            Full
        }

        public static string FormatTime(float seconds, TimeFormat format)
        {
            int totalSeconds = (int)seconds;

            int h = totalSeconds / 3600;
            int m = totalSeconds / 60;
            int s = totalSeconds % 60;

            if (format == TimeFormat.Short)
                return $"{m}:{s:00}";

            int mm = (totalSeconds % 3600) / 60;

            if (h > 0)
                return $"{h}:{mm:00}:{s:00}";

            return $"{mm}:{s:00}";
        }
    }
}