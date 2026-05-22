using Engine;
using Game;

namespace ZanJhat.Core
{
    public class ShoulderCamera : BasePerspectiveCamera
    {
        public Vector3 m_position;

        public float ShoulderOffset = 0.75f;
        public float Distance = 1.25f;
        public float Height = 1.75f;
        public float AimOffset = 0.35f;

        public override bool UsesMovementControls => false;
        public override bool IsEntityControlEnabled => true;

        public ShoulderCamera(GameWidget gameWidget) : base(gameWidget) { }

        public override void Activate(Camera previousCamera)
        {
            m_position = previousCamera.ViewPosition;
            SetupPerspectiveCamera(m_position, previousCamera.ViewDirection, previousCamera.ViewUp);
        }

        public override void Update(float dt)
        {
            if (GameWidget.Target == null)
                return;

            Matrix matrix = Matrix.CreateFromQuaternion(GameWidget.Target.ComponentCreatureModel.EyeRotation);

            matrix.Translation =
                GameWidget.Target.ComponentBody.Position +
                0.9f * GameWidget.Target.ComponentBody.BoxSize.Y * Vector3.UnitY;

            // Shoulder offset
            Vector3 v =
                -Distance * matrix.Forward +
                Height * matrix.Up +
                ShoulderOffset * matrix.Right;

            Vector3 desiredPosition = matrix.Translation + v;

            if (Vector3.Distance(desiredPosition, m_position) < 10f)
            {
                Vector3 delta = desiredPosition - m_position;
                m_position += 3f * dt * delta;
            }
            else
            {
                m_position = desiredPosition;
            }

            Vector3 vector2 = m_position - matrix.Translation;

            float? num = null;

            Vector3 vector3 = Vector3.Normalize(Vector3.Cross(vector2, Vector3.UnitY));
            Vector3 v3 = Vector3.Normalize(Vector3.Cross(vector2, vector3));

            SubsystemTerrain subsystemTerrain = GameWidget.SubsystemGameWidgets.SubsystemTerrain;

            for (int i = 0; i <= 0; i++)
            {
                for (int j = 0; j <= 0; j++)
                {
                    Vector3 v4 = 0.5f * (vector3 * i + v3 * j);

                    Vector3 start = matrix.Translation + v4;
                    Vector3 end = start + vector2 + Vector3.Normalize(vector2) * 0.5f;

                    TerrainRaycastResult? result =
                        subsystemTerrain.Raycast(
                            start,
                            end,
                            false,
                            true,
                            (value, _) =>
                            {
                                Block block = BlocksManager.Blocks[Terrain.ExtractContents(value)];

                                for (int k = 0; k < 6; k++)
                                {
                                    if (!block.IsFaceTransparent(subsystemTerrain, k, value))
                                        return true;
                                }

                                return false;
                            });

                    if (result.HasValue)
                    {
                        num = num.HasValue
                            ? MathUtils.Min(num.Value, result.Value.Distance)
                            : result.Value.Distance;
                    }
                }
            }

            Vector3 cameraPos =
                !num.HasValue
                ? matrix.Translation + vector2
                : matrix.Translation + Vector3.Normalize(vector2) * MathUtils.Max(num.Value - 0.5f, 0.2f);

            Vector3 aimTarget = matrix.Translation + AimOffset * matrix.Right;

            SetupPerspectiveCamera(cameraPos, aimTarget - cameraPos, Vector3.UnitY);
        }
    }
}
