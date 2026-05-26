using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SimRedes.Network;

namespace SimRedes.UI
{
    public static class UIComponents
    {
        public static class Colors
        {
            public static readonly Color backgroundBase = new Color(0.051f, 0.067f, 0.090f, 0.95f);
            public static readonly Color surfacePanel = new Color(0.086f, 0.106f, 0.133f, 0.95f);
            public static readonly Color surfaceElevated = new Color(0.129f, 0.149f, 0.176f, 0.95f);
            public static readonly Color border = new Color(0.188f, 0.212f, 0.239f, 0.6f);
            public static readonly Color borderAccent = new Color(0.345f, 0.651f, 1f, 0.7f);

            public static readonly Color textPrimary = new Color(0.902f, 0.929f, 0.953f, 1f);
            public static readonly Color textSecondary = new Color(0.545f, 0.580f, 0.620f, 1f);
            public static readonly Color textAccent = new Color(0.345f, 0.651f, 1f, 1f);
            public static readonly Color textWarning = new Color(0.824f, 0.600f, 0.133f, 1f);

            public static readonly Color buttonNormal = new Color(0.200f, 0.380f, 0.580f, 1f);
            public static readonly Color buttonHover = new Color(0.280f, 0.460f, 0.660f, 1f);
            public static readonly Color buttonSelected = new Color(0.320f, 0.520f, 0.720f, 1f);
            public static readonly Color buttonPressed = new Color(0.150f, 0.280f, 0.450f, 1f);

            public static readonly Color buttonDanger = new Color(0.455f, 0.129f, 0.129f, 1f);
            public static readonly Color buttonDangerHover = new Color(0.545f, 0.176f, 0.176f, 1f);

            public static readonly Color buttonWarning = new Color(0.565f, 0.314f, 0.063f, 1f);
            public static readonly Color buttonWarningHover = new Color(0.647f, 0.376f, 0.102f, 1f);

            public static Color GetColorForDeviceType(Network.DeviceType type)
            {
                switch (type)
                {
                    case Network.DeviceType.Router:
                        return new Color(0.3f, 0.5f, 0.9f); // Azul
                    case Network.DeviceType.Switch:
                        return new Color(0.2f, 0.8f, 0.8f); // Cyan
                    case Network.DeviceType.PC:
                        return Color.green;
                    default:
                        return Color.gray;
                }
            }
        }

        private static string TEXT_OBJ = "Text";

        public static Font GetFont(int size = 14)
        {
            Font font = Font.CreateDynamicFontFromOSFont("Arial", size);
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font;
        }

        public static ColorBlock GetButtonColors(Color normalColor)
        {
            var colors = new ColorBlock();
            colors.normalColor = normalColor;
            colors.highlightedColor = normalColor * 1.3f;
            colors.pressedColor = normalColor * 0.6f;
            colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            colors.colorMultiplier = 1f;
            return colors;
        }

