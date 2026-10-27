using Engine;
using System;
using Game;

namespace ZanJhat.Core
{
    public class EffectSettings
    {
        public static string PackageName => CoreSettingsManager.PackageName;
        public const string GroupName = "EffectSettings";

        public bool ShowHUD { get; set; } = true;

        public Anchor Anchor { get; set; } = Anchor.TopLeft;

        public float MarginX { get; set; } = 72f;

        public float MarginY { get; set; } = 11f;

        public LayoutDirection LayoutDirection { get; set; } = LayoutDirection.Horizontal;

        public float Scale { get; set; } = 0.9f;

        public void LoadSettings()
        {
            if (ModSettingsManager.TryGet<bool>(out bool hud, PackageName, GroupName, "ShowHUD"))
                ShowHUD = hud;

            if (ModSettingsManager.TryGet<Anchor>(out Anchor anchor, PackageName, GroupName, "Anchor"))
                Anchor = anchor;

            if (ModSettingsManager.TryGet<float>(out float marginX, PackageName, GroupName, "MarginX"))
                MarginX = marginX;

            if (ModSettingsManager.TryGet<float>(out float marginY, PackageName, GroupName, "MarginY"))
                MarginY = marginY;

            if (ModSettingsManager.TryGet<LayoutDirection>(out LayoutDirection direction, PackageName, GroupName, "LayoutDirection"))
                LayoutDirection = direction;

            if (ModSettingsManager.TryGet<float>(out float scale, PackageName, GroupName, "Scale"))
                Scale = scale;
        }

        public void SettingChanged(string path, object newValue)
        {
            if (path == $"{PackageName}/{GroupName}/ShowHUD")
                ShowHUD = (bool)newValue;

            else if (path == $"{PackageName}/{GroupName}/Anchor")
                Anchor = (Anchor)newValue;

            else if (path == $"{PackageName}/{GroupName}/MarginX")
                MarginX = (float)newValue;

            else if (path == $"{PackageName}/{GroupName}/MarginY")
                MarginY = (float)newValue;

            else if (path == $"{PackageName}/{GroupName}/LayoutDirection")
                LayoutDirection = (LayoutDirection)newValue;

            else if (path == $"{PackageName}/{GroupName}/Scale")
                Scale = (float)newValue;
        }
    }
}
