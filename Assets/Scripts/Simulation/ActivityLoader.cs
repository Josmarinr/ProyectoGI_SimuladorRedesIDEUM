using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections.Generic;
using SimRedes.Network;
using SimRedes.Tangible;
using SimRedes.UI;
using UIComp = SimRedes.UI.UIComponents;
using UIColors = SimRedes.UI.UIComponents.Colors;

namespace SimRedes.Simulation
{
    public class ActivityLoader : MonoBehaviour
    {
        internal string selectedProtocol = null;
        private DynamicRoutingProtocol dynProtocol = null;
        private TopologyManager topology;
        private Canvas canvas;
        private Font font;

        private void Start()
        {
            topology = FindObjectOfType<TopologyManager>();
            canvas = FindObjectOfType<Canvas>();
        }

        public void SetSelectedProtocol(string protocol)
        {
            selectedProtocol = protocol;
        }

        private void EnsureTopology()
        {
            if (topology == null) topology = FindObjectOfType<TopologyManager>();
        }

        public void SelectActivity(int activityIndex)
        {
            UnityEngine.Debug.Log("[ActivityLoader] Actividad seleccionada: " + activityIndex);

            var activitiesPanel = GameObject.Find("ActivitiesPanel");
            if (activitiesPanel != null) Destroy(activitiesPanel);

            var gameManagerObj = GameObject.Find("GameManager");
            if (gameManagerObj == null)
                gameManagerObj = new GameObject("GameManager");

            if (gameManagerObj.GetComponent<BuildTopologyActivity>() == null)
                gameManagerObj.AddComponent<BuildTopologyActivity>();

            if (gameManagerObj.GetComponent<SimulationControls>() == null)
                gameManagerObj.AddComponent<SimulationControls>();

            if (canvas == null) canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            CreateSimulationHUDPanel(canvas.transform);

            switch (activityIndex)
            {
                case 0:
                    UIPanelFactory.CreateBuildTopologyInfoPanel(canvas.transform);
                    UnityEngine.Debug.Log("[ActivityLoader] Iniciando: Construir Topologia");
                    break;
                case 1:
                    if (gameManagerObj.GetComponent<FindFaultActivity>() == null)
                        gameManagerObj.AddComponent<FindFaultActivity>();
                    UnityEngine.Debug.Log("[ActivityLoader] Iniciando: Encontrar Fallos");
                    break;
                case 2:
                    if (gameManagerObj.GetComponent<RoutingTablesActivity>() == null)
                        gameManagerObj.AddComponent<RoutingTablesActivity>();
                    UIPanelFactory.CreateRoutingTablesPanel(canvas.transform, () => GoBackToMainMenu());
                    UnityEngine.Debug.Log("[ActivityLoader] Iniciando: Tablas de Enrutamiento");
                    break;
                case 3:
                    if (gameManagerObj.GetComponent<BestRouteActivity>() == null)
                        gameManagerObj.AddComponent<BestRouteActivity>();
                    UIPanelFactory.CreateBestRoutePanel(canvas.transform);
                    UnityEngine.Debug.Log("[ActivityLoader] Iniciando: Mejor Ruta");
                    break;
                case 4:
                    if (gameManagerObj.GetComponent<StaticRoutingActivity>() == null)
                        gameManagerObj.AddComponent<StaticRoutingActivity>();
                    UIPanelFactory.CreateStaticRoutingPanel(canvas.transform, () => GoBackToMainMenu());
                    UnityEngine.Debug.Log("[ActivityLoader] Iniciando: Enrutamiento Estatico");
                    break;
                case 5:
                    if (gameManagerObj.GetComponent<DynamicRoutingActivity>() == null)
                        gameManagerObj.AddComponent<DynamicRoutingActivity>();
                    UIPanelFactory.CreateDynamicRoutingPanel(
                        canvas.transform,
                        onSelectRIP: () => SetSelectedProtocol("RIP"),
                        onSelectOSPF: () => SetSelectedProtocol("OSPF"),
                        onStart: (panel) => StartDynamicProtocol(panel),
                        onStop: (panel) => StopDynamicProtocol(panel),
                        onClearRoutes: (panel) => ClearDynamicRoutes(panel),
                        onViewRoutes: (panel) => ShowAllRouterRoutes(panel),
                        onBack: () => GoBackToMainMenu()
                    );
                    UnityEngine.Debug.Log("[ActivityLoader] Iniciando: Enrutamiento Dinamico");
                    break;
                case 6:
                    CreateScenariosPanel(canvas.transform);
                    UnityEngine.Debug.Log("[ActivityLoader] Iniciando: Escenarios Preconfigurados");
                    break;
            }

            if (gameManagerObj.GetComponent<BuildTopologyActivity>() == null)
                gameManagerObj.AddComponent<BuildTopologyActivity>();
        }

