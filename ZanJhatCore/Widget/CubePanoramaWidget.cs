using Engine;
using Engine.Graphics;
using System;
using Game;

namespace ZanJhat.Core
{
    public class CubePanoramaWidget : Widget
    {
        // Mảng chứa 6 mặt của hình lập phương
        public Texture2D[] m_textures = new Texture2D[6];

        public float m_timeOffset;

        private PrimitivesRenderer3D m_primitivesRenderer3D = new PrimitivesRenderer3D();

        public CubePanoramaWidget()
        {
            m_timeOffset = new Game.Random().Float(0f, 1000f);

            // Hãy đảm bảo chúng ghép nối liền mạch với nhau ở các viền
            m_textures[0] = ContentManager.Get<Texture2D>("Textures/CubePanorama/PanoramaFront");
            m_textures[1] = ContentManager.Get<Texture2D>("Textures/CubePanorama/PanoramaBack");
            m_textures[2] = ContentManager.Get<Texture2D>("Textures/CubePanorama/PanoramaLeft");
            m_textures[3] = ContentManager.Get<Texture2D>("Textures/CubePanorama/PanoramaRight");
            m_textures[4] = ContentManager.Get<Texture2D>("Textures/CubePanorama/PanoramaTop");
            m_textures[5] = ContentManager.Get<Texture2D>("Textures/CubePanorama/PanoramaBottom");
        }

        public override void MeasureOverride(Vector2 parentAvailableSize)
        {
            IsDrawRequired = true;
        }

        public override void Draw(DrawContext dc)
        {
            DrawCube(dc);
        }

        public virtual void DrawCube(DrawContext dc)
        {
            // 1. TÍNH TOÁN MA TRẬN CAMERA (Góc nhìn 85 độ giống Minecraft)
            float aspect = ActualSize.X / ActualSize.Y;
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathUtils.DegToRad(85f), aspect, 0.1f, 10f);

            // 2. CHUYỂN ĐỘNG CAMERA (Quay ngẫu nhiên và mượt mà)
            float time = (float)Time.FrameStartTime + m_timeOffset;
            float yaw = time * 0.03f; // Tốc độ xoay ngang
            float pitch = MathF.Sin(time * 0.015f) * 0.2f; // Lắc lư lên xuống nhẹ

            Matrix view = Matrix.CreateRotationX(pitch) * Matrix.CreateRotationY(yaw);
            Matrix viewProjection = view * projection;

            // 3. ĐỊNH NGHĨA 8 GÓC CỦA HÌNH LẬP PHƯƠNG
            float s = 1f; // Kích thước (Không quan trọng lắm vì camera ở đúng trọng tâm)
            Vector3 p000 = new Vector3(-s, -s, -s); // Trái - Dưới - Trước
            Vector3 p100 = new Vector3(s, -s, -s); // Phải - Dưới - Trước
            Vector3 p110 = new Vector3(s, s, -s); // Phải - Trên - Trước
            Vector3 p010 = new Vector3(-s, s, -s); // Trái - Trên - Trước

            Vector3 p001 = new Vector3(-s, -s, s); // Trái - Dưới - Sau
            Vector3 p101 = new Vector3(s, -s, s); // Phải - Dưới - Sau
            Vector3 p111 = new Vector3(s, s, s); // Phải - Trên - Sau
            Vector3 p011 = new Vector3(-s, s, s); // Trái - Trên - Sau

            // 4. TỌA ĐỘ UV CHUẨN CỦA 1 BỨC ẢNH
            Vector2 uvTL = new Vector2(0, 0); // Top-Left
            Vector2 uvTR = new Vector2(1, 0); // Top-Right
            Vector2 uvBR = new Vector2(1, 1); // Bottom-Right
            Vector2 uvBL = new Vector2(0, 1); // Bottom-Left

            Color color = Color.White;

            // Dùng DepthStencilState.None để bầu trời luôn bị vẽ chìm dưới cùng (Background)
            // Dùng RasterizerState.CullNone để vẽ thấy được mặt bên trong của hình lập phương

            // MẶT TRƯỚC (Front: hướng -Z)
            if (m_textures[0] != null)
            {
                var batch = m_primitivesRenderer3D.TexturedBatch(m_textures[0], false, 0, DepthStencilState.None, RasterizerState.CullNone, BlendState.Opaque, SamplerState.LinearClamp);
                batch.QueueQuad(p010, p110, p100, p000, uvTL, uvTR, uvBR, uvBL, color);
            }

            // MẶT SAU (Back: hướng +Z)
            if (m_textures[1] != null)
            {
                var batch = m_primitivesRenderer3D.TexturedBatch(m_textures[1], false, 0, DepthStencilState.None, RasterizerState.CullNone, BlendState.Opaque, SamplerState.LinearClamp);
                batch.QueueQuad(p111, p011, p001, p101, uvTL, uvTR, uvBR, uvBL, color);
            }

            // MẶT TRÁI (Left: hướng -X)
            if (m_textures[2] != null)
            {
                var batch = m_primitivesRenderer3D.TexturedBatch(m_textures[2], false, 0, DepthStencilState.None, RasterizerState.CullNone, BlendState.Opaque, SamplerState.LinearClamp);
                batch.QueueQuad(p011, p010, p000, p001, uvTL, uvTR, uvBR, uvBL, color);
            }

            // MẶT PHẢI (Right: hướng +X)
            if (m_textures[3] != null)
            {
                var batch = m_primitivesRenderer3D.TexturedBatch(m_textures[3], false, 0, DepthStencilState.None, RasterizerState.CullNone, BlendState.Opaque, SamplerState.LinearClamp);
                batch.QueueQuad(p110, p111, p101, p100, uvTL, uvTR, uvBR, uvBL, color);
            }

            // MẶT TRÊN (Top: hướng +Y)
            if (m_textures[4] != null)
            {
                var batch = m_primitivesRenderer3D.TexturedBatch(m_textures[4], false, 0, DepthStencilState.None, RasterizerState.CullNone, BlendState.Opaque, SamplerState.LinearClamp);
                // Lưu ý: Nếu mặt trên bị xoay ngược, bạn cần xoay lại tọa độ UV ở dòng này
                batch.QueueQuad(p011, p111, p110, p010, uvTL, uvTR, uvBR, uvBL, color);
            }

            // MẶT DƯỚI (Bottom: hướng -Y)
            if (m_textures[5] != null)
            {
                var batch = m_primitivesRenderer3D.TexturedBatch(m_textures[5], false, 0, DepthStencilState.None, RasterizerState.CullNone, BlendState.Opaque, SamplerState.LinearClamp);
                batch.QueueQuad(p000, p100, p101, p001, uvTL, uvTR, uvBR, uvBL, color);
            }

            // 5. RENDER LÊN MÀN HÌNH
            m_primitivesRenderer3D.Flush(viewProjection);
        }
    }
}
