using Engine;
using Engine.Graphics;
using Game;

namespace ZanJhat.Core
{
    public class RainbowRectangleWidget : Widget
    {
        public Subtexture m_subtexture;

        public bool m_textureWrap;

        public bool m_textureLinearFilter;

        public bool m_textureAnisotropicFilter;

        public bool m_depthWriteEnabled;

        public Vector2 Size { get; set; }

        public float Depth { get; set; }

        public bool DepthWriteEnabled
        {
            get => m_depthWriteEnabled;
            set => m_depthWriteEnabled = value;
        }

        public Subtexture Subtexture
        {
            get => m_subtexture;
            set => m_subtexture = value;
        }

        public bool TextureWrap
        {
            get => m_textureWrap;
            set => m_textureWrap = value;
        }

        public bool TextureLinearFilter
        {
            get => m_textureLinearFilter;
            set => m_textureLinearFilter = value;
        }

        public bool TextureAnisotropicFilter
        {
            get => m_textureAnisotropicFilter;
            set => m_textureAnisotropicFilter = value;
        }

        public bool FlipHorizontal { get; set; }

        public bool FlipVertical
        {
            get;
            set;
        }

        public Color FillColor { get; set; }

        public Color OutlineColor { get; set; }

        // == Rainbow ==

        public bool OutlineRainbowColor { get; set; }

        public float RainbowDensity { get; set; } // càng lớn màu càng ngắn

        public bool ProportionalRainbow { get; set; }

        public float HueSpeed { get; set; }

        // ====

        public float OutlineThickness { get; set; }

        public Vector2 Texcoord1 { get; set; }

        public Vector2 Texcoord2 { get; set; }

        public BlendState BlendState { get; set; } = BlendState.AlphaBlend;

        public RainbowRectangleWidget()
        {
            Size = new Vector2(float.PositiveInfinity);
            TextureLinearFilter = true;
            FillColor = Color.Black;
            OutlineColor = Color.White;

            OutlineRainbowColor = true;
            RainbowDensity = 2f;
            ProportionalRainbow = true;
            HueSpeed = 0.35f;

            OutlineThickness = 1f;
            IsHitTestVisible = false;
            Texcoord1 = Vector2.Zero;
            Texcoord2 = Vector2.One;
        }

