using System;
using Engine;
using Engine.Graphics;
using Game;

namespace ZanJhat.Core
{
    public class TechHudBackgroundWidget : Widget
    {
        public Vector2 m_position;
        public TechHudBackgroundRenderer2D TechHudBackgroundRenderer2D = new();
        public RenderTarget2D m_defaultRT = new RenderTarget2D(1920, 1080, 1, ColorFormat.Rgba8888, DepthFormat.Depth24Stencil8);

        private float m_scannerStartTime = 0f;
        private Vector2 m_scannerPos = Vector2.Zero;
        private float m_prevDt;

        public TechHudBackgroundWidget()
        {
            TechHudBackgroundBatch2D techHudBackgroundBatch2D = TechHudBackgroundRenderer2D.FlatBatch(1);
            Vector2 zero = Vector2.Zero;
            Vector2 corner = new Vector2(m_defaultRT.Width, m_defaultRT.Height);
            techHudBackgroundBatch2D.QueueQuad(zero, corner, 0f, Color.Transparent);
            techHudBackgroundBatch2D.TransformLines(base.GlobalTransform, techHudBackgroundBatch2D.TriangleVertices.Count);
            techHudBackgroundBatch2D.TransformTriangles(base.GlobalTransform, techHudBackgroundBatch2D.TriangleVertices.Count);
            RenderTarget2D renderTarget = Display.RenderTarget;
            Display.RenderTarget = m_defaultRT;
            techHudBackgroundBatch2D.Flush();
            Display.RenderTarget = renderTarget;
        }

        public override void MeasureOverride(Vector2 parentAvailableSize)
        {
            base.IsDrawRequired = true;
        }

        public override void Draw(DrawContext dc)
        {
            DrawImage(dc);
            DrawDetailedUiPanels(dc);
        }

        public void DrawImage(DrawContext dc)
        {
            TechHudBackgroundBatch2D techHudBackgroundBatch2D = TechHudBackgroundRenderer2D.FlatBatch(1);
            Vector2 zero2 = Vector2.Zero;
            Vector2 corner = base.GlobalBounds.Size();
            techHudBackgroundBatch2D.QueueQuad(zero2, corner, 0f, Color.Transparent);
            techHudBackgroundBatch2D.TransformLines(base.GlobalTransform, techHudBackgroundBatch2D.TriangleVertices.Count);
            techHudBackgroundBatch2D.TransformTriangles(base.GlobalTransform, techHudBackgroundBatch2D.TriangleVertices.Count);
            techHudBackgroundBatch2D.Flush();
        }

        // ---------- HỆ THỐNG VẼ UI CHI TIẾT (BLOOMED) ----------

