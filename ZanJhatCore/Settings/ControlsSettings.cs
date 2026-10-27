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
    public class ControlsSettings
    {
        public static string PackageName => CoreSettingsManager.PackageName;
        public const string GroupName = "GeneralSettings";

        public bool DoubleTapDash { get; set; } = true;

        public void LoadSettings()
        {
            if (ModSettingsManager.TryGet<bool>(out bool dash, PackageName, GroupName, "DoubleTapDash"))
                DoubleTapDash = dash;
        }

        public void SettingChanged(string path, object newValue)
        {
            if (path == $"{PackageName}/{GroupName}/DoubleTapDash")
                DoubleTapDash = (bool)newValue;
        }
    }
}
