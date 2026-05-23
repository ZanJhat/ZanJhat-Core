using System;
using System.Linq;
using System.Collections.Generic;
using Engine;
using Game;

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

        protected override void ExecuteCore(SubsystemConsole subsystemConsole, ComponentConsole executor, object[] args)
        {
            bool enable = (bool)args[0];

            CoreSettings coreSettings = CoreSettingsManager.Get<CoreSettings>();

            if (coreSettings == null)
            {
                subsystemConsole.AddMessage(null, MessageType.Error, "System", "ZJCoreSettings is Null");
                return;
            }

            coreSettings.DevMode = enable;

            subsystemConsole.AddMessage(null, MessageType.Info, "System", $"Dev mode: {coreSettings.DevMode}");
        }
    }
}
