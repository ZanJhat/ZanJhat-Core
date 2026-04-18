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
        public override void __ModInitialize()
        {
            ModsManager.RegisterHook("AfterWidgetUpdate", this);
            ModsManager.RegisterHook("OnSettingsScreenCreated", this);
            ModsManager.RegisterHook("OnLoadingFinished", this);
        }

        public override void AfterWidgetUpdate(Widget widget)
        {
            if (widget == null) return;

            if (widget is SettingsScreen settingsScreen)
            {
                ButtonWidget zjSettingsButton = settingsScreen.Children.Find<ButtonWidget>("ZJSettings", false);
                if (zjSettingsButton != null)
                {
                    Color color = ColorUtils.GetRainbowColor();
                    zjSettingsButton.Color = color;
                }
            }
        }

        public override void OnSettingsScreenCreated(SettingsScreen settingsScreen, out Dictionary<ButtonWidget, Action> buttonsToAdd)
        {
            buttonsToAdd = [];

            if (settingsScreen.Children.Find<ButtonWidget>("ZJSettings", false) == null)
            {
                BevelledButtonWidget zjSettingsButton = new BevelledButtonWidget
                {
                    Name = "ZJSettings",
                    Text = "Mod Settings",
                    Style = ContentManager.Get<XElement>("Styles/ButtonStyle_310x60"),
                    HorizontalAlignment = WidgetAlignment.Center,
                    VerticalAlignment = WidgetAlignment.Center,
                    Margin = new Vector2(0f, 5f)
                };
                Action action = () =>
                {
                    ScreensManager.SwitchScreen("ZJSettings");
                };
                buttonsToAdd.Add(zjSettingsButton, action);
            }
        }

        public override void OnLoadingFinished(List<System.Action> actions)
        {
            actions.Add(() =>
            {
                CommandManager.Initialize();
                CoreSettingsManager.Initialize();
                ScreensManager.AddScreen("ZJSettings", new ZJSettingsScreen());
            });
        }
    }
}