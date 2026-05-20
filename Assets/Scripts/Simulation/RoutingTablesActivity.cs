using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimRedes.Network;

namespace SimRedes.Simulation
{
    public class RoutingTablesActivity : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private GameObject tablePanel;
        [SerializeField] public Text infoText;
        [SerializeField] public Text tableText;

        private TopologyManager topologyManager;
        private RoutingSimulator routingSimulator;
        private List<NetworkNode> routers = new List<NetworkNode>();

        private void Start()
        {
            topologyManager = FindObjectOfType<TopologyManager>();
            if (topologyManager == null)
            {
                var go = new GameObject("TopologyManager");
                topologyManager = go.AddComponent<TopologyManager>();
            }

            routingSimulator = new RoutingSimulator();
            UpdateUI();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RefreshRoutingTables();
            }
        }

        public void RefreshRoutingTables()
        {
            routers.Clear();
            routingSimulator.ClearAllTables();

            var allNodes = topologyManager.GetAllNodes();
            foreach (var node in allNodes)
            {
                if (node.Type == SimRedes.Network.DeviceType.Router)
                {
                    routers.Add(node);
                    routingSimulator.InitializeRouterTable(node);
                    GenerateSampleRoutes(node);
                }
            }

            UpdateTableDisplay();
            UnityEngine.Debug.Log("[RoutingTables] Tablas actualizadas. Routers: " + routers.Count);
        }

        private void GenerateSampleRoutes(NetworkNode router)
        {
            string routerIP = $"192.168.{router.DiscId}.1";
            routingSimulator.AddStaticRoute(router.DiscId, "0.0.0.0", "0.0.0.0", "192.168.1.254", "G0/0");
            routingSimulator.AddStaticRoute(router.DiscId, "10.0.0.0", "255.0.0.0", "10.1.1.1", "G0/1");
            routingSimulator.AddStaticRoute(router.DiscId, "172.16.0.0", "255.240.0.0", "172.16.0.1", "G0/2");
        }

        private void UpdateUI()
        {
            if (infoText != null)
            {
                infoText.text = "TABLAS DE ENRUTAMIENTO\n\n" +
                    "Esta actividad muestra las tablas de\n" +
                    "enrutamiento de los routers en la\n" +
                    "topología actual.\n\n" +
                    "PRESIONA 'R' para actualizar las\n" +
                    "tablas de todos los routers.\n\n" +
                    "Coloca routers con la tecla 1 para\n" +
                    "ver sus tablas de enrutamiento.";
            }

            UpdateTableDisplay();
        }

        private void UpdateTableDisplay()
        {
            if (tableText == null) return;

            if (routers.Count == 0)
            {
                tableText.text = "No hay routers en la topología.\n\nColoca routers usando la tecla 1.";
                return;
            }

            string content = "=== TABLAS DE ENRUTAMIENTO ===\n\n";

            foreach (var router in routers)
            {
                content += $"Router {router.DiscId} ({router.Name}):\n";
                content += "----------------------------------------\n";
                content += "Red Destino    | Máscara       | Siguiente Salto\n";
                content += "----------------------------------------\n";

                content += "0.0.0.0        | 0.0.0.0       | 192.168.1.254  (Default)\n";
                content += "10.0.0.0       | 255.0.0.0     | 10.1.1.1       (Static)\n";
                content += "172.16.0.0     | 255.240.0.0   | 172.16.0.1     (Static)\n";
                content += "192.168." + router.DiscId + ".0   | 255.255.255.0 | Directa         (Connected)\n";
                content += "----------------------------------------\n\n";
            }

            content += "\nC = Limpiar | R = Actualizar | ESC = Menú";

            tableText.text = content;
        }

        public List<NetworkNode> GetRouters()
        {
            return routers;
        }

        public string GetRouterTableSummary(int routerDiscId)
        {
            return routingSimulator.GetRoutingTableSummary(routerDiscId);
        }
    }
}