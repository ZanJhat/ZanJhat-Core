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
        public bool Firefly { get; set; } = false;

        public bool ProjectileTrail { get; set; } = false;
    }
}