        public void DrawDetailedUiPanels(DrawContext dc)
        {
            FlatBatch2D flatBatch = dc.PrimitivesRenderer2D.FlatBatch(1, DepthStencilState.None, null, BlendState.AlphaBlend);
            FontBatch2D fontBatch = dc.PrimitivesRenderer2D.FontBatch();

            // ĐÃ SỬA LỖI: Lấy cả Line và Triangle để không bị lệch khung
            int lineCountStart = flatBatch.LineVertices.Count;
            int triCountStart = flatBatch.TriangleVertices.Count;
            int fontTriCountStart = fontBatch.TriangleVertices.Count;

            float t = (float)Time.FrameStartTime;

            Color cyanMain = new Color(0f, 0.8f, 1f, 1f);
            Color cyanGlow = new Color(0f, 0.8f, 1f, 1f);
            Color orange = new Color(1f, 0.4f, 0f, 1f);

            float w = base.ActualSize.X;
            float h = base.ActualSize.Y;
            Vector2 center = new Vector2(w * 0.5f, h * 0.5f);

            float coreRadius = h * 0.2f;

            // Mạng lưới dây dẫn (Bloom Toàn Bộ)
            DrawCircuitPath(flatBatch, center + new Vector2(-coreRadius, -coreRadius * 0.5f), new Vector2(w * 0.1f, h * 0.1f), cyanMain, cyanGlow, t);
            DrawCircuitPath(flatBatch, center + new Vector2(-coreRadius * 0.8f, -coreRadius), new Vector2(w * 0.3f, h * 0.05f), cyanMain, cyanGlow, t + 2f);
            DrawCircuitPath(flatBatch, center + new Vector2(coreRadius, -coreRadius * 0.5f), new Vector2(w * 0.85f, h * 0.15f), cyanMain, cyanGlow, t + 5f);
            DrawCircuitPath(flatBatch, center + new Vector2(-coreRadius, 0f), new Vector2(0f, h * 0.5f), cyanMain, cyanGlow, t + 8f);
            DrawCircuitPath(flatBatch, center + new Vector2(coreRadius, coreRadius * 0.2f), new Vector2(w, h * 0.6f), cyanMain, cyanGlow, t + 12f);
            DrawCircuitPath(flatBatch, center + new Vector2(-coreRadius * 0.8f, coreRadius * 0.8f), new Vector2(w * 0.15f, h * 0.9f), cyanMain, cyanGlow, t + 15f);
            DrawCircuitPath(flatBatch, center + new Vector2(coreRadius * 0.5f, coreRadius), new Vector2(w * 0.8f, h * 0.95f), cyanMain, cyanGlow, t + 18f);

            // Khung Giao Diện
            Vector2 nBPos = center + new Vector2(-w * 0.4f, -h * 0.325f);
            DrawNeonBox(flatBatch, nBPos, new Vector2(w * 0.12f, h * 0.25f), cyanMain, cyanGlow, t);

            Vector2 eqPos = center + new Vector2(-w * 0.34f, h * 0.3f);
            DrawEqualizer(flatBatch, eqPos, new Vector2(w * 0.12f, h * 0.1f), cyanMain, cyanGlow, t);

            Vector2 mGPos = center + new Vector2(w * 0.25f, -h * 0.2f);
            DrawMatrixGrid(flatBatch, mGPos, new Vector2(w * 0.12f, h * 0.2f), cyanMain, cyanGlow, t);

            Vector2 progPos = center + new Vector2(w * 0.25f, h * 0.2f);
            DrawProgressBars(flatBatch, progPos, new Vector2(w * 0.12f, h * 0.1f), cyanMain, cyanGlow, t);

            string mainNumber = ((int)(t * 3) % 99).ToString("00");
            fontBatch.QueueText(mainNumber, center + new Vector2(-w * 0.15f, -h * 0.05f), 0f, cyanMain * (0.8f + 0.2f * MathF.Sin(t * 20f)), TextAnchor.Default, new Vector2(3.5f, 3.5f));
            fontBatch.QueueText("SECURE CONNECTION\nLATENCY: 12ms", new Vector2(w * 0.85f, h * 0.1f), 0f, cyanMain, TextAnchor.Default, new Vector2(0.8f, 0.8f));

            DrawTargetingScanner(flatBatch, t, w, h, cyanMain, cyanGlow);

            flatBatch.TransformLines(base.GlobalTransform, lineCountStart);
            flatBatch.TransformTriangles(base.GlobalTransform, triCountStart);
            fontBatch.TransformTriangles(base.GlobalTransform, fontTriCountStart);
        }

        // --- HỆ THỐNG VẼ ANIMATION QUÉT MỤC TIÊU ---

