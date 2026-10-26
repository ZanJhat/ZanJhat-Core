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
using ZanJhat.Core;

namespace ZanJhat.Core
{
    public class ComponentEffect : Component, IUpdateable, IDrawable
    {
        public SubsystemTime m_subsystemTime;
        public SubsystemGameInfo m_subsystemGameInfo;
        public SubsystemParticles m_subsystemParticles;
        public SubsystemModelsRenderer m_subsystemModelsRenderer;

        public ComponentPlayer m_componentPlayer;
        public ComponentSickness m_componentSickness;
        public ComponentFlu m_componentFlu;
        public ComponentOnFire m_componentOnFire;
        public ComponentLocomotion m_componentLocomotion;
        public ComponentHealth m_componentHealth;
        public ComponentBody m_componentBody;
        public ComponentGui m_componentGui;
        public ComponentCreatureModel m_componentCreatureModel;
        public ComponentModel m_componentModel;

        public OutlineShader m_outlineShader;

        public EffectSettings m_effectSettings;
        public float Scale => m_effectSettings.Scale;
        // Last settings
        public float m_lastScale;
        public LayoutDirection m_lastLayoutDirection;

        public AutoSizeCanvasWidget m_effectsWidget;
        public StackPanelWidget m_effectsPanel;

        private List<Effect> m_effects = new List<Effect>();
        public IReadOnlyList<Effect> Effects => m_effects;

        public bool m_isGuiDirty;

        public PrimitivesRenderer3D PrimitivesRenderer = new();

        // Hook trước khi Add: Cho phép module khác quyết định có cho phép add effect này không (Ví dụ: Đeo nhẫn kháng độc -> return false)
        public event Func<Effect, bool> EffectAdding;

        // Hook sau khi Add/Merge thành công: Dùng để play âm thanh, hiện particle, update chỉ số...
        public event Action<Effect> EffectAdded;
        public event Action<Effect> EffectMerged;

        // Hook khi Effect bị xóa (bất kể là do hết giờ, bị dispel hay gọi hàm Remove)
        public event Action<Effect> EffectRemoved;

        // Hook cho UI: Cho phép module khác chèn thêm icon phụ, đổi màu viền, hoặc add thêm tooltip vào nút Effect
        public event Action<Effect, BitmapButtonWidget> EffectUiBuilt;

        public UpdateOrder UpdateOrder => UpdateOrder.Default;

        public int[] DrawOrders
        {
            get => m_effects.SelectMany(e => e.DrawOrders).Distinct().ToArray();
        }

        public virtual void AddEffect(Effect effect)
        {
            // --- HOOK: PRE-ADD (HỆ THỐNG KHÁNG TÍNH) ---
            if (EffectAdding != null)
            {
                // Duyệt qua tất cả các hàm đăng ký, nếu có bất kỳ hàm nào trả về false -> Hủy bỏ add effect
                foreach (Func<Effect, bool> handler in EffectAdding.GetInvocationList())
                {
                    if (!handler.Invoke(effect))
                        return;
                }
            }

            double currentTime = m_subsystemGameInfo.TotalElapsedGameTime;

            // Tìm effect cùng loại (theo Name)
            Effect existingEffect = m_effects.FirstOrDefault(e => e.Name == effect.Name);

            if (existingEffect == null)
            {
                // Nếu chưa tồn tại thì thêm mới
                m_effects.Add(effect);
                effect.OnEffectAdded(currentTime);

                // --- HOOK: POST-ADD ---
                EffectAdded?.Invoke(effect);
            }
            else
            {
                // Nếu đã tồn tại thì hợp nhất
                existingEffect.Merge(effect, currentTime);

                // --- HOOK: MERGED ---
                EffectMerged?.Invoke(existingEffect);
            }

            m_isGuiDirty = true;
        }

        public virtual void RemoveEffect(Effect effect)
        {
            effect.OnEffectRemoved();

            // Hook
            EffectRemoved?.Invoke(effect);

            m_effects.Remove(effect);
            m_isGuiDirty = true;
        }

        public virtual void ExpireEffect(Effect effect)
        {
            double currentTime = m_subsystemGameInfo.TotalElapsedGameTime;

            if (effect.OnEffectExpired(currentTime))
            {
                // Hook
                EffectRemoved?.Invoke(effect);

                m_effects.Remove(effect);
                m_isGuiDirty = true;
            }
        }