        public static Text CreateInfoText(Transform parent, string text, Vector2 position, Font font, int fontSize, Color color, bool bold)
        {
            var textObj = new GameObject("InfoText");
            textObj.transform.SetParent(parent, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = position;
            textRect.sizeDelta = new Vector2(260, 20);

            var textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.color = color;
            textComponent.fontSize = fontSize;
            textComponent.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.font = font;

            return textComponent;
        }

        public static GameObject CreateSimpleText(Transform parent, string text, Font font, int fontSize, Color color, FontStyle style)
        {
            var textObj = new GameObject(TEXT_OBJ);
            textObj.transform.SetParent(parent, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(500, 30);

            var textComp = textObj.AddComponent<Text>();
            textComp.text = text;
            textComp.color = color;
            textComp.fontSize = fontSize;
            textComp.fontStyle = style;
            textComp.alignment = TextAnchor.MiddleLeft;
            textComp.font = font;

            return textObj;
        }

        public static void ApplyTitleStyle(Text text)
        {
            if (text != null)
            {
                text.fontStyle = FontStyle.Bold;
                text.color = Colors.textPrimary;
            }
        }

        public static Button CreateSmallInfoButton(Transform parent, Vector2 position, Font font)
        {
            var btnObj = new GameObject("InfoButton");
            btnObj.transform.SetParent(parent, false);

            var rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(34, 34);

            var img = btnObj.AddComponent<Image>();
            Texture2D tex = CreateRoundedRectTexture(34, 34, 10, Colors.textPrimary, Colors.textPrimary, 0f);
            img.sprite = Sprite.Create(tex, new Rect(0, 0, 34, 34), new Vector2(0.5f, 0.5f), 100);
            img.type = Image.Type.Sliced;

            var btn = btnObj.AddComponent<Button>();
            btn.colors = GetButtonColors(Colors.textPrimary);

            Font btnFont = GetFont(26);

            var textObj = new GameObject(TEXT_OBJ);
            textObj.transform.SetParent(btnObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(0, 2);
            textRect.offsetMax = new Vector2(0, -2);

            var text = textObj.AddComponent<Text>();
            text.text = "i";
            text.color = Colors.backgroundBase;
            text.fontSize = 26;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = btnFont;

            return btn;
        }

        public static GameObject CreateMenuPanel(Transform parent, string name, Vector2 size)
        {
            return CreateRoundedPanel(parent, size, 25, Colors.surfacePanel, Colors.borderAccent, name);
        }

        public static GameObject CreateRoundedPanel(Transform parent, Vector2 size, int cornerRadius = 20, Color? backgroundColor = null, Color? borderColor = null, string name = "RoundedPanel")
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;

            Image image = panel.AddComponent<Image>();
            image.color = backgroundColor ?? Colors.surfacePanel;

            Color bg = backgroundColor ?? Colors.surfacePanel;
            Color border = borderColor ?? Colors.borderAccent;
            Texture2D tex = CreateRoundedRectTexture((int)size.x, (int)size.y, cornerRadius, bg, border, 2f);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, (int)size.x, (int)size.y), new Vector2(0.5f, 0.5f), 100);
            image.sprite = sprite;
            image.type = Image.Type.Sliced;

            return panel;
        }

        public static Text CreateMenuTitle(Transform parent, string title, int fontSize, Vector2 position, Font font)
        {
            var titleObj = new GameObject("MenuTitle");
            titleObj.transform.SetParent(parent, false);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = position;
            titleRect.sizeDelta = new Vector2(600, 40);

            var titleText = titleObj.AddComponent<Text>();
            titleText.text = title;
            titleText.color = Colors.textAccent;
            titleText.fontSize = fontSize;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.font = font;

            return titleText;
        }

        public static Button CreateMenuButton(Transform parent, string name, string label, Vector2 position, Vector2 size, Font font, int fontSize = 18)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            var rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var img = btnObj.AddComponent<Image>();
            Texture2D btnTex = CreateRoundedRectTexture((int)size.x, (int)size.y, 12, Colors.buttonNormal, Colors.borderAccent, 1.5f);
            img.sprite = Sprite.Create(btnTex, new Rect(0, 0, (int)size.x, (int)size.y), new Vector2(0.5f, 0.5f), 100);
            img.type = Image.Type.Sliced;

            var btn = btnObj.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Colors.buttonNormal;
            colors.highlightedColor = Colors.buttonHover;
            colors.pressedColor = Colors.buttonPressed;
            colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            colors.colorMultiplier = 1f;
            btn.colors = colors;

            var textObj = new GameObject(TEXT_OBJ);
            textObj.transform.SetParent(btnObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textObj.AddComponent<Text>();
            text.text = label;
            text.color = Colors.textPrimary;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = font;

            return btn;
        }

        public static Button CreateSmallButton(Transform parent, string name, string label, Vector2 position, float width, float height, Font font)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            var rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(width, height);

            var img = btnObj.AddComponent<Image>();
            img.color = Colors.buttonNormal;
            img.raycastTarget = true;

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.colors = GetButtonColors(Colors.buttonNormal);

