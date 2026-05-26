using NUnit.Framework;
using SimRedes.Network;
using SimRedes.Simulation;
using SimRedes.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;

namespace Tests.EditMode.Simulation
{
    /// <summary>
    /// Tests for ActivityLoader.
    /// Validates activity selection, panel creation, and scenario loading.
    /// NOTE: ActivityLoader.LoadScenario() is private — tests use reflection.
    /// </summary>
    public class TestActivityLoader
    {
        private GameObject loaderGo;
        private ActivityLoader loader;
        private GameObject canvasGo;

        [SetUp]
        public void SetUp()
        {
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
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (startMethod != null)
                startMethod.Invoke(loader, null);
        }

        [TearDown]
        public void TearDown()
        {
            // Destroy GameManager if created by tests
            var gameManager = GameObject.Find("GameManager");
            if (gameManager != null)
                Object.DestroyImmediate(gameManager);

            // Clean up singleton instances created by SetupScenarioDependencies
            CleanupSingleton<ScoringSystem>();
            CleanupSingleton<PredefinedScenarios>();

            if (canvasGo != null)
                Object.DestroyImmediate(canvasGo);

            if (loaderGo != null)
                Object.DestroyImmediate(loaderGo);
        }

        /// <summary>
        /// Destroys the singleton MonoBehaviour and nulls its static Instance field.
        /// </summary>
        private void CleanupSingleton<T>() where T : MonoBehaviour
        {
            var instance = Object.FindAnyObjectByType<T>();
            if (instance != null)
                Object.DestroyImmediate(instance.gameObject);

            // Null the static Instance backing field to prevent conflicts in EditMode
            var backingField = typeof(T).GetField("<Instance>k__BackingField",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (backingField != null)
                backingField.SetValue(null, null);
        }

        // ================================================================
        // Helpers
        // ================================================================

        /// <summary>
        /// Invokes a method by name via reflection on any MonoBehaviour.
        /// </summary>
        private void InvokeMethod(MonoBehaviour mb, string methodName)
        {
            var flags = System.Reflection.BindingFlags.Instance
                      | System.Reflection.BindingFlags.NonPublic
                      | System.Reflection.BindingFlags.Public;
            var method = mb.GetType().GetMethod(methodName, flags);
            if (method != null)
                method.Invoke(mb, null);
        }

        /// <summary>
        /// Invokes the private LoadScenario method via reflection.
        /// </summary>
        private void InvokeLoadScenario(int index)
        {
            var method = typeof(ActivityLoader).GetMethod("LoadScenario",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(method, "LoadScenario method not found");
            method.Invoke(loader, new object[] { index });
        }

        /// <summary>
        /// Helper to find a GameObject by name (returns null if not found).
        /// </summary>
        private GameObject FindPanel(string name)
        {
            return GameObject.Find(name);
        }

        // ================================================================
        // ACTIVITY SELECTION — Panel Creation
        // ================================================================

        [Test]
        public void SelectActivity_0_CreatesBuildTopologyInfoPanel()
        {
            // Act
            loader.SelectActivity(0);

            // Assert
            var panel = FindPanel("BuildTopologyInfoPanel");
            Assert.IsNotNull(panel, "Activity 0 should create BuildTopologyInfoPanel");
        }

        [Test]
        public void SelectActivity_1_CreatesFindFaultPanel()
        {
            // Act
            loader.SelectActivity(1);

            // Assert
            var panel = FindPanel("FindFaultPanel");
            Assert.IsNotNull(panel, "Activity 1 should create FindFaultPanel");
        }

        [Test]
        public void SelectActivity_2_CreatesRoutingTablesPanel()
        {
            // Act
            loader.SelectActivity(2);

            // Assert
            var panel = FindPanel("RoutingTablesPanel");
            Assert.IsNotNull(panel, "Activity 2 should create RoutingTablesPanel");
        }

        [Test]
        public void SelectActivity_3_CreatesBestRoutePanel()
        {
            // Act
            loader.SelectActivity(3);

            // Assert
            var panel = FindPanel("BestRoutePanel");
            Assert.IsNotNull(panel, "Activity 3 should create BestRoutePanel");
        }

        [Test]
        public void SelectActivity_4_CreatesStaticRoutingPanel()
        {
            // Act
            loader.SelectActivity(4);

            // Assert
            var panel = FindPanel("StaticRoutingPanel");
            Assert.IsNotNull(panel, "Activity 4 should create StaticRoutingPanel");
        }

        [Test]
        public void SelectActivity_5_CreatesDynamicRoutingPanel()
        {
            // Act
            loader.SelectActivity(5);

            // Assert
            var panel = FindPanel("DynamicRoutingPanel");
            Assert.IsNotNull(panel, "Activity 5 should create DynamicRoutingPanel");
        }

        [Test]
        public void SelectActivity_6_CreatesScenariosPanel()
        {
            // Arrange: PredefinedScenarios singleton must exist for panel creation
            var scenariosGo = new GameObject("TestScenarios");
            var scenarios = scenariosGo.AddComponent<PredefinedScenarios>();
            var awakeMethod = typeof(PredefinedScenarios).GetMethod("Awake",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (awakeMethod != null)
                awakeMethod.Invoke(scenarios, null);

            // Act
            loader.SelectActivity(6);

            // Assert
            var panel = FindPanel("ScenariosPanel");
            Assert.IsNotNull(panel, "Activity 6 should create ScenariosPanel");
        }

        [Test]
        public void SelectActivity_InvalidIndex_DoesNothing()
        {
            // Act — index 7 has no case, should still create HUD but no activity panel
            loader.SelectActivity(7);

            // There should be no specific panel for invalid index
            // The HUD panel is always created via CreateSimulationHUDPanel
            var infoPanel = FindPanel("TopologyInfoPanel");
            Assert.IsNotNull(infoPanel, "HUD panel should always be created");
            // No specific activity panel for index 7
            var bestRoutePanel = FindPanel("BestRoutePanel");
            Assert.IsNull(bestRoutePanel, "Should not create a panel for invalid index");
        }

        // ================================================================
        // COMPONENT CREATION
        // ================================================================

        [Test]
        public void SelectActivity_0_CreatesBuildTopologyActivity()
        {
            // Act
            loader.SelectActivity(0);

            // Assert
            var gameManager = GameObject.Find("GameManager");
            Assert.IsNotNull(gameManager, "GameManager should be created");
            var buildTopoAct = gameManager.GetComponent<BuildTopologyActivity>();
            Assert.IsNotNull(buildTopoAct, "BuildTopologyActivity should be added to GameManager");
        }

        [Test]
        public void SelectActivity_0_CreatesSimulationControls()
        {
            // Act
            loader.SelectActivity(0);

            // Assert
            var gameManager = GameObject.Find("GameManager");
            Assert.IsNotNull(gameManager);
            var simControls = gameManager.GetComponent<SimulationControls>();
            Assert.IsNotNull(simControls, "SimulationControls should be added to GameManager");
        }

        // ================================================================
        // CONNECTIVITY PANEL
        // ================================================================

        [Test]
        public void ShowConnectivityPanel_CreatesConnectivityPanel()
        {
            // Act
            loader.ShowConnectivityPanel(canvasGo.transform);

            // Assert
            var panel = FindPanel("ConnectivityPanel");
            Assert.IsNotNull(panel, "ShowConnectivityPanel should create ConnectivityPanel");
        }

        [Test]
        public void ShowConnectivityPanel_CreatesTopologyManager()
        {
            // Act
            loader.ShowConnectivityPanel(canvasGo.transform);

            // Assert
            var topo = Object.FindAnyObjectByType<TopologyManager>();
            Assert.IsNotNull(topo, "ShowConnectivityPanel should ensure TopologyManager exists");
        }

        [Test]
        public void ShowConnectivityPanel_CreatesNodeVisualizer()
        {
            // Act
            loader.ShowConnectivityPanel(canvasGo.transform);

            // Assert
            var vis = Object.FindAnyObjectByType<NodeVisualizer>();
            Assert.IsNotNull(vis, "ShowConnectivityPanel should ensure NodeVisualizer exists");
        }

        [Test]
        public void ShowConnectivityPanel_WithoutCanvas_DoesNotThrow()
        {
            // Arrange: destroy the canvas
            Object.DestroyImmediate(canvasGo);
            canvasGo = null;

            // Act & Assert — pass null transform
            Assert.DoesNotThrow(() => loader.ShowConnectivityPanel(null));
        }

        // ================================================================
        // SCENARIO LOADING
        // ================================================================

        [Test]
        public void LoadScenario_0_LoadsEstrellaSimple()
        {
            // Arrange: Set up PredefinedScenarios and ScoringSystem
            SetupScenarioDependencies();

            // Act
            InvokeLoadScenario(0);

            // Assert
            var infoPanel = FindPanel("ScenarioInfoPanel");
            Assert.IsNotNull(infoPanel, "Scenario 0 should create ScenarioInfoPanel");
        }

        [Test]
        public void LoadScenario_1_LoadsDosRouters()
        {
            SetupScenarioDependencies();
            InvokeLoadScenario(1);
            var infoPanel = FindPanel("ScenarioInfoPanel");
            Assert.IsNotNull(infoPanel, "Scenario 1 should create ScenarioInfoPanel");
        }

        [Test]
        public void LoadScenario_2_LoadsTopologiaAnillo()
        {
            SetupScenarioDependencies();
            InvokeLoadScenario(2);
            var infoPanel = FindPanel("ScenarioInfoPanel");
            Assert.IsNotNull(infoPanel, "Scenario 2 should create ScenarioInfoPanel");
        }

        [Test]
        public void LoadScenario_3_LoadsRedArbol()
        {
            SetupScenarioDependencies();
            InvokeLoadScenario(3);
            var infoPanel = FindPanel("ScenarioInfoPanel");
            Assert.IsNotNull(infoPanel, "Scenario 3 should create ScenarioInfoPanel");
        }

        [Test]
        public void LoadScenario_4_LoadsDetectarFallos()
        {
            SetupScenarioDependencies();
            InvokeLoadScenario(4);
            var infoPanel = FindPanel("ScenarioInfoPanel");
            Assert.IsNotNull(infoPanel, "Scenario 4 should create ScenarioInfoPanel");
        }

        [Test]
        public void LoadScenario_InvalidIndex_DoesNothing()
        {
            // Arrange
            SetupScenarioDependencies();

            // Act: index 5 is out of bounds (scenarios are 0-4)
            // Expect the error log that LoadScenario emits for invalid index
            LogAssert.Expect(LogType.Error, "[Scenarios] Escenario 5 no encontrado");
            InvokeLoadScenario(5);

            // Assert: ScenarioInfoPanel should NOT be created for invalid index
            var infoPanel = FindPanel("ScenarioInfoPanel");
            Assert.IsNull(infoPanel,
                "Invalid scenario index should not create ScenarioInfoPanel");
        }

        // ================================================================
        // Setup Helpers
        // ================================================================

        /// <summary>
        /// Creates and initializes PredefinedScenarios and ScoringSystem
        /// singletons needed by LoadScenario.
        /// </summary>
        private void SetupScenarioDependencies()
        {
            // PredefinedScenarios
            var scenariosGo = new GameObject("Scenarios");
            var scenarios = scenariosGo.AddComponent<PredefinedScenarios>();
            var awakeMethod = typeof(PredefinedScenarios).GetMethod("Awake",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (awakeMethod != null)
                awakeMethod.Invoke(scenarios, null);

            // ScoringSystem
            var scoringGo = new GameObject("ScoringSystem");
            var scoring = scoringGo.AddComponent<ScoringSystem>();
            var scoringAwake = typeof(ScoringSystem).GetMethod("Awake",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (scoringAwake != null)
                scoringAwake.Invoke(scoring, null);
        }
    }
}
