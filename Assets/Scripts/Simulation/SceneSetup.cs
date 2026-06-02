using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
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
    /// <summary>
    /// Punto de entrada principal de la escena. Configura resolucion, camara, canvas,
    /// managers globales, menu principal y el ciclo de vida de la simulacion.
    /// Se ejecuta en Awake() y orquesta la inicializacion completa del simulador.
    /// </summary>
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

        /// <summary>
        /// Inicializa la escena: configura resolucion, camara, canvas y decide
        /// si mostrar el menu principal o arrancar la simulacion directamente.
        /// </summary>
        private void Awake()
        {
            Debug.Log("[SceneSetup] Awake() iniciando...");
            if (!autoSetup) return;
            SetupResolution();
            SetupCamera();
            canvas = SetupCanvas();
            SetupEventSystem();
            canvasTransform = canvas.transform;
            font = UIComp.GetFont();
            bigFont = UIComp.GetFont(16);

            if (showMainMenuFirst)
                CreateMainMenu(canvasTransform);
            else
                StartSimulation();
            Debug.Log("[SceneSetup] Awake() completado");
        }

        /// <summary>
        /// Re-aplica configuracion de camara (por si URP la sobrescribe),
        /// cachea referencias a managers y se suscribe a eventos de topologia.
        /// </summary>
        private void Start()
        {
            Debug.Log("[SceneSetup] Start() - re-aplicando configuracion de camara por seguridad");
            // Re-aplicar configuracion de camara en Start() por si URP la sobrescribio
            EnsureCameraSetup();
            CacheReferences();
            SubscribeToTopologyEvents();
            Debug.Log("[SceneSetup] Start() completado");
        }

        /// <summary>
        /// Verifica y re-aplica la configuracion ortogonal de la camara principal.
        /// Ejecutado en Start() como medida de seguridad ante sobrescritura de URP.
        /// </summary>
        private void EnsureCameraSetup()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 540;
                cam.backgroundColor = new Color(0.05f, 0.067f, 0.09f);
                cam.clearFlags = CameraClearFlags.SolidColor;
                Debug.Log($"[SceneSetup] Camara re-configurada: clearFlags={cam.clearFlags}, bg={cam.backgroundColor}");
            }
            else
            {
                Debug.LogError("[SceneSetup] Camera.main es NULL en Start()");
            }
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
            Screen.SetResolution(4096, 2160, FullScreenMode.FullScreenWindow);
        }

        /// <summary>
        /// Crea o configura la camara principal en modo ortogonal con fondo oscuro.
        /// </summary>
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

        /// <summary>
        /// Crea o reutiliza el Canvas principal configurando el CanvasScaler
        /// con resolucion de referencia 4096x2160 y ScaleWithScreenSize.
        /// <see cref="SetupScene"/>
        /// </summary>
        /// <returns>Canvas configurado de la escena.</returns>
        private Canvas SetupCanvas()
        {
            Canvas existingCanvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
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

        /// <summary>
        /// Crea el EventSystem con InputSystemUIInputModule para que los clicks
        /// y el tactil funcionen con el nuevo Input System (activeInputHandler=2).
        /// Sin esto, los botones UI nunca reciben eventos de puntero.
        /// </summary>
        private void SetupEventSystem()
        {
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
            Debug.Log("[SceneSetup] EventSystem + InputSystemUIInputModule creados");
        }

        /// <summary>
        /// Instancia todos los managers globales (TangibleDiscManager, TopologyManager,
        /// DiscEventHandler, TangibleBridge, etc.) en un unico GameObject "GameManager".
        /// Tambien crea SceneCleanupService y TE.TangibleEngine si no existen.
        /// </summary>
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
            if (UnityEngine.Object.FindAnyObjectByType<SceneCleanupService>() == null)
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

            if (UnityEngine.Object.FindAnyObjectByType<TE.TangibleEngine>() == null)
            {
                var teObj = new GameObject("TE.TangibleEngine");
                teObj.AddComponent<TE.TangibleEngine>();
            }

            // Log del modo TangibleEngine
#if UNITY_EDITOR
            UnityEngine.Debug.Log("[SceneSetup] TangibleEngine modo: EDITOR (Simulator - sin conexion TCP)");
#else
            UnityEngine.Debug.Log("[SceneSetup] TangibleEngine modo: RUNTIME (Service - TCP localhost:4949)");
#endif

            Debug.Log("[SceneSetup] Managers configurados");
        }

        /// <summary>
        /// Crea u obtiene el NodeVisualizer y sus contenedores de nodos y enlaces
        /// bajo el transform del Canvas.
        /// </summary>
        /// <param name="ct">Transform del Canvas donde se insertara el visualizador.</param>
        public void CreateVisualizer(Transform ct)
        {
            if (UnityEngine.Object.FindAnyObjectByType<NodeVisualizer>() == null)
            {
                var visObj = new GameObject("NodeVisualizer");
                visObj.transform.SetParent(ct, false);
                visObj.transform.SetAsLastSibling(); // Renderizar encima de otros paneles
                var vis = visObj.AddComponent<NodeVisualizer>();
                vis.nodeContainer = new GameObject("NodeContainer").transform;
                vis.nodeContainer.SetParent(visObj.transform, false);
                vis.linkContainer = new GameObject("LinkContainer").transform;
                vis.linkContainer.SetParent(visObj.transform, false);
                visualizer = vis;
            }
            else
            {
                visualizer = UnityEngine.Object.FindAnyObjectByType<NodeVisualizer>();
            }
        }

        /// <summary>
        /// Configura manualmente resolucion, camara y canvas.
        /// Util cuando se omite el autoSetup en favor de inicializacion diferida.
        /// </summary>
        public void SetupScene()
        {
            SetupResolution();
            SetupCamera();
            canvas = SetupCanvas();
            SetupEventSystem();
            canvasTransform = canvas.transform;
            font = UIComp.GetFont();
            bigFont = UIComp.GetFont(16);
        }

        /// <summary>
        /// Busca y cachea referencias a los managers principales (TopologyManager,
        /// NodeVisualizer, NodeInteractionController, DevicePanelController, ActivityLoader).
        /// </summary>
        private void CacheReferences()
        {
            topology = UnityEngine.Object.FindAnyObjectByType<TopologyManager>();
            visualizer = UnityEngine.Object.FindAnyObjectByType<NodeVisualizer>();
            canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (canvas != null) canvasTransform = canvas.transform;
            nodeInteraction = UnityEngine.Object.FindAnyObjectByType<NodeInteractionController>();
            devicePanel = UnityEngine.Object.FindAnyObjectByType<DevicePanelController>();
            activityLoader = UnityEngine.Object.FindAnyObjectByType<ActivityLoader>();
        }

        /// <summary>
        /// Se suscribe al evento OnTopologyChanged del TopologyManager
        /// para refrescar automaticamente el panel de dispositivos.
        /// </summary>
        public void SubscribeToTopologyEvents()
        {
            if (topology != null)
            {
                topology.OnTopologyChanged -= OnTopologyChangedCallback;
            }
            topology = UnityEngine.Object.FindAnyObjectByType<TopologyManager>();
            visualizer = UnityEngine.Object.FindAnyObjectByType<NodeVisualizer>();
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

        /// <summary>
        /// Elimina solo el menu principal al hacer clic en una opcion (INICIAR, ACTIVIDADES, etc.),
        /// conservando el MenuNavigator para que los sub-paneles tengan animaciones.
        /// </summary>
        private void DestroyMainMenu()
        {
            GameObject panel = GameObject.Find("MainMenuPanel");
            if (panel != null) GameObject.Destroy(panel);

            GameObject hudPanel = GameObject.Find("TopologyInfoPanel");
            if (hudPanel != null) GameObject.Destroy(hudPanel);

            // Usar DestroyImmediate para evitar conflictos de singleton con destruccion diferida
            // al recrear el menu en el mismo frame (ej: desde el panel de Instrucciones)
            var mm = UnityEngine.Object.FindAnyObjectByType<MainMenuManager>();
            if (mm != null) GameObject.DestroyImmediate(mm.gameObject);

            // NO destruir MenuNavigator — se reutiliza para dar animaciones a los sub-paneles
            Debug.Log("[SceneSetup] Menu principal destruido (navigator conservado)");
        }

        /// <summary>
        /// Crea el menu principal limpiando paneles previos y delegando en
        /// <see cref="UIPanelFactory.CreateMainMenu"/> con sus callbacks.
        /// </summary>
        /// <param name="ct">Transform del Canvas donde se instancia el menu.</param>
        private void CreateMainMenu(Transform ct)
        {
            // Limpiar cualquier panel que haya quedado abierto antes de crear el menu
            DestroyPreviousPanels();
            UIPanelFactory.CreateMainMenu(ct,
                () => StartSimulation(),
                () => ShowActivities(ct),
                () => ShowConnectivity(ct),
                () => ShowInstructions(ct),
                () => ShowDiscLegend(ct),
                () => ExitApplication());
        }

        /// <summary>
        /// Elimina cualquier panel de actividades, instrucciones o conectividad
        /// que haya quedado abierto al volver al menu principal.
        /// Conserva MenuNavigator para que el nuevo menu tenga animaciones.
        /// </summary>
        private void DestroyPreviousPanels()
        {
            string[] panelNames = { "ActivitiesPanel", "InstructionsPanel", "ConnectivityPanel", "MainMenuPanel",
                "TopologyInfoPanel", "BuildTopologyInfoPanel", "DiscLegendPanel", "FindFaultPanel", "ScenariosPanel",
                "BestRoutePanel", "RoutingTablesPanel", "StaticRoutingPanel", "DynamicRoutingPanel" };
            foreach (string name in panelNames)
            {
                GameObject panel = GameObject.Find(name);
                if (panel != null)
                {
                    GameObject.Destroy(panel);
                    Debug.Log($"[SceneSetup] Panel '{name}' destruido");
                }
            }

            // Destruir MainMenuManager viejo (DestroyImmediate para evitar conflictos
            // de singleton cuando se recrea el menu en el mismo frame)
            var mm = UnityEngine.Object.FindAnyObjectByType<MainMenuManager>();
            if (mm != null) GameObject.DestroyImmediate(mm.gameObject);

            // Limpiar MenuNavigator si existe para evitar referencias colgadas
            if (MenuNavigator.Instance != null)
                MenuNavigator.Instance.ClearPanel();

            // NO destruir MenuNavigator — se reutiliza para animaciones del nuevo menu
        }

        /// <summary>
        /// Punto de entrada publico para recrear el menu principal desde otros componentes.
        /// </summary>
        /// <param name="ct">Transform del Canvas donde se instancia el menu.</param>
        public void CreateMainMenuPublic(Transform ct)
        {
            CreateMainMenu(ct);
        }

        /// <summary>
        /// Inicia la simulacion: destruye el menu, configura managers,
        /// crea el visualizador, se suscribe a eventos y notifica al ActivityLoader.
        /// </summary>
        private void StartSimulation()
        {
            DestroyMainMenu();
            SetupManagers();
            CreateVisualizer(canvasTransform);
            SubscribeToTopologyEvents();
            CacheReferences();
            if (activityLoader != null) activityLoader.StartSimulation(canvasTransform);
        }

        /// <summary>
        /// Muestra el panel de seleccion de actividades. Al elegir una,
        /// configura managers y delega en <see cref="ActivityLoader.SelectActivity"/>.
        /// </summary>
        /// <param name="ct">Transform del Canvas donde instanciar el panel.</param>
        private void ShowActivities(Transform ct)
        {
            DestroyMainMenu();
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

        /// <summary>
        /// Muestra el panel de prueba de conectividad. Inicializa managers
        /// y visualizador si aun no estan listos.
        /// </summary>
        /// <param name="ct">Transform del Canvas donde instanciar el panel.</param>
        private void ShowConnectivity(Transform ct)
        {
            DestroyMainMenu();
            SetupManagers();
            CreateVisualizer(canvasTransform);
            SubscribeToTopologyEvents();
            CacheReferences();
            if (activityLoader != null) activityLoader.ShowConnectivityPanel(ct);
        }

        /// <summary>
        /// Muestra el panel de instrucciones de uso del simulador.
        /// </summary>
        /// <param name="ct">Transform del Canvas donde instanciar el panel.</param>
        private void ShowInstructions(Transform ct)
        {
            DestroyMainMenu();
            UIPanelFactory.CreateInstructionsPanel(ct,
                () => { CreateMainMenu(ct); });
        }

        /// <summary>
        /// Muestra la leyenda de discos fisicos y virtuales con sus tipos y colores.
        /// </summary>
        /// <param name="ct">Transform del Canvas donde instanciar el panel.</param>
        private void ShowDiscLegend(Transform ct)
        {
            DestroyMainMenu();
            UIPanelFactory.CreateDiscLegendPanel(ct,
                () => { DestroyPreviousPanels(); CreateMainMenu(ct); });
        }

        /// <summary>
        /// Cierra la aplicacion. En el Editor de Unity detiene el modo Play.
        /// </summary>
        public void ExitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// Selecciona y carga una actividad por su indice, delegando en <see cref="ActivityLoader.SelectActivity"/>.
        /// </summary>
        /// <param name="index">Indice de la actividad (0-based) a cargar.</param>
        public void SelectActivity(int index)
        {
            if (activityLoader != null)
                activityLoader.SelectActivity(index);
        }

        // ==================== PUBLIC FORWARDS ====================

        /// <summary>
        /// Activa o desactiva el modo de conexion manual de enlaces.
        /// </summary>
        /// <param name="mode">"CONNECT" para crear enlaces, "DISCONNECT" para eliminarlos.</param>
        // LinkMode
        public void ToggleLinkMode(string mode)
        {
            var lm = UnityEngine.Object.FindAnyObjectByType<LinkModeController>();
            if (lm != null) lm.ToggleLinkMode(mode);
        }

        /// <summary>
        /// Indica si el modo de conexion de enlaces esta activo.
        /// </summary>
        public bool IsLinkModeActive()
        {
            var lm = UnityEngine.Object.FindAnyObjectByType<LinkModeController>();
            return lm != null && lm.IsLinkModeActive();
        }

        /// <summary>
        /// Indica si el modo de ping esta activo.
        /// </summary>
        public bool IsPingModeActive()
        {
            var pm = UnityEngine.Object.FindAnyObjectByType<PingModeController>();
            return pm != null && pm.IsPingModeActive();
        }

        /// <summary>
        /// Indica si el panel de configuracion IP esta abierto actualmente.
        /// </summary>
        public bool IsIPConfigPanelOpen()
        {
            var ip = UnityEngine.Object.FindAnyObjectByType<IPConfigController>();
            return ip != null && ip.IsIPConfigPanelOpen();
        }

        /// <summary>
        /// Devuelve el DiscId del nodo cuyo panel IP esta abierto, o -1 si no hay ninguno.
        /// </summary>
        public int GetCurrentIPConfigNodeDiscId()
        {
            var ip = UnityEngine.Object.FindAnyObjectByType<IPConfigController>();
            return ip != null ? ip.GetCurrentIPConfigNodeDiscId() : -1;
        }

        /// <summary>
        /// Cierra el panel de configuracion IP si esta abierto.
        /// </summary>
        public void CloseIPConfigPanelPublic()
        {
            var ip = UnityEngine.Object.FindAnyObjectByType<IPConfigController>();
            if (ip != null) ip.CloseIPConfigPanelPublic();
        }

        /// <summary>
        /// Maneja el clic sobre un nodo, delegando en <see cref="NodeInteractionController.HandleNodeClick"/>.
        /// </summary>
        /// <param name="discId">Identificador unico del disco/nodo clickeado.</param>
        public void HandleNodeClick(int discId)
        {
            if (nodeInteraction != null) nodeInteraction.HandleNodeClick(discId);
        }

        /// <summary>
        /// Muestra el panel de configuracion IP para un nodo especifico.
        /// </summary>
        /// <param name="node">Nodo de red a configurar.</param>
        /// <param name="discId">Identificador del disco asociado al nodo.</param>
        public void ShowIPConfigPanel(NetworkNode node, int discId)
        {
            if (nodeInteraction != null) nodeInteraction.ShowIPConfigPanel(node, discId);
        }

        /// <summary>
        /// Elimina el nodo actualmente seleccionado en el panel de dispositivos.
        /// </summary>
        // DevicePanel
        public void RemoveSelectedNodePublic()
        {
            if (devicePanel != null) devicePanel.RemoveSelectedNodePublic();
        }

        /// <summary>
        /// Limpia la seleccion actual de nodo en el panel de dispositivos.
        /// </summary>
        public void ClearSelectedNode()
        {
            if (devicePanel != null) devicePanel.ClearSelectedNode();
        }

        /// <summary>
        /// Refresca la lista de dispositivos mostrada en el panel de dispositivos.
        /// </summary>
        public void RefreshDevicesPanel()
        {
            if (devicePanel != null) devicePanel.RefreshDevicesPanel();
        }

        /// <summary>
        /// Actualiza la visualizacion del puntaje en el panel de dispositivos.
        /// </summary>
        // Score
        public void UpdateScoreDisplay()
        {
            if (devicePanel != null) devicePanel.UpdateScoreDisplay();
        }
    }
}
