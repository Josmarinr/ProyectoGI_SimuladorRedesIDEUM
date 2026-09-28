using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SimRedes.Network;

namespace SimRedes.UI
{
    /// <summary>
    /// Metodos de utilidad para la creacion de componentes UI del simulador.
    /// Proporciona fabricas de paneles, botones, textos, dropdowns y campos de entrada
    /// con la paleta de colores y estilo visual definidos.
    /// </summary>
    public static class UIComponents
    {
        /// <summary>
        /// Define la paleta de colores consistente del simulador. Todos los paneles, botones
        /// y textos usan estos colores para mantener una apariencia uniforme.
        /// </summary>
        public static class Colors
        {
            /// <summary>Color de fondo base de paneles y ventanas.</summary>
            public static readonly Color backgroundBase = new Color(0.051f, 0.067f, 0.090f, 0.95f);
            /// <summary>Color de superficie de paneles estandar.</summary>
            public static readonly Color surfacePanel = new Color(0.086f, 0.106f, 0.133f, 0.95f);
            /// <summary>Color de superficie elevada (dropdowns, elementos interactivos).</summary>
            public static readonly Color surfaceElevated = new Color(0.129f, 0.149f, 0.176f, 0.95f);
            /// <summary>Color de borde por defecto.</summary>
            public static readonly Color border = new Color(0.188f, 0.212f, 0.239f, 0.6f);
            /// <summary>Color de borde de acento (paneles activos o destacados).</summary>
            public static readonly Color borderAccent = new Color(0.345f, 0.651f, 1f, 0.7f);

            /// <summary>Color de texto principal (blanco azulado claro).</summary>
            public static readonly Color textPrimary = new Color(0.902f, 0.929f, 0.953f, 1f);
            /// <summary>Color de texto secundario (gris suave).</summary>
            public static readonly Color textSecondary = new Color(0.545f, 0.580f, 0.620f, 1f);
            /// <summary>Color de texto de acento (azul).</summary>
            public static readonly Color textAccent = new Color(0.345f, 0.651f, 1f, 1f);
            /// <summary>Color de texto de advertencia (amarillo/dorado).</summary>
            public static readonly Color textWarning = new Color(0.824f, 0.600f, 0.133f, 1f);

            /// <summary>Color de fondo de boton en estado normal.</summary>
            public static readonly Color buttonNormal = new Color(0.200f, 0.380f, 0.580f, 1f);
            /// <summary>Color de boton en hover.</summary>
            public static readonly Color buttonHover = new Color(0.280f, 0.460f, 0.660f, 1f);
            /// <summary>Color de boton seleccionado.</summary>
            public static readonly Color buttonSelected = new Color(0.320f, 0.520f, 0.720f, 1f);
            /// <summary>Color de boton presionado.</summary>
            public static readonly Color buttonPressed = new Color(0.150f, 0.280f, 0.450f, 1f);

            /// <summary>Color de fondo de boton de peligro/eliminacion.</summary>
            public static readonly Color buttonDanger = new Color(0.455f, 0.129f, 0.129f, 1f);
            /// <summary>Color de boton de peligro en hover.</summary>
            public static readonly Color buttonDangerHover = new Color(0.545f, 0.176f, 0.176f, 1f);

            /// <summary>Color de fondo de boton de advertencia.</summary>
            public static readonly Color buttonWarning = new Color(0.565f, 0.314f, 0.063f, 1f);
            /// <summary>Color de boton de advertencia en hover.</summary>
            public static readonly Color buttonWarningHover = new Color(0.647f, 0.376f, 0.102f, 1f);

            /// <summary>
            /// Devuelve el color asociado a un tipo de dispositivo de red.
            /// </summary>
            /// <param name="type">Tipo de dispositivo (Router, Switch, PC).</param>
            /// <returns>Color representativo del dispositivo.</returns>
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

