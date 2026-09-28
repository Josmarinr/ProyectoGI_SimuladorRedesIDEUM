using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SimRedes.Network;
using SimRedes.Simulation;
using SimRedes.UI;
using UnityEngine;
using UnityEngine.UI;
using DeviceType = SimRedes.Network.DeviceType;

namespace Tests.EditMode.Simulation
{
    /// <summary>
    /// Pruebas de paridad de la division de god class C5a (<see cref="ActivityLoader"/>):
    /// la fachada conserva su superficie publica y cada responsabilidad extraida
    /// (<see cref="ActivityHudFactory"/>, <see cref="ScenarioLoader"/>,
    /// <see cref="ActivityDispatcher"/>) reproduce el comportamiento del codigo inline original.
    /// </summary>
    public class TestActivityLoaderSplit
    {
        private GameObject loaderGo;
        private ActivityLoader loader;
        private GameObject canvasGo;

        private static readonly System.Type[] SingletonTypes =
        {
            typeof(ScoringSystem),
            typeof(PredefinedScenarios),
            typeof(TopologyManager),
            typeof(PingVisualizer),
            typeof(LinkModeController),
            typeof(SceneCleanupService)
        };

        [SetUp]
        public void SetUp()
        {
            // Aislar el test de residuos de fixtures anteriores
            DestroyByName("GameManager");
            DestroyByName("TopologyManager");
            DestroyByName("ScoringSystem");
            DestroyByName("PingVisualizer");
            DestroyByName("TopologyInfoPanel");
            DestroyByName("ScorePanel");
            foreach (var type in SingletonTypes) ClearSingleton(type);

            // Create Canvas (required by ActivityLoader.Start)
            canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();

            // Create ActivityLoader
            loaderGo = new GameObject("ActivityLoader");
            loader = loaderGo.AddComponent<ActivityLoader>();

            // Invoke Start via reflection (not auto-called in EditMode)
            var startMethod = typeof(ActivityLoader).GetMethod("Start",
                BindingFlags.Instance | BindingFlags.NonPublic);
            startMethod?.Invoke(loader, null);
        }

        [TearDown]
        public void TearDown()
        {
            DestroyByName("GameManager");
            DestroyByName("TopologyManager");
            DestroyByName("ScoringSystem");
            DestroyByName("PingVisualizer");
            DestroyByName("TopologyInfoPanel");
            DestroyByName("ScorePanel");
            foreach (var type in SingletonTypes) ClearSingleton(type);

            if (canvasGo != null) Object.DestroyImmediate(canvasGo);
            if (loaderGo != null) Object.DestroyImmediate(loaderGo);
        }

        // ================================================================
        // Helpers
        // ================================================================

        private static void DestroyByName(string name)
        {
            var go = GameObject.Find(name);
            if (go != null) Object.DestroyImmediate(go);
        }

