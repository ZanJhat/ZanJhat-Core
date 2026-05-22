using Engine;
using Game;

namespace ZanJhat.Core
{
    public class AutoSizeCanvasWidget : CanvasWidget
    {
        public float? MaxWidth { get; set; }

        public float? MaxHeight { get; set; }

        private Vector2 m_padding;

        public Vector2 Padding
        {
            get => m_padding;
            set => m_padding = value;
        }

        public AutoSizeCanvasWidget()
        {
            Padding = Vector2.Zero;
        }

        public override void MeasureOverride(Vector2 parentAvailableSize)
        {
            ContainerWidget root = ScreensManager.RootWidget;

            // Kích thước tối đa thực tế (Mặc định hoặc theo limit chia đôi màn hình của bạn)
            Vector2 screenLimit = root != null ? new Vector2(root.ActualSize.X / 2f - 48f, root.ActualSize.Y) : parentAvailableSize;

            float maxWidth = MaxWidth ?? float.PositiveInfinity;
            float maxHeight = MaxHeight ?? float.PositiveInfinity;

            Vector2 maxAvailable = new Vector2(
                MathUtils.Min(screenLimit.X, maxWidth),
                MathUtils.Min(screenLimit.Y, maxHeight)
            );

            Vector2 measured = Vector2.Zero;

            foreach (Widget child in Children)
            {
                // TỐI ƯU 1: Bỏ qua widget đang ẩn
                if (!child.IsVisible) continue;

                // TỐI ƯU 2: Lấy tọa độ của child trong Canvas (nếu có set)
                Vector2 widgetPosition = GetWidgetPosition(child) ?? Vector2.Zero;

                float marginX = child.MarginLeft + child.MarginRight;
                float marginY = child.MarginTop + child.MarginBottom;

                // Tính toán không gian còn lại cho child, trừ đi tọa độ và margin
                Vector2 available = new Vector2(
                    maxAvailable.X - widgetPosition.X - marginX,
                    maxAvailable.Y - widgetPosition.Y - marginY
                );

                available = Vector2.Max(available, Vector2.Zero);

                child.Measure(available);

                // Giữ nguyên ParentDesiredSize vì nó là chuẩn xác để cover LayoutTransform
                Vector2 childSize = child.ParentDesiredSize;

                if (!float.IsPositiveInfinity(childSize.X))
                    measured.X = MathUtils.Max(measured.X, widgetPosition.X + childSize.X + marginX);

                if (!float.IsPositiveInfinity(childSize.Y))
                    measured.Y = MathUtils.Max(measured.Y, widgetPosition.Y + childSize.Y + marginY);
            }

            measured += Padding * 2f;

            // Clamp lần cuối để đảm bảo không bung quá MaxWidth/MaxHeight
            measured.X = MathUtils.Min(measured.X, maxWidth);
            measured.Y = MathUtils.Min(measured.Y, maxHeight);

            DesiredSize = measured;
        }
    }
}
