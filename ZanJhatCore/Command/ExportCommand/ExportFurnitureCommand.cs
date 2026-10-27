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
    public class ExportFurnitureCommand : Command
    {
        public override string Name => "furniture";
        public override string Usage => "/export furniture";

        protected override bool ExecuteCore(SubsystemConsole subsystemConsole, ComponentConsole executor, object[] args)
        {
            SubsystemFurnitureBlockBehavior subsystemFurniture = executor.Project.FindSubsystem<SubsystemFurnitureBlockBehavior>(true);

            // Thu thập các thiết kế nội thất chưa thuộc FurnitureSet nào (chưa đăng ký)
            List<FurnitureDesign> uncategorizedDesigns = new List<FurnitureDesign>();
            
            foreach (FurnitureDesign design in subsystemFurniture.m_furnitureDesigns)
            {
                if (design != null && design.FurnitureSet == null)
                {
                    uncategorizedDesigns.Add(design);
                }
            }

            if (uncategorizedDesigns.Count == 0)
            {
                subsystemConsole.AddMessage(null, MessageType.Warning, "System", "No uncategorized furniture designs were found.");
                return false;
            }

            try
            {
                // Đặt tên gói kèm timestamp để không bị trùng lặp
                string packName = "Uncategorized_Export_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                
                // Gọi hàm gốc của game: Hàm này sẽ tạo XML, nén Zip và tự động lưu vào thư mục FurniturePacks
                string importedName = FurniturePacksManager.CreateFurniturePack(packName, uncategorizedDesigns);
                
                subsystemConsole.AddMessage(null, MessageType.Info, "System", $"Successfully exported {uncategorizedDesigns.Count} furniture designs to '{importedName}.scfpack'.");
                return true;
            }
            catch (Exception ex)
            {
                subsystemConsole.AddMessage(null, MessageType.Error, "System", $"Failed to export furniture designs: {ex.Message}");
                return false;
            }
        }
    }
}
