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
        Player
    }

    public class CmdArgument
    {
        public string Name;
        public CmdArgType Type;
        public bool Optional;

        public CmdArgument(string name, CmdArgType type, bool optional = false)
        {
            Name = name;
            Type = type;
            Optional = optional;
        }
    }

    public abstract class Command
    {
        public abstract string Name { get; }

        public abstract string Usage { get; }

        public virtual CmdArgument[] Arguments => Array.Empty<CmdArgument>();

        public abstract void Execute(ComponentConsole sender, object[] args);
    }
}