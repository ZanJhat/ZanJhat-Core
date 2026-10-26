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
using System.Globalization;
using TemplatesDatabase;
using System.IO;
using System.Text;
using XmlUtilities;
using Game;

namespace ZanJhat.Core
{
    public class SicknessEffect : Effect
    {
        public override string Name => "Sickness";
        public override string IconPath => "Textures/Effects/Sickness";
        public override EffectType EffectType => EffectType.Debuff;
        public override bool NeedSave => false;

        public SicknessEffect(ComponentEffect owner, double duration)
          : base(owner, duration)
        {
        }

        public override void Update(float dt)
        {
            if (ComponentSickness == null || !ComponentSickness.IsSick)
                EndTime = SubsystemGameInfo.TotalElapsedGameTime;
        }

        public override bool OnEffectDispelled(double currentTime)
        {
            if (ComponentSickness != null && ComponentSickness.IsSick)
            {
                ComponentSickness.m_sicknessDuration = 0f;
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
