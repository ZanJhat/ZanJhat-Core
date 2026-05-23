using Engine;
using Engine.Graphics;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using System.Linq;
using TemplatesDatabase;
using Game;

namespace ZanJhat.Core
{
    public class SubsystemFireflies : Subsystem, IDrawable, IUpdateable
    {
        public class Firefly
        {
            public Firefly(Vector3 position, double time, float hue, float saturation, ComponentPlayer owner)
            {
                Position = position;
                NextPosition = position;
                SpawnTime = time;
                Hue = hue;
                Saturation = saturation;
                Owner = owner;
            }

            public Vector3 Position;
            public Vector3 NextPosition;
            public double TimeToStopMoving;
            public double SpawnTime;
            public float Hue;
            public float Saturation;
            public ComponentPlayer Owner;
            public bool IsVisible;
            public double LastVisibleTime;
        }

        public class PlayerFireflyManager
        {
            public ComponentPlayer Owner { get; set; }
            public List<Firefly> Fireflies { get; set; } = new List<Firefly>();
            public Vector3 LastUpdatePosition { get; set; }
            public double LastSpawnTime { get; set; }
            public int MaxFireflies { get; set; } = 96;
        }

        public SubsystemPlayers m_subsystemPlayers;
        public SubsystemTime m_subsystemTime;
        public SubsystemSky m_subsystemSky;
        public SubsystemWeather m_subsystemWeather;
        public SubsystemTerrain m_subsystemTerrain;
        public SubsystemGameInfo m_subsystemGameInfo;
        public SubsystemModelsRenderer m_subsystemModelsRenderer;

        public Game.Random m_random = new Game.Random();
        public Texture2D m_texture;
        public PrimitivesRenderer3D m_primitivesRenderer;

        public Dictionary<ComponentPlayer, PlayerFireflyManager> m_playerFireflyManagers = new Dictionary<ComponentPlayer, PlayerFireflyManager>();

        public List<Firefly> m_allFireflies = new List<Firefly>();

        public Func<ComponentPlayer, bool> AllowSpawnFirefly;
        public event Action<Firefly> FireflySpawned;
        public event Action<Firefly, float> FireflyUpdated;
        public event Action<Firefly> FireflyRemoved;

        public int[] DrawOrders => new[] { 10 };
        public UpdateOrder UpdateOrder => UpdateOrder.Default;

        public override void Load(ValuesDictionary valuesDictionary)
        {
            base.Load(valuesDictionary);
            m_subsystemPlayers = Project.FindSubsystem<SubsystemPlayers>(true);
            m_subsystemTime = Project.FindSubsystem<SubsystemTime>(true);
            m_subsystemSky = Project.FindSubsystem<SubsystemSky>(true);
            m_subsystemWeather = Project.FindSubsystem<SubsystemWeather>(true);
            m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(true);
            m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(true);
            m_subsystemModelsRenderer = Project.FindSubsystem<SubsystemModelsRenderer>(true);

            m_texture = ContentManager.Get<Texture2D>("Textures/RoundGlow");
            m_primitivesRenderer = m_subsystemModelsRenderer.PrimitivesRenderer;
        }

        public void Update(float dt)
        {
            UpdatePlayerManagers();

            CleanupOldFireflies();

            foreach (Firefly firefly in m_allFireflies)
            {
                ComponentPlayer owner = firefly.Owner;
                if (owner == null)
                    continue;

                float distanceSq = Vector3.DistanceSquared(firefly.Position, owner.ComponentBody.Position);

                if (distanceSq < 32f * 32f)
                {
                    UpdateFireflyMovement(firefly, m_subsystemTime.GameTime, dt);
                    FireflyUpdated?.Invoke(firefly, dt);
                }
            }
        }

        public void UpdatePlayerManagers()
        {
            double nowTime = m_subsystemTime.GameTime;

            // Tạo manager cho player mới
            foreach (ComponentPlayer componentPlayer in m_subsystemPlayers.ComponentPlayers)
            {
                if (!m_playerFireflyManagers.ContainsKey(componentPlayer))
                {
                    m_playerFireflyManagers[componentPlayer] = new PlayerFireflyManager
                    {
                        Owner = componentPlayer,
                        LastUpdatePosition = componentPlayer.ComponentBody.Position,
                        LastSpawnTime = nowTime
                    };
                }
            }

            // Tạo HashSet chứa player đang active
            HashSet<ComponentPlayer> activePlayers = new HashSet<ComponentPlayer>(m_subsystemPlayers.ComponentPlayers);

            // Tìm manager cần xóa
            List<ComponentPlayer> managersToRemove = new List<ComponentPlayer>();

            foreach (ComponentPlayer componentPlayer in m_playerFireflyManagers.Keys)
            {
                if (!activePlayers.Contains(componentPlayer))
                {
                    managersToRemove.Add(componentPlayer);
                }
            }

            // Xóa manager + fireflies của player đã rời
            foreach (ComponentPlayer componentPlayer in managersToRemove)
            {
                PlayerFireflyManager manager = m_playerFireflyManagers[componentPlayer];

                foreach (Firefly firefly in manager.Fireflies)
                {
                    m_allFireflies.Remove(firefly);
                    FireflyRemoved?.Invoke(firefly);
                }

                m_playerFireflyManagers.Remove(componentPlayer);
            }
        }

