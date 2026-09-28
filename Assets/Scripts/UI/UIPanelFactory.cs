using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using SimRedes.Network;
using SimRedes.Simulation;
using UIComp = SimRedes.UI.UIComponents;
using UIColors = SimRedes.UI.UIComponents.Colors;

namespace SimRedes.UI
{
    /// <summary>
    /// Fabrica de paneles de interfaz de usuario para navegacion e informacion general.
    /// Crea menus, paneles de conectividad, leyenda de discos y componentes UI base.
    /// </summary>
    public static class UIPanelFactory
    {
        /// <summary>
        /// Referencias a los elementos del panel de conectividad.
        /// </summary>
        public struct ConnectivityPanelRefs
        {
            /// <summary>Objeto raiz del panel.</summary>
            public GameObject panelObj;
            /// <summary>Texto que muestra el nodo origen.</summary>
            public Text sourceText;
            /// <summary>Texto que muestra el nodo destino.</summary>
            public Text destText;
            /// <summary>Texto del resultado de la prueba.</summary>
            public Text resultText;
            /// <summary>Icono del resultado (check/cruz).</summary>
            public Text resultIcon;
            /// <summary>Boton para ejecutar ping.</summary>
            public Button pingButton;
            /// <summary>Texto de estadisticas de pings.</summary>
            public Text statusText;
        }

        /// <summary>
        /// Obtiene una fuente dinamica Arial del tamano especificado.
        /// Retorna la fuente unica cacheada del proyecto.
        /// </summary>
        /// <param name="size">Ignorado (se usa fontSizede cada Text).</param>
        /// <returns>Fuente unica cacheada.</returns>
        public static Font GetFont(int size = 14)
        {
            return UIComp.GetFont();
        }

        /// <summary>
        /// Crea el panel de puntaje ubicado en la esquina inferior derecha.
        /// Muestra la etiqueta "Puntaje" y el puntaje numerico.
        /// </summary>
        /// <param name="canvas">Canvas raiz donde se instancia el panel.</param>
        /// <returns>Objeto del panel creado.</returns>
        public static GameObject CreateScorePanel(Transform canvas)
        {
            Font font = GetFont(18);
            Font bigFont = GetFont(28);

            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(220, 110), 15,
                UIColors.surfacePanel, UIColors.borderAccent, "ScorePanel");

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0f);
            panelRect.pivot = new Vector2(1f, 0f);
            panelRect.anchoredPosition = new Vector2(-10, 10);

            UIComp.CreateInfoText(panelObj.transform, "Puntaje", new Vector2(0, 22), font, 22, UIColors.textSecondary, false);
            UIComp.CreateInfoText(panelObj.transform, "0", new Vector2(0, -8), bigFont, 34, UIColors.textAccent, true);

