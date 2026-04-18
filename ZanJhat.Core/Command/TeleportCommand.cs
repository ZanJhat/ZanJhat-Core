using System;
using System.Linq;
using System.Collections.Generic;
using Engine;
using Game;

namespace ZanJhat.Core
{
    public class TeleportCommand : Command
    {
        public override string Name => "tp";

        public override string Usage => "/tp <string x> <string y> <string z>";

        public override CmdArgument[] Arguments => new[]
        {
            new CmdArgument("x", CmdArgType.String),
            new CmdArgument("y", CmdArgType.String),
            new CmdArgument("z", CmdArgType.String)
        };

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

            try
            {
                string xStr = args[0].ToString();
                string yStr = args[1].ToString();
                string zStr = args[2].ToString();

                Vector3 currentPos = componentBody.Position;

                float x = ParseCoordinate(xStr, currentPos.X);
                float y = ParseCoordinate(yStr, currentPos.Y);
                float z = ParseCoordinate(zStr, currentPos.Z);

                componentBody.Position = new Vector3(x, y, z);

                sender.AddMessage(MessageType.Info, "System", $"Teleported to: {x:0.##}, {y:0.##}, {z:0.##}");
            }
            catch (Exception ex)
            {
                sender.AddMessage(MessageType.Error, "System", ex.Message);
            }
        }

        private float ParseCoordinate(string input, float current)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new Exception("Invalid coordinate");

            // Trường hợp "~"
            if (input == "~")
                return current;

            // Trường hợp "~something"
            if (input.StartsWith("~"))
            {
                string valuePart = input.Substring(1);

                if (string.IsNullOrEmpty(valuePart))
                    return current;

                if (float.TryParse(valuePart, out float offset))
                    return current + offset;

                throw new Exception($"Invalid relative coordinate: {input}");
            }

            // Trường hợp số thường
            if (float.TryParse(input, out float result))
                return result;

            throw new Exception($"Invalid coordinate: {input}");
        }
    }
}