        // Fuente unica cacheada para todo el proyecto.
        // Crea UNA sola instancia de fuente y la reutiliza, evitando agotar
        // los atlas de textura de fuente con multiples instancias.
        private static Font cachedFont = null;

        // Cache de texturas redondeadas para evitar crear cientos de Texture2D
        // (que Unity NO libera automaticamente al destruir GameObjects).
        // La clave generica combina: width_height_cornerRadius_colorR_colorG_colorB_colorA_borderR_borderG_borderB_borderA_borderWidth
        private static Dictionary<string, Texture2D> roundedTextureCache = new Dictionary<string, Texture2D>();

        // Textura blanca 1x1 compartida para lineas de enlace y otros usos
        // donde solo importa el color del RawImage/Image, no la textura en si.
        private static Texture2D sharedWhiteTexture = null;
        private static bool sharedWhiteTextureCreated = false;

        /// <summary>
        /// Obtiene una textura blanca 1x1 compartida. Se crea una sola vez
        /// y se reutiliza en toda la aplicacion. Util para lineas de enlace
        /// u otros elementos donde el color se controla via Image.color.
        /// </summary>
        public static Texture2D GetSharedWhiteTexture()
        {
            if (!sharedWhiteTextureCreated)
            {
                sharedWhiteTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                sharedWhiteTexture.SetPixel(0, 0, Color.white);
                sharedWhiteTexture.Apply();
                sharedWhiteTextureCreated = true;
            }
            return sharedWhiteTexture;
        }

        /// <summary>
        /// Limpia el cache de texturas redondeadas y el cache de iconos de dispositivo.
        /// Llamar en SceneCleanupService al limpiar la escena para liberar memoria
        /// de texturas y sprites no utilizados. Los iconos se reconstruyen bajo
        /// demanda la proxima vez que CreateDeviceIcon sea llamado.
        /// </summary>
        public static void ClearTextureCache()
        {
            foreach (var tex in roundedTextureCache.Values)
            {
                if (tex != null) UnityEngine.Object.Destroy(tex);
            }
            roundedTextureCache.Clear();

            // Iconos de dispositivo: la cache es duena del sprite y de su textura.
            // Se destruyen juntos y se quitan del registro de sprites protegidos.
            foreach (var kvp in deviceIconCache)
            {
                if (kvp.Value == null) continue;
                if (kvp.Value.texture != null) UnityEngine.Object.Destroy(kvp.Value.texture);
                UnregisterOwnedCachedSprite(kvp.Value);
                UnityEngine.Object.Destroy(kvp.Value);
            }
            deviceIconCache.Clear();
        }

        // Sprites con dueno (caches internas): nunca se destruyen al limpiar paneles,
        // porque la cache que los creo es responsable de destruirlos.
        private static readonly HashSet<Sprite> ownedCachedSprites = new HashSet<Sprite>();

        /// <summary>
        /// Registra un Sprite perteneciente a una cache interna (p. ej. deviceIconCache).
        /// Los sprites registrados se omiten en <see cref="SafeDestroyPanelSprites"/>
        /// porque su destruccion le corresponde a la cache que los creo.
        /// </summary>
        /// <param name="sprite">Sprite registrado como propiedad de una cache.</param>
        public static void RegisterOwnedCachedSprite(Sprite sprite)
        {
            if (sprite != null) ownedCachedSprites.Add(sprite);
        }

        /// <summary>
        /// Quita un Sprite del registro de sprites con dueno. Llamar justo antes
        /// de destruirlo desde la cache que lo creo.
        /// </summary>
        /// <param name="sprite">Sprite a dejar sin registrar.</param>
        public static void UnregisterOwnedCachedSprite(Sprite sprite)
        {
            if (sprite != null) ownedCachedSprites.Remove(sprite);
        }

