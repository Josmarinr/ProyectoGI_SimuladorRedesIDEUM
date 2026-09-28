using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using SimRedes.UI;
using SimRedes.Network;

namespace SimRedes.Simulation
{
    /// <summary>
    /// Controles globales de la simulacion durante el play: ESC (volver/cerrar),
    /// P (ping, unico dueno de la tecla) y R (eliminar nodo seleccionado,
    /// cediendo el tecla al panel de tablas de enrutamiento cuando esta abierto).
    /// </summary>
    public class SimulationControls : MonoBehaviour
    {
        private SceneSetup sceneSetup;
        private TopologyManager topology;
        private IPConfigController ipConfig;
        private DevicePanelController devicePanel;

        private void Start()
        {
            sceneSetup = Object.FindAnyObjectByType<SceneSetup>();
            topology = Object.FindAnyObjectByType<TopologyManager>();
            ipConfig = Object.FindAnyObjectByType<IPConfigController>();
            devicePanel = Object.FindAnyObjectByType<DevicePanelController>();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                var infoPanel = GameObject.Find("ScenarioInfoPanel");
                if (infoPanel != null)
                {
                    // B4: liberar los Sprite del panel antes de destruirlo
                    UIComponents.SafeDestroyPanelSprites(infoPanel);
                    Destroy(infoPanel);
                    return;
                }

                if (ipConfig != null && ipConfig.IsIPConfigPanelOpen())
                {
                    ipConfig.CloseIPConfigPanel();
                    return;
                }

                GoBackToMainMenu();
            }
            else if (keyboard != null && keyboard.pKey.wasPressedThisFrame)
            {
                // C4b: P = Ping. DUENO UNICO de la tecla P (DebugDiscSimulator ya
                // no la maneja); ver AGENTS.md "Comandos Debug (Keyboard)".
                ExecutePing();
            }
            else if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            {
                // C4b: R = Eliminar es el comportamiento global documentado.
                // La prioridad R = refrescar tablas queda scopada al panel de
                // tablas de enrutamiento: solo existe mientras ese panel esta
                // visible (ahi SimulationControls cede el tecla).
                if (ShouldRemoveSelectedNodeOnR())
                    RemoveSelectedNode();
            }
        }

        private void RemoveSelectedNode()
        {
            if (devicePanel != null)
            {
                devicePanel.RemoveSelectedNode();
            }
        }

        /// <summary>
        /// Decide si la tecla R debe eliminar el nodo seleccionado. La tabla de
        /// controles documenta R = "Eliminar" como comportamiento global; la
        /// actividad de tablas de enrutamiento tiene prioridad unicamente
        /// mientras su panel esta visible (alli R = refrescar tablas).
        /// La busqueda de la actividad ocurre solo al presionar R, no en cada frame.
        /// </summary>
        /// <returns>true si R debe eliminar el nodo seleccionado.</returns>
        public static bool ShouldRemoveSelectedNodeOnR()
        {
            var routingTables = Object.FindAnyObjectByType<RoutingTablesActivity>();
            return routingTables == null || !routingTables.IsRefreshPanelVisible;
        }

        private void ExecutePing()
        {
            if (topology == null) return;

            var nodes = topology.GetAllNodes();
            if (nodes.Count < 2)
            {
                UnityEngine.Debug.Log("[Ping] Necesitas al menos 2 nodos");
                return;
            }

            int sourceDiscId = nodes[0].DiscId;
            int destDiscId = nodes[1].DiscId;

            var pingVis = Object.FindAnyObjectByType<PingVisualizer>();
            if (pingVis != null)
            {
                pingVis.AnimatePing(sourceDiscId, destDiscId, (success) => {
                    UnityEngine.Debug.Log($"[Ping] {nodes[0].Name} -> {nodes[1].Name}: {(success ? "OK" : "FALLO")}");
                });
            }
            else
            {
                bool connected = topology.CheckConnectivity(sourceDiscId, destDiscId);
                UnityEngine.Debug.Log($"[Ping] {nodes[0].Name} -> {nodes[1].Name}: {(connected ? "OK" : "FALLO")}");
            }
        }

        /// <summary>
        /// Indica si el barrido destructivo de vuelta al menu debe saltarse.
        /// Sin Canvas (o sin SceneSetup) no hay donde recrear el menu: ejecutar
        /// el barrido dejaria la escena sin menu (C4c). Con ambos elementos
        /// existe la ruta normal, identica a la version anterior.
        /// </summary>
        /// <param name="canvas">Canvas de la escena (puede ser null).</param>
        /// <param name="sceneSetup">SceneSetup que recrea el menu (puede ser null).</param>
        /// <returns>true si el barrido destructivo debe omitirse.</returns>
        public static bool ShouldSkipMainMenuSweep(Canvas canvas, SceneSetup sceneSetup)
        {
            return canvas == null || sceneSetup == null;
        }

        /// <summary>
        /// Vuelve al menu principal delegando en la ruta unica de limpieza
        /// <see cref="SceneCleanupService.GoBackToMainMenu"/>. Si no hay Canvas
        /// o SceneSetup con los que recrear el menu, se omite el barrido
        /// destructivo para no dejar una escena sin menu (C4c).
        /// </summary>
        public void GoBackToMainMenu()
        {
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (ShouldSkipMainMenuSweep(canvas, sceneSetup))
            {
                UnityEngine.Debug.LogWarning("[SimulationControls] Vuelta al menu omitida: sin Canvas o SceneSetup no se puede recrear el menu");
                return;
            }

            var cleanup = SceneCleanupService.GetOrCreate();
            cleanup.GoBackToMainMenu(canvas.transform, () => sceneSetup.CreateMainMenu(canvas.transform));

            UnityEngine.Debug.Log("[SimulationControls] Volviendo al menu principal");
        }
    }
}
