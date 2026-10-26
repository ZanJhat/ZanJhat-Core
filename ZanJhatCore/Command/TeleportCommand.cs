using System;
using Engine;
using Game;

namespace ZanJhat.Core
{
    public class TeleportCommand : Command
    {
        public override string Name => "tp";
        public override string Usage => "/tp <x> <y> <z> [yaw] [pitch]";

        protected override CmdArgument[] Arguments => new[]
        {
            new CmdArgument("x", CmdArgType.Custom, customParser: (c, e, t) =>
            {
                ComponentBody componentBody = e.Entity.FindComponent<ComponentBody>();
                if (componentBody == null)
                    return (false, null, "ComponentBody is null");

                return CommandParsers.ParseCoordinate(c, e, t, componentBody.Position.X);
            }),

            new CmdArgument("y", CmdArgType.Custom, customParser: (c, e, t) =>
            {
                ComponentBody componentBody = e.Entity.FindComponent<ComponentBody>();
                if (componentBody == null)
                    return (false, null, "ComponentBody is null");

                return CommandParsers.ParseCoordinate(c, e, t, componentBody.Position.Y);
            }),

            new CmdArgument("z", CmdArgType.Custom, customParser: (c, e, t) =>
            {
                ComponentBody componentBody = e.Entity.FindComponent<ComponentBody>();
                if (componentBody == null)
                    return (false, null, "ComponentBody is null");

                return CommandParsers.ParseCoordinate(c, e, t, componentBody.Position.Z);
            }),

            new CmdArgument("yaw", CmdArgType.Float, true),
            new CmdArgument("pitch", CmdArgType.Float, true)
        };

        protected override void ExecuteCore(SubsystemConsole subsystemConsole, ComponentConsole executor, object[] args)
        {
            ComponentBody componentBody = executor.Entity.FindComponent<ComponentBody>();
            ComponentLocomotion componentLocomotion = executor.Entity.FindComponent<ComponentLocomotion>();

            if (componentBody == null)
            {
                subsystemConsole.AddMessage(null, MessageType.Error, "System", "ComponentBody is null");
                return;
            }

            float x = (float)args[0];
            float y = (float)args[1];
            float z = (float)args[2];

            componentBody.Position = new Vector3(x, y, z);

            // Nếu có tham số yaw + pitch
            if (componentLocomotion != null && args.Length > 3 && args[3] != null)
            {
                float yawRad = MathUtils.DegToRad((float)args[3]);
                float pitchRad = componentLocomotion.LookAngles.Y;

                if (args.Length > 4 && args[4] != null)
                    pitchRad = MathUtils.DegToRad((float)args[4]);

                componentLocomotion.LookAngles = new Vector2(yawRad, pitchRad);

                // đồng bộ body rotation
                componentBody.Rotation = Quaternion.CreateFromYawPitchRoll(yawRad, pitchRad, 0f);
            }

            subsystemConsole.AddMessage(null, MessageType.Info, "System", $"Teleported to: {x:0.##}, {y:0.##}, {z:0.##}");
        }
    }
}
