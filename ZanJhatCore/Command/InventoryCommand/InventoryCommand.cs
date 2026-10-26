using System;
using System.Linq;
using System.Collections.Generic;
using Engine;
using Game;

namespace ZanJhat.Core
{
    public class InventoryCommand : Command
    {
        public override string Name => "inv";
        public override string Usage => "/inv <slot>";

        public InventoryCommand()
        {
            Register(new InventorySlotCommand());
        }
    }
}
