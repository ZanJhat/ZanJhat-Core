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
    public abstract class StackEffect : Effect
    {
        public int CurrentStack { get; protected set; }

        public int MaximumStack { get; protected set; }

        public bool ReduceStackOnExpire { get; protected set; }

        public double StackDuration { get; protected set; }

        public StackEffect(ComponentEffect owner, double duration, int currentStack, int maximumStack, bool reduceStackOnExpire, double stackDuration)
          : base(owner, duration)
        {
            MaximumStack = MathUtils.Max(1, maximumStack);
            CurrentStack = MathUtils.Clamp(currentStack, 1, MaximumStack);
            ReduceStackOnExpire = reduceStackOnExpire;
            StackDuration = stackDuration;
        }

        public override bool OnEffectExpired(double currentTime)
        {
            if (ReduceStackOnExpire && CurrentStack > 1)
            {
                // Nếu không có thời gian duy trì stack => xóa luôn effect
                if (StackDuration <= 0.0)
                    return true;

                CurrentStack--;
                EndTime = currentTime + StackDuration;
                StartTime = currentTime;

                return false;
            }
            return true;
        }

        public override void Merge(Effect effect, double currentTime)
        {
            if (!(effect is StackEffect stackEffect))
                return;

            double oldRemaining = MathUtils.Max(0.0, EndTime - currentTime);
            double newRemaining = MathUtils.Max(0.0, stackEffect.EndTime - currentTime);

            if (CurrentStack < MaximumStack)
            {
                int oldStack = CurrentStack;
                int addedStack = stackEffect.CurrentStack;

                int newStack = MathUtils.Min(CurrentStack + addedStack, MaximumStack);
                int actualAdded = newStack - oldStack;

                CurrentStack = newStack;

                double totalRemaining = (oldRemaining * oldStack) + (newRemaining * actualAdded);
                double averageRemaining = totalRemaining / CurrentStack;

                EndTime = currentTime + averageRemaining;
            }
            else
            {
                EndTime = currentTime + MathUtils.Max(oldRemaining, newRemaining);
            }

            StartTime = currentTime;
        }

        public override void Save(ValuesDictionary valuesDictionary)
        {
            base.Save(valuesDictionary);
            valuesDictionary.SetValue("CurrentStack", CurrentStack);
        }

        public override void Load(ValuesDictionary valuesDictionary)
        {
            base.Load(valuesDictionary);
            CurrentStack = valuesDictionary.GetValue<int>("CurrentStack", 1);
        }
    }
}
