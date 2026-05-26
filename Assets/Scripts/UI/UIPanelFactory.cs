using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using SimRedes.Network;
using SimRedes.Simulation;
using UIComp = SimRedes.UI.UIComponents;
using UIColors = SimRedes.UI.UIComponents.Colors;

namespace SimRedes.UI
{
    public static class UIPanelFactory
    {
        public struct ConnectivityPanelRefs
        {
            public GameObject panelObj;
            public Text sourceText;
            public Text destText;
            public Text resultText;
            public Text resultIcon;
            public Button pingButton;
            public Text statusText;
        }

        public static Font GetFont(int size = 14)
        {
            Font font = Font.CreateDynamicFontFromOSFont("Arial", size);
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font;
        }

        internal static void CreateNumericKeypad(Transform parent, float startY, Font font, Font bigFont, InputField ipField, InputField maskField)
        {
            string[] keys = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", ".", "DEL" };
            float keySize = 54;
            float keySpacing = 62;
            float startX = -(keySpacing * 1.5f);

            for (int i = 0; i < keys.Length; i++)
            {
                int row = i / 3;
                int col = i % 3;
                float x = startX + (col * keySpacing);
                float y = startY - (row * keySpacing);

                var keyObj = new GameObject("Key_" + keys[i]);
                keyObj.transform.SetParent(parent, false);
                var keyRect = keyObj.AddComponent<RectTransform>();
                keyRect.anchorMin = new Vector2(0.5f, 0.5f);
                keyRect.anchorMax = new Vector2(0.5f, 0.5f);
                keyRect.anchoredPosition = new Vector2(x, y);
                keyRect.sizeDelta = new Vector2(keySize, keySize);

                var keyImg = keyObj.AddComponent<Image>();
                Texture2D keyTex = UIComp.CreateRoundedRectTexture((int)keySize, (int)keySize, 10, UIColors.buttonNormal, UIColors.borderAccent, 1f);
                keyImg.sprite = Sprite.Create(keyTex, new Rect(0, 0, (int)keySize, (int)keySize), new Vector2(0.5f, 0.5f), 100);
                keyImg.type = Image.Type.Sliced;

                var keyBtn = keyObj.AddComponent<Button>();
                keyBtn.targetGraphic = keyImg;
                keyBtn.colors = UIComp.GetButtonColors(UIColors.buttonNormal);

                var keyTextObj = new GameObject("Text");
                keyTextObj.transform.SetParent(keyObj.transform, false);
                var keyTextRect = keyTextObj.AddComponent<RectTransform>();
                keyTextRect.anchorMin = Vector2.zero;
                keyTextRect.anchorMax = Vector2.one;
                keyTextRect.offsetMin = Vector2.zero;
                keyTextRect.offsetMax = Vector2.zero;

                var keyText = keyTextObj.AddComponent<Text>();
                keyText.text = keys[i];
                keyText.color = UIColors.textPrimary;
                keyText.fontSize = keys[i] == "DEL" ? 10 : 20;
                keyText.alignment = TextAnchor.MiddleCenter;
                keyText.font = keys[i] == "DEL" ? font : bigFont;

                string captured = keys[i];
                keyBtn.onClick.AddListener(() =>
                {
                    InputField activeField = ipField.isFocused ? ipField : maskField;
                    if (captured == "DEL")
                    {
                        if (activeField.text.Length > 0)
                            activeField.text = activeField.text.Substring(0, activeField.text.Length - 1);
                    }
                    else if (captured == ".")
                    {
                        char lastChar = activeField.text.Length > 0 ? activeField.text[activeField.text.Length - 1] : ' ';
                        if (lastChar != '.' && lastChar != ' ')
                            activeField.text = activeField.text + captured;
                    }
                    else
                    {
                        if (activeField.text.Length < 18)
                            activeField.text = activeField.text + captured;
                    }
                });
            }
        }

        public static GameObject CreateScorePanel(Transform canvas)
        {
            Font font = GetFont(18);
            Font bigFont = GetFont(28);

            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(180, 90), 15,
                UIColors.surfacePanel, UIColors.borderAccent, "ScorePanel");

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0f);
            panelRect.pivot = new Vector2(1f, 0f);
            panelRect.anchoredPosition = new Vector2(-10, 10);

            UIComp.CreateInfoText(panelObj.transform, "Puntaje", new Vector2(0, 18), font, 18, UIColors.textSecondary, false);
            UIComp.CreateInfoText(panelObj.transform, "0", new Vector2(0, -5), bigFont, 28, UIColors.textAccent, true);