        private void DrawTargetingScanner(FlatBatch2D batch, float t, float w, float h, Color cMain, Color cGlow)
        {
            float cycleDuration = 4.5f;
            float dt = t - m_scannerStartTime;

            if (dt > cycleDuration || m_scannerStartTime == 0f)
            {
                m_scannerStartTime = t;
                dt = 0f;
                m_prevDt = 0f;
                float randX = (float)(MathUtils.Remainder(t * 123.45f, 1.0) * (w * 0.7f) + w * 0.15f);
                float randY = (float)(MathUtils.Remainder(t * 678.90f, 1.0) * (h * 0.7f) + h * 0.15f);
                m_scannerPos = new Vector2(randX, randY);
            }

            float fragRadius1 = 45f;
            float fragRadius2 = 55f; // Vòng 2 to hơn 1 chút

            // --- PHA 1: QUÉT & CHỚP GIẬT TÂM NGẮM (0.0s đến 2.0s) ---
            if (dt < 2.0f)
            {
                // Xoay 2 vòng không đồng bộ: 1 nhanh xuôi kim đồng hồ, 1 chậm ngược kim đồng hồ
                float rotAngle1 = t * 4f;
                float rotAngle2 = -t * 2.5f + MathF.PI / 4f;

                DrawArcFragments(batch, m_scannerPos, fragRadius1, rotAngle1, cMain, cGlow, 1.0f, 4f);
                DrawArcFragments(batch, m_scannerPos, fragRadius2, rotAngle2, cMain, cGlow, 0.7f, 2f); // Vòng 2 mỏng và mờ hơn

                float jitterSpeed = 25f;
                float step = MathF.Floor(t * jitterSpeed);
                float hashVis = MathF.Abs(MathF.Sin(step * 112.3f));

                if (hashVis > 0.3f)
                {
                    float jx = MathF.Sin(step * 12.3f) * (fragRadius1 * 1.5f);
                    float jy = MathF.Cos(step * 45.6f) * (fragRadius1 * 1.5f);
                    DrawCrosshair(batch, m_scannerPos + new Vector2(jx, jy), 20f, cMain, cGlow, 1.0f, 1.0f);
                }
            }
            // --- PHA 2: KHÓA MỤC TIÊU (2.0s đến 2.5s) ---
            else if (dt < 2.5f)
            {
                float localDt = (dt - 2.0f) / 0.5f;
                float alpha = 1.0f - localDt;

                // Hai vòng chậm dần và nới rộng ra rồi biến mất
                float rotAngle1 = t * 4f - (localDt * 2f);
                float rotAngle2 = -t * 2.5f + MathF.PI / 4f + (localDt * 1.5f);

                DrawArcFragments(batch, m_scannerPos, fragRadius1 + (localDt * 20f), rotAngle1, cMain, cGlow, alpha, 4f);
                DrawArcFragments(batch, m_scannerPos, fragRadius2 + (localDt * 25f), rotAngle2, cMain, cGlow, alpha * 0.7f, 2f);

                DrawCrosshair(batch, m_scannerPos, 20f, cMain, cGlow, 1.0f, 1.0f);
            }
            // --- PHA 3: TÂM NGẮM TO LÊN RỒI RÚT VỀ 0 (2.5s đến 3.2s) ---
            else if (dt < 3.2f)
            {
                float localDt = (dt - 2.5f) / 0.7f;
                float scale;
                float alpha = 1.0f;

                if (localDt < 0.4f)
                {
                    float progress = localDt / 0.4f;
                    scale = 1.0f + (progress * 2.0f);
                }
                else
                {
                    float progress = (localDt - 0.4f) / 0.6f;
                    scale = 3.0f * (1.0f - progress);
                    alpha = 1.0f - progress;
                }

                DrawCrosshair(batch, m_scannerPos, 20f, cMain, cGlow, scale, alpha);
            }
            // --- PHA 4: VỤ NỔ NĂNG LƯỢNG KẾT THÚC (3.2s đến 3.8s) ---
            else if (dt < 3.8f)
            {
                float expDt = (dt - 3.2f) / 0.6f;

                float expRadius = 10f + (100f * MathF.Pow(expDt, 0.5f));
                float expAlpha = 1.0f - expDt;

                // THUẬT TOÁN BLOOM CHÂN THỰC: 
                // Xếp chồng 12 lớp bát giác (Octagon) siêu mỏng lên nhau để tạo Gradient sương mù ánh sáng
                int layers = 12;
                for (int i = 1; i <= layers; i++)
                {
                    float f = (float)i / layers; // 0.08 -> 1.0
                    float r = expRadius * (1.1f - f); // Bán kính giảm dần

                    // Hàm mũ giúp ánh sáng đậm đặc ở lõi và mờ tản dần ở rìa ngoài
                    float a = MathF.Pow(f, 2.5f) * expAlpha * 0.15f;

                    // Lõi màu Trắng chói, tản ra ngoài màu Xanh lơ (Cyan)
                    Color col = Color.Lerp(cGlow, Color.SkyBlue, f);

                    // 1. Hình vuông mờ (Trục thẳng)
                    batch.QueueQuad(m_scannerPos - new Vector2(r), m_scannerPos + new Vector2(r), 0f, col * a);

                    // 2. Hình thoi mờ (Trục xoay 45 độ) - Ráp lại tạo thành Bát Giác mô phỏng Vòng tròn
                    float rRot = r * 1.414f;
                    Vector2 top = m_scannerPos + new Vector2(0, -rRot);
                    Vector2 right = m_scannerPos + new Vector2(rRot, 0);
                    Vector2 bottom = m_scannerPos + new Vector2(0, rRot);
                    Vector2 left = m_scannerPos + new Vector2(-rRot, 0);
                    batch.QueueQuad(top, right, bottom, left, 0f, col * a);
                }

                // --- LENS FLARE (Tia chớp chữ thập cũng được làm mềm) ---
                float flareLen = expRadius * 4f;
                float flareThick = 2f + 15f * expAlpha;

                // Chồng 6 lớp để tia sáng mịn, mất đi vẻ góc cạnh "nhựa"
                for (int i = 1; i <= 6; i++)
                {
                    float f = (float)i / 6f;
                    float fl = flareLen * (1.1f - f);
                    float ft = flareThick * (1.1f - f);
                    float a = MathF.Pow(f, 2.0f) * expAlpha * 0.2f;
                    Color col = Color.Lerp(cGlow, Color.White, f);

                    // Trục Ngang
                    batch.QueueQuad(m_scannerPos - new Vector2(fl, ft), m_scannerPos + new Vector2(fl, ft), 0f, col * a);
                    // Trục Dọc
                    batch.QueueQuad(m_scannerPos - new Vector2(ft, fl), m_scannerPos + new Vector2(ft, fl), 0f, col * a);
                }

                // Sóng Xung Kích (Shockwave Ring mảnh chạy nhanh)
                if (expDt < 0.4f)
                {
                    float swRadius = expRadius * 1.5f;
                    float swAlpha = (1f - (expDt / 0.4f)) * expAlpha;

                    int segments = 16;
                    for (int i = 0; i < segments; i++)
                    {
                        float a1 = (i / (float)segments) * 2f * MathF.PI;
                        float a2 = ((i + 1) / (float)segments) * 2f * MathF.PI;
                        Vector2 p1 = m_scannerPos + new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * swRadius;
                        Vector2 p2 = m_scannerPos + new Vector2(MathF.Cos(a2), MathF.Sin(a2)) * swRadius;
                        batch.QueueLine(p1, p2, 0f, Color.White * swAlpha * 0.8f);
                    }
                }
            }

            if (dt >= 2.3f && m_prevDt < 2.3f)
                AudioManager.PlaySound("Audio/EnergyBlast", 1f, 0f, 0f);

            m_prevDt = dt;
        }

