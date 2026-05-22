using System.Xml.Linq;
using Engine;
using Game;

namespace ZanJhat.Core
{
    public class ConsoleDialog : Dialog
    {
        public ComponentConsole m_componentConsole;

        public StackPanelWidget m_logStackPanel;
        public BevelledButtonWidget m_qcButton;
        public TextBoxWidget m_inputTextBox;
        public BevelledButtonWidget m_sendButton;
        public BevelledButtonWidget m_cancelButton;

        public bool m_scrollPending;

        public ConsoleDialog(ComponentConsole componentConsole)
        {
            XElement node = ContentManager.Get<XElement>("Dialogs/ConsoleDialog");
            LoadContents(this, node);

            m_componentConsole = componentConsole;

            m_logStackPanel = Children.Find<StackPanelWidget>("LogStackPanel");
            m_qcButton = Children.Find<BevelledButtonWidget>("QCButton");
            m_inputTextBox = Children.Find<TextBoxWidget>("InputTextBox");
            m_sendButton = Children.Find<BevelledButtonWidget>("SendButton");
            m_cancelButton = Children.Find<BevelledButtonWidget>("CancelButton");

            UpdateLogUI();
        }

        public override void Update()
        {
            if (m_scrollPending)
            {
                ScrollToBottom();
                m_scrollPending = false;
            }

            if (m_sendButton.IsClicked)
            {
                if (!string.IsNullOrEmpty(m_inputTextBox.Text))
                {
                    PlayerData playerData = m_componentConsole.m_componentPlayer.PlayerData;
                    m_componentConsole.AddMessage(MessageType.Chat, playerData.Name, m_inputTextBox.Text);
                    UpdateLogUI();
                    m_inputTextBox.Text = string.Empty;
                }
                else
                {
                    UpdateLogUI();
                }
            }

            if (Input.Cancel || m_cancelButton.IsClicked)
            {
                Dismiss();
            }
        }

        public virtual void UpdateLogUI()
        {
            m_logStackPanel.Children.Clear();
            for (int i = 0; i < m_componentConsole.m_subsystemConsole.m_logs.Count; i++)
            {
                MessageLogEntry entry = m_componentConsole.m_subsystemConsole.m_logs[i];

                MessageType type = entry.Type;
                Color color = m_componentConsole.m_subsystemConsole.GetColor(type);
                string sender = string.IsNullOrEmpty(entry.Sender) ? string.Empty : $"{entry.Sender}: ";
                string content = entry.Content;

                LabelWidget label = new LabelWidget
                {
                    Color = color,
                    FontScale = 1f,
                    HorizontalAlignment = WidgetAlignment.Near,
                    WordWrap = true,
                    Text = $" [{type}] {sender}{content}"
                };
                m_logStackPanel.Children.Add(label);
            }

            m_scrollPending = true;
        }

        public void ScrollToBottom()
        {
            if (m_logStackPanel.ParentWidget is ScrollPanelWidget scrollPanel)
            {
                float maxScroll = MathUtils.Max(scrollPanel.m_scrollAreaLength - scrollPanel.ActualSize.Y, 0f);

                scrollPanel.ScrollPosition = maxScroll;
                scrollPanel.ScrollSpeed = 0f;
            }
        }

        public void Dismiss()
        {
            DialogsManager.HideDialog(this);
        }
    }
}
