using Engine;
using System;
using Game;

namespace ZanJhat.Core
{
    public static class MathUtilsEx
    {
        public const float TwoPi = MathUtils.PI * 2f;

        public static float Fract(float v) => v - MathUtils.Floor(v);
    }
}