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

            Vector2 screenLimit = root != null ? new Vector2(root.ActualSize.X / 2f - 48f, root.ActualSize.Y) : parentAvailableSize;

            float maxWidth = MaxWidth ?? float.PositiveInfinity;
            float maxHeight = MaxHeight ?? float.PositiveInfinity;

            Vector2 measured = Vector2.Zero;

            foreach (Widget child in Children)
            {
                float marginX = child.MarginLeft + child.MarginRight;
                float marginY = child.MarginTop + child.MarginBottom;

                // Giới hạn theo từng trục
                Vector2 available = new Vector2(MathUtils.Min(screenLimit.X, maxWidth) - marginX, MathUtils.Min(screenLimit.Y, maxHeight) - marginY);

                available = Vector2.Max(available, Vector2.Zero);

                child.Measure(available);

                Vector2 childSize = child.ParentDesiredSize;

                if (childSize.X != float.PositiveInfinity)
                    measured.X = MathUtils.Max(measured.X, childSize.X + marginX);

                if (childSize.Y != float.PositiveInfinity)
                    measured.Y = MathUtils.Max(measured.Y, childSize.Y + marginY);
            }
            measured += Padding * 2f;

            // Clamp
            measured.X = MathUtils.Min(measured.X, maxWidth);
            measured.Y = MathUtils.Min(measured.Y, maxHeight);

            DesiredSize = measured;
        }
    }
}
