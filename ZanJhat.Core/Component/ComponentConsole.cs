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
    public class ComponentConsole : Component, IUpdateable
    {
        public SubsystemTime m_subsystemTime;
        public SubsystemGameInfo m_subsystemGameInfo;
        public SubsystemParticles m_subsystemParticles;
        public SubsystemBodies m_subsystemBodies;
        public SubsystemTerrain m_subsystemTerrain;
        public SubsystemMovingBlocks m_subsystemMovingBlocks;
        public SubsystemPlayers m_subsystemPlayers;
        public SubsystemConsole m_subsystemConsole;

        public ComponentPlayer m_componentPlayer;
        public ComponentHealth m_componentHealth;
        public ComponentBody m_componentBody;

        public ButtonWidget m_consoleButton;

        public UpdateOrder UpdateOrder => UpdateOrder.Default;

        public virtual void Update(float dt)
        {
            ComponentGui componentGui = m_componentPlayer.ComponentGui;
            if (m_consoleButton.IsClicked)
            {
                DialogsManager.ShowDialog(m_componentPlayer.GuiWidget, new ConsoleDialog(this));
            }
        }

        public virtual void AddMessage(MessageType type, string senderName, string message)
        {
            m_subsystemConsole.AddMessage(this, type, senderName, message);
        }

        public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
        {
            base.Load(valuesDictionary, idToEntityMap);

            m_subsystemTime = Project.FindSubsystem<SubsystemTime>(true);
            m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(true);
            m_subsystemParticles = Project.FindSubsystem<SubsystemParticles>(true);
            m_subsystemBodies = Project.FindSubsystem<SubsystemBodies>(true);
            m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(true);
            m_subsystemMovingBlocks = Project.FindSubsystem<SubsystemMovingBlocks>(true);
            m_subsystemPlayers = Project.FindSubsystem<SubsystemPlayers>(true);
            m_subsystemConsole = Project.FindSubsystem<SubsystemConsole>(true);

            m_componentPlayer = Entity.FindComponent<ComponentPlayer>(true);
            m_componentHealth = Entity.FindComponent<ComponentHealth>(true);
            m_componentBody = Entity.FindComponent<ComponentBody>(true);

            GameWidget gameWidget = m_componentPlayer.GameWidget;

            m_consoleButton = gameWidget.Children.Find<ButtonWidget>("ConsoleButton", false);

            if (m_consoleButton == null)
            {
                StackPanelWidget moreContents = gameWidget.Children.Find<StackPanelWidget>("MoreContents");
                m_consoleButton = new BitmapButtonWidget
                {
                    Size = new Vector2(68f, 64f),
                    NormalSubtexture = ContentManager.Get<Subtexture>("Textures/Gui/ConsoleButton"),
                    ClickedSubtexture = ContentManager.Get<Subtexture>("Textures/Gui/ConsoleButton_Pressed"),
                    Margin = new Vector2(4f, 0f)
                };
                moreContents.Children.Add(m_consoleButton);
            }
        }

        public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
        {
            base.Save(valuesDictionary, entityToIdMap);
        }
    }
}