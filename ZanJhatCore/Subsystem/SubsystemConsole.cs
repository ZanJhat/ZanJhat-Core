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

    public delegate bool MessageAddingEventHandler(ComponentConsole sender, ref MessageLogEntry entry);

    public class SubsystemConsole : Subsystem
    {
        public SubsystemTime m_subsystemTime;
        public SubsystemGameInfo m_subsystemGameInfo;
        public SubsystemParticles m_subsystemParticles;
        public SubsystemBodies m_subsystemBodies;
        public SubsystemTerrain m_subsystemTerrain;
        public SubsystemMovingBlocks m_subsystemMovingBlocks;
        public SubsystemPlayers m_subsystemPlayers;

        public const int MaxLogs = 512;

        public List<MessageLogEntry> m_logs = new();

        public event MessageAddingEventHandler MessageAdding; // Trước khi tin nhắn được thêm (Có thể hủy/Sửa)
        public event Action<MessageLogEntry> MessageAdded;    // Sau khi tin nhắn đã được thêm

        public event Func<ComponentConsole, string, string[], bool> CommandExecuting; // Trước khi chạy lệnh (Có thể hủy)
        public event Action<ComponentConsole, string, string[], bool> CommandExecuted; // Sau khi lệnh chạy xong (bool: thành công/thất bại)

        public Game.Random m_random = new Game.Random();

        public void ClearLogs()
        {
            m_logs.Clear();
        }

        public virtual void AddMessage(ComponentConsole sender, MessageType type, string senderName, string message)
        {
            string input = message.Trim();

            if (type == MessageType.Chat || type == MessageType.Command)
            {
                type = IsCommand(input) ? MessageType.Command : MessageType.Chat;
            }

            MessageLogEntry entry = new MessageLogEntry
            {
                Type = type,
                Sender = senderName,
                Content = input,
                Time = DateTime.Now
            };

            if (MessageAdding != null)
            {
                // Cho phép các mod khác kiểm duyệt hoặc đổi nội dung (ref entry)
                foreach (MessageAddingEventHandler handler in MessageAdding.GetInvocationList())
                {
                    // Nếu một mod trả về false, tin nhắn bị chặn (không add vào log nữa)
                    if (!handler(sender, ref entry)) return;
                }
            }

            m_logs.Add(entry);
            MessageAdded?.Invoke(entry);

            if (m_logs.Count > MaxLogs)
                m_logs.RemoveAt(0);

            if (entry.Type == MessageType.Command)
                ExecuteCommand(sender, input);
        }

        public bool IsCommand(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && input.StartsWith("/");
        }

        public bool ExecuteCommand(ComponentConsole executor, string commandText)
        {
            if (!IsCommand(commandText))
                return false;

            string[] tokens = commandText.Substring(1).Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length == 0)
                return true;

            string cmdName = tokens[0].ToLowerInvariant();
            string[] rawArgs = tokens.Skip(1).ToArray();

            Func<ComponentConsole, string, string[], bool> handlers = CommandExecuting;

            if (handlers != null)
            {
                foreach (Func<ComponentConsole, string, string[], bool> handler in handlers.GetInvocationList())
                {
                    if (!handler(executor, cmdName, rawArgs))
                        return true;
                }
            }

            if (!CommandManager.TryGet(cmdName, out Command command))
            {
                AddMessage(null, MessageType.Error, "System", $"Unknown command: /{cmdName}");
                return true;
            }

            command.Execute(this, executor, rawArgs);
            CommandExecuted?.Invoke(executor, cmdName, rawArgs, true); // Báo cáo lệnh thành công

            return true;
        }

        public virtual Color GetColor(MessageType type)
        {
            switch (type)
            {
                case MessageType.Chat:
                    return Color.White;
                case MessageType.Command:
                    return ColorPalette.GameAccentColor;
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
