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
using System.Diagnostics.CodeAnalysis;

namespace ZanJhat.Core
{
    public class GlowingEffect : Effect
    {
        public override string Name => "Glowing";
        public override string IconPath => "Textures/Effects/Glowing";
        public override EffectType EffectType => EffectType.Normal;
        public override bool NeedSave => true;

        public override int[] DrawOrders => [2000];

        public static RasterizerState OutlineRasterizerState = new RasterizerState
        {
            CullMode = CullMode.None,
            ScissorTestEnable = true
        };

        public OutlineShader OutlineShader => Owner?.m_outlineShader;

        private List<ComponentModel> m_targetModels;

        public List<ComponentModel> TargetModels
        {
            get
            {
                if (m_targetModels == null && Owner != null)
                {
                    m_targetModels = new List<ComponentModel>();

                    // 1. Thêm model body
                    ComponentModel bodyModel = ComponentCreatureModel != null ? ComponentCreatureModel : ComponentModel;
                    if (bodyModel != null)
                        m_targetModels.Add(bodyModel);

                    // 2. Thêm quần áo ngoài (Áo giáp, mũ...)
                    ComponentModel outerClothing = Owner.Entity.FindComponent<ComponentOuterClothingModel>();
                    if (outerClothing != null)
                        m_targetModels.Add(outerClothing);
                }

                return m_targetModels;
            }
        }

        public GlowingEffect(ComponentEffect owner, double duration)
          : base(owner, duration)
        {
        }

        public override void Draw(Camera camera, int drawOrder)
        {
            if (TargetModels == null || TargetModels.Count == 0)
                return;

            RasterizerState originalRasterizerState = Display.RasterizerState;
            DepthStencilState originalDepthState = Display.DepthStencilState;
            BlendState originalBlendState = Display.BlendState;

            Display.RasterizerState = OutlineRasterizerState;

            // =========================================================
            // PASS 1: TẠO BỨC TƯỜNG TÀNG HÌNH CHO TẤT CẢ CÁC LỚP MODEL
            // =========================================================
            Display.DepthStencilState = DepthStencilState.Default;
            Display.BlendState = BlendState.AlphaBlend;

            foreach (ComponentModel model in TargetModels)
            {
                if (model.Model == null || !model.IsVisibleForCamera) continue;

                Texture2D texture = model.TextureOverride;
                OutlineShader.SetParameters(Matrix.Identity, Color.Transparent, Vector2.Zero, 0.001f, texture);

                Matrix[] boneTransforms = model.AbsoluteBoneTransformsForCamera;
                foreach (ModelMesh mesh in model.Model.Meshes)
                {
                    Matrix worldViewProjMatrix = boneTransforms[mesh.ParentBone.Index] * camera.ProjectionMatrix;
                    OutlineShader.m_worldViewProjectionMatrixParameter.SetValue(worldViewProjMatrix);

                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        Display.DrawIndexed(PrimitiveType.TriangleList, OutlineShader, meshPart.VertexBuffer, meshPart.IndexBuffer, meshPart.StartIndex, meshPart.IndicesCount);
                    }
                }
            }

            // =========================================================
            // PASS 2: VẼ HÀO QUANG BAO BỌC
            // =========================================================
            Display.DepthStencilState = DepthStencilState.DepthRead;
            Display.BlendState = BlendState.Additive;

            float width = 0.015f;
            float aspectRatio = (float)Display.Viewport.Width / Display.Viewport.Height;
            float diag = width * 0.7071f;

            Vector2[] offsets = new Vector2[]
            {
                new Vector2(width / aspectRatio, 0), new Vector2(-width / aspectRatio, 0),
                new Vector2(0, width), new Vector2(0, -width),
                new Vector2(diag / aspectRatio, diag), new Vector2(-diag / aspectRatio, diag),
                new Vector2(diag / aspectRatio, -diag), new Vector2(-diag / aspectRatio, -diag)
            };

            Color glowColor = new Color(255, 200, 0, 70);

            foreach (Vector2 offset in offsets)
            {
                // Lặp qua từng Model để vẽ
                foreach (ComponentModel model in TargetModels)
                {
                    if (model.Model == null || !model.IsVisibleForCamera) continue;

                    Texture2D texture = model.TextureOverride;
                    OutlineShader.SetParameters(Matrix.Identity, glowColor, offset, 0.005f, texture);

                    Matrix[] boneTransforms = model.AbsoluteBoneTransformsForCamera;
                    foreach (ModelMesh mesh in model.Model.Meshes)
                    {
                        Matrix worldViewProjMatrix = boneTransforms[mesh.ParentBone.Index] * camera.ProjectionMatrix;
                        OutlineShader.m_worldViewProjectionMatrixParameter.SetValue(worldViewProjMatrix);

                        foreach (ModelMeshPart meshPart in mesh.MeshParts)
                        {
                            Display.DrawIndexed(PrimitiveType.TriangleList, OutlineShader, meshPart.VertexBuffer, meshPart.IndexBuffer, meshPart.StartIndex, meshPart.IndicesCount);
                        }
                    }
                }
            }

            // DỌN DẸP TRẢ LẠI STATE GỐC
            Display.RasterizerState = originalRasterizerState;
            Display.DepthStencilState = originalDepthState;
            Display.BlendState = originalBlendState;
        }
    }
}
