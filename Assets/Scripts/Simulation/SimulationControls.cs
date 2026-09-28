using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using SimRedes.UI;
using SimRedes.Network;

namespace SimRedes.Simulation
{
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
                ExecutePing();
            }
            else if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            {
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
        /// Vuelve al menu principal delegando en la ruta unica de limpieza
        /// <see cref="SceneCleanupService.GoBackToMainMenu"/>.
        /// </summary>
        public void GoBackToMainMenu()
        {
            var cleanup = SceneCleanupService.GetOrCreate();
            var canvas = Object.FindAnyObjectByType<Canvas>();
            cleanup.GoBackToMainMenu(canvas != null ? canvas.transform : null, () => {
                if (sceneSetup != null && canvas != null) sceneSetup.CreateMainMenu(canvas.transform);
            });

            UnityEngine.Debug.Log("[SimulationControls] Volviendo al menu principal");
        }
    }
}
