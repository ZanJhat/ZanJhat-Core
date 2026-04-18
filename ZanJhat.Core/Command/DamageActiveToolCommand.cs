using System;
using System.Linq;
using System.Collections.Generic;
using Engine;
using Game;

namespace ZanJhat.Core
{
    public class DamageActiveToolCommand : Command
    {
        public override string Name => "dat";

        public override string Usage => "/dat <int count>";

        public override CmdArgument[] Arguments => new[]
        {
            new CmdArgument("count", CmdArgType.Int)
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

            int damageCount = (int)args[0];

            componentMiner.DamageActiveTool(damageCount);

            IInventory inventory = componentMiner.Inventory;

            if (inventory == null)
            {
                sender.AddMessage(MessageType.Error, "System", "IInventory is null");
                return;
            }

            int activeSlotIndex = inventory.ActiveSlotIndex;
            int count = inventory.GetSlotCount(activeSlotIndex);
            int value = inventory.GetSlotValue(activeSlotIndex);
            int contents = Terrain.ExtractContents(value);
            Block block = BlocksManager.Blocks[contents];
            int data = Terrain.ExtractData(value);
            int damage = block.GetDamage(value);
            int durability = block.GetDurability(value);

            sender.AddMessage(MessageType.Info, "System", $"Remaining durability: {durability - damage}/{durability}");
        }
    }
}