using System;
using System.Linq;
using System.Collections.Generic;
using Engine;
using Game;

namespace ZanJhat.Core
{
    public class InventorySlotCommand : Command
    {
        public override string Name => "slot";
        public override string Usage => "/inv slot <info>";

        public InventorySlotCommand()
        {
            Register(new InventorySlotInfoCommand());
        }
    }

    public class InventorySlotInfoCommand : Command
    {
        public override string Name => "info";
        public override string Usage => "/inv slot info <int slot | a>";
        protected override CmdArgument[] Arguments => new[]
        {
            new CmdArgument("slot", CmdArgType.String)
        };

        protected override void ExecuteCore(SubsystemConsole subsystemConsole, ComponentConsole executor, object[] args)
        {
            ComponentMiner componentMiner = executor.Entity.FindComponent<ComponentMiner>();

            if (componentMiner == null)
            {
                subsystemConsole.AddMessage(null, MessageType.Error, "System", "ComponentMiner is null");
                return;
            }

            IInventory inventory = componentMiner.Inventory;

            if (inventory == null)
            {
                subsystemConsole.AddMessage(null, MessageType.Error, "System", "IInventory is null");
                return;
            }

            string slotArg = (string)args[0];
            int slotIndex;

            if (slotArg == "a")
            {
                slotIndex = inventory.ActiveSlotIndex;
            }
            else if (!int.TryParse(slotArg, out slotIndex))
            {
                subsystemConsole.AddMessage(null, MessageType.Error, "System", $"Invalid slot. Use a or 0-{inventory.SlotsCount - 1}");
                return;
            }

            if (slotIndex < 0 || slotIndex >= inventory.SlotsCount)
            {
                subsystemConsole.AddMessage(null, MessageType.Error, "System", $"Invalid slot. Use a or slot number (0 - {inventory.SlotsCount - 1})");
                return;
            }

            int count = inventory.GetSlotCount(slotIndex);
            int value = inventory.GetSlotValue(slotIndex);
            int contents = Terrain.ExtractContents(value);
            Block block = BlocksManager.Blocks[contents];
            int data = Terrain.ExtractData(value);

            subsystemConsole.AddMessage(null, MessageType.Info, "System", $"Slot {slotIndex}: value={value}, contents={contents}, data={data}, count={count}");
        }
    }
}
