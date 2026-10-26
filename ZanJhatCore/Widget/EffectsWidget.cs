using Engine;
using System.Xml.Linq;
using System;
using Game;

namespace ZanJhat.Core
{
    public class EffectsWidget : CanvasWidget
    {
        public ComponentEffect m_componentEffect;

        public StackPanelWidget m_effectsPanel;

        public int m_lastEffectCount;

        public EffectsWidget(ComponentEffect componentEffect)
        {
            m_componentEffect = componentEffect;
            XElement node = ContentManager.Get<XElement>("Widgets/EffectsWidget");
            LoadContents(this, node);

            m_effectsPanel = Children.Find<StackPanelWidget>("EffectsPanel");

            UpdateEffectsPanel();
            m_lastEffectCount = m_componentEffect.Effects.Count;
        }

        public void UpdateEffectsPanel()
        {
            if (m_effectsPanel == null || m_componentEffect == null)
                return;

            m_effectsPanel.Children.Clear();

            for (int i = m_componentEffect.Effects.Count - 1; i >= 0; i--)
            {
                Effect effect = m_componentEffect.Effects[i];
                CanvasWidget effectWidget = new CanvasWidget
                {
                    Size = new Vector2(float.PositiveInfinity, 96f),
                    Margin = new Vector2(20f, 10f)
                };
                m_effectsPanel.Children.Add(effectWidget);

                RectangleWidget background = new RectangleWidget
                {
                    OutlineThickness = 2f,
                    FillColor = new Color(33, 33, 33),
                    OutlineColor = new Color(85, 85, 85)
                };
                effectWidget.Children.Add(background);

                StackPanelWidget row = new StackPanelWidget
                {
                    Direction = LayoutDirection.Horizontal,
                    HorizontalAlignment = WidgetAlignment.Near,
                    VerticalAlignment = WidgetAlignment.Center,
                    Margin = new Vector2(10f)
                };
                effectWidget.Children.Add(row);

                Subtexture icon = ContentManager.Get<Subtexture>(effect.IconPath, null, false);
                if (icon == null)
                {
                    Log.Warning($"[Effect] {effect.IconPath} is missing. Use fallback");
                    icon = ContentManager.Get<Subtexture>("Textures/Gui/Unavailable");
                }

                RectangleWidget iconWidget = new RectangleWidget
                {
                    Size = new Vector2(76f),
                    OutlineColor = Color.Transparent,
                    FillColor = Color.White,
                    TextureLinearFilter = false,
                    Subtexture = icon,
                    HorizontalAlignment = WidgetAlignment.Near,
                    VerticalAlignment = WidgetAlignment.Center
                };
                row.Children.Add(iconWidget);

                StackPanelWidget col = new StackPanelWidget
                {
                    Direction = LayoutDirection.Vertical,
                    HorizontalAlignment = WidgetAlignment.Near,
                    VerticalAlignment = WidgetAlignment.Center,
                    Margin = new Vector2(10f, 0f)
                };
                row.Children.Add(col);

                LabelWidget name = new LabelWidget
                {
                    Color = Color.White,
                    FontScale = 1f,
                    HorizontalAlignment = WidgetAlignment.Near,
                    VerticalAlignment = WidgetAlignment.Center,
                    Text = effect.Name +
                        (effect is StackEffect stackEffect ? $" x{stackEffect.CurrentStack}" :
                        effect is LevelEffect levelEffect ? $" Lv {levelEffect.CurrentLevel}" : "")
                };
                col.Children.Add(name);

                double timeRemaining = effect.EndTime - m_componentEffect.m_subsystemGameInfo.TotalElapsedGameTime;
                string formatted = TimeUtils.FormatTime((float)timeRemaining, TimeUtils.TimeFormat.Short);

                LabelWidget time = new LabelWidget
                {
                    Color = Color.White,
                    FontScale = 0.75f,
                    HorizontalAlignment = WidgetAlignment.Near,
                    VerticalAlignment = WidgetAlignment.Center,
                    Text = formatted
                };
                col.Children.Add(time);
            }
        }

        public override void Update()
        {
            if (!m_componentEffect.IsAddedToProject)
            {
                ParentWidget.Children.Remove(this);
                return;
            }

            if (m_lastEffectCount != m_componentEffect.Effects.Count)
            {
                UpdateEffectsPanel();
                m_lastEffectCount = m_componentEffect.Effects.Count;
            }

            UpdateLabels();
        }

        public void UpdateLabels()
        {
            double currentTime = m_componentEffect.m_subsystemGameInfo.TotalElapsedGameTime;

            for (int i = 0; i < m_effectsPanel.Children.Count; i++)
            {
                CanvasWidget effectWidget = m_effectsPanel.Children[i] as CanvasWidget;
                StackPanelWidget row = effectWidget.Children[1] as StackPanelWidget;
                StackPanelWidget col = row.Children[1] as StackPanelWidget;

                LabelWidget name = col.Children[0] as LabelWidget;
                LabelWidget time = col.Children[1] as LabelWidget;

                Effect effect = m_componentEffect.Effects[m_componentEffect.Effects.Count - 1 - i];

                name.Text = effect.Name +
                    (effect is StackEffect stack ? $" x{stack.CurrentStack}" :
                     effect is LevelEffect level ? $" Lv {level.CurrentLevel}" : "");

                double timeRemaining = effect.EndTime - currentTime;

                time.Text = TimeUtils.FormatTime((float)timeRemaining, TimeUtils.TimeFormat.Short);
            }
        }
    }
}