        public void CleanupOldFireflies()
        {
            double nowTime = m_subsystemTime.GameTime;

            foreach (PlayerFireflyManager manager in m_playerFireflyManagers.Values)
            {
                manager.Fireflies.RemoveAll(firefly =>
                {
                    float distanceSq = Vector3.DistanceSquared(firefly.Position, manager.Owner.ComponentBody.Position);
                    bool remove = firefly.Owner == null || distanceSq > 32f * 32f || nowTime - firefly.SpawnTime > 60.0;

                    if (remove)
                    {
                        m_allFireflies.Remove(firefly);
                        FireflyRemoved?.Invoke(firefly);
                    }

                    return remove;
                });
            }
        }

        public void UpdateFireflyMovement(Firefly firefly, double nowTime, float dt)
        {
            if (nowTime >= firefly.TimeToStopMoving)
            {
                firefly.NextPosition += m_random.Vector3(0.3f);
                firefly.TimeToStopMoving = nowTime + Vector3.Distance(firefly.Position, firefly.NextPosition) * m_random.Float(2f, 10f);
            }
            else
            {
                float timeLeft = (float)(firefly.TimeToStopMoving - nowTime);
                if (timeLeft > 0)
                {
                    Vector3 speed = (firefly.NextPosition - firefly.Position) / timeLeft * dt;

                    Vector3 delta = firefly.NextPosition - firefly.Position;

                    if (delta.LengthSquared() < speed.LengthSquared())
                        firefly.Position = firefly.NextPosition;
                    else
                        firefly.Position += speed;
                }
            }
        }

        public void Draw(Camera camera, int drawOrder)
        {
            if (m_subsystemGameInfo.WorldSettings.EnvironmentBehaviorMode != EnvironmentBehaviorMode.Living || !CoreSettingsManager.GraphicsSettings.Firefly)
                return;

            // Reset trạng thái visible cho tất cả đom đóm
            foreach (Firefly firefly in m_allFireflies)
            {
                firefly.IsVisible = false;
            }

            double nowTime = m_subsystemTime.GameTime;

            // Tìm người chơi tương ứng với camera hiện tại
            ComponentPlayer currentPlayer = camera.GameWidget?.PlayerData?.ComponentPlayer;

            if (currentPlayer == null)
                return;

            Vector3 playerPosition = currentPlayer.ComponentBody.Position;

            if (currentPlayer != null && m_playerFireflyManagers.TryGetValue(currentPlayer, out PlayerFireflyManager currentManager))
            {
                // Cập nhật vị trí cho manager hiện tại
                currentManager.LastUpdatePosition = playerPosition;

                // Xử lý spawn đom đóm cho người chơi hiện tại
                SpawnFirefliesForPlayer(currentManager, playerPosition, nowTime);

                // Giới hạn số lượng đom đóm cho người chơi hiện tại
                LimitFirefliesForPlayer(currentManager, playerPosition);
            }

            TexturedBatch3D batch = m_primitivesRenderer.TexturedBatch(m_texture, true, 0, null, RasterizerState.CullCounterClockwiseScissor, null, SamplerState.AnisotropicWrap);

            // Vẽ tất cả đom đóm trong phạm vi camera
            foreach (Firefly firefly in m_allFireflies)
            {
                float distance = Vector3.Distance(firefly.Position, camera.ViewPosition);

                if (distance < 32f)
                {
                    firefly.IsVisible = true;
                    firefly.LastVisibleTime = nowTime;
                    DrawFirefly(camera, firefly, distance, nowTime, batch);
                }
            }

            // Xóa đom đóm không visible
            RemoveInvisibleFireflies();
        }

