using UnityEngine;
using SimRedes.Network;
using SimRedes.Tangible;
using SimRedes.UI;

namespace SimRedes.Simulation
{
    /// <summary>
    /// Servicio de limpieza de escena. Destruye managers, paneles y resetea el canvas
    /// al volver al menu principal o al limpiar la simulacion.
    /// </summary>
    public class SceneCleanupService : MonoBehaviour
    {
        /// <summary>
        /// Instancia singleton del servicio de limpieza.
        /// </summary>
        public static SceneCleanupService Instance { get; private set; }

        /// <summary>
        /// Inicializa el singleton. Si ya existe otra instancia, la destruye.
        /// </summary>
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        /// <summary>
        /// Limpia la referencia al singleton al destruirse.
        /// </summary>
        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// Limpia la simulacion actual: discos, topologia y elementos visuales.
        /// Tambien libera el cache de texturas.
        /// </summary>
        /// <param name="topology">Gestor de topologia a limpiar.</param>
        /// <param name="visualizer">Visualizador de nodos y enlaces a destruir.</param>
        /// <param name="discSim">Simulador de discos a forzar limpieza.</param>
        public void ClearSimulation(TopologyManager topology, NodeVisualizer visualizer, DebugDiscSimulator discSim)
        {
            if (discSim != null) discSim.ForceClearAll();
            if (topology != null) topology.ClearTopology();
            if (visualizer != null)
            {
                if (visualizer.nodeContainer != null)
                    foreach (Transform child in visualizer.nodeContainer)
                        Object.Destroy(child.gameObject);
                if (visualizer.linkContainer != null)
                    foreach (Transform child in visualizer.linkContainer)
                        Object.Destroy(child.gameObject);
            }
            UIComponents.ClearTextureCache();
        }

        /// <summary>
        /// Limpia todos los managers, paneles y el canvas, y luego invoca el callback
        /// para reconstruir el menu principal. Asegura el CanvasScaler con resolucion 4096x2160.
        /// </summary>
        /// <param name="canvasTransform">Transform del canvas (no se usa directamente, se busca por Find).</param>
        /// <param name="createMainMenuCallback">Callback para recrear el menu principal.</param>
        public void GoBackToMainMenu(Transform canvasTransform, System.Action createMainMenuCallback)
        {
            var managers = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var m in managers)
            {
                if (m is TopologyManager || m is TangibleDiscManager ||
                    m is NodeVisualizer || m is PingVisualizer ||
                    m is SimulationControls || m is ConnectivityTestPanel ||
                    m is DebugDiscSimulator || m is ScoringSystem ||
                    m is BuildTopologyActivity || m is FindFaultActivity ||
                    m is RoutingTablesActivity || m is StaticRoutingActivity ||
                    m is DynamicRoutingActivity || m is DynamicRoutingProtocol ||
                    m is BestRouteActivity ||
                    m is LinkModeController || m is PingModeController ||
                    m is IPConfigController || m is DevicePanelController ||
                    m is NodeInteractionController || m is DiscEventHandler ||
                    m is TangibleBridge)
                    Object.Destroy(m.gameObject);
            }

            string[] panelNames = { "TopologyInfoPanel", "DevicesPanel", "ScorePanel", "ScenarioInfoPanel",
                "IPConfigPanel",                 "IPConfigBackground", "StatusPanel", "NetworkAdvancedPanel",
                "ConnectivityPanel", "BuildTopologyInfoPanel", "DiscLegendPanel", "FindFaultPanel", "ScenariosPanel",
                "BestRoutePanel", "RoutingTablesPanel", "StaticRoutingPanel",
                "DynamicRoutingPanel", "ActivitiesPanel", "MainMenuPanel", "InstructionsPanel",
                "ClickOutsideBG" };
            foreach (var name in panelNames)
            {
                var obj = GameObject.Find(name);
                if (obj != null) Object.Destroy(obj);
            }

            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
                if (scaler == null) scaler = canvas.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(4096, 2160);
                scaler.matchWidthOrHeight = 0.5f;
            }

            // Limpiar cache de texturas para liberar memoria de paneles destruidos
            UIComponents.ClearTextureCache();

            createMainMenuCallback?.Invoke();
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
    }
}