        public override void Draw(DrawContext dc)
        {
            if (FillColor.A == 0
                && (OutlineColor.A == 0 || OutlineThickness <= 0f))
            {
                return;
            }
            DepthStencilState depthStencilState = DepthWriteEnabled ? DepthStencilState.DepthWrite : DepthStencilState.None;
            Matrix m = GlobalTransform;
            Vector2 v = Vector2.Zero;
            Vector2 v2 = new(ActualSize.X, 0f);
            Vector2 v3 = ActualSize;
            Vector2 v4 = new(0f, ActualSize.Y);
            Vector2.Transform(ref v, ref m, out Vector2 result);
            Vector2.Transform(ref v2, ref m, out Vector2 result2);
            Vector2.Transform(ref v3, ref m, out Vector2 result3);
            Vector2.Transform(ref v4, ref m, out Vector2 result4);
            Color color = FillColor * GlobalColorTransform;
            if (color.A != 0)
            {
                if (Subtexture != null)
                {
                    SamplerState samplerState;
                    if (TextureAnisotropicFilter)
                    {
                        samplerState = TextureWrap ? SamplerState.AnisotropicWrap : SamplerState.AnisotropicClamp;
                    }
                    else
                    {
                        samplerState = !TextureWrap ? TextureLinearFilter ? SamplerState.LinearClamp : SamplerState.PointClamp :
                            TextureLinearFilter ? SamplerState.LinearWrap : SamplerState.PointWrap;
                    }
                    TexturedBatch2D texturedBatch2D = dc.PrimitivesRenderer2D.TexturedBatch(
                        Subtexture.Texture,
                        true,
                        0,
                        depthStencilState,
                        null,
                        BlendState,
                        samplerState
                    );
                    Vector2 zero = default;
                    Vector2 texCoord;
                    Vector2 texCoord2 = default;
                    Vector2 texCoord3;
                    if (TextureWrap)
                    {
                        zero = Vector2.Zero;
                        texCoord = new Vector2(ActualSize.X / Subtexture.Texture.Width, 0f);
                        texCoord2 = new Vector2(ActualSize.X / Subtexture.Texture.Width, ActualSize.Y / Subtexture.Texture.Height);
                        texCoord3 = new Vector2(0f, ActualSize.Y / Subtexture.Texture.Height);
                    }
                    else
                    {
                        zero.X = MathUtils.Lerp(Subtexture.TopLeft.X, Subtexture.BottomRight.X, Texcoord1.X);
                        zero.Y = MathUtils.Lerp(Subtexture.TopLeft.Y, Subtexture.BottomRight.Y, Texcoord1.Y);
                        texCoord2.X = MathUtils.Lerp(Subtexture.TopLeft.X, Subtexture.BottomRight.X, Texcoord2.X);
                        texCoord2.Y = MathUtils.Lerp(Subtexture.TopLeft.Y, Subtexture.BottomRight.Y, Texcoord2.Y);
                        texCoord = new Vector2(texCoord2.X, zero.Y);
                        texCoord3 = new Vector2(zero.X, texCoord2.Y);
                    }
                    if (FlipHorizontal)
                    {
                        Utilities.Swap(ref zero.X, ref texCoord.X);
                        Utilities.Swap(ref texCoord2.X, ref texCoord3.X);
                    }
                    if (FlipVertical)
                    {
                        Utilities.Swap(ref zero.Y, ref texCoord2.Y);
                        Utilities.Swap(ref texCoord.Y, ref texCoord3.Y);
                    }
                    texturedBatch2D.QueueQuad(
                        result,
                        result2,
                        result3,
                        result4,
                        Depth,
                        zero,
                        texCoord,
                        texCoord2,
                        texCoord3,
                        color
                    );
                }
                else
                {
                    dc.PrimitivesRenderer2D.FlatBatch(1, depthStencilState).QueueQuad(result, result2, result3, result4, Depth, color);
                }
            }

            Color color2 = OutlineColor * GlobalColorTransform;
            float time = (float)Time.RealTime;

            if ((color2.A != 0 || OutlineRainbowColor) && OutlineThickness > 0f)
            {
                FlatBatch2D flatBatch2D = dc.PrimitivesRenderer2D.FlatBatch(1, depthStencilState);
                Vector2 vector = Vector2.Normalize(GlobalTransform.Right.XY);
                Vector2 v5 = -Vector2.Normalize(GlobalTransform.Up.XY);
                int num = (int)MathUtils.Max(MathUtils.Round(OutlineThickness * GlobalTransform.Right.Length()), 1f);

                for (int i = 0; i < num; i++)
                {
                    if (OutlineRainbowColor)
                    {
                        float len1 = Vector2.Distance(result, result2);
                        float len2 = Vector2.Distance(result2, result3);
                        float len3 = Vector2.Distance(result3, result4);
                        float len4 = Vector2.Distance(result4, result);

                        float perimeter = len1 + len2 + len3 + len4;

                        float t = 0f;

                        DrawRainbowLine(flatBatch2D, result, result2, t, perimeter, Depth, time);
                        t += len1 / perimeter;

                        DrawRainbowLine(flatBatch2D, result2, result3, t, perimeter, Depth, time);
                        t += len2 / perimeter;

                        DrawRainbowLine(flatBatch2D, result3, result4, t, perimeter, Depth, time);
                        t += len3 / perimeter;

                        DrawRainbowLine(flatBatch2D, result4, result, t, perimeter, Depth, time);
                    }
                    else
                    {
                        flatBatch2D.QueueLine(result, result2, Depth, color2);
                        flatBatch2D.QueueLine(result2, result3, Depth, color2);
                        flatBatch2D.QueueLine(result3, result4, Depth, color2);
                        flatBatch2D.QueueLine(result4, result, Depth, color2);
                    }

                    result += vector - v5;
                    result2 += -vector - v5;
                    result3 += -vector + v5;
                    result4 += vector + v5;
                }
            }
        }

        public void DrawRainbowLine(FlatBatch2D batch, Vector2 a, Vector2 b, float startT, float perimeter, float depth, float time)
        {
            float length = Vector2.Distance(a, b);

            // Độ phân giải của đoạn vẽ (segment size). Từ 1-8px là cực kỳ mịn mượt.
            float segmentSize = MathUtils.Max(1f, 8f / RainbowDensity);
            int segments = MathUtils.Max(2, (int)(length / segmentSize));

            for (int s = 0; s < segments; s++)
            {
                float t1 = (float)s / segments;
                float t2 = (float)(s + 1) / segments;

                Vector2 p1 = Vector2.Lerp(a, b, t1);
                Vector2 p2 = Vector2.Lerp(a, b, t2);

                // Tỷ lệ quãng đường trên tổng chu vi (0.0 -> 1.0)
                float globalT = startT + t1 * (length / perimeter);

                float hueOffset;

                if (ProportionalRainbow)
                {
                    // Co dãn theo widget: nhân với số vòng (Density)
                    hueOffset = globalT * RainbowDensity;
                }
                else
                {
                    // Cố định pixel thực tế: 1 vòng màu = 100 pixels
                    float pixelDistance = globalT * perimeter;
                    hueOffset = (pixelDistance / 100f) * RainbowDensity;
                }

                // Dùng nguyên bản HueSpeed làm nhịp thời gian toàn cục
                Color c = ColorUtils.ColorFromHSV(time * HueSpeed + hueOffset) * GlobalColorTransform;

                batch.QueueLine(p1, p2, depth, c);
            }
        }

        public override void MeasureOverride(Vector2 parentAvailableSize)
        {
            IsDrawRequired = FillColor.A != 0 || (OutlineColor.A != 0 && OutlineThickness > 0f);
            DesiredSize = Size;
        }
    }

}