            return panelObj;
        }

        /// <summary>
        /// Crea el panel de dispositivos ubicado en la esquina superior izquierda.
        /// Muestra la lista de nodos disponibles en la topologia.
        /// </summary>
        /// <param name="canvas">Canvas raiz donde se instancia el panel.</param>
        /// <param name="nodeCount">Cantidad de nodos a mostrar.</param>
        /// <param name="onItemClicked">Callback al hacer clic en un nodo de la lista.</param>
        /// <returns>Objeto del panel creado.</returns>
        public static GameObject CreateDevicesPanel(Transform canvas, int nodeCount,
            Action<int> onItemClicked)
        {
            Font font = GetFont(16);

            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(400, 640), 20,
                UIColors.surfacePanel, UIColors.borderAccent, "DevicesPanel");

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(10, -10);

            UIComp.CreateMenuTitle(panelObj.transform, "Dispositivos", 30, new Vector2(0, 290), font);

            return panelObj;
        }

        /// <summary>
        /// Crea el panel de prueba de conectividad con campos de origen/destino,
        /// boton de ping, icono de resultado y estadisticas.
        /// </summary>
        /// <param name="canvas">Canvas raiz donde se instancia el panel.</param>
        /// <param name="onBack">Callback al presionar el boton VOLVER.</param>
        /// <returns>Estructura con referencias a los componentes del panel.</returns>
        public static ConnectivityPanelRefs CreateConnectivityPanel(Transform canvas,
            Action onBack = null)
        {
            Font font = GetFont();
            Font bigFont = GetFont(16);

            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(820, 700), 25,
                UIColors.surfacePanel, UIColors.borderAccent, "ConnectivityPanel");

            UIComp.CreateMenuTitle(panelObj.transform, "Test de Conectividad", 38, new Vector2(0, 290), bigFont);

            // Source
            var sourceObj = new GameObject("SourceNode");
            sourceObj.transform.SetParent(panelObj.transform, false);
            var sourceRect = sourceObj.AddComponent<RectTransform>();
            sourceRect.anchorMin = new Vector2(0.5f, 0.5f);
            sourceRect.anchorMax = new Vector2(0.5f, 0.5f);
            sourceRect.anchoredPosition = new Vector2(-180, 150);
            sourceRect.sizeDelta = new Vector2(240, 55);
            var sourceText = sourceObj.AddComponent<Text>();
            sourceText.text = "Origen: -";
            sourceText.color = UIColors.textPrimary;
            sourceText.fontSize = 22;
            sourceText.alignment = TextAnchor.MiddleLeft;
            sourceText.font = font;

            // Arrow
            var arrowObj = new GameObject("Arrow");
            arrowObj.transform.SetParent(panelObj.transform, false);
            var arrowRect = arrowObj.AddComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(0.5f, 0.5f);
            arrowRect.anchorMax = new Vector2(0.5f, 0.5f);
            arrowRect.anchoredPosition = new Vector2(0, 150);
            arrowRect.sizeDelta = new Vector2(80, 40);
            var arrowText = arrowObj.AddComponent<Text>();
            arrowText.text = "→ → →";
            arrowText.color = UIColors.textAccent;
            arrowText.fontSize = 28;
            arrowText.alignment = TextAnchor.MiddleCenter;
            arrowText.font = font;

            // Destination
            var destObj = new GameObject("DestNode");
            destObj.transform.SetParent(panelObj.transform, false);
            var destRect = destObj.AddComponent<RectTransform>();
            destRect.anchorMin = new Vector2(0.5f, 0.5f);
            destRect.anchorMax = new Vector2(0.5f, 0.5f);
            destRect.anchoredPosition = new Vector2(180, 150);
            destRect.sizeDelta = new Vector2(240, 55);
            var destText = destObj.AddComponent<Text>();
            destText.text = "Destino: -";
            destText.color = UIColors.textPrimary;
            destText.fontSize = 22;
            destText.alignment = TextAnchor.MiddleRight;
            destText.font = font;

            // Ping button (listeners added by ConnectivityTestPanel caller)
            Button pingBtn = UIComp.CreateMenuButton(panelObj.transform, "PingButton", "HACER PING",
                new Vector2(0, 70), new Vector2(280, 75), font, 26);

            // Result icon
            var resultIconObj = new GameObject("ResultDisplay");
            resultIconObj.transform.SetParent(panelObj.transform, false);
            var resultIconRect = resultIconObj.AddComponent<RectTransform>();
            resultIconRect.anchorMin = new Vector2(0.5f, 0.5f);
            resultIconRect.anchorMax = new Vector2(0.5f, 0.5f);
            resultIconRect.anchoredPosition = new Vector2(0, -30);
            resultIconRect.sizeDelta = new Vector2(85, 85);

            var resultIconImg = resultIconObj.AddComponent<Image>();
            Texture2D resultTex = UIComp.CreateRoundedRectTexture(85, 85, 18, new Color(0.5f, 0.5f, 0.5f, 0.3f), UIColors.borderAccent, 2f);
            resultIconImg.sprite = Sprite.Create(resultTex, new Rect(0, 0, 85, 85), new Vector2(0.5f, 0.5f), 100);
            resultIconImg.type = Image.Type.Sliced;

            var resultIconInnerObj = new GameObject("ResultIcon");
            resultIconInnerObj.transform.SetParent(resultIconObj.transform, false);
            var resultIconInnerRect = resultIconInnerObj.AddComponent<RectTransform>();
            resultIconInnerRect.anchorMin = Vector2.zero;
            resultIconInnerRect.anchorMax = Vector2.one;
            resultIconInnerRect.offsetMin = new Vector2(15, 15);
            resultIconInnerRect.offsetMax = new Vector2(-15, -15);
            var resultIconInner = resultIconInnerObj.AddComponent<Text>();
            resultIconInner.text = "?";
            resultIconInner.color = UIColors.textSecondary;
            resultIconInner.fontSize = 38;
            resultIconInner.fontStyle = FontStyle.Bold;
            resultIconInner.alignment = TextAnchor.MiddleCenter;
            resultIconInner.font = font;

            // Result text
            var resultTextObj = new GameObject("ResultText");
            resultTextObj.transform.SetParent(panelObj.transform, false);
            var resultTextRect = resultTextObj.AddComponent<RectTransform>();
            resultTextRect.anchorMin = new Vector2(0.5f, 0.5f);
            resultTextRect.anchorMax = new Vector2(0.5f, 0.5f);
            resultTextRect.anchoredPosition = new Vector2(0, -130);
            resultTextRect.sizeDelta = new Vector2(600, 50);
            var resultText = resultTextObj.AddComponent<Text>();
            resultText.text = "Presiona PING para probar conectividad";
            resultText.color = UIColors.textSecondary;
            resultText.fontSize = 24;
            resultText.alignment = TextAnchor.MiddleCenter;
            resultText.font = font;

            // Status text
            var statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(panelObj.transform, false);
            var statusRect = statusObj.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.5f, 0.5f);
            statusRect.anchorMax = new Vector2(0.5f, 0.5f);
            statusRect.anchoredPosition = new Vector2(0, -190);
            statusRect.sizeDelta = new Vector2(560, 40);
            var statusText = statusObj.AddComponent<Text>();
            statusText.text = "Pings: 0 | Exitosos: 0 | Fallidos: 0";
            statusText.color = UIColors.textSecondary;
            statusText.fontSize = 20;
            statusText.alignment = TextAnchor.MiddleCenter;
            statusText.font = font;

            // Hint text
            var hintObj = new GameObject("HintText");
            hintObj.transform.SetParent(panelObj.transform, false);
            var hintRect = hintObj.AddComponent<RectTransform>();
            hintRect.anchorMin = new Vector2(0.5f, 0.5f);
            hintRect.anchorMax = new Vector2(0.5f, 0.5f);
            hintRect.anchoredPosition = new Vector2(0, -240);
            hintRect.sizeDelta = new Vector2(560, 35);
            var hintText = hintObj.AddComponent<Text>();
            hintText.text = "Tecla P = Ping | C = Limpiar";
            hintText.color = UIColors.textSecondary;
            hintText.fontSize = 18;
            hintText.alignment = TextAnchor.MiddleCenter;
            hintText.font = font;

            // Back button
            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "Volver",
                new Vector2(0, -300), new Vector2(240, 65), font, 24);
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

        /// <summary>
        /// Crea el panel de instrucciones "Como Usar" con tres columnas:
        /// controles de teclado, descripcion de actividades y conceptos de red.
        /// </summary>
        /// <param name="canvas">Canvas raiz donde se instancia el panel.</param>
        /// <param name="onBack">Callback al presionar VOLVER o ESC.</param>
        /// <returns>Objeto del panel creado.</returns>
        public static GameObject CreateInstructionsPanel(Transform canvas,
            Action onBack)
        {
            Font font = GetFont();
            Font bigFont = GetFont(24);

            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(1050, 850), 25,
                UIColors.surfacePanel, UIColors.borderAccent, "InstructionsPanel");

            UIComp.CreateMenuTitle(panelObj.transform, "Como Usar", 40, new Vector2(0, 380), bigFont);

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
                "0) Construye la Topolog\u00eda:\n" +
                "  crea redes y descubre\n" +
                "  el tipo de topologia.\n\n" +
                "1) Encuentra el Fallo:\n" +
                "  identifica y resuelve\n" +
                "  problemas en la red.\n\n" +
                "2) Tabla de Enrutamiento Tangible:\n" +
                "  visualiza las tablas\n" +
                "  de enrutamiento.\n\n" +
                "3) Simulaci\u00f3n de Mejor Ruta:\n" +
                "  elige la ruta optima\n" +
                "  hacia un destino.\n\n" +
                "4) Enrutamiento Est\u00e1tico Tangible:\n" +
                "  agrega rutas manuales.\n\n" +
                "5) Protocolo de Enrutamiento Din\u00e1mico Tangible:\n" +
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

            UIComp.CreateInfoColumn(panelObj.transform, new Vector2(-320, 60), 310, 540, font, leftText, 20);
            UIComp.CreateInfoColumn(panelObj.transform, new Vector2(0, 60), 310, 540, font, centerText, 20);
            UIComp.CreateInfoColumn(panelObj.transform, new Vector2(320, 60), 310, 540, font, rightText, 20);

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "Volver", new Vector2(0, -350), new Vector2(240, 65), font, 24);
            backBtn.onClick.AddListener(() => onBack?.Invoke());

            // Configurar MenuNavigator para que ESC funcione en el panel de instrucciones
            var navigator = UnityEngine.Object.FindAnyObjectByType<MenuNavigator>();
            if (navigator != null)
                navigator.SetupPanel(panelObj, () => onBack?.Invoke());

            return panelObj;
        }

        /// <summary>
        /// Crea el panel de leyenda de discos mostrando todos los IDs (1-18)
        /// con su color, etiqueta y descripcion, organizados en tres secciones.
        /// </summary>
        /// <param name="canvas">Canvas raiz donde se instancia el panel.</param>
        /// <param name="onBack">Callback al presionar VOLVER o ESC.</param>
        /// <returns>Objeto del panel creado.</returns>
        public static GameObject CreateDiscLegendPanel(Transform canvas, Action onBack)
        {
            Font font = GetFont();
            Font bigFont = GetFont(24);

            // Panel: 960x1200 da espacio holgado para título + 3 secciones + hint + botón
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(960, 1200), 25,
                UIColors.surfacePanel, UIColors.borderAccent, "DiscLegendPanel");

            UIComp.CreateMenuTitle(panelObj.transform, "Leyenda de Discos", 38, new Vector2(0, 480), bigFont);

            float spacing = 32f;
            float sectionGap = 38f;
            float xHeader = 0f;     // X para títulos de sección (MidLeft, ancho 500 → centrado)
            float xCircle = -300f;  // X para círculos de color (izquierda)
            float xLabel  = -170f;  // X para labels (MidLeft, ancho 180 → texto empieza en -240, a la derecha del círculo)
            float xDesc   = 130f;   // X para descripciones (MidLeft, ancho 320 → texto empieza en -50)

            // Sección 1: Físicos (1-3) — header en y=320, items bajan hasta ~y=208
            CreateLegendSection(panelObj.transform, "DISCOS F\u00cdSICOS (Mesa IDEUM)", new[] { 1, 2, 3 },
                320, spacing, font, xHeader, xCircle, xLabel, xDesc);

            // Sección 2: Actividades (4-6)
            float sec2Header = 320 - (3 * spacing + 20f) - sectionGap;
            CreateLegendSection(panelObj.transform, "MANEJADOS POR ACTIVIDADES", new[] { 4, 5, 6 },
                sec2Header, spacing, font, xHeader, xCircle, xLabel, xDesc);

            // Sección 3: Routing virtual (7-18)
            float sec3Header = sec2Header - (3 * spacing + 20f) - sectionGap;
            CreateLegendSection(panelObj.transform, "CONFIGURACI\u00d3N DE ROUTING (Botones en actividades)",
                new int[] { 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 },
                sec3Header, spacing, font, xHeader, xCircle, xLabel, xDesc);

            // Hint
            float sec3Bottom = sec3Header - (12 * spacing + 20f);
            float hintY = sec3Bottom - 80f;
            string hintText = "NOTA: Solo los discos 1-3 (Router/Switch/PC) son f\u00edsicos en la mesa IDEUM.\n" +
                "Enlaces, fallos y configuraci\u00f3n de routing se manejan desde las actividades.\n" +
                "En Actividad 4 (Enrutamiento Est\u00e1tico Tangible): presiona A\u00d1ADIR MANUAL para rutas personalizadas\n" +
                "o A\u00d1ADIR RUTA para rutas de ejemplo. Las rutas se escriben directamente en el Router.";
            var hintObj = new GameObject("UsageHint");
            hintObj.transform.SetParent(panelObj.transform, false);
            var hintRect = hintObj.AddComponent<RectTransform>();
            hintRect.anchorMin = new Vector2(0.5f, 0.5f);
            hintRect.anchorMax = new Vector2(0.5f, 0.5f);
            hintRect.anchoredPosition = new Vector2(0, hintY);
            hintRect.sizeDelta = new Vector2(860, 80);
            var hintTextComp = hintObj.AddComponent<Text>();
            hintTextComp.text = hintText;
            hintTextComp.color = new Color(1f, 0.7f, 0.2f);
            hintTextComp.fontSize = 15;
            hintTextComp.alignment = TextAnchor.UpperLeft;
            hintTextComp.font = font;
            hintTextComp.lineSpacing = 1.4f;

            // Botón VOLVER
            float backY = hintY - 80f;
            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "Volver", new Vector2(0, backY), new Vector2(240, 65), font, 24);
            backBtn.onClick.AddListener(() => onBack?.Invoke());

            // Configurar ESC
            var navigator = UnityEngine.Object.FindAnyObjectByType<MenuNavigator>();
            if (navigator != null)
                navigator.SetupPanel(panelObj, () => onBack?.Invoke());

            return panelObj;
        }

        /// <summary>
        /// Crea una seccion de la leyenda con un encabezado y una lista de discos,
        /// cada uno con circulo de color, etiqueta y descripcion.
        /// </summary>
        /// <param name="parent">Transform padre.</param>
        /// <param name="sectionName">Texto del encabezado de la seccion.</param>
        /// <param name="discIds">Arreglo de IDs de discos a incluir.</param>
        /// <param name="headerY">Posicion Y del encabezado.</param>
        /// <param name="spacing">Espaciado entre items.</param>
        /// <param name="font">Fuente a utilizar.</param>
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
            headerRect.sizeDelta = new Vector2(600, 26);
            var headerText = headerObj.AddComponent<Text>();
            headerText.text = sectionName;
            headerText.color = UIColors.textAccent;
            headerText.fontSize = 18;
            headerText.fontStyle = FontStyle.Bold;
            headerText.alignment = TextAnchor.MiddleLeft;
            headerText.font = font;

            // Items de la sección
            for (int i = 0; i < discIds.Length; i++)
            {
                int discId = discIds[i];
                var config = DiscConfiguration.GetConfiguration(discId);
                float yPos = headerY - spacing * (i + 1) - 24f;

                // Círculo de color
                var circleObj = new GameObject($"Disc_{discId}_Color");
                circleObj.transform.SetParent(parent, false);
                var circleRect = circleObj.AddComponent<RectTransform>();
                circleRect.anchorMin = new Vector2(0.5f, 0.5f);
                circleRect.anchorMax = new Vector2(0.5f, 0.5f);
                circleRect.anchoredPosition = new Vector2(xCircle, yPos);
                circleRect.sizeDelta = new Vector2(20, 20);
                var circleImg = circleObj.AddComponent<Image>();
                circleImg.sprite = UIComp.CreateRoundedRectSprite(20, 20, 10, config.DisplayColor, Color.clear, 0f);
                circleImg.color = Color.white;

                // Label (negrita)
                var labelObj = new GameObject($"Disc_{discId}_Label");
                labelObj.transform.SetParent(parent, false);
                var labelRect = labelObj.AddComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0.5f, 0.5f);
                labelRect.anchorMax = new Vector2(0.5f, 0.5f);
                labelRect.anchoredPosition = new Vector2(xLabel, yPos);
                labelRect.sizeDelta = new Vector2(200, 24);
                var labelText = labelObj.AddComponent<Text>();
                labelText.text = config.Label;
                labelText.color = UIColors.textPrimary;
                labelText.fontSize = 16;
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
                descRect.sizeDelta = new Vector2(360, 24);
                var descText = descObj.AddComponent<Text>();
                descText.text = config.Description;
                descText.color = UIColors.textSecondary;
                descText.fontSize = 15;
                descText.alignment = TextAnchor.MiddleLeft;
                descText.font = font;
            }
        }

        /// <summary>
        /// Alterna la visibilidad del panel de ejemplos de topologias.
        /// Si ya existe, lo destruye; si no, lo crea con los tipos de topologia.
        /// </summary>
        /// <param name="canvas">Canvas raiz donde se instancia el panel.</param>
        /// <param name="font">Fuente a utilizar.</param>
        public static void ToggleTopologyExamplePanel(Transform canvas, Font font)
        {
            var existingPanel = GameObject.Find("TopologyExamplePanel");
            if (existingPanel != null)
            {
                // B4: liberar los Sprite del panel antes de destruirlo
                UIComp.SafeDestroyPanelSprites(existingPanel);
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
            closeBtn.onClick.AddListener(() =>
            {
                // B4: liberar los Sprite del panel antes de destruirlo
                UIComp.SafeDestroyPanelSprites(panelObj);
                UnityEngine.Object.Destroy(panelObj);
            });

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
            backBtn.onClick.AddListener(() =>
            {
                // B4: liberar los Sprite del panel antes de destruirlo
                UIComp.SafeDestroyPanelSprites(panelObj);
                UnityEngine.Object.Destroy(panelObj);
            });
        }

        /// <summary>
        /// Crea el menu principal con botones para iniciar simulacion, actividades,
        /// pruebas de conectividad, instrucciones, leyenda de discos y salir.
        /// Configura MenuNavigator y MainMenuManager.
        /// </summary>
        /// <param name="canvas">Canvas raiz donde se instancia el menu.</param>
        /// <param name="onStartSimulation">Callback para iniciar simulacion.</param>
        /// <param name="onActivities">Callback para abrir actividades.</param>
        /// <param name="onConnectivity">Callback para pruebas de conectividad.</param>
        /// <param name="onInstructions">Callback para panel de instrucciones.</param>
        /// <param name="onDiscLegend">Callback para leyenda de discos.</param>
        /// <param name="onExit">Callback para salir de la aplicacion.</param>
        /// <returns>Objeto del panel del menu principal.</returns>
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

            GameObject mainMenuPanel = UIComp.CreateMenuPanel(canvas, "MainMenuPanel", new Vector2(1000, 1100));
            UIComp.CreateMenuTitle(mainMenuPanel.transform, "Simulador de Redes", 56, new Vector2(0, 500), font);

            float buttonWidth = 480, buttonHeight = 145, startY = 320, btnSpacing = 150;
            int btnRadius = 26;
            float btnBorder = 0f;
            int btnFontSize = 30;

            Button startBtn = UIComp.CreateMenuButton(mainMenuPanel.transform, "Btn_Iniciar", "Iniciar Simulaci\u00f3n", new Vector2(0, startY), new Vector2(buttonWidth, buttonHeight), font, btnFontSize, btnRadius, btnBorder);
            startBtn.onClick.AddListener(() => onStartSimulation?.Invoke());

            Button activitiesBtn = UIComp.CreateMenuButton(mainMenuPanel.transform, "Btn_Actividades", "Actividades", new Vector2(0, startY - btnSpacing), new Vector2(buttonWidth, buttonHeight), font, btnFontSize, btnRadius, btnBorder);
            activitiesBtn.onClick.AddListener(() => onActivities?.Invoke());

            Button connectivityBtn = UIComp.CreateMenuButton(mainMenuPanel.transform, "Btn_Conectividad", "Pruebas y Conexiones", new Vector2(0, startY - btnSpacing * 2), new Vector2(buttonWidth, buttonHeight), font, btnFontSize, btnRadius, btnBorder);
            connectivityBtn.onClick.AddListener(() => onConnectivity?.Invoke());

            Button instructionsBtn = UIComp.CreateMenuButton(mainMenuPanel.transform, "Btn_Instrucciones", "C\u00f3mo Usar", new Vector2(0, startY - btnSpacing * 3), new Vector2(buttonWidth, buttonHeight), font, btnFontSize, btnRadius, btnBorder);
            instructionsBtn.onClick.AddListener(() => onInstructions?.Invoke());

            Button discLegendBtn = UIComp.CreateMenuButton(mainMenuPanel.transform, "Btn_DiscLegend", "Leyenda de Discos", new Vector2(0, startY - btnSpacing * 4), new Vector2(buttonWidth, buttonHeight), font, btnFontSize, btnRadius, btnBorder);
            discLegendBtn.onClick.AddListener(() => onDiscLegend?.Invoke());

            Button exitBtn = UIComp.CreateMenuButton(mainMenuPanel.transform, "Btn_Salir", "Salir", new Vector2(0, startY - btnSpacing * 5), new Vector2(buttonWidth, buttonHeight), font, btnFontSize, btnRadius, btnBorder);
            exitBtn.onClick.AddListener(() => onExit?.Invoke());

            menuManager.mainMenuPanel = mainMenuPanel;
            var navigator = navigatorObj.GetComponent<MenuNavigator>();
            navigator.SetupPanel(mainMenuPanel, () => {
                UnityEngine.Debug.Log("[Menu] ESC en menu principal");
            });
            mainMenuPanel.SetActive(true);
            return mainMenuPanel;
        }

        /// <summary>
        /// Crea el panel de seleccion de actividades con botones para cada una
        /// de las 7 actividades disponibles (Construye Topologia a Escenarios).
        /// </summary>
        /// <param name="canvas">Canvas raiz.</param>
        /// <param name="onSelectActivity">Callback con el indice de la actividad seleccionada.</param>
        /// <param name="onBack">Callback para volver al menu principal.</param>
        /// <returns>Objeto del panel de actividades.</returns>
        public static GameObject CreateActivitiesPanel(Transform canvas, Action<int> onSelectActivity, Action onBack)
        {
            Font font = GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(1000, 1100), 25,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "ActivitiesPanel";
            UIComp.CreateMenuTitle(panelObj.transform, "Selecciona una Actividad", 56, new Vector2(0, 480), font);

            float buttonWidth = 500, buttonHeight = 110, startY = 350, btnSpacing = 116;
            int btnRadius = 24;
            float btnBorder = 0f;
            int actFontSize = 22;

            string[] activities = { "Construye la Topolog\u00eda", "Encuentra el Fallo", "Tabla de Enrutamiento Tangible", "Simulaci\u00f3n de Mejor Ruta", "Enrutamiento Est\u00e1tico Tangible", "Protocolo de Enrutamiento Din\u00e1mico Tangible", "Escenarios" };

            for (int i = 0; i < activities.Length; i++)
            {
                int capturedIndex = i;
                float yPos = startY - (i * btnSpacing);
                Button btn = UIComp.CreateMenuButton(panelObj.transform, "Activity_" + i, activities[i], new Vector2(0, yPos), new Vector2(buttonWidth, buttonHeight), font, actFontSize, btnRadius, btnBorder);
                btn.onClick.AddListener(() => onSelectActivity?.Invoke(capturedIndex));
            }

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "Volver al Men\u00fa", new Vector2(0, startY - 7 * btnSpacing), new Vector2(buttonWidth, buttonHeight), font, 22, btnRadius, btnBorder);
            backBtn.onClick.AddListener(() => onBack?.Invoke());

            var navigator = UnityEngine.Object.FindAnyObjectByType<MenuNavigator>();
            if (navigator != null)
                navigator.SetupPanel(panelObj, () => onBack?.Invoke());

            return panelObj;
        }

        /// <summary>
        /// Crea un campo de entrada de texto estilizado para configuracion de red.
        /// Incluye imagen de fondo, texto de placeholder y el InputField.
        /// </summary>
        /// <param name="parent">Transform padre.</param>
        /// <param name="placeholder">Texto placeholder del campo.</param>
        /// <param name="leftX">Posicion X del borde izquierdo.</param>
        /// <param name="rightX">Posicion X del borde derecho.</param>
        /// <param name="yPos">Posicion Y del campo.</param>
        /// <param name="font">Fuente a utilizar.</param>
        /// <returns>InputField creado.</returns>
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
            textComp.raycastTarget = false;
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

        /// <summary>
        /// Crea un dropdown (lista desplegable) con las opciones indicadas.
        /// Configura template, caption, texto y flecha.
        /// </summary>
        /// <param name="parent">Transform padre.</param>
        /// <param name="options">Lista de opciones del dropdown.</param>
        /// <param name="font">Fuente a utilizar.</param>
        /// <param name="position">Posicion anclada del dropdown.</param>
        /// <param name="size">Tamano del dropdown.</param>
        /// <returns>Objeto GameObject del dropdown.</returns>
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
        /// Actualiza el texto de estado del panel de enrutamiento dinamico
        /// con el nombre del protocolo seleccionado.
        /// </summary>
        /// <param name="panel">Objeto del panel que contiene RightPanel.</param>
        /// <param name="protocol">Nombre del protocolo activo (RIP, OSPF, EIGRP).</param>
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
