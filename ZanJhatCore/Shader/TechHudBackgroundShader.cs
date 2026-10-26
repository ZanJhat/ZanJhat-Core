using System;
using Engine;
using Engine.Graphics;
using Engine.Media;
using Game;

namespace ZanJhat.Core
{
    public class TechHudBackgroundShader : Shader
    {
        public ShaderParameter m_worldViewProjectionMatrixParameter;

        public ShaderParameter m_renderSize;

        public ShaderParameter m_time;

        public readonly ShaderTransforms Transforms;

        public Vector2 RenderSize
        {
            set => m_renderSize.SetValue(value);
        }

        public float Time
        {
            set => m_time.SetValue(value);

        }

        public TechHudBackgroundShader()
            : base(
                ShaderCodeManager.GetFast("Shaders/TechHudBackground.vsh"),
                ShaderCodeManager.GetFast("Shaders/TechHudBackground.psh"))
        {
            m_worldViewProjectionMatrixParameter = GetParameter("u_worldViewProjectionMatrix", allowNull: true);
            m_renderSize = GetParameter("u_renderSize", allowNull: true);
            m_time = GetParameter("u_time", allowNull: true);
            Transforms = new ShaderTransforms(1);
        }

        public override void PrepareForDrawingOverride()
        {
            Transforms.UpdateMatrices(1, worldView: false, viewProjection: false, worldViewProjection: true);
            m_worldViewProjectionMatrixParameter.SetValue(Transforms.WorldViewProjection, 1);
        }
    }

    public class TechHudBackgroundBatch2D : FlatBatch2D
    {
        public static TechHudBackgroundShader Shader = new();

        public new void Flush(bool clearAfterFlush = true)
        {
            Display.DepthStencilState = base.DepthStencilState;
            Display.RasterizerState = base.RasterizerState;
            Display.BlendState = base.BlendState;
            FlushWithCurrentState2(clearAfterFlush);
        }

        public void FlushWithCurrentState2(bool clearAfterFlush = true)
        {
            // Sử dụng Display.Viewport để luôn lấy đúng độ phân giải màn hình hiện tại
            Shader.RenderSize = new Vector2(Display.Viewport.Width, Display.Viewport.Height);

            Shader.Time = (float)(DateTime.Now.ToOADate() % 1.0 * 24.0 * 60.0 * 60.0 % 600.0);
            Shader.Transforms.World[0] = PrimitivesRenderer2D.ViewportMatrix();
            FlushWithDeviceState(Shader, clearAfterFlush);
        }
    }

    public class TechHudBackgroundRenderer2D : BasePrimitivesRenderer<TechHudBackgroundBatch2D, TexturedBatch2D, FontBatch2D>
    {
        public static Matrix ViewportMatrix()
        {
            Viewport viewport = Display.Viewport;
            float num = 1f / (float)viewport.Width;
            float num2 = 1f / (float)viewport.Height;
            return new Matrix(2f * num, 0f, 0f, 0f, 0f, -2f * num2, 0f, 0f, 0f, 0f, 1f, 0f, -1f, 1f, 0f, 1f);
        }

        public TechHudBackgroundBatch2D FlatBatch(int layer = 0, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, BlendState blendState = null)
        {
            depthStencilState = depthStencilState ?? DepthStencilState.None;
            rasterizerState = rasterizerState ?? RasterizerState.CullNoneScissor;
            blendState = blendState ?? BlendState.AlphaBlend;
            return FindFlatBatch(layer, depthStencilState, rasterizerState, blendState);
        }

        public TexturedBatch2D TexturedBatch(Texture2D texture, bool useAlphaTest = false, int layer = 0, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, BlendState blendState = null, SamplerState samplerState = null)
        {
            depthStencilState = depthStencilState ?? DepthStencilState.None;
            rasterizerState = rasterizerState ?? RasterizerState.CullNoneScissor;
            blendState = blendState ?? BlendState.AlphaBlend;
            samplerState = samplerState ?? SamplerState.LinearClamp;
            return FindTexturedBatch(texture, useAlphaTest, layer, depthStencilState, rasterizerState, blendState, samplerState);
        }

        public FontBatch2D FontBatch(BitmapFont font = null, int layer = 0, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, BlendState blendState = null, SamplerState samplerState = null)
        {
            font = font ?? BitmapFont.DebugFont;
            depthStencilState = depthStencilState ?? DepthStencilState.None;
            rasterizerState = rasterizerState ?? RasterizerState.CullNoneScissor;
            blendState = blendState ?? BlendState.AlphaBlend;
            samplerState = samplerState ?? SamplerState.LinearClamp;
            return FindFontBatch(font, layer, depthStencilState, rasterizerState, blendState, samplerState);
        }

        public void Flush(bool clearAfterFlush = true, int maxLayer = int.MaxValue)
        {
            Flush(ViewportMatrix(), clearAfterFlush, maxLayer);
        }
    }
}