        public void StartSimulation(Transform canvasTransform)
        {
            var menuMgr = FindObjectOfType<MainMenuManager>();
            if (menuMgr != null) Destroy(menuMgr.gameObject);

            var menuNavigator = FindObjectOfType<MenuNavigator>();
            if (menuNavigator != null) Destroy(menuNavigator.gameObject);

            var mainMenu = GameObject.Find("MainMenuPanel");
            if (mainMenu != null) Destroy(mainMenu);

            var gameManagerObj = GameObject.Find("GameManager");
            if (gameManagerObj == null)
                gameManagerObj = new GameObject("GameManager");

            if (canvas == null) canvas = FindObjectOfType<Canvas>();

            CreateSimulationHUDPanel(canvasTransform);

            var scoring = FindObjectOfType<ScoringSystem>();
            if (scoring == null)
            {
                var scoringObj = new GameObject("ScoringSystem");
                scoringObj.AddComponent<ScoringSystem>();
            }
            ScoringSystem.Instance?.StartSession("Simulacion Libre");

            if (gameManagerObj.GetComponent<BuildTopologyActivity>() == null)
                gameManagerObj.AddComponent<BuildTopologyActivity>();

            if (gameManagerObj.GetComponent<SimulationControls>() == null)
                gameManagerObj.AddComponent<SimulationControls>();

            var existingStatus = GameObject.Find("StatusPanel");
            if (existingStatus != null) Destroy(existingStatus);

            UnityEngine.Debug.Log("[ActivityLoader] Simulacion iniciada");
        }

        public void ShowConnectivityPanel(Transform canvasTransform)
        {
            var menuMgr = FindObjectOfType<MainMenuManager>();
            if (menuMgr != null) Destroy(menuMgr.gameObject);

            var mainMenu = GameObject.Find("MainMenuPanel");
            if (mainMenu != null) Destroy(mainMenu);

            if (canvas == null) canvas = FindObjectOfType<Canvas>();

            Font arialFont = UIComp.GetFont();

            GameObject panelObj = UIComp.CreateRoundedPanel(canvasTransform, new Vector2(550, 480), 25,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "ConnectivityPanel";

            UIComp.CreateMenuTitle(panelObj.transform, "Test de Conectividad", 28, new Vector2(0, 195), arialFont);

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
            sourceText.font = arialFont;

            var arrowObj = new GameObject("Arrow");
            arrowObj.transform.SetParent(panelObj.transform, false);
            var arrowRect = arrowObj.AddComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(0.5f, 0.5f);
            arrowRect.anchorMax = new Vector2(0.5f, 0.5f);
            arrowRect.anchoredPosition = new Vector2(0, 100);
            arrowRect.sizeDelta = new Vector2(60, 30);
            var arrowText = arrowObj.AddComponent<Text>();
            arrowText.text = "\u2192 \u2192 \u2192";
            arrowText.color = UIColors.textAccent;
            arrowText.fontSize = 20;
            arrowText.alignment = TextAnchor.MiddleCenter;
            arrowText.font = arialFont;

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
            destText.font = arialFont;

            Button pingBtn = UIComp.CreateMenuButton(panelObj.transform, "PingButton", "HACER PING",
                new Vector2(0, 40), new Vector2(180, 55), arialFont);

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
            resultIconInner.font = arialFont;

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
            resultText.font = arialFont;

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
            statusText.font = arialFont;

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
            hintText.font = arialFont;

            var connectTestPanel = panelObj.AddComponent<ConnectivityTestPanel>();
            connectTestPanel.Initialize(sourceText, destText, resultText, resultIconInner, pingBtn, statusText);

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "VOLVER",
                new Vector2(0, -215), new Vector2(160, 45), arialFont);
            backBtn.onClick.AddListener(() => GoBackToMainMenu());

            var navigator = FindObjectOfType<MenuNavigator>();
            if (navigator != null)
                navigator.SetupPanel(panelObj, () => GoBackToMainMenu());

            UnityEngine.Debug.Log("[ActivityLoader] Panel de conectividad creado");
        }

