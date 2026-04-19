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

        public override void Execute(ComponentConsole sender, object[] args)
        {
            if (sender == null)
                return;

            ComponentBody componentBody = sender.Entity.FindComponent<ComponentBody>();

            if (componentBody == null)
            {
                sender.AddMessage(MessageType.Error, "System", "ComponentBody is null");
                return;
            }

            Vector3 position = componentBody.Position;
            string x = position.X.ToString("0.0");
            string y = position.Y.ToString("0.0");
            string z = position.Z.ToString("0.0");

            sender.AddMessage(MessageType.Info, "System", $"Position: {x}, {y}, {z}");
        }
    }
}