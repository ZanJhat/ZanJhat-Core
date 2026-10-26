using System;
using System.Linq;
using System.Collections.Generic;
using Engine;
using Game;

namespace ZanJhat.Core
{
    public static class ColorPalette
    {
        public static readonly Color GameAccentColor = new Color(50, 150, 35);

        public static readonly Color HealthLitColor = new Color(224, 24, 0);

        public static readonly Color AccentBlue = new(102, 102, 255, 255);

        public static readonly Color[] RainbowColors =
        {
            new Color(255, 0, 0), // Red
            new Color(255, 127, 0), // Orange
            new Color(255, 255, 0), // Yellow
            new Color(0, 255, 0), // Green
            new Color(0, 0, 255), // Blue
            new Color(75, 0, 130), // Indigo
            new Color(148, 0, 211) // Violet
};
    }
}
