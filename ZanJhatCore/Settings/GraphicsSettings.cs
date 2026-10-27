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
    public class GraphicsSettings
    {
        public static string PackageName => CoreSettingsManager.PackageName;
        public const string GroupName = "GeneralSettings";

        public bool Firefly { get; set; } = false;

        public bool ProjectileTrail { get; set; } = false;

        public void LoadSettings()
        {
            if (ModSettingsManager.TryGet<bool>(out bool firefly, PackageName, GroupName, "Firefly"))
                Firefly = firefly;

            if (ModSettingsManager.TryGet<bool>(out bool trail, PackageName, GroupName, "ProjectileTrail"))
                ProjectileTrail = trail;
        }

        public void SettingChanged(string path, object newValue)
        {
            if (path == $"{PackageName}/{GroupName}/Firefly")
                Firefly = (bool)newValue;

            else if (path == $"{PackageName}/{GroupName}/ProjectileTrail")
                ProjectileTrail = (bool)newValue;
        }
    }
}