        public void SpawnFirefliesForPlayer(PlayerFireflyManager manager, Vector3 playerPosition, double nowTime)
        {
            int x = Terrain.ToCell(playerPosition.X);
            int z = Terrain.ToCell(playerPosition.Z);

            // Kiểm tra điều kiện môi trường
            if (m_subsystemSky.SkyLightIntensity < 0.4f && m_subsystemWeather.PrecipitationIntensity == 0f && !SubsystemWeather.IsPlaceFrozen(m_subsystemTerrain.Terrain.GetTemperature(x, z), (int)playerPosition.Y))
            {
                if (AllowSpawnFirefly != null && !AllowSpawnFirefly(manager.Owner))
                    return;

                // Spawn đom đóm dựa trên độ ẩm khu vực
                float fillRatio = manager.Fireflies.Count / (float)manager.MaxFireflies;
                double spawnInterval = MathUtils.Lerp(0.2, 1.0, fillRatio);

                if (nowTime > manager.LastSpawnTime + spawnInterval)
                {
                    manager.LastSpawnTime = nowTime;

                    int humidity = m_subsystemTerrain.Terrain.GetHumidity(x, z);
                    float spawnChance = CalculateSpawnChance(humidity);

                    if (m_random.Float(0f, 1f) < spawnChance)
                    {
                        if (FindValidSpawnPosition(playerPosition, out Vector3 spawnPosition))
                        {
                            Firefly firefly = new Firefly(
                                spawnPosition,
                                nowTime,
                                m_random.Float(60f, 150f),
                                m_random.Float(0.5f, 1f),
                                manager.Owner);

                            manager.Fireflies.Add(firefly);
                            m_allFireflies.Add(firefly);

                            FireflySpawned?.Invoke(firefly);
                        }
                    }
                }
            }
        }

        public float CalculateSpawnChance(int humidity)
        {
            if (humidity > 12)
                return 0.9f;
            if (humidity > 10)
                return 0.75f;
            if (humidity > 8)
                return 0.6f;
            if (humidity > 6)
                return 0.45f;

            return 0.3f;
        }

        public bool FindValidSpawnPosition(Vector3 center, out Vector3 result)
        {
            for (int i = 0; i < 10; i++)
            {
                float angle = m_random.Float(0f, MathUtilsEx.TwoPi);
                //float radius = MathUtils.Sqrt(m_random.Float(0f, 1f)) * 16f;
                float radius = MathUtils.Lerp(6f, 16f, MathUtils.Sqrt(m_random.Float(0f, 1f)));

                Vector3 randomPosition = new Vector3(
                    center.X + MathUtils.Cos(angle) * radius,
                    0f,
                    center.Z + MathUtils.Sin(angle) * radius);

                int rx = Terrain.ToCell(randomPosition.X);
                int rz = Terrain.ToCell(randomPosition.Z);

                // Kiểm tra ánh sáng tại vị trí spawn
                int topHeight = m_subsystemTerrain.Terrain.GetTopHeight(rx, rz);
                randomPosition.Y = topHeight + m_random.Float(0.4f, 6f);

                int lightAtPosition = m_subsystemTerrain.Terrain.GetCellLight(rx, (int)randomPosition.Y, rz);

                if (lightAtPosition < 9)
                {
                    result = randomPosition;
                    return true;
                }
            }

            result = Vector3.Zero;
            return false;
        }

        public void LimitFirefliesForPlayer(PlayerFireflyManager manager, Vector3 playerPosition)
        {
            // Đếm số lượng đom đóm trong khu vực xung quanh người chơi
            int nearbyFirefliesCount = CountNearbyFireflies(playerPosition, 40f);

            // Đếm số lượng người chơi trong khu vực xung quanh người chơi
            int nearbyPlayersCount = CountNearbyPlayers(playerPosition, 40f);

            // Điều chỉnh giới hạn dựa trên số lượng người chơi gần nhau
            int adjustedMaxFireflies = CalculateAdjustedMaxFireflies(manager.MaxFireflies, nearbyPlayersCount);

            // Nếu có quá nhiều đom đóm, xóa bớt
            if (nearbyFirefliesCount > adjustedMaxFireflies)
            {
                RemoveExcessFireflies(manager, nearbyFirefliesCount - adjustedMaxFireflies);
            }
        }

        public int CountNearbyFireflies(Vector3 position, float radius)
        {
            int count = 0;
            foreach (Firefly firefly in m_allFireflies)
            {
                if (Vector3.DistanceSquared(firefly.Position, position) < radius * radius)
                {
                    count++;
                }
            }
            return count;
        }

        public int CountNearbyPlayers(Vector3 position, float radius)
        {
            int count = 0;
            foreach (ComponentPlayer componentPlayer in m_subsystemPlayers.ComponentPlayers)
            {
                if (Vector3.DistanceSquared(componentPlayer.ComponentBody.Position, position) < radius * radius)
                {
                    count++;
                }
            }
            return count;
        }

        public int CalculateAdjustedMaxFireflies(int baseMax, int nearbyPlayersCount)
        {
            // Nếu có nhiều người chơi gần nhau, giảm số lượng đom đóm cho mỗi người để tránh quá tải
            if (nearbyPlayersCount <= 1)
                return baseMax;
            if (nearbyPlayersCount == 2)
                return (int)(baseMax * 0.8f);
            if (nearbyPlayersCount == 3)
                return (int)(baseMax * 0.6f);

            return (int)(baseMax * 0.4f);
        }

