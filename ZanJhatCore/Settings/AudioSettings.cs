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
using Game;

namespace ZanJhat.Core
{
    public class AudioSettings
    {
        public static string PackageName => CoreSettingsManager.PackageName;
        public const string GroupName = "GeneralSettings";

        public bool BreakSound { get; set; } = true;

        public void LoadSettings()
        {
            if (ModSettingsManager.TryGet<bool>(out bool breakSound, PackageName, GroupName, "BreakSound"))
                BreakSound = breakSound;
        }

        public void SettingChanged(string path, object newValue)
        {
            if (path == $"{PackageName}/{GroupName}/BreakSound")
                BreakSound = (bool)newValue;
        }
    }
}
