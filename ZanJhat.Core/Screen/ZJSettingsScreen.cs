using System.Xml.Linq;
using Engine;
using Game;

namespace ZanJhat.Core
{
    public class ZJSettingsScreen : Screen
    {
        public StackPanelWidget m_contentsStackPanel;

        SettingsScreenBuilder m_builder;

        public ZJSettingsScreen()
        {
            XElement node = ContentManager.Get<XElement>("Screens/CustomScreen");
            LoadContents(this, node);

            Children.Find<LabelWidget>("TopBar.Label").Text = "Adjust Mod Settings";
            m_contentsStackPanel = Children.Find<StackPanelWidget>("ContentsStackPanel");

        }

        public override void Enter(object[] parameters)
        {
            BuildSettings();
        }

        public override void Update()
        {
            m_builder.Update();

            if (Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
            {
                ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
            }
        }

        public void BuildSettings()
        {
            m_contentsStackPanel.Children.Clear();

            m_builder = new SettingsScreenBuilder(m_contentsStackPanel);

            foreach (var build in SettingsScreenRegistry.Builders)
            {
                build(m_builder);
            }
        }
    }
}
