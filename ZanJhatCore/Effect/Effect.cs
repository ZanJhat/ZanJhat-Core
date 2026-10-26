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
    public enum EffectType
    {
        Normal,
        Buff,
        Debuff
    }

    public abstract class Effect
    {
        public abstract string Name { get; }
        public abstract string IconPath { get; }
        public abstract EffectType EffectType { get; }
        public abstract bool NeedSave { get; }

        public virtual int[] DrawOrders => Array.Empty<int>();

        public ComponentEffect Owner { get; protected set; }
        public double StartTime { get; protected set; }
        public double EndTime { get; protected set; }

        public SubsystemTime SubsystemTime => Owner?.m_subsystemTime;
        public SubsystemGameInfo SubsystemGameInfo => Owner?.m_subsystemGameInfo;
        public SubsystemParticles SubsystemParticles => Owner?.m_subsystemParticles;
        public SubsystemModelsRenderer SubsystemModelsRenderer => Owner?.m_subsystemModelsRenderer;

        public ComponentPlayer ComponentPlayer => Owner?.m_componentPlayer;
        public ComponentSickness ComponentSickness => Owner?.m_componentSickness;
        public ComponentFlu ComponentFlu => Owner?.m_componentFlu;
        public ComponentOnFire ComponentOnFire => Owner?.m_componentOnFire;
        public ComponentLocomotion ComponentLocomotion => Owner?.m_componentLocomotion;
        public ComponentHealth ComponentHealth => Owner?.m_componentHealth;
        public ComponentBody ComponentBody => Owner?.m_componentBody;
        public ComponentGui ComponentGui => Owner?.m_componentGui;
        public ComponentCreatureModel ComponentCreatureModel => Owner?.m_componentCreatureModel;
        public ComponentModel ComponentModel => Owner?.m_componentModel;

        public PrimitivesRenderer3D PrimitivesRenderer => Owner?.PrimitivesRenderer;

        public Effect(ComponentEffect owner, double duration)
        {
            Owner = owner;
            StartTime = SubsystemGameInfo.TotalElapsedGameTime;
            EndTime = StartTime + duration;
        }

        public double Duration => EndTime - StartTime;

        public float Progress
        {
            get
            {
                if (Duration <= 0)
                    return 0f;

                double currentTime = SubsystemGameInfo.TotalElapsedGameTime;
                double progress = (currentTime - StartTime) / Duration;
                return MathUtils.Saturate((float)progress);
            }
        }

        public float RemainingProgress
        {
            get
            {
                if (Duration <= 0)
                    return 0f;

                double currentTime = SubsystemGameInfo.TotalElapsedGameTime;
                double remainingProgress = (EndTime - currentTime) / Duration;
                return MathUtils.Saturate((float)remainingProgress);
            }
        }

        public virtual void Update(float dt)
        {
            // Cập nhật mỗi frame
        }

        public virtual void Draw(Camera camera, int drawOrder)
        {
            // Vẽ
        }

        public virtual void Save(ValuesDictionary valuesDictionary)
        {
            valuesDictionary.SetValue("StartTime", StartTime);
            valuesDictionary.SetValue("EndTime", EndTime);
        }

        public virtual void Load(ValuesDictionary valuesDictionary)
        {
            StartTime = valuesDictionary.GetValue<double>("StartTime", SubsystemGameInfo?.TotalElapsedGameTime ?? 0);
            EndTime = valuesDictionary.GetValue<double>("EndTime", StartTime);
        }

        public virtual void OnEntityRemoved()
        {
            // Gọi khi Owner bị xoá
        }

        public virtual void OnEffectRemoved()
        {
            // Gọi khi xóa hiệu ứng
        }

        public virtual bool OnEffectDispelled(double currentTime)
        {
            // Gọi khi hiệu ứng được hóa giải
            return true;
        }

        public virtual bool OnEffectExpired(double currentTime)
        {
            // Gọi khi hiệu ứng hết thời gian
            return true;
        }

        public virtual void OnEffectAdded(double currentTime)
        {
            // Gọi khi hiệu ứng vừa được áp dụng
        }

        public virtual void Merge(Effect effect, double currentTime)
        {
            // Gọi khi dính hiệu ứng trùng lặp

            if (effect.EndTime > EndTime)
            {
                EndTime = effect.EndTime;
                StartTime = currentTime;
            }
        }
    }
}
