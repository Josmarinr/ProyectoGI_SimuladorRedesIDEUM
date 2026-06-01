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
    /// <summary>
    /// Cargador de actividades del simulador. Administra la transicion entre el menu principal
    /// y las distintas actividades (topologia, fallos, enrutamiento estatico/dinamico, etc.).
    /// Crea los paneles UI necesarios y los componentes asociados a cada actividad.
    /// </summary>
    public class ActivityLoader : MonoBehaviour
    {
        internal string selectedProtocol = null;
        private DynamicRoutingProtocol dynProtocol = null;
        private TopologyManager topology;
        private Canvas canvas;
        private Font font;

        private void Start()
        {
            topology = UnityEngine.Object.FindAnyObjectByType<TopologyManager>();
            canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        }

        /// <summary>
        /// Almacena el protocolo de enrutamiento dinamico seleccionado (RIP, OSPF o EIGRP).
        /// </summary>
        /// <param name="protocol">Nombre del protocolo ("RIP", "OSPF" o "EIGRP").</param>
        public void SetSelectedProtocol(string protocol)
        {
            selectedProtocol = protocol;
        }

        private void EnsureTopology()
        {
            if (topology == null) topology = UnityEngine.Object.FindAnyObjectByType<TopologyManager>();
        }

        /// <summary>
        /// Garantiza que los gestores necesarios para el panel de conectividad existan
        /// (TopologyManager, NodeVisualizer, PingVisualizer), creandolos si es necesario.
        /// </summary>
        private void EnsureManagersForConnectivity()
        {
            var gameManagerObj = GameObject.Find("GameManager");
            if (gameManagerObj == null)
            {
                gameManagerObj = new GameObject("GameManager");
                gameManagerObj.AddComponent<TopologyManager>();
            }
            else if (gameManagerObj.GetComponent<TopologyManager>() == null)
            {
                gameManagerObj.AddComponent<TopologyManager>();
            }

            if (UnityEngine.Object.FindAnyObjectByType<NodeVisualizer>() == null)
            {
                var visObj = new GameObject("NodeVisualizer");
                visObj.transform.SetParent(UnityEngine.Object.FindAnyObjectByType<Canvas>()?.transform, false);
                var vis = visObj.AddComponent<NodeVisualizer>();
                vis.nodeContainer = new GameObject("NodeContainer").transform;
                vis.nodeContainer.SetParent(visObj.transform, false);
                vis.linkContainer = new GameObject("LinkContainer").transform;
                vis.linkContainer.SetParent(visObj.transform, false);
            }

            if (UnityEngine.Object.FindAnyObjectByType<PingVisualizer>() == null)
                new GameObject("PingVisualizer").AddComponent<PingVisualizer>();

            topology = UnityEngine.Object.FindAnyObjectByType<TopologyManager>();
            canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        }

        /// <summary>
        /// Inicia una actividad por su indice. Destruye el panel de actividades, crea el HUD
        /// de simulacion y agrega los componentes especificos segun la actividad elegida.
        /// </summary>
        /// <param name="activityIndex">Indice de la actividad (0: BuildTopology, 1: FindFault,
        /// 2: RoutingTables, 3: BestRoute, 4: StaticRouting, 5: DynamicRouting, 6: Escenarios).</param>
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

            if (canvas == null) canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            CreateSimulationHUDPanel(canvas.transform);

            switch (activityIndex)
            {
                case 0:
                    ActivityPanelFactory.CreateBuildTopologyInfoPanel(canvas.transform);
                    UnityEngine.Debug.Log("[ActivityLoader] Iniciando: Construye la Topolog\u00eda");
                    break;
                case 1:
                    // Limpiar topologia y cargar escenario pre-hecho
                    if (topology != null) topology.ClearTopology();
                    var findFault = gameManagerObj.GetComponent<FindFaultActivity>();
                    if (findFault == null)
                    {
                        gameManagerObj.AddComponent<FindFaultActivity>();
                    }
                    else
                    {
                        // Re-ingreso: forzar reinicio — FindFaultActivity.ConnectUI() cargará escenario 0
                        findFault.Restart();
                    }
                    ActivityPanelFactory.CreateFindFaultPanel(canvas.transform,
                        onDiscClick: () => {
                            var act = gameManagerObj.GetComponent<FindFaultActivity>();
                            if (act != null) act.OnDiscButtonClicked();
                        },
                        onNext: () => {
                            var act = gameManagerObj.GetComponent<FindFaultActivity>();
                            if (act != null) act.OnNextClicked();
                        },
                        onPrev: () => {
                            var act = gameManagerObj.GetComponent<FindFaultActivity>();
                            if (act != null) act.OnPrevClicked();
                        },
                        onBack: () => GoBackToMainMenu()
                    );
                    UnityEngine.Debug.Log("[ActivityLoader] Iniciando: Encuentra el Fallo");
                    break;
                case 2:
                    if (gameManagerObj.GetComponent<RoutingTablesActivity>() == null)
                        gameManagerObj.AddComponent<RoutingTablesActivity>();
                    ActivityPanelFactory.CreateRoutingTablesPanel(canvas.transform, () => GoBackToMainMenu());
                    UnityEngine.Debug.Log("[ActivityLoader] Iniciando: Tabla de Enrutamiento Tangible");
                    break;
                case 3:
                    if (gameManagerObj.GetComponent<BestRouteActivity>() == null)
                        gameManagerObj.AddComponent<BestRouteActivity>();
                    ActivityPanelFactory.CreateBestRoutePanel(canvas.transform, () => GoBackToMainMenu());
                    UnityEngine.Debug.Log("[ActivityLoader] Iniciando: Simulaci\u00f3n de Mejor Ruta");
                    break;
                case 4:
                    if (gameManagerObj.GetComponent<StaticRoutingActivity>() == null)
                        gameManagerObj.AddComponent<StaticRoutingActivity>();
                    ActivityPanelFactory.CreateStaticRoutingPanel(canvas.transform, () => GoBackToMainMenu());
                    UnityEngine.Debug.Log("[ActivityLoader] Iniciando: Enrutamiento Est\u00e1tico Tangible");
                    break;
                case 5:
                    if (gameManagerObj.GetComponent<DynamicRoutingActivity>() == null)
                        gameManagerObj.AddComponent<DynamicRoutingActivity>();
                    ActivityPanelFactory.CreateDynamicRoutingPanel(
                        canvas.transform,
                        onSelectRIP: () => SetSelectedProtocol("RIP"),
                        onSelectOSPF: () => SetSelectedProtocol("OSPF"),
                        onSelectEIGRP: () => SetSelectedProtocol("EIGRP"),
                        onStart: (panel) => {
                            var act = UnityEngine.Object.FindAnyObjectByType<DynamicRoutingActivity>();
                            if (act != null) act.StartProtocol(panel);
                        },
                        onStop: (panel) => {
                            var act = UnityEngine.Object.FindAnyObjectByType<DynamicRoutingActivity>();
                            if (act != null) act.StopProtocol(panel);
                        },
                        onClearRoutes: (panel) => {
                            var act = UnityEngine.Object.FindAnyObjectByType<DynamicRoutingActivity>();
                            if (act != null) act.ClearAllRoutes(panel);
                        },
                        onViewRoutes: (panel) => {
                            var act = UnityEngine.Object.FindAnyObjectByType<DynamicRoutingActivity>();
                            if (act != null) act.ShowRoutes(panel);
                        },
                        onBack: () => GoBackToMainMenu(),
                        // Discos virtuales 15-18: configuracion desde UI
                        onSetNeighbor: (neighbor) => {
                            var act = UnityEngine.Object.FindAnyObjectByType<DynamicRoutingActivity>();
                            if (act != null) act.SetNeighborRouter(neighbor);
                        },
                        onSetNetwork: (network) => {
                            var act = UnityEngine.Object.FindAnyObjectByType<DynamicRoutingActivity>();
                            if (act != null) act.SetNetworkToAdvertise(network);
                        },
                        onSetCost: (cost) => {
                            var act = UnityEngine.Object.FindAnyObjectByType<DynamicRoutingActivity>();
                            if (act != null) act.SetLinkCost(cost);
                        },
                        onSetBW: (bw) => {
                            var act = UnityEngine.Object.FindAnyObjectByType<DynamicRoutingActivity>();
                            if (act != null) act.SetBandwidth(bw);
                        }
                    );
                    UnityEngine.Debug.Log("[ActivityLoader] Iniciando: Protocolo de Enrutamiento Din\u00e1mico Tangible");
                    break;
                case 6:
                    CreateScenariosPanel(canvas.transform);
                    UnityEngine.Debug.Log("[ActivityLoader] Iniciando: Escenarios Preconfigurados");
                    break;
            }

            if (gameManagerObj.GetComponent<BuildTopologyActivity>() == null)
                gameManagerObj.AddComponent<BuildTopologyActivity>();
        }

        /// <summary>
        /// Inicia una simulacion libre. Destruye el menu principal, crea el HUD de simulacion
        /// y los componentes base (BuildTopologyActivity, SimulationControls, ScoringSystem).
        /// </summary>
        /// <param name="canvasTransform">Transform del canvas donde se crea el HUD.</param>
        public void StartSimulation(Transform canvasTransform)
        {
            var menuMgr = UnityEngine.Object.FindAnyObjectByType<MainMenuManager>();
            if (menuMgr != null) Destroy(menuMgr.gameObject);

            var menuNavigator = UnityEngine.Object.FindAnyObjectByType<MenuNavigator>();
            if (menuNavigator != null) Destroy(menuNavigator.gameObject);

            var mainMenu = GameObject.Find("MainMenuPanel");
            if (mainMenu != null) Destroy(mainMenu);

            var gameManagerObj = GameObject.Find("GameManager");
            if (gameManagerObj == null)
                gameManagerObj = new GameObject("GameManager");

            if (canvas == null) canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();

            CreateSimulationHUDPanel(canvasTransform);

            var scoring = UnityEngine.Object.FindAnyObjectByType<ScoringSystem>();
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

        /// <summary>
        /// Muestra el panel de conectividad independiente. Crea los gestores si no existen
        /// y configura el ConnectivityTestPanel con referencias a los textos UI.
        /// </summary>
        /// <param name="canvasTransform">Transform del canvas donde se crea el panel.</param>
        public void ShowConnectivityPanel(Transform canvasTransform)
        {
            EnsureManagersForConnectivity();

            var menuMgr = UnityEngine.Object.FindAnyObjectByType<MainMenuManager>();
            if (menuMgr != null) Destroy(menuMgr.gameObject);

            var mainMenu = GameObject.Find("MainMenuPanel");
            if (mainMenu != null) Destroy(mainMenu);

            if (canvas == null) canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();

            var refs = UIPanelFactory.CreateConnectivityPanel(canvasTransform, () => GoBackToMainMenu());

            var connectTestPanel = refs.panelObj.AddComponent<ConnectivityTestPanel>();
            connectTestPanel.Initialize(refs.sourceText, refs.destText, refs.resultText, refs.resultIcon, refs.pingButton, refs.statusText);

            var navigator = UnityEngine.Object.FindAnyObjectByType<MenuNavigator>();
            if (navigator != null)
                navigator.SetupPanel(refs.panelObj, () => GoBackToMainMenu());

            UnityEngine.Debug.Log("[ActivityLoader] Panel de conectividad creado");
        }

        /// <summary>
        /// Crea el panel HUD de simulacion con informacion de topologia, botones de accion
        /// (LIMPIAR, PING, VOLVER, CONECTAR, DESCONECTAR) y botones de red avanzada (VLAN, ACL, NAT).
        /// </summary>
        /// <param name="ct">Transform del canvas raiz.</param>
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
                var cleanup = UnityEngine.Object.FindAnyObjectByType<SceneCleanupService>();
                if (cleanup != null)
                    cleanup.ClearSimulation(UnityEngine.Object.FindAnyObjectByType<TopologyManager>(),
                        UnityEngine.Object.FindAnyObjectByType<NodeVisualizer>(),
                        UnityEngine.Object.FindAnyObjectByType<DebugDiscSimulator>());
            });

            Button pingBtn = UIComp.CreateMenuButton(panelObj.transform, "PingBtn", "PING",
                new Vector2(0, btnY), new Vector2(105, 44), font, 16);
            var pingCtrl = UnityEngine.Object.FindAnyObjectByType<PingModeController>();
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
            var linkCtrl = UnityEngine.Object.FindAnyObjectByType<LinkModeController>();
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
            var devicePanelCtrl = UnityEngine.Object.FindAnyObjectByType<DevicePanelController>();
            if (devicePanelCtrl != null) devicePanelCtrl.RefreshDevicesPanel();

            UIPanelFactory.CreateScorePanel(ct);

            UnityEngine.Debug.Log("[ActivityLoader] Panel de topologia creado");
        }

        /// <summary>
        /// Crea el panel de configuracion avanzada de red (VLAN, ACL o NAT) delegando en
        /// ConfigPanelFactory segun el tipo solicitado.
        /// </summary>
        /// <param name="type">Tipo de panel: "VLAN", "ACL" o "NAT".</param>
        private void CreateNetworkAdvancedPanel(string type)
        {
            var topo = UnityEngine.Object.FindAnyObjectByType<TopologyManager>();
            if (topo == null) return;

            if (canvas == null) canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            switch (type)
            {
                case "VLAN": ConfigPanelFactory.CreateVLANPanel(canvas.transform, topo); break;
                case "ACL": ConfigPanelFactory.CreateACLPanel(canvas.transform, topo); break;
                case "NAT": ConfigPanelFactory.CreateNATPanel(canvas.transform, topo); break;
            }
        }

        /// <summary>
        /// Crea el panel de seleccion de escenarios preconfigurados. Si no hay escenarios
        /// registrados, agrega uno de prueba rapida por defecto.
        /// </summary>
        /// <param name="canvasTransform">Transform del canvas donde se crea el panel.</param>
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

            ActivityPanelFactory.CreateScenariosPanel(
                canvasTransform, scenarios,
                onScenarioClick: (idx) => LoadScenario(idx),
                onBack: () => GoBackToMainMenu()
            );
        }

        /// <summary>
        /// Carga un escenario preconfigurado: configura los gestores, crea el HUD de simulacion,
        /// inicia la sesion de puntuacion y muestra la informacion del escenario.
        /// </summary>
        /// <param name="scenarioIndex">Indice del escenario en la lista de PredefinedScenarios.</param>
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

            var sceneSetup = UnityEngine.Object.FindAnyObjectByType<SceneSetup>();
            if (sceneSetup != null)
            {
                sceneSetup.SetupManagers();
                sceneSetup.SubscribeToTopologyEvents();
            }

            if (canvas == null) canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
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

        /// <summary>
        /// Muestra el panel de informacion de un escenario con opcion para iniciarlo.
        /// </summary>
        /// <param name="scenario">Escenario a mostrar.</param>
        private void ShowScenarioInfo(PredefinedScenarios.NetworkScenario scenario)
        {
            if (canvas == null) canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            ActivityPanelFactory.CreateScenarioInfoPanel(
                canvas.transform, scenario,
                onStart: () => BuildScenarioTopology(scenario),
                onCancel: null
            );
        }

        /// <summary>
        /// Construye la topologia de red a partir de un escenario preconfigurado.
        /// Crea dispositivos, enlaces, configuraciones IP y fallos segun la definicion del escenario.
        /// Tambien agrega rutas estaticas predefinidas para escenarios conocidos (Dos Routers, Anillo, Arbol).
        /// </summary>
        /// <param name="scenario">Escenario con la configuracion de dispositivos, enlaces y rutas.</param>
        private void BuildScenarioTopology(PredefinedScenarios.NetworkScenario scenario)
        {
            var topology = UnityEngine.Object.FindAnyObjectByType<TopologyManager>();
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

            var devicePanelCtrl = UnityEngine.Object.FindAnyObjectByType<DevicePanelController>();
            if (devicePanelCtrl != null) devicePanelCtrl.RefreshDevicesPanel();
        }

        /// <summary>
        /// Vuelve al menu principal. Utiliza SceneCleanupService si existe para limpiar la escena,
        /// o crea el menu directamente via SceneSetup.CreateMainMenuPublic.
        /// </summary>
        private void GoBackToMainMenu()
        {
            var cleanup = UnityEngine.Object.FindAnyObjectByType<SceneCleanupService>();
            if (cleanup != null)
            {
                var ct = canvas != null ? canvas.transform : UnityEngine.Object.FindAnyObjectByType<Canvas>()?.transform;
                cleanup.GoBackToMainMenu(ct, () => {
                    var ss = UnityEngine.Object.FindAnyObjectByType<SceneSetup>();
                    if (ss != null && ct != null) ss.CreateMainMenuPublic(ct);
                });
            }
            else
            {
                var ss = UnityEngine.Object.FindAnyObjectByType<SceneSetup>();
                if (ss != null)
                {
                    var ct = canvas != null ? canvas.transform : UnityEngine.Object.FindAnyObjectByType<Canvas>()?.transform;
                    if (ct != null) ss.CreateMainMenuPublic(ct);
                }
            }
        }

        // ==================== DYNAMIC ROUTING ====================

        /// <summary>
        /// Inicia el protocolo de enrutamiento dinamico seleccionado (RIP/OSPF/EIGRP).
        /// Valida que existan al menos 2 routers y que se haya seleccionado un protocolo.
        /// Configura los callbacks de log y convergencia, y arranca DynamicRoutingProtocol.
        /// </summary>
        /// <param name="panel">Panel de la actividad de enrutamiento dinamico (contiene RightPanel con Text de estado).</param>
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
            if (selectedProtocol == "RIP")
                dynProtocol.protocol = DynamicRoutingProtocol.ProtocolType.RIP;
            else if (selectedProtocol == "EIGRP")
                dynProtocol.protocol = DynamicRoutingProtocol.ProtocolType.EIGRP;
            else
                dynProtocol.protocol = DynamicRoutingProtocol.ProtocolType.OSPF;

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

        /// <summary>
        /// Detiene el protocolo de enrutamiento dinamico activo. Actualiza el texto de estado
        /// del panel indicando que el protocolo esta pausado.
        /// </summary>
        /// <param name="panel">Panel de la actividad (contiene RightPanel con Text de estado).</param>
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

        /// <summary>
        /// Limpia todas las rutas dinamicas del protocolo activo y vacia las tablas de
        /// enrutamiento de todos los routers en la topologia.
        /// </summary>
        /// <param name="panel">Panel de la actividad (contiene RightPanel con Text de estado).</param>
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

        /// <summary>
        /// Muestra las tablas de enrutamiento de todos los routers en un panel de texto.
        /// Trunca el contenido a 400 caracteres si es necesario.
        /// </summary>
        /// <param name="panel">Panel de la actividad (contiene RightPanel con Text de salida).</param>
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