        private void CreateSimulationHUDPanel(Transform ct)
        {
            var existingPanel = GameObject.Find("TopologyInfoPanel");
            if (existingPanel != null) Destroy(existingPanel);

            var existingInfo = GameObject.Find("TopologyExamplePanel");
            if (existingInfo != null) Destroy(existingInfo);

            font = UIComp.GetFont();

            GameObject panelObj = UIComp.CreateRoundedPanel(ct, new Vector2(420, 520), 20,
                UIColors.surfaceElevated, UIColors.borderAccent);
            panelObj.name = "TopologyInfoPanel";

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);
            panelRect.anchoredPosition = new Vector2(-20, -20);

            UIComp.CreateMenuTitle(panelObj.transform, "Topologia", 26, new Vector2(0, 210), font);

            Button infoBtn = UIComp.CreateSmallInfoButton(panelObj.transform, new Vector2(165, 210), font);
            infoBtn.onClick.AddListener(() => UIPanelFactory.ToggleTopologyExamplePanel(ct, font));

            float infoY = 155f;
            float lineHeight = 32f;

            var topologyTypeText = UIComp.CreateInfoText(panelObj.transform, "Topologia: Sin topologia",
                new Vector2(0, infoY), font, 19, UIColors.textAccent, true);
            topologyTypeText.gameObject.name = "TopologyTypeText";

            infoY -= lineHeight;
            var linkText = UIComp.CreateInfoText(panelObj.transform, "Enlaces: 0",
                new Vector2(0, infoY), font, 18, UIColors.textPrimary, true);
            linkText.gameObject.name = "LinkCountText";

            infoY -= lineHeight + 12;
            var devicesTitle = UIComp.CreateInfoText(panelObj.transform, "Dispositivos:",
                new Vector2(0, infoY), font, 17, UIColors.textSecondary, false);
            UIComp.ApplyTitleStyle(devicesTitle);

            infoY -= 28;
            var routerText = UIComp.CreateInfoText(panelObj.transform, "  Routers: 0",
                new Vector2(0, infoY), font, 17, UIColors.textSecondary, false);
            routerText.gameObject.name = "RouterCountText";

            infoY -= 26;
            var switchText = UIComp.CreateInfoText(panelObj.transform, "  Switches: 0",
                new Vector2(0, infoY), font, 17, UIColors.textSecondary, false);
            switchText.gameObject.name = "SwitchCountText";

            infoY -= 26;
            var pcText = UIComp.CreateInfoText(panelObj.transform, "  PCs: 0",
                new Vector2(0, infoY), font, 17, UIColors.textSecondary, false);
            pcText.gameObject.name = "PCCountText";

            infoY -= lineHeight + 10;
            var pingResultText = UIComp.CreateInfoText(panelObj.transform, "Ping: Seleccionar origen",
                new Vector2(0, infoY), font, 18, UIColors.textSecondary, false);
            pingResultText.gameObject.name = "PingResultText";

            float btnY = -95f;
            float btnSpacing = 115f;

            Button clearBtn = UIComp.CreateMenuButton(panelObj.transform, "ClearBtn", "LIMPIAR",
                new Vector2(-btnSpacing, btnY), new Vector2(105, 44), font, 16);
            clearBtn.onClick.AddListener(() => {
                var cleanup = FindObjectOfType<SceneCleanupService>();
                if (cleanup != null)
                    cleanup.ClearSimulation(FindObjectOfType<TopologyManager>(),
                        FindObjectOfType<NodeVisualizer>(),
                        FindObjectOfType<DebugDiscSimulator>());
            });

            Button pingBtn = UIComp.CreateMenuButton(panelObj.transform, "PingBtn", "PING",
                new Vector2(0, btnY), new Vector2(105, 44), font, 16);
            var pingCtrl = FindObjectOfType<PingModeController>();
            if (pingCtrl != null)
            {
                pingCtrl.StorePingReferences(pingBtn, pingResultText);
                pingBtn.onClick.AddListener(() => pingCtrl.TogglePingMode());
            }

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "VOLVER",
                new Vector2(btnSpacing, btnY), new Vector2(105, 44), font, 16);
            backBtn.onClick.AddListener(() => GoBackToMainMenu());

            float linkBtnY = -150f;
            float linkBtnSpacing = 120f;

