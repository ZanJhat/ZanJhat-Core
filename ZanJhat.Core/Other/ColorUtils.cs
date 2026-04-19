using System;
using System.Linq;
using System.Collections.Generic;
using Engine;
using Game;

namespace ZanJhat.Core
{
    public static class ColorUtils
    {
        public static Color BlendWithDistance(Color bottomColor, Color topColor, int distance, int maxDistance, float minFactor) => BlendWithDistance(bottomColor, topColor, (float)distance, (float)maxDistance, minFactor);

        public static Color BlendWithDistance(Color bottomColor, Color topColor, float distance, float maxDistance, float minFactor)
        {
            if (maxDistance <= 0f)
                return topColor;

            float baseAlpha = topColor.A / 255f;

            float t = MathUtils.Clamp(distance / maxDistance, 0f, 1f);

            float distanceFactor = minFactor + (1f - minFactor) * t;

            float alpha = baseAlpha * distanceFactor;

            Color blended = Color.Lerp(bottomColor, topColor, alpha);
            blended.A = 255;

            return blended;
        }

        public static Color GetRainbowColor() => GetRainbowColor((float)Time.RealTime);

        public static Color GetRainbowColor(float time)
        {
            time = MathUtilsEx.Fract(time);
            Vector3 rgb = Color.HsvToRgb(new Vector3(time * 360f, 1f, 1f));
            return new Color(rgb);
        }
    }
}
