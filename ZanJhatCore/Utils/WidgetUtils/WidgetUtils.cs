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

namespace ZanJhat.Core
{
    public enum Anchor
    {
        TopLeft = 0,
        TopCenter = 1,
        TopRight = 2,

        LeftCenter = 3,
        Center = 4,
        RightCenter = 5,

        BottomLeft = 6,
        BottomCenter = 7,
        BottomRight = 8
    }

    public static class WidgetUtils
    {
        public static bool IsPointInBounds(Vector2 point, Vector2 topLeft, Vector2 bottomRight)
        {
            return point.X >= topLeft.X &&
                   point.X <= bottomRight.X &&
                   point.Y >= topLeft.Y &&
                   point.Y <= bottomRight.Y;
        }

        public static void SetAnchor(Widget widget, Widget parent, Anchor anchor, float marginX, float marginY, bool clampToScreen = true, float depth = 0f)
        {
            if (parent == null)
            {
                Log.Warning($"[SetAnchor] Parent is null: {parent}");
                return;
            }

            SetAnchor(widget, parent.ActualSize, anchor, marginX, marginY, clampToScreen, depth);
        }

        public static void SetAnchor(Widget widget, Vector2 screenSize, Anchor anchor, float marginX, float marginY, bool clampToScreen = true, float depth = 0f)
        {
            Vector2 size = widget.ActualSize;

            float posX = 0f;
            float posY = 0f;

            switch (anchor)
            {
                case Anchor.TopLeft:
                    posX = marginX;
                    posY = marginY;
                    break;

                case Anchor.TopCenter:
                    posX = (screenSize.X - size.X) * 0.5f + marginX;
                    posY = marginY;
                    break;

                case Anchor.TopRight:
                    posX = screenSize.X - size.X - marginX;
                    posY = marginY;
                    break;

                case Anchor.LeftCenter:
                    posX = marginX;
                    posY = (screenSize.Y - size.Y) * 0.5f + marginY;
                    break;

                case Anchor.Center:
                    posX = (screenSize.X - size.X) * 0.5f + marginX;
                    posY = (screenSize.Y - size.Y) * 0.5f + marginY;
                    break;

                case Anchor.RightCenter:
                    posX = screenSize.X - size.X - marginX;
                    posY = (screenSize.Y - size.Y) * 0.5f + marginY;
                    break;

                case Anchor.BottomLeft:
                    posX = marginX;
                    posY = screenSize.Y - size.Y - marginY;
                    break;

                case Anchor.BottomCenter:
                    posX = (screenSize.X - size.X) * 0.5f + marginX;
                    posY = screenSize.Y - size.Y - marginY;
                    break;

                case Anchor.BottomRight:
                    posX = screenSize.X - size.X - marginX;
                    posY = screenSize.Y - size.Y - marginY;
                    break;

                default:
                    Log.Warning($"SetAnchor: Unknown anchor '{anchor}' for widget '{widget.GetType().Name}', fallback to TopLeft.");
                    posX = marginX;
                    posY = marginY;
                    break;
            }

            if (clampToScreen)
            {
                posX = MathUtils.Clamp(posX, 0f, screenSize.X - size.X);
                posY = MathUtils.Clamp(posY, 0f, screenSize.Y - size.Y);
            }

            widget.LayoutTransform = Matrix.CreateTranslation(posX, posY, depth);
        }

        public static CanvasWidget AddInventorySlot(ContainerWidget parent, int value, int count, Vector2 cavasSize, Vector2 iconSize, float fontScale, Vector2 margin, WidgetAlignment verticalAlignment, WidgetAlignment horizontalAlignment)
        {
            if (parent != null)
            {
                CanvasWidget canvas = new CanvasWidget
                {
                    Size = cavasSize,
                    VerticalAlignment = verticalAlignment,
                    HorizontalAlignment = horizontalAlignment,
                    Margin = margin
                };

                BlockIconWidget blockIcon = new BlockIconWidget
                {
                    Value = value,
                    HorizontalAlignment = WidgetAlignment.Center,
                    VerticalAlignment = WidgetAlignment.Center,
                    Margin = new Vector2(2f, 2f)
                };
                canvas.Children.Add(blockIcon);

                LabelWidget label = new LabelWidget
                {
                    FontScale = fontScale,
                    VerticalAlignment = WidgetAlignment.Far,
                    HorizontalAlignment = WidgetAlignment.Far,
                    Text = count.ToString(),
                    Margin = new Vector2(6f, 2f)
                };
                canvas.Children.Add(label);

                parent.Children.Add(canvas);
                return canvas;
            }
            return null;
        }

        public static LabelWidget AddLabel(ContainerWidget parent, string text, Color color, float fontScale, bool wordWrap, Vector2 margin, WidgetAlignment verticalAlignment, WidgetAlignment horizontalAlignment) => AddLabel(parent, null, text, color, fontScale, wordWrap, margin, verticalAlignment, horizontalAlignment);

        public static LabelWidget AddLabel(ContainerWidget parent, string name, string text, Color color, float fontScale, bool wordWrap, Vector2 margin, WidgetAlignment verticalAlignment, WidgetAlignment horizontalAlignment)
        {
            if (parent != null)
            {
                LabelWidget label = new LabelWidget
                {
                    Name = name,
                    FontScale = fontScale,
                    Color = color,
                    VerticalAlignment = verticalAlignment,
                    HorizontalAlignment = horizontalAlignment,
                    WordWrap = wordWrap,
                    Text = text,
                    Margin = margin
                };

                parent.Children.Add(label);
                return label;
            }
            return null;
        }

        /* Ví dụ gọi:
        WidgetUtils.AddRichText(
            parent: this, 
            text: "Mở khóa <color=255,215,0>Rương Vàng</color> <icon=Textures/Gui/GoldenKey> thành công!", 
            defaultColor: Color.White, 
            fontScale: 1f, 
            margin: new Vector2(10f, 10f), 
            verticalAlignment: WidgetAlignment.Near, 
            horizontalAlignment: WidgetAlignment.Near
        );*/
        public static StackPanelWidget AddRichText(ContainerWidget parent, string text, Color defaultColor, float fontScale, Vector2 margin, WidgetAlignment verticalAlignment, WidgetAlignment horizontalAlignment) => AddRichText(parent, null, text, defaultColor, fontScale, margin, verticalAlignment, horizontalAlignment);

        public static StackPanelWidget AddRichText(ContainerWidget parent, string name, string text, Color defaultColor, float fontScale, Vector2 margin, WidgetAlignment verticalAlignment, WidgetAlignment horizontalAlignment)
        {
            if (parent != null)
            {
                StackPanelWidget stackPanel = new StackPanelWidget
                {
                    Name = name,
                    Direction = LayoutDirection.Horizontal,
                    VerticalAlignment = verticalAlignment,
                    HorizontalAlignment = horizontalAlignment,
                    Margin = margin
                };

                // Phân tích cú pháp và nhét Label/Icon vào stackPanel
                RichTextParser.PopulateRichText(stackPanel, text, fontScale, defaultColor);

                parent.Children.Add(stackPanel);
                return stackPanel;
            }
            return null;
        }

        public static void DisableHitTestRecursive(Widget widget, bool includeChildren = true)
        {
            if (widget == null)
                return;

            widget.IsHitTestVisible = false;
            widget.ClampToBounds = false;

            if (includeChildren && widget is ContainerWidget container)
            {
                foreach (Widget child in container.Children)
                    DisableHitTestRecursive(child);
            }
        }
    }
}
