using System;
using System.Linq;
using System.Collections.Generic;
using Engine;
using Game;

namespace ZanJhat.Core
{
    public enum CmdArgType
    {
        Int,
        Float,
        String,
        Bool,
        Player,
        Custom
    }

    public class CmdArgument
    {
        public string Name;
        public CmdArgType Type;
        public bool Optional;

        public Func<SubsystemConsole, ComponentConsole, string, (bool success, object value, string error)> CustomParser;

        public CmdArgument(string name, CmdArgType type, bool optional = false, Func<SubsystemConsole, ComponentConsole, string, (bool success, object value, string error)> customParser = null)
        {
            Name = name;
            Type = type;
            Optional = optional;
            CustomParser = customParser;
        }
    }

    public abstract class Command
    {
        private Dictionary<string, Command> m_children = new();

        public abstract string Name { get; }
        public abstract string Usage { get; }

        protected virtual CmdArgument[] Arguments => Array.Empty<CmdArgument>();

        protected Game.Random m_random = new();

        protected virtual void Register(Command cmd)
        {
            m_children[cmd.Name] = cmd;
        }

        protected bool TryGetChild(string name, out Command cmd)
        {
            return m_children.TryGetValue(name, out cmd);
        }

        public virtual void Execute(SubsystemConsole subsystemConsole, ComponentConsole executor, string[] rawArgs)
        {
            // 1. CHILD ROUTING
            if (m_children.Count > 0)
            {
                if (rawArgs.Length > 0 && TryGetChild(rawArgs[0], out Command child))
                {
                    child.Execute(subsystemConsole, executor, rawArgs.Skip(1).ToArray());
                    return;
                }

                subsystemConsole.AddMessage(null, MessageType.Error, "System", $"Unknown subcommand");
                subsystemConsole.AddMessage(null, MessageType.Info, "System", $"Usage: {Usage}");

                return;
            }

            // 2. NORMAL COMMAND (no child)
            if (!TryParseArguments(subsystemConsole, executor, Arguments, rawArgs, out object[] args, out string error))
            {
                subsystemConsole.AddMessage(null, MessageType.Error, "System", error);
                subsystemConsole.AddMessage(null, MessageType.Info, "System", $"Usage: {Usage}");
                return;
            }

            // 3. EXECUTE CORE
            ExecuteCore(subsystemConsole, executor, args);
        }

        protected virtual void ExecuteCore(SubsystemConsole subsystemConsole, ComponentConsole executor, object[] args)
        {
        }

        protected bool TryParseArguments(SubsystemConsole subsystemConsole, ComponentConsole executor, CmdArgument[] expected, string[] rawArgs, out object[] args, out string error)
        {
            args = null;
            error = null;

            if (expected.Length == 0 && rawArgs.Length > 0)
            {
                error = "Too many arguments";
                return false;
            }

            if (rawArgs.Length < expected.Count(a => !a.Optional) || rawArgs.Length > expected.Length)
            {
                error = "Invalid argument count";
                return false;
            }

            args = new object[expected.Length];

            for (int i = 0; i < expected.Length; i++)
            {
                if (i >= rawArgs.Length)
                    break;

                string token = rawArgs[i];
                CmdArgument arg = expected[i];

                if (!TryParseArg(subsystemConsole, executor, token, arg, out object value, out string error2))
                {
                    error = string.IsNullOrEmpty(error2) ? $"Invalid <{arg.Type}> for {arg.Name}" : error2;
                    return false;
                }

                args[i] = value;
            }

            return true;
        }

        protected bool TryParseArg(SubsystemConsole subsystemConsole, ComponentConsole executor, string token, CmdArgument arg, out object value, out string error)
        {
            value = null;
            error = null;

            switch (arg.Type)
            {
                case CmdArgType.Int:

                    if (int.TryParse(token, out int i))
                    {
                        value = i;
                        return true;
                    }
                    return false;

                case CmdArgType.Float:

                    if (float.TryParse(token, out float f))
                    {
                        value = f;
                        return true;
                    }
                    return false;

                case CmdArgType.String:
                    value = token;
                    return true;

                case CmdArgType.Bool:

                    if (bool.TryParse(token, out bool b))
                    {
                        value = b;
                        return true;
                    }
                    return false;

                case CmdArgType.Player:
                    if (TryParsePlayer(subsystemConsole, executor, token, out List<ComponentPlayer> players))
                    {
                        value = players;
                        return true;
                    }
                    return false;

                case CmdArgType.Custom:
                    if (arg.CustomParser != null)
                    {
                        var result = arg.CustomParser(subsystemConsole, executor, token);

                        if (result.success)
                        {
                            value = result.value;
                            return true;
                        }

                        error = result.error;
                        return false;
                    }
                    return false;

            }

            return false;
        }

        protected bool TryParsePlayer(SubsystemConsole subsystemConsole, ComponentConsole executor, string token, out List<ComponentPlayer> value)
        {
            SubsystemPlayers subsystemPlayers = subsystemConsole.m_subsystemPlayers;

            value = null;
            List<ComponentPlayer> players = new();

            if (!token.StartsWith("@"))
            {
                foreach (ComponentPlayer p in subsystemPlayers.ComponentPlayers)
                {
                    if (p.PlayerData.Name == token)
                    {
                        players.Add(p);
                        break;
                    }
                }

                if (players.Count > 0)
                {
                    value = players;
                    return true;
                }

                return false;
            }

            switch (token)
            {
                case "@a":
                    players = subsystemPlayers.ComponentPlayers.ToList();
                    break;

                case "@s":
                    if (executor?.m_componentPlayer != null)
                        players.Add(executor.m_componentPlayer);
                    break;

                case "@r":
                    var list = subsystemPlayers.ComponentPlayers;
                    if (list.Count > 0)
                        players.Add(list[m_random.Int(list.Count)]);
                    break;

                default:
                    return false;
            }

            value = players;
            return true;
        }
    }

    public static class CommandParsers
    {
        public static (bool success, object value, string error) ParseCoordinate(SubsystemConsole console, ComponentConsole executor, string input, float current)
        {
            if (string.IsNullOrWhiteSpace(input))
                return (false, null, "Empty coordinate");

            if (input == "~")
                return (true, current, null);

            if (input.StartsWith("~"))
            {
                string part = input.Substring(1);

                if (string.IsNullOrEmpty(part))
                    return (true, current, null);

                if (float.TryParse(part, out float offset))
                    return (true, current + offset, null);

                return (false, null, $"Invalid relative coordinate: {input}");
            }

            if (float.TryParse(input, out float value))
                return (true, value, null);

            return (false, null, $"Invalid coordinate: {input}");
        }
    }
}
