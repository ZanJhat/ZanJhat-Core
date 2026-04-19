using System;
using System.Collections.Generic;
using Game;

namespace ZanJhat.Core
{
    public static class SettingsScreenRegistry
    {
        public static List<Action<SettingsScreenBuilder>> Builders = new();

        public static void Register(Action<SettingsScreenBuilder> build)
        {
            Builders.Add(build);
        }
    }
}