using Engine;
using System;
using Game;

namespace ZanJhat.Core
{
    public class EffectSettings
    {
        public bool Enable { get; set; } = true;

        public Anchor Anchor { get; set; } = Anchor.TopLeft;

        public float MarginX { get; set; } = 72f;

        public float MarginY { get; set; } = 11f;

        public LayoutDirection LayoutDirection { get; set; } = LayoutDirection.Horizontal;

        public float Scale { get; set; } = 0.9f;
    }
}
