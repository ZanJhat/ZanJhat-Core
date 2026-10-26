using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Engine;
using Engine.Graphics;
using Game;

namespace ZanJhat.Core
{
    public static class RichTextParser
    {
        public const string TypeName = "RichTextParser";

        // Biểu thức chính quy để tìm các tag nằm trong dấu < >
        private static readonly Regex TagRegex = new Regex(@"<(.*?)>", RegexOptions.Compiled);

        // Phân tích chuỗi và thêm các Widget vào một StackPanelWidget nằm ngang.
        // Cú pháp hỗ trợ:
        // - Đổi màu: <color=R,G,B,A> (ví dụ: <color=255,0,0,255>)
        // - Trả về màu cũ: </color>
        // - Chèn Icon: <icon=Đường/Dẫn/Ảnh>
        public static void PopulateRichText(StackPanelWidget parent, string text, float fontScale, Color defaultColor)
        {
            // Ép buộc StackPanel phải nằm ngang để chữ và icon nối tiếp nhau
            parent.Direction = LayoutDirection.Horizontal;

            // Sử dụng Stack để lưu trữ lịch sử màu sắc (giúp tag </color> hoạt động đúng)
            Stack<Color> colorStack = new Stack<Color>();
            colorStack.Push(defaultColor); // Màu mặc định

            int lastProcessedIndex = 0;

            foreach (Match match in TagRegex.Matches(text))
            {
                // 1. Tạo Label cho phần text bình thường nằm TRƯỚC tag hiện tại
                if (match.Index > lastProcessedIndex)
                {
                    string plainText = text.Substring(lastProcessedIndex, match.Index - lastProcessedIndex);
                    CreateLabel(parent, plainText, colorStack.Peek(), fontScale);
                }

                // 2. Xử lý nội dung của Tag
                string tagContent = match.Groups[1].Value.Trim();

                if (tagContent.StartsWith("color=", StringComparison.OrdinalIgnoreCase))
                {
                    ParseAndPushColor(tagContent.Substring(6), colorStack);
                }
                else if (tagContent.Equals("/color", StringComparison.OrdinalIgnoreCase))
                {
                    if (colorStack.Count > 1) colorStack.Pop();
                }
                else if (tagContent.StartsWith("icon=", StringComparison.OrdinalIgnoreCase))
                {
                    string iconPath = tagContent.Substring(5).Trim();
                    CreateIcon(parent, iconPath, fontScale);
                }

                // Đánh dấu vị trí tiếp theo sau tag
                lastProcessedIndex = match.Index + match.Length;
            }

            // 3. Xử lý đoạn text còn sót lại ở cuối chuỗi (nếu có)
            if (lastProcessedIndex < text.Length)
            {
                string remainingText = text.Substring(lastProcessedIndex);
                CreateLabel(parent, remainingText, colorStack.Peek(), fontScale);
            }
        }

        private static void ParseAndPushColor(string colorData, Stack<Color> colorStack)
        {
            string[] components = colorData.Split(',');

            // Hỗ trợ:
            // R,G,B
            // R,G,B,A

            if (components.Length == 3 || components.Length == 4)
            {
                byte r = 255;
                byte g = 255;
                byte b = 255;
                byte a = 255;

                bool valid =
                    byte.TryParse(components[0], out r) &&
                    byte.TryParse(components[1], out g) &&
                    byte.TryParse(components[2], out b);

                if (components.Length == 4)
                {
                    valid &= byte.TryParse(components[3], out a);
                }

                if (valid)
                {
                    colorStack.Push(new Color(r, g, b, a));
                    return;
                }
            }

            // Fallback nếu tag sai cú pháp
            colorStack.Push(colorStack.Peek());
        }

        private static void CreateLabel(ContainerWidget parent, string text, Color color, float fontScale)
        {
            if (string.IsNullOrEmpty(text))
                return;

            LabelWidget label = new LabelWidget
            {
                Text = text,
                Color = color,
                FontScale = fontScale,
                VerticalAlignment = WidgetAlignment.Center, // Căn giữa theo chiều dọc để khớp với Icon
                WordWrap = false // Tắt WordWrap ở cấp độ Label con
            };
            parent.Children.Add(label);
        }

        private static void CreateIcon(ContainerWidget parent, string iconPath, float fontScale)
        {
            Subtexture icon = ContentManager.Get<Subtexture>(iconPath, null, false);

            if (icon == null)
            {
                Log.Warning($"[{TypeName}/CreateIcon]: Texture not found at {iconPath}. Use fallback");
                icon = ContentManager.Get<Subtexture>("Textures/Gui/Unavailable");
            }

            float iconSize = fontScale * 28f;

            RectangleWidget iconWidget = new RectangleWidget
            {
                Subtexture = icon,
                Size = new Vector2(iconSize),
                FillColor = Color.White,
                OutlineColor = Color.Transparent,
                VerticalAlignment = WidgetAlignment.Center
            };
            parent.Children.Add(iconWidget);
        }
    }
}