        // Hàm hỗ trợ: Vẽ các mảnh viền đứt đoạn quay tròn CÓ HIỆU ỨNG BLOOM
        private void DrawArcFragments(FlatBatch2D batch, Vector2 center, float radius, float angleOffset, Color cMain, Color cGlow, float alpha, float thickness)
        {
            Color drawCol = cMain * alpha;
            // Ánh sáng tỏa ra (Bloom)
            Color glowCol1 = cGlow * (alpha * 0.3f);
            Color glowCol2 = cGlow * (alpha * 0.1f);

            int numFragments = 4;
            float fragmentSweep = MathF.PI / 4f;

            for (int f = 0; f < numFragments; f++)
            {
                float startAngle = angleOffset + f * (MathF.PI / 2f);

                int segments = 8;
                float angleStep = fragmentSweep / segments;

                for (int i = 0; i < segments; i++)
                {
                    float a1 = startAngle + i * angleStep;
                    float a2 = startAngle + (i + 1) * angleStep;

                    // Lớp 1: Lõi cứng (Core)
                    DrawArcSegment(batch, center, a1, a2, radius, radius - thickness, drawCol);
                    // Lớp 2: Bloom tỏa nhẹ
                    DrawArcSegment(batch, center, a1, a2, radius + 2f, radius - thickness - 2f, glowCol1);
                    // Lớp 3: Bloom tỏa xa
                    DrawArcSegment(batch, center, a1, a2, radius + 5f, radius - thickness - 5f, glowCol2);
                }
            }
        }

