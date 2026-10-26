using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Engine;
using Engine.Serialization;
using TemplatesDatabase;
using XmlUtilities;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Game;

namespace ZanJhat.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EncodeAttribute : Attribute
    {
    }

    public static class CoreSettingsManager
    {
        private static readonly string SettingsName = "CoreSettings";
        private static readonly string EncodeKey = "ZC@SecretKey123";

        public static string SettingsDirectory => PathManager.SettingsDirectory;
        public static string SettingsFile => PathManager.SettingsFile;

        private static readonly Dictionary<string, XElement> m_pendingSections = new();
        private static readonly Dictionary<Type, object> m_registeredSettings = new();

        public static CoreSettings CoreSettings;
        public static GraphicsSettings GraphicsSettings;
        public static ControlsSettings ControlsSettings;
        public static AudioSettings AudioSettings;
        public static EffectSettings EffectSettings;

        public static void Register(object settingsInstance)
        {
            Type type = settingsInstance.GetType();

            if (m_registeredSettings.ContainsKey(type))
                return;

            m_registeredSettings[type] = settingsInstance;

            string className = type.FullName;

            if (m_pendingSections.TryGetValue(className, out XElement section))
            {
                ApplySection(settingsInstance, section);
                m_pendingSections.Remove(className);
            }
        }

        public static T Get<T>() where T : class
        {
            m_registeredSettings.TryGetValue(typeof(T), out object value);
            return value as T;
        }

        public static void Initialize()
        {
            RegisterDefaultSettings();
            LoadSettings();

            Window.Deactivated += delegate
            {
                SaveSettings();
            };

            ResolveSettings();
            RegisterSettingsScreen();
        }

        public static void RegisterDefaultSettings()
        {
            Register(new CoreSettings());
            Register(new GraphicsSettings());
            Register(new ControlsSettings());
            Register(new AudioSettings());
            Register(new EffectSettings());
        }

        public static void ResolveSettings()
        {
            CoreSettings = Get<CoreSettings>();
            GraphicsSettings = Get<GraphicsSettings>();
            ControlsSettings = Get<ControlsSettings>();
            AudioSettings = Get<AudioSettings>();
            EffectSettings = Get<EffectSettings>();
        }

        public static void RegisterSettingsScreen()
        {
            // Graphics
            SettingsScreenRegistry.Register(SettingsSections.Graphics, builder =>
            {
                builder.AddToggle("Firefly",
                    () => GraphicsSettings.Firefly,
                    v => GraphicsSettings.Firefly = v);

                builder.AddToggle("Projectile Trail",
                    () => GraphicsSettings.ProjectileTrail,
                    v => GraphicsSettings.ProjectileTrail = v);
            });

            // Controls
            SettingsScreenRegistry.Register(SettingsSections.Controls, builder =>
            {
                builder.AddToggle("Double Tap to Dash",
                    () => ControlsSettings.DoubleTapDash,
                    v => ControlsSettings.DoubleTapDash = v);
            });

            // Audio
            SettingsScreenRegistry.Register(SettingsSections.Audio, builder =>
            {
                builder.AddToggle("Damage Item",
                    () => AudioSettings.DamageItem,
                    v => AudioSettings.DamageItem = v);
            });

            // Effect
            SettingsScreenRegistry.Register("Effect", builder =>
            {
                builder.AddToggle("Enable",
                    () => EffectSettings.Enable,
                    v => EffectSettings.Enable = v);

                builder.AddEnum("Anchor",
                    () => EffectSettings.Anchor,
                    v => EffectSettings.Anchor = v,
                    v => Regex.Replace(v.ToString(), "([a-z])([A-Z])", "$1 $2"));

                builder.AddSlider("Margin X",
                   () => EffectSettings.MarginX,
                   v => EffectSettings.MarginX = v,
                   -256f, 256f, 1f);

                builder.AddSlider("Margin Y",
                   () => EffectSettings.MarginY,
                   v => EffectSettings.MarginY = v,
                   -128f, 128f, 1f);

                builder.AddEnum("Layout Direction",
                    () => EffectSettings.LayoutDirection,
                    v => EffectSettings.LayoutDirection = v,
                    v => Regex.Replace(v.ToString(), "([a-z])([A-Z])", "$1 $2"));

                builder.AddSlider("Scale",
                   () => EffectSettings.Scale,
                   v => EffectSettings.Scale = v,
                   0.5f, 1.5f, 0.1f);
            });
        }

        public static void LoadSettings()
        {
            try
            {
                if (!Storage.FileExists(SettingsFile))
                    return;

                XElement root;

                using (Stream stream = Storage.OpenFile(SettingsFile, OpenFileMode.Read))
                {
                    root = XElement.Load(stream);
                }

                foreach (XElement section in root.Elements("Section"))
                {
                    string className = section.Attribute("Class")?.Value;
                    if (string.IsNullOrEmpty(className))
                        continue;

                    Type type = m_registeredSettings.Keys.FirstOrDefault(t => t.FullName == className);

                    if (type != null)
                    {
                        object settingsObj = m_registeredSettings[type];
                        ApplySection(settingsObj, section);
                    }
                    else
                    {
                        m_pendingSections[className] = section;
                    }
                }
                Log.Information($"[{SettingsName}] Loaded all mod settings.");
            }
            catch (Exception ex)
            {
                Log.Warning($"[{SettingsName}] Load failed: {ex}");
            }
        }

        private static void ApplySection(object settingsObj, XElement section)
        {
            Type type = settingsObj.GetType();

            foreach (XElement entry in section.Elements("Setting"))
            {
                string name = entry.Attribute("Name")?.Value;
                string valueAttr = entry.Attribute("Value")?.Value;
                bool isEncoded = entry.Attribute("Encoded")?.Value == "True";

                PropertyInfo prop = type.GetProperty(name);
                if (prop == null || !prop.CanWrite || string.IsNullOrEmpty(valueAttr))
                    continue;

                string finalValueStr = valueAttr;

                if (isEncoded)
                {
                    try
                    {
                        string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(valueAttr));

                        if (decoded.StartsWith(EncodeKey + ":"))
                            decoded = decoded.Substring((EncodeKey + ":").Length);

                        int index = decoded.IndexOf(':');
                        finalValueStr = index >= 0 ? decoded[(index + 1)..] : decoded;
                    }
                    catch
                    {
                        Log.Warning($"[{SettingsName}] Failed to decode setting: {name}. Skipping.");
                        continue;
                    }
                }

                try
                {
                    object convertedValue;

                    if (prop.PropertyType.IsEnum)
                        convertedValue = Enum.Parse(prop.PropertyType, finalValueStr, true);
                    else
                        convertedValue = Convert.ChangeType(finalValueStr, prop.PropertyType, CultureInfo.InvariantCulture);

                    prop.SetValue(settingsObj, convertedValue);
                }
                catch (Exception ex)
                {
                    Log.Warning($"[{SettingsName}] Invalid value for {name}: {finalValueStr}. Using default. Error: {ex.Message}");
                }
            }
        }

        public static void SaveSettings()
        {
            try
            {
                XElement root = new XElement(SettingsName);

                // Save registered settings
                foreach (KeyValuePair<Type, object> kv in m_registeredSettings)
                {
                    Type type = kv.Key;
                    object settingsObj = kv.Value;

                    XElement modSection = new XElement("Section", new XAttribute("Class", type.FullName));

                    foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (!prop.CanRead || !prop.CanWrite)
                            continue;

                        object value = prop.GetValue(settingsObj);
                        string valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
                        bool isEncoded = prop.GetCustomAttribute<EncodeAttribute>() != null;

                        XElement entry = new XElement("Setting", new XAttribute("Name", prop.Name));

                        if (isEncoded)
                        {
                            string raw = $"{EncodeKey}:{prop.Name}:{valueString}";
                            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
                            entry.Add(new XAttribute("Value", encoded), new XAttribute("Encoded", "True"));
                        }
                        else
                        {
                            entry.Add(new XAttribute("Value", valueString ?? ""));
                        }

                        modSection.Add(entry);
                    }
                    root.Add(modSection);
                }

                // SAVE pending sections
                foreach (XElement pending in m_pendingSections.Values)
                    root.Add(pending);

                if (!Storage.DirectoryExists(SettingsDirectory))
                    Storage.CreateDirectory(SettingsDirectory);

                using (StreamWriter writer = new StreamWriter(Storage.OpenFile(SettingsFile, OpenFileMode.Create), Encoding.UTF8))
                {
                    root.Save(writer);
                }

                Log.Information($"[{SettingsName}] Saved all mod settings.");
            }
            catch (Exception ex)
            {
                Log.Warning($"[{SettingsName}] Save failed: {ex}");
            }
        }
    }
}
