using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using GameEntitySystem;
using XmlUtilities;
using Engine;
using Game;

namespace ZanJhat.Core
{
    public static class ModEntityHelper
    {
        public const string DefaultPackageName = "survivalcraft";

        // Tên EntityTemplate -> Tên Package của Mod cuối cùng sửa đổi/thêm nó
        private static readonly Dictionary<string, string> m_entityToModMap = new Dictionary<string, string>();

        // Guid -> Tên EntityTemplate
        private static readonly Dictionary<string, string> m_guidToNameMap = new Dictionary<string, string>();

        private static bool m_isInitialized = false;

        // Khởi tạo và quét toàn bộ tệp .xdb từ các mod đang hoạt động
        public static void InitializeEntitySources()
        {
            if (m_isInitialized) return;

            // ModsManager.ModList chứa các mod theo thứ tự load
            foreach (ModEntity mod in ModsManager.ModList)
            {
                string packageName = mod.modInfo?.PackageName ?? DefaultPackageName;

                // Lấy trực tiếp dữ liệu file .xdb từ bộ nhớ của ModArchive
                mod.GetFiles(".xdb", (filename, stream) =>
                {
                    try
                    {
                        XElement root = XmlUtils.LoadXmlFromStream(stream, Encoding.UTF8, true);

                        // Sử dụng Descendants để quét mọi thẻ EntityTemplate (bất kể nó nằm sâu ở đâu)
                        foreach (XElement element in root.Descendants("EntityTemplate"))
                        {
                            string name = element.Attribute("Name")?.Value;
                            string guid = element.Attribute("Guid")?.Value;

                            // Nếu thẻ có cả Name và Guid (thường là Vanilla hoặc entity tạo mới)
                            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(guid))
                            {
                                m_guidToNameMap[guid] = name;
                            }
                            // Nếu thẻ chỉ có Guid (trường hợp mod ghi đè vanilla)
                            else if (string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(guid))
                            {
                                if (m_guidToNameMap.TryGetValue(guid, out string mappedName))
                                {
                                    name = mappedName;
                                }
                            }

                            // Cập nhật tên Package sở hữu/sửa đổi entity này
                            if (!string.IsNullOrEmpty(name))
                            {
                                // Mod load sau sẽ ghi đè lên mod load trước, đảm bảo tính cập nhật
                                m_entityToModMap[name] = packageName;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Failed to parse .xdb in {packageName}: {e.Message}");
                    }
                });
            }

            m_isInitialized = true;
        }

        public static ModEntity GetModEntity(Entity entity)
        {
            if (entity == null)
                return null;

            string templateName = entity.ValuesDictionary.DatabaseObject.Name;
            return GetModEntity(templateName);
        }

        public static ModEntity GetModEntity(string templateName)
        {
            if (!m_isInitialized)
            {
                InitializeEntitySources();
            }

            if (m_entityToModMap.TryGetValue(templateName, out string modPackageName))
            {
                return ModsManager.ModList.FirstOrDefault(m => m.modInfo?.PackageName == modPackageName);
            }

            return null;
        }

        public static string GetPackageName(Entity entity)
        {
            if (entity == null)
                return null;

            string templateName = entity.ValuesDictionary.DatabaseObject.Name;

            return GetPackageName(templateName);
        }

        public static string GetPackageName(string templateName)
        {
            ModEntity modEntity = GetModEntity(templateName);

            string packageName = modEntity?.modInfo?.PackageName;

            return string.IsNullOrEmpty(packageName) ? DefaultPackageName : packageName;
        }

        public static ModEntity GetModEntity(Block block) => GetModEntity(block.GetType());

        public static ModEntity GetModEntity(Type blockType) => ModsManager.ModList.FirstOrDefault(m => m.BlockTypes.Contains(blockType));

        public static string GetPackageName(Block block)
        {
            if (block == null)
                return DefaultPackageName;

            return GetPackageName(block.GetType());
        }

        public static string GetPackageName(Type blockType)
        {
            ModEntity modEntity = GetModEntity(blockType);

            string packageName = modEntity?.modInfo?.PackageName;

            return string.IsNullOrEmpty(packageName) ? DefaultPackageName : packageName;
        }
    }
}
