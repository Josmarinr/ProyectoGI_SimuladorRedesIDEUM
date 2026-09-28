using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using SimRedes.UI;
using SimRedes.Network;
using SimRedes.Tangible;

namespace SimRedes.Simulation
{
    public class SimulationControls : MonoBehaviour
    {
        private SceneSetup sceneSetup;
        private TopologyManager topology;
        private DebugDiscSimulator discSim;
        private IPConfigController ipConfig;
        private DevicePanelController devicePanel;

        private void Start()
        {
            sceneSetup = Object.FindAnyObjectByType<SceneSetup>();
            topology = Object.FindAnyObjectByType<TopologyManager>();
            discSim = Object.FindAnyObjectByType<DebugDiscSimulator>();
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

        public void GoBackToMainMenu()
        {
            var cleanup = Object.FindAnyObjectByType<SceneCleanupService>();
            if (cleanup != null)
            {
                var canvas = Object.FindAnyObjectByType<Canvas>();
                if (canvas != null)
                {
                    cleanup.GoBackToMainMenu(canvas.transform, () => {
                        if (sceneSetup != null) sceneSetup.CreateMainMenu(canvas.transform);
                    });
                }
            }
            else
            {
                if (discSim != null) discSim.enabled = false;

                var visualizer = Object.FindAnyObjectByType<NodeVisualizer>();
                if (visualizer != null) Destroy(visualizer.gameObject);

                var topologyPanel = GameObject.Find("TopologyInfoPanel");
                if (topologyPanel != null) Destroy(topologyPanel);

                var devicesPanel = GameObject.Find("DevicesPanel");
                if (devicesPanel != null) Destroy(devicesPanel);

                var ipConfigPanel = GameObject.Find("IPConfigPanel");
                if (ipConfigPanel != null) Destroy(ipConfigPanel);

                var ipConfigBg = GameObject.Find("IPConfigBackground");
                if (ipConfigBg != null) Destroy(ipConfigBg);

                var statusPanel = GameObject.Find("StatusPanel");
                if (statusPanel != null) Destroy(statusPanel);

                var canvas = Object.FindAnyObjectByType<Canvas>();
                if (canvas != null && sceneSetup != null)
                    sceneSetup.CreateMainMenu(canvas.transform);
            }

            UnityEngine.Debug.Log("[SimulationControls] Volviendo al menu principal");
        }
    }
}
