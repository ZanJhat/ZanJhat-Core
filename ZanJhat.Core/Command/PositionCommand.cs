using System;
using System.Linq;
using System.Collections.Generic;
using Engine;
using Game;

namespace ZanJhat.Core
{
    public class PositionCommand : Command
    {
        public override string Name => "position";
        public override string Usage => "/position";

        protected override void ExecuteCore(SubsystemConsole subsystemConsole, ComponentConsole executor, object[] args)
        {
            ComponentBody componentBody = executor.Entity.FindComponent<ComponentBody>();

            if (componentBody == null)
            {
                subsystemConsole.AddMessage(null, MessageType.Error, "System", "ComponentBody is null");
                return;
            }

            Vector3 position = componentBody.Position;
            string x = position.X.ToString("0.0");
            string y = position.Y.ToString("0.0");
            string z = position.Z.ToString("0.0");

            subsystemConsole.AddMessage(null, MessageType.Info, "System", $"Position: {x}, {y}, {z}");
        }
    }
}