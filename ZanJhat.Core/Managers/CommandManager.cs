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
            Register(new InventoryCommand());
            Register(new DamageActiveToolCommand());
            Register(new DevModeCommand());
            Register(new TeleportCommand());
            Register(new ExportCommand());
            Register(new CinematicRecordCommand());
        }

        public static IEnumerable<Command> GetAll()
        {
            return m_commands.Values;
        }

        public static void Register(Command command)
        {
            string name = command.Name.ToLowerInvariant();

            if (m_commands.ContainsKey(name))
            {
                Log.Warning($"Command '{name}' already registered.");
                return;
            }

            m_commands[name] = command;
        }

        public static bool Unregister(string name)
        {
            return m_commands.Remove(name.ToLowerInvariant());
        }

        public static bool TryGet(string name, out Command command)
        {
            return m_commands.TryGetValue(name.ToLowerInvariant(), out command);
        }
    }
}
