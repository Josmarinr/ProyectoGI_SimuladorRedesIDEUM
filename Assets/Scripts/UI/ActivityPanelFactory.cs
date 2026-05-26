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
    public static class ActivityPanelFactory
    {
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
            closeBtn.onClick.AddListener(() => UnityEngine.Object.Destroy(panelObj));

            return panelObj;
        }

        public static GameObject CreateBestRoutePanel(Transform canvas, Action onBack)
        {
            Font font = UIPanelFactory.GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(520, 600), 25,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "BestRoutePanel";
            UIComp.CreateMenuTitle(panelObj.transform, "Simulaci\u00f3n de Mejor Ruta", 24, new Vector2(0, 250), font);

            var destObj = new GameObject("DestIPText");
            destObj.transform.SetParent(panelObj.transform, false);
            var destRect = destObj.AddComponent<RectTransform>();
            destRect.anchorMin = new Vector2(0.5f, 0.5f);
            destRect.anchorMax = new Vector2(0.5f, 0.5f);
            destRect.anchoredPosition = new Vector2(0, 190);
            destRect.sizeDelta = new Vector2(460, 30);
            var destText = destObj.AddComponent<Text>();
            destText.text = "Destino: --";
            destText.color = Color.white;
            destText.fontSize = 16;
            destText.alignment = TextAnchor.MiddleCenter;
            destText.font = font;

            var resultObj = new GameObject("ResultText");
            resultObj.transform.SetParent(panelObj.transform, false);
            var resultRect = resultObj.AddComponent<RectTransform>();
            resultRect.anchorMin = new Vector2(0.5f, 0.5f);
            resultRect.anchorMax = new Vector2(0.5f, 0.5f);
            resultRect.anchoredPosition = new Vector2(0, -200);
            resultRect.sizeDelta = new Vector2(460, 60);
            var resultText = resultObj.AddComponent<Text>();
            resultText.text = "Selecciona la mejor ruta para llegar al destino.";
            resultText.color = Color.white;
            resultText.fontSize = 13;
            resultText.alignment = TextAnchor.MiddleLeft;
            resultText.font = font;

            var scoreObj = new GameObject("ScoreText");
            scoreObj.transform.SetParent(panelObj.transform, false);
            var scoreRect = scoreObj.AddComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0.5f, 0.5f);
            scoreRect.anchorMax = new Vector2(0.5f, 0.5f);
            scoreRect.anchoredPosition = new Vector2(0, -135);
            scoreRect.sizeDelta = new Vector2(460, 30);
            var scoreText = scoreObj.AddComponent<Text>();
            scoreText.text = "Puntaje: 0 / 0";
            scoreText.color = new Color(0.8f, 0.8f, 0.3f);
            scoreText.fontSize = 14;
            scoreText.alignment = TextAnchor.MiddleCenter;
            scoreText.font = font;

            Button nextBtn = UIComp.CreateMenuButton(panelObj.transform, "NextBtn", "SIGUIENTE", new Vector2(-80, -260), new Vector2(140, 40), font, 14);
            nextBtn.gameObject.SetActive(false);

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "VOLVER", new Vector2(80, -260), new Vector2(140, 40), font, 14);
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

        public static GameObject CreateFindFaultPanel(Transform canvas, Action onSolve, Action onBack)
        {
            Font font = UIPanelFactory.GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(520, 540), 25,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "FindFaultPanel";
            UIComp.CreateMenuTitle(panelObj.transform, "Encuentra el Fallo", 26, new Vector2(0, 230), font);

            var descObj = new GameObject("FaultDescriptionText");
            descObj.transform.SetParent(panelObj.transform, false);
            var descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.5f, 0.5f);
            descRect.anchorMax = new Vector2(0.5f, 0.5f);
            descRect.anchoredPosition = new Vector2(0, 140);
            descRect.sizeDelta = new Vector2(440, 80);
            var descText = descObj.AddComponent<Text>();
            descText.text = "Coloca dispositivos y genera un fallo para identificarlo.";
            descText.color = UIColors.textPrimary;
            descText.fontSize = 15;
            descText.alignment = TextAnchor.MiddleCenter;
            descText.font = font;
            descText.lineSpacing = 1.2f;

            var hintObj = new GameObject("HintText");
            hintObj.transform.SetParent(panelObj.transform, false);
            var hintRect = hintObj.AddComponent<RectTransform>();
            hintRect.anchorMin = new Vector2(0.5f, 0.5f);
            hintRect.anchorMax = new Vector2(0.5f, 0.5f);
            hintRect.anchoredPosition = new Vector2(0, 50);
            hintRect.sizeDelta = new Vector2(440, 40);
            var hintTextComp = hintObj.AddComponent<Text>();
            hintTextComp.text = "Pista: --";
            hintTextComp.color = new Color(0.8f, 0.8f, 0.3f);
            hintTextComp.fontSize = 14;
            hintTextComp.alignment = TextAnchor.MiddleCenter;
            hintTextComp.font = font;

            var resultObj = new GameObject("ResultText");
            resultObj.transform.SetParent(panelObj.transform, false);
            var resultRect = resultObj.AddComponent<RectTransform>();
            resultRect.anchorMin = new Vector2(0.5f, 0.5f);
            resultRect.anchorMax = new Vector2(0.5f, 0.5f);
            resultRect.anchoredPosition = new Vector2(0, -10);
            resultRect.sizeDelta = new Vector2(440, 40);
            var resultTextComp = resultObj.AddComponent<Text>();
            resultTextComp.text = "";
            resultTextComp.color = UIColors.textPrimary;
            resultTextComp.fontSize = 15;
            resultTextComp.alignment = TextAnchor.MiddleCenter;
            resultTextComp.font = font;

            Button solveBtn = UIComp.CreateMenuButton(panelObj.transform, "SolveBtn", "RESOLVER", new Vector2(0, -80), new Vector2(160, 45), font, 16);
            solveBtn.onClick.AddListener(() => onSolve?.Invoke());

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "VOLVER", new Vector2(0, -160), new Vector2(140, 40), font, 14);
            backBtn.onClick.AddListener(() => onBack?.Invoke());

            var statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(panelObj.transform, false);
            var statusRect = statusObj.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.5f, 0f);
            statusRect.anchorMax = new Vector2(0.5f, 0f);
            statusRect.pivot = new Vector2(0.5f, 0f);
            statusRect.anchoredPosition = new Vector2(0, 30);
            statusRect.sizeDelta = new Vector2(440, 30);
            var statusTextComp = statusObj.AddComponent<Text>();
            statusTextComp.text = "Modo: Encuentra el Fallo";
            statusTextComp.color = UIColors.textSecondary;
            statusTextComp.fontSize = 13;
            statusTextComp.alignment = TextAnchor.MiddleCenter;
            statusTextComp.font = font;

            var activity = UnityEngine.Object.FindAnyObjectByType<FindFaultActivity>();
            if (activity != null)
            {
                activity.faultDescriptionText = descText;
                activity.hintText = hintTextComp;
                activity.resultText = resultTextComp;
                activity.solveButton = solveBtn;
                activity.statusText = statusTextComp;
            }

            return panelObj;
        }

        public static GameObject CreateRoutingTablesPanel(Transform canvas, Action onBack)
        {
            Font font = UIPanelFactory.GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(650, 500), 25,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "RoutingTablesPanel";
            UIComp.CreateMenuTitle(panelObj.transform, "Tabla de Enrutamiento Tangible", 24, new Vector2(0, 200), font);

            var leftPanel = new GameObject("LeftPanel");
            leftPanel.transform.SetParent(panelObj.transform, false);
            var leftRect = leftPanel.AddComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0.5f, 0.5f);
            leftRect.anchorMax = new Vector2(0.5f, 0.5f);
            leftRect.anchoredPosition = new Vector2(-140, 20);
            leftRect.sizeDelta = new Vector2(280, 300);
            var leftText = leftPanel.AddComponent<Text>();
            leftText.text =                 "TABLA DE ENRUTAMIENTO TANGIBLE\n\n" +
                "Visualiza la tabla de\n" +
                "enrutamiento de cada router.\n\n" +
                "ATENCION:\n  1 = Router\n  4 = Enlace\n  P = Ping\n\n" +
                "Presiona ACTUALIZAR para\n" +
                "refrescar las tablas.";
            leftText.color = UIColors.textPrimary;
            leftText.fontSize = 14;
            leftText.alignment = TextAnchor.UpperLeft;
            leftText.font = font;
            leftText.lineSpacing = 1.1f;

            var rightPanel = new GameObject("RightPanel");
            rightPanel.transform.SetParent(panelObj.transform, false);
            var rightRect = rightPanel.AddComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(0.5f, 0.5f);
            rightRect.anchorMax = new Vector2(0.5f, 0.5f);
            rightRect.anchoredPosition = new Vector2(140, 20);
            rightRect.sizeDelta = new Vector2(280, 300);
            var rightText = rightPanel.AddComponent<Text>();
            rightText.text = "RUTAS:\n\nNo hay routers.\n\nUsa la tecla 1 o coloca\ndiscos para anadir\nrouters.";
            rightText.color = UIColors.textAccent;
            rightText.fontSize = 13;
            rightText.alignment = TextAnchor.UpperLeft;
            rightText.font = font;

            var routingAct = UnityEngine.Object.FindAnyObjectByType<RoutingTablesActivity>();
            if (routingAct != null)
            {
                routingAct.tableText = rightText;
                routingAct.infoText = leftText;
            }

            Button refreshBtn = UIComp.CreateMenuButton(panelObj.transform, "RefreshBtn", "ACTUALIZAR", new Vector2(-80, -200), new Vector2(130, 40), font, 14);
            if (routingAct != null)
                refreshBtn.onClick.AddListener(() => routingAct.RefreshRoutingTables());

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "VOLVER", new Vector2(80, -200), new Vector2(130, 40), font, 14);
            backBtn.onClick.AddListener(() => onBack?.Invoke());

            return panelObj;
        }

        public static GameObject CreateStaticRoutingPanel(Transform canvas, Action onBack)
        {
            Font font = UIPanelFactory.GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(650, 500), 25,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "StaticRoutingPanel";
            UIComp.CreateMenuTitle(panelObj.transform, "Enrutamiento Est\u00e1tico Tangible", 24, new Vector2(0, 200), font);

            var leftPanel = new GameObject("LeftPanel");
            leftPanel.transform.SetParent(panelObj.transform, false);
            var leftRect = leftPanel.AddComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0.5f, 0.5f);
            leftRect.anchorMax = new Vector2(0.5f, 0.5f);
            leftRect.anchoredPosition = new Vector2(-140, 20);
            leftRect.sizeDelta = new Vector2(280, 300);
            var leftText = leftPanel.AddComponent<Text>();
            leftText.text = "ENRUTAMIENTO EST\u00c1TICO TANGIBLE\n\n" +
                "Configura manualmente\nrutas en los routers.\n\n" +
                "BOTONES:\n" +
                "  AÑADIR MANUAL = Ruta propia\n" +
                "  AÑADIR RUTA = Ruta ejemplo\n" +
                "  TEST = Probar enrutamiento\n\n" +
                "  P = Ping (teclado)";
            leftText.color = UIColors.textPrimary;
            leftText.fontSize = 14;
            leftText.alignment = TextAnchor.UpperLeft;
            leftText.font = font;
            leftText.lineSpacing = 1.1f;

            var rightPanel = new GameObject("RightPanel");
            rightPanel.transform.SetParent(panelObj.transform, false);
            var rightRect = rightPanel.AddComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(0.5f, 0.5f);
            rightRect.anchorMax = new Vector2(0.5f, 0.5f);
            rightRect.anchoredPosition = new Vector2(140, 20);
            rightRect.sizeDelta = new Vector2(280, 300);
            var rightText = rightPanel.AddComponent<Text>();
            rightText.text = "RUTAS CONFIGURADAS:\n\nPresiona AÑADIR RUTA\npara agregar rutas de\nejemplo al router.";
            rightText.color = UIColors.textAccent;
            rightText.fontSize = 12;
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
                feedbackRect.anchoredPosition = new Vector2(0, 125);
                feedbackRect.sizeDelta = new Vector2(500, 30);
                var feedbackTextComp = feedbackObj.AddComponent<Text>();
                feedbackTextComp.text = "";
                feedbackTextComp.color = UIColors.textAccent;
                feedbackTextComp.fontSize = 14;
                feedbackTextComp.alignment = TextAnchor.MiddleCenter;
                feedbackTextComp.font = font;
                staticAct.feedbackText = feedbackTextComp;
            }

            Button manualBtn = UIComp.CreateMenuButton(panelObj.transform, "ManualRouteBtn", "AÑADIR MANUAL", new Vector2(-140, -100), new Vector2(130, 40), font, 14);
            if (staticAct != null)
                manualBtn.onClick.AddListener(() => staticAct.ShowAddRoutePanel());

            Button addBtn = UIComp.CreateMenuButton(panelObj.transform, "AddRouteBtn", "AÑADIR RUTA", new Vector2(0, -100), new Vector2(130, 40), font, 14);
            if (staticAct != null)
                addBtn.onClick.AddListener(() => staticAct.AddSampleRoute());

            Button testBtn = UIComp.CreateMenuButton(panelObj.transform, "TestRouteBtn", "TEST", new Vector2(140, -100), new Vector2(130, 40), font, 14);
            if (staticAct != null)
                testBtn.onClick.AddListener(() => staticAct.TestRouting());

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "VOLVER", new Vector2(0, -170), new Vector2(140, 40), font, 14);
            backBtn.onClick.AddListener(() => onBack?.Invoke());
            return panelObj;
        }

        public static GameObject CreateDynamicRoutingPanel(Transform canvas,
            Action onSelectRIP, Action onSelectOSPF, Action<GameObject> onStart,
            Action<GameObject> onStop, Action<GameObject> onClearRoutes,
            Action<GameObject> onViewRoutes, Action onBack,
            Action<string> onSetNeighbor = null, Action<string> onSetNetwork = null,
            Action<int> onSetCost = null, Action<int> onSetBW = null)
        {
            Font font = UIPanelFactory.GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(750, 720), 25,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "DynamicRoutingPanel";
            UIComp.CreateMenuTitle(panelObj.transform, "Protocolo de Enrutamiento Din\u00e1mico Tangible", 24, new Vector2(0, 260), font);

            var leftPanel = new GameObject("LeftPanel");
            leftPanel.transform.SetParent(panelObj.transform, false);
            var leftRect = leftPanel.AddComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0.5f, 0.5f);
            leftRect.anchorMax = new Vector2(0.5f, 0.5f);
            leftRect.anchoredPosition = new Vector2(-180, 10);
            leftRect.sizeDelta = new Vector2(320, 340);
            var leftText = leftPanel.AddComponent<Text>();
            leftText.text =                 "PROTOCOLO DE ENRUTAMIENTO\n" +
                "DIN\u00c1MICO TANGIBLE\n\n" +
                "RIP (conteo de hops)\n" +
                "OSPF (costo por enlace)\n\n" +
                "BOTONES:\n  START = Iniciar envio\n  STOP = Detener\n  LIMPIAR = Borrar rutas\n  VER RUTAS = Mostrar\n\n" +
                "Usa teclas 1-4 para\ncrear la topologia.";
            leftText.color = UIColors.textPrimary;
            leftText.fontSize = 14;
            leftText.alignment = TextAnchor.UpperLeft;
            leftText.font = font;
            leftText.lineSpacing = 1.2f;

            var rightPanel = new GameObject("RightPanel");
            rightPanel.transform.SetParent(panelObj.transform, false);
            var rightRect = rightPanel.AddComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(0.5f, 0.5f);
            rightRect.anchorMax = new Vector2(0.5f, 0.5f);
            rightRect.anchoredPosition = new Vector2(180, 10);
            rightRect.sizeDelta = new Vector2(320, 340);
            var rightText = rightPanel.AddComponent<Text>();
            rightText.text = "SIMULACION:\n\nProtocolo: Ninguno\n\nPresiona START para\niniciar el protocolo.";
            rightText.color = UIColors.textAccent;
            rightText.fontSize = 14;
            rightText.alignment = TextAnchor.UpperLeft;
            rightText.font = font;
            rightText.name = "StatusText";

            float btnY = -165, btnSpacing = 130;

            var dynAct = UnityEngine.Object.FindAnyObjectByType<DynamicRoutingActivity>();

            Button ripBtn = UIComp.CreateMenuButton(panelObj.transform, "RIPBtn", "RIP", new Vector2(-btnSpacing, btnY), new Vector2(100, 45), font, 16);
            ripBtn.onClick.AddListener(() => {
                UIPanelFactory.UpdateStatusText(panelObj, "RIP");
                if (dynAct != null) dynAct.SetProtocol(RoutingProtocol.RIP);
            });

            Button ospfBtn = UIComp.CreateMenuButton(panelObj.transform, "OSBFBtn", "OSPF", new Vector2(0, btnY), new Vector2(100, 45), font, 16);
            ospfBtn.onClick.AddListener(() => {
                UIPanelFactory.UpdateStatusText(panelObj, "OSPF");
                if (dynAct != null) dynAct.SetProtocol(RoutingProtocol.OSPF);
            });

            Button startBtn = UIComp.CreateMenuButton(panelObj.transform, "StartBtn", "START", new Vector2(btnSpacing, btnY), new Vector2(100, 45), font, 16);
            startBtn.onClick.AddListener(() => onStart?.Invoke(panelObj));

            btnY -= 60;
            Button stopBtn = UIComp.CreateMenuButton(panelObj.transform, "StopBtn", "STOP", new Vector2(-btnSpacing, btnY), new Vector2(100, 40), font, 14);
            stopBtn.onClick.AddListener(() => onStop?.Invoke(panelObj));

            Button clearBtn = UIComp.CreateMenuButton(panelObj.transform, "ClearBtn", "LIMPIAR", new Vector2(0, btnY), new Vector2(100, 40), font, 14);
            clearBtn.onClick.AddListener(() => onClearRoutes?.Invoke(panelObj));

            Button viewBtn = UIComp.CreateMenuButton(panelObj.transform, "ViewBtn", "VER RUTAS", new Vector2(btnSpacing, btnY), new Vector2(100, 40), font, 14);
            viewBtn.onClick.AddListener(() => onViewRoutes?.Invoke(panelObj));

            var protocolStatusObj = new GameObject("ProtocolStatusText");
            protocolStatusObj.transform.SetParent(panelObj.transform, false);
            var protoRect = protocolStatusObj.AddComponent<RectTransform>();
            protoRect.anchorMin = new Vector2(0.5f, 0.5f);
            protoRect.anchorMax = new Vector2(0.5f, 0.5f);
            protoRect.anchoredPosition = new Vector2(0, 195);
            protoRect.sizeDelta = new Vector2(400, 30);
            var protoTextComp = protocolStatusObj.AddComponent<Text>();
            protoTextComp.text = "Protocolo: RIP";
            protoTextComp.color = new Color(0.3f, 0.8f, 1f);
            protoTextComp.fontSize = 16;
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
                feedbackRect.anchoredPosition = new Vector2(0, 200);
                feedbackRect.sizeDelta = new Vector2(500, 30);
                var feedbackTextComp = feedbackObj.AddComponent<Text>();
                feedbackTextComp.text = "";
                feedbackTextComp.color = UIColors.textAccent;
                feedbackTextComp.fontSize = 14;
                feedbackTextComp.alignment = TextAnchor.MiddleCenter;
                feedbackTextComp.font = font;
                dynAct.feedbackText = feedbackTextComp;
            }

            var discConfigLabel = new GameObject("DiscConfigLabel");
            discConfigLabel.transform.SetParent(panelObj.transform, false);
            var dclRect = discConfigLabel.AddComponent<RectTransform>();
            dclRect.anchorMin = new Vector2(0.5f, 0f);
            dclRect.anchorMax = new Vector2(0.5f, 0f);
            dclRect.pivot = new Vector2(0.5f, 1f);
            dclRect.anchoredPosition = new Vector2(0, -60);
            dclRect.sizeDelta = new Vector2(650, 20);
            var dclText = discConfigLabel.AddComponent<Text>();
            dclText.text = "--- CONFIG. AVANZADA (Discos Virtuales 15-18) ---";
            dclText.color = new Color(0.6f, 0.6f, 0.6f);
            dclText.fontSize = 11;
            dclText.alignment = TextAnchor.MiddleCenter;
            dclText.font = font;

            Text CreateConfigLabel(Transform parent, string text, Vector2 pos, float w, float h, int fsize)
            {
                var obj = new GameObject("ConfigLabel");
                obj.transform.SetParent(parent, false);
                var r = obj.AddComponent<RectTransform>();
                r.anchorMin = new Vector2(0.5f, 0.5f);
                r.anchorMax = new Vector2(0.5f, 0.5f);
                r.anchoredPosition = pos;
                r.sizeDelta = new Vector2(w, h);
                var t = obj.AddComponent<Text>();
                t.text = text; t.font = font; t.fontSize = fsize;
                t.color = UIColors.textSecondary;
                t.alignment = TextAnchor.MiddleLeft;
                return t;
            }

            float discY = -90;
            CreateConfigLabel(panelObj.transform, "Vecino (15):", new Vector2(-250, discY), 200, 20, 11);
            var neighborInput = UIComp.CreateInputField(panelObj.transform, "NeighborInput", new Vector2(-120, discY), new Vector2(140, 28), "", "Router1", font, 13);
            Button neighborBtn = UIComp.CreateMenuButton(panelObj.transform, "NeighborBtn", "APLICAR", new Vector2(40, discY), new Vector2(80, 28), font, 11);
            if (onSetNeighbor != null) neighborBtn.onClick.AddListener(() => onSetNeighbor(neighborInput.text));

            discY -= 35;
            CreateConfigLabel(panelObj.transform, "Anunciar Red (16):", new Vector2(-250, discY), 200, 20, 11);
            var networkInput = UIComp.CreateInputField(panelObj.transform, "NetworkInput", new Vector2(-120, discY), new Vector2(140, 28), "", "10.0.0.0/8", font, 13);
            Button networkBtn = UIComp.CreateMenuButton(panelObj.transform, "NetworkBtn", "APLICAR", new Vector2(40, discY), new Vector2(80, 28), font, 11);
            if (onSetNetwork != null) networkBtn.onClick.AddListener(() => onSetNetwork(networkInput.text));

            discY -= 35;
            CreateConfigLabel(panelObj.transform, "Costo OSPF (17):", new Vector2(-250, discY), 200, 20, 11);
            var costInput = UIComp.CreateInputField(panelObj.transform, "CostInput", new Vector2(-120, discY), new Vector2(140, 28), "10", "10", font, 13);
            Button costBtn = UIComp.CreateMenuButton(panelObj.transform, "CostBtn", "APLICAR", new Vector2(40, discY), new Vector2(80, 28), font, 11);
            if (onSetCost != null) costBtn.onClick.AddListener(() => {
                if (int.TryParse(costInput.text, out int cost)) onSetCost(cost);
            });

            discY -= 35;
            CreateConfigLabel(panelObj.transform, "Ancho Banda (18):", new Vector2(-250, discY), 200, 20, 11);
            var bwInput = UIComp.CreateInputField(panelObj.transform, "BWInput", new Vector2(-120, discY), new Vector2(140, 28), "1000", "1000", font, 13);
            Button bwBtn = UIComp.CreateMenuButton(panelObj.transform, "BWBtn", "APLICAR", new Vector2(40, discY), new Vector2(80, 28), font, 11);
            if (onSetBW != null) bwBtn.onClick.AddListener(() => {
                if (int.TryParse(bwInput.text, out int bw)) onSetBW(bw);
            });

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "VOLVER", new Vector2(0, discY - 40), new Vector2(140, 40), font, 14);
            backBtn.onClick.AddListener(() => onBack?.Invoke());
            return panelObj;
        }

        public static GameObject CreateScenariosPanel(Transform canvas,
            List<PredefinedScenarios.NetworkScenario> scenarios,
            Action<int> onScenarioClick, Action onBack)
        {
            Font font = UIPanelFactory.GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(800, 800), 25,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "ScenariosPanel";
            UIComp.CreateMenuTitle(panelObj.transform, "Escenarios Preconfigurados", 32, new Vector2(0, 320), font);

            float startY = 170;
            float itemSpacing = 115;
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
                itemRect.sizeDelta = new Vector2(700, 100);

                var itemImg = itemBg.AddComponent<Image>();
                Texture2D itemTex = UIComp.CreateRoundedRectTexture(700, 100, 15, UIColors.surfaceElevated, UIColors.borderAccent, 3f);
                itemImg.sprite = Sprite.Create(itemTex, new Rect(0, 0, 700, 100), new Vector2(0.5f, 0.5f), 100);
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
                nameRect.anchoredPosition = new Vector2(20, 18);
                nameRect.offsetMin = Vector2.zero;
                nameRect.offsetMax = new Vector2(0, 25);
                var nameText = nameObj.AddComponent<Text>();
                nameText.text = scenario.name;
                nameText.font = font;
                nameText.fontSize = 18;
                nameText.color = UIColors.textPrimary;
                nameText.fontStyle = FontStyle.Bold;
                nameText.alignment = TextAnchor.MiddleLeft;
                nameText.resizeTextForBestFit = true;
                nameText.resizeTextMinSize = 14;

                var diffObj = new GameObject("Difficulty");
                diffObj.transform.SetParent(itemBg.transform, false);
                var diffRect = diffObj.AddComponent<RectTransform>();
                diffRect.anchorMin = new Vector2(0.5f, 0.5f);
                diffRect.anchorMax = new Vector2(1f, 0.5f);
                diffRect.pivot = new Vector2(0.5f, 0.5f);
                diffRect.anchoredPosition = new Vector2(0, 18);
                diffRect.offsetMin = Vector2.zero;
                diffRect.offsetMax = new Vector2(-20, 25);
                var diffText = diffObj.AddComponent<Text>();
                diffText.text = difficultyText;
                diffText.font = font;
                diffText.fontSize = 13;
                diffText.color = diffColor;
                diffText.fontStyle = FontStyle.Bold;
                diffText.alignment = TextAnchor.MiddleCenter;

                var descObj = new GameObject("Description");
                descObj.transform.SetParent(itemBg.transform, false);
                var descRect = descObj.AddComponent<RectTransform>();
                descRect.anchorMin = new Vector2(0f, 0.5f);
                descRect.anchorMax = new Vector2(1f, 0.5f);
                descRect.pivot = new Vector2(0.5f, 0.5f);
                descRect.anchoredPosition = new Vector2(0, -12);
                descRect.sizeDelta = new Vector2(660, 25);
                var descText = descObj.AddComponent<Text>();
                descText.text = scenario.description;
                descText.font = font;
                descText.fontSize = 14;
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

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "VOLVER", new Vector2(0, -360), new Vector2(200, 55), font, 20);
            backBtn.onClick.AddListener(() => onBack?.Invoke());
            return panelObj;
        }

        public static GameObject CreateScenarioInfoPanel(Transform canvas,
            PredefinedScenarios.NetworkScenario scenario,
            Action onStart, Action onCancel)
        {
            Font font = UIPanelFactory.GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(550, 500), 20,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "ScenarioInfoPanel";

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;

            UIComp.CreateMenuTitle(panelObj.transform, scenario.name, 22, new Vector2(0, 200), font);

            float yPos = 155;
            float lineHeight = 30;

            UIComp.CreateInfoText(panelObj.transform, "OBJETIVOS:", new Vector2(0, yPos), font, 14, UIColors.textAccent, true);
            yPos -= lineHeight;

            foreach (var obj in scenario.objectives)
            {
                UIComp.CreateInfoText(panelObj.transform, "- " + obj, new Vector2(-20, yPos), font, 12, UIColors.textPrimary, false);
                yPos -= lineHeight - 5;
            }

            yPos -= 10;
            UIComp.CreateInfoText(panelObj.transform, "AYUDAS:", new Vector2(0, yPos), font, 14, UIColors.textSecondary, true);
            yPos -= lineHeight;

            foreach (var hint in scenario.hints)
            {
                UIComp.CreateInfoText(panelObj.transform, "* " + hint, new Vector2(-20, yPos), font, 11, UIColors.textSecondary, false);
                yPos -= lineHeight - 5;
            }

            Button startBtn = UIComp.CreateMenuButton(panelObj.transform, "StartScenarioBtn", "INICIAR", new Vector2(-80, -200), new Vector2(130, 45), font, 16);
            startBtn.onClick.AddListener(() => { onStart?.Invoke(); UnityEngine.Object.Destroy(panelObj); });

            Button cancelBtn = UIComp.CreateMenuButton(panelObj.transform, "CancelBtn", "CANCELAR", new Vector2(80, -200), new Vector2(130, 45), font, 16);
            cancelBtn.onClick.AddListener(() => { onCancel?.Invoke(); UnityEngine.Object.Destroy(panelObj); });

            return panelObj;
        }
    }
}
