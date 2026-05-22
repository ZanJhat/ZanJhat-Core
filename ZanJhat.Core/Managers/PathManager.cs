using Engine;
using Engine.Graphics;
using Engine.Media;
using Engine.Serialization;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using TemplatesDatabase;
using System.IO;
using System.Text;
using XmlUtilities;
using Game;

namespace ZanJhat.Core
{
    public static class PathManager
    {
        public static readonly string RootDirectory = Path.Combine(ModsManager.DocPath, "ZanJhat");

        // Directory
        public static readonly string SettingsDirectory = Path.Combine(RootDirectory, "Settings");
        public static readonly string ExportDirectory = Path.Combine(RootDirectory, "Export");
        public static readonly string LogsDirectory = Path.Combine(RootDirectory, "Logs");
        public static readonly string CinematicDirectory = Path.Combine(RootDirectory, "CinematicRecords");

        // File
        public static readonly string SettingsFile = Path.Combine(SettingsDirectory, "Settings.xml");
    }
}
