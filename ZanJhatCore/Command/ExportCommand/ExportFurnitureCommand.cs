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
            // Lấy Subsystem quản lý nội thất từ Project
            SubsystemFurnitureBlockBehavior subsystemFurniture = executor.Project.FindSubsystem<SubsystemFurnitureBlockBehavior>(true);

            // Thu thập toàn bộ nội thất mồ côi (chưa đăng ký)
            List<FurnitureDesign> uncategorizedDesigns = new List<FurnitureDesign>();
            
            // Duyệt qua mảng m_furnitureDesigns chứa toàn bộ thiết kế hiện có trong thế giới
            foreach (FurnitureDesign design in subsystemFurniture.m_furnitureDesigns)
            {
                // Nếu thiết kế tồn tại và không thuộc bất kỳ gói (FurnitureSet) nào => Nó nằm ở Uncategorized
                if (design != null && design.FurnitureSet == null)
                {
                    uncategorizedDesigns.Add(design);
                }
            }

            if (uncategorizedDesigns.Count == 0)
            {
                subsystemConsole.AddMessage(null, MessageType.Warning, "ZanJhat", "Không tìm thấy thiết kế nào trong mục chưa đăng ký (Uncategorized).");
                return false;
            }

            try
            {
                // Đặt tên gói kèm timestamp để không bị trùng lặp
                string packName = "Uncategorized_Export_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                
                // Gọi hàm gốc của game: Hàm này sẽ tạo XML, nén Zip và tự động lưu vào thư mục FurniturePacks
                string importedName = FurniturePacksManager.CreateFurniturePack(packName, uncategorizedDesigns);
                
                subsystemConsole.AddMessage(null, MessageType.Info, "ZanJhat", $"Thành công! Đã gom {uncategorizedDesigns.Count} khối thành gói: {importedName}.scfpack");
                return true;
            }
            catch (Exception ex)
            {
                subsystemConsole.AddMessage(null, MessageType.Error, "ZanJhat", $"Lỗi khi xuất nội thất: {ex.Message}");
                return false;
            }
        }
    }
}
