using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using SimRedes.Tangible;
using SimRedes.Network;
using SimRedes.UI;
using UIComp = SimRedes.UI.UIComponents;

namespace SimRedes.Simulation
{
    /// <summary>
    /// Raiz de composicion de la escena: configura resolucion, camara, canvas, EventSystem,
    /// instancia todos los gestores globales en el contenedor GameManager, crea el
    /// NodeVisualizer y cachea/suscribe las referencias compartidas de la escena.
    /// Extraido de <see cref="SceneSetup"/> (tarea C5) sin cambios de comportamiento;
    /// el estado compartido vive en la fachada dueña (<see cref="SceneSetup"/>).
    /// </summary>
    public class SceneBootstrap
    {
        private readonly SceneSetup owner;
        private Font font;
        private Font bigFont;

        /// <summary>
        /// Crea un composition root asociado a la fachada <see cref="SceneSetup"/> dueña,
        /// cuyos campos comparten el canvas, los gestores y las referencias.
        /// </summary>
        /// <param name="owner">Fachada <see cref="SceneSetup"/> que usa como contexto.</param>
        public SceneBootstrap(SceneSetup owner)
        {
            this.owner = owner;
        }

        // ==================== SETUP DE ESCENA ====================

        /// <summary>
        /// Configura resolucion, camara, canvas y EventSystem, y cachea el transform
        /// del canvas y las fuentes. Usado por SceneSetup.Awake() y SceneSetup.SetupScene().
        /// </summary>
        public void SetupSceneCore()
        {
            SetupResolution();
            SetupCamera();
            owner.canvas = SetupCanvas();
            SetupEventSystem();
            owner.canvasTransform = owner.canvas.transform;
            font = UIComp.GetFont();
            bigFont = UIComp.GetFont(16);
        }

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
        /// <see cref="SceneSetup.SetupScene"/>
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
        /// TouchScript deshabilitado en Editor via TouchScriptDisabler para evitar dobles clicks.
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
        /// Verifica y re-aplica la configuracion ortogonal de la camara principal.
        /// Ejecutado en Start() como medida de seguridad ante sobrescritura de URP.
        /// </summary>
        public void EnsureCameraSetup()
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

        // ==================== MANAGERS ====================

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
            // Servicio de limpieza: creacion unica via GetOrCreate()
            // (tambien lo usa el menu al montar la escena)
            SceneCleanupService.GetOrCreate();
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

        // ==================== REFERENCIAS Y EVENTOS ====================

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
                owner.visualizer = vis;
            }
            else
            {
                owner.visualizer = UnityEngine.Object.FindAnyObjectByType<NodeVisualizer>();
            }
        }

        /// <summary>
        /// Busca y cachea referencias a los managers principales (TopologyManager,
        /// NodeVisualizer, NodeInteractionController, DevicePanelController, ActivityLoader).
        /// </summary>
        public void CacheReferences()
        {
            owner.topology = UnityEngine.Object.FindAnyObjectByType<TopologyManager>();
            owner.visualizer = UnityEngine.Object.FindAnyObjectByType<NodeVisualizer>();
            owner.canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (owner.canvas != null) owner.canvasTransform = owner.canvas.transform;
            owner.nodeInteraction = UnityEngine.Object.FindAnyObjectByType<NodeInteractionController>();
            owner.devicePanel = UnityEngine.Object.FindAnyObjectByType<DevicePanelController>();
            owner.activityLoader = UnityEngine.Object.FindAnyObjectByType<ActivityLoader>();
        }

        /// <summary>
        /// Se suscribe al evento OnTopologyChanged del TopologyManager
        /// para refrescar automaticamente el panel de dispositivos.
        /// </summary>
        public void SubscribeToTopologyEvents()
        {
            if (owner.topology != null)
            {
                owner.topology.OnTopologyChanged -= OnTopologyChangedCallback;
            }
            owner.topology = UnityEngine.Object.FindAnyObjectByType<TopologyManager>();
            owner.visualizer = UnityEngine.Object.FindAnyObjectByType<NodeVisualizer>();
            if (owner.topology != null)
            {
                owner.topology.OnTopologyChanged += OnTopologyChangedCallback;
            }
        }

        /// <summary>
        /// Cancela la suscripcion a OnTopologyChanged (llamado desde SceneSetup.OnDestroy).
        /// </summary>
        public void UnsubscribeFromTopologyEvents()
        {
            if (owner.topology != null)
            {
                owner.topology.OnTopologyChanged -= OnTopologyChangedCallback;
            }
        }

        private void OnTopologyChangedCallback()
        {
            // B2: no reconstruir el panel en forma sincrona por cada evento.
            // Mover un disco dispara OnTopologyChanged a decenas de Hz; aqui solo
            // se marca el refresco pendiente y DevicePanelController lo consume
            // acotado en el tiempo (solo si cambio la composicion de nodos).
            if (owner.devicePanel != null) owner.devicePanel.RequestRefresh();
        }
    }
}
