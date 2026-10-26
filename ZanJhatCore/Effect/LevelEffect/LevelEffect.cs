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
    public abstract class LevelEffect : Effect
    {
        public int CurrentLevel { get; protected set; }

        public int MaximumLevel { get; protected set; }

        public LevelEffect(ComponentEffect owner, double duration, int currentLevel, int maximumLevel)
          : base(owner, duration)
        {
            MaximumLevel = MathUtils.Max(1, maximumLevel);
            CurrentLevel = MathUtils.Clamp(currentLevel, 1, MaximumLevel);
        }

        public override void Merge(Effect effect, double currentTime)
        {
            if (!(effect is LevelEffect levelEffect))
                return;

            double oldRemaining = MathUtils.Max(0.0, EndTime - currentTime);
            double newRemaining = MathUtils.Max(0.0, levelEffect.EndTime - currentTime);

            if (CurrentLevel < levelEffect.CurrentLevel)
            {
                int differenceLevel = levelEffect.CurrentLevel - CurrentLevel;
                double bonusTime = oldRemaining / (double)Math.Pow(2, differenceLevel);
                EndTime = levelEffect.EndTime + bonusTime;
                CurrentLevel = levelEffect.CurrentLevel;

            }
            else if (CurrentLevel == levelEffect.CurrentLevel)
            {
                EndTime = MathUtils.Max(EndTime, levelEffect.EndTime);
            }
            else if (CurrentLevel > levelEffect.CurrentLevel)
            {
                int differenceLevel = CurrentLevel - levelEffect.CurrentLevel;
                double bonusTime = newRemaining / (double)Math.Pow(2, differenceLevel);
                EndTime += bonusTime;
            }

            StartTime = currentTime;
        }

        public override void Save(ValuesDictionary valuesDictionary)
        {
            base.Save(valuesDictionary);
            valuesDictionary.SetValue("CurrentLevel", CurrentLevel);
        }

        public override void Load(ValuesDictionary valuesDictionary)
        {
            base.Load(valuesDictionary);
            CurrentLevel = valuesDictionary.GetValue<int>("CurrentLevel", 1);
        }
    }
}
