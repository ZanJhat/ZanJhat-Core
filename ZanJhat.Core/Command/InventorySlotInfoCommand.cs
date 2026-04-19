using System;
using System.Linq;
using System.Collections.Generic;
using Engine;
using Game;

namespace ZanJhat.Core
{
    public class InventorySlotInfoCommand : Command
    {
        public override string Name => "inv-slot-info";

        public override string Usage => "/inv-slot-info <string slot>";

        public override CmdArgument[] Arguments => new[]
        {
            new CmdArgument("slot", CmdArgType.String)
        };

        public override void Execute(ComponentConsole sender, object[] args)
        {
            if (sender == null)
                return;

            ComponentMiner componentMiner = sender.Entity.FindComponent<ComponentMiner>();

            if (componentMiner == null)
            {
                sender.AddMessage(MessageType.Error, "System", "ComponentMiner is null");
                return;
            }

            IInventory inventory = componentMiner.Inventory;

            if (inventory == null)
            {
                sender.AddMessage(MessageType.Error, "System", "IInventory is null");
                return;
            }

            string slotArg = (string)args[0];

            int slotIndex;

            if (slotArg == "a")
            {
                // Slot đang hoạt động
                slotIndex = inventory.ActiveSlotIndex;
            }
            else if (!int.TryParse(slotArg, out slotIndex))
            {
                sender.AddMessage(MessageType.Error, "System", $"Invalid slot. Use a or slot number (0 - {inventory.SlotsCount})");
                return;
            }

            if (slotIndex < 0 || slotIndex >= inventory.SlotsCount)
            {
                sender.AddMessage(MessageType.Error, "System", $"Invalid slot. Use a or slot number (0 - {inventory.SlotsCount - 1})");
                return;
            }

            int count = inventory.GetSlotCount(slotIndex);
            int value = inventory.GetSlotValue(slotIndex);
            int contents = Terrain.ExtractContents(value);
            Block block = BlocksManager.Blocks[contents];
            int data = Terrain.ExtractData(value);

            sender.AddMessage(MessageType.Info, "System", $"Slot {slotIndex}: value={value}, contents={contents}, data={data}, count={count}");
        }
    }
}