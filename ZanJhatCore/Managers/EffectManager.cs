using System;
using System.Collections.Generic;
using Engine;
using Game;

namespace ZanJhat.Core
{
    public static class EffectManager
    {
        private static Dictionary<string, Func<ComponentEffect, double, Effect>> m_factories = new Dictionary<string, Func<ComponentEffect, double, Effect>>();

        public static void Initialize()
        {
            Register("Sickness", (owner, endTime) => new SicknessEffect(owner, endTime));
            Register("Flu", (owner, endTime) => new FluEffect(owner, endTime));
            Register("On Fire", (owner, endTime) => new OnFireEffect(owner, endTime));
            Register("Glowing", (owner, endTime) => new GlowingEffect(owner, endTime));
        }

        // Cập nhật hàm Register
        public static void Register(string effectName, Func<ComponentEffect, double, Effect> factory)
        {
            if (!m_factories.ContainsKey(effectName))
                m_factories.Add(effectName, factory);
            else
                Log.Warning($"[EffectManager] Duplicate registration for effect '{effectName}'. Ignored.");
        }

        public static Effect CreateEffect(string effectName, ComponentEffect owner, double endTime = 0.0)
        {
            if (m_factories.TryGetValue(effectName, out var factory))
                return factory(owner, endTime);

            Log.Warning($"[EffectManager] Effect '{effectName}' not found. Returning null.");
            return null;
        }
    }
}
