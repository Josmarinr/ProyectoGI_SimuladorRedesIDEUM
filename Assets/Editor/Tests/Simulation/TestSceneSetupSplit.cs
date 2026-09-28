using System.Reflection;
using NUnit.Framework;
using SimRedes.Network;
using SimRedes.Simulation;
using SimRedes.Tangible;
using SimRedes.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Tests.EditMode.Simulation
{
    /// <summary>
    /// Pruebas de paridad de la division de god class C5b (<see cref="SceneSetup"/>):
    /// la fachada conserva su superficie publica, la raiz de composicion
    /// (<see cref="SceneBootstrap"/>) reproduce el wiring original y la navegacion
    /// (<see cref="SceneNavigation"/>) maneja menu y paneles como antes.
    /// </summary>
    public class TestSceneSetupSplit
    {
        private GameObject setupGo;
        private SceneSetup setup;
        private GameObject canvasGo;
        private bool mainCameraExisted;

        private static readonly System.Type[] SingletonTypes =
        {
            typeof(TopologyManager),
            typeof(TangibleDiscManager),
            typeof(PingVisualizer),
            typeof(LinkModeController),
            typeof(SceneCleanupService),
            typeof(MenuNavigator),
            typeof(MainMenuManager),
            typeof(ScoringSystem),
            typeof(PredefinedScenarios)
        };

        [SetUp]
        public void SetUp()
        {
            mainCameraExisted = GameObject.Find("MainCamera") != null;
            CleanupSceneObjects();
            foreach (var type in SingletonTypes) ClearSingleton(type);
            ClearTangibleEngineSingleton();

            canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();

            // GO inactivo: Awake() de SceneSetup no debe ejecutarse al agregarlo
            setupGo = new GameObject("SceneSetup");
            setupGo.SetActive(false);
            setup = setupGo.AddComponent<SceneSetup>();
        }

        [TearDown]
        public void TearDown()
        {
            CleanupSceneObjects();
            if (!mainCameraExisted) DestroyByName("MainCamera");
            foreach (var type in SingletonTypes) ClearSingleton(type);
            ClearTangibleEngineSingleton();

            if (canvasGo != null) Object.DestroyImmediate(canvasGo);
            if (setupGo != null) Object.DestroyImmediate(setupGo);
        }

        // ================================================================
        // Helpers
        // ================================================================

        private static void DestroyByName(string name)
        {
            var go = GameObject.Find(name);
            if (go != null) Object.DestroyImmediate(go);
        }

        private static void CleanupSceneObjects()
        {
            DestroyByName("GameManager");
            DestroyByName("TE.TangibleEngine");
            DestroyByName("EventSystem");
            DestroyByName("MainMenuPanel");
            DestroyByName("MainMenuManager");
            DestroyByName("MenuNavigator");
            DestroyByName("ActivitiesPanel");
            DestroyByName("SceneCleanupService");
            DestroyByName("PingVisualizer");
            DestroyByName("TopologyInfoPanel");
            DestroyByName("TopologyManager");
            DestroyByName("NodeVisualizer");
            // Residuos de otros fixtures (paneles volantes con Sprite)
            DestroyByName("ConnectivityPanel");
            foreach (var rt in Object.FindObjectsByType<RectTransform>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (rt != null && rt.name.StartsWith("ClickOutsideBG"))
                    Object.DestroyImmediate(rt.gameObject);
            }
        }

        private static void ClearSingleton(System.Type type)
        {
            var field = type.GetField("<Instance>k__BackingField",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (field != null) field.SetValue(null, null);
        }

        private static void ClearTangibleEngineSingleton()
        {
            var field = typeof(TE.TangibleEngine).GetField("_instance",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (field != null) field.SetValue(null, null);
        }

        // ================================================================
        // FACHADA — superficie publica preservada (C5b)
        // ================================================================

        [Test]
        public void Facade_PublicForwards_AreCallableInEmptyScene()
        {
            // Act & Assert — cada forward responde con sus defaults sin lanzar
            Assert.DoesNotThrow(() =>
            {
                setup.ToggleLinkMode("connect");
                setup.HandleNodeClick(1);
                setup.ClearSelectedNode();
                setup.RefreshDevicesPanel();
                setup.UpdateScoreDisplay();
                setup.SelectActivity(0);
                setup.SubscribeToTopologyEvents();
            }, "Los forwards publicos de la fachada deben seguir siendo invocables");

            Assert.IsFalse(setup.IsLinkModeActive(), "Sin LinkModeController el modoConexion es false");
            Assert.IsFalse(setup.IsPingModeActive(), "Sin PingModeController el modo ping es false");
            Assert.IsFalse(setup.IsIPConfigPanelOpen(), "Sin IPConfigPanel el panel es false");
            Assert.AreEqual(-1, setup.GetCurrentIPConfigNodeDiscId(),
                "Sin IPConfigController el DiscId por defecto es -1");
        }

        // ================================================================
        // RAIZ DE COMPOSICION — SceneBootstrap extraida (C5b)
        // ================================================================

        [Test]
        public void Bootstrap_SetupScene_ConfiguresCanvasScalerAndEventSystem()
        {
            // Act — SetupScene() delega en SceneBootstrap.SetupSceneCore()
            setup.SetupScene();

            // Assert — canvas reutilizado con la resolucion de referencia del simulador
            var canvas = Object.FindAnyObjectByType<Canvas>();
            Assert.IsNotNull(canvas, "SetupScene debe resolver el Canvas");
            var scaler = canvas.GetComponent<CanvasScaler>();
            Assert.IsNotNull(scaler, "SetupScene debe configurar el CanvasScaler");
            Assert.AreEqual(new Vector2(4096, 2160), scaler.referenceResolution,
                "La resolucion de referencia debe ser 4096x2160");
            Assert.AreEqual(0.5f, scaler.matchWidthOrHeight,
                "matchWidthOrHeight debe ser 0.5");

            // Assert — EventSystem con el modulo del Input System nuevo
            var eventSystem = Object.FindAnyObjectByType<EventSystem>();
            Assert.IsNotNull(eventSystem, "SetupScene debe crear el EventSystem si falta");
            Assert.IsNotNull(eventSystem.GetComponent<InputSystemUIInputModule>(),
                "El EventSystem debe usar InputSystemUIInputModule");
        }

        [Test]
        public void SetupManagers_CreatesGameManagerWithAllManagers()
        {
            // Act — el wiring original de SetupManagers, ahora en SceneBootstrap
            setup.SetupManagers();

            // Assert
            var gameManager = GameObject.Find("GameManager");
            Assert.IsNotNull(gameManager, "SetupManagers debe crear el GameObject GameManager");
            Assert.IsNotNull(gameManager.GetComponent<TangibleDiscManager>(), "TangibleDiscManager");
            Assert.IsNotNull(gameManager.GetComponent<TopologyManager>(), "TopologyManager");
            Assert.IsNotNull(gameManager.GetComponent<DiscEventHandler>(), "DiscEventHandler");
            Assert.IsNotNull(gameManager.GetComponent<TangibleBridge>(), "TangibleBridge");
            Assert.IsNotNull(gameManager.GetComponent<SimulationControls>(), "SimulationControls");
            Assert.IsNotNull(gameManager.GetComponent<PingVisualizer>(), "PingVisualizer");
            Assert.IsNotNull(gameManager.GetComponent<LinkModeController>(), "LinkModeController");
            Assert.IsNotNull(gameManager.GetComponent<PingModeController>(), "PingModeController");
            Assert.IsNotNull(gameManager.GetComponent<IPConfigController>(), "IPConfigController");
            Assert.IsNotNull(gameManager.GetComponent<DevicePanelController>(), "DevicePanelController");
            Assert.IsNotNull(gameManager.GetComponent<NodeInteractionController>(), "NodeInteractionController");
            Assert.IsNotNull(gameManager.GetComponent<ActivityLoader>(), "ActivityLoader");
            Assert.IsNotNull(Object.FindAnyObjectByType<TE.TangibleEngine>(),
                "SetupManagers debe crear TE.TangibleEngine si falta");
            Assert.IsNotNull(Object.FindAnyObjectByType<SceneCleanupService>(),
                "SetupManagers debe crear SceneCleanupService si falta");
        }

        // ================================================================
        // NAVEGACION — SceneNavigation extraida (C5b)
        // ================================================================

        [Test]
        public void Facade_CreateMainMenu_BuildsMenuPanel()
        {
            // En EditMode Object.Destroy loguea un Error ("Destroy may not be called
            // from edit mode") al barrer paneles con Sprite en DestroyPreviousPanels.
            // Es comportamiento del entorno de test, no del codigo bajo test: en
            // runtime (play mode) Destroy es la llamada correcta. Mismo criterio que
            // TestSceneCleanupRoutes.DestroyPreviousPanels_WithPanelAndBackdrop_DoesNotThrow.
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
            try
            {
                // Act — la fachada delega en SceneNavigation.CreateMainMenu
                setup.CreateMainMenu(canvasGo.transform);
            }
            finally
            {
                UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
            }

            // Assert
            var menu = GameObject.Find("MainMenuPanel");
            Assert.IsNotNull(menu, "CreateMainMenu debe crear MainMenuPanel");
            Assert.IsNotNull(menu.transform.Find("Btn_Iniciar"),
                "El menu debe exponer el boton de iniciar simulacion");
            Assert.IsNotNull(Object.FindAnyObjectByType<MainMenuManager>(),
                "El menu debe montar su MainMenuManager");
        }

        [Test]
        public void Navigation_ShowActivities_ReplacesMenuWithActivitiesPanel()
        {
            // Arranque de menu y DestroyMainMenu usan Object.Destroy, que en EditMode
            // loguea "Destroy may not be called from edit mode" (ruido del entorno de
            // test; ver TestSceneCleanupRoutes). Se ignora solo durante la ejecucion.
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
            try
            {
                // Arrange — menu principal abierto
                var navigation = new SceneNavigation(setup);
                navigation.CreateMainMenu(canvasGo.transform);
                Assert.IsNotNull(GameObject.Find("MainMenuPanel"), "Precondicion: menu abierto");

                // Act
                navigation.ShowActivities(canvasGo.transform);
            }
            finally
            {
                UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
            }

            // Assert — el menu se reemplaza por el panel de actividades
            Assert.IsNotNull(GameObject.Find("ActivitiesPanel"),
                "ShowActivities debe crear el panel de actividades");
            Assert.IsNull(Object.FindAnyObjectByType<MainMenuManager>(),
                "DestroyMainMenu debe eliminar MainMenuManager con DestroyImmediate");
        }
    }
}
