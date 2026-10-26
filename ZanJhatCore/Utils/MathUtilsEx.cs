using Engine;
using System;
using Game;

namespace ZanJhat.Core
{
    public static class MathUtilsEx
    {
        public const float TwoPi = MathUtils.PI * 2f;

        public static float Fract(float v) => v - MathUtils.Floor(v);

        public static float MoveTowards(float current, float target, float maxDelta)
        {
            if (MathUtils.Abs(target - current) <= maxDelta)
                return target;

            return current + MathUtils.Sign(target - current) * maxDelta;
        }
    }
}