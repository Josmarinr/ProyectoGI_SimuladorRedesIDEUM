using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using SimRedes.Network;
using SimRedes.Simulation;
using SimRedes.Tangible;
using SimRedes.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
// Ambiguedad DeviceType (regla unity-code-style):Namespace explicito de la red.
using DeviceType = SimRedes.Network.DeviceType;

namespace Tests.EditMode.UI
{
    /// <summary>
    /// Tests for bug P2 (connection lines invisible with physical discs).
    /// Covers: the center-relative coordinate convention end-to-end (TE →
    /// TopologyManager → icon → line), icons following moved discs, RectTransform
    /// containers / layer stacking / raycast hardening, lazy topology resolution
    /// plus visual-cache resets after direct container wipes, and the gated
    /// [LinkDiag] telemetry used to confirm the fix on the physical table.
    /// </summary>
    public class TestNodeVisualizerLinks
    {
        private GameObject canvasGo;
        private Canvas canvas;
        private GameObject topologyGo;
        private TopologyManager topology;
        private GameObject setupGo;
        private SceneSetup setup;
        private GameObject bridgeGo;
        private TangibleBridge bridge;

        private static readonly BindingFlags DeclaredInstancePrivate =
            BindingFlags.Instance | BindingFlags.NonPublic;

        [SetUp]
        public void SetUp()
        {
            // Isolation (verified on the first run): otro fixture puede dejar un
            // TopologyManager o NodeVisualizer ACTIVO; FindAnyObjectByType los
            // devolveria a ellos en lugar de a los de este test (el visualizador
            // se suscribiria a una topologia ajena: 0 enlaces, nodos ajenos).
            foreach (var t in Object.FindObjectsByType<TopologyManager>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(t.gameObject);
            }
            foreach (var v in Object.FindObjectsByType<NodeVisualizer>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(v.gameObject);
            }

            // Exactly one CanvasScaler (ours): TangibleBridge resolves the canvas
            // reference resolution from the active scaler, so strays from other
            // fixtures must not be visible here.
            foreach (var scaler in Object.FindObjectsByType<CanvasScaler>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(scaler);
            }

            canvasGo = new GameObject("Canvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scalerComp = canvasGo.AddComponent<CanvasScaler>();
            scalerComp.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scalerComp.referenceResolution = new Vector2(4096f, 2160f);
            scalerComp.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            topology = CreateTopology("TopologyManager");

            // GO inactivo: Awake() de SceneSetup no debe ejecutarse al agregarlo
            // (haria SetupSceneCore con camara/resolucion). Patron de TestSceneSetupSplit.
            setupGo = new GameObject("SceneSetup");
            setupGo.SetActive(false);
            setup = setupGo.AddComponent<SceneSetup>();

            // Bridge sin Start() (no suscribirse a TE.TangibleEngine)
            bridgeGo = new GameObject("TangibleBridge");
            bridge = bridgeGo.AddComponent<TangibleBridge>();
        }

        [TearDown]
        public void TearDown()
        {
            // ClearTextureCache usa Object.Destroy (correcto en runtime, error en
            // EditMode): patrón de TestSceneCleanupRoutes — ignorar los mensajes
            // fallidos SOLO alrededor de la limpieza de texturas.
            LogAssert.ignoreFailingMessages = true;
            try
            {
                UIComponents.ClearTextureCache();
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }

            if (canvasGo != null) Object.DestroyImmediate(canvasGo);
            if (bridgeGo != null) Object.DestroyImmediate(bridgeGo);
            // setupGo esta inactivo: GameObject.Find no lo encuentra, destruir por referencia
            if (setupGo != null) Object.DestroyImmediate(setupGo);
            DestroyByName("NodeVisualizer");
            DestroyByName("TopologyManager");
            DestroyByName("GameManager");
            DestroyByName("SceneCleanupService");
            DestroyByName("DebugDiscSimulator");
            ClearSingleton<TopologyManager>();
            ClearSingleton<SceneCleanupService>();
        }

        // ================================================================
        // FIX 3 — Containers created as RectTransforms with centered anchors
        // ================================================================

        [Test]
        public void CreateVisualizer_CreatesCenterAnchoredRectTransformContainers()
        {
            // Act — the production creation path (SceneNavigation.StartSimulation →
            // SceneSetup.CreateVisualizer → SceneBootstrap.CreateVisualizer)
            var visualizer = CreateVisualizer();

            // Assert
            Assert.IsInstanceOf<RectTransform>(visualizer.nodeContainer,
                "P2: NodeContainer must be a RectTransform so anchors resolve against a real rect");
            Assert.IsInstanceOf<RectTransform>(visualizer.linkContainer,
                "P2: LinkContainer must be a RectTransform so anchors resolve against a real rect");

            var nodeRect = (RectTransform)visualizer.nodeContainer;
            Assert.AreEqual(new Vector2(0.5f, 0.5f), nodeRect.anchorMin, "NodeContainer anchored at center");
            Assert.AreEqual(new Vector2(0.5f, 0.5f), nodeRect.anchorMax, "NodeContainer anchored at center");
            Assert.AreEqual(new Vector2(0.5f, 0.5f), nodeRect.pivot, "NodeContainer pivot at center");
            Assert.AreEqual(Vector2.zero, nodeRect.anchoredPosition, "NodeContainer at the layer origin");

            var linkRect = (RectTransform)visualizer.linkContainer;
            Assert.AreEqual(new Vector2(0.5f, 0.5f), linkRect.anchorMin, "LinkContainer anchored at center");
            Assert.AreEqual(new Vector2(0.5f, 0.5f), linkRect.anchorMax, "LinkContainer anchored at center");
            Assert.AreEqual(new Vector2(0.5f, 0.5f), linkRect.pivot, "LinkContainer pivot at center");
            Assert.AreEqual(Vector2.zero, linkRect.anchoredPosition, "LinkContainer at the layer origin");
        }

        // ================================================================
        // FIX 1 — Full pipeline: TE → bridge → topology → icon on screen
        // ================================================================

        [Test]
        public void Pipeline_TableCenterDisc_IconLandsAtCanvasCenter()
        {
            // Arrange — the IDEUM table reports its center as (960,540) in the
            // 1920x1080 touch frame. Under the P2 center-relative convention this
            // must convert to (0,0).
            Vector2 tableCenter = InvokeConvertToCanvasPosition(new Vector2(960f, 540f));
            Assert.AreEqual(0f, tableCenter.x, 0.001f,
                "P2/H1: table center X must convert to 0 (center-relative convention)");
            Assert.AreEqual(0f, tableCenter.y, 0.001f,
                "P2/H1: table center Y must convert to 0 (center-relative convention)");

            topology.AddNode(1, DeviceType.Router, tableCenter);

            var visualizer = CreateVisualizer();
            InvokeStart(visualizer);
            Canvas.ForceUpdateCanvases();

            // Act/Assert — the created Node_* icon lands where the node says
            var icon = FindIcon(visualizer, "Node_Router_1");
            Assert.IsNotNull(icon, "The Node_Router_1 icon must be created");

            Vector2 local = CanvasLocalOf(icon);
            Assert.AreEqual(0f, local.x, 0.5f,
                "Icon of a table-center node must sit at canvas center X");
            Assert.AreEqual(0f, local.y, 0.5f,
                "Icon of a table-center node must sit at canvas center Y");

            // Canvas center == screen center for a ScreenSpaceOverlay canvas.
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, icon.position);
            Assert.AreEqual(Screen.width / 2f, screen.x, 2f,
                "Table-center icon must land at the horizontal screen center");
            Assert.AreEqual(Screen.height / 2f, screen.y, 2f,
                "Table-center icon must land at the vertical screen center");

            // A literal center-relative node.Position is honored verbatim.
            // NOTE: under the new convention (2048,1080) is NOT the center — the
            // center is (0,0) — but the icon must land exactly where the node says.
            topology.AddNode(2, DeviceType.PC, new Vector2(2048f, 1080f));
            Canvas.ForceUpdateCanvases();

            var icon2 = FindIcon(visualizer, "Node_PC_2");
            Assert.IsNotNull(icon2, "The Node_PC_2 icon must be created");
            Vector2 local2 = CanvasLocalOf(icon2);
            Assert.AreEqual(2048f, local2.x, 0.5f,
                "node.Position is used verbatim as center-relative offset");
            Assert.AreEqual(1080f, local2.y, 0.5f,
                "node.Position is used verbatim as center-relative offset");
        }

