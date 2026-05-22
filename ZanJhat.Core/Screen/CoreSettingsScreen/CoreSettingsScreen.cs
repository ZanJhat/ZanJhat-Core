using System.Xml.Linq;
using System;
using System.Collections.Generic;
using Engine.Audio;
using Engine;
using Game;

namespace ZanJhat.Core
{
    public class CoreSettingsScreen : Screen
    {
        public StackPanelWidget m_contentsStackPanel;
        public SettingsScreenBuilder m_builder;

        private MusicManager.Mix m_previousMix;

        public CoreSettingsScreen()
        {
            XElement node = ContentManager.Get<XElement>("Screens/CustomScreenTech");
            LoadContents(this, node);

            Children.Find<LabelWidget>("TopBar.Label").Text = "Adjust Mod Settings";
            m_contentsStackPanel = Children.Find<StackPanelWidget>("ContentsStackPanel");
        }

        public override void Enter(object[] parameters)
        {
            base.Enter(parameters);

            BuildSettings();

            m_previousMix = MusicManager.CurrentMix;
            MusicManager.CurrentMix = MusicManager.Mix.Other;

            if (MusicManager.m_fadeSound != null)
            {
                MusicManager.m_fadeSound.Dispose();
                MusicManager.m_fadeSound = null;
            }

            MusicManager.PlayMusic("Music/BinaryCode", new Game.Random().Float(0f, 0.3f));

            if (MusicManager.m_sound != null)
                MusicManager.m_sound.Volume = MusicManager.Volume;
        }

        public override void Leave()
        {
            base.Leave();

            MusicManager.StopMusic();
            MusicManager.CurrentMix = m_previousMix;
        }

        public override void Update()
        {
            base.Update();

            m_builder.Update();

            if (MusicManager.m_sound == null || MusicManager.m_sound.State != SoundState.Playing)
                MusicManager.PlayMusic("Music/BinaryCode", new Game.Random().Float(0f, 0.3f));

            if (Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
                ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
        }

        public void BuildSettings()
        {
            m_contentsStackPanel.Children.Clear();

            m_builder = new SettingsScreenBuilder(m_contentsStackPanel);

            foreach (SettingsSection section in SettingsScreenRegistry.Sections.Values)
            {
                if (section.ShowHeader)
                    m_builder.AddHeader(section.Name);

                foreach (Action<SettingsScreenBuilder> build in section.Builders)
                    build(m_builder);
            }
        }
    }
}