        public virtual void DispelleEffect(Effect effect)
        {
            double currentTime = m_subsystemGameInfo.TotalElapsedGameTime;

            if (effect.OnEffectDispelled(currentTime))
            {
                // Hook
                EffectRemoved?.Invoke(effect);

                m_effects.Remove(effect);
                m_isGuiDirty = true;
            }
        }

        public virtual void Update(float dt)
        {
            double currentTime = m_subsystemGameInfo.TotalElapsedGameTime;

            for (int i = m_effects.Count - 1; i >= 0; i--)
            {
                Effect effect = m_effects[i];
                effect.Update(dt);
                if (currentTime >= effect.EndTime)
                {
                    ExpireEffect(effect);
                }
            }

            if (m_subsystemTime.PeriodicGameTimeEvent(0.1, 0.0))
            {
                UpdateBaseEffect();
            }

            // Kiểm tra Settings thay đổi
            if (m_effectSettings.Scale != m_lastScale || m_effectSettings.LayoutDirection != m_lastLayoutDirection)
            {
                m_lastScale = m_effectSettings.Scale;
                m_lastLayoutDirection = m_effectSettings.LayoutDirection;

                if (m_effectsPanel != null)
                {
                    m_effectsPanel.Direction = m_effectSettings.LayoutDirection;
                    m_effectsPanel.Margin = new Vector2(-4f * m_lastScale);
                }

                m_isGuiDirty = true;
            }

            // Kiểm tra click từng button
            if (m_componentGui != null && m_effectsWidget != null && m_effectsPanel != null)
            {
                WidgetUtils.SetAnchor(m_effectsWidget, m_componentGui.ControlsContainerWidget, m_effectSettings.Anchor, m_effectSettings.MarginX, m_effectSettings.MarginY);

                foreach (BitmapButtonWidget childButton in m_effectsPanel.Children.OfType<BitmapButtonWidget>())
                {
                    if (childButton.IsClicked)
                    {
                        if (m_componentPlayer.ComponentGui.ModalPanelWidget is EffectsWidget)
                            m_componentPlayer.ComponentGui.ModalPanelWidget = null;
                        else
                            m_componentPlayer.ComponentGui.ModalPanelWidget = new EffectsWidget(this);
                    }
                }
            }

            if (m_isGuiDirty)
            {
                UpdateGui();
                m_isGuiDirty = false;
            }

            UpdateProgress(currentTime);

            if (m_effectsWidget != null)
                m_effectsWidget.IsVisible = m_effectSettings.Enable;

            /*if (m_subsystemTime.PeriodicGameTimeEvent(5.0, 0.0))
            {
                AddEffect(new GlowingEffect(this, 10.0));

                if (m_componentSickness != null)
                    m_componentSickness.m_sicknessDuration = 10f;

                if (m_componentFlu != null)
                    m_componentFlu.m_fluDuration = 10f;
            }*/

        }

        public void UpdateBaseEffect()
        {
            if (m_componentSickness != null && m_componentSickness.IsSick)
                AddEffect(new SicknessEffect(this, m_componentSickness.m_sicknessDuration));

            if (m_componentFlu != null && m_componentFlu.HasFlu)
                AddEffect(new FluEffect(this, m_componentFlu.m_fluDuration));

            if (m_componentOnFire != null && m_componentOnFire.IsOnFire)
                AddEffect(new OnFireEffect(this, m_componentOnFire.m_fireDuration));


            if (m_componentLocomotion != null && m_componentLocomotion.StunTime > 0f)
                AddEffect(new StunEffect(this, m_componentLocomotion.StunTime));
        }

