using Engine;
using Game;

namespace ZanJhat.Core
{
    public class InputGestureWidget : CanvasWidget
    {
        private Vector2? m_lastDragPosition;

        private bool m_clickEnable;

        public float? Scroll;

        public Vector2? Drag;

        public Vector2? Press;

        private Vector2? m_clickedPosition;

        public LayoutDirection? DragDirection;

        public bool IsClicked => m_clickedPosition.HasValue;

        public Vector2? ClickedPosition => m_clickedPosition;

        public override bool IsHitTestVisible => true;

        public InputGestureWidget()
        {
            m_clickEnable = true;
        }

        public override void Update()
        {
            Press = null;
            Drag = null;
            Scroll = null;
            m_clickedPosition = null;
            if (Input.Scroll.HasValue && HitTestPanel(Input.Scroll.Value.XY))
            {
                Scroll = Input.Scroll.Value.Z;
            }
            if (Input.Tap.HasValue && HitTestPanel(Input.Tap.Value))
            {
                m_lastDragPosition = ScreenToWidget(Input.Tap.Value);
            }
            if (m_lastDragPosition.HasValue)
            {
                if (Input.Press.HasValue)
                {
                    Vector2 value = Input.Press.Value;
                    if (HitTestPanel(value))
                    {
                        Vector2 value2 = ScreenToWidget(value) - m_lastDragPosition.Value;
                        if (value2.Length() >= 10f)
                        {
                            m_clickEnable = false;
                            if (!DragDirection.HasValue)
                            {
                                if (MathUtils.Abs(value2.X) >= MathUtils.Abs(value2.Y))
                                {
                                    DragDirection = LayoutDirection.Horizontal;
                                }
                                else
                                {
                                    DragDirection = LayoutDirection.Vertical;
                                }
                            }
                            Drag = value2;
                        }
                        else
                        {
                            Press = value;
                        }
                    }
                }
                else
                {
                    if (m_clickEnable)
                    {
                        m_clickedPosition = m_lastDragPosition.Value;
                    }
                    m_lastDragPosition = null;
                    m_clickEnable = true;
                }
            }
            if (!Drag.HasValue)
            {
                DragDirection = null;
            }
        }

        public bool HitTestPanel(Vector2 position)
        {
            bool found = false;
            HitTestGlobal(position, delegate (Widget widget)
            {
                found = widget.IsChildWidgetOf(this) || widget == this;
                return true;
            });
            return found;
        }
    }
}