            Button connectBtn = UIComp.CreateMenuButton(panelObj.transform, "ConnectBtn", "CONECTAR",
                new Vector2(-linkBtnSpacing, linkBtnY), new Vector2(105, 44), font, 15);
            var linkCtrl = FindObjectOfType<LinkModeController>();
            if (linkCtrl != null)
            {
                connectBtn.onClick.AddListener(() => linkCtrl.ToggleLinkMode("connect"));
            }

            Button disconnectBtn = UIComp.CreateMenuButton(panelObj.transform, "DisconnectBtn", "DESCONECTAR",
                new Vector2(linkBtnSpacing, linkBtnY), new Vector2(105, 44), font, 13);
            if (linkCtrl != null)
            {
                disconnectBtn.onClick.AddListener(() => linkCtrl.ToggleLinkMode("disconnect"));
                linkCtrl.StoreLinkButtons(connectBtn, disconnectBtn);
            }

            float advBtnY = -210f;
            float advSpacing = 100f;

            Button vlanBtn = UIComp.CreateMenuButton(panelObj.transform, "VLANBtn", "VLAN",
                new Vector2(-advSpacing, advBtnY), new Vector2(90, 35), font, 14);
            vlanBtn.onClick.AddListener(() => CreateNetworkAdvancedPanel("VLAN"));

            Button aclBtn = UIComp.CreateMenuButton(panelObj.transform, "ACLBtn", "ACL",
                new Vector2(0, advBtnY), new Vector2(90, 35), font, 14);
            aclBtn.onClick.AddListener(() => CreateNetworkAdvancedPanel("ACL"));

            Button natBtn = UIComp.CreateMenuButton(panelObj.transform, "NATBtn", "NAT",
                new Vector2(advSpacing, advBtnY), new Vector2(90, 35), font, 14);
            natBtn.onClick.AddListener(() => CreateNetworkAdvancedPanel("NAT"));

            // Create DevicesPanel and ScorePanel via controllers
            var devicePanelCtrl = FindObjectOfType<DevicePanelController>();
            if (devicePanelCtrl != null) devicePanelCtrl.RefreshDevicesPanel();

            UIPanelFactory.CreateScorePanel(ct);

