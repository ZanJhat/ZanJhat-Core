using Engine;
using Engine.Graphics;
using Engine.Media;
using Engine.Serialization;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using TemplatesDatabase;
using System.IO;
using System.Text;
using XmlUtilities;
using Game;

namespace ZanJhat.Core
{
    public enum MessageType
    {
        Chat = 0,
        Command = 1,
        Info = 2,
        Warning = 3,
        Error = 4
    }

    public struct MessageLogEntry
    {
        public MessageType Type;
        public string Sender;
        public string Content;
        public DateTime Time;
    }

    public class SubsystemConsole : Subsystem, IUpdateable
    {
        public SubsystemTime m_subsystemTime;
        public SubsystemGameInfo m_subsystemGameInfo;
        public SubsystemParticles m_subsystemParticles;
        public SubsystemBodies m_subsystemBodies;
        public SubsystemTerrain m_subsystemTerrain;
        public SubsystemMovingBlocks m_subsystemMovingBlocks;
        public SubsystemPlayers m_subsystemPlayers;

        public List<MessageLogEntry> m_logs = new();
        public Game.Random m_random = new Game.Random();

        public UpdateOrder UpdateOrder => UpdateOrder.Default;

        public virtual void Update(float dt)
        {
        }

        public virtual void AddMessage(ComponentConsole sender, MessageType type, string senderName, string message)
        {
            string input = message.Trim();

            if (type == MessageType.Chat || type == MessageType.Command)
            {
                type = IsCommand(input) ? MessageType.Command : MessageType.Chat;
            }

            m_logs.Add(new MessageLogEntry
            {
                Type = type,
                Sender = senderName,
                Content = input
            });

            if (type == MessageType.Command)
                ExecuteCommand(sender, input);
        }

        public bool IsCommand(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && input.StartsWith("/");
        }

        public bool ExecuteCommand(ComponentConsole sender, string commandText)
        {
            if (!IsCommand(commandText))
                return false;

            string[] tokens = commandText.Substring(1).Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length == 0)
                return true;

            string cmdName = tokens[0].ToLowerInvariant();
            string[] rawArgs = tokens.Skip(1).ToArray();

            if (!CommandManager.TryGet(cmdName, out Command command))
            {
                AddMessage(null, MessageType.Error, "System", $"Unknown command: /{cmdName}");
                return true;
            }

            if (!TryParseArguments(sender, command, rawArgs, out object[] args, out string error))
            {
                AddMessage(null, MessageType.Error, "System", error);
                AddMessage(null, MessageType.Info, "System", $"Usage: {command.Usage}");
                return true;
            }

            command.Execute(sender, args);
            return true;
        }

        public bool TryParseArguments(ComponentConsole executor, Command command, string[] input, out object[] args, out string error)
        {
            args = null;
            error = null;

            CmdArgument[] expected = command.Arguments ?? Array.Empty<CmdArgument>();

            if (expected.Length == 0 && input.Length > 0)
            {
                error = "Too many arguments";
                return false;
            }

            if (input.Length < expected.Count(a => !a.Optional) || input.Length > expected.Length)
            {
                error = "Invalid argument count";
                return false;
            }

            args = new object[expected.Length];

            for (int i = 0; i < expected.Length; i++)
            {
                if (i >= input.Length)
                    break;

                string token = input[i];
                CmdArgument arg = expected[i];

                if (!TryParseArg(executor, token, arg.Type, out object value))
                {
                    error = $"Invalid <{arg.Type}> for {arg.Name}";
                    return false;
                }

                args[i] = value;
            }

            return true;
        }

        public bool TryParseArg(ComponentConsole executor, string token, CmdArgType type, out object value)
        {
            value = null;

            switch (type)
            {
                case CmdArgType.Int:
                    {
                        if (int.TryParse(token, out int i))
                        {
                            value = i;
                            return true;
                        }
                        return false;
                    }
                case CmdArgType.Float:
                    {
                        if (float.TryParse(token, out float f))
                        {
                            value = f;
                            return true;
                        }
                        return false;
                    }
                case CmdArgType.String:
                    value = token;
                    return true;
                case CmdArgType.Bool:
                    {
                        if (bool.TryParse(token, out bool b))
                        {
                            value = b;
                            return true;
                        }
                        return false;
                    }
                case CmdArgType.Player:
                    return TryParsePlayer(executor, token, out value);
            }

            return false;
        }

        public bool TryParsePlayer(ComponentConsole executor, string token, out object value)
        {
            value = null;

            List<ComponentPlayer> players = new();

            if (!token.StartsWith("@"))
            {
                foreach (ComponentPlayer p in m_subsystemPlayers.ComponentPlayers)
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
                    players = m_subsystemPlayers.ComponentPlayers.ToList();
                    break;

                case "@s":
                    if (executor?.m_componentPlayer != null)
                        players.Add(executor.m_componentPlayer);
                    break;

                case "@p":
                    players.Add(FindNearestPlayer(executor));
                    break;

                case "@r":
                    var list = m_subsystemPlayers.ComponentPlayers;
                    if (list.Count > 0)
                        players.Add(list[m_random.Int(list.Count)]);
                    break;

                default:
                    return false;
            }

            value = players;
            return true;
        }

        public ComponentPlayer FindNearestPlayer(ComponentConsole executor)
        {
            Vector3 pos = executor.m_componentBody.Position;

            ComponentPlayer nearest = null;
            float best = float.MaxValue;

            foreach (ComponentPlayer p in m_subsystemPlayers.ComponentPlayers)
            {
                float d = Vector3.DistanceSquared(pos, p.ComponentBody.Position);

                if (d < best)
                {
                    best = d;
                    nearest = p;
                }
            }

            return nearest;
        }

        public virtual Color GetColor(MessageType type)
        {
            switch (type)
            {
                case MessageType.Chat:
                    return Color.White;
                case MessageType.Command:
                    return ColorPresets.GrassGreenColor;
                case MessageType.Info:
                    return Color.LightGray;
                case MessageType.Warning:
                    return Color.Yellow;
                case MessageType.Error:
                    return Color.Red;
                default:
                    return Color.White;
            }
        }

        public override void Load(ValuesDictionary valuesDictionary)
        {
            base.Load(valuesDictionary);
            m_subsystemTime = Project.FindSubsystem<SubsystemTime>(true);
            m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(true);
            m_subsystemParticles = Project.FindSubsystem<SubsystemParticles>(true);
            m_subsystemBodies = Project.FindSubsystem<SubsystemBodies>(true);
            m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(true);
            m_subsystemMovingBlocks = Project.FindSubsystem<SubsystemMovingBlocks>(true);
            m_subsystemPlayers = Project.FindSubsystem<SubsystemPlayers>(true);
        }

        public override void Save(ValuesDictionary valuesDictionary)
        {
            base.Save(valuesDictionary);
        }
    }
}
