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
        private TopologyManager topology;
        private Canvas canvas;
        private Font font;
        // Proteccion contra doble clic (EventSystem puede generar 2 eventos con Both input mode)
        private float lastAddTime = 0f;
        private SimRedes.Network.DeviceType lastAddType = SimRedes.Network.DeviceType.Unknown;
        private int spawnIndex = 0;
        private Vector2[] spawnPositions = new Vector2[]
        {
            new Vector2(100, 400), new Vector2(300, 350), new Vector2(200, 500),
            new Vector2(150, 600), new Vector2(250, 600), new Vector2(300, 250),
            new Vector2(100, 250), new Vector2(200, 700), new Vector2(300, 700)
        };

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
            if (activitiesPanel != null)
            {
                // B4: liberar los Sprite del panel antes de destruirlo
                UIComp.SafeDestroyPanelSprites(activitiesPanel);
                Destroy(activitiesPanel);
            }

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
                    // Siempre crear fresco: si ya existe (re-ingreso), destruir y recrear
                    var oldFindFault = gameManagerObj.GetComponent<FindFaultActivity>();
                    if (oldFindFault != null) Destroy(oldFindFault);
                    gameManagerObj.AddComponent<FindFaultActivity>();
                    // NOTA: ConnectUI() se llamara desde CreateFindFaultPanel() y cargara escenario 0
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
            if (existingStatus != null)
            {
                // B4: liberar los Sprite del panel antes de destruirlo
                UIComp.SafeDestroyPanelSprites(existingStatus);
                Destroy(existingStatus);
            }

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
            if (existingPanel != null)
            {
                existingPanel.SetActive(false);
                // B4: liberar los Sprite del panel antes de destruirlo
                UIComp.SafeDestroyPanelSprites(existingPanel);
                Destroy(existingPanel);
            }

            var existingInfo = GameObject.Find("TopologyExamplePanel");
            if (existingInfo != null)
            {
                existingInfo.SetActive(false);
                UIComp.SafeDestroyPanelSprites(existingInfo);
                Destroy(existingInfo);
            }

            font = UIComp.GetFont();

            GameObject panelObj = UIComp.CreateRoundedPanel(ct, new Vector2(560, 760), 20,
                UIColors.surfaceElevated, UIColors.borderAccent);
            panelObj.name = "TopologyInfoPanel";

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);
            panelRect.anchoredPosition = new Vector2(-20, -20);

            UIComp.CreateMenuTitle(panelObj.transform, "Topologia", 36, new Vector2(0, 280), font);

            Button infoBtn = UIComp.CreateSmallInfoButton(panelObj.transform, new Vector2(220, 280), font);
            infoBtn.onClick.AddListener(() => UIPanelFactory.ToggleTopologyExamplePanel(ct, font));

            float infoY = 210f;
            float lineHeight = 42f;

            var topologyTypeText = UIComp.CreateInfoText(panelObj.transform, "Topologia: Sin topologia",
                new Vector2(0, infoY), font, 26, UIColors.textAccent, true);
            topologyTypeText.gameObject.name = "TopologyTypeText";

            infoY -= lineHeight;
            var linkText = UIComp.CreateInfoText(panelObj.transform, "Enlaces: 0",
                new Vector2(0, infoY), font, 24, UIColors.textPrimary, true);
            linkText.gameObject.name = "LinkCountText";

            infoY -= lineHeight + 12;
            var devicesTitle = UIComp.CreateInfoText(panelObj.transform, "Dispositivos:",
                new Vector2(0, infoY), font, 23, UIColors.textSecondary, false);
            UIComp.ApplyTitleStyle(devicesTitle);

            infoY -= 36;
            var routerText = UIComp.CreateInfoText(panelObj.transform, "  Routers: 0",
                new Vector2(0, infoY), font, 23, UIColors.textSecondary, false);
            routerText.gameObject.name = "RouterCountText";

            infoY -= 34;
            var switchText = UIComp.CreateInfoText(panelObj.transform, "  Switches: 0",
                new Vector2(0, infoY), font, 23, UIColors.textSecondary, false);
            switchText.gameObject.name = "SwitchCountText";

            infoY -= 34;
            var pcText = UIComp.CreateInfoText(panelObj.transform, "  PCs: 0",
                new Vector2(0, infoY), font, 23, UIColors.textSecondary, false);
            pcText.gameObject.name = "PCCountText";

            infoY -= lineHeight + 10;
            var pingResultText = UIComp.CreateInfoText(panelObj.transform, "Ping: Seleccionar origen",
                new Vector2(0, infoY), font, 24, UIColors.textSecondary, false);
            pingResultText.gameObject.name = "PingResultText";

            float btnY = -125f;
            float btnSpacing = 150f;

            Button clearBtn = UIComp.CreateMenuButton(panelObj.transform, "ClearBtn", "LIMPIAR",
                new Vector2(-btnSpacing, btnY), new Vector2(140, 58), font, 21);
            clearBtn.onClick.AddListener(() => {
                var cleanup = UnityEngine.Object.FindAnyObjectByType<SceneCleanupService>();
                if (cleanup != null)
                    cleanup.ClearSimulation(UnityEngine.Object.FindAnyObjectByType<TopologyManager>(),
                        UnityEngine.Object.FindAnyObjectByType<NodeVisualizer>(),
                        UnityEngine.Object.FindAnyObjectByType<DebugDiscSimulator>());
            });

            Button pingBtn = UIComp.CreateMenuButton(panelObj.transform, "PingBtn", "PING",
                new Vector2(0, btnY), new Vector2(140, 58), font, 21);
            var pingCtrl = UnityEngine.Object.FindAnyObjectByType<PingModeController>();
            if (pingCtrl != null)
            {
                pingCtrl.StorePingReferences(pingBtn, pingResultText);
                pingBtn.onClick.AddListener(() => pingCtrl.TogglePingMode());
            }

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "VOLVER",
                new Vector2(btnSpacing, btnY), new Vector2(140, 58), font, 21);
            backBtn.onClick.AddListener(() => GoBackToMainMenu());

            float linkBtnY = -195f;
            float linkBtnSpacing = 155f;

            // Asegurar que LinkModeController existe (por si la limpieza anterior lo destruyo)
            var linkCtrl = LinkModeController.Instance;
            if (linkCtrl == null)
            {
                var gm = GameObject.Find("GameManager");
                if (gm != null)
                {
                    linkCtrl = gm.GetComponent<LinkModeController>();
                    if (linkCtrl == null)
                        linkCtrl = gm.AddComponent<LinkModeController>();
                }
            }

            Button connectBtn = UIComp.CreateMenuButton(panelObj.transform, "ConnectBtn", "CONECTAR",
                new Vector2(-linkBtnSpacing, linkBtnY), new Vector2(140, 58), font, 20);
            if (linkCtrl != null)
            {
                connectBtn.onClick.AddListener(() => linkCtrl.ToggleLinkMode("connect"));
            }

            Button disconnectBtn = UIComp.CreateMenuButton(panelObj.transform, "DisconnectBtn", "DESCONECTAR",
                new Vector2(linkBtnSpacing, linkBtnY), new Vector2(140, 58), font, 18);
            if (linkCtrl != null)
            {
                disconnectBtn.onClick.AddListener(() => linkCtrl.ToggleLinkMode("disconnect"));
                linkCtrl.StoreLinkButtons(connectBtn, disconnectBtn);
            }

            float devBtnY = -265f;
            float devSpacing = 120f;

            Button routerBtn = UIComp.CreateMenuButton(panelObj.transform, "RouterBtn", "ROUTER",
                new Vector2(-devSpacing, devBtnY), new Vector2(110, 48), font, 17);
            routerBtn.onClick.AddListener(() => AddDeviceAtSpawn(Network.DeviceType.Router));

            Button switchBtn = UIComp.CreateMenuButton(panelObj.transform, "SwitchBtn", "SWITCH",
                new Vector2(0, devBtnY), new Vector2(110, 48), font, 17);
            switchBtn.onClick.AddListener(() => AddDeviceAtSpawn(Network.DeviceType.Switch));

            Button pcBtn = UIComp.CreateMenuButton(panelObj.transform, "PCBtn", "PC",
                new Vector2(devSpacing, devBtnY), new Vector2(110, 48), font, 17);
            pcBtn.onClick.AddListener(() => AddDeviceAtSpawn(Network.DeviceType.PC));

            float advBtnY = -330f;
            float advSpacing = 130f;

            Button vlanBtn = UIComp.CreateMenuButton(panelObj.transform, "VLANBtn", "VLAN",
                new Vector2(-advSpacing, advBtnY), new Vector2(120, 48), font, 19);
            vlanBtn.onClick.AddListener(() => CreateNetworkAdvancedPanel("VLAN"));

            Button aclBtn = UIComp.CreateMenuButton(panelObj.transform, "ACLBtn", "ACL",
                new Vector2(0, advBtnY), new Vector2(120, 48), font, 19);
            aclBtn.onClick.AddListener(() => CreateNetworkAdvancedPanel("ACL"));

            Button natBtn = UIComp.CreateMenuButton(panelObj.transform, "NATBtn", "NAT",
                new Vector2(advSpacing, advBtnY), new Vector2(110, 42), font, 17);
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
            if (existingPanel != null)
            {
                // B4: liberar los Sprite del panel antes de destruirlo
                UIComp.SafeDestroyPanelSprites(existingPanel);
                Destroy(existingPanel);
            }

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
            if (scenariosPanel != null)
            {
                // B4: liberar los Sprite del panel antes de destruirlo
                UIComp.SafeDestroyPanelSprites(scenariosPanel);
                Destroy(scenariosPanel);
            }

            var existingInfoPanel = GameObject.Find("ScenarioInfoPanel");
            if (existingInfoPanel != null)
            {
                UIComp.SafeDestroyPanelSprites(existingInfoPanel);
                Destroy(existingInfoPanel);
            }

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
                {
                    var fNode = createdNodes[fault.deviceIndex];
                    topology.SetNodeFault(fNode.DiscId, fault.faultType);
                    // Aplicar IP/mask incorrecta del fallo al nodo (el usuario debe corregirla)
                    if (!string.IsNullOrEmpty(fault.ip))
                        fNode.IpAddress = fault.ip;
                    if (!string.IsNullOrEmpty(fault.mask))
                        fNode.SubnetMask = fault.mask;
                }
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
                // Router0 -> LANs via Router1(10.0.0.2) y Router2(10.0.0.3)
                routers[0].RoutingTable.AddStaticRoute("192.168.1.0", "255.255.255.0", "10.0.0.2", "G0/0");
                routers[0].RoutingTable.AddStaticRoute("192.168.2.0", "255.255.255.0", "10.0.0.3", "G0/0");
                // Router1 y Router2 -> default via Router0(10.0.0.1)
                routers[1].RoutingTable.Clear();
                routers[1].RoutingTable.AddStaticRoute("0.0.0.0", "0.0.0.0", "10.0.0.1", "G0/0");
                routers[2].RoutingTable.Clear();
                routers[2].RoutingTable.AddStaticRoute("0.0.0.0", "0.0.0.0", "10.0.0.1", "G0/0");
            }

            var devicePanelCtrl = UnityEngine.Object.FindAnyObjectByType<DevicePanelController>();
            if (devicePanelCtrl != null) devicePanelCtrl.RefreshDevicesPanel();
        }

        /// <summary>
        /// Agrega un dispositivo en la siguiente posicion predefinida.
        /// Alternativa a los atajos de teclado (1/2/3) para uso sin teclado fisico.
        /// </summary>
        private void AddDeviceAtSpawn(Network.DeviceType type)
        {
            // Verificar si es una llamada duplicada (EventSystem puede procesar el clic dos veces
            // con activeInputHandler=Both en el Editor). Skip si el ultimo dispositivo se creo
            // hace menos de 100ms con el mismo tipo.
            float now = Time.unscaledTime;
            if (now - lastAddTime < 0.1f && lastAddType == type)
            {
                UnityEngine.Debug.Log($"[ActivityLoader] Llamada duplicada ignorada: {type}");
                return;
            }
            lastAddTime = now;
            lastAddType = type;

            // Usar TopologyManager.Instance (singleton siempre fresco) en vez de campo cacheado
            var tm = TopologyManager.Instance;
            if (tm == null)
            {
                var gm = GameObject.Find("GameManager");
                if (gm == null) gm = new GameObject("GameManager");
                tm = gm.AddComponent<TopologyManager>();
                // Asegurar que DevicePanelController exista (se destruye con GameManager al limpiar)
                if (gm.GetComponent<DevicePanelController>() == null)
                    gm.AddComponent<DevicePanelController>();
            }
            else
            {
                // Si TopologyManager ya existe pero DevicePanelController no (ej: tras limpiar),
                // crearlo en el GameManager
                var gm = GameObject.Find("GameManager");
                if (gm != null && gm.GetComponent<DevicePanelController>() == null)
                    gm.AddComponent<DevicePanelController>();
            }
            Vector2 pos = spawnPositions[spawnIndex % spawnPositions.Length];
            spawnIndex++;
            int discId = spawnIndex + 200; // ID unico (sobre 100 usado por TangibleDiscManager)
            tm.AddNode(discId, type, pos);
            var devicePanel = UnityEngine.Object.FindAnyObjectByType<DevicePanelController>();
            if (devicePanel != null) devicePanel.RefreshDevicesPanel();
            UnityEngine.Debug.Log($"[ActivityLoader] Dispositivo agregado: {type} en {pos}, discId={discId}");
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
    }
}
