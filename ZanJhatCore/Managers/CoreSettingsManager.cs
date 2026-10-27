using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Engine;
using Engine.Serialization;
using TemplatesDatabase;
using XmlUtilities;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Game;

namespace ZanJhat.Core
{
    public static class CoreSettingsManager
    {
        public const string PackageName = "ZanJhat.Core";

        public static bool DevMode { get; set; } = false;

        public static GraphicsSettings GraphicsSettings = new GraphicsSettings();
        public static ControlsSettings ControlsSettings = new ControlsSettings();
        public static AudioSettings AudioSettings = new AudioSettings();
        public static EffectSettings EffectSettings = new EffectSettings();

        public static void Initialize()
        {
            LoadSettings();
            ModSettingsManager.SettingChanged += OnSettingChanged;
        }

        public static void LoadSettings()
        {
            GraphicsSettings.LoadSettings();
            ControlsSettings.LoadSettings();
            AudioSettings.LoadSettings();
            EffectSettings.LoadSettings();
        }

        public static void OnSettingChanged(string path, object newValue)
        {
            GraphicsSettings.SettingChanged(path, newValue);
            ControlsSettings.SettingChanged(path, newValue);
            AudioSettings.SettingChanged(path, newValue);
            EffectSettings.SettingChanged(path, newValue);
        }
    }
}