        /// <summary>
        /// Destruye un Sprite creado en tiempo de ejecucion sin tocar su textura.
        /// Usa esto en el teardown de paneles: las texturas vienen de caches
        /// compartidas (roundedTextureCache, sharedWhiteTexture) y solo
        /// <see cref="ClearTextureCache"/> puede destruirlas. Si la textura es
        /// propiedad exclusiva del sprite, destruirla en el sitio que la creo.
        /// </summary>
        /// <param name="sprite">Sprite a destruir (acepta null).</param>
        public static void SafeDestroySprite(Sprite sprite)
        {
            if (sprite == null) return;
            UnityEngine.Object.Destroy(sprite);
        }

        /// <summary>
        /// Destruye los Sprite (nunca las texturas) de todos los Image de un panel
        /// que va a ser destruido. Los sprites con dueno (registrados via
        /// RegisterOwnedCachedSprite) se omiten sin blanquear la referencia del
        /// Image, porque su cache sigue usandolos en otros paneles. Llamar SIEMPRE
        /// antes de Destroy/DestroyImmediate del panel para que los sprites no
        /// queden huerfanos.
        /// </summary>
        /// <param name="root">Panel o GameObject raiz a despedir.</param>
        public static void SafeDestroyPanelSprites(GameObject root)
        {
            if (root == null) return;
            var images = root.GetComponentsInChildren<Image>(true);
            var processed = new HashSet<Sprite>();
            foreach (var img in images)
            {
                if (img == null) continue;
                Sprite sprite = img.sprite;
                if (sprite == null) continue;
                // Los sprites con dueno no se tocan: la cache que los creo es la
                // responsable de destruirlos. Solo los que se van a destruir se
                // blanquean del Image (L3).
                if (ownedCachedSprites.Contains(sprite)) continue;
                img.sprite = null;
                if (!processed.Add(sprite)) continue; // mismo sprite en varios Image
                SafeDestroySprite(sprite);
            }
        }

        /// <summary>
        /// Obtiene la fuente del sistema (Helvetica Neue en macOS, Arial como fallback).
        /// Helvetica Neue es mas legible y moderna que Arial, con mejor soporte Bold.
        /// La fuente se crea una sola vez y se reutiliza en todo el proyecto.
        /// El tamano se controla via <c>text.fontSize</c> en cada componente Text.
        /// </summary>
        /// <param name="size">Ignorado (se usa fontSizede cada Text).</param>
        /// <returns>Fuente unica cacheada.</returns>
        public static Font GetFont(int size = 14)
        {
            if (cachedFont != null) return cachedFont;
            cachedFont = Font.CreateDynamicFontFromOSFont("Helvetica Neue", 14);
            if (cachedFont == null)
                cachedFont = Font.CreateDynamicFontFromOSFont("Arial", 14);
            if (cachedFont == null)
                cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return cachedFont;
        }

        /// <summary>
        /// Construye un ColorBlock para un boton a partir de su color normal.
        /// Los colores de hover, presionado y deshabilitado se derivan automaticamente.
        /// </summary>
        /// <param name="normalColor">Color base del boton en estado normal.</param>
        /// <returns>ColorBlock configurado con los colores derivados.</returns>
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

