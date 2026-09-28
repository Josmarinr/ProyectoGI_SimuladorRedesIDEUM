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

namespace SimRedes.Simulation
{
    /// <summary>
    /// Punto de entrada principal de la escena y fachada del simulador. Conserva la
    /// superficie publica historica y delega la implementacion en los componentes
    /// extraidos: <see cref="SceneBootstrap"/> (raiz de composicion: managers, canvas,
    /// camara, EventSystem) y <see cref="SceneNavigation"/> (menu y paneles).
    /// Se ejecuta en Awake() y orquesta la inicializacion completa del simulador.
    /// </summary>
    public class SceneSetup : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private bool autoSetup = true;
        [SerializeField] private bool showMainMenuFirst = true;

        // Estado compartido con SceneBootstrap y SceneNavigation (antes privado; interno
        // para que los componentes extraidos accedan a el — C5).
        internal TopologyManager topology;
        internal NodeVisualizer visualizer;
        internal NodeInteractionController nodeInteraction;
        internal DevicePanelController devicePanel;
        internal ActivityLoader activityLoader;
        internal Canvas canvas;
        internal Transform canvasTransform;
        private float scoreUpdateTimer = 0f;

        private SceneBootstrap bootstrap;
        private SceneNavigation navigation;

        /// <summary>Raiz de composicion de la escena (creada perezosamente).</summary>
        internal SceneBootstrap Bootstrap => bootstrap ?? (bootstrap = new SceneBootstrap(this));

        /// <summary>Navegacion de menu/paneles (creada perezosamente).</summary>
        internal SceneNavigation Navigation => navigation ?? (navigation = new SceneNavigation(this));

        // ==================== MONOBEHAVIOUR ====================

        /// <summary>
        /// Inicializa la escena: configura resolucion, camara, canvas y decide
        /// si mostrar el menu principal o arrancar la simulacion directamente.
        /// </summary>
        private void Awake()
        {
            Debug.Log("[SceneSetup] Awake() iniciando...");
            if (!autoSetup) return;
            Bootstrap.SetupSceneCore();
            if (showMainMenuFirst)
                Navigation.CreateMainMenu(canvasTransform);
            else
                Navigation.StartSimulation();
            Debug.Log("[SceneSetup] Awake() completado");
        }

        /// <summary>
        /// Re-aplica configuracion de camara (por si URP la sobrescribe),
        /// cachea referencias a managers y se suscribe a eventos de topologia.
        /// Deshabilita TouchScriptInputModule para evitar dobles clicks.
        /// </summary>
        private void Start()
        {
            Debug.Log("[SceneSetup] Start() - re-aplicando configuracion de camara por seguridad");
            // Re-aplicar configuracion de camara en Start() por si URP la sobrescribio
            Bootstrap.EnsureCameraSetup();
            Bootstrap.CacheReferences();
            Bootstrap.SubscribeToTopologyEvents();
            Debug.Log("[SceneSetup] Start() completado");
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
            Bootstrap.UnsubscribeFromTopologyEvents();
        }

        // ==================== SETUP (publico) ====================

        /// <summary>
        /// Instancia todos los managers globales en un unico GameObject "GameManager".
        /// Delega en <see cref="SceneBootstrap.SetupManagers"/>.
        /// </summary>
        public void SetupManagers()
        {
            Bootstrap.SetupManagers();
        }

        /// <summary>
        /// Crea u obtiene el NodeVisualizer y sus contenedores de nodos y enlaces
        /// bajo el transform del Canvas. Delega en <see cref="SceneBootstrap.CreateVisualizer"/>.
        /// </summary>
        /// <param name="ct">Transform del Canvas donde se insertara el visualizador.</param>
        public void CreateVisualizer(Transform ct)
        {
            Bootstrap.CreateVisualizer(ct);
        }

        /// <summary>
        /// Configura manualmente resolucion, camara y canvas.
        /// Util cuando se omite el autoSetup en favor de inicializacion diferida.
        /// Delega en <see cref="SceneBootstrap.SetupSceneCore"/>.
        /// </summary>
        public void SetupScene()
        {
            Bootstrap.SetupSceneCore();
        }

        /// <summary>
        /// Se suscribe al evento OnTopologyChanged del TopologyManager
        /// para refrescar automaticamente el panel de dispositivos.
        /// Delega en <see cref="SceneBootstrap.SubscribeToTopologyEvents"/>.
        /// </summary>
        public void SubscribeToTopologyEvents()
        {
            Bootstrap.SubscribeToTopologyEvents();
        }

        // ==================== MENU / NAVEGACION (publico) ====================

        /// <summary>
        /// Crea el menu principal limpiando paneles previos y delegando en
        /// <see cref="SceneNavigation.CreateMainMenu"/> con sus callbacks.
        /// Punto de entrada publico para recrear el menu desde otros componentes.
        /// </summary>
        /// <param name="ct">Transform del Canvas donde se instancia el menu.</param>
        public void CreateMainMenu(Transform ct)
        {
            Navigation.CreateMainMenu(ct);
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
            var lm = LinkModeController.Instance;
            if (lm != null) lm.ToggleLinkMode(mode);
        }

        /// <summary>
        /// Indica si el modo de conexion de enlaces esta activo.
        /// </summary>
        public bool IsLinkModeActive()
        {
            var lm = LinkModeController.Instance;
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