        // Hàm vẽ từng đoạn hình quạt nhỏ để ráp thành vòng cung
        private void DrawArcSegment(FlatBatch2D batch, Vector2 center, float a1, float a2, float rOut, float rIn, Color col)
        {
            Vector2 out1 = center + new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * rOut;
            Vector2 out2 = center + new Vector2(MathF.Cos(a2), MathF.Sin(a2)) * rOut;
            Vector2 in2 = center + new Vector2(MathF.Cos(a2), MathF.Sin(a2)) * rIn;
            Vector2 in1 = center + new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * rIn;

            batch.QueueQuad(out1, out2, in2, in1, 0f, col);
        }

        // Hàm hỗ trợ: Vẽ tâm ngắm chuẩn Sci-Fi
        private void DrawCrosshair(FlatBatch2D batch, Vector2 pos, float size, Color cMain, Color cGlow, float scale, float alpha)
        {
            float s = size * scale;
            float t = 3f * scale; // Độ dày
            float gap = s * 0.4f; // Khoảng trống ở giữa
            Color col = cMain * alpha;
            Color glow = cGlow * alpha * 0.3f;

            // Dấu chấm nhỏ xíu ở giữa
            batch.QueueQuad(pos - new Vector2(t), pos + new Vector2(t), 0f, col);

            // 4 đường ngắm (Trái, Phải, Trên, Dưới) có thêm Bloom nhẹ
            // Trái
            batch.QueueQuad(pos + new Vector2(-s, -t / 2), pos + new Vector2(-gap, t / 2), 0f, col);
            batch.QueueQuad(pos + new Vector2(-s, -t), pos + new Vector2(-gap, t), 0f, glow);
            // Phải
            batch.QueueQuad(pos + new Vector2(gap, -t / 2), pos + new Vector2(s, t / 2), 0f, col);
            batch.QueueQuad(pos + new Vector2(gap, -t), pos + new Vector2(s, t), 0f, glow);
            // Trên
            batch.QueueQuad(pos + new Vector2(-t / 2, -s), pos + new Vector2(t / 2, -gap), 0f, col);
            batch.QueueQuad(pos + new Vector2(-t, -s), pos + new Vector2(t, -gap), 0f, glow);
            // Dưới
            batch.QueueQuad(pos + new Vector2(-t / 2, gap), pos + new Vector2(t / 2, s), 0f, col);
            batch.QueueQuad(pos + new Vector2(-t, gap), pos + new Vector2(t, s), 0f, glow);
        }

        // --- CÁC HÀM TIỆN ÍCH ĐỂ TẠO BLOOM KHỐI ĐẶC (QUAD) ---

        private void DrawBloomedWire(FlatBatch2D batch, Vector2 start, Vector2 end, Color cMain, Color cGlow)
        {
            Vector2 min = new Vector2(MathF.Min(start.X, end.X), MathF.Min(start.Y, end.Y));
            Vector2 max = new Vector2(MathF.Max(start.X, end.X), MathF.Max(start.Y, end.Y));

            if (MathF.Abs(max.X - min.X) < 0.1f) { min.X -= 0.5f; max.X += 0.5f; }
            if (MathF.Abs(max.Y - min.Y) < 0.1f) { min.Y -= 0.5f; max.Y += 0.5f; }

            // ĐÃ SỬA: Dùng QueueQuad để tạo khối sương mù đặc (Filled), QueueRectangle chỉ tạo viền rỗng
            batch.QueueQuad(min - new Vector2(6f), max + new Vector2(6f), 0f, cGlow * 0.05f);
            batch.QueueQuad(min - new Vector2(3f), max + new Vector2(3f), 0f, cGlow * 0.15f);
            batch.QueueQuad(min - new Vector2(1f), max + new Vector2(1f), 0f, cGlow * 0.4f);
            batch.QueueQuad(min, max, 0f, cMain);
        }