        /// <summary>
        /// Crea un texto informativo centrado en una posicion dada del Canvas.
        /// </summary>
        /// <param name="parent">Transform padre del Canvas.</param>
        /// <param name="text">Contenido del texto.</param>
        /// <param name="position">Posicion anclada en el Canvas.</param>
        /// <param name="font">Fuente a usar.</param>
        /// <param name="fontSize">Tamano de fuente.</param>
        /// <param name="color">Color del texto.</param>
        /// <param name="bold">Si el texto debe mostrarse en negrita.</param>
        /// <returns>Componente Text creado.</returns>
        public static Text CreateInfoText(Transform parent, string text, Vector2 position, Font font, int fontSize, Color color, bool bold)
        {
            var textObj = new GameObject("InfoText");
            textObj.transform.SetParent(parent, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = position;
            textRect.sizeDelta = new Vector2(300, 28);

            var textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.color = color;
            textComponent.fontSize = fontSize;
            textComponent.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.font = font;

            return textComponent;
        }

        /// <summary>
        /// Aplica el estilo de titulo (negrita + color primario) a un componente Text existente.
        /// </summary>
        /// <param name="text">Componente Text a modificar.</param>
        public static void ApplyTitleStyle(Text text)
        {
            if (text != null)
            {
                text.fontStyle = FontStyle.Bold;
                text.color = Colors.textPrimary;
            }
        }

        /// <summary>
        /// Crea un boton circular pequeno con el texto "i" para informacion.
        /// </summary>
        /// <param name="parent">Transform padre del Canvas.</param>
        /// <param name="position">Posicion anclada del boton.</param>
        /// <param name="font">Fuente para el texto "i".</param>
        /// <returns>Componente Button creado.</returns>
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

        /// <summary>
        /// Crea un panel de menu redondeado con el estilo visual por defecto.
        /// </summary>
        /// <param name="parent">Transform padre del Canvas.</param>
        /// <param name="name">Nombre del GameObject.</param>
        /// <param name="size">Dimensiones del panel.</param>
        /// <returns>GameObject del panel creado.</returns>
        public static GameObject CreateMenuPanel(Transform parent, string name, Vector2 size)
        {
            return CreateRoundedPanel(parent, size, 25, Colors.surfacePanel, Colors.borderAccent, name);
        }

        /// <summary>
        /// Crea un panel con esquinas redondeadas, color de relleno y borde opcional.
        /// </summary>
        /// <param name="parent">Transform padre del Canvas.</param>
        /// <param name="size">Dimensiones del panel.</param>
        /// <param name="cornerRadius">Radio de las esquinas redondeadas.</param>
        /// <param name="backgroundColor">Color de relleno; si es null usa surfacePanel.</param>
        /// <param name="borderColor">Color del borde; si es null usa borderAccent.</param>
        /// <param name="name">Nombre del GameObject.</param>
        /// <returns>GameObject del panel redondeado.</returns>
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

        /// <summary>
        /// Crea un texto de titulo centrado con el color de acento del simulador.
        /// </summary>
        /// <param name="parent">Transform padre del Canvas.</param>
        /// <param name="title">Contenido del titulo.</param>
        /// <param name="fontSize">Tamano de fuente.</param>
        /// <param name="position">Posicion anclada en el Canvas.</param>
        /// <param name="font">Fuente a usar.</param>
        /// <returns>Componente Text del titulo.</returns>
        public static Text CreateMenuTitle(Transform parent, string title, int fontSize, Vector2 position, Font font)
        {
            var titleObj = new GameObject("MenuTitle");
            titleObj.transform.SetParent(parent, false);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = position;
            titleRect.sizeDelta = new Vector2(700, 55);

            var titleText = titleObj.AddComponent<Text>();
            titleText.text = title;
            titleText.color = Colors.textAccent;
            titleText.fontSize = fontSize;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.font = font;

            return titleText;
        }

        /// <summary>
        /// Crea un boton de menu redondeado con texto centrado y colores del simulador.
        /// </summary>
        /// <param name="parent">Transform padre del Canvas.</param>
        /// <param name="name">Nombre del GameObject del boton.</param>
        /// <param name="label">Texto a mostrar en el boton.</param>
        /// <param name="position">Posicion anclada del boton.</param>
        /// <param name="size">Dimensiones del boton.</param>
        /// <param name="font">Fuente del texto.</param>
        /// <param name="fontSize">Tamano de fuente (por defecto 18).</param>
        /// <param name="cornerRadius">Radio de esquinas redondeadas (por defecto 12).</param>
        /// <param name="borderWidth">Grosor del borde en pixeles (0 = sin borde, por defecto 1.5).</param>
        /// <returns>Componente Button creado.</returns>
        public static Button CreateMenuButton(Transform parent, string name, string label, Vector2 position, Vector2 size, Font font, int fontSize = 18, int cornerRadius = 12, float borderWidth = 1.5f)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            var rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var img = btnObj.AddComponent<Image>();
            Texture2D btnTex = CreateRoundedRectTexture((int)size.x, (int)size.y, cornerRadius, Colors.buttonNormal, Colors.borderAccent, borderWidth);
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
            text.horizontalOverflow = HorizontalWrapMode.Overflow;

            return btn;
        }

