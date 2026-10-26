using System;
using System.Linq;
using System.Collections.Generic;
using Engine;
using GameEntitySystem;
using Game;

namespace ZanJhat.Core
{
    public class DamageItemContext
    {
        public Block Block;
        public int Value;
        public int DamageCount;
        public Entity Owner;
        public Vector3? Position;
        public bool PlaySound;
        public string SoundPath;
    }
}
