using Engine;
using Game;

namespace ZanJhat.Core
{
    public class CinematicCamera : BasePerspectiveCamera
    {
        public override bool UsesMovementControls => false;
        public override bool IsEntityControlEnabled => false;

        public CinematicCamera(GameWidget gameWidget) : base(gameWidget)
        {
        }

        public override void Activate(Camera previousCamera)
        {
            // Để trống để không bị copy góc nhìn của người chơi
        }

        public override void Update(float dt)
        {
            // Để trống
        }

        // FIX LỆCH TÂM: Ghi đè trực tiếp ProjectionMatrix
        // Bỏ qua hoàn toàn hàm tính toán ViewWidget UI của Engine
        public override Matrix ProjectionMatrix
        {
            get
            {
                if (!m_projectionMatrix.HasValue)
                {
                    // Lấy góc FOV từ cài đặt người chơi
                    float viewAngle = 80f * SettingsManager.ViewAngle;

                    // Khóa cứng tỉ lệ khung hình theo kích thước Video (Ví dụ 1920x1080)
                    float videoAspectRatio = 1920f / 1080f;

                    // Tạo một ma trận hoàn toàn sạch, tâm chuẩn xác 100% ở giữa màn hình
                    m_projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathUtils.DegToRad(viewAngle), videoAspectRatio, 0.1f, 2048f);
                }
                return m_projectionMatrix.Value;
            }
        }

        // KHÓA KÍCH THƯỚC: Ép Viewport khớp với RenderTarget
        public override Vector2 ViewportSize
        {
            get
            {
                return new Vector2(1920f, 1080f);
            }
        }
    }
}
