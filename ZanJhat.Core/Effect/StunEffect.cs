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
    public class StunEffect : Effect
    {
        public override string Name => "Stun";
        public override string IconPath => "Textures/Effects/Stun";
        public override EffectType EffectType => EffectType.Debuff;
        public override bool NeedSave => false;

        public StunEffect(ComponentEffect owner, double duration)
          : base(owner, duration)
        {
        }

        public override void Update(float dt)
        {
            if (ComponentLocomotion == null || ComponentLocomotion.StunTime <= 0f)
                EndTime = SubsystemGameInfo.TotalElapsedGameTime;
        }

        public override bool OnEffectDispelled(double currentTime)
        {
            if (ComponentLocomotion != null && ComponentLocomotion.StunTime > 0f)
            {
                ComponentLocomotion.StunTime = 0f;
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