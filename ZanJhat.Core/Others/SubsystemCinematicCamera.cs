using Engine;
using GameEntitySystem;
using System;
using Game;

namespace ZanJhat.Core
{
    public class SubsystemCinematicCamera : Subsystem, IUpdateable
    {
        private CinematicCamera m_orbitCamera;

        public static float OrbitRadius = 15f;
        public static float OrbitHeight = 8f;
        public static float OrbitSpeed = 1f;
        public static float TargetYOffset = 1.5f;

        public static float CaptureFPS = 60f;

        private float m_currentAngle = 0f;
        private float m_timeSinceLastCapture = 0f;

        public UpdateOrder UpdateOrder => UpdateOrder.Default;

        public void Update(float dt)
        {
            if (!CinematicRecorderManager.IsRecording)
            {
                return;
            }

            ComponentPlayer targetPlayer = CinematicRecorderManager.RecordingPlayer;

            if (targetPlayer == null)
            {
                CinematicRecorderManager.StopRecording(null);
                return;
            }

            Vector3 targetPosition = targetPlayer.ComponentBody.Position + new Vector3(0f, TargetYOffset, 0f);

            m_currentAngle += OrbitSpeed * dt;
            if (m_currentAngle > MathF.PI * 2f) m_currentAngle -= MathF.PI * 2f;

            float camX = targetPosition.X + MathF.Cos(m_currentAngle) * OrbitRadius;
            float camZ = targetPosition.Z + MathF.Sin(m_currentAngle) * OrbitRadius;
            float camY = targetPosition.Y + OrbitHeight;

            m_timeSinceLastCapture += dt;
            float captureInterval = 1f / CaptureFPS;

            if (m_timeSinceLastCapture >= captureInterval)
            {
                Vector3 cameraPosition = new Vector3(camX, camY, camZ);
                Vector3 cameraDirection = Vector3.Normalize(targetPosition - cameraPosition);

                if (m_orbitCamera == null)
                {
                    m_orbitCamera = new CinematicCamera(targetPlayer.GameWidget);
                }

                m_orbitCamera.SetupPerspectiveCamera(cameraPosition, cameraDirection, Vector3.UnitY);
                m_orbitCamera.PrepareForDrawing();

                // FIX 1: Bổ sung truyền targetPlayer vào để xử lý lỗi tàng hình
                CinematicRecorderManager.RecordFrame(m_orbitCamera, targetPlayer);

                // Reset cứng về 0 thay vì trừ dần để tránh lỗi dồn dập khung hình khi máy lag
                m_timeSinceLastCapture = 0f;
            }
        }
    }
}