        [Test]
        public void Pipeline_KnownPositions_LinkMidpointAtExpectedCoordinate()
        {
            // Arrange — two known center-relative positions
            var p1 = new Vector2(-400f, -200f);
            var p2 = new Vector2(400f, 400f);
            topology.AddNode(1, DeviceType.Router, p1);
            topology.AddNode(2, DeviceType.PC, p2);
            topology.AddLink(1, 2);

            var visualizer = CreateVisualizer();
            InvokeStart(visualizer);
            Canvas.ForceUpdateCanvases();

            // Assert — exactly one line, mid-way between the two CURRENT positions
            Assert.AreEqual(1, visualizer.linkContainer.childCount,
                "Exactly one link line must be drawn");

            var line = visualizer.linkContainer.GetChild(0) as RectTransform;
            Assert.IsNotNull(line, "The link line must have a RectTransform");

            Vector2 expectedMid = (p1 + p2) / 2f;
            Vector2 lineLocal = CanvasLocalOf(line);
            Assert.AreEqual(expectedMid.x, lineLocal.x, 0.5f,
                "Line midpoint X must be the midpoint of the two node positions");
            Assert.AreEqual(expectedMid.y, lineLocal.y, 0.5f,
                "Line midpoint Y must be the midpoint of the two node positions");

            // …and at the expected screen coordinate (same mapping the icons use).
            var canvasRect = (RectTransform)canvas.transform;
            Vector2 expectedScreen = RectTransformUtility.WorldToScreenPoint(
                null, canvasRect.TransformPoint(expectedMid));
            Vector2 actualScreen = RectTransformUtility.WorldToScreenPoint(null, line.position);
            Assert.AreEqual(expectedScreen.x, actualScreen.x, 1f,
                "Line midpoint screen X must match the expected coordinate");
            Assert.AreEqual(expectedScreen.y, actualScreen.y, 1f,
                "Line midpoint screen Y must match the expected coordinate");
        }

