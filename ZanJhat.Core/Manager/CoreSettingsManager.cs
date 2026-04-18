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
using Game;

namespace ZanJhat.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EncodeAttribute : Attribute
    {
    }

    public static class CoreSettingsManager
    {
        private static readonly string SettingsName = "ZanJhatSettings";
        private static readonly string EncodeKey = "ZJ@SecretKey123";
        private static readonly string SettingsPath = ModsManager.DocPath + "/ZJSettings.xml";

        private static readonly Dictionary<string, XElement> m_pendingSections = new();
        private static readonly Dictionary<Type, object> m_registeredSettings = new();

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
        }

        public static void RegisterDefaultSettings()
        {
            Register(new CoreSettings());
        }

        public static void SaveSettings()
        {
            try
            {
                XElement root = new XElement(SettingsName);

                foreach (var kv in m_registeredSettings)
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
                            string raw = $"{prop.Name}:{valueString}";
                            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw + EncodeKey));
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

                if (!Storage.DirectoryExists(ModsManager.DocPath))
                    Storage.CreateDirectory(ModsManager.DocPath);

                using (StreamWriter writer = new StreamWriter(Storage.OpenFile(SettingsPath, OpenFileMode.Create), Encoding.UTF8))
                {
                    writer.Write(root.ToString());
                }
                Log.Information($"[{SettingsName}] Saved all mod settings.");
            }
            catch (Exception ex)
            {
                Log.Warning($"[{SettingsName}] Save failed: {ex}");
            }
        }

        public static void LoadSettings()
        {
            try
            {
                if (!Storage.FileExists(SettingsPath))
                    return;

                XElement root;
                using (StreamReader reader = new StreamReader(Storage.OpenFile(SettingsPath, OpenFileMode.Read), Encoding.UTF8))
                {
                    root = XElement.Parse(reader.ReadToEnd());
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
                    string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(valueAttr));
                    decoded = decoded.Replace(EncodeKey, "");
                    int index = decoded.IndexOf(':');
                    finalValueStr = index >= 0 ? decoded[(index + 1)..] : decoded;
                }

                object convertedValue;

                if (prop.PropertyType.IsEnum)
                    convertedValue = Enum.Parse(prop.PropertyType, finalValueStr);
                else
                    convertedValue = Convert.ChangeType(finalValueStr, prop.PropertyType, CultureInfo.InvariantCulture);

                prop.SetValue(settingsObj, convertedValue);
            }
        }
    }

    public class CoreSettings
    {
        [Encode]
        public bool DevMode { get; set; } = false;
    }
}
