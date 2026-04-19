using System;
using System.Linq;
using System.Collections.Generic;
using Engine;
using Game;

namespace ZanJhat.Core
{
    public class DevModCommand : Command
    {
        public override string Name => "dm";

        public override string Usage => "/dm <bool toggle>";

        public override CmdArgument[] Arguments => new[]
        {
            new CmdArgument("toggle", CmdArgType.Bool)
        };

        public override void Execute(ComponentConsole sender, object[] args)
        {
            bool toggle = (bool)args[0];

            CoreSettings coreSettings = CoreSettingsManager.Get<CoreSettings>();

            if (coreSettings == null)
            {
                sender.AddMessage(MessageType.Error, "System", "ZJCoreSettings is Null");
                return;
            }

            coreSettings.DevMode = toggle;

            sender.AddMessage(MessageType.Info, "System", $"Dev mode: {coreSettings.DevMode}");
        }
    }
}