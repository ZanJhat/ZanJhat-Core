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

        public static Color ColorFromHSV(float hue, float saturation = 1f, float value = 1f)
        {
            hue = hue - MathUtils.Floor(hue);
            float h = hue * 6f;
            int i = (int)MathUtils.Floor(h);
            float f = h - i;

            float p = value * (1f - saturation);
            float q = value * (1f - f * saturation);
            float t = value * (1f - (1f - f) * saturation);

            return i switch
            {
                0 => new Color(value, t, p),
                1 => new Color(q, value, p),
                2 => new Color(p, value, t),
                3 => new Color(p, q, value),
                4 => new Color(t, p, value),
                _ => new Color(value, p, q),
            };
        }
    }
}
