using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using System.Collections.Generic;
using SimRedes.Tangible;
using SimRedes.Network;
using SimRedes.UI;
using SimRedes.Simulation;
using DeviceType = SimRedes.Network.DeviceType;
using UIComp = SimRedes.UI.UIComponents;
using UIColors = SimRedes.UI.UIComponents.Colors;
using IPValidation = SimRedes.Network.IPValidation;
using PingVis = SimRedes.UI.PingVisualizer;

namespace SimRedes
{
    public class SceneSetup : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private bool autoSetup = true;
        [SerializeField] private bool showMainMenuFirst = true;

        private TopologyManager topology;
        private NodeVisualizer visualizer;
        private NodeInteractionController nodeInteraction;
        private DevicePanelController devicePanel;
        private ActivityLoader activityLoader;
        private Canvas canvas;
        private Transform canvasTransform;
        private float scoreUpdateTimer = 0f;

        private Font font;
        private Font bigFont;

        // ==================== MONOBEHAVIOUR ====================

        private void Awake()
        {
            if (!autoSetup) return;
            SetupResolution();
            SetupCamera();
            canvas = SetupCanvas();
            canvasTransform = canvas.transform;
            font = UIComp.GetFont();
            bigFont = UIComp.GetFont(16);

            if (showMainMenuFirst)
                CreateMainMenu(canvasTransform);
            else
                StartSimulation();
        }

        private void Start()
        {
            CacheReferences();
            SubscribeToTopologyEvents();
        }

        private void Update()
        {
            scoreUpdateTimer += Time.deltaTime;
            if (scoreUpdateTimer >= 1f)
            {
                scoreUpdateTimer = 0f;
                if (devicePanel != null) devicePanel.UpdateScoreDisplay();
            }
        }

        protected virtual void OnDestroy()
        {
            UnsubscribeFromTopologyEvents();
        }

        // ==================== SETUP ====================

        private void SetupResolution()
        {
            Screen.SetResolution(1920, 1080, false);
        }

