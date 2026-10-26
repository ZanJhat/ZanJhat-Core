using System;
using System.Linq;
using System.Collections.Generic;
using Engine;
using TemplatesDatabase;
using GameEntitySystem;
using Engine.Graphics;
using Game;

namespace ZanJhat.Core
{
    public class TrailConfig
    {
        public Color Color = Color.White;
        public float WidthOnScreen = 3f;
        public int MaxPoints = 30;
        public double FadeDuration = 0.025;
        public float MinDistance = 0.14f;
    }

    public class TrailStyle
    {
        public Projectile Projectile;
        public TrailConfig TrailConfig;
        public Queue<Vector3> Points;
        public Vector3? LastPoint;
        public bool IsFading;
        public double FadeStartTime;
    }

    public class SubsystemProjectileTrail : SubsystemBlockBehavior, IDrawable
    {
        public SubsystemGameInfo m_subsystemGameInfo;

        private readonly PrimitivesRenderer3D m_primitivesRenderer = new();
        private readonly Queue<TrailStyle> m_trailPool = new();
        private readonly List<TrailStyle> m_activeTrails = new();

        public List<TrailStyle> m_toRemove = new();

        private readonly Dictionary<Type, TrailConfig> m_trailRegistry = new();

        public int[] DrawOrders => new[] { 6 };

        public override int[] HandledBlocks
        {
            get
            {
                List<int> allBlockIds = new List<int>();

                foreach (Block block in BlocksManager.Blocks)
                {
                    if (block != null)
                    {
                        allBlockIds.Add(block.BlockIndex);
                    }
                }

                return allBlockIds.ToArray();
            }
        }

        public override void OnFiredAsProjectile(Projectile projectile)
        {
            if (!CoreSettingsManager.GraphicsSettings.ProjectileTrail)
                return;

            int value = projectile.Value;
            int contents = Terrain.ExtractContents(value);
            int data = Terrain.ExtractData(value);
            Block block = BlocksManager.Blocks[contents];

            if (m_trailRegistry.TryGetValue(block.GetType(), out TrailConfig config))
            {
                AddTrail(projectile, config);
            }
            else if (block is ArrowBlock)
                AddTrail(projectile, Color.White, 3.5f, 30);
            else if (block is SpearBlock)
                AddTrail(projectile, Color.White, 5f, 20);
            else if (block is BulletBlock)
            {
                BulletBlock.BulletType bulletType = BulletBlock.GetBulletType(data);
                if (bulletType == BulletBlock.BulletType.MusketBall)
                    AddTrail(projectile, Color.White, 3.5f, 50);
                else
                    AddTrail(projectile, Color.White, 2.5f, 50);
            }
        }