            var textObj = new GameObject(TEXT_OBJ);
            textObj.transform.SetParent(btnObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textObj.AddComponent<Text>();
            text.text = label;
            text.color = Colors.textPrimary;
            text.fontSize = 13;
            text.fontStyle = FontStyle.Normal;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = font;

            return btn;
        }

        public static GameObject CreateClickOutsideToClose(Transform canvasTransform, string panelName)
        {
            var bgObj = new GameObject("ClickOutsideBG_" + panelName);
            bgObj.transform.SetParent(canvasTransform, false);

            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0);
            bgRect.anchorMax = new Vector2(1, 1);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            var bgCanvas = bgObj.AddComponent<Canvas>();
            bgCanvas.sortingOrder = 50;

            var bgRaycaster = bgObj.AddComponent<GraphicRaycaster>();

            var bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0.3f);
            bgImg.raycastTarget = true;

            var bgBtn = bgObj.AddComponent<Button>();
            bgBtn.transition = Selectable.Transition.None;
            bgBtn.targetGraphic = bgImg;
            bgBtn.onClick.AddListener(() =>
            {
                GameObject panel = GameObject.Find(panelName);
                if (panel != null) GameObject.Destroy(panel);
                GameObject.Destroy(bgObj);
            });

            return bgObj;
        }

        public static GameObject CreateSimpleDropdown(Transform parent, List<string> options, Font font, Vector2 position, Vector2 size)
        {
            var dropdownObj = new GameObject("Dropdown");
            dropdownObj.transform.SetParent(parent, false);
            var dropRect = dropdownObj.AddComponent<RectTransform>();
            dropRect.anchorMin = new Vector2(0.5f, 0.5f);
            dropRect.anchorMax = new Vector2(0.5f, 0.5f);
            dropRect.anchoredPosition = position;
            dropRect.sizeDelta = size;

            var dropImg = dropdownObj.AddComponent<Image>();
            dropImg.color = Colors.surfaceElevated;

            var drop = dropdownObj.AddComponent<Dropdown>();
            drop.targetGraphic = dropImg;
            drop.ClearOptions();
            drop.AddOptions(options);

            var templateObj = new GameObject("Template");
            templateObj.transform.SetParent(dropdownObj.transform, false);
            var templateRect = templateObj.AddComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0, 0);
            templateRect.anchorMax = new Vector2(1, 0);
            templateRect.pivot = new Vector2(0.5f, 1);
            templateRect.anchoredPosition = new Vector2(0, 0);
            templateRect.sizeDelta = new Vector2(0, size.y * options.Count);

            var templateImg = templateObj.AddComponent<Image>();
            templateImg.color = Colors.surfaceElevated;

            var templateScroll = templateObj.AddComponent<ScrollRect>();

            var itemObj = new GameObject("Item");
            itemObj.transform.SetParent(templateObj.transform, false);
            var itemRect = itemObj.AddComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0, 1);
            itemRect.anchorMax = new Vector2(1, 1);
            itemRect.pivot = new Vector2(0.5f, 1);
            itemRect.anchoredPosition = Vector2.zero;
            itemRect.sizeDelta = new Vector2(0, size.y);

            var itemImg = itemObj.AddComponent<Image>();
            itemImg.color = Colors.surfaceElevated;

            var itemToggle = itemObj.AddComponent<Toggle>();
            itemToggle.targetGraphic = itemImg;

            var itemLabel = new GameObject("Label");
            itemLabel.transform.SetParent(itemObj.transform, false);
            var itemLabelRect = itemLabel.AddComponent<RectTransform>();
            itemLabelRect.anchorMin = Vector2.zero;
            itemLabelRect.anchorMax = Vector2.one;
            itemLabelRect.offsetMin = new Vector2(5, 0);
            itemLabelRect.offsetMax = Vector2.zero;

            var itemText = itemLabel.AddComponent<Text>();
            itemText.text = "Option";
            itemText.color = Colors.textPrimary;
            itemText.fontSize = 14;
            itemText.alignment = TextAnchor.MiddleLeft;
            itemText.font = font;

            drop.template = templateRect;
            drop.captionText = null;
            drop.itemText = itemText;

            return dropdownObj;
        }

        public static void SetupNumericInput(InputField inputField, System.Action<string> onValueChanged)
        {
            inputField.onValueChanged.AddListener((value) =>
            {
                string filtered = "";
                foreach (char c in value)
                {
                    if (char.IsDigit(c) || c == '.')
                        filtered += c;
                }
                if (filtered != value)
                    inputField.text = filtered;
                onValueChanged?.Invoke(filtered);
            });

            inputField.onValidateInput = (string text, int charIndex, char addedChar) =>
            {
                if (char.IsDigit(addedChar) || addedChar == '.')
                    return addedChar;
                return '\0';
            };
        }

        public static Text CreateInfoTextFull(Transform parent, string text, Vector2 position, Vector2 size,
            Font font, int fontSize, Color color, bool bold, TextAnchor alignment)
        {
            var textObj = new GameObject("InfoText");
            textObj.transform.SetParent(parent, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = position;
            textRect.sizeDelta = size;

            var textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.color = color;
            textComponent.fontSize = fontSize;
            textComponent.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            textComponent.alignment = alignment;
            textComponent.font = font;

            return textComponent;
        }

        public static Texture2D CreateRoundedRectTexture(int width, int height, int cornerRadius, Color fillColor, Color borderColor = default, float borderWidth = 0f)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Trilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];

            float halfBorder = borderWidth / 2f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = y * width + x;
                    Vector2 center = new Vector2(width / 2f, height / 2f);
                    Vector2 pos = new Vector2(x, y);

                    float dist = DistanceToRoundedRect(pos, center, width, height, cornerRadius);

                    if (borderWidth > 0 && dist <= cornerRadius + halfBorder && dist >= cornerRadius - halfBorder)
                    {
                        pixels[idx] = borderColor;
                    }
                    else if (dist <= cornerRadius)
                    {
                        pixels[idx] = fillColor;
                    }
                    else
                    {
                        pixels[idx] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private static float DistanceToRoundedRect(Vector2 point, Vector2 center, float width, float height, float radius)
        {
            float halfW = width / 2f - radius;
            float halfH = height / 2f - radius;

            Vector2 localPoint = point - center;

            float dx = Mathf.Abs(localPoint.x);
            float dy = Mathf.Abs(localPoint.y);

            if (dx <= halfW && dy <= halfH)
                return 0f;
            if (dx <= halfW)
                return dy - halfH;
            if (dy <= halfH)
                return dx - halfW;

            return Mathf.Sqrt(Mathf.Pow(dx - halfW, 2) + Mathf.Pow(dy - halfH, 2));
        }

        public static Sprite CreateRoundedRectSprite(int width, int height, int cornerRadius, Color fillColor, Color borderColor = default, float borderWidth = 2f)
        {
            Texture2D tex = CreateRoundedRectTexture(width, height, cornerRadius, fillColor, borderColor, borderWidth);
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100);
        }

        public static Sprite CreateDeviceIcon(int size, Color color, bool antiAlias = true)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            int center = size / 2;
            float radius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int idx = y * size + x;
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));

                    if (antiAlias)
                    {
                        float r = radius - 1;
                        if (dist < r - 1)
                        {
                            pixels[idx] = color;
                        }
                        else if (dist < r + 1)
                        {
                            float alpha = Mathf.Lerp(1f, 0f, (dist - (r - 1)) / 2f);
                            pixels[idx] = new Color(color.r, color.g, color.b, alpha);
                        }
                        else
                        {
                            pixels[idx] = Color.clear;
                        }
                    }
                    else
                    {
                        if (dist <= radius - 2)
                        {
                            pixels[idx] = color;
                        }
                        else
                        {
                            pixels[idx] = Color.clear;
                        }
                    }
                }
            }

            tex.filterMode = antiAlias ? FilterMode.Bilinear : FilterMode.Point;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Button CreateRoundedButton(Transform parent, string name, string label, Vector2 position, Vector2 size, int cornerRadius = 15)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Image image = btnObj.AddComponent<Image>();
            Texture2D normalTex = CreateRoundedRectTexture((int)size.x, (int)size.y, cornerRadius, Colors.buttonNormal);
            image.sprite = Sprite.Create(normalTex, new Rect(0, 0, (int)size.x, (int)size.y), new Vector2(0.5f, 0.5f), 100);
            image.type = Image.Type.Sliced;

            Button button = btnObj.AddComponent<Button>();
            button.targetGraphic = image;

            var colors = button.colors;
            colors.normalColor = Colors.buttonNormal;
            colors.highlightedColor = Colors.buttonHover;
            colors.pressedColor = Colors.buttonPressed;
            colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            colors.colorMultiplier = 1f;
            button.colors = colors;

            Font font = GetFont(16);

            GameObject textObj = new GameObject(TEXT_OBJ);
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text text = textObj.AddComponent<Text>();
            text.text = label;
            text.color = Colors.textPrimary;
            text.fontSize = 16;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = font;

            return button;
        }

        public static GameObject CreateInfoColumn(Transform parent, Vector2 position, float width, float height, Font font, string text)
        {
            var colObj = new GameObject("InfoColumn");
            colObj.transform.SetParent(parent, false);
            var colRect = colObj.AddComponent<RectTransform>();
            colRect.anchorMin = new Vector2(0.5f, 0.5f);
            colRect.anchorMax = new Vector2(0.5f, 0.5f);
            colRect.anchoredPosition = position;
            colRect.sizeDelta = new Vector2(width, height);
            var colText = colObj.AddComponent<Text>();
            colText.text = text;
            colText.color = Colors.textPrimary;
            colText.fontSize = 14;
            colText.alignment = TextAnchor.UpperLeft;
            colText.font = font;
            return colObj;
        }

        public static InputField CreateInputField(Transform parent, string name, Vector2 position, Vector2 size,
            string defaultValue, string placeholder, Font font, int fontSize)
        {
            var inputObj = new GameObject(name);
            inputObj.transform.SetParent(parent, false);
            var inputRect = inputObj.AddComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0.5f, 0.5f);
            inputRect.anchorMax = new Vector2(0.5f, 0.5f);
            inputRect.anchoredPosition = position;
            inputRect.sizeDelta = size;

            // Background
            var bgImage = inputObj.AddComponent<Image>();
            bgImage.color = new Color(0.15f, 0.17f, 0.20f, 1f);
            bgImage.type = Image.Type.Sliced;

            // Text component
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(inputObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(4, 2);
            textRect.offsetMax = new Vector2(-4, -2);
            var inputText = textObj.AddComponent<Text>();
            inputText.text = defaultValue;
            inputText.color = Colors.textPrimary;
            inputText.fontSize = fontSize;
            inputText.font = font;
            inputText.alignment = TextAnchor.MiddleLeft;

            // Placeholder
            var placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(inputObj.transform, false);
            var phRect = placeholderObj.AddComponent<RectTransform>();
            phRect.anchorMin = Vector2.zero;
            phRect.anchorMax = Vector2.one;
            phRect.offsetMin = new Vector2(4, 2);
            phRect.offsetMax = new Vector2(-4, -2);
            var phText = placeholderObj.AddComponent<Text>();
            phText.text = placeholder;
            phText.color = new Color(0.4f, 0.4f, 0.4f, 1f);
            phText.fontSize = fontSize;
            phText.font = font;
            phText.alignment = TextAnchor.MiddleLeft;

            var inputField = inputObj.AddComponent<InputField>();
            inputField.textComponent = inputText;
            inputField.placeholder = phText;
            inputField.text = defaultValue;

            return inputField;
        }
    }
}