            return panelObj;
        }

        public static GameObject CreateDevicesPanel(Transform canvas, int nodeCount,
            Action<int> onItemClicked)
        {
            Font font = GetFont(14);

            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(220, 400), 20,
                UIColors.surfacePanel, UIColors.borderAccent, "DevicesPanel");

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(10, -10);

            UIComp.CreateMenuTitle(panelObj.transform, "Dispositivos", 18, new Vector2(0, 170), font);

            return panelObj;
        }

        public static ConnectivityPanelRefs CreateConnectivityPanel(Transform canvas,
            Action onBack = null)
        {
            Font font = GetFont();
            Font bigFont = GetFont(16);

            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(550, 500), 25,
                UIColors.surfacePanel, UIColors.borderAccent, "ConnectivityPanel");

            UIComp.CreateMenuTitle(panelObj.transform, "Test de Conectividad", 28, new Vector2(0, 195), bigFont);

            // Source
            var sourceObj = new GameObject("SourceNode");
            sourceObj.transform.SetParent(panelObj.transform, false);
            var sourceRect = sourceObj.AddComponent<RectTransform>();
            sourceRect.anchorMin = new Vector2(0.5f, 0.5f);
            sourceRect.anchorMax = new Vector2(0.5f, 0.5f);
            sourceRect.anchoredPosition = new Vector2(-120, 100);
            sourceRect.sizeDelta = new Vector2(150, 40);
            var sourceText = sourceObj.AddComponent<Text>();
            sourceText.text = "Origen: -";
            sourceText.color = UIColors.textPrimary;
            sourceText.fontSize = 14;
            sourceText.alignment = TextAnchor.MiddleLeft;
            sourceText.font = font;

            // Arrow
            var arrowObj = new GameObject("Arrow");
            arrowObj.transform.SetParent(panelObj.transform, false);
            var arrowRect = arrowObj.AddComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(0.5f, 0.5f);
            arrowRect.anchorMax = new Vector2(0.5f, 0.5f);
            arrowRect.anchoredPosition = new Vector2(0, 100);
            arrowRect.sizeDelta = new Vector2(60, 30);
            var arrowText = arrowObj.AddComponent<Text>();
            arrowText.text = "→ → →";
            arrowText.color = UIColors.textAccent;
            arrowText.fontSize = 20;
            arrowText.alignment = TextAnchor.MiddleCenter;
            arrowText.font = font;

            // Destination
            var destObj = new GameObject("DestNode");
            destObj.transform.SetParent(panelObj.transform, false);
            var destRect = destObj.AddComponent<RectTransform>();
            destRect.anchorMin = new Vector2(0.5f, 0.5f);
            destRect.anchorMax = new Vector2(0.5f, 0.5f);
            destRect.anchoredPosition = new Vector2(120, 100);
            destRect.sizeDelta = new Vector2(150, 40);
            var destText = destObj.AddComponent<Text>();
            destText.text = "Destino: -";
            destText.color = UIColors.textPrimary;
            destText.fontSize = 14;
            destText.alignment = TextAnchor.MiddleRight;
            destText.font = font;

            // Ping button (listeners added by ConnectivityTestPanel caller)
            Button pingBtn = UIComp.CreateMenuButton(panelObj.transform, "PingButton", "HACER PING",
                new Vector2(0, 40), new Vector2(180, 55), font);

            // Result icon
            var resultIconObj = new GameObject("ResultDisplay");
            resultIconObj.transform.SetParent(panelObj.transform, false);
            var resultIconRect = resultIconObj.AddComponent<RectTransform>();
            resultIconRect.anchorMin = new Vector2(0.5f, 0.5f);
            resultIconRect.anchorMax = new Vector2(0.5f, 0.5f);
            resultIconRect.anchoredPosition = new Vector2(0, -30);
            resultIconRect.sizeDelta = new Vector2(60, 60);

            var resultIconImg = resultIconObj.AddComponent<Image>();
            Texture2D resultTex = UIComp.CreateRoundedRectTexture(60, 60, 15, new Color(0.5f, 0.5f, 0.5f, 0.3f), UIColors.borderAccent, 2f);
            resultIconImg.sprite = Sprite.Create(resultTex, new Rect(0, 0, 60, 60), new Vector2(0.5f, 0.5f), 100);
            resultIconImg.type = Image.Type.Sliced;

            var resultIconInnerObj = new GameObject("ResultIcon");
            resultIconInnerObj.transform.SetParent(resultIconObj.transform, false);
            var resultIconInnerRect = resultIconInnerObj.AddComponent<RectTransform>();
            resultIconInnerRect.anchorMin = Vector2.zero;
            resultIconInnerRect.anchorMax = Vector2.one;
            resultIconInnerRect.offsetMin = new Vector2(10, 10);
            resultIconInnerRect.offsetMax = new Vector2(-10, -10);
            var resultIconInner = resultIconInnerObj.AddComponent<Text>();
            resultIconInner.text = "?";
            resultIconInner.color = UIColors.textSecondary;
            resultIconInner.fontSize = 28;
            resultIconInner.fontStyle = FontStyle.Bold;
            resultIconInner.alignment = TextAnchor.MiddleCenter;
            resultIconInner.font = font;

            // Result text
            var resultTextObj = new GameObject("ResultText");
            resultTextObj.transform.SetParent(panelObj.transform, false);
            var resultTextRect = resultTextObj.AddComponent<RectTransform>();
            resultTextRect.anchorMin = new Vector2(0.5f, 0.5f);
            resultTextRect.anchorMax = new Vector2(0.5f, 0.5f);
            resultTextRect.anchoredPosition = new Vector2(0, -95);
            resultTextRect.sizeDelta = new Vector2(450, 40);
            var resultText = resultTextObj.AddComponent<Text>();
            resultText.text = "Presiona PING para probar conectividad";
            resultText.color = UIColors.textSecondary;
            resultText.fontSize = 16;
            resultText.alignment = TextAnchor.MiddleCenter;
            resultText.font = font;

            // Status text
            var statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(panelObj.transform, false);
            var statusRect = statusObj.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.5f, 0.5f);
            statusRect.anchorMax = new Vector2(0.5f, 0.5f);
            statusRect.anchoredPosition = new Vector2(0, -140);
            statusRect.sizeDelta = new Vector2(400, 30);
            var statusText = statusObj.AddComponent<Text>();
            statusText.text = "Pings: 0 | Exitosos: 0 | Fallidos: 0";
            statusText.color = UIColors.textSecondary;
            statusText.fontSize = 12;
            statusText.alignment = TextAnchor.MiddleCenter;
            statusText.font = font;

            // Hint text
            var hintObj = new GameObject("HintText");
            hintObj.transform.SetParent(panelObj.transform, false);
            var hintRect = hintObj.AddComponent<RectTransform>();
            hintRect.anchorMin = new Vector2(0.5f, 0.5f);
            hintRect.anchorMax = new Vector2(0.5f, 0.5f);
            hintRect.anchoredPosition = new Vector2(0, -175);
            hintRect.sizeDelta = new Vector2(400, 25);
            var hintText = hintObj.AddComponent<Text>();
            hintText.text = "Tecla P = Ping | C = Limpiar";
            hintText.color = UIColors.textSecondary;
            hintText.fontSize = 11;
            hintText.alignment = TextAnchor.MiddleCenter;
            hintText.font = font;

            // Back button
            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "VOLVER",
                new Vector2(0, -215), new Vector2(160, 45), font);
            backBtn.onClick.AddListener(() => onBack?.Invoke());

            return new ConnectivityPanelRefs
            {
                panelObj = panelObj,
                sourceText = sourceText,
                destText = destText,
                resultText = resultText,
                resultIcon = resultIconInner,
                pingButton = pingBtn,
                statusText = statusText
            };
        }

        public static GameObject CreateInstructionsPanel(Transform canvas,
            Action onBack)
        {
            Font font = GetFont();
            Font bigFont = GetFont(20);

            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(900, 720), 25,
                UIColors.surfacePanel, UIColors.borderAccent, "InstructionsPanel");

            UIComp.CreateMenuTitle(panelObj.transform, "Como Usar", 32, new Vector2(0, 320), bigFont);

            // ─── COLUMNA IZQUIERDA: Controles y Navegacion ───
            string leftText =
                "TECLADO (DEBUG)\n" +
                "───────────────\n" +
                "1 = Agregar Router\n" +
                "2 = Agregar Switch\n" +
                "3 = Agregar PC\n" +
                "4 = Modo CONEXION\n" +
                "5 = Agregar Fallo\n" +
                "C = Limpiar todo\n" +
                "P = Hacer Ping\n" +
                "R = Eliminar selec.\n" +
                "ESC = Volver / Menu\n\n" +
                "En la mesa IDEUM usa\n" +
                "discos fisicos en vez\n" +
                "de teclas numericas.";

            // ─── COLUMNA CENTRAL: Actividades ───
            string centerText =
                "ACTIVIDADES\n" +
                "───────────\n" +
                "0) Construir Topologia:\n" +
                "  crea redes y descubre\n" +
                "  el tipo de topologia.\n\n" +
                "1) Encontrar Fallos:\n" +
                "  identifica y resuelve\n" +
                "  problemas en la red.\n\n" +
                "2) Tablas Enrutamiento:\n" +
                "  visualiza las tablas\n" +
                "  de los routers.\n\n" +
                "3) Mejor Ruta:\n" +
                "  elige la ruta optima\n" +
                "  hacia un destino.\n\n" +
                "4) Enrutamiento Estatico:\n" +
                "  agrega rutas manuales.\n\n" +
                "5) Enrutamiento Dinamico:\n" +
                "  simula RIP y OSPF.\n\n" +
                "6) Escenarios:\n" +
                "  desafia con casos\n" +
                "  preconfigurados.";

            // ─── COLUMNA DERECHA: Conceptos ───
            string rightText =
                "CONCEPTOS\n" +
                "─────────\n" +
                "Router: dirige trafico\n" +
                "entre distintas redes.\n\n" +
                "Switch: conecta dispositivos\n" +
                "en la misma red local.\n\n" +
                "PC: dispositivo final que\n" +
                "envia y recibe datos.\n\n" +
                "Enlace: conexion entre\n" +
                "dos dispositivos.\n\n" +
                "Ping: prueba si dos nodos\n" +
                "pueden comunicarse.\n\n" +
                "Topologia: forma en que\n" +
                "los nodos se conectan\n" +
                "(Estrella, Bus, etc.).\n\n" +
                "Usa los botones en cada\n" +
                "actividad para operar.";

            UIComp.CreateInfoColumn(panelObj.transform, new Vector2(-280, 40), 270, 460, font, leftText);
            UIComp.CreateInfoColumn(panelObj.transform, new Vector2(0, 40), 270, 460, font, centerText);
            UIComp.CreateInfoColumn(panelObj.transform, new Vector2(280, 40), 270, 460, font, rightText);

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "VOLVER", new Vector2(0, -330), new Vector2(200, 50), font);
            backBtn.onClick.AddListener(() => onBack?.Invoke());

            // Configurar MenuNavigator para que ESC funcione en el panel de instrucciones
            var navigator = UnityEngine.Object.FindAnyObjectByType<MenuNavigator>();
            if (navigator != null)
                navigator.SetupPanel(panelObj, () => onBack?.Invoke());

            return panelObj;
        }

        public static GameObject CreateDiscLegendPanel(Transform canvas, Action onBack)
        {
            Font font = GetFont();
            Font bigFont = GetFont(20);

            // Panel: 820x1060 da espacio holgado para título + 3 secciones + hint + botón
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(820, 1060), 25,
                UIColors.surfacePanel, UIColors.borderAccent, "DiscLegendPanel");

            UIComp.CreateMenuTitle(panelObj.transform, "Leyenda de Discos", 30, new Vector2(0, 400), bigFont);

            float spacing = 26f;
            float sectionGap = 30f;
            float xHeader = 0f;     // X para títulos de sección (MidLeft, ancho 500 → centrado)
            float xCircle = -260f;  // X para círculos de color (izquierda)
            float xLabel  = -150f;  // X para labels (MidLeft, ancho 180 → texto empieza en -240, a la derecha del círculo)
            float xDesc   = 110f;   // X para descripciones (MidLeft, ancho 320 → texto empieza en -50)

            // Sección 1: Físicos (1-3) — header en y=270, items bajan hasta ~y=164
            CreateLegendSection(panelObj.transform, "DISCOS F\u00cdSICOS (Mesa IDEUM)", new[] { 1, 2, 3 },
                270, spacing, font, xHeader, xCircle, xLabel, xDesc);

            // Sección 2: Actividades (4-6) — header en y≈142, items hasta y≈44
            float sec2Header = 270 - (3 * spacing + 20f) - sectionGap;
            CreateLegendSection(panelObj.transform, "MANEJADOS POR ACTIVIDADES", new[] { 4, 5, 6 },
                sec2Header, spacing, font, xHeader, xCircle, xLabel, xDesc);

            // Sección 3: Routing virtual (7-18) — header en y≈14, items hasta y≈-318
            float sec3Header = sec2Header - (3 * spacing + 20f) - sectionGap;
            CreateLegendSection(panelObj.transform, "CONFIGURACI\u00d3N DE ROUTING (Botones en actividades)",
                new int[] { 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
                sec3Header, spacing, font, xHeader, xCircle, xLabel, xDesc);

            // Hint bien separado del último item de la sección 3
            float sec3Bottom = sec3Header - (12 * spacing + 20f);
            float hintY = sec3Bottom - 70f;
            string hintText = "NOTA: Solo los discos 1-3 (Router/Switch/PC) son f\u00edsicos en la mesa IDEUM.\n" +
                "Enlaces, fallos y configuraci\u00f3n de routing se manejan desde las actividades.\n" +
                "En Actividad 4 (Enrutamiento Est\u00e1tico): presiona A\u00d1ADIR MANUAL para rutas personalizadas\n" +
                "o A\u00d1ADIR RUTA para rutas de ejemplo. Las rutas se escriben directamente en el Router.";
            var hintObj = new GameObject("UsageHint");
            hintObj.transform.SetParent(panelObj.transform, false);
            var hintRect = hintObj.AddComponent<RectTransform>();
            hintRect.anchorMin = new Vector2(0.5f, 0.5f);
            hintRect.anchorMax = new Vector2(0.5f, 0.5f);
            hintRect.anchoredPosition = new Vector2(0, hintY);
            hintRect.sizeDelta = new Vector2(760, 70);
            var hintTextComp = hintObj.AddComponent<Text>();
            hintTextComp.text = hintText;
            hintTextComp.color = new Color(1f, 0.7f, 0.2f);
            hintTextComp.fontSize = 12;
            hintTextComp.alignment = TextAnchor.UpperLeft;
            hintTextComp.font = font;
            hintTextComp.lineSpacing = 1.4f;

            // Botón VOLVER, bien separado del hint
            float backY = hintY - 75f;
            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "VOLVER", new Vector2(0, backY), new Vector2(200, 45), font);
            backBtn.onClick.AddListener(() => onBack?.Invoke());

            // Configurar ESC
            var navigator = UnityEngine.Object.FindAnyObjectByType<MenuNavigator>();
            if (navigator != null)
                navigator.SetupPanel(panelObj, () => onBack?.Invoke());

            return panelObj;
        }

        private static void CreateLegendSection(Transform parent, string sectionName, int[] discIds,
            float headerY, float spacing, Font font,
            float xHeader, float xCircle, float xLabel, float xDesc)
        {
            // Título de sección
            var headerObj = new GameObject("SectionHeader");
            headerObj.transform.SetParent(parent, false);
            var headerRect = headerObj.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.5f, 0.5f);
            headerRect.anchorMax = new Vector2(0.5f, 0.5f);
            headerRect.anchoredPosition = new Vector2(xHeader, headerY);
            headerRect.sizeDelta = new Vector2(500, 22);
            var headerText = headerObj.AddComponent<Text>();
            headerText.text = sectionName;
            headerText.color = UIColors.textAccent;
            headerText.fontSize = 14;
            headerText.fontStyle = FontStyle.Bold;
            headerText.alignment = TextAnchor.MiddleLeft;
            headerText.font = font;

            // Items de la sección
            for (int i = 0; i < discIds.Length; i++)
            {
                int discId = discIds[i];
                var config = DiscConfiguration.GetConfiguration(discId);
                float yPos = headerY - spacing * (i + 1) - 20f;

                // Círculo de color (usamos CreateRoundedRectSprite con cornerRadius=8 → círculo perfecto)
                var circleObj = new GameObject($"Disc_{discId}_Color");
                circleObj.transform.SetParent(parent, false);
                var circleRect = circleObj.AddComponent<RectTransform>();
                circleRect.anchorMin = new Vector2(0.5f, 0.5f);
                circleRect.anchorMax = new Vector2(0.5f, 0.5f);
                circleRect.anchoredPosition = new Vector2(xCircle, yPos);
                circleRect.sizeDelta = new Vector2(16, 16);
                var circleImg = circleObj.AddComponent<Image>();
                circleImg.sprite = UIComp.CreateRoundedRectSprite(16, 16, 8, config.DisplayColor, Color.clear, 0f);
                circleImg.color = Color.white;

                // Label (negrita)
                var labelObj = new GameObject($"Disc_{discId}_Label");
                labelObj.transform.SetParent(parent, false);
                var labelRect = labelObj.AddComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0.5f, 0.5f);
                labelRect.anchorMax = new Vector2(0.5f, 0.5f);
                labelRect.anchoredPosition = new Vector2(xLabel, yPos);
                labelRect.sizeDelta = new Vector2(180, 20);
                var labelText = labelObj.AddComponent<Text>();
                labelText.text = config.Label;
                labelText.color = UIColors.textPrimary;
                labelText.fontSize = 13;
                labelText.fontStyle = FontStyle.Bold;
                labelText.alignment = TextAnchor.MiddleLeft;
                labelText.font = font;

                // Descripción
                var descObj = new GameObject($"Disc_{discId}_Desc");
                descObj.transform.SetParent(parent, false);
                var descRect = descObj.AddComponent<RectTransform>();
                descRect.anchorMin = new Vector2(0.5f, 0.5f);
                descRect.anchorMax = new Vector2(0.5f, 0.5f);
                descRect.anchoredPosition = new Vector2(xDesc, yPos);
                descRect.sizeDelta = new Vector2(320, 20);
                var descText = descObj.AddComponent<Text>();
                descText.text = config.Description;
                descText.color = UIColors.textSecondary;
                descText.fontSize = 12;
                descText.alignment = TextAnchor.MiddleLeft;
                descText.font = font;
            }
        }

        public static void ToggleTopologyExamplePanel(Transform canvas, Font font)
        {
            var existingPanel = GameObject.Find("TopologyExamplePanel");
            if (existingPanel != null)
            {
                UnityEngine.Object.Destroy(existingPanel);
                return;
            }

            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(700, 600), 20,
                UIColors.surfacePanel, UIColors.textAccent);
            panelObj.name = "TopologyExamplePanel";

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;

            UIComp.CreateMenuTitle(panelObj.transform, "Tipos de Topologia", 26, new Vector2(0, 250), font);

            string[] topologyTypes = {
                "ESTRELLA: Un router central conectado a varios nodos perifericos",
                "BUS: Todos los nodos conectados a un cable principal",
                "ANILLO: Cada nodo conectado al siguiente formando un ciclo",
                "ARBOL: Estructura jerarquica con routers y switches",
                "MALLA: Cada nodo conectado a todos los demas"
            };

            float startY = 180;
            for (int i = 0; i < topologyTypes.Length; i++)
            {
                var textObj = new GameObject("TopologyType_" + i);
                textObj.transform.SetParent(panelObj.transform, false);
                var textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0.5f, 0.5f);
                textRect.anchorMax = new Vector2(0.5f, 0.5f);
                textRect.anchoredPosition = new Vector2(0, startY - (i * 45));
                textRect.sizeDelta = new Vector2(550, 40);

                var text = textObj.AddComponent<Text>();
                text.text = topologyTypes[i];
                text.color = UIColors.textPrimary;
                text.fontSize = 14;
                text.alignment = TextAnchor.MiddleLeft;
                text.font = font;
            }

            var instructionsObj = new GameObject("Instructions");
            instructionsObj.transform.SetParent(panelObj.transform, false);
            var instructionsRect = instructionsObj.AddComponent<RectTransform>();
            instructionsRect.anchorMin = new Vector2(0.5f, 0.5f);
            instructionsRect.anchorMax = new Vector2(0.5f, 0.5f);
            instructionsRect.anchoredPosition = new Vector2(0, -150);
            instructionsRect.sizeDelta = new Vector2(600, 150);

            var instructions = instructionsObj.AddComponent<Text>();
            instructions.text = "COMO CONSTRUIR:\n\n" +
                "1. Coloca 2 Routers (tecla 1), 1 Switch (tecla 2) y 1 PC (tecla 3)\n" +
                "2. Coloca un Enlace (tecla 4) entre cada par de dispositivos\n" +
                "3. Verifica que la topologia se detecte automaticamente\n" +
                "4. Usa el boton PING para probar conectividad\n\n" +
                "TIP: Acercar dispositivos crea enlaces automaticamente";
            instructions.color = UIColors.textSecondary;
            instructions.fontSize = 12;
            instructions.alignment = TextAnchor.UpperLeft;
            instructions.font = font;

            var closeBtnObj = new GameObject("CloseBtn");
            closeBtnObj.transform.SetParent(panelObj.transform, false);
            var closeBtnRect = closeBtnObj.AddComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(1f, 1f);
            closeBtnRect.anchorMax = new Vector2(1f, 1f);
            closeBtnRect.anchoredPosition = new Vector2(-15, -15);
            closeBtnRect.sizeDelta = new Vector2(30, 30);

            var closeBtnImg = closeBtnObj.AddComponent<Image>();
            Texture2D closeTex = UIComp.CreateRoundedRectTexture(30, 30, 8, new Color(0.7f, 0.2f, 0.2f), UIColors.borderAccent, 1.5f);
            closeBtnImg.sprite = Sprite.Create(closeTex, new Rect(0, 0, 30, 30), new Vector2(0.5f, 0.5f), 100);
            closeBtnImg.type = Image.Type.Sliced;

            var closeBtn = closeBtnObj.AddComponent<Button>();
            closeBtn.colors = UIComp.GetButtonColors(new Color(0.7f, 0.2f, 0.2f));
            closeBtn.onClick.AddListener(() => UnityEngine.Object.Destroy(panelObj));

            var closeTextObj = new GameObject("Text");
            closeTextObj.transform.SetParent(closeBtnObj.transform, false);
            var closeTextRect = closeTextObj.AddComponent<RectTransform>();
            closeTextRect.anchorMin = Vector2.zero;
            closeTextRect.anchorMax = Vector2.one;
            closeTextRect.offsetMin = Vector2.zero;
            closeTextRect.offsetMax = Vector2.zero;

            var closeText = closeTextObj.AddComponent<Text>();
            closeText.text = "X";
            closeText.color = UIColors.textPrimary;
            closeText.fontSize = 14;
            closeText.fontStyle = FontStyle.Bold;
            closeText.alignment = TextAnchor.MiddleCenter;
            closeText.font = font;

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "CERRAR", new Vector2(0, -255), new Vector2(140, 40), font);
            backBtn.onClick.AddListener(() => UnityEngine.Object.Destroy(panelObj));
        }

        public static GameObject CreateMainMenu(Transform canvas,
            Action onStartSimulation, Action onActivities, Action onConnectivity,
            Action onInstructions, Action onDiscLegend, Action onExit)
        {
            Font font = GetFont();

            // Destruir MenuNavigator anterior si existe para evitar duplicados
            var oldNav = UnityEngine.Object.FindAnyObjectByType<MenuNavigator>();
            if (oldNav != null) UnityEngine.Object.DestroyImmediate(oldNav.gameObject);

            var navigatorObj = new GameObject("MenuNavigator");
            navigatorObj.AddComponent<MenuNavigator>();

            // Reusar MainMenuManager existente si lo hay, en lugar de crear uno nuevo
            // (previene conflictos de singleton con destruccion diferida)
            MainMenuManager menuManager = UnityEngine.Object.FindAnyObjectByType<MainMenuManager>();
            if (menuManager == null)
            {
                var menuObj = new GameObject("MainMenuManager");
                menuManager = menuObj.AddComponent<MainMenuManager>();
            }

            GameObject mainMenuPanel = UIComp.CreateMenuPanel(canvas, "MainMenuPanel", new Vector2(800, 720));
            UIComp.CreateMenuTitle(mainMenuPanel.transform, "Simulador de Redes", 40, new Vector2(0, 250), font);

            float buttonWidth = 250, buttonHeight = 60, startY = 140;

            Button startBtn = UIComp.CreateMenuButton(mainMenuPanel.transform, "Btn_Iniciar", "INICIAR SIMULACION", new Vector2(0, startY), new Vector2(buttonWidth, buttonHeight), font);
            startBtn.onClick.AddListener(() => onStartSimulation?.Invoke());

            Button activitiesBtn = UIComp.CreateMenuButton(mainMenuPanel.transform, "Btn_Actividades", "ACTIVIDADES", new Vector2(0, startY - 80), new Vector2(buttonWidth, buttonHeight), font);
            activitiesBtn.onClick.AddListener(() => onActivities?.Invoke());

            Button connectivityBtn = UIComp.CreateMenuButton(mainMenuPanel.transform, "Btn_Conectividad", "PRUEBAS Y CONEXIONES", new Vector2(0, startY - 160), new Vector2(buttonWidth, buttonHeight), font);
            connectivityBtn.onClick.AddListener(() => onConnectivity?.Invoke());

            Button instructionsBtn = UIComp.CreateMenuButton(mainMenuPanel.transform, "Btn_Instrucciones", "COMO USAR", new Vector2(0, startY - 240), new Vector2(buttonWidth, buttonHeight), font);
            instructionsBtn.onClick.AddListener(() => onInstructions?.Invoke());

            Button discLegendBtn = UIComp.CreateMenuButton(mainMenuPanel.transform, "Btn_DiscLegend", "LEYENDA DE DISCOS", new Vector2(0, startY - 320), new Vector2(buttonWidth, buttonHeight), font);
            discLegendBtn.onClick.AddListener(() => onDiscLegend?.Invoke());

            Button exitBtn = UIComp.CreateMenuButton(mainMenuPanel.transform, "Btn_Salir", "SALIR", new Vector2(0, startY - 400), new Vector2(buttonWidth, buttonHeight), font);
            exitBtn.onClick.AddListener(() => onExit?.Invoke());

            menuManager.mainMenuPanel = mainMenuPanel;
            var navigator = navigatorObj.GetComponent<MenuNavigator>();
            navigator.SetupPanel(mainMenuPanel, () => {
                UnityEngine.Debug.Log("[Menu] ESC en menu principal");
            });
            mainMenuPanel.SetActive(true);
            return mainMenuPanel;
        }

        public static GameObject CreateActivitiesPanel(Transform canvas, Action<int> onSelectActivity, Action onBack)
        {
            Font font = GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(700, 650), 25,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "ActivitiesPanel";
            UIComp.CreateMenuTitle(panelObj.transform, "Selecciona una Actividad", 32, new Vector2(0, 260), font);

            float startY = 200;
            string[] activities = { "Construir Topologia", "Encontrar Fallos", "Tablas de Enrutamiento", "Mejor Ruta", "Enrutamiento Estatico", "Enrutamiento Dinamico", "Escenarios" };

            for (int i = 0; i < activities.Length; i++)
            {
                int capturedIndex = i;
                float yPos = startY - (i * 60);
                Button btn = UIComp.CreateMenuButton(panelObj.transform, "Activity_" + i, activities[i], new Vector2(0, yPos), new Vector2(280, 50), font, 16);
                btn.onClick.AddListener(() => onSelectActivity?.Invoke(capturedIndex));
            }

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "VOLVER AL MENU", new Vector2(0, -290), new Vector2(200, 50), font);
            backBtn.onClick.AddListener(() => onBack?.Invoke());

            var navigator = UnityEngine.Object.FindAnyObjectByType<MenuNavigator>();
            if (navigator != null)
                navigator.SetupPanel(panelObj, () => onBack?.Invoke());

            return panelObj;
        }

        internal static InputField CreateConfigField(Transform parent, string placeholder, float leftX, float rightX, float yPos, Font font)
        {
            var inputObj = new GameObject("ConfigField");
            inputObj.transform.SetParent(parent, false);
            var inputRect = inputObj.AddComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0.5f, 0.5f);
            inputRect.anchorMax = new Vector2(0.5f, 0.5f);
            inputRect.anchoredPosition = new Vector2((leftX + rightX) / 2, yPos);
            inputRect.sizeDelta = new Vector2(rightX - leftX, 35);

            var inputImg = inputObj.AddComponent<Image>();
            inputImg.color = new Color(0.2f, 0.25f, 0.3f, 0.9f);

            var inputField = inputObj.AddComponent<InputField>();
            inputField.targetGraphic = inputImg;

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(inputObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8, 3);
            textRect.offsetMax = new Vector2(-8, -3);
            var textComp = textObj.AddComponent<Text>();
            textComp.text = "";
            textComp.font = font;
            textComp.fontSize = 14;
            textComp.color = Color.white;
            textComp.alignment = TextAnchor.MiddleLeft;
            inputField.textComponent = textComp;

            var placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(inputObj.transform, false);
            var placeholderRect = placeholderObj.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = new Vector2(8, 3);
            placeholderRect.offsetMax = new Vector2(-8, -3);
            var placeholderComp = placeholderObj.AddComponent<Text>();
            placeholderComp.text = placeholder;
            placeholderComp.font = font;
            placeholderComp.fontSize = 14;
            placeholderComp.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            placeholderComp.alignment = TextAnchor.MiddleLeft;
            inputField.placeholder = placeholderComp;

            return inputField;
        }

        internal static GameObject CreateDropdown(Transform parent, System.Collections.Generic.List<string> options, Font font, Vector2 position, Vector2 size)
        {
            GameObject dropdownObj = new GameObject("Dropdown");
            dropdownObj.transform.SetParent(parent, false);
            var dropdownRect = dropdownObj.AddComponent<RectTransform>();
            dropdownRect.anchorMin = new Vector2(0.5f, 0.5f);
            dropdownRect.anchorMax = new Vector2(0.5f, 0.5f);
            dropdownRect.anchoredPosition = position;
            dropdownRect.sizeDelta = size;

            var dropdown = dropdownObj.AddComponent<UnityEngine.UI.Dropdown>();
            dropdown.options.Clear();
            foreach (string opt in options)
                dropdown.options.Add(new UnityEngine.UI.Dropdown.OptionData(opt));

            var template = new GameObject("Template");
            template.transform.SetParent(dropdownObj.transform, false);
            var templateRect = template.AddComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0f, 0f);
            templateRect.anchorMax = new Vector2(1f, 0f);
            templateRect.pivot = new Vector2(0.5f, 1f);
            templateRect.anchoredPosition = Vector2.zero;
            templateRect.sizeDelta = new Vector2(0, 150);
            template.SetActive(false);
            dropdown.template = templateRect;

            var caption = new GameObject("Label");
            caption.transform.SetParent(dropdownObj.transform, false);
            var captionRect = caption.AddComponent<RectTransform>();
            captionRect.anchorMin = Vector2.zero;
            captionRect.anchorMax = Vector2.one;
            captionRect.offsetMin = new Vector2(10, 0);
            captionRect.offsetMax = new Vector2(-25, 0);
            var captionText = caption.AddComponent<Text>();
            captionText.text = options[0];
            captionText.font = font;
            captionText.fontSize = 14;
            captionText.color = UIColors.textPrimary;
            captionText.alignment = TextAnchor.MiddleLeft;
            dropdown.captionText = captionText;

            var arrow = new GameObject("Arrow");
            arrow.transform.SetParent(dropdownObj.transform, false);
            var arrowRect = arrow.AddComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(1f, 0.5f);
            arrowRect.anchorMax = new Vector2(1f, 0.5f);
            arrowRect.pivot = new Vector2(0.5f, 0.5f);
            arrowRect.anchoredPosition = new Vector2(-12, 0);
            arrowRect.sizeDelta = new Vector2(15, 15);
            var arrowText = arrow.AddComponent<Text>();
            arrowText.text = "▼";
            arrowText.fontSize = 10;
            arrowText.color = UIColors.textSecondary;
            arrowText.alignment = TextAnchor.MiddleCenter;

            return dropdownObj;
        }

        /// <summary>
        /// DEPRECATED — Este método no se usa en ninguna parte. Se deja como stub
        /// para no romper referencias en builds previas.
        /// </summary>
        [Obsolete("CreateStatusPanel is deprecated and will be removed. No callers exist.")]
        public static void CreateStatusPanel(Transform canvasTransform)
        {
            Debug.LogWarning("[UIPanelFactory] CreateStatusPanel is deprecated — no-op.");
        }

        internal static void UpdateStatusText(GameObject panel, string protocol)
        {
            var statusText = panel.transform.Find("RightPanel")?.GetComponent<Text>();
            if (statusText != null)
            {
                statusText.text = $"SIMULACION:\n\nProtocolo: {protocol}\n\nPresiona START para\niniciar el protocolo.";
                statusText.color = UIColors.textAccent;
            }
        }
    }
}