        public void UpdateGui()
        {
            if (m_componentGui == null || m_effectsWidget == null || m_effectsPanel == null)
                return;

            m_effectsPanel.Children.Clear();
            double currentTime = m_subsystemGameInfo.TotalElapsedGameTime;

            int totalEffects = m_effects.Count;

            for (int i = m_effects.Count - 1; i >= 0; i--)
            {
                // Nếu đã hiển thị đủ 4 ô thì dừng
                if (m_effectsPanel.Children.Count >= 4)
                    break;

                Effect effect = m_effects[i];

                Vector2 margin = m_effectSettings.LayoutDirection == LayoutDirection.Vertical ? new Vector2(0f, 2f * Scale) : new Vector2(2f * Scale, 0f);

                BitmapButtonWidget bitmapButton = new BitmapButtonWidget
                {
                    Size = new Vector2(64f * Scale),
                    Margin = margin,
                    FontScale = 1f * Scale
                };
                bitmapButton.m_rectangleWidget.FillColor = new Color(33, 33, 33);
                bitmapButton.m_rectangleWidget.OutlineColor = new Color(85, 85, 85);
                bitmapButton.m_rectangleWidget.OutlineThickness = 2f * Scale;

                // Nếu đã hiển thị 3 ô VÀ tổng số hiệu ứng > 4
                if (m_effectsPanel.Children.Count == 3 && totalEffects > 4)
                {
                    // Hiển thị ô thứ 4 với số lượng còn lại sau 3 ô đầu
                    bitmapButton.Text = $"+{totalEffects - 3}";

                    m_effectsPanel.Children.Add(bitmapButton);
                    break;
                }
                else
                {
                    // Hiển thị icon hiệu ứng bình thường
                    Subtexture icon = ContentManager.Get<Subtexture>(effect.IconPath, null, false);

                    if (icon == null)
                    {
                        Log.Warning($"[Effect] {effect.IconPath} is missing. Use fallback");
                        icon = ContentManager.Get<Subtexture>("Textures/Gui/Unavailable");
                    }

                    bitmapButton.m_imageWidget.Subtexture = icon;
                    bitmapButton.m_imageWidget.Margin = new Vector2(5f * Scale);
                    bitmapButton.m_imageWidget.IsVisible = true;
                    bitmapButton.m_imageWidget.OutlineColor = Color.Transparent;
                    bitmapButton.m_imageWidget.TextureLinearFilter = false;

                    string text = "";

                    if (effect is StackEffect stackEffect)
                        text = $"x{stackEffect.CurrentStack}";
                    else if (effect is LevelEffect levelEffect)
                        text = $"Lv {levelEffect.CurrentLevel}";

                    WidgetUtils.AddLabel(bitmapButton, text, Color.White, 0.5f * Scale, false, new Vector2(2f * Scale), WidgetAlignment.Far, WidgetAlignment.Near);

                    ValueFillWidget valueFill = new ValueFillWidget
                    {
                        Value = MathUtils.Saturate(1f - effect.Progress),
                        BarSize = new Vector2(2f * Scale, 0f),
                        LayoutDirection = LayoutDirection.Vertical,
                        VerticalAlignment = WidgetAlignment.Stretch,
                        HorizontalAlignment = WidgetAlignment.Far,
                        FlipDirection = true,
                        Margin = new Vector2(2f * Scale)
                    };
                    bitmapButton.Children.Add(valueFill);

                    // --- HOOK: UI BUILT ---
                    // Cho phép các hệ thống khác sửa đổi bitmapButton trước khi nó được render (VD: đổi màu chữ, thêm viền, thêm Tooltip)
                    EffectUiBuilt?.Invoke(effect, bitmapButton);

                    m_effectsPanel.Children.Add(bitmapButton);
                }
            }
        }

        public void UpdateProgress(double currentTime)
        {
            if (m_effectsPanel == null || m_effects.Count == 0)
                return;

            int childIndex = 0;

            for (int i = m_effects.Count - 1; i >= 0; i--)
            {
                if (childIndex >= m_effectsPanel.Children.Count)
                    break;

                // Nếu đây là ô thứ 4 và tổng số effect > 4 (ô hiển thị "+X"), thì bỏ qua vì nó không có progress bar
                if (childIndex == 3 && m_effects.Count > 4)
                    break;

                Widget child = m_effectsPanel.Children[childIndex];

                if (child is BitmapButtonWidget bitmapButton)
                {
                    // Tìm ValueFillWidget bên trong button
                    ValueFillWidget valueFill = bitmapButton.Children.OfType<ValueFillWidget>().FirstOrDefault();
                    if (valueFill != null)
                    {
                        Effect effect = m_effects[i];
                        valueFill.Value = MathUtils.Saturate(1f - effect.Progress);
                    }
                }
                childIndex++;
            }
        }

        public override void OnEntityRemoved()
        {
            for (int i = m_effects.Count - 1; i >= 0; i--)
            {
                Effect effect = m_effects[i];
                effect.OnEntityRemoved();
            }
        }

        public void Draw(Camera camera, int drawOrder)
        {
            bool hasDrawnAnything = false;

            foreach (Effect effect in m_effects)
            {
                if (effect.DrawOrders.Contains(drawOrder))
                {
                    effect.Draw(camera, drawOrder);
                    hasDrawnAnything = true;
                }
            }

            if (hasDrawnAnything)
                PrimitivesRenderer.Flush(camera.ViewProjectionMatrix);
        }

