using UnityEngine;
using System.Linq;
using SimRedes.Network;
using SimRedes.UI;
using UIComp = SimRedes.UI.UIComponents;

namespace SimRedes.Simulation
{
    /// <summary>
    /// Carga y construye escenarios preconfigurados. Administra el panel de seleccion
    /// de escenarios, la carga de un escenario (HUD, sesion de puntuacion, informacion)
    /// y la construccion de la topologia a partir de la definicion del escenario.
    /// Extraido de <see cref="ActivityLoader"/> (tarea C5) sin cambios de comportamiento.
    /// </summary>
    public class ScenarioLoader
    {
        private readonly ActivityLoader owner;

        /// <summary>
        /// Crea un cargador de escenarios asociado al <see cref="ActivityLoader"/> dueño,
        /// que aporta el canvas compartido y la ruta de vuelta al menu.
        /// </summary>
        /// <param name="owner">Fachada <see cref="ActivityLoader"/> que usa como contexto.</param>
        public ScenarioLoader(ActivityLoader owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Crea el panel de seleccion de escenarios preconfigurados. Si no hay escenarios
        /// registrados, agrega uno de prueba rapida por defecto.
        /// </summary>
        /// <param name="canvasTransform">Transform del canvas donde se crea el panel.</param>
        public void CreateScenariosPanel(Transform canvasTransform)
        {
            var existingPanel = GameObject.Find("ScenariosPanel");
            if (existingPanel != null)
            {
                // B4: liberar los Sprite del panel antes de destruirlo
                UIComp.SafeDestroyPanelSprites(existingPanel);
                UnityEngine.Object.Destroy(existingPanel);
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
                onBack: () => owner.GoBackToMainMenu()
            );
        }

        /// <summary>
        /// Carga un escenario preconfigurado: configura los gestores, crea el HUD de simulacion,
        /// inicia la sesion de puntuacion y muestra la informacion del escenario.
        /// </summary>
        /// <param name="scenarioIndex">Indice del escenario en la lista de PredefinedScenarios.</param>
        public void LoadScenario(int scenarioIndex)
        {
            var scenariosPanel = GameObject.Find("ScenariosPanel");
            if (scenariosPanel != null)
            {
                // B4: liberar los Sprite del panel antes de destruirlo
                UIComp.SafeDestroyPanelSprites(scenariosPanel);
                UnityEngine.Object.Destroy(scenariosPanel);
            }

            var existingInfoPanel = GameObject.Find("ScenarioInfoPanel");
            if (existingInfoPanel != null)
            {
                UIComp.SafeDestroyPanelSprites(existingInfoPanel);
                UnityEngine.Object.Destroy(existingInfoPanel);
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

            if (owner.canvas == null) owner.canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (owner.canvas != null)
            {
                if (sceneSetup != null) sceneSetup.CreateVisualizer(owner.canvas.transform);
                owner.Hud.CreateSimulationHUDPanel(owner.canvas.transform);
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
            if (owner.canvas == null) owner.canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (owner.canvas == null) return;

            ActivityPanelFactory.CreateScenarioInfoPanel(
                owner.canvas.transform, scenario,
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
        public void BuildScenarioTopology(PredefinedScenarios.NetworkScenario scenario)
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
    }
}