        // ================================================================
        // FIX 2 (H2) — Icons follow moved discs; lines use CURRENT positions
        // ================================================================

        [Test]
        public void DrawLinks_AfterNodeMoved_IconsAndLineFollowCurrentPositions()
        {
            var p1 = new Vector2(-400f, -200f);
            var p2 = new Vector2(400f, 400f);
            topology.AddNode(1, DeviceType.Router, p1);
            topology.AddNode(2, DeviceType.PC, p2);
            topology.AddLink(1, 2);

            var visualizer = CreateVisualizer();
            InvokeStart(visualizer);

            // Act — sliding a disc on the table only updates node.Position
            var moved = new Vector2(100f, -350f);
            topology.UpdateNodePosition(1, moved);
            Canvas.ForceUpdateCanvases();

            // Assert — the icon follows the disc (P2/H2)
            var icon1 = FindIcon(visualizer, "Node_Router_1");
            Assert.IsNotNull(icon1, "The icon must still exist after the move");
            Vector2 local1 = CanvasLocalOf(icon1);
            Assert.AreEqual(moved.x, local1.x, 0.5f,
                "P2/H2: the icon must follow the moved disc (anchoredPosition refreshed from node.Position)");
            Assert.AreEqual(moved.y, local1.y, 0.5f,
                "P2/H2: the icon must follow the moved disc (anchoredPosition refreshed from node.Position)");

            // …and the line reconnects the CURRENT positions
            Assert.AreEqual(1, visualizer.linkContainer.childCount,
                "Exactly one link line must be drawn after the move");
            var line = visualizer.linkContainer.GetChild(0) as RectTransform;
            Vector2 expectedMid = (moved + p2) / 2f;
            Vector2 lineLocal = CanvasLocalOf(line);
            Assert.AreEqual(expectedMid.x, lineLocal.x, 0.5f,
                "P2/H2: line midpoint must be computed from the CURRENT icon positions");
            Assert.AreEqual(expectedMid.y, lineLocal.y, 0.5f,
                "P2/H2: line midpoint must be computed from the CURRENT icon positions");
        }

        // ================================================================
        // FIX 4 (H4) — Lazy topology resolution + resubscription
        // ================================================================

