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
    public class ExportCommand : Command
    {
        public override string Name => "export";
        public override string Usage => "/export <blocks/furniture>";

        public ExportCommand()
        {
            Register(new ExportBlocksCommand());
            Register(new ExportFurnitureCommand());
        }
    }

}
