using System;
using System.Linq;
using System.Collections.Generic;
using Engine;
using Game;

namespace ZanJhat.Core
{
    public static class CommandManager
    {
        private static Dictionary<string, Command> m_commands = new();

        public static void Initialize()
        {
            Register(new PositionCommand());
            Register(new InventorySlotInfoCommand());
            Register(new DamageActiveToolCommand());
            Register(new DevModCommand());
            Register(new TeleportCommand());
        }

        public static void Register(Command command)
        {
            m_commands[command.Name] = command;
        }

        public static bool TryGet(string name, out Command command)
        {
            return m_commands.TryGetValue(name, out command);
        }
    }
}