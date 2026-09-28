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
    /// <summary>
    /// Fabrica de paneles de interfaz para las actividades del simulador.
    /// Crea los paneles especificos de cada actividad (topologia, fallo, routing, etc.).
    /// </summary>
    public static class ActivityPanelFactory
    {
        /// <summary>
        /// Crea el panel de informacion para la actividad "Construye la Topologia".
        /// Muestra teclas de control y ejemplos de topologias.
        /// </summary>
        /// <param name="canvas">Canvas raiz donde se instancia el panel.</param>
        /// <returns>Objeto del panel creado.</returns>
        public static GameObject CreateBuildTopologyInfoPanel(Transform canvas)
        {
            Font font = UIPanelFactory.GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(380, 400), 20,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "BuildTopologyInfoPanel";

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;

            UIComp.CreateMenuTitle(panelObj.transform, "Construye la Topolog\u00eda", 22, new Vector2(0, 155), font);

            float y = 110;
            UIComp.CreateInfoText(panelObj.transform, "Construye la topolog\u00eda colocando", new Vector2(0, y), font, 13, UIColors.textSecondary, false);
            y -= 22;
            UIComp.CreateInfoText(panelObj.transform, "discos en la mesa de trabajo.", new Vector2(0, y), font, 13, UIColors.textSecondary, false);
            y -= 30;
            UIComp.CreateInfoText(panelObj.transform, "Teclas: 1=Router  2=Switch  3=PC", new Vector2(0, y), font, 12, UIColors.textAccent, false);
            y -= 22;
            UIComp.CreateInfoText(panelObj.transform, "Tecla 4 = Modo CONEXION", new Vector2(0, y), font, 12, UIColors.textAccent, false);
            y -= 26;

            string tipText =
                "Estrella: 1 central + perifericos\n" +
                "Bus: Linea de nodos\n" +
                "Anillo: Ciclo cerrado\n" +
                "Arbol: Routers + Switches\n" +
                "Malla: Todos conectados";

            var tipObj = new GameObject("TipText");
            tipObj.transform.SetParent(panelObj.transform, false);
            var tipRect = tipObj.AddComponent<RectTransform>();
            tipRect.anchorMin = new Vector2(0.5f, 0.5f);
            tipRect.anchorMax = new Vector2(0.5f, 0.5f);
            tipRect.anchoredPosition = new Vector2(0, -70);
            tipRect.sizeDelta = new Vector2(320, 110);
            var tipTextComp = tipObj.AddComponent<Text>();
            tipTextComp.text = tipText;
            tipTextComp.color = UIColors.textPrimary;
            tipTextComp.fontSize = 13;
            tipTextComp.alignment = TextAnchor.UpperLeft;
            tipTextComp.font = font;
            tipTextComp.lineSpacing = 1.3f;

            Button closeBtn = UIComp.CreateMenuButton(panelObj.transform, "CloseBtn", "CERRAR", new Vector2(0, -170), new Vector2(120, 40), font, 14);
            closeBtn.onClick.AddListener(() =>
            {
                // B4: liberar los Sprite del panel antes de destruirlo
                UIComp.SafeDestroyPanelSprites(panelObj);
                UnityEngine.Object.Destroy(panelObj);
            });

            return panelObj;
        }

        /// <summary>
        /// Crea el panel de la actividad "Simulacion de Mejor Ruta".
        /// Muestra IP destino, resultados, puntaje y botones SIGUIENTE/VOLVER.
        /// Conecta los componentes con BestRouteActivity.
        /// </summary>
        /// <param name="canvas">Canvas raiz.</param>
        /// <param name="onBack">Callback para volver.</param>
        /// <returns>Objeto del panel creado.</returns>
        public static GameObject CreateBestRoutePanel(Transform canvas, Action onBack)
        {
            Font font = UIPanelFactory.GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(960, 850), 25,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "BestRoutePanel";
            UIComp.CreateMenuTitle(panelObj.transform, "Simulaci\u00f3n de Mejor Ruta", 44, new Vector2(0, 370), font);

            var destObj = new GameObject("DestIPText");
            destObj.transform.SetParent(panelObj.transform, false);
            var destRect = destObj.AddComponent<RectTransform>();
            destRect.anchorMin = new Vector2(0.5f, 0.5f);
            destRect.anchorMax = new Vector2(0.5f, 0.5f);
            destRect.anchoredPosition = new Vector2(0, 280);
            destRect.sizeDelta = new Vector2(700, 50);
            var destText = destObj.AddComponent<Text>();
            destText.text = "Destino: --";
            destText.color = Color.white;
            destText.fontSize = 26;
            destText.alignment = TextAnchor.MiddleCenter;
            destText.font = font;

            var resultObj = new GameObject("ResultText");
            resultObj.transform.SetParent(panelObj.transform, false);
            var resultRect = resultObj.AddComponent<RectTransform>();
            resultRect.anchorMin = new Vector2(0.5f, 0.5f);
            resultRect.anchorMax = new Vector2(0.5f, 0.5f);
            resultRect.anchoredPosition = new Vector2(0, -300);
            resultRect.sizeDelta = new Vector2(700, 80);
            var resultText = resultObj.AddComponent<Text>();
            resultText.text = "Selecciona la mejor ruta para llegar al destino.";
            resultText.color = Color.white;
            resultText.fontSize = 22;
            resultText.alignment = TextAnchor.MiddleLeft;
            resultText.font = font;

            var scoreObj = new GameObject("ScoreText");
            scoreObj.transform.SetParent(panelObj.transform, false);
            var scoreRect = scoreObj.AddComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0.5f, 0.5f);
            scoreRect.anchorMax = new Vector2(0.5f, 0.5f);
            scoreRect.anchoredPosition = new Vector2(0, -190);
            scoreRect.sizeDelta = new Vector2(700, 45);
            var scoreText = scoreObj.AddComponent<Text>();
            scoreText.text = "Puntaje: 0 / 0";
            scoreText.color = new Color(0.8f, 0.8f, 0.3f);
            scoreText.fontSize = 24;
            scoreText.alignment = TextAnchor.MiddleCenter;
            scoreText.font = font;

            Button nextBtn = UIComp.CreateMenuButton(panelObj.transform, "NextBtn", "Siguiente", new Vector2(-140, -380), new Vector2(220, 65), font, 24);
            nextBtn.gameObject.SetActive(false);

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "Volver", new Vector2(140, -380), new Vector2(220, 65), font, 24);
            backBtn.onClick.AddListener(() => onBack?.Invoke());

            var activity = UnityEngine.Object.FindAnyObjectByType<BestRouteActivity>();
            if (activity != null)
            {
                activity.destIPText = destText;
                activity.resultText = resultText;
                activity.scoreText = scoreText;
                activity.nextButton = nextBtn;
                nextBtn.onClick.AddListener(() => { activity.NextScenario(); });
            }
            return panelObj;
        }

        /// <summary>
        /// Crea el panel de la actividad "Encuentra el Fallo" con 4 escenarios pre-hechos.
        /// Muestra descripcion del fallo, pista, disco tactil, resultado y navegacion.
        /// Conecta los componentes con FindFaultActivity.
        /// </summary>
        /// <param name="canvas">Canvas raiz.</param>
        /// <param name="onDiscClick">Callback al presionar el disco tactil.</param>
        /// <param name="onNext">Callback al presionar SIGUIENTE/FINALIZAR.</param>
        /// <param name="onPrev">Callback al presionar ANTERIOR.</param>
        /// <param name="onBack">Callback para volver al menu.</param>
        /// <returns>Objeto del panel creado.</returns>
        public static GameObject CreateFindFaultPanel(Transform canvas,
            Action onDiscClick, Action onNext, Action onPrev, Action onBack)
        {
            Font font = UIPanelFactory.GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(600, 680), 25,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "FindFaultPanel";

            UIComp.CreateMenuTitle(panelObj.transform, "Encuentra el Fallo", 24, new Vector2(0, 290), font);

            // ─── Titulo de escenario ───
            var titleObj = new GameObject("ScenarioTitleText");
            titleObj.transform.SetParent(panelObj.transform, false);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = new Vector2(0, 230);
            titleRect.sizeDelta = new Vector2(520, 30);
            var titleTextComp = titleObj.AddComponent<Text>();
            titleTextComp.text = "Escenario 1/4";
            titleTextComp.color = UIColors.textAccent;
            titleTextComp.fontSize = 16;
            titleTextComp.fontStyle = FontStyle.Bold;
            titleTextComp.alignment = TextAnchor.MiddleCenter;
            titleTextComp.font = font;

            // ─── Descripcion del fallo ───
            var descObj = new GameObject("FaultDescriptionText");
            descObj.transform.SetParent(panelObj.transform, false);
            var descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.5f, 0.5f);
            descRect.anchorMax = new Vector2(0.5f, 0.5f);
            descRect.anchoredPosition = new Vector2(0, 145);
            descRect.sizeDelta = new Vector2(500, 100);
            var descText = descObj.AddComponent<Text>();
            descText.text = "Descripci\u00f3n del fallo";
            descText.color = UIColors.textPrimary;
            descText.fontSize = 15;
            descText.alignment = TextAnchor.MiddleCenter;
            descText.font = font;
            descText.lineSpacing = 1.3f;

            // ─── Pista ───
            var hintObj = new GameObject("HintText");
            hintObj.transform.SetParent(panelObj.transform, false);
            var hintRect = hintObj.AddComponent<RectTransform>();
            hintRect.anchorMin = new Vector2(0.5f, 0.5f);
            hintRect.anchorMax = new Vector2(0.5f, 0.5f);
            hintRect.anchoredPosition = new Vector2(0, 55);
            hintRect.sizeDelta = new Vector2(500, 35);
            var hintTextComp = hintObj.AddComponent<Text>();
            hintTextComp.text = "\u2139\ufe0f Pista:";
            hintTextComp.color = new Color(0.8f, 0.8f, 0.3f);
            hintTextComp.fontSize = 13;
            hintTextComp.alignment = TextAnchor.MiddleCenter;
            hintTextComp.font = font;

            // ─── Disco tactil (boton grande) ───
            Color discBtnColor = new Color(0.2f, 0.5f, 0.8f, 1f);
            var discObj = new GameObject("TactileDiscBtn");
            discObj.transform.SetParent(panelObj.transform, false);
            var discRect = discObj.AddComponent<RectTransform>();
            discRect.anchorMin = new Vector2(0.5f, 0.5f);
            discRect.anchorMax = new Vector2(0.5f, 0.5f);
            discRect.anchoredPosition = new Vector2(0, -25);
            discRect.sizeDelta = new Vector2(240, 70);
            var discImg = discObj.AddComponent<Image>();
            var discTex = UIComp.CreateRoundedRectTexture(240, 70, 18, discBtnColor, UIColors.borderAccent, 3f);
            discImg.sprite = Sprite.Create(discTex, new Rect(0, 0, 240, 70), new Vector2(0.5f, 0.5f), 100);
            discImg.type = Image.Type.Sliced;
            var discBtn = discObj.AddComponent<Button>();
            discBtn.colors = UIComp.GetButtonColors(discBtnColor);
            discBtn.onClick.AddListener(() => onDiscClick?.Invoke());

            var discTextObj = new GameObject("Text");
            discTextObj.transform.SetParent(discObj.transform, false);
            var discTextRect = discTextObj.AddComponent<RectTransform>();
            discTextRect.anchorMin = Vector2.zero;
            discTextRect.anchorMax = Vector2.one;
            discTextRect.offsetMin = Vector2.zero;
            discTextRect.offsetMax = Vector2.zero;
            var discTextComp = discTextObj.AddComponent<Text>();
            discTextComp.text = "Disco T\u00e1ctil";
            discTextComp.color = Color.white;
            discTextComp.fontSize = 16;
            discTextComp.fontStyle = FontStyle.Bold;
            discTextComp.alignment = TextAnchor.MiddleCenter;
            discTextComp.font = font;

            // ─── Resultado ───
            var resultObj = new GameObject("ResultText");
            resultObj.transform.SetParent(panelObj.transform, false);
            var resultRect = resultObj.AddComponent<RectTransform>();
            resultRect.anchorMin = new Vector2(0.5f, 0.5f);
            resultRect.anchorMax = new Vector2(0.5f, 0.5f);
            resultRect.anchoredPosition = new Vector2(0, -110);
            resultRect.sizeDelta = new Vector2(500, 40);
            var resultTextComp = resultObj.AddComponent<Text>();
            resultTextComp.text = "";
            resultTextComp.color = UIColors.textPrimary;
            resultTextComp.fontSize = 15;
            resultTextComp.fontStyle = FontStyle.Bold;
            resultTextComp.alignment = TextAnchor.MiddleCenter;
            resultTextComp.font = font;

            // ─── Botones de navegacion ───
            Button prevBtn = UIComp.CreateMenuButton(panelObj.transform, "PrevBtn", "\u25c0 ANTERIOR", new Vector2(-150, -170), new Vector2(130, 40), font, 13);
            prevBtn.onClick.AddListener(() => onPrev?.Invoke());

            Button nextBtn = UIComp.CreateMenuButton(panelObj.transform, "NextBtn", "SIGUIENTE \u25b6", new Vector2(150, -170), new Vector2(130, 40), font, 13);
            nextBtn.onClick.AddListener(() => onNext?.Invoke());

            // ─── Boton volver ───
            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "VOLVER", new Vector2(0, -240), new Vector2(140, 40), font, 14);
            backBtn.onClick.AddListener(() => onBack?.Invoke());

            // ─── Status ───
            var statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(panelObj.transform, false);
            var statusRect = statusObj.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.5f, 0f);
            statusRect.anchorMax = new Vector2(0.5f, 0f);
            statusRect.pivot = new Vector2(0.5f, 0f);
            statusRect.anchoredPosition = new Vector2(0, 30);
            statusRect.sizeDelta = new Vector2(500, 25);
            var statusTextComp = statusObj.AddComponent<Text>();
            statusTextComp.text = "Modo: Encuentra el Fallo";
            statusTextComp.color = UIColors.textSecondary;
            statusTextComp.fontSize = 12;
            statusTextComp.alignment = TextAnchor.MiddleCenter;
            statusTextComp.font = font;

            // ─── Conectar con FindFaultActivity ───
            var activity = UnityEngine.Object.FindAnyObjectByType<FindFaultActivity>();
            if (activity != null)
            {
                activity.ConnectUI(titleTextComp, descText, hintTextComp, resultTextComp,
                    statusTextComp, discBtn, discTextComp, nextBtn,
                    nextBtn.GetComponentInChildren<Text>(), prevBtn);
            }

            return panelObj;
        }

        /// <summary>
        /// Crea el panel de la actividad "Tabla de Enrutamiento Tangible".
        /// Muestra informacion a la izquierda y las tablas de rutas a la derecha.
        /// Conecta los componentes con RoutingTablesActivity.
        /// </summary>
        /// <param name="canvas">Canvas raiz.</param>
        /// <param name="onBack">Callback para volver.</param>
        /// <returns>Objeto del panel creado.</returns>
        public static GameObject CreateRoutingTablesPanel(Transform canvas, Action onBack)
        {
            Font font = UIPanelFactory.GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(900, 750), 25,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "RoutingTablesPanel";
            UIComp.CreateMenuTitle(panelObj.transform, "Tabla de Enrutamiento Tangible", 38, new Vector2(0, 310), font);

            var leftPanel = new GameObject("LeftPanel");
            leftPanel.transform.SetParent(panelObj.transform, false);
            var leftRect = leftPanel.AddComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0.5f, 0.5f);
            leftRect.anchorMax = new Vector2(0.5f, 0.5f);
            leftRect.anchoredPosition = new Vector2(-200, 30);
            leftRect.sizeDelta = new Vector2(380, 420);
            var leftText = leftPanel.AddComponent<Text>();
            leftText.text =                 "TABLA DE ENRUTAMIENTO TANGIBLE\n\n" +
                "Visualiza la tabla de\n" +
                "enrutamiento de cada router.\n\n" +
                "ATENCION:\n  1 = Router\n  4 = Enlace\n  P = Ping\n\n" +
                "Presiona ACTUALIZAR para\n" +
                "refrescar las tablas.";
            leftText.color = UIColors.textPrimary;
            leftText.fontSize = 20;
            leftText.alignment = TextAnchor.UpperLeft;
            leftText.font = font;
            leftText.lineSpacing = 1.2f;

            var rightPanel = new GameObject("RightPanel");
            rightPanel.transform.SetParent(panelObj.transform, false);
            var rightRect = rightPanel.AddComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(0.5f, 0.5f);
            rightRect.anchorMax = new Vector2(0.5f, 0.5f);
            rightRect.anchoredPosition = new Vector2(200, 30);
            rightRect.sizeDelta = new Vector2(380, 420);
            var rightText = rightPanel.AddComponent<Text>();
            rightText.text = "RUTAS:\n\nNo hay routers.\n\nUsa la tecla 1 o coloca\ndiscos para anadir\nrouters.";
            rightText.color = UIColors.textAccent;
            rightText.fontSize = 20;
            rightText.alignment = TextAnchor.UpperLeft;
            rightText.font = font;

            var routingAct = UnityEngine.Object.FindAnyObjectByType<RoutingTablesActivity>();
            if (routingAct != null)
            {
                routingAct.tableText = rightText;
                routingAct.infoText = leftText;
            }

            Button refreshBtn = UIComp.CreateMenuButton(panelObj.transform, "RefreshBtn", "Actualizar", new Vector2(-140, -330), new Vector2(200, 60), font, 22);
            if (routingAct != null)
                refreshBtn.onClick.AddListener(() => routingAct.RefreshRoutingTables());

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "Volver", new Vector2(140, -330), new Vector2(200, 60), font, 22);
            backBtn.onClick.AddListener(() => onBack?.Invoke());

            return panelObj;
        }

        /// <summary>
        /// Crea el panel de la actividad "Enrutamiento Estatico Tangible".
        /// Incluye botones para anadir rutas manuales, de ejemplo y probar enrutamiento.
        /// Conecta los componentes con StaticRoutingActivity.
        /// </summary>
        /// <param name="canvas">Canvas raiz.</param>
        /// <param name="onBack">Callback para volver.</param>
        /// <returns>Objeto del panel creado.</returns>
        public static GameObject CreateStaticRoutingPanel(Transform canvas, Action onBack)
        {
            Font font = UIPanelFactory.GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(960, 800), 25,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "StaticRoutingPanel";
            UIComp.CreateMenuTitle(panelObj.transform, "Enrutamiento Est\u00e1tico Tangible", 40, new Vector2(0, 350), font);

            var leftPanel = new GameObject("LeftPanel");
            leftPanel.transform.SetParent(panelObj.transform, false);
            var leftRect = leftPanel.AddComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0.5f, 0.5f);
            leftRect.anchorMax = new Vector2(0.5f, 0.5f);
            leftRect.anchoredPosition = new Vector2(-200, 40);
            leftRect.sizeDelta = new Vector2(420, 400);
            var leftText = leftPanel.AddComponent<Text>();
            leftText.text = "ENRUTAMIENTO EST\u00c1TICO TANGIBLE\n\n" +
                "Configura manualmente\nrutas en los routers.\n\n" +
                "BOTONES:\n" +
                "  AÑADIR MANUAL = Ruta propia\n" +
                "  AÑADIR RUTA = Ruta ejemplo\n" +
                "  TEST = Probar enrutamiento\n\n" +
                "  P = Ping (teclado)";
            leftText.color = UIColors.textPrimary;
            leftText.fontSize = 22;
            leftText.alignment = TextAnchor.UpperLeft;
            leftText.font = font;
            leftText.lineSpacing = 1.2f;

            var rightPanel = new GameObject("RightPanel");
            rightPanel.transform.SetParent(panelObj.transform, false);
            var rightRect = rightPanel.AddComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(0.5f, 0.5f);
            rightRect.anchorMax = new Vector2(0.5f, 0.5f);
            rightRect.anchoredPosition = new Vector2(200, 40);
            rightRect.sizeDelta = new Vector2(420, 400);
            var rightText = rightPanel.AddComponent<Text>();
            rightText.text = "RUTAS CONFIGURADAS:\n\nPresiona AÑADIR RUTA\npara agregar rutas de\nejemplo al router.";
            rightText.color = UIColors.textAccent;
            rightText.fontSize = 22;
            rightText.alignment = TextAnchor.UpperLeft;
            rightText.font = font;

            var staticAct = UnityEngine.Object.FindAnyObjectByType<StaticRoutingActivity>();
            if (staticAct != null)
            {
                staticAct.routesText = rightText;
                staticAct.infoText = leftText;
                var feedbackObj = new GameObject("FeedbackText");
                feedbackObj.transform.SetParent(panelObj.transform, false);
                var feedbackRect = feedbackObj.AddComponent<RectTransform>();
                feedbackRect.anchorMin = new Vector2(0.5f, 0f);
                feedbackRect.anchorMax = new Vector2(0.5f, 0f);
                feedbackRect.pivot = new Vector2(0.5f, 0f);
                feedbackRect.anchoredPosition = new Vector2(0, 245);
                feedbackRect.sizeDelta = new Vector2(700, 45);
                var feedbackTextComp = feedbackObj.AddComponent<Text>();
                feedbackTextComp.text = "";
                feedbackTextComp.color = UIColors.textAccent;
                feedbackTextComp.fontSize = 22;
                feedbackTextComp.alignment = TextAnchor.MiddleCenter;
                feedbackTextComp.font = font;
                staticAct.feedbackText = feedbackTextComp;
            }

            Button manualBtn = UIComp.CreateMenuButton(panelObj.transform, "ManualRouteBtn", "A\u00f1adir Manual", new Vector2(-220, -280), new Vector2(200, 60), font, 22);
            if (staticAct != null)
                manualBtn.onClick.AddListener(() => staticAct.ShowAddRoutePanel());

            Button addBtn = UIComp.CreateMenuButton(panelObj.transform, "AddRouteBtn", "A\u00f1adir Ruta", new Vector2(0, -280), new Vector2(200, 60), font, 22);
            if (staticAct != null)
                addBtn.onClick.AddListener(() => staticAct.AddSampleRoute());

            Button testBtn = UIComp.CreateMenuButton(panelObj.transform, "TestRouteBtn", "Test", new Vector2(220, -280), new Vector2(200, 60), font, 22);
            if (staticAct != null)
                testBtn.onClick.AddListener(() => staticAct.TestRouting());

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "Volver", new Vector2(0, -360), new Vector2(240, 65), font, 24);
            backBtn.onClick.AddListener(() => onBack?.Invoke());
            return panelObj;
        }

        /// <summary>
        /// Crea el panel de la actividad "Protocolo de Enrutamiento Dinamico Tangible".
        /// Incluye botones para RIP, OSPF, EIGRP, START/STOP, configuracion avanzada
        /// con discos virtuales 15-18 y conecta componentes con DynamicRoutingActivity.
        /// </summary>
        /// <param name="canvas">Canvas raiz.</param>
        /// <param name="onSelectRIP">Callback al seleccionar RIP.</param>
        /// <param name="onSelectOSPF">Callback al seleccionar OSPF.</param>
        /// <param name="onStart">Callback al presionar START.</param>
        /// <param name="onStop">Callback al presionar STOP.</param>
        /// <param name="onClearRoutes">Callback para limpiar rutas.</param>
        /// <param name="onViewRoutes">Callback para ver rutas.</param>
        /// <param name="onBack">Callback para volver.</param>
        /// <param name="onSetNeighbor">Callback para configurar vecino (disco 15).</param>
        /// <param name="onSetNetwork">Callback para anunciar red (disco 16).</param>
        /// <param name="onSetCost">Callback para establecer costo/delay (disco 17).</param>
        /// <param name="onSetBW">Callback para establecer ancho de banda (disco 18).</param>
        /// <param name="onSelectEIGRP">Callback al seleccionar EIGRP.</param>
        /// <returns>Objeto del panel creado.</returns>
        public static GameObject CreateDynamicRoutingPanel(Transform canvas,
            Action onSelectRIP, Action onSelectOSPF, Action<GameObject> onStart,
            Action<GameObject> onStop, Action<GameObject> onClearRoutes,
            Action<GameObject> onViewRoutes, Action onBack,
            Action<string> onSetNeighbor = null, Action<string> onSetNetwork = null,
            Action<int> onSetCost = null, Action<int> onSetBW = null,
            Action onSelectEIGRP = null)
        {
            Font font = UIPanelFactory.GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(960, 1000), 25,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "DynamicRoutingPanel";
            UIComp.CreateMenuTitle(panelObj.transform, "Protocolo de Enrutamiento Din\u00e1mico Tangible", 40, new Vector2(0, 420), font);

            var leftPanel = new GameObject("LeftPanel");
            leftPanel.transform.SetParent(panelObj.transform, false);
            var leftRect = leftPanel.AddComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0.5f, 0.5f);
            leftRect.anchorMax = new Vector2(0.5f, 0.5f);
            leftRect.anchoredPosition = new Vector2(-220, 30);
            leftRect.sizeDelta = new Vector2(420, 380);
            var leftText = leftPanel.AddComponent<Text>();
            leftText.text =                 "PROTOCOLO DE ENRUTAMIENTO\n" +
                "DIN\u00c1MICO TANGIBLE\n\n" +
                "RIP (conteo de hops)\n" +
                "OSPF (costo por enlace)\n" +
                "EIGRP (metrica compuesta)\n\n" +
                "BOTONES:\n  START = Iniciar envio\n  STOP = Detener\n  LIMPIAR = Borrar rutas\n  VER RUTAS = Mostrar\n\n" +
                "Usa teclas 1-4 para\ncrear la topologia.";
            leftText.color = UIColors.textPrimary;
            leftText.fontSize = 20;
            leftText.alignment = TextAnchor.UpperLeft;
            leftText.font = font;
            leftText.lineSpacing = 1.2f;

            var rightPanel = new GameObject("RightPanel");
            rightPanel.transform.SetParent(panelObj.transform, false);
            var rightRect = rightPanel.AddComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(0.5f, 0.5f);
            rightRect.anchorMax = new Vector2(0.5f, 0.5f);
            rightRect.anchoredPosition = new Vector2(220, 30);
            rightRect.sizeDelta = new Vector2(420, 380);
            var rightText = rightPanel.AddComponent<Text>();
            rightText.text = "SIMULACION:\n\nProtocolo: Ninguno\n\nPresiona START para\niniciar el protocolo.";
            rightText.color = UIColors.textAccent;
            rightText.fontSize = 20;
            rightText.alignment = TextAnchor.UpperLeft;
            rightText.font = font;
            rightText.name = "StatusText";

            float btnY = -180, btnSpacing = 140;

            var dynAct = UnityEngine.Object.FindAnyObjectByType<DynamicRoutingActivity>();

            Button ripBtn = UIComp.CreateMenuButton(panelObj.transform, "RIPBtn", "RIP", new Vector2(-btnSpacing * 1.5f, btnY), new Vector2(120, 55), font, 20);
            ripBtn.onClick.AddListener(() => {
                UIPanelFactory.UpdateStatusText(panelObj, "RIP");
                if (dynAct != null) dynAct.SetProtocol(RoutingProtocol.RIP);
                onSelectRIP?.Invoke();
            });

            Button ospfBtn = UIComp.CreateMenuButton(panelObj.transform, "OSBFBtn", "OSPF", new Vector2(-btnSpacing * 0.5f, btnY), new Vector2(120, 55), font, 20);
            ospfBtn.onClick.AddListener(() => {
                UIPanelFactory.UpdateStatusText(panelObj, "OSPF");
                if (dynAct != null) dynAct.SetProtocol(RoutingProtocol.OSPF);
                onSelectOSPF?.Invoke();
            });

            Button eigrpBtn = UIComp.CreateMenuButton(panelObj.transform, "EIGRPBtn", "EIGRP", new Vector2(btnSpacing * 0.5f, btnY), new Vector2(120, 55), font, 20);
            eigrpBtn.onClick.AddListener(() => {
                UIPanelFactory.UpdateStatusText(panelObj, "EIGRP");
                if (dynAct != null) dynAct.SetProtocol(RoutingProtocol.EIGRP);
                onSelectEIGRP?.Invoke();
            });

            Button startBtn = UIComp.CreateMenuButton(panelObj.transform, "StartBtn", "START", new Vector2(btnSpacing * 1.5f, btnY), new Vector2(120, 55), font, 20);
            startBtn.onClick.AddListener(() => onStart?.Invoke(panelObj));

            btnY -= 70;
            Button stopBtn = UIComp.CreateMenuButton(panelObj.transform, "StopBtn", "STOP", new Vector2(-btnSpacing * 1.5f, btnY), new Vector2(120, 50), font, 18);
            stopBtn.onClick.AddListener(() => onStop?.Invoke(panelObj));

            Button clearBtn = UIComp.CreateMenuButton(panelObj.transform, "ClearBtn", "Limpiar", new Vector2(-btnSpacing * 0.5f, btnY), new Vector2(120, 50), font, 18);
            clearBtn.onClick.AddListener(() => onClearRoutes?.Invoke(panelObj));

            Button viewBtn = UIComp.CreateMenuButton(panelObj.transform, "ViewBtn", "Ver Rutas", new Vector2(btnSpacing * 0.5f, btnY), new Vector2(120, 50), font, 18);
            viewBtn.onClick.AddListener(() => onViewRoutes?.Invoke(panelObj));

            var protocolStatusObj = new GameObject("ProtocolStatusText");
            protocolStatusObj.transform.SetParent(panelObj.transform, false);
            var protoRect = protocolStatusObj.AddComponent<RectTransform>();
            protoRect.anchorMin = new Vector2(0.5f, 0.5f);
            protoRect.anchorMax = new Vector2(0.5f, 0.5f);
            protoRect.anchoredPosition = new Vector2(0, 280);
            protoRect.sizeDelta = new Vector2(600, 40);
            var protoTextComp = protocolStatusObj.AddComponent<Text>();
            protoTextComp.text = "Protocolo: RIP";
            protoTextComp.color = new Color(0.3f, 0.8f, 1f);
            protoTextComp.fontSize = 24;
            protoTextComp.alignment = TextAnchor.MiddleCenter;
            protoTextComp.font = font;
            protoTextComp.fontStyle = FontStyle.Bold;

            if (dynAct != null)
            {
                dynAct.tablesText = rightText;
                dynAct.infoText = leftText;
                dynAct.protocolText = protoTextComp;
                var feedbackObj = new GameObject("FeedbackText");
                feedbackObj.transform.SetParent(panelObj.transform, false);
                var feedbackRect = feedbackObj.AddComponent<RectTransform>();
                feedbackRect.anchorMin = new Vector2(0.5f, 0f);
                feedbackRect.anchorMax = new Vector2(0.5f, 0f);
                feedbackRect.pivot = new Vector2(0.5f, 0f);
                feedbackRect.anchoredPosition = new Vector2(0, 320);
                feedbackRect.sizeDelta = new Vector2(700, 45);
                var feedbackTextComp = feedbackObj.AddComponent<Text>();
                feedbackTextComp.text = "";
                feedbackTextComp.color = UIColors.textAccent;
                feedbackTextComp.fontSize = 20;
                feedbackTextComp.alignment = TextAnchor.MiddleCenter;
                feedbackTextComp.font = font;
                dynAct.feedbackText = feedbackTextComp;
            }

            var discConfigLabel = new GameObject("DiscConfigLabel");
            discConfigLabel.transform.SetParent(panelObj.transform, false);
            var dclRect = discConfigLabel.AddComponent<RectTransform>();
            dclRect.anchorMin = new Vector2(0.5f, 0.5f);
            dclRect.anchorMax = new Vector2(0.5f, 0.5f);
            dclRect.pivot = new Vector2(0.5f, 0.5f);
            dclRect.anchoredPosition = new Vector2(0, -270);
            dclRect.sizeDelta = new Vector2(800, 28);
            var dclText = discConfigLabel.AddComponent<Text>();
            dclText.text = "--- CONFIG. AVANZADA (Discos Virtuales 15-18) ---";
            dclText.color = new Color(0.6f, 0.6f, 0.6f);
            dclText.fontSize = 16;
            dclText.alignment = TextAnchor.MiddleCenter;
            dclText.font = font;

            Text CreateConfigLabel(string text, float y, float w = 220, float h = 28, int fsize = 16)
            {
                var obj = new GameObject("ConfigLabel");
                obj.transform.SetParent(panelObj.transform, false);
                var r = obj.AddComponent<RectTransform>();
                r.anchorMin = new Vector2(0.5f, 0.5f);
                r.anchorMax = new Vector2(0.5f, 0.5f);
                r.anchoredPosition = new Vector2(-300, y);
                r.sizeDelta = new Vector2(w, h);
                var t = obj.AddComponent<Text>();
                t.text = text; t.font = font; t.fontSize = fsize;
                t.color = UIColors.textSecondary;
                t.alignment = TextAnchor.MiddleLeft;
                return t;
            }

            float discY = -305;
            CreateConfigLabel("Vecino (15):", discY);
            var neighborInput = UIComp.CreateInputField(panelObj.transform, "NeighborInput", new Vector2(-150, discY), new Vector2(200, 36), "", "Router1", font, 16);
            Button neighborBtn = UIComp.CreateMenuButton(panelObj.transform, "NeighborBtn", "Aplicar", new Vector2(80, discY), new Vector2(120, 36), font, 15);
            if (onSetNeighbor != null) neighborBtn.onClick.AddListener(() => onSetNeighbor(neighborInput.text));

            discY -= 28;
            CreateConfigLabel("Anunciar Red (16):", discY);
            var networkInput = UIComp.CreateInputField(panelObj.transform, "NetworkInput", new Vector2(-150, discY), new Vector2(200, 36), "", "10.0.0.0/8", font, 16);
            Button networkBtn = UIComp.CreateMenuButton(panelObj.transform, "NetworkBtn", "Aplicar", new Vector2(80, discY), new Vector2(120, 36), font, 15);
            if (onSetNetwork != null) networkBtn.onClick.AddListener(() => onSetNetwork(networkInput.text));

            discY -= 28;
            CreateConfigLabel("Costo/Delay (17):", discY);
            var costInput = UIComp.CreateInputField(panelObj.transform, "CostInput", new Vector2(-150, discY), new Vector2(200, 36), "10", "10", font, 16);
            Button costBtn = UIComp.CreateMenuButton(panelObj.transform, "CostBtn", "Aplicar", new Vector2(80, discY), new Vector2(120, 36), font, 15);
            if (onSetCost != null) costBtn.onClick.AddListener(() => {
                if (int.TryParse(costInput.text, out int cost)) onSetCost(cost);
            });

            discY -= 28;
            CreateConfigLabel("Ancho Banda (18):", discY);
            var bwInput = UIComp.CreateInputField(panelObj.transform, "BWInput", new Vector2(-150, discY), new Vector2(200, 36), "1000", "1000", font, 16);
            Button bwBtn = UIComp.CreateMenuButton(panelObj.transform, "BWBtn", "Aplicar", new Vector2(80, discY), new Vector2(120, 36), font, 15);
            if (onSetBW != null) bwBtn.onClick.AddListener(() => {
                if (int.TryParse(bwInput.text, out int bw)) onSetBW(bw);
            });

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "Volver", new Vector2(0, -460), new Vector2(240, 65), font, 24);
            backBtn.onClick.AddListener(() => onBack?.Invoke());
            return panelObj;
        }

        /// <summary>
        /// Crea el panel de seleccion de escenarios preconfigurados.
        /// Muestra hasta 5 escenarios con nombre, dificultad, descripcion y boton de click.
        /// </summary>
        /// <param name="canvas">Canvas raiz.</param>
        /// <param name="scenarios">Lista de escenarios disponibles.</param>
        /// <param name="onScenarioClick">Callback con el indice del escenario seleccionado.</param>
        /// <param name="onBack">Callback para volver.</param>
        /// <returns>Objeto del panel creado.</returns>
        public static GameObject CreateScenariosPanel(Transform canvas,
            List<PredefinedScenarios.NetworkScenario> scenarios,
            Action<int> onScenarioClick, Action onBack)
        {
            Font font = UIPanelFactory.GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(1000, 1000), 25,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "ScenariosPanel";
            UIComp.CreateMenuTitle(panelObj.transform, "Escenarios Preconfigurados", 42, new Vector2(0, 430), font);

            float startY = 190;
            float itemSpacing = 150;
            int displayCount = Mathf.Min(scenarios.Count, 5);

            for (int i = 0; i < displayCount; i++)
            {
                int scenarioIndex = i;
                var scenario = scenarios[i];

                var itemBg = new GameObject($"ScenarioItem_{i}");
                itemBg.transform.SetParent(panelObj.transform, false);
                var itemRect = itemBg.AddComponent<RectTransform>();
                itemRect.anchorMin = new Vector2(0.5f, 1f);
                itemRect.anchorMax = new Vector2(0.5f, 1f);
                itemRect.pivot = new Vector2(0.5f, 1f);
                itemRect.anchoredPosition = new Vector2(0, -startY - (i * itemSpacing));
                itemRect.sizeDelta = new Vector2(880, 130);

                var itemImg = itemBg.AddComponent<Image>();
                Texture2D itemTex = UIComp.CreateRoundedRectTexture(880, 130, 18, UIColors.surfaceElevated, UIColors.borderAccent, 3f);
                itemImg.sprite = Sprite.Create(itemTex, new Rect(0, 0, 880, 130), new Vector2(0.5f, 0.5f), 100);
                itemImg.type = Image.Type.Sliced;

                string difficultyText = scenario.difficulty == PredefinedScenarios.ScenarioDifficulty.Basico ? "BASICO" :
                    (scenario.difficulty == PredefinedScenarios.ScenarioDifficulty.Intermedio ? "INTERMEDIO" : "AVANZADO");
                Color diffColor = scenario.difficulty == PredefinedScenarios.ScenarioDifficulty.Basico ? Color.green :
                    (scenario.difficulty == PredefinedScenarios.ScenarioDifficulty.Intermedio ? Color.yellow : Color.red);

                var nameObj = new GameObject("Name");
                nameObj.transform.SetParent(itemBg.transform, false);
                var nameRect = nameObj.AddComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0f, 0.5f);
                nameRect.anchorMax = new Vector2(0.5f, 0.5f);
                nameRect.pivot = new Vector2(0f, 0.5f);
                nameRect.anchoredPosition = new Vector2(25, 22);
                nameRect.offsetMin = Vector2.zero;
                nameRect.offsetMax = new Vector2(0, 30);
                var nameText = nameObj.AddComponent<Text>();
                nameText.text = scenario.name;
                nameText.font = font;
                nameText.fontSize = 24;
                nameText.color = UIColors.textPrimary;
                nameText.fontStyle = FontStyle.Bold;
                nameText.alignment = TextAnchor.MiddleLeft;
                nameText.resizeTextForBestFit = true;
                nameText.resizeTextMinSize = 18;

                var diffObj = new GameObject("Difficulty");
                diffObj.transform.SetParent(itemBg.transform, false);
                var diffRect = diffObj.AddComponent<RectTransform>();
                diffRect.anchorMin = new Vector2(0.5f, 0.5f);
                diffRect.anchorMax = new Vector2(1f, 0.5f);
                diffRect.pivot = new Vector2(0.5f, 0.5f);
                diffRect.anchoredPosition = new Vector2(0, 22);
                diffRect.offsetMin = Vector2.zero;
                diffRect.offsetMax = new Vector2(-25, 30);
                var diffText = diffObj.AddComponent<Text>();
                diffText.text = difficultyText;
                diffText.font = font;
                diffText.fontSize = 18;
                diffText.color = diffColor;
                diffText.fontStyle = FontStyle.Bold;
                diffText.alignment = TextAnchor.MiddleCenter;

                var descObj = new GameObject("Description");
                descObj.transform.SetParent(itemBg.transform, false);
                var descRect = descObj.AddComponent<RectTransform>();
                descRect.anchorMin = new Vector2(0f, 0.5f);
                descRect.anchorMax = new Vector2(1f, 0.5f);
                descRect.pivot = new Vector2(0.5f, 0.5f);
                descRect.anchoredPosition = new Vector2(0, -18);
                descRect.sizeDelta = new Vector2(840, 35);
                var descText = descObj.AddComponent<Text>();
                descText.text = scenario.description;
                descText.font = font;
                descText.fontSize = 20;
                descText.color = UIColors.textSecondary;
                descText.alignment = TextAnchor.MiddleCenter;

                var btn = itemBg.AddComponent<Button>();
                ColorBlock colors = new ColorBlock();
                colors.normalColor = UIColors.surfaceElevated;
                colors.highlightedColor = UIColors.buttonHover;
                colors.pressedColor = UIColors.buttonNormal;
                colors.colorMultiplier = 1f;
                btn.colors = colors;
                btn.onClick.AddListener(() => onScenarioClick?.Invoke(scenarioIndex));
            }

            var navigator = UnityEngine.Object.FindAnyObjectByType<MenuNavigator>();
            if (navigator != null)
                navigator.SetupPanel(panelObj, () => onBack?.Invoke());

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "Volver", new Vector2(0, -460), new Vector2(240, 65), font, 24);
            backBtn.onClick.AddListener(() => onBack?.Invoke());
            return panelObj;
        }

        /// <summary>
        /// Crea el panel de informacion detallada de un escenario.
        /// Muestra el nombre, objetivos, ayudas y botones INICIAR/CANCELAR.
        /// </summary>
        /// <param name="canvas">Canvas raiz.</param>
        /// <param name="scenario">Escenario a mostrar.</param>
        /// <param name="onStart">Callback al presionar INICIAR.</param>
        /// <param name="onCancel">Callback al presionar CANCELAR.</param>
        /// <returns>Objeto del panel creado.</returns>
        public static GameObject CreateScenarioInfoPanel(Transform canvas,
            PredefinedScenarios.NetworkScenario scenario,
            Action onStart, Action onCancel)
        {
            Font font = UIPanelFactory.GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(800, 700), 25,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "ScenarioInfoPanel";

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;

            UIComp.CreateMenuTitle(panelObj.transform, scenario.name, 36, new Vector2(0, 290), font);

            float yPos = 230;
            float lineHeight = 40;

            UIComp.CreateInfoText(panelObj.transform, "OBJETIVOS:", new Vector2(0, yPos), font, 22, UIColors.textAccent, true);
            yPos -= lineHeight;

            foreach (var obj in scenario.objectives)
            {
                UIComp.CreateInfoText(panelObj.transform, "- " + obj, new Vector2(-20, yPos), font, 18, UIColors.textPrimary, false);
                yPos -= lineHeight - 5;
            }

            yPos -= 10;
            UIComp.CreateInfoText(panelObj.transform, "AYUDAS:", new Vector2(0, yPos), font, 22, UIColors.textSecondary, true);
            yPos -= lineHeight;

            foreach (var hint in scenario.hints)
            {
                UIComp.CreateInfoText(panelObj.transform, "* " + hint, new Vector2(-20, yPos), font, 18, UIColors.textSecondary, false);
                yPos -= lineHeight - 5;
            }

            Button startBtn = UIComp.CreateMenuButton(panelObj.transform, "StartScenarioBtn", "Iniciar", new Vector2(-140, -300), new Vector2(220, 65), font, 26);
            startBtn.onClick.AddListener(() =>
            {
                // B4: liberar los Sprite del panel antes de destruirlo
                UIComp.SafeDestroyPanelSprites(panelObj);
                onStart?.Invoke();
                UnityEngine.Object.Destroy(panelObj);
            });

            Button cancelBtn = UIComp.CreateMenuButton(panelObj.transform, "CancelBtn", "Cancelar", new Vector2(140, -300), new Vector2(220, 65), font, 26);
            cancelBtn.onClick.AddListener(() =>
            {
                // B4: liberar los Sprite del panel antes de destruirlo
                UIComp.SafeDestroyPanelSprites(panelObj);
                onCancel?.Invoke();
                UnityEngine.Object.Destroy(panelObj);
            });

            return panelObj;
        }
    }
}
