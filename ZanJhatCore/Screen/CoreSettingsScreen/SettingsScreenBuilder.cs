using System;
using System.Collections.Generic;
using Engine;
using Game;

namespace ZanJhat.Core
{
    public class SettingsScreenBuilder
    {
        StackPanelWidget m_panel;

        List<ButtonToggle> m_toggles = new();
        List<ButtonEnum> m_enums = new();
        List<SliderItem> m_sliders = new();

        public SettingsScreenBuilder(StackPanelWidget panel)
        {
            m_panel = panel;
        }

        public void Update()
        {
            foreach (var t in m_toggles)
            {
                if (t.Button.IsClicked)
                    t.Setter(!t.Getter());

                bool value = t.Getter();
                t.Button.Text = t.TextFormatter != null
                    ? t.TextFormatter(value)
                    : (value ? "ON" : "OFF");
            }

            foreach (var e in m_enums)
            {
                if (e.Button.IsClicked)
                {
                    Array values = Enum.GetValues(e.EnumType);
                    int index = Array.IndexOf(values, e.Getter());
                    index = (index + 1) % values.Length;

                    e.Setter(values.GetValue(index));
                }

                object value = e.Getter();

                e.Button.Text = e.TextFormatter != null
                    ? e.TextFormatter(value)
                    : value.ToString();
            }

            foreach (var s in m_sliders)
            {
                if (s.Slider.IsSliding)
                {
                    float v = s.Slider.Value;

                    if (s.Step > 0)
                        v = MathUtils.Round(v / s.Step) * s.Step;

                    s.Setter(v);
                }
                else
                    s.Slider.Value = s.Getter();

                float value = s.Getter();

                s.Slider.Text = s.TextFormatter != null
                    ? s.TextFormatter(value)
                    : value.ToString("0.#");
            }
        }

        public void AddHeader(string text)
        {
            m_panel.Children.Add(new LabelWidget
            {
                Text = text,
                HorizontalAlignment = WidgetAlignment.Center,
                Margin = new Vector2(0, 20),
                Color = new Color(128, 128, 128)
            });
        }

        public void AddToggle(
            string name,
            Func<bool> getter,
            Action<bool> setter,
            Func<bool, string> formatter = null)
        {
            var row = CreateRow(name);

            var button = new BevelledButtonWidget
            {
                Size = new Vector2(310, 60)
            };

            row.Children.Add(button);
            m_panel.Children.Add(row);

            m_toggles.Add(new ButtonToggle
            {
                Button = button,
                Getter = getter,
                Setter = setter,
                TextFormatter = formatter
            });
        }

        public void AddEnum<T>(
            string name,
            Func<T> getter,
            Action<T> setter,
            Func<object, string> formatter = null) where T : Enum
        {
            var row = CreateRow(name);

            var button = new BevelledButtonWidget
            {
                Size = new Vector2(310, 60)
            };

            row.Children.Add(button);
            m_panel.Children.Add(row);

            m_enums.Add(new ButtonEnum
            {
                Button = button,
                Getter = () => getter(),
                Setter = v => setter((T)v),
                EnumType = typeof(T),
                TextFormatter = formatter
            });
        }

        public void AddSlider(
            string name,
            Func<float> getter,
            Action<float> setter,
            float min,
            float max,
            float step = 0,
            Func<float, string> formatter = null)
        {
            var row = CreateRow(name);

            var slider = new SliderWidget
            {
                Size = new Vector2(410, 60),
                MinValue = min,
                MaxValue = max
            };

            row.Children.Add(slider);
            m_panel.Children.Add(row);

            m_sliders.Add(new SliderItem
            {
                Slider = slider,
                Getter = getter,
                Setter = setter,
                Step = step,
                TextFormatter = formatter
            });
        }

        UniformSpacingPanelWidget CreateRow(string labelText)
        {
            var row = new UniformSpacingPanelWidget
            {
                Direction = LayoutDirection.Horizontal,
                Margin = new Vector2(0, 3)
            };

            row.Children.Add(new LabelWidget
            {
                Text = labelText,
                HorizontalAlignment = WidgetAlignment.Far,
                VerticalAlignment = WidgetAlignment.Center,
                Margin = new Vector2(20, 0)
            });

            return row;
        }

        class ButtonToggle
        {
            public ButtonWidget Button;
            public Func<bool> Getter;
            public Action<bool> Setter;
            public Func<bool, string> TextFormatter;
        }

        class ButtonEnum
        {
            public ButtonWidget Button;
            public Func<object> Getter;
            public Action<object> Setter;
            public Type EnumType;
            public Func<object, string> TextFormatter;
        }

        class SliderItem
        {
            public SliderWidget Slider;
            public Func<float> Getter;
            public Action<float> Setter;
            public float Step;
            public Func<float, string> TextFormatter;
        }
    }
}