        private void DrawCircuitPath(FlatBatch2D batch, Vector2 start, Vector2 end, Color cMain, Color cGlow, float t)
        {
            Vector2 mid1 = new Vector2(start.X + (end.X - start.X) * 0.5f, start.Y);
            Vector2 mid2 = new Vector2(mid1.X, end.Y);

            DrawBloomedWire(batch, start, mid1, cMain, cGlow);
            DrawBloomedWire(batch, mid1, mid2, cMain, cGlow);
            DrawBloomedWire(batch, mid2, end, cMain, cGlow);

            batch.QueueQuad(start - new Vector2(2f), start + new Vector2(2f), 0f, cMain);
            batch.QueueQuad(mid1 - new Vector2(1.5f), mid1 + new Vector2(1.5f), 0f, cMain);
            batch.QueueQuad(mid2 - new Vector2(1.5f), mid2 + new Vector2(1.5f), 0f, cMain);
            batch.QueueQuad(end - new Vector2(4f), end + new Vector2(4f), 0f, cMain);

            float len1 = MathF.Abs(mid1.X - start.X);
            float len2 = MathF.Abs(mid2.Y - mid1.Y);
            float len3 = MathF.Abs(end.X - mid2.X);
            float totalLength = len1 + len2 + len3;

            float pulse = (t * 220f) % totalLength;
            float streakLen = 60f;

            for (float s = 0; s <= streakLen; s += 0.5f)
            {
                float pd = pulse - s;
                if (pd < 0f || pd > totalLength) continue;

                Vector2 pPos;
                if (pd < len1) pPos = start + new Vector2(MathF.Sign(mid1.X - start.X) * pd, 0);
                else if (pd < len1 + len2) pPos = mid1 + new Vector2(0, MathF.Sign(mid2.Y - mid1.Y) * (pd - len1));
                else pPos = mid2 + new Vector2(MathF.Sign(end.X - mid2.X) * (pd - len1 - len2), 0);

                float normalizedS = s / streakLen;
                float intensity = MathF.Pow(1f - normalizedS, 2.5f);
                Color pCol = Color.Lerp(new Color(0f, 0.6f, 1f, 0f), Color.White, intensity);

                float rad = 1f + (3f * intensity);
                batch.QueueQuad(pPos - new Vector2(rad * 2f), pPos + new Vector2(rad * 2f), 0f, pCol * intensity * 0.05f);
                batch.QueueQuad(pPos - new Vector2(rad), pPos + new Vector2(rad), 0f, pCol * intensity * 0.2f);
                batch.QueueQuad(pPos - new Vector2(rad * 0.5f), pPos + new Vector2(rad * 0.5f), 0f, Color.White * intensity);
            }

            // TÍNH NĂNG MỚI: HIỆU ỨNG NỔ NĂNG LƯỢNG (BLOOM EXPLOSION) KHI ĐẾN ĐÍCH
            // Tính toán khoảng cách kể từ khi tia sáng vừa vòng lại đầu tiên
            float explosionDist = 80f; // Độ dài tương đương thời gian nổ tan dần
            if (pulse < explosionDist)
            {
                // Intensity giảm dần từ 1.0 xuống 0.0
                float expIntensity = 1f - (pulse / explosionDist);
                // Bán kính nở rộng dần ra
                float expRadius = 4f + (25f * (1f - expIntensity));

                Color expColor = Color.Lerp(new Color(0f, 0.6f, 1f, 0f), Color.White, expIntensity);

                // Vẽ 2 khối toả mờ dạng sóng xung kích (Shockwave)
                batch.QueueQuad(end - new Vector2(expRadius), end + new Vector2(expRadius), 0f, expColor * expIntensity * 0.3f);
                batch.QueueQuad(end - new Vector2(expRadius * 0.5f), end + new Vector2(expRadius * 0.5f), 0f, Color.White * expIntensity * 0.8f);
            }
        }