        [Test]
        public void DrawLinks_TopologyRecreated_ResolvesLazilyAndResubscribes()
        {
            topology.AddNode(1, DeviceType.Router, Vector2.zero);
            topology.AddNode(2, DeviceType.PC, new Vector2(400f, 300f));
            topology.AddLink(1, 2);

            var visualizer = CreateVisualizer();
            InvokeStart(visualizer);
            Assert.AreEqual(1, visualizer.linkContainer.childCount,
                "Baseline: the link is drawn with the original topology");

            // GameManager recreated: the cached topology becomes Unity fake-null
            Object.DestroyImmediate(topologyGo);
            topology = CreateTopology("TopologyManager");
            topology.AddNode(1, DeviceType.Router, Vector2.zero);
            topology.AddNode(2, DeviceType.PC, new Vector2(400f, 300f));
            topology.AddNode(3, DeviceType.Router, new Vector2(-300f, 150f));
            topology.AddLink(1, 2);

            // Act — DrawLinks must lazily resolve the NEW topology, resubscribe and
            // sync nodes that have no icon yet
            visualizer.DrawLinks();
            Canvas.ForceUpdateCanvases();

            Assert.AreEqual(1, visualizer.linkContainer.childCount,
                "P2/H4: DrawLinks must draw using the freshly resolved topology (not bail out on fake-null)");
            Assert.IsNotNull(FindIcon(visualizer, "Node_Router_3"),
                "P2/H4: nodes created after the recreation get their icons on the lazy resync");

            // The new subscription must be live: a later event creates its icon too
            topology.AddNode(4, DeviceType.PC, new Vector2(50f, -50f));
            Assert.IsNotNull(FindIcon(visualizer, "Node_PC_4"),
                "P2/H4: the visualizer must be resubscribed to the recreated topology");
        }

        // ================================================================
        // FIX 4 (H4) — Direct container wipes must reset the visual cache
        // ================================================================

        [Test]
        public void ClearSimulation_WipesContainersAndResetsVisualCache()
        {
            topology.AddNode(1, DeviceType.Router, Vector2.zero);
            var visualizer = CreateVisualizer();
            InvokeStart(visualizer);
            Assert.AreEqual(1, GetNodeObjectsCount(visualizer),
                "Baseline: the icon cache is populated");

            var cleanup = SceneCleanupService.GetOrCreate();

            // En EditMode el barrido de hijos (Object.Destroy) loguea un Error de
            // entorno — patrón de TestSceneCleanupRoutes: solo se ignoran los
            // mensajes durante el barrido; la aserción de abajo es la que importa.
            LogAssert.ignoreFailingMessages = true;
            try
            {
                cleanup.ClearSimulation(null, visualizer, null);
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }

            Assert.AreEqual(0, GetNodeObjectsCount(visualizer),
                "P2/H4: ResetVisuals must empty nodeObjects after the direct wipe, " +
                "otherwise DrawLinks skips every link with stale entries");
        }

        [Test]
        public void ForceClearAll_WithoutLiveTopology_ResetsVisualCache()
        {
            topology.AddNode(1, DeviceType.Router, Vector2.zero);
            var visualizer = CreateVisualizer();
            InvokeStart(visualizer);
            Assert.AreEqual(1, GetNodeObjectsCount(visualizer),
                "Baseline: the icon cache is populated");

            // The visualizer stays subscribed to an instance that no longer exists
            // (GameManager-recreated scenario): the direct wipe fires no events.
            Object.DestroyImmediate(topologyGo);
            topology = null;

            var discSim = new GameObject("DebugDiscSimulator").AddComponent<DebugDiscSimulator>();

            LogAssert.ignoreFailingMessages = true;
            try
            {
                discSim.ForceClearAll();
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }

            Assert.AreEqual(0, GetNodeObjectsCount(visualizer),
                "P2/H4: ClearAllDiscs must reset the visualizer cache after its direct wipe");
        }

        // ================================================================
        // FIX 3 — Raycast hardening for CONNECT mode
        // ================================================================

        [Test]
        public void CreateLinkLine_DisablesRaycastTarget()
        {
            topology.AddNode(1, DeviceType.Router, Vector2.zero);
            topology.AddNode(2, DeviceType.PC, new Vector2(300f, 0f));
            topology.AddLink(1, 2);

            var visualizer = CreateVisualizer();
            InvokeStart(visualizer);
            Canvas.ForceUpdateCanvases();

            Assert.AreEqual(1, visualizer.linkContainer.childCount, "One line must be drawn");
            var lineImage = visualizer.linkContainer.GetChild(0).GetComponent<RawImage>();
            Assert.IsNotNull(lineImage, "The link line is a RawImage");
            Assert.IsFalse(lineImage.raycastTarget,
                "P2: thick lines must not swallow taps needed by CONNECT mode");
        }