        public void Draw(Camera camera, int drawOrder)
        {
            if (!CoreSettingsManager.GraphicsSettings.ProjectileTrail)
            {
                if (m_activeTrails.Count > 0)
                {
                    foreach (TrailStyle trail in m_activeTrails)
                    {
                        trail.Points?.Clear();
                        m_trailPool.Enqueue(trail);
                    }

                    m_activeTrails.Clear();
                    m_toRemove.Clear();
                }

                return;
            }

            FlatBatch3D batch = m_primitivesRenderer.FlatBatch(
                layer: 0,
                depthStencilState: DepthStencilState.Default,
                rasterizerState: null,
                blendState: BlendState.Additive);

            foreach (TrailStyle trail in m_activeTrails)
            {
                Projectile projectile = trail.Projectile;
                TrailConfig config = trail.TrailConfig;

                if (projectile == null)
                {
                    if (!trail.IsFading)
                        continue;
                }

                if (projectile != null)
                {
                    Vector3 position = projectile.Position;
                    float visibilitySqr = MathUtils.Sqr(projectile.CalcVisibilityRange());

                    if ((projectile.IsInFluid || projectile.Velocity.LengthSquared() <= 0.05f || projectile.NoChunk || Vector3.DistanceSquared(camera.ViewPosition, position) >= visibilitySqr) && !trail.IsFading)
                    {
                        trail.IsFading = true;
                        trail.FadeStartTime = m_subsystemGameInfo.TotalElapsedGameTime;
                    }

                    float minDistSqr = config.MinDistance * config.MinDistance;

                    if (trail.LastPoint == null || Vector3.DistanceSquared(trail.LastPoint.Value, projectile.Position) > minDistSqr)
                    {
                        trail.Points.Enqueue(projectile.Position);
                        trail.LastPoint = projectile.Position;

                        if (trail.Points.Count > config.MaxPoints)
                            trail.Points.Dequeue();
                    }
                }

                if (trail.IsFading)
                {
                    double gameTime = m_subsystemGameInfo.TotalElapsedGameTime;

                    // xóa dần point
                    if (trail.Points.Count > 0 && gameTime - trail.FadeStartTime >= config.FadeDuration)
                    {
                        trail.FadeStartTime = gameTime;
                        trail.Points.Dequeue();
                    }

                    // hết point → xoá trail thật sự
                    if (trail.Points.Count <= 1)
                    {
                        trail.IsFading = false;
                        trail.FadeStartTime = 0;
                        trail.LastPoint = null;

                        if (projectile == null)
                        {
                            m_toRemove.Add(trail);
                        }
                        continue;
                    }
                }

                if (trail.Points.Count < 2)
                    continue;

                Vector3[] pts = trail.Points.ToArray();

                float alpha = trail.Points.Count / (float)config.MaxPoints;
                Color color = config.Color * alpha;

                for (int i = 0; i < pts.Length - 1; i++)
                {
                    Vector3 p0 = pts[i];
                    Vector3 p1 = pts[i + 1];

                    Vector3 segment = p1 - p0;
                    float segLen = segment.Length();
                    if (segLen < 0.0001f)
                        continue;

                    Vector3 segmentDir = segment / segLen;

                    Vector3 mid = (p0 + p1) * 0.5f;
                    Vector3 viewDir = Vector3.Normalize(camera.ViewPosition - mid);

                    Vector3 right = Vector3.Cross(segmentDir, viewDir);
                    float rightLen = right.Length();

                    if (rightLen < 0.0001f)
                        continue;

                    right /= rightLen;

                    float t = i / (float)(pts.Length - 1);
                    float widthPixels = MathUtils.Lerp(0f, config.WidthOnScreen, t);

                    float dist = Vector3.Distance(camera.ViewPosition, mid);
                    float widthWorld = widthPixels * dist * 0.0012f;

                    Vector3 offset = right * widthWorld;

                    Vector3 v0 = p0 - offset;
                    Vector3 v1 = p0 + offset;
                    Vector3 v2 = p1 + offset;
                    Vector3 v3 = p1 - offset;

                    batch.QueueQuad(
                        v0, v1, v2, v3,
                        color, color, color, color);
                }
            }

            if (m_toRemove.Count > 0)
            {
                foreach (TrailStyle trail in m_toRemove)
                {
                    trail.Points.Clear();
                    m_activeTrails.Remove(trail);
                    m_trailPool.Enqueue(trail);
                }
            }

            m_toRemove.Clear();

            m_primitivesRenderer.Flush(camera.ViewProjectionMatrix);
        }

        public void AddTrail(Projectile projectile, Color color, float widthOnScreen, int maxPoints, double fadeDuration = 0.025, float minDistance = 0.14f)
        {
            TrailConfig config = new()
            {
                Color = color,
                WidthOnScreen = widthOnScreen,
                MaxPoints = maxPoints,
                FadeDuration = fadeDuration,
                MinDistance = minDistance
            };

            AddTrail(projectile, config);
        }

        public void AddTrail(Projectile projectile, TrailConfig config)
        {
            if (config == null)
                return;

            TrailStyle trail;

            if (m_trailPool.Count > 0)
            {
                trail = m_trailPool.Dequeue();
            }
            else
            {
                trail = new TrailStyle();
            }

            trail.Projectile = projectile;
            trail.TrailConfig = config;
            trail.Points = new Queue<Vector3>(config.MaxPoints);
            trail.LastPoint = null;
            trail.IsFading = false;
            trail.FadeStartTime = 0;

            m_activeTrails.Add(trail);

            projectile.OnRemove += () =>
            {
                trail.Projectile = null;
                trail.IsFading = true;
                trail.FadeStartTime = m_subsystemGameInfo.TotalElapsedGameTime;
            };
        }

        public void RemoveTrail(Projectile projectile)
        {
            foreach (TrailStyle trail in m_activeTrails)
            {
                if (trail.Projectile == projectile)
                {
                    trail.Projectile = null;
                    trail.IsFading = true;
                    trail.FadeStartTime = m_subsystemGameInfo.TotalElapsedGameTime;
                    break;
                }
            }
        }

        public override void Load(ValuesDictionary valuesDictionary)
        {
            base.Load(valuesDictionary);
            m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(true);
        }

        public void RegisterTrail(Type projectileBlock, TrailConfig config)
        {
            if (projectileBlock == null || config == null)
                return;

            m_trailRegistry[projectileBlock] = config;
        }

        public void RegisterTrail<T>(TrailConfig config) where T : Block
        {
            if (config == null)
                return;

            m_trailRegistry[typeof(T)] = config;
        }
    }
}