        private void DrawNeonBox(FlatBatch2D batch, Vector2 pos, Vector2 size, Color cMain, Color cGlow, float t)
        {
            DrawBloomedWire(batch, pos, new Vector2(pos.X + size.X, pos.Y), cMain, cGlow);
            DrawBloomedWire(batch, pos, new Vector2(pos.X, pos.Y + size.Y), cMain, cGlow);
            DrawBloomedWire(batch, pos + size, new Vector2(pos.X, pos.Y + size.Y), cMain, cGlow);
            DrawBloomedWire(batch, pos + size, new Vector2(pos.X + size.X, pos.Y), cMain, cGlow);

            int blocks = 5;
            float blockW = size.X / blocks;
            for (int i = 0; i < blocks; i++)
            {
                if (MathF.Sin(t * 5f + i) > 0)
                {
                    batch.QueueQuad(pos + new Vector2(i * blockW + 5f, 5f), pos + new Vector2((i + 1) * blockW - 5f, size.Y - 5f), 0f, cMain * 0.5f);
                }
            }
        }

        private void DrawEqualizer(FlatBatch2D batch, Vector2 pos, Vector2 size, Color cMain, Color cGlow, float t)
        {
            int bars = 15;
            float barW = size.X / bars;
            for (int i = 0; i < bars; i++)
            {
                float heightMod = MathF.Abs(MathF.Sin(i * 0.5f + t * 5f)) * 0.8f + 0.2f;
                float h = size.Y * heightMod;
                Vector2 p1 = pos + new Vector2(i * barW + 2f, size.Y);
                Vector2 p2 = pos + new Vector2((i + 1) * barW - 2f, size.Y - h);

                batch.QueueQuad(p1, p2, 0f, cMain * 0.8f);
                batch.QueueQuad(p1 - new Vector2(3f), p2 + new Vector2(3f), 0f, cGlow * 0.25f);
            }
        }

        private void DrawMatrixGrid(FlatBatch2D batch, Vector2 pos, Vector2 size, Color cMain, Color cGlow, float t)
        {
            int cols = 4;
            int rows = 4;
            float cellW = size.X / cols;
            float cellH = size.Y / rows;

            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    Vector2 cellPos = pos + new Vector2(x * cellW, y * cellH);
                    batch.QueueRectangle(cellPos, cellPos + new Vector2(cellW - 2f, cellH - 2f), 0f, cGlow * 0.5f); // Viền lưới tĩnh

                    if (MathF.Sin(x * 13.0f + y * 27.0f + t * 10.0f) > 0.8f)
                    {
                        // Khối đặc chớp sáng
                        batch.QueueQuad(cellPos - new Vector2(2f), cellPos + new Vector2(cellW, cellH), 0f, cGlow * 0.4f);
                        batch.QueueQuad(cellPos, cellPos + new Vector2(cellW - 2f, cellH - 2f), 0f, cMain);
                    }
                }
            }
        }

        private void DrawProgressBars(FlatBatch2D batch, Vector2 pos, Vector2 size, Color cMain, Color cGlow, float t)
        {
            int bars = 3;
            float barH = size.Y / bars;
            for (int i = 0; i < bars; i++)
            {
                Vector2 barPos = pos + new Vector2(0f, i * barH + 5f);
                batch.QueueRectangle(barPos, barPos + new Vector2(size.X, barH - 10f), 0f, cGlow); // Khung trượt

                float fill = (MathF.Sin(t * 2f + i) + 1f) * 0.5f * size.X;
                batch.QueueQuad(barPos, barPos + new Vector2(fill, barH - 10f), 0f, cMain);
                batch.QueueQuad(barPos - new Vector2(0f, 2f), barPos + new Vector2(fill, barH - 8f), 0f, cGlow * 0.5f); // Bloom cho thanh trượt
            }
        }
    }
}
