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
    public class OnFireEffect : Effect
    {
        public override string Name => "On Fire";
        public override string IconPath => "Textures/Effects/OnFire";
        public override EffectType EffectType => EffectType.Debuff;
        public override bool NeedSave => false;

        public OnFireEffect(ComponentEffect owmer, double duration)
          : base(owmer, duration)
        {
        }

        public override void Update(float dt)
        {
            if (ComponentOnFire == null || !ComponentOnFire.IsOnFire)
                EndTime = SubsystemGameInfo.TotalElapsedGameTime;
        }

        public override bool OnEffectDispelled(double currentTime)
        {
            if (ComponentOnFire != null && ComponentOnFire.IsOnFire)
            {
                ComponentOnFire.m_fireDuration = 0f;
                return true;
            }
            return false;
        }

        public override void Merge(Effect effect, double currentTime)
        {
            EndTime = effect.EndTime;
        }
    }
}
