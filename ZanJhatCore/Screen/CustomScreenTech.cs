using System.Xml.Linq;
using System;
using System.Collections.Generic;
using Engine.Audio;
using Engine;
using Game;

namespace ZanJhat.Core
{
    public class CustomScreenTech : Screen
    {
        public StackPanelWidget m_contentsStackPanel;

        private MusicManager.Mix m_previousMix;

        public const string TechScreenMusicPath = "Music/HiTechUISoundEffects";

        public CustomScreenTech()
        {
            XElement node = ContentManager.Get<XElement>("Screens/CustomScreenTech");
            LoadContents(this, node);

            //Children.Find<LabelWidget>("TopBar.Label").Text = "Custom Screen Tech";
            m_contentsStackPanel = Children.Find<StackPanelWidget>("ContentsStackPanel");
        }

        public override void Enter(object[] parameters)
        {
            base.Enter(parameters);

            m_previousMix = MusicManager.CurrentMix;
            MusicManager.CurrentMix = MusicManager.Mix.Other;

            if (MusicManager.m_fadeSound != null)
            {
                MusicManager.m_fadeSound.Dispose();
                MusicManager.m_fadeSound = null;
            }

            MusicManager.PlayMusic(TechScreenMusicPath, new Game.Random().Float(0f, 0.3f));

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

            if (MusicManager.m_sound == null || MusicManager.m_sound.State != SoundState.Playing)
                MusicManager.PlayMusic(TechScreenMusicPath, new Game.Random().Float(0f, 0.3f));

            if (Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
                ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
        }
    }
}
