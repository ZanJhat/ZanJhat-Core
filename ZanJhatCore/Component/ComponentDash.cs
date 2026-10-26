using Engine;
using Engine.Input;
using GameEntitySystem;
using TemplatesDatabase;
using System;
using Game;

namespace ZanJhat.Core
{
    public struct DashCheckResult
    {
        public bool Allowed;
        public string Reason;
    }

    public class ComponentDash : Component, IUpdateable
    {
        public SubsystemGameInfo m_subsystemGameInfo;

        public ComponentPlayer m_componentPlayer;
        public ComponentLocomotion m_componentLocomotion;
        public ComponentVitalStats m_componentVitalStats;
        public ComponentLevel m_componentLevel;

        public const float DoubleTapTime = 0.3f;
        public const float BaseStaminaCost = 0.05f;
        public const float BaseFoodCost = 0.008f;
        public const float BaseHeat = 0.5f;
        public const float DashSpeedMultiplier = 1.25f;
        public const float DashFovMultiplier = 1.25f;
        public const float FovChangeSpeed = 0.5f;

        private bool m_isDashing;
        private bool m_lastIsDashing;
        private bool m_pressed;
        private bool m_released;
        private double m_lastForwardTime;
        private float m_dashFov = 1f;

        public bool IsDashing => m_isDashing;
        public float DashFov => m_dashFov;

        public event Func<ComponentDash, DashCheckResult> CanDashCheck;
        public event Func<ComponentDash, float, float> ModifyDashSpeed;
        public event Action<ComponentDash> DashStarted;
        public event Action<ComponentDash> DashStopped;
        public event Action<ComponentDash, float> DashUpdated;

        public UpdateOrder UpdateOrder => UpdateOrder.Default;

        public virtual void Update(float dt)
        {
            if (!CoreSettingsManager.ControlsSettings.DoubleTapDash)
            {
                StopDashing();
                return;
            }

            if (m_componentPlayer.ComponentInput.PlayerInput.Move.Z > 0f)
            {
                if (m_released)
                {
                    m_released = false;

                    if (Time.RealTime - m_lastForwardTime < DoubleTapTime)
                    {
                        m_isDashing = true;
                        m_lastForwardTime = 0.0;
                    }
                }

                m_pressed = true;
            }
            else
            {
                if (m_pressed)
                {
                    m_pressed = false;
                    m_released = true;
                    m_lastForwardTime = m_isDashing ? 0.0 : Time.RealTime;
                }

                m_isDashing = false;
            }

            if (m_isDashing)
            {
                GameMode gameMode = m_subsystemGameInfo.WorldSettings.GameMode;

                if (gameMode != GameMode.Creative && !CanDash(out string reason))
                {
                    m_componentPlayer.ComponentGui.DisplaySmallMessage(reason ?? "Cannot dash", Color.Yellow, false, true);
                    m_isDashing = false;
                }
                else
                {
                    if (m_lastIsDashing != m_isDashing)
                        m_componentPlayer.ComponentGui.DisplaySmallMessage("Dash!", Color.White, false, false);

                    float speed = DashSpeedMultiplier;

                    Func<ComponentDash, float, float> handlers = ModifyDashSpeed;

                    if (handlers != null)
                    {
                        foreach (Func<ComponentDash, float, float> handler in handlers.GetInvocationList())
                            speed = handler(this, speed);
                    }

                    m_componentLevel.m_speedFactors.Add(
                        new ComponentLevel.Factor
                        {
                            Name = "ZanJhat.Dash",
                            Value = speed,
                            Description = "Dashing",
                            FactorAdditionType = FactorAdditionType.Multiply
                        });

                    if (gameMode != GameMode.Creative)
                    {
                        float lastWalkOrder = m_componentLocomotion.LastWalkOrder?.Length() ?? 0f;

                        m_componentVitalStats.Stamina -= dt * BaseStaminaCost * lastWalkOrder;

                        float hungerFactor = m_componentLevel.HungerFactor;
                        m_componentVitalStats.Food -= hungerFactor * dt * BaseFoodCost * lastWalkOrder;

                        float dashHeat = dt * BaseHeat * lastWalkOrder;
                        m_componentVitalStats.m_targetTemperature += dashHeat;
                    }
                }
            }

            float target = m_isDashing ? DashFovMultiplier : 1f;
            m_dashFov = MathUtilsEx.MoveTowards(m_dashFov, target, FovChangeSpeed * dt);

            if (!m_lastIsDashing && m_isDashing)
                DashStarted?.Invoke(this);

            if (m_lastIsDashing && !m_isDashing)
                DashStopped?.Invoke(this);

            DashUpdated?.Invoke(this, dt);

            m_lastIsDashing = m_isDashing;
        }

        public virtual bool CanDash(out string reason)
        {
            reason = null;

            Func<ComponentDash, DashCheckResult> handlers = CanDashCheck;

            if (handlers != null)
            {
                foreach (Func<ComponentDash, DashCheckResult> handler in handlers.GetInvocationList())
                {
                    DashCheckResult result = handler(this);

                    if (!result.Allowed)
                    {
                        reason = result.Reason;
                        return false;
                    }
                }
            }

            if (m_componentVitalStats.Food < 0.2f)
                reason = "You can't dash when you're hungry";
            else if (m_componentVitalStats.Stamina < 0.33f)
                reason = "Cannot dash when exhausted";
            else if (m_componentPlayer.ComponentSickness.IsSick)
                reason = "You can't dash when you're sick";
            else if (m_componentPlayer.ComponentFlu.HasFlu)
                reason = "You can't dash when you have the flu";
            else if (m_componentPlayer.ComponentRider.Mount != null)
                reason = "You cannot dash while riding";
            else if (m_componentPlayer.ComponentBody.IsCrouching)
                reason = "You cannot dash while crouching";
            else if (m_componentPlayer.ComponentBody.ImmersionFactor > 0.33f && !m_componentPlayer.ComponentBody.StandingOnValue.HasValue)
                reason = "It is not possible to dash while in liquid";

            return reason == null;
        }

        public virtual void StopDashing()
        {
            m_isDashing = false;
            m_pressed = false;
            m_released = false;
            m_lastForwardTime = 0.0;
            m_dashFov = 1f;
        }

        public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
        {
            base.Load(valuesDictionary, idToEntityMap);
            m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(true);

            m_componentPlayer = Entity.FindComponent<ComponentPlayer>(true);
            m_componentLocomotion = Entity.FindComponent<ComponentLocomotion>(true);
            m_componentVitalStats = Entity.FindComponent<ComponentVitalStats>(true);
            m_componentLevel = Entity.FindComponent<ComponentLevel>(true);
        }

        public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
        {
            base.Save(valuesDictionary, entityToIdMap);
        }
    }
}
