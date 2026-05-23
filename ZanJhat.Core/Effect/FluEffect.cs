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
    public class FluEffect : Effect
    {
        public override string Name => "Flu";
        public override string IconPath => "Textures/Effects/Flu";
        public override EffectType EffectType => EffectType.Debuff;
        public override bool NeedSave => false;

        public FluEffect(ComponentEffect owner, double duration)
          : base(owner, duration)
        {
        }

        public override void Update(float dt)
        {
            if (ComponentFlu == null || !ComponentFlu.HasFlu)
                EndTime = SubsystemGameInfo.TotalElapsedGameTime;
        }

        public override bool OnEffectDispelled(double currentTime)
        {
            if (ComponentFlu != null && ComponentFlu.HasFlu)
            {
                ComponentFlu.m_fluDuration = 0f;
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
