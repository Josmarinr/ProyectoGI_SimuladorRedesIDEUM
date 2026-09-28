using System.Reflection;
using NUnit.Framework;
using SimRedes.Network;
using SimRedes.Simulation;
using SimRedes.Tangible;
using SimRedes.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Tests.EditMode.Simulation
{
    /// <summary>
    /// Tests de la tarea C4 (input unificado): loop de teclado en un solo
    /// lugar (MenuNavigator vs MainMenuManager), colisiones de teclas R y P
    /// resueltas, guardia de vuelta al menu sin Canvas y tolerancia a
    /// TopologyManager null en DynamicRoutingActivity.
    /// </summary>
    public class TestInputUnification
    {
        private const BindingFlags DeclaredInstancePrivate =
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        [SetUp]
        public void SetUp()
        {
            ClearTopologyManagers();
            ClearSceneCleanupServices();
            ClearRoutingTablesActivities();
            ClearCanvases();
        }

        [TearDown]
        public void TearDown()
        {
            ClearTopologyManagers();
            ClearSceneCleanupServices();
            ClearRoutingTablesActivities();
            ClearCanvases();
        }

        // ================================================================
        // Helpers de limpieza (aislamiento entre suites)
        // ================================================================

        private static void ClearTopologyManagers()
        {
            foreach (var tm in Object.FindObjectsByType<TopologyManager>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(tm.gameObject);
            }

            var field = typeof(TopologyManager).GetField("<Instance>k__BackingField",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (field != null) field.SetValue(null, null);
        }

        private static void ClearSceneCleanupServices()
        {
            foreach (var s in Object.FindObjectsByType<SceneCleanupService>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(s.gameObject);
            }

            var field = typeof(SceneCleanupService).GetField("<Instance>k__BackingField",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (field != null) field.SetValue(null, null);
        }

        private static void ClearRoutingTablesActivities()
        {
            foreach (var a in Object.FindObjectsByType<RoutingTablesActivity>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(a.gameObject);
            }
        }

        private static void ClearCanvases()
        {
            foreach (var c in Object.FindObjectsByType<Canvas>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(c.gameObject);
            }
        }

        private static Button CreateButton(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.AddComponent<Button>();
        }

        // ================================================================
        // C4a — Loop de teclado en un solo lugar
        // ================================================================

        [Test]
        public void MainMenuManager_DoesNotDeclareKeyboardLoop()
        {
            // Arrange & Act: reflexion sobre los miembros declarados por la clase
            Assert.IsNull(typeof(MainMenuManager).GetMethod("Update", DeclaredInstancePrivate),
                "MainMenuManager no debe declarar su propio Update de teclado");
            Assert.IsNull(typeof(MainMenuManager).GetMethod("HandleKeyboardNavigation", DeclaredInstancePrivate),
                "El loop de navegacion vive solo en MenuNavigator (C4a)");
            Assert.IsNull(typeof(MainMenuManager).GetMethod("SelectButtonByIndex", DeclaredInstancePrivate),
                "Sin loop no debe sobrevivir el helper muerto de seleccion numerica");
            Assert.IsNull(typeof(MainMenuManager).GetMethod("GoBackToPreviousMenu", DeclaredInstancePrivate),
                "ESC sin dueño en MainMenuManager: MenuNavigator.onEscape absorbe el caso");
        }

        [Test]
        public void MenuNavigator_DeclaresKeyboardPollingLoop()
        {
            var update = typeof(MenuNavigator).GetMethod("Update", DeclaredInstancePrivate);
            Assert.IsNotNull(update,
                "MenuNavigator es el unico dueño del loop de navegacion (flechas, digitos, ESC)");
        }

        [Test]
        public void MenuNavigator_SelectAndInvoke_InvokesMappedButton()
        {
            // Arrange: panel con 2 botones; el digito N selecciona el boton N-1
            var panelGo = new GameObject("NavPanel");
            var navGo = new GameObject("MenuNavigator");
            try
            {
                Button btn0 = CreateButton(panelGo.transform, "Btn0");
                Button btn1 = CreateButton(panelGo.transform, "Btn1");
                int invoked = 0;
                btn0.onClick.AddListener(() => invoked += 10);
                btn1.onClick.AddListener(() => invoked += 1);

                var nav = navGo.AddComponent<MenuNavigator>();
                nav.SetupPanel(panelGo, null);

                var select = typeof(MenuNavigator).GetMethod("SelectAndInvoke", DeclaredInstancePrivate);
                Assert.IsNotNull(select, "SelectAndInvoke debe existir (atajos numericos 1-6)");

                // Act
                select.Invoke(nav, new object[] { 1 });
                select.Invoke(nav, new object[] { 0 });

                // Assert
                Assert.AreEqual(11, invoked,
                    "Los digitos deben invocar exactamente una vez el boton mapeado");
            }
            finally
            {
                Object.DestroyImmediate(navGo);
                Object.DestroyImmediate(panelGo);
            }
        }

        [Test]
        public void MenuNavigator_SelectAndInvoke_IndexOutOfRange_DoesNotInvoke()
        {
            var panelGo = new GameObject("NavPanel");
            var navGo = new GameObject("MenuNavigator");
            try
            {
                CreateButton(panelGo.transform, "Btn0");
                CreateButton(panelGo.transform, "Btn1");
                int invoked = 0;
                foreach (var b in panelGo.GetComponentsInChildren<Button>())
                    b.onClick.AddListener(() => invoked++);

                var nav = navGo.AddComponent<MenuNavigator>();
                nav.SetupPanel(panelGo, null);

                var select = typeof(MenuNavigator).GetMethod("SelectAndInvoke", DeclaredInstancePrivate);

                // Act: digito fuera de rango (equivale al digito 6 del MainMenuPanel)
                Assert.DoesNotThrow(() => select.Invoke(nav, new object[] { 5 }));

                // Assert
                Assert.AreEqual(0, invoked, "Un digito fuera de rango no debe invocar nada");
            }
            finally
            {
                Object.DestroyImmediate(navGo);
                Object.DestroyImmediate(panelGo);
            }
        }

        // ================================================================
        // C4b.1 — R: Eliminar (global) vs Refrescar tablas (panel abierto)
        // ================================================================

        [Test]
        public void IsRefreshPanelVisible_FollowsPanelVisibility()
        {
            // Arrange
            var actGo = new GameObject("RoutingTablesActivity");
            var textGo = new GameObject("TableText", typeof(RectTransform));
            var act = actGo.AddComponent<RoutingTablesActivity>();
            try
            {
                act.tableText = textGo.AddComponent<Text>();

                // Act & Assert: panel visible → R = refrescar
                Assert.IsTrue(act.IsRefreshPanelVisible,
                    "Con el panel de tablas visible R debe refrescar");

                // Panel oculto → R deja de ser refresh
                textGo.SetActive(false);
                Assert.IsFalse(act.IsRefreshPanelVisible,
                    "Con el panel oculto R vuelve a ser Eliminar");

                // Texto destruido (panel cerrado y destruido) → fuera de scope
                textGo.SetActive(true);
                Object.DestroyImmediate(textGo);
                Assert.IsFalse(act.IsRefreshPanelVisible,
                    "Con el texto destruido R vuelve a ser Eliminar");

                act.tableText = null;
                Assert.IsFalse(act.IsRefreshPanelVisible,
                    "Sin texto de tabla no hay panel que reclamar la tecla R");
            }
            finally
            {
                Object.DestroyImmediate(actGo);
            }
        }

        [Test]
        public void ShouldRemoveSelectedNodeOnR_WithoutRoutingActivity_ReturnsTrue()
        {
            // Arrange: ninguna actividad de tablas en escena
            ClearRoutingTablesActivities();

            // Act
            bool shouldRemove = SimulationControls.ShouldRemoveSelectedNodeOnR();

            // Assert: fuera de la actividad, R = Eliminar (comportamiento global)
            Assert.IsTrue(shouldRemove,
                "Sin panel de tablas, R debe eliminar el nodo seleccionado");
        }

        [Test]
        public void ShouldRemoveSelectedNodeOnR_WithVisiblePanel_ReturnsFalse()
        {
            // Arrange
            ClearRoutingTablesActivities();
            var actGo = new GameObject("RoutingTablesActivity");
            var textGo = new GameObject("TableText", typeof(RectTransform));
            var act = actGo.AddComponent<RoutingTablesActivity>();
            act.tableText = textGo.AddComponent<Text>();
            try
            {
                // Act
                bool shouldRemove = SimulationControls.ShouldRemoveSelectedNodeOnR();

                // Assert: con el panel visible, R la reclama la actividad (refresh)
                Assert.IsFalse(shouldRemove,
                    "Con el panel de tablas abierto R no debe eliminar nodos");
            }
            finally
            {
                Object.DestroyImmediate(actGo);
                Object.DestroyImmediate(textGo);
            }
        }

        [Test]
        public void ShouldRemoveSelectedNodeOnR_WithHiddenPanel_ReturnsTrue()
        {
            // Arrange
            ClearRoutingTablesActivities();
            var actGo = new GameObject("RoutingTablesActivity");
            var textGo = new GameObject("TableText", typeof(RectTransform));
            textGo.SetActive(false);
            var act = actGo.AddComponent<RoutingTablesActivity>();
            act.tableText = textGo.AddComponent<Text>();
            try
            {
                // Act
                bool shouldRemove = SimulationControls.ShouldRemoveSelectedNodeOnR();

                // Assert
                Assert.IsTrue(shouldRemove,
                    "Con el panel inactivo R debe seguir siendo Eliminar");
            }
            finally
            {
                Object.DestroyImmediate(actGo);
                Object.DestroyImmediate(textGo);
            }
        }

        // ================================================================
        // C4b.2 — P: un unico dueño (SimulationControls.ExecutePing)
        // ================================================================

        [Test]
        public void DebugDiscSimulator_HasNoPingKeyBinding()
        {
            var field = typeof(DebugDiscSimulator).GetField("pingTestKey",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNull(field,
                "DebugDiscSimulator ya no maneja P: el unico dueño del ping es SimulationControls");
        }

        [Test]
        public void SimulationControls_DeclaresExecutePingAsPOwner()
        {
            var executePing = typeof(SimulationControls).GetMethod("ExecutePing", DeclaredInstancePrivate);

            Assert.IsNotNull(executePing,
                "SimulationControls conserva ExecutePing como comportamiento de la tecla P");
        }

        // ================================================================
        // C4c.1 — GoBackToMainMenu sin Canvas no debe dejar escena sin menu
        // ================================================================

        [Test]
        public void ShouldSkipMainMenuSweep_GuardsMenuRebuildability()
        {
            // Arrange
            var canvasGo = new GameObject("SweepCanvas");
            // Inactivo para que Awake() de SceneSetup no se ejecute al agregarlo
            var setupGo = new GameObject("SweepSceneSetup");
            setupGo.SetActive(false);
            try
            {
                Canvas canvas = canvasGo.AddComponent<Canvas>();
                SceneSetup sceneSetup = setupGo.AddComponent<SceneSetup>();

                // Act & Assert
                Assert.IsTrue(SimulationControls.ShouldSkipMainMenuSweep(null, sceneSetup),
                    "Sin Canvas no hay donde recrear el menu: barrido saltado");
                Assert.IsTrue(SimulationControls.ShouldSkipMainMenuSweep(canvas, null),
                    "Sin SceneSetup no hay quien recree el menu: barrido saltado");
                Assert.IsFalse(SimulationControls.ShouldSkipMainMenuSweep(canvas, sceneSetup),
                    "Con Canvas y SceneSetup la ruta normal (barrido + recrear menu) se ejecuta");
            }
            finally
            {
                Object.DestroyImmediate(canvasGo);
                Object.DestroyImmediate(setupGo);
            }
        }

        [Test]
        public void GoBackToMainMenu_WithoutCanvas_SkipsDestructiveSweep()
        {
            // Arrange: escena sin Canvas ni SceneSetup
            ClearSceneCleanupServices();
            var go = new GameObject("SimulationControls_NoCanvas");
            var controls = go.AddComponent<SimulationControls>();
            try
            {
                // Act
                controls.GoBackToMainMenu();

                // Assert: la ruta destructiva (GetOrCreate + barrido) nunca arranco
                Assert.IsNull(Object.FindAnyObjectByType<SceneCleanupService>(),
                    "Sin Canvas el barrido destructivo no debe iniciarse (dejaria escena sin menu)");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        // ================================================================
        // C4c.2 — DynamicRoutingActivity tolera TopologyManager null
        // ================================================================

        [Test]
        public void DynamicRouting_StartWithoutTopology_ShowsEmptyStateInTablesText()
        {
            // Arrange: sin TopologyManager en escena
            ClearTopologyManagers();
            var textGo = new GameObject("TablesText", typeof(RectTransform));
            var actGo = new GameObject("DynamicRoutingActivity");
            var act = actGo.AddComponent<DynamicRoutingActivity>();
            act.tablesText = textGo.AddComponent<Text>();
            try
            {
                var start = typeof(DynamicRoutingActivity).GetMethod("Start", DeclaredInstancePrivate);

                // Act
                Assert.DoesNotThrow(() => start.Invoke(act, null),
                    "Start() no debe fallar sin TopologyManager");

                // Assert: estado vacio renderizado, no texto viejo sin actualizar
                StringAssert.Contains("Necesitas al menos 2 routers", act.tablesText.text,
                    "Sin topologia el display debe mostrar el estado vacio");
            }
            finally
            {
                Object.DestroyImmediate(actGo);
                Object.DestroyImmediate(textGo);
            }
        }

        [Test]
        public void DynamicRouting_StartProtocolWithoutTopology_ShowsFeedbackAndDoesNotThrow()
        {
            // Arrange
            var feedbackGo = new GameObject("FeedbackText", typeof(RectTransform));
            var actGo = new GameObject("DynamicRoutingActivity");
            var act = actGo.AddComponent<DynamicRoutingActivity>();
            act.feedbackText = feedbackGo.AddComponent<Text>();
            try
            {
                // Act: topologyManager sigue null (Start no corrio)
                Assert.DoesNotThrow(() => act.StartProtocol(null),
                    "StartProtocol sin topologia no debe lanzar NRE");

                // Assert: estado vacio en el feedback (antes era retorno silencioso)
                StringAssert.Contains("topología", act.feedbackText.text,
                    "Sin topologia debe reportar el estado vacio al usuario");
            }
            finally
            {
                Object.DestroyImmediate(actGo);
                Object.DestroyImmediate(feedbackGo);
            }
        }

        [Test]
        public void DynamicRouting_ShowRoutesWithoutTopology_DoesNotThrow()
        {
            // Arrange
            var actGo = new GameObject("DynamicRoutingActivity");
            var act = actGo.AddComponent<DynamicRoutingActivity>();
            var panelGo = new GameObject("DynPanel", typeof(RectTransform));
            var rightGo = new GameObject("RightPanel", typeof(RectTransform));
            rightGo.transform.SetParent(panelGo.transform, false);
            var rightText = rightGo.AddComponent<Text>();
            try
            {
                // Act
                Assert.DoesNotThrow(() => act.ShowRoutes(panelGo),
                    "ShowRoutes sin topologia no debe lanzar NRE");

                // Assert: encabezado con lista vacia de routers
                StringAssert.Contains("TABLAS DE RUTAS", rightText.text);
            }
            finally
            {
                Object.DestroyImmediate(actGo);
                Object.DestroyImmediate(panelGo);
            }
        }

        [Test]
        public void DynamicRouting_ClearAllRoutesWithoutTopology_DoesNotThrow()
        {
            // Arrange
            var feedbackGo = new GameObject("FeedbackText", typeof(RectTransform));
            var actGo = new GameObject("DynamicRoutingActivity");
            var act = actGo.AddComponent<DynamicRoutingActivity>();
            act.feedbackText = feedbackGo.AddComponent<Text>();
            try
            {
                // Act
                Assert.DoesNotThrow(() => act.ClearAllRoutes(null),
                    "ClearAllRoutes sin topologia no debe lanzar NRE");

                // Assert
                StringAssert.Contains("Rutas limpiadas", act.feedbackText.text);
            }
            finally
            {
                Object.DestroyImmediate(actGo);
                Object.DestroyImmediate(feedbackGo);
            }
        }
    }
}
