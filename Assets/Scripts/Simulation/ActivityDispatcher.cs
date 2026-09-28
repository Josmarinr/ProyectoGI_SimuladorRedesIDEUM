using UnityEngine;
using SimRedes.Network;
using SimRedes.UI;
using UIComp = SimRedes.UI.UIComponents;

namespace SimRedes.Simulation
{
    /// <summary>
    /// Despachador de actividades del simulador. Orquesta la seleccion de actividades
    /// (0-6), el arranque de la simulacion libre y el panel de conectividad: destruye
    /// paneles previos, crea el HUD, agrega los componentes de cada actividad y conecta
    /// sus callbacks. Extraido de <see cref="ActivityLoader"/> (tarea C5) sin cambios
    /// de comportamiento.
    /// </summary>
    public class ActivityDispatcher
    {
        private readonly ActivityLoader owner;

        /// <summary>
        /// Crea un despachador asociado al <see cref="ActivityLoader"/> dueño, que aporta
        /// el canvas y la topologia compartidos, la fabrica de HUD, la carga de escenarios
        /// y la ruta de vuelta al menu.
        /// </summary>
        /// <param name="owner">Fachada <see cref="ActivityLoader"/> que usa como contexto.</param>
        public ActivityDispatcher(ActivityLoader owner)
        {
            this.owner = owner;
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
                UnityEngine.Object.Destroy(activitiesPanel);
            }

            var gameManagerObj = GameObject.Find("GameManager");
            if (gameManagerObj == null)
                gameManagerObj = new GameObject("GameManager");

            if (gameManagerObj.GetComponent<BuildTopologyActivity>() == null)
                gameManagerObj.AddComponent<BuildTopologyActivity>();

            if (gameManagerObj.GetComponent<SimulationControls>() == null)
                gameManagerObj.AddComponent<SimulationControls>();

            if (owner.canvas == null) owner.canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (owner.canvas == null) return;

            owner.Hud.CreateSimulationHUDPanel(owner.canvas.transform);