        public void RemoveExcessFireflies(PlayerFireflyManager manager, int excessCount)
        {
            Vector3 center = manager.LastUpdatePosition;

            for (int i = 0; i < excessCount && manager.Fireflies.Count > 0; i++)
            {
                int farthestIndex = -1;
                float farthestDistance = -1f;

                for (int j = 0; j < manager.Fireflies.Count; j++)
                {
                    float d = Vector3.DistanceSquared(manager.Fireflies[j].Position, center);

                    if (d > farthestDistance)
                    {
                        farthestDistance = d;
                        farthestIndex = j;
                    }
                }

                if (farthestIndex >= 0)
                {
                    Firefly f = manager.Fireflies[farthestIndex];

                    manager.Fireflies.RemoveAt(farthestIndex);
                    m_allFireflies.Remove(f);

                    FireflyRemoved?.Invoke(f);
                }
            }
        }

        public void RemoveInvisibleFireflies()
        {
            double nowTime = m_subsystemTime.GameTime;
            List<Firefly> firefliesToRemove = new List<Firefly>();

            foreach (Firefly firefly in m_allFireflies)
            {
                // Xóa nếu không được vẽ trong 2 giây
                if (!firefly.IsVisible && nowTime - firefly.LastVisibleTime > 2.0)
                {
                    firefliesToRemove.Add(firefly);
                }
            }

            foreach (Firefly firefly in firefliesToRemove)
            {
                if (m_playerFireflyManagers.TryGetValue(firefly.Owner, out PlayerFireflyManager manager))
                {
                    manager.Fireflies.Remove(firefly);
                }
                m_allFireflies.Remove(firefly);
                FireflyRemoved?.Invoke(firefly);
            }
        }

        public void DrawFirefly(Camera camera, Firefly firefly, float distance, double nowTime, TexturedBatch3D batch)
        {
            if (!firefly.IsVisible)
                return;

            double timePassed = (nowTime - firefly.SpawnTime) * 0.5;

            float pulse = 0.5f + 0.5f * MathUtils.Sin((float)(timePassed * 3f + firefly.SpawnTime));
            pulse = pulse * pulse * pulse;

            float size = MathUtils.Lerp(0.006f, 0.035f, pulse);

            // Điều chỉnh kích thước dựa trên khoảng cách
            size *= MathUtils.Clamp(1.5f - distance / 32f, 0.5f, 1.5f);

            Vector3 v1 = Vector3.Cross(camera.ViewDirection, Vector3.UnitY);
            if (v1.LengthSquared() < 0.0001f)
                v1 = Vector3.UnitX;
            else
                v1 = Vector3.Normalize(v1);

            Vector3 v2 = -Vector3.Normalize(Vector3.Cross(camera.ViewDirection, v1));

            Vector3 p1 = Vector3.Transform(firefly.Position + size * (-v1 - v2), camera.ViewMatrix);
            Vector3 p2 = Vector3.Transform(firefly.Position + size * (v1 - v2), camera.ViewMatrix);
            Vector3 p3 = Vector3.Transform(firefly.Position + size * (-v1 + v2), camera.ViewMatrix);
            Vector3 p4 = Vector3.Transform(firefly.Position + size * (v1 + v2), camera.ViewMatrix);

            // Điều chỉnh độ sáng dựa trên khoảng cách
            float brightness = MathUtils.Clamp(0.85f + size * 10 - distance * 0.015f, 0.2f, 1f);

            Vector3 hsv = new Vector3(firefly.Hue, firefly.Saturation, brightness);

            batch.QueueQuad(
                p1, p3, p4, p2,
                new Vector2(0, 1), new Vector2(0, 0),
                new Vector2(1, 0), new Vector2(1, 1),
                new Color(Color.HsvToRgb(hsv)));
        }

        public Firefly SpawnFirefly(Vector3 position, ComponentPlayer owner)
        {
            Firefly firefly = new Firefly(
                position,
                m_subsystemTime.GameTime,
                m_random.Float(60f, 150f),
                m_random.Float(0.5f, 1f),
                owner);

            m_allFireflies.Add(firefly);
            FireflySpawned?.Invoke(firefly);

            if (m_playerFireflyManagers.TryGetValue(owner, out PlayerFireflyManager manager))
                manager.Fireflies.Add(firefly);

            return firefly;
        }

        public void RemoveFirefly(Firefly firefly)
        {
            if (firefly == null)
                return;

            if (m_playerFireflyManagers.TryGetValue(firefly.Owner, out PlayerFireflyManager manager))
                manager.Fireflies.Remove(firefly);

            m_allFireflies.Remove(firefly);

            FireflyRemoved?.Invoke(firefly);
        }
    }
}