        public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
        {
            base.Load(valuesDictionary, idToEntityMap);
            m_subsystemTime = Project.FindSubsystem<SubsystemTime>(true);
            m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(true);
            m_subsystemParticles = Project.FindSubsystem<SubsystemParticles>(true);
            m_subsystemModelsRenderer = Project.FindSubsystem<SubsystemModelsRenderer>(true);

            m_componentPlayer = Entity.FindComponent<ComponentPlayer>();
            m_componentSickness = Entity.FindComponent<ComponentSickness>();
            m_componentFlu = Entity.FindComponent<ComponentFlu>();
            m_componentOnFire = Entity.FindComponent<ComponentOnFire>();
            m_componentLocomotion = Entity.FindComponent<ComponentLocomotion>();
            m_componentHealth = Entity.FindComponent<ComponentHealth>();
            m_componentBody = Entity.FindComponent<ComponentBody>();
            m_componentGui = Entity.FindComponent<ComponentGui>();
            m_componentCreatureModel = Entity.FindComponent<ComponentCreatureModel>();
            m_componentModel = Entity.FindComponent<ComponentModel>();

            m_outlineShader = new OutlineShader(
                ContentManager.Get<string>("Shaders/OutlineShader", ".vsh"),
                ContentManager.Get<string>("Shaders/OutlineShader", ".psh"));

            m_effectSettings = CoreSettingsManager.EffectSettings;
            m_lastScale = m_effectSettings.Scale;
            m_lastLayoutDirection = m_effectSettings.LayoutDirection;

            if (m_componentGui != null)
            {
                m_effectsWidget = m_componentGui.ControlsContainerWidget.Children.Find<AutoSizeCanvasWidget>("EffectsWidget", false);
                m_effectsPanel = m_componentGui.ControlsContainerWidget.Children.Find<StackPanelWidget>("EffectsPanel", false);

                if (m_effectsWidget == null || m_effectsPanel == null)
                    CreateEffectsWidget();
            }

            LoadEffect(valuesDictionary);
        }

        public void CreateEffectsWidget()
        {
            m_effectsWidget = new AutoSizeCanvasWidget
            {
                Name = "EffectsWidget"
            };

            m_effectsPanel = new StackPanelWidget
            {
                Name = "EffectsPanel",
                Direction = m_effectSettings.LayoutDirection,
                Margin = new Vector2(-4f * Scale)
            };
            m_effectsWidget.Children.Add(m_effectsPanel);

            m_componentGui.ControlsContainerWidget.Children.Insert(0, m_effectsWidget);
        }

        public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
        {
            base.Save(valuesDictionary, entityToIdMap);
            SaveEffect(valuesDictionary);
        }

        public virtual void LoadEffect(ValuesDictionary valuesDictionary)
        {
            ValuesDictionary effectsDataDict = valuesDictionary.GetValue<ValuesDictionary>("EffectsData", null);

            if (effectsDataDict != null)
            {
                // Duyệt qua các key (Effect_0, Effect_1...)
                int index = 0;
                while (true)
                {
                    ValuesDictionary singleEffectDict = effectsDataDict.GetValue<ValuesDictionary>($"Effect_{index}", null);

                    // Nếu không tìm thấy effect nào ở index này nữa thì thoát vòng lặp
                    if (singleEffectDict == null)
                        break;

                    string effectName = singleEffectDict.GetValue<string>("Name", null);

                    if (!string.IsNullOrEmpty(effectName))
                    {
                        Effect effect = EffectManager.CreateEffect(effectName, this);

                        if (effect != null)
                        {
                            effect.Load(singleEffectDict);
                            m_effects.Add(effect);
                        }
                        else
                        {
                            Log.Warning($"[ComponentEffect] Failed to create effect: {effectName}");
                        }
                    }

                    index++;
                }
            }

            m_isGuiDirty = true;
        }

        public virtual void SaveEffect(ValuesDictionary valuesDictionary)
        {
            ValuesDictionary effectsDataDict = new ValuesDictionary();
            int index = 0;

            foreach (Effect effect in m_effects)
            {
                if (effect.NeedSave)
                {
                    ValuesDictionary singleEffectDict = new ValuesDictionary();

                    // Bắt buộc phải lưu Name để lúc Load biết đường tạo đúng class
                    singleEffectDict.SetValue("Name", effect.Name);

                    // Gọi hàm Save của từng effect
                    effect.Save(singleEffectDict);

                    // Lưu vào dictionary tổng với key tăng dần (Effect_0, Effect_1,...)
                    effectsDataDict.SetValue($"Effect_{index}", singleEffectDict);
                    index++;
                }
            }

            valuesDictionary.SetValue("EffectsData", effectsDataDict);
        }
    }
}
