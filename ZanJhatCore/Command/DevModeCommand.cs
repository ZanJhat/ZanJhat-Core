using System;
using System.Linq;
using System.Collections.Generic;
using Engine;
using Game;
using ZanJhat.Core;

namespace ZanJhat.Core
{
    public class DevModeCommand : Command
    {
        public override string Name => "dm";
        public override string Usage => "/dm <bool enable>";

        protected override CmdArgument[] Arguments => new[]
        {
            new CmdArgument("enable", CmdArgType.Bool)
        };

        protected override bool ExecuteCore(SubsystemConsole subsystemConsole, ComponentConsole executor, object[] args)
        {
            bool enable = (bool)args[0];

            CoreSettingsManager.DevMode = enable;
            subsystemConsole.AddMessage(null, MessageType.Info, "System", $"Dev mode: {CoreSettingsManager.DevMode}");
            return true;
        }
    }
}
