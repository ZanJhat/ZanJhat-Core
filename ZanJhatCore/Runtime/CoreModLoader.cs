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
using Engine.Input;
using System.Globalization;
using Game;

namespace ZanJhat.Core
{
    public class CoreModLoader : ModLoader
    {
        public SubsystemGameInfo m_subsystemGameInfo;
        public SubsystemTime m_subsystemTime;
        public SubsystemAudio m_subsystemAudio;

        // Hook Event
        public static event Action<DamageItemContext> OnDamageItem;

        public override void __ModInitialize()
        {
            ModsManager.RegisterHook("OnProjectLoaded", this);
            ModsManager.RegisterHook("OnCameraListInit", this);
            ModsManager.RegisterHook("ManageCameras", this);
            ModsManager.RegisterHook("DamageItem", this);
            ModsManager.RegisterHook("RecalculateCameraProjection", this);
            ModsManager.RegisterHook("AfterWidgetUpdate", this);
            ModsManager.RegisterHook("OnSettingsScreenCreated", this);
            ModsManager.RegisterHook("OnLoadingFinished", this);
        }

        public override void OnProjectLoaded(Project project)
        {
            m_subsystemGameInfo = project.FindSubsystem<SubsystemGameInfo>(true);
            m_subsystemTime = project.FindSubsystem<SubsystemTime>(true);
            m_subsystemAudio = project.FindSubsystem<SubsystemAudio>(true);
        }

        public override IEnumerable<KeyValuePair<string, int>> GetCameraList()
        {
            yield return new KeyValuePair<string, int>("ZanJhat.Core.ShoulderCamera", 5);
        }

        public override void ManageCameras(GameWidget gameWidget)
        {
            gameWidget.AddCamera(new ShoulderCamera(gameWidget));
        }

        public override int DamageItem(Block block, int value, int damageCount, Entity owner, out bool skipVanilla)
        {
            skipVanilla = false;

            int durability = block.GetDurability(value);

            if (durability >= 0)
            {
                int currentDamage = block.GetDamage(value);
                int newDamage = currentDamage + damageCount;
                if (newDamage > durability)
                {
                    ComponentMiner componentMiner = owner?.FindComponent<ComponentMiner>();
                    Vector3? position = componentMiner?.ComponentCreature?.ComponentBody?.Position;

                    DamageItemContext ctx = new DamageItemContext
                    {
                        Block = block,
                        Value = value,
                        DamageCount = damageCount,
                        Owner = owner,
                        Position = position,
                        PlaySound = CoreSettingsManager.AudioSettings.DamageItem,
                        SoundPath = "Audio/Break"
                    };

                    OnDamageItem?.Invoke(ctx);

                    if (ctx.PlaySound)
                    {
                        string sound = ctx.SoundPath ?? "Audio/Break";

                        if (position.HasValue)
                            m_subsystemAudio.PlaySound(sound, 1f, 0f, position.Value, 8f, true);
                        else
                            AudioManager.PlaySound(sound, 1f, 0.0f, 0.0f);
                    }
                }
            }

            return value;
        }

        public override void RecalculateCameraProjection(Camera camera, ref Matrix projectionMatrix)
        {
            ComponentPlayer componentPlayer = camera?.GameWidget?.PlayerData?.ComponentPlayer;

            if (componentPlayer == null)
                return;

            ComponentDash componentDash = componentPlayer.Entity.FindComponent<ComponentDash>();

            if (componentDash != null)
            {
                float dashFov = componentDash.DashFov;

                projectionMatrix.M11 /= dashFov;
                projectionMatrix.M22 /= dashFov;
            }
        }

        public override void AfterWidgetUpdate(Widget widget)
        {
            if (widget == null) return;
        }

        public override void OnSettingsScreenCreated(SettingsScreen settingsScreen, out Dictionary<ButtonWidget, Action> buttonsToAdd)
        {
            buttonsToAdd = [];

            if (settingsScreen.Children.Find<ButtonWidget>("CoreSettings", false) == null)
            {
                // RainbowBevelledButtonWidget
                BevelledButtonWidget coreSettingsButton = new BevelledButtonWidget
                {
                    Name = "CoreSettings",
                    Text = "Core Settings",
                    Style = ContentManager.Get<XElement>("Styles/ButtonStyle_310x60"),
                    HorizontalAlignment = WidgetAlignment.Center,
                    VerticalAlignment = WidgetAlignment.Center,
                    Margin = new Vector2(0f, 5f),
                    BevelColor = Color.SkyBlue,
                    CenterColor = Color.SkyBlue //new Color(181, 172, 154, 128)
                };

                Action action = () =>
                {
                    ScreensManager.SwitchScreen("CoreSettings");
                };

                buttonsToAdd.Add(coreSettingsButton, action);
            }
        }

        public override void OnLoadingFinished(List<Action> actions)
        {
            actions.Add(() =>
            {
                // 1. Core systems
                CoreManager.Initialize();

                // 2. Settings
                CoreSettingsManager.Initialize();

                // 3. Gameplay systems
                EffectManager.Initialize();
                CommandManager.Initialize();

                // 4. UI
                ScreensManager.AddScreen("CoreSettings", new CoreSettingsScreen());
            });
        }
    }
}
