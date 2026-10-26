using System;
using System.Collections.Generic;
using Game;

namespace ZanJhat.Core
{
    public class SettingsSection
    {
        public string Name;
        public bool ShowHeader = true;
        public List<Action<SettingsScreenBuilder>> Builders = new();
    }

    public static class SettingsScreenRegistry
    {
        public static readonly Dictionary<string, SettingsSection> Sections = new();

        [Obsolete("Use Register(string section, Action<SettingsScreenBuilder> build, bool showHeader) instead.")]
        public static void Register(Action<SettingsScreenBuilder> build)
        {
            Register("General", build, false);
        }

        public static void Register(string section, Action<SettingsScreenBuilder> build, bool showHeader = true)
        {
            if (!Sections.TryGetValue(section, out SettingsSection settingsSection))
            {
                settingsSection = new SettingsSection
                {
                    Name = section,
                    ShowHeader = showHeader
                };

                Sections[section] = settingsSection;
            }

            settingsSection.Builders.Add(build);
        }
    }
}
