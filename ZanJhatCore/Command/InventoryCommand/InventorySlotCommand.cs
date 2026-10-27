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
}