        /// <summary>
        /// Anula el singleton estatico (backing field de la propiedad auto) para que
        /// ningun test herede instancia de otro.
        /// </summary>
        private static void ClearSingleton(System.Type type)
        {
            var field = type.GetField("<Instance>k__BackingField",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (field != null) field.SetValue(null, null);
        }

        private static object GetSelectedProtocol(ActivityLoader target)
        {
            var field = typeof(ActivityLoader).GetField("selectedProtocol",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, "El campo selectedProtocol de la fachada debe existir");
            return field.GetValue(target);
        }

        // ================================================================
        // FACHADA — superficie publica preservada (C5a)
        // ================================================================

        [Test]
        public void Facade_SetSelectedProtocol_StoresProtocol()
        {
            // Act
            loader.SetSelectedProtocol("OSPF");

            // Assert
            Assert.AreEqual("OSPF", GetSelectedProtocol(loader),
                "La fachada debe seguir almacenando el protocolo seleccionado");
        }

        [Test]
        public void Facade_StartSimulation_CreatesHudAndScoringSystem()
        {
            // Act — StartSimulation nunca estaba cubierto por tests
            loader.StartSimulation(canvasGo.transform);

            // Assert — HUD de simulacion creado (parity con CreateSimulationHUDPanel original)
            Assert.IsNotNull(GameObject.Find("TopologyInfoPanel"),
                "StartSimulation debe crear el HUD TopologyInfoPanel");
            Assert.IsNotNull(Object.FindAnyObjectByType<ScoringSystem>(),
                "StartSimulation debe crear el ScoringSystem si no existe");
        }

        // ================================================================
        // HUD FACTORY — responsabilidad extraida (C5a)
        // ================================================================

        [Test]
        public void HudFactory_CreateSimulationHUDPanel_BuildsControlsAndScore()
        {
            // Arrange
            var hud = new ActivityHudFactory(loader);

            // Act
            hud.CreateSimulationHUDPanel(canvasGo.transform);

            // Assert — panel HUD con sus controles (misma estructura que el codigo inline)
            var panel = GameObject.Find("TopologyInfoPanel");
            Assert.IsNotNull(panel, "El HUD debe crear TopologyInfoPanel");
            Assert.IsNotNull(panel.transform.Find("TopologyTypeText"),
                "El HUD debe crear el texto de tipo de topologia");
            Assert.IsNotNull(panel.transform.Find("ClearBtn"), "El HUD debe crear el boton LIMPIAR");
            Assert.IsNotNull(panel.transform.Find("PingBtn"), "El HUD debe crear el boton PING");
            Assert.IsNotNull(panel.transform.Find("BackBtn"), "El HUD debe crear el boton VOLVER");
            Assert.IsNotNull(panel.transform.Find("VLANBtn"), "El HUD debe crear el boton VLAN");
            Assert.IsNotNull(panel.transform.Find("ACLBtn"), "El HUD debe crear el boton ACL");
            Assert.IsNotNull(panel.transform.Find("NATBtn"), "El HUD debe crear el boton NAT");
            Assert.IsNotNull(GameObject.Find("ScorePanel"),
                "El HUD debe crear el ScorePanel via UIPanelFactory");
        }

        [Test]
        public void HudFactory_AddDeviceAtSpawn_DebouncesRapidDuplicateClicks()
        {
            // Arrange — sin TopologyManager: el primer click crea GameManager+TM+DPC
            var hud = new ActivityHudFactory(loader);

            // Act
            hud.AddDeviceAtSpawn(DeviceType.Router);
            hud.AddDeviceAtSpawn(DeviceType.Router); // duplicado dentro de 100ms

            // Assert — anti-doble-clic: solo un nodo
            var tm = Object.FindAnyObjectByType<TopologyManager>();
            Assert.IsNotNull(tm, "AddDeviceAtSpawn debe crear TopologyManager si no existe");
            Assert.AreEqual(1, tm.GetAllNodes().Count,
                "El segundo clic inmediato del mismo tipo debe ignorarse");

            // En EditMode AddComponent no ejecuta Awake: fijar el singleton como lo haria
            // Awake en runtime, para que la proxima llamada resuelva el manager existente.
            var instanceField = typeof(TopologyManager).GetField("<Instance>k__BackingField",
                BindingFlags.Static | BindingFlags.NonPublic);
            instanceField.SetValue(null, tm);

            // Act — tipo distinto si agrega
            hud.AddDeviceAtSpawn(DeviceType.PC);

            // Assert
            Assert.AreEqual(2, tm.GetAllNodes().Count,
                "Un tipo distinto no esta cubierto por el debounce y debe agregar el nodo");
            var gameManager = GameObject.Find("GameManager");
            Assert.IsNotNull(gameManager, "AddDeviceAtSpawn debe crear GameManager");
            Assert.IsNotNull(gameManager.GetComponent<DevicePanelController>(),
                "AddDeviceAtSpawn debe asegurar DevicePanelController en GameManager");
        }

        // ================================================================
        // DISPATCH — responsabilidad extraida (C5a)
        // ================================================================

        [Test]
        public void Dispatcher_SelectActivity_CreatesActivityPanelAndComponents()
        {
            // Act — invocar el despachador directamente (sin pasar por la fachada)
            var dispatcher = new ActivityDispatcher(loader);
            dispatcher.SelectActivity(5);

            // Assert — panel de la actividad 5 + componentes base, igual que antes
            Assert.IsNotNull(GameObject.Find("DynamicRoutingPanel"),
                "El despachador debe crear el panel de la actividad 5");
            Assert.IsNotNull(GameObject.Find("TopologyInfoPanel"),
                "El despachador debe crear el HUD antes de despachar");
            var gameManager = GameObject.Find("GameManager");
            Assert.IsNotNull(gameManager);
            Assert.IsNotNull(gameManager.GetComponent<DynamicRoutingActivity>(),
                "El despachador debe agregar DynamicRoutingActivity al GameManager");
            Assert.IsNotNull(gameManager.GetComponent<BuildTopologyActivity>(),
                "El despachador debe asegurar BuildTopologyActivity al final");
        }

        [Test]
        public void Dispatcher_RipButton_WritesProtocolIntoFacade()
        {
            // Arrange
            var dispatcher = new ActivityDispatcher(loader);
            dispatcher.SelectActivity(5);

            // Act — clic en RIP: el callback debe escribir en la fachada
            var panel = GameObject.Find("DynamicRoutingPanel");
            Assert.IsNotNull(panel, "Se requiere el panel de enrutamiento dinamico");
            var ripBtn = panel.transform.Find("RIPBtn");
            Assert.IsNotNull(ripBtn, "El panel debe exponer el boton RIP");
            ripBtn.GetComponent<Button>().onClick.Invoke();

            // Assert
            Assert.AreEqual("RIP", GetSelectedProtocol(loader),
                "El callback onSelectRIP del despachador debe llamar SetSelectedProtocol en la fachada");
        }

        // ================================================================
        // SCENARIO LOADER — responsabilidad extraida (C5a)
        // ================================================================

        [Test]
        public void ScenarioLoader_BuildScenarioTopology_CreatesNodesLinksIpsAndRoutes()
        {
            // Arrange — TopologyManager directo (BuildScenarioTopology lo resuelve por Find)
            var topoGo = new GameObject("TopologyManager");
            var topo = topoGo.AddComponent<TopologyManager>();
            var scenario = new PredefinedScenarios.NetworkScenario
            {
                name = "Dos Routers",
                description = "Par de routers con ruta cruzada",
                difficulty = PredefinedScenarios.ScenarioDifficulty.Basico,
                devices = new List<PredefinedScenarios.DeviceConfig>
                {
                    new PredefinedScenarios.DeviceConfig { type = "Router", x = 0.3f, y = 0.5f },
                    new PredefinedScenarios.DeviceConfig { type = "Router", x = 0.7f, y = 0.5f }
                },
                links = new List<PredefinedScenarios.LinkConfig>
                {
                    new PredefinedScenarios.LinkConfig { fromIndex = 0, toIndex = 1 }
                },
                ipConfigurations = new List<PredefinedScenarios.IPConfig>
                {
                    new PredefinedScenarios.IPConfig { deviceIndex = 0, ip = "10.0.0.1", mask = "255.255.255.0" },
                    new PredefinedScenarios.IPConfig { deviceIndex = 1, ip = "10.0.0.2", mask = "255.255.255.0" }
                },
                faults = new List<PredefinedScenarios.FaultConfig>()
            };

            // Act
            var loaderScenario = new ScenarioLoader(loader);
            loaderScenario.BuildScenarioTopology(scenario);

            // Assert — dispositivos, enlaces e IP
            var nodes = topo.GetAllNodes();
            Assert.AreEqual(2, nodes.Count, "El escenario debe crear sus dos dispositivos");
            Assert.IsTrue(nodes.All(n => n.Type == DeviceType.Router),
                "Ambos dispositivos deben ser routers");
            Assert.AreEqual(1, topo.GetAllLinks().Count, "El escenario debe crear el enlace");
            Assert.AreEqual("10.0.0.1", topo.GetNode(1).IpAddress, "La IP del dispositivo 0 debe aplicarse");
            Assert.AreEqual("10.0.0.2", topo.GetNode(2).IpAddress, "La IP del dispositivo 1 debe aplicarse");

            // Assert — rutas estaticas predefinidas del escenario "Dos Routers"
            var routes0 = topo.GetNode(1).RoutingTable.GetAllEntries();
            var routes1 = topo.GetNode(2).RoutingTable.GetAllEntries();
            Assert.IsTrue(routes0.Any(r => r.DestinationNetwork == "192.168.2.0" && r.NextHop == "10.0.0.2"),
                "Router 0 debe recibir su ruta estatica a 192.168.2.0 via 10.0.0.2");
            Assert.IsTrue(routes1.Any(r => r.DestinationNetwork == "192.168.1.0" && r.NextHop == "10.0.0.1"),
                "Router 1 debe recibir su ruta estatica a 192.168.1.0 via 10.0.0.1");

            Object.DestroyImmediate(topoGo);
        }

        [Test]
        public void ScenarioLoader_BuildScenarioTopology_AppliesFaultToNode()
        {
            // Arrange
            var topoGo = new GameObject("TopologyManager");
            var topo = topoGo.AddComponent<TopologyManager>();
            var scenario = new PredefinedScenarios.NetworkScenario
            {
                name = "Detectar Fallos",
                devices = new List<PredefinedScenarios.DeviceConfig>
                {
                    new PredefinedScenarios.DeviceConfig { type = "PC", x = 0.5f, y = 0.5f }
                },
                links = new List<PredefinedScenarios.LinkConfig>(),
                ipConfigurations = new List<PredefinedScenarios.IPConfig>(),
                faults = new List<PredefinedScenarios.FaultConfig>
                {
                    new PredefinedScenarios.FaultConfig
                    {
                        deviceIndex = 0,
                        faultType = "ip_incorrecta",
                        ip = "192.168.99.50",
                        mask = "255.255.255.0"
                    }
                }
            };

            // Act
            var loaderScenario = new ScenarioLoader(loader);
            loaderScenario.BuildScenarioTopology(scenario);

            // Assert — el fallo y su IP/mask incorrecta quedan aplicados al nodo
            var node = topo.GetNode(1);
            Assert.IsNotNull(node, "El escenario debe crear el dispositivo");
            Assert.IsTrue(node.HasFault(), "El nodo debe quedar marcado con fallo");
            Assert.AreEqual("192.168.99.50", node.IpAddress,
                "La IP incorrecta del fallo debe sobreescribir la del nodo");

            Object.DestroyImmediate(topoGo);
        }
    }
}