        // ================================================================
        // FIX 5 — Gated [LinkDiag] telemetry
        // ================================================================

        [Test]
        public void DrawLinks_DiagnosticsEnabled_EmitsLinkDiagLine()
        {
            topology.AddNode(1, DeviceType.Router, Vector2.zero);
            topology.AddNode(2, DeviceType.PC, new Vector2(300f, 0f));
            topology.AddLink(1, 2);

            var visualizer = CreateVisualizer();
            SetLogLinkDiagnostics(visualizer, true);

            // The final DrawLinks of Start() draws 1 line with 2 icons → matches.
            // Intermediate [LinkDiag] lines (type Log) do not fail the expectation.
            LogAssert.Expect(LogType.Log,
                new Regex(@"\[LinkDiag\] links=1 linkChildren=1 nodeObjects=2 first=anchored="));

            InvokeStart(visualizer);
        }

        // ================================================================
        // Helpers
        // ================================================================

        private NodeVisualizer CreateVisualizer()
        {
            var bootstrap = new SceneBootstrap(setup);
            bootstrap.CreateVisualizer(canvas.transform);
            var visualizer = Object.FindAnyObjectByType<NodeVisualizer>();
            Assert.IsNotNull(visualizer, "SceneBootstrap.CreateVisualizer must create the visualizer");
            return visualizer;
        }

        /// <summary>
        /// Crea una topologia con su Awake invocado y REGISTRA su GameObject en
        /// <c>topologyGo</c>: los tests que simulan la recreacion del GameManager
        /// hacen DestroyImmediate(topologyGo) y deben destruir la instancia REAL
        /// (si no, sobrevive y FindAnyObjectByType la devuelve a ella).
        /// </summary>
        private TopologyManager CreateTopology(string name)
        {
            var go = new GameObject(name);
            var topo = go.AddComponent<TopologyManager>();
            // Awake() no se invoca automaticamente en EditMode (guia de testing)
            var awake = typeof(TopologyManager).GetMethod("Awake", DeclaredInstancePrivate);
            awake?.Invoke(topo, null);
            topologyGo = go;
            return topo;
        }

        private static void InvokeStart(NodeVisualizer visualizer)
        {
            var start = typeof(NodeVisualizer).GetMethod("Start", DeclaredInstancePrivate);
            Assert.IsNotNull(start, "NodeVisualizer.Start not found");
            start.Invoke(visualizer, null);
        }

        private Vector2 InvokeConvertToCanvasPosition(Vector2 input)
        {
            var method = typeof(TangibleBridge).GetMethod("ConvertToCanvasPosition",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "ConvertToCanvasPosition not found");
            return (Vector2)method.Invoke(bridge, new object[] { input });
        }

        private static RectTransform FindIcon(NodeVisualizer visualizer, string iconName)
        {
            if (visualizer.nodeContainer == null) return null;
            var child = visualizer.nodeContainer.Find(iconName);
            return child as RectTransform;
        }

        /// <summary>
        /// Position of a transform expressed in canvas reference units (the space
        /// where node.Position lives). Asserting against this is independent of
        /// Screen size / CanvasScaler factor while still going through the real
        /// anchor + transform chain.
        /// </summary>
        private Vector2 CanvasLocalOf(Transform target)
        {
            var canvasRect = (RectTransform)canvas.transform;
            return canvasRect.InverseTransformPoint(target.position);
        }

        private static int GetNodeObjectsCount(NodeVisualizer visualizer)
        {
            var field = typeof(NodeVisualizer).GetField("nodeObjects", DeclaredInstancePrivate);
            Assert.IsNotNull(field, "nodeObjects field not found");
            return ((Dictionary<int, GameObject>)field.GetValue(visualizer)).Count;
        }

        private static void SetLogLinkDiagnostics(NodeVisualizer visualizer, bool value)
        {
            var field = typeof(NodeVisualizer).GetField("logLinkDiagnostics", DeclaredInstancePrivate);
            Assert.IsNotNull(field, "logLinkDiagnostics field not found");
            field.SetValue(visualizer, value);
        }

        private static void DestroyByName(string name)
        {
            var go = GameObject.Find(name);
            if (go != null) Object.DestroyImmediate(go);
        }

        private static void ClearSingleton<T>()
        {
            var field = typeof(T).GetField("<Instance>k__BackingField",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (field != null) field.SetValue(null, null);
        }
    }
}