        /// <summary>
        /// Crea un boton pequeno sin bordes redondeados, util para acciones secundarias.
        /// </summary>
        /// <param name="parent">Transform padre del Canvas.</param>
        /// <param name="name">Nombre del GameObject del boton.</param>
        /// <param name="label">Texto del boton.</param>
        /// <param name="position">Posicion anclada del boton.</param>
        /// <param name="width">Ancho del boton.</param>
        /// <param name="height">Alto del boton.</param>
        /// <param name="font">Fuente del texto.</param>
        /// <returns>Componente Button creado.</returns>
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

        /// <summary>
        /// Crea un fondo semitransparente que destruye el panel indicado al hacer clic fuera de el.
        /// </summary>
        /// <param name="canvasTransform">Transform del Canvas raiz.</param>
        /// <param name="panelName">Nombre del GameObject del panel a cerrar.</param>
        /// <returns>GameObject del fondo de cierre.</returns>
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
                if (panel != null)
                {
                    // Liberar los Sprite del panel y del fondo antes de destruirlos
                    SafeDestroyPanelSprites(panel);
                    GameObject.Destroy(panel);
                }
                SafeDestroyPanelSprites(bgObj);
                GameObject.Destroy(bgObj);
            });

            return bgObj;
        }

        /// <summary>
        /// Genera una textura con esquinas redondeadas y borde opcional.
        /// Utiliza el cache interno para reutilizar texturas identicas.
        /// Utiliza distancia a rectangulo redondeado para determinar cada pixel.
        /// </summary>
        /// <param name="width">Ancho de la textura en pixeles.</param>
        /// <param name="height">Alto de la textura en pixeles.</param>
        /// <param name="cornerRadius">Radio de las esquinas redondeadas.</param>
        /// <param name="fillColor">Color de relleno interior.</param>
        /// <param name="borderColor">Color del borde (por defecto transparente).</param>
        /// <param name="borderWidth">Grosor del borde en pixeles (0 = sin borde).</param>
        /// <returns>Texture2D con esquinas redondeadas (del cache si ya existe).</returns>
        public static Texture2D CreateRoundedRectTexture(int width, int height, int cornerRadius, Color fillColor, Color borderColor = default, float borderWidth = 0f)
        {
            // Generar clave unica para el cache
            string cacheKey = string.Format("{0}_{1}_{2}_{3:F4}_{4:F4}_{5:F4}_{6:F4}_{7:F4}_{8:F4}_{9:F4}_{10:F4}_{11:F4}",
                width, height, cornerRadius,
                fillColor.r, fillColor.g, fillColor.b, fillColor.a,
                borderColor.r, borderColor.g, borderColor.b, borderColor.a,
                borderWidth);

            // Reutilizar textura del cache si existe
            if (roundedTextureCache.TryGetValue(cacheKey, out var cachedTex) && cachedTex != null)
                return cachedTex;

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

            // Almacenar en cache para reutilizar
            roundedTextureCache[cacheKey] = tex;

            return tex;
        }

        /// <summary>
        /// Calcula la distancia desde un punto al borde de un rectangulo redondeado.
        /// Usada internamente por CreateRoundedRectTexture para el pintado pixel a pixel.
        /// </summary>
        /// <param name="point">Punto a evaluar.</param>
        /// <param name="center">Centro del rectangulo.</param>
        /// <param name="width">Ancho del rectangulo.</param>
        /// <param name="height">Alto del rectangulo.</param>
        /// <param name="radius">Radio de las esquinas.</param>
        /// <returns>Distancia al borde: 0 si esta dentro, positiva si esta fuera.</returns>
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

        /// <summary>
        /// Crea un Sprite con esquinas redondeadas a partir de una textura generada.
        /// </summary>
        /// <param name="width">Ancho del sprite en pixeles.</param>
        /// <param name="height">Alto del sprite en pixeles.</param>
        /// <param name="cornerRadius">Radio de las esquinas redondeadas.</param>
        /// <param name="fillColor">Color de relleno.</param>
        /// <param name="borderColor">Color del borde.</param>
        /// <param name="borderWidth">Grosor del borde en pixeles.</param>
        /// <returns>Sprite redondeado listo para usar en Image.</returns>
        public static Sprite CreateRoundedRectSprite(int width, int height, int cornerRadius, Color fillColor, Color borderColor = default, float borderWidth = 2f)
        {
            Texture2D tex = CreateRoundedRectTexture(width, height, cornerRadius, fillColor, borderColor, borderWidth);
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100);
        }

        // Cache de sprites de dispositivo: key = "size_colorR_colorG_colorB_antiAlias"
        private static Dictionary<string, Sprite> deviceIconCache = new Dictionary<string, Sprite>();

        /// <summary>
        /// Crea un icono redondo para representar un dispositivo de red.
        /// Soporta anti-aliasing para bordes suaves. Los resultados se cachean
        /// por combinacion de parametros.
        /// </summary>
        /// <param name="size">Tamano del icono en pixeles.</param>
        /// <param name="color">Color del icono.</param>
        /// <param name="antiAlias">Si debe aplicar suavizado de bordes.</param>
        /// <returns>Sprite circular del dispositivo.</returns>
        public static Sprite CreateDeviceIcon(int size, Color color, bool antiAlias = true)
        {
            string cacheKey = $"{size}_{color.r:F4}_{color.g:F4}_{color.b:F4}_{antiAlias}";
            if (deviceIconCache.TryGetValue(cacheKey, out var cached) && cached != null)
                return cached;

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
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            deviceIconCache[cacheKey] = sprite;
            // La cache es duena del sprite y su textura: protegerlo del teardown de paneles
            RegisterOwnedCachedSprite(sprite);
            return sprite;
        }

        /// <summary>
        /// Crea una columna informativa de texto con alineacion superior izquierda.
        /// </summary>
        /// <param name="parent">Transform padre del Canvas.</param>
        /// <param name="position">Posicion anclada de la columna.</param>
        /// <param name="width">Ancho de la columna.</param>
        /// <param name="height">Alto de la columna.</param>
        /// <param name="font">Fuente del texto.</param>
        /// <param name="text">Contenido del texto.</param>
        /// <returns>GameObject de la columna con componente Text.</returns>
        public static GameObject CreateInfoColumn(Transform parent, Vector2 position, float width, float height, Font font, string text, int fontSize = 14)
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
            colText.fontSize = fontSize;
            colText.alignment = TextAnchor.UpperLeft;
            colText.font = font;
            return colObj;
        }

        /// <summary>
        /// Crea un campo de entrada de texto con fondo, texto y placeholder configurados.
        /// </summary>
        /// <param name="parent">Transform padre del Canvas.</param>
        /// <param name="name">Nombre del GameObject del campo.</param>
        /// <param name="position">Posicion anclada del campo.</param>
        /// <param name="size">Dimensiones del campo.</param>
        /// <param name="defaultValue">Valor inicial del campo.</param>
        /// <param name="placeholder">Texto de placeholder cuando esta vacio.</param>
        /// <param name="font">Fuente del texto.</param>
        /// <param name="fontSize">Tamano de fuente.</param>
        /// <returns>Componente InputField creado.</returns>
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
            inputText.raycastTarget = false;

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
            phText.raycastTarget = false;

            var inputField = inputObj.AddComponent<InputField>();
            inputField.textComponent = inputText;
            inputField.placeholder = phText;
            inputField.text = defaultValue;

            return inputField;
        }
    }
}