            UnityEngine.Debug.Log("[ActivityLoader] Panel de topologia creado");
        }

        private void CreateNetworkAdvancedPanel(string type)
        {
            var topo = FindObjectOfType<TopologyManager>();
            if (topo == null) return;

            if (canvas == null) canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            switch (type)
            {
                case "VLAN": UIPanelFactory.CreateVLANPanel(canvas.transform, topo); break;
                case "ACL": UIPanelFactory.CreateACLPanel(canvas.transform, topo); break;
                case "NAT": UIPanelFactory.CreateNATPanel(canvas.transform, topo); break;
            }
        }

        private void CreateScenariosPanel(Transform canvasTransform)
        {
            var existingPanel = GameObject.Find("ScenariosPanel");
            if (existingPanel != null) Destroy(existingPanel);

            var gameManager = GameObject.Find("GameManager");
            if (gameManager == null)
                gameManager = new GameObject("GameManager");

            if (gameManager.GetComponent<PredefinedScenarios>() == null)
                gameManager.AddComponent<PredefinedScenarios>();

            if (PredefinedScenarios.Instance == null)
            {
                UnityEngine.Debug.LogError("[Scenarios] PredefinedScenarios.Instance es null");
                return;
            }

            var scenarios = PredefinedScenarios.Instance.GetScenarios();
            if (scenarios.Count == 0)
            {
                scenarios.Add(new PredefinedScenarios.NetworkScenario
                {
                    name = "Prueba Rapida",
                    description = "Un router y dos PCs para probar conectividad",
                    difficulty = PredefinedScenarios.ScenarioDifficulty.Basico,
                    devices = new System.Collections.Generic.List<PredefinedScenarios.DeviceConfig>
                    {
                        new PredefinedScenarios.DeviceConfig { type = "Router", x = 0.5f, y = 0.5f },
                        new PredefinedScenarios.DeviceConfig { type = "PC", x = 0.3f, y = 0.3f },
                        new PredefinedScenarios.DeviceConfig { type = "PC", x = 0.7f, y = 0.3f }
                    },
                    links = new System.Collections.Generic.List<PredefinedScenarios.LinkConfig>
                    {
                        new PredefinedScenarios.LinkConfig { fromIndex = 0, toIndex = 1 },
                        new PredefinedScenarios.LinkConfig { fromIndex = 0, toIndex = 2 }
                    },
                    ipConfigurations = new System.Collections.Generic.List<PredefinedScenarios.IPConfig>
                    {
                        new PredefinedScenarios.IPConfig { deviceIndex = 0, ip = "192.168.1.1", mask = "255.255.255.0" },
                        new PredefinedScenarios.IPConfig { deviceIndex = 1, ip = "192.168.1.10", mask = "255.255.255.0" },
                        new PredefinedScenarios.IPConfig { deviceIndex = 2, ip = "192.168.1.20", mask = "255.255.255.0" }
                    }
                });
            }

            UIPanelFactory.CreateScenariosPanel(
                canvasTransform, scenarios,
                onScenarioClick: (idx) => LoadScenario(idx),
                onBack: () => GoBackToMainMenu()
            );
        }

        private void LoadScenario(int scenarioIndex)
        {
            var scenariosPanel = GameObject.Find("ScenariosPanel");
            if (scenariosPanel != null) Destroy(scenariosPanel);

            var existingInfoPanel = GameObject.Find("ScenarioInfoPanel");
            if (existingInfoPanel != null) Destroy(existingInfoPanel);

            var gameManagerObj = GameObject.Find("GameManager");
            if (gameManagerObj == null)
                gameManagerObj = new GameObject("GameManager");

            if (gameManagerObj.GetComponent<PredefinedScenarios>() == null)
                gameManagerObj.AddComponent<PredefinedScenarios>();

            var sceneSetup = FindObjectOfType<SceneSetup>();
            if (sceneSetup != null)
            {
                sceneSetup.SetupManagers();
                sceneSetup.SubscribeToTopologyEvents();
            }

            if (canvas == null) canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                if (sceneSetup != null) sceneSetup.CreateVisualizer(canvas.transform);
                CreateSimulationHUDPanel(canvas.transform);
            }

            if (gameManagerObj.GetComponent<SimulationControls>() == null)
                gameManagerObj.AddComponent<SimulationControls>();

            var scenario = PredefinedScenarios.Instance.GetScenario(scenarioIndex);
            if (scenario == null)
            {
                UnityEngine.Debug.LogError($"[Scenarios] Escenario {scenarioIndex} no encontrado");
                return;
            }

            if (ScoringSystem.Instance != null)
                ScoringSystem.Instance.StartSession(scenario.name);
            else
            {
                var scoringObj = new GameObject("ScoringSystem");
                scoringObj.AddComponent<ScoringSystem>();
                ScoringSystem.Instance.StartSession(scenario.name);
            }

            UnityEngine.Debug.Log($"[Scenarios] Cargando escenario: {scenario.name}");
            ShowScenarioInfo(scenario);
        }

        private void ShowScenarioInfo(PredefinedScenarios.NetworkScenario scenario)
        {
            if (canvas == null) canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;

            UIPanelFactory.CreateScenarioInfoPanel(
                canvas.transform, scenario,
                onStart: () => BuildScenarioTopology(scenario),
                onCancel: null
            );
        }

        private void BuildScenarioTopology(PredefinedScenarios.NetworkScenario scenario)
        {
            var topology = FindObjectOfType<TopologyManager>();
            if (topology == null)
                topology = GameObject.Find("GameManager").AddComponent<TopologyManager>();

            topology.ClearTopology();

            float viewW = 1000f;
            float viewH = 500f;
            float offsetX = 100f;
            float offsetY = -50f;

            var createdNodes = new System.Collections.Generic.List<NetworkNode>();

            foreach (var dev in scenario.devices)
            {
                float x = offsetX + (dev.x - 0.5f) * viewW;
                float y = offsetY + ((1f - dev.y) - 0.5f) * viewH;
                Vector2 pos = new Vector2(x, y);

                int discId = createdNodes.Count + 1;
                Network.DeviceType devType = Network.DeviceType.PC;

                if (dev.type == "Router") devType = Network.DeviceType.Router;
                else if (dev.type == "Switch") devType = Network.DeviceType.Switch;

                topology.AddNode(discId, devType, pos);
                var node = topology.GetNode(discId);
                if (node != null) createdNodes.Add(node);
            }

            foreach (var link in scenario.links)
            {
                if (link.fromIndex < createdNodes.Count && link.toIndex < createdNodes.Count)
                    topology.AddLink(createdNodes[link.fromIndex].DiscId, createdNodes[link.toIndex].DiscId);
            }

            foreach (var ipConfig in scenario.ipConfigurations)
            {
                if (ipConfig.deviceIndex < createdNodes.Count)
                {
                    var node = createdNodes[ipConfig.deviceIndex];
                    node.IpAddress = ipConfig.ip;
                    node.SubnetMask = ipConfig.mask;
                }
            }

            foreach (var fault in scenario.faults)
            {
                if (fault.deviceIndex < createdNodes.Count)
                    topology.SetNodeFault(createdNodes[fault.deviceIndex].DiscId, fault.faultType);
            }

            var routers = createdNodes.Where(n => n.Type == Network.DeviceType.Router).ToList();
            if (scenario.name == "Dos Routers" && routers.Count >= 2)
            {
                routers[0].RoutingTable.AddStaticRoute("192.168.2.0", "255.255.255.0", "10.0.0.2", "G0/0");
                routers[1].RoutingTable.AddStaticRoute("192.168.1.0", "255.255.255.0", "10.0.0.1", "G0/1");
            }
            else if (scenario.name == "Topologia en Anillo" && routers.Count >= 3)
            {
                routers[0].RoutingTable.AddStaticRoute("172.16.1.0", "255.255.255.0", "172.16.1.1", "G0/0");
                routers[0].RoutingTable.AddStaticRoute("172.16.2.0", "255.255.255.0", "172.16.2.1", "G0/0");
                routers[1].RoutingTable.AddStaticRoute("172.16.0.0", "255.255.255.0", "172.16.0.1", "G0/1");
                routers[1].RoutingTable.AddStaticRoute("172.16.2.0", "255.255.255.0", "172.16.2.1", "G0/1");
                routers[2].RoutingTable.AddStaticRoute("172.16.0.0", "255.255.255.0", "172.16.0.1", "G0/2");
                routers[2].RoutingTable.AddStaticRoute("172.16.1.0", "255.255.255.0", "172.16.1.1", "G0/2");
            }
            else if (scenario.name == "Red en Arbol" && routers.Count >= 3)
            {
                routers[0].RoutingTable.AddStaticRoute("192.168.1.0", "255.255.255.0", "10.0.1.1", "G0/0");
                routers[0].RoutingTable.AddStaticRoute("192.168.2.0", "255.255.255.0", "10.0.2.1", "G0/0");
                routers[1].RoutingTable.Clear();
                routers[1].RoutingTable.AddStaticRoute("0.0.0.0", "0.0.0.0", "10.0.0.1", "G0/0");
                routers[2].RoutingTable.Clear();
                routers[2].RoutingTable.AddStaticRoute("0.0.0.0", "0.0.0.0", "10.0.0.1", "G0/0");
            }

            var devicePanelCtrl = FindObjectOfType<DevicePanelController>();
            if (devicePanelCtrl != null) devicePanelCtrl.RefreshDevicesPanel();
        }

        private void GoBackToMainMenu()
        {
            var cleanup = FindObjectOfType<SceneCleanupService>();
            if (cleanup != null)
            {
                var ct = canvas != null ? canvas.transform : FindObjectOfType<Canvas>()?.transform;
                cleanup.GoBackToMainMenu(ct, () => {
                    var ss = FindObjectOfType<SceneSetup>();
                    if (ss != null && ct != null) ss.CreateMainMenuPublic(ct);
                });
            }
            else
            {
                var ss = FindObjectOfType<SceneSetup>();
                if (ss != null)
                {
                    var ct = canvas != null ? canvas.transform : FindObjectOfType<Canvas>()?.transform;
                    if (ct != null) ss.CreateMainMenuPublic(ct);
                }
            }
        }

        // ==================== DYNAMIC ROUTING ====================

        public void StartDynamicProtocol(GameObject panel)
        {
            EnsureTopology();
            if (topology == null) return;

            var allNodes = topology.GetAllNodes();
            var routers = new System.Collections.Generic.List<NetworkNode>();
            foreach (var n in allNodes)
            {
                if (n.Type == Network.DeviceType.Router) routers.Add(n);
            }
            if (routers.Count < 2)
            {
                var statusText = panel.transform.Find("RightPanel")?.GetComponent<Text>();
                if (statusText != null)
                    statusText.text = "ERROR: Se necesitan\nal menos 2 routers";
                return;
            }

            if (selectedProtocol == null)
            {
                var statusText = panel.transform.Find("RightPanel")?.GetComponent<Text>();
                if (statusText != null)
                    statusText.text = "ERROR: Selecciona\nun protocolo primero";
                return;
            }

            var gameManager = GameObject.Find("GameManager");
            if (gameManager == null) gameManager = new GameObject("GameManager");

            if (dynProtocol != null)
            {
                dynProtocol.StopProtocol();
                Destroy(dynProtocol);
            }

            dynProtocol = gameManager.AddComponent<DynamicRoutingProtocol>();
            dynProtocol.protocol = selectedProtocol == "RIP" ?
                DynamicRoutingProtocol.ProtocolType.RIP :
                DynamicRoutingProtocol.ProtocolType.OSPF;

            dynProtocol.OnProtocolLog += (msg) => {
                UnityEngine.Debug.Log($"[{selectedProtocol}] {msg}");
                var statusText = panel.transform.Find("RightPanel")?.GetComponent<Text>();
                if (statusText != null)
                    statusText.text = $"[{selectedProtocol}] {msg}\n\nStatus: Ejecutando...";
            };

            dynProtocol.OnConvergence += () => {
                var statusText = panel.transform.Find("RightPanel")?.GetComponent<Text>();
                if (statusText != null)
                {
                    statusText.text = $"[{selectedProtocol}] CONVERGENCIA\n\n" +
                        $"Todas las rutas han\nsido intercambiadas.\n\n" +
                        $"Verifica las tablas de\nenrutamiento.";
                    statusText.color = new Color(0.2f, 0.8f, 0.2f);
                }
            };

            dynProtocol.StartProtocol();

            var statusTextStart = panel.transform.Find("RightPanel")?.GetComponent<Text>();
            if (statusTextStart != null)
            {
                statusTextStart.text = $"[{selectedProtocol}] Iniciando...\n\n" +
                    $"Routers: {routers.Count}\nAdvertisements en progreso...";
            }
        }

        public void StopDynamicProtocol(GameObject panel)
        {
            if (dynProtocol != null)
            {
                dynProtocol.StopProtocol();
                var statusText = panel.transform.Find("RightPanel")?.GetComponent<Text>();
                if (statusText != null)
                {
                    statusText.text = $"[{selectedProtocol}] Detenido\n\n" +
                        "Protocolo pausado.\nPresiona START para\ncontinuar.";
                    statusText.color = UIColors.textSecondary;
                }
            }
        }

        public void ClearDynamicRoutes(GameObject panel)
        {
            if (dynProtocol != null) dynProtocol.ClearAllRoutes();
            EnsureTopology();
            if (topology != null)
            {
                var routers = topology.GetAllNodes().Where(n => n.Type == Network.DeviceType.Router).ToList();
                foreach (var router in routers) router.RoutingTable.Clear();
            }
            var statusText = panel.transform.Find("RightPanel")?.GetComponent<Text>();
            if (statusText != null)
            {
                statusText.text = "RUTAS LIMPIADAS\n\nTodas las tablas de\nenrutamiento han sido\nborradas.";
                statusText.color = Color.red;
            }
        }

        public void ShowAllRouterRoutes(GameObject panel)
        {
            EnsureTopology();
            if (topology == null) return;

            var routers = topology.GetAllNodes().Where(n => n.Type == Network.DeviceType.Router).ToList();
            string routeInfo = "TABLAS DE RUTAS:\n\n";
            foreach (var router in routers)
            {
                var entries = router.RoutingTable.GetAllEntries();
                routeInfo += $"{router.Name}:\n";
                if (entries.Count == 0)
                    routeInfo += "  Sin rutas aprendidas\n\n";
                else
                {
                    foreach (var entry in entries)
                        routeInfo += $"  {entry.DestinationNetwork}/{entry.GetPrefixLength()}\n    via {entry.NextHop} ({entry.Protocol})\n";
                    routeInfo += "\n";
                }
            }

            var statusText = panel.transform.Find("RightPanel")?.GetComponent<Text>();
            if (statusText != null)
            {
                statusText.text = routeInfo.Length > 400 ? routeInfo.Substring(0, 400) + "..." : routeInfo;
                statusText.fontSize = 11;
            }
        }
    }
}
