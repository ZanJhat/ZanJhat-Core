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
    public static class GameDataExporter
    {
        public const string TypeName = "GameDataExporter";

        public static Result<bool> ExportBlocks(string filePath, bool skipInvalid, bool includeMods)
        {
            try
            {
                string directory = Path.GetDirectoryName(filePath);

                if (!string.IsNullOrEmpty(directory))
                {
                    if (!Storage.DirectoryExists(directory))
                        Storage.CreateDirectory(directory);
                }

                using (StreamWriter writer = new StreamWriter(Storage.OpenFile(filePath, OpenFileMode.Create), Encoding.UTF8))
                {
                    int max = includeMods ? BlocksManager.Blocks.Length : Math.Min(300, BlocksManager.Blocks.Length);

                    for (int i = 0; i < max; i++)
                    {
                        Block block = BlocksManager.Blocks[i];

                        if (block == null)
                            continue;

                        if (skipInvalid && block is AirBlock && i != 0)
                            continue;

                        string className = block.GetType().Name;

                        writer.WriteLine($"{i}:{className}");
                    }
                }

                Log.Information($"[{TypeName}/ExportBlocks] Export success!");
                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                Log.Warning($"[{TypeName}/ExportBlocks] Export failed: {ex}");
                return Result<bool>.Fail(ex.ToString());
            }
        }
    }
}
