using Engine;
using Engine.Graphics;

namespace ZanJhat.Core
{
    public class OutlineShader : Shader
    {
        public ShaderParameter m_worldViewProjectionMatrixParameter;
        public ShaderParameter m_outlineColorParameter;
        public ShaderParameter m_offsetParameter;
        public ShaderParameter m_zOffsetParameter;

        public ShaderParameter m_textureParameter;
        public ShaderParameter m_samplerParameter;
        public ShaderParameter m_useTextureParameter;

        public OutlineShader(string vsh, string psh) : base(vsh, psh)
        {
            m_worldViewProjectionMatrixParameter = GetParameter("u_worldViewProjectionMatrix", true);
            m_outlineColorParameter = GetParameter("u_outlineColor", true);
            m_offsetParameter = GetParameter("u_offset", true);
            m_zOffsetParameter = GetParameter("u_zOffset", true);

            m_textureParameter = GetParameter("u_texture", true);
            m_samplerParameter = GetParameter("u_sampler", true);
            m_useTextureParameter = GetParameter("u_useTexture", true);
        }

        public void SetParameters(Matrix worldViewProjection, Color color, Vector2 offset, float zOffset, Texture2D texture = null)
        {
            m_worldViewProjectionMatrixParameter.SetValue(worldViewProjection);
            m_outlineColorParameter.SetValue(new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f));
            m_offsetParameter.SetValue(offset);
            m_zOffsetParameter.SetValue(zOffset);

            // Kiểm tra xem Model có Texture không
            if (texture != null)
            {
                m_textureParameter.SetValue(texture);
                m_samplerParameter.SetValue(SamplerState.PointClamp); // Giữ pixel sắc nét như Minecraft
                m_useTextureParameter.SetValue(1f); // Bật chế độ đục lỗ rỗng
            }
            else
            {
                m_useTextureParameter.SetValue(0f); // Model không có ảnh -> Cứ vẽ viền khối vuông bình thường
            }
        }
    }
}
