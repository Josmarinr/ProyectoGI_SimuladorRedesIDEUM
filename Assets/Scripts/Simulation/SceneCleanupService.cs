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
                // Paneles de config/actividad que faltaban en la limpieza (B5)
                "VLANPanel", "ACLPanel", "NATPanel", "ARPPanel", "RoutingPanel",
                "AddRoutePanel", "PingSelectionPanel", "TopologyExamplePanel" };
            // "PingPackets" NO va en esta lista a proposito: el bucle de managers
            // de arriba destruye PingVisualizer (y su GameObject GameManager), y su
            // OnDestroy es quien destruye el contenedor y sus Sprite/texturas.
            // Destruirlo aqui ademas crearia una carrera en el mismo frame: si el
            // contenedor se destruye primero, OnDestroy hace early-out por
            // packetContainer == null y los paquetes en vuelo dejarian texturas
            // huerfanas (B2).
            foreach (var name in panelNames)
            {
                var obj = GameObject.Find(name);
                if (obj != null)
                {
                    UIComponents.SafeDestroyPanelSprites(obj);
                    Object.Destroy(obj);
                }
            }

            // Destruir TODOS los fondos ClickOutsideBG_* por prefijo. Los nombres
            // reales son ClickOutsideBG_VLANPanel / _ACLPanel / _NATPanel / etc.;
            // buscar el nombre exacto "ClickOutsideBG" no encontraba ninguno (B5).
            // Cada fondo tiene SU PROPIO componente Canvas (CreateClickOutsideToClose),
            // asi que partir de un unico Canvas podia devolver el de un fondo: la
            // barrida solo veia ese subarbol y los demas fondos sobrevivian con
            // raycastTarget=true y sortingOrder=50, tragandose los clics (M1).
            // Se barren TODOS los RectTransform de la escena, incluidos inactivos.
            var backdrops = Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var rt in backdrops)
            {
                if (rt == null || !rt.name.StartsWith("ClickOutsideBG")) continue;
                UIComponents.SafeDestroyPanelSprites(rt.gameObject);
                Object.Destroy(rt.gameObject);
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

            // B4: barrida de seguridad. Libera sprites/texturas huerfanos de paneles
            // destruidos por rutas que no tienen teardown explicito de Sprite.
            // Se espera al fin del frame para que los Destroy pendientes se apliquen.
            StartCoroutine(UnloadUnusedAssetsAfterFrame());

            createMainMenuCallback?.Invoke();
        }

        /// <summary>
        /// Espera al final del frame (cuando Unity aplica los Destroy pendientes)
        /// y descarga los assets que ya nadie referencia.
        /// </summary>
        private System.Collections.IEnumerator UnloadUnusedAssetsAfterFrame()
        {
            yield return new UnityEngine.WaitForEndOfFrame();
            Resources.UnloadUnusedAssets();
        }
    }
}
