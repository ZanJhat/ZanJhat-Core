using Engine;
using Engine.Graphics;
using System;
using Game;

namespace ZanJhat.Core
{
    public class ValueFillWidget : Widget
    {
        public float m_value;
        public LayoutDirection m_layoutDirection;
        public float m_flashCount;
        public Vector2 m_barSize;

        public float Value
        {
            get => m_value;
            set => m_value = MathUtils.Saturate(value);
        }

        public LayoutDirection LayoutDirection
        {
            get => m_layoutDirection;
            set => m_layoutDirection = value;
        }

        public Vector2 BarSize
        {
            get => m_barSize;
            set => m_barSize = value;
        }

        public Subtexture Subtexture { get; set; }

        public Color Color { get; set; }

        public bool TextureLinearFilter { get; set; }

        public bool FlipDirection { get; set; }

        public ValueFillWidget()
        {
            IsHitTestVisible = false;
            Color = Color.White;
            TextureLinearFilter = true;
        }

        public void Flash(int count)
        {
            m_flashCount = MathUtils.Max(m_flashCount, (float)count);
        }

        public override void Draw(DrawContext dc)
        {
            // Bỏ qua nếu không có gì để vẽ
            if (m_value <= 0f)
            {
                m_flashCount = MathUtils.Max(m_flashCount - 4f * Time.FrameDuration, 0f);
                return;
            }

            // Tính toán hiệu ứng flash
            float flashScalar = m_flashCount > 0f ? 1f - MathUtils.Abs(MathUtils.Sin(m_flashCount * (float)MathUtils.PI)) : 1f;
            Color finalLitColor = Color * flashScalar * GlobalColorTransform;

            // --- BẮT ĐẦU XỬ LÝ KÍCH THƯỚC VÀ CLAMP ---
            Vector2 drawSize = ActualSize;

            if (ParentWidget != null)
            {
                // Tính không gian tối đa cho phép bên trong Parent (trừ đi margin)
                // Dùng MathUtils.Max với 0f để tránh lỗi số âm nếu Margin vô tình bị set quá to
                float maxAvailableX = MathUtils.Max(ParentWidget.ActualSize.X - MarginLeft - MarginRight, 0f);
                float maxAvailableY = MathUtils.Max(ParentWidget.ActualSize.Y - MarginTop - MarginBottom, 0f);

                // Trục X
                if (HorizontalAlignment == WidgetAlignment.Stretch)
                {
                    drawSize.X = maxAvailableX;
                }
                else
                {
                    // Nếu không Stretch, dùng BarSize nhưng Clamp trong giới hạn an toàn
                    drawSize.X = MathUtils.Clamp(BarSize.X, 0f, maxAvailableX);
                }

                // Trục Y
                if (VerticalAlignment == WidgetAlignment.Stretch)
                {
                    drawSize.Y = maxAvailableY;
                }
                else
                {
                    drawSize.Y = MathUtils.Clamp(BarSize.Y, 0f, maxAvailableY);
                }
            }
            else
            {
                // Fallback an toàn phòng khi Parent = null
                if (drawSize.X <= 0f)
                    drawSize.X = MathUtils.Max(BarSize.X, 0f);
                if (drawSize.Y <= 0f)
                    drawSize.Y = MathUtils.Max(BarSize.Y, 0f);
            }

            // --- Tính toán vị trí phần được lấp đầy ---
            Vector2 p1 = Vector2.Zero;
            Vector2 p2 = drawSize;
            float fill = m_value;

            if (m_layoutDirection == LayoutDirection.Horizontal)
            {
                if (FlipDirection) // Lấp đầy từ phải sang trái
                    p1.X = drawSize.X * (1f - fill);
                else // Lấp đầy từ trái sang phải (mặc định)
                    p2.X = drawSize.X * fill;
            }
            else // LayoutDirection.Vertical
            {
                if (FlipDirection) // Lấp đầy từ dưới lên trên
                    p1.Y = drawSize.Y * (1f - fill);
                else // Lấp đầy từ trên xuống dưới (mặc định)
                    p2.Y = drawSize.Y * fill;
            }

            // --- Quyết định vẽ Texture hay Flat ---
            if (Subtexture != null)
            {
                // Vẽ bằng Texture
                Vector2 tc1 = Subtexture.TopLeft;
                Vector2 tc2 = Subtexture.BottomRight;

                // Tính toán UV Map
                if (m_layoutDirection == LayoutDirection.Horizontal)
                {
                    if (FlipDirection) tc1.X = MathUtils.Lerp(Subtexture.TopLeft.X, Subtexture.BottomRight.X, 1f - fill);
                    else tc2.X = MathUtils.Lerp(Subtexture.TopLeft.X, Subtexture.BottomRight.X, fill);
                }
                else
                {
                    if (FlipDirection) tc1.Y = MathUtils.Lerp(Subtexture.TopLeft.Y, Subtexture.BottomRight.Y, 1f - fill);
                    else tc2.Y = MathUtils.Lerp(Subtexture.TopLeft.Y, Subtexture.BottomRight.Y, fill);
                }

                TexturedBatch2D litBatch = dc.PrimitivesRenderer2D.TexturedBatch(
                    Subtexture.Texture,
                    false, 0, DepthStencilState.None, null, null,
                    TextureLinearFilter ? SamplerState.LinearClamp : SamplerState.PointClamp
                );

                int litVertStart = litBatch.TriangleVertices.Count;
                litBatch.QueueQuad(p1, p2, 0f, tc1, tc2, finalLitColor);
                litBatch.TransformTriangles(GlobalTransform, litVertStart);
            }
            else
            {
                // Vẽ bằng Flat (Màu trơn) khi Subtexture bị null
                FlatBatch2D flatBatch = dc.PrimitivesRenderer2D.FlatBatch(0, DepthStencilState.None);

                int flatVertStart = flatBatch.TriangleVertices.Count;
                flatBatch.QueueQuad(p1, p2, 0f, finalLitColor);
                flatBatch.TransformTriangles(GlobalTransform, flatVertStart);
            }

            // Cập nhật đếm flash
            m_flashCount = MathUtils.Max(m_flashCount - 4f * Time.FrameDuration, 0f);
        }

        public override void MeasureOverride(Vector2 parentAvailableSize)
        {
            IsDrawRequired = true;
            DesiredSize = BarSize;
        }
    }
}
