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
        public override string Usage => "/export <blocks>";

        public ExportCommand()
        {
            Register(new ExportBlocksCommand());
        }
    }

    public class ExportBlocksCommand : Command
    {
        public override string Name => "blocks";
        public override string Usage => "/export blocks <bool skipInvalid> <bool includeMods>";

        protected override CmdArgument[] Arguments => new[]
        {
            new CmdArgument("skipInvalid", CmdArgType.Bool),
            new CmdArgument("includeMods", CmdArgType.Bool)
        };

        protected override void ExecuteCore(SubsystemConsole subsystemConsole, ComponentConsole executor, object[] args)
        {
            string filePath = Path.Combine(PathManager.ExportDirectory, "BlocksList.txt");
            bool skipInvalid = (bool)args[0];
            bool includeMods = (bool)args[1];

            Result<bool> result = GameDataExporter.ExportBlocks(filePath, skipInvalid, includeMods);

            if (result.Success)
                subsystemConsole.AddMessage(null, MessageType.Info, "System", $"Blocks exported: {filePath}");
            else
                subsystemConsole.AddMessage(null, MessageType.Error, "System", $"Blocks export failed: {result.Error}");
        }
    }
}