            switch (activityIndex)
            {
                case 0:
                    ActivityPanelFactory.CreateBuildTopologyInfoPanel(owner.canvas.transform);
                    UnityEngine.Debug.Log("[ActivityLoader] Iniciando: Construye la Topolog\u00eda");
                    break;
                case 1:
                    // Limpiar topologia y cargar escenario pre-hecho
                    if (owner.topology != null) owner.topology.ClearTopology();
                    // Siempre crear fresco: si ya existe (re-ingreso), destruir y recrear
                    var oldFindFault = gameManagerObj.GetComponent<FindFaultActivity>();
                    if (oldFindFault != null) UnityEngine.Object.Destroy(oldFindFault);
                    gameManagerObj.AddComponent<FindFaultActivity>();
                    // NOTA: ConnectUI() se llamara desde CreateFindFaultPanel() y cargara escenario 0
                    ActivityPanelFactory.CreateFindFaultPanel(owner.canvas.transform,
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
                        onBack: () => owner.GoBackToMainMenu()
                    );
                    UnityEngine.Debug.Log("[ActivityLoader] Iniciando: Encuentra el Fallo");
                    break;
                case 2:
                    if (gameManagerObj.GetComponent<RoutingTablesActivity>() == null)
                        gameManagerObj.AddComponent<RoutingTablesActivity>();
                    ActivityPanelFactory.CreateRoutingTablesPanel(owner.canvas.transform, () => owner.GoBackToMainMenu());
                    UnityEngine.Debug.Log("[ActivityLoader] Iniciando: Tabla de Enrutamiento Tangible");
                    break;
                case 3:
                    if (gameManagerObj.GetComponent<BestRouteActivity>() == null)
                        gameManagerObj.AddComponent<BestRouteActivity>();
                    ActivityPanelFactory.CreateBestRoutePanel(owner.canvas.transform, () => owner.GoBackToMainMenu());
                    UnityEngine.Debug.Log("[ActivityLoader] Iniciando: Simulaci\u00f3n de Mejor Ruta");
                    break;
                case 4:
                    if (gameManagerObj.GetComponent<StaticRoutingActivity>() == null)
                        gameManagerObj.AddComponent<StaticRoutingActivity>();
                    ActivityPanelFactory.CreateStaticRoutingPanel(owner.canvas.transform, () => owner.GoBackToMainMenu());
                    UnityEngine.Debug.Log("[ActivityLoader] Iniciando: Enrutamiento Est\u00e1tico Tangible");
                    break;
                case 5:
                    if (gameManagerObj.GetComponent<DynamicRoutingActivity>() == null)
                        gameManagerObj.AddComponent<DynamicRoutingActivity>();
                    ActivityPanelFactory.CreateDynamicRoutingPanel(
                        owner.canvas.transform,
                        onSelectRIP: () => owner.SetSelectedProtocol("RIP"),
                        onSelectOSPF: () => owner.SetSelectedProtocol("OSPF"),
                        onSelectEIGRP: () => owner.SetSelectedProtocol("EIGRP"),
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
                        onBack: () => owner.GoBackToMainMenu(),
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
                    owner.Scenarios.CreateScenariosPanel(owner.canvas.transform);
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
            if (menuMgr != null) UnityEngine.Object.Destroy(menuMgr.gameObject);

            var menuNavigator = UnityEngine.Object.FindAnyObjectByType<MenuNavigator>();
            if (menuNavigator != null) UnityEngine.Object.Destroy(menuNavigator.gameObject);

            var mainMenu = GameObject.Find("MainMenuPanel");
            if (mainMenu != null) UnityEngine.Object.Destroy(mainMenu);

            var gameManagerObj = GameObject.Find("GameManager");
            if (gameManagerObj == null)
                gameManagerObj = new GameObject("GameManager");

            if (owner.canvas == null) owner.canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();

            owner.Hud.CreateSimulationHUDPanel(canvasTransform);

            var scoring = UnityEngine.Object.FindAnyObjectByType<ScoringSystem>();
            if (scoring == null)
            {
                var scoringObj = new GameObject("ScoringSystem");
                scoringObj.AddComponent<ScoringSystem>();
            }
            // C6: check de Unity (== null) en vez de `?.`. El null-conditional usa
            // igualdad de referencia y salta el operador == sobrecargado de Unity,
            // asi que con una instancia destruida (ScoringSystem no limpia Instance
            // en OnDestroy) StartSession se invocaria sobre un objeto muerto.
            var scoringInstance = ScoringSystem.Instance;
            if (scoringInstance != null)
                scoringInstance.StartSession("Simulacion Libre");

            if (gameManagerObj.GetComponent<BuildTopologyActivity>() == null)
                gameManagerObj.AddComponent<BuildTopologyActivity>();

            if (gameManagerObj.GetComponent<SimulationControls>() == null)
                gameManagerObj.AddComponent<SimulationControls>();

            var existingStatus = GameObject.Find("StatusPanel");
            if (existingStatus != null)
            {
                // B4: liberar los Sprite del panel antes de destruirlo
                UIComp.SafeDestroyPanelSprites(existingStatus);
                UnityEngine.Object.Destroy(existingStatus);
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
            if (menuMgr != null) UnityEngine.Object.Destroy(menuMgr.gameObject);

            var mainMenu = GameObject.Find("MainMenuPanel");
            if (mainMenu != null) UnityEngine.Object.Destroy(mainMenu);

            if (owner.canvas == null) owner.canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();

            var refs = UIPanelFactory.CreateConnectivityPanel(canvasTransform, () => owner.GoBackToMainMenu());

            var connectTestPanel = refs.panelObj.AddComponent<ConnectivityTestPanel>();
            connectTestPanel.Initialize(refs.sourceText, refs.destText, refs.resultText, refs.resultIcon, refs.pingButton, refs.statusText);

            var navigator = UnityEngine.Object.FindAnyObjectByType<MenuNavigator>();
            if (navigator != null)
                navigator.SetupPanel(refs.panelObj, () => owner.GoBackToMainMenu());

            UnityEngine.Debug.Log("[ActivityLoader] Panel de conectividad creado");
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

            owner.topology = UnityEngine.Object.FindAnyObjectByType<TopologyManager>();
            owner.canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        }
    }
}