        private void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var camObj = new GameObject("MainCamera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }
            cam.orthographic = true;
            cam.orthographicSize = 540;
            cam.backgroundColor = new Color(0.05f, 0.067f, 0.09f);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        private Canvas SetupCanvas()
        {
            Canvas existingCanvas = FindObjectOfType<Canvas>();
            if (existingCanvas != null)
            {
                var scaler = existingCanvas.GetComponent<CanvasScaler>();
                if (scaler == null) scaler = existingCanvas.gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(4096, 2160);
                scaler.matchWidthOrHeight = 0.5f;
                return existingCanvas;
            }

            var canvasObj = new GameObject("Canvas");
            var canvasComp = canvasObj.AddComponent<Canvas>();
            canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            var scalerComp = canvasComp.GetComponent<CanvasScaler>();
            scalerComp.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scalerComp.referenceResolution = new Vector2(4096, 2160);
            scalerComp.matchWidthOrHeight = 0.5f;

            return canvasComp;
        }

        public void SetupManagers()
        {
            var gameManagerObj = GameObject.Find("GameManager");
            if (gameManagerObj == null)
                gameManagerObj = new GameObject("GameManager");

            if (gameManagerObj.GetComponent<TangibleDiscManager>() == null)
                gameManagerObj.AddComponent<TangibleDiscManager>();
            if (gameManagerObj.GetComponent<TopologyManager>() == null)
                gameManagerObj.AddComponent<TopologyManager>();
            if (gameManagerObj.GetComponent<DiscEventHandler>() == null)
                gameManagerObj.AddComponent<DiscEventHandler>();
            if (gameManagerObj.GetComponent<TangibleBridge>() == null)
                gameManagerObj.AddComponent<TangibleBridge>();
            if (gameManagerObj.GetComponent<DebugDiscSimulator>() == null)
                gameManagerObj.AddComponent<DebugDiscSimulator>();
            if (gameManagerObj.GetComponent<SimulationControls>() == null)
                gameManagerObj.AddComponent<SimulationControls>();
            if (gameManagerObj.GetComponent<TouchScriptDisabler>() == null)
                gameManagerObj.AddComponent<TouchScriptDisabler>();
            if (gameManagerObj.GetComponent<PingVisualizer>() == null)
                gameManagerObj.AddComponent<PingVisualizer>();
            if (FindObjectOfType<SceneCleanupService>() == null)
            {
                var cleanupObj = new GameObject("SceneCleanupService");
                var cleanup = cleanupObj.AddComponent<SceneCleanupService>();
                UnityEngine.Object.DontDestroyOnLoad(cleanupObj);
            }
            if (gameManagerObj.GetComponent<LinkModeController>() == null)
                gameManagerObj.AddComponent<LinkModeController>();
            if (gameManagerObj.GetComponent<PingModeController>() == null)
                gameManagerObj.AddComponent<PingModeController>();
            if (gameManagerObj.GetComponent<IPConfigController>() == null)
                gameManagerObj.AddComponent<IPConfigController>();
            if (gameManagerObj.GetComponent<DevicePanelController>() == null)
                gameManagerObj.AddComponent<DevicePanelController>();
            if (gameManagerObj.GetComponent<NodeInteractionController>() == null)
                gameManagerObj.AddComponent<NodeInteractionController>();
            if (gameManagerObj.GetComponent<ActivityLoader>() == null)
                gameManagerObj.AddComponent<ActivityLoader>();

            if (FindObjectOfType<TE.TangibleEngine>() == null)
            {
                var teObj = new GameObject("TE.TangibleEngine");
                teObj.AddComponent<TE.TangibleEngine>();
            }

            Debug.Log("[SceneSetup] Managers configurados");
        }

        public void CreateVisualizer(Transform ct)
        {
            if (FindObjectOfType<NodeVisualizer>() == null)
            {
                var visObj = new GameObject("NodeVisualizer");
                visObj.transform.SetParent(ct, false);
                var vis = visObj.AddComponent<NodeVisualizer>();
                vis.nodeContainer = new GameObject("NodeContainer").transform;
                vis.nodeContainer.SetParent(visObj.transform, false);
                vis.linkContainer = new GameObject("LinkContainer").transform;
                vis.linkContainer.SetParent(visObj.transform, false);
                visualizer = vis;
            }
            else
            {
                visualizer = FindObjectOfType<NodeVisualizer>();
            }
        }

        public void SetupScene()
        {
            SetupResolution();
            SetupCamera();
            canvas = SetupCanvas();
            canvasTransform = canvas.transform;
            font = UIComp.GetFont();
            bigFont = UIComp.GetFont(16);
        }

        private void CacheReferences()
        {
            topology = FindObjectOfType<TopologyManager>();
            visualizer = FindObjectOfType<NodeVisualizer>();
            canvas = FindObjectOfType<Canvas>();
            if (canvas != null) canvasTransform = canvas.transform;
            nodeInteraction = FindObjectOfType<NodeInteractionController>();
            devicePanel = FindObjectOfType<DevicePanelController>();
            activityLoader = FindObjectOfType<ActivityLoader>();
        }

        public void SubscribeToTopologyEvents()
        {
            if (topology != null)
            {
                topology.OnTopologyChanged -= OnTopologyChangedCallback;
            }
            topology = FindObjectOfType<TopologyManager>();
            visualizer = FindObjectOfType<NodeVisualizer>();
            if (topology != null)
            {
                topology.OnTopologyChanged += OnTopologyChangedCallback;
            }
        }

        private void UnsubscribeFromTopologyEvents()
        {
            if (topology != null)
            {
                topology.OnTopologyChanged -= OnTopologyChangedCallback;
            }
        }

        private void OnTopologyChangedCallback()
        {
            if (devicePanel != null) devicePanel.RefreshDevicesPanel();
        }

        // ==================== UI ====================

        private void CreateMainMenu(Transform ct)
        {
            UIPanelFactory.CreateMainMenu(ct,
                () => StartSimulation(),
                () => ShowActivities(ct),
                () => ShowConnectivity(ct),
                () => ShowInstructions(ct),
                () => ExitApplication());
        }

        public void CreateMainMenuPublic(Transform ct)
        {
            CreateMainMenu(ct);
        }

        private void StartSimulation()
        {
            SetupManagers();
            CreateVisualizer(canvasTransform);
            SubscribeToTopologyEvents();
            CacheReferences();
            if (activityLoader != null) activityLoader.StartSimulation(canvasTransform);
        }

        private void ShowActivities(Transform ct)
        {
            UIPanelFactory.CreateActivitiesPanel(ct,
                (index) => {
                    SetupManagers();
                    CreateVisualizer(canvasTransform);
                    SubscribeToTopologyEvents();
                    CacheReferences();
                    if (activityLoader != null)
                        activityLoader.SelectActivity(index);
                },
                () => { CreateMainMenu(ct); });
        }

        private void ShowConnectivity(Transform ct)
        {
            SetupManagers();
            CreateVisualizer(canvasTransform);
            SubscribeToTopologyEvents();
            CacheReferences();
            if (activityLoader != null) activityLoader.ShowConnectivityPanel(ct);
        }

        private void ShowInstructions(Transform ct)
        {
            UIPanelFactory.CreateInstructionsPanel(ct,
                () => { CreateMainMenu(ct); });
        }

        public void ExitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void SelectActivity(int index)
        {
            if (activityLoader != null)
                activityLoader.SelectActivity(index);
        }

        // ==================== PUBLIC FORWARDS ====================

        // LinkMode
        public void ToggleLinkMode(string mode)
        {
            var lm = FindObjectOfType<LinkModeController>();
            if (lm != null) lm.ToggleLinkMode(mode);
        }

        public bool IsLinkModeActive()
        {
            var lm = FindObjectOfType<LinkModeController>();
            return lm != null && lm.IsLinkModeActive();
        }

        public bool IsPingModeActive()
        {
            var pm = FindObjectOfType<PingModeController>();
            return pm != null && pm.IsPingModeActive();
        }

        public bool IsIPConfigPanelOpen()
        {
            var ip = FindObjectOfType<IPConfigController>();
            return ip != null && ip.IsIPConfigPanelOpen();
        }

        public int GetCurrentIPConfigNodeDiscId()
        {
            var ip = FindObjectOfType<IPConfigController>();
            return ip != null ? ip.GetCurrentIPConfigNodeDiscId() : -1;
        }

        public void CloseIPConfigPanelPublic()
        {
            var ip = FindObjectOfType<IPConfigController>();
            if (ip != null) ip.CloseIPConfigPanelPublic();
        }

        public void HandleNodeClick(int discId)
        {
            if (nodeInteraction != null) nodeInteraction.HandleNodeClick(discId);
        }

        public void ShowIPConfigPanel(NetworkNode node, int discId)
        {
            if (nodeInteraction != null) nodeInteraction.ShowIPConfigPanel(node, discId);
        }

        // DevicePanel
        public void RemoveSelectedNodePublic()
        {
            if (devicePanel != null) devicePanel.RemoveSelectedNodePublic();
        }

        public void ClearSelectedNode()
        {
            if (devicePanel != null) devicePanel.ClearSelectedNode();
        }

        public void RefreshDevicesPanel()
        {
            if (devicePanel != null) devicePanel.RefreshDevicesPanel();
        }

        // Score
        public void UpdateScoreDisplay()
        {
            if (devicePanel != null) devicePanel.UpdateScoreDisplay();
        }
    }
}
