using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
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
        private List<NetworkNode> routers = new List<NetworkNode>();

        private void Start()
        {
            topologyManager = Object.FindAnyObjectByType<TopologyManager>();
            if (topologyManager == null)
            {
                var go = new GameObject("TopologyManager");
                topologyManager = go.AddComponent<TopologyManager>();
            }

            UpdateUI();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            {
                RefreshRoutingTables();
            }
        }

        public void RefreshRoutingTables()
        {
            routers.Clear();

            var allNodes = topologyManager.GetAllNodes();
            foreach (var node in allNodes)
            {
                if (node.Type == SimRedes.Network.DeviceType.Router)
                {
                    routers.Add(node);
                    GenerateSampleRoutes(node);
                }
            }

            UpdateTableDisplay();
            UnityEngine.Debug.Log("[RoutingTables] Tablas actualizadas. Routers: " + routers.Count);
        }

        private void GenerateSampleRoutes(NetworkNode router)
        {
            router.RoutingTable.Clear();
            router.RoutingTable.AddStaticRoute("0.0.0.0", "0.0.0.0", "192.168.1.254", "G0/0");
            router.RoutingTable.AddStaticRoute("10.0.0.0", "255.0.0.0", "10.1.1.1", "G0/1");
            router.RoutingTable.AddStaticRoute("172.16.0.0", "255.240.0.0", "172.16.0.1", "G0/2");
        }

        private void UpdateUI()
        {
            if (infoText != null)
            {
                infoText.text = "TABLAS DE ENRUTAMIENTO\n\n" +
                    "Esta actividad muestra las tablas de\n" +
                    "enrutamiento de los routers en la\n" +
                    "topolog\u00eda actual.\n\n" +
                    "Presiona ACTUALIZAR para refrescar\n" +
                    "las tablas de todos los routers.\n\n" +
                    "Coloca routers (tecla 1 o discos)\n" +
                    "para ver sus tablas de enrutamiento.";
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
                content += "------------------------------------------------------------\n";
                content += "Red Destino/Mascara | Siguiente Salto | Interfaz | Métrica | Protocolo\n";
                content += "------------------------------------------------------------\n";

                var entries = router.RoutingTable.GetAllEntries();
                if (entries.Count == 0)
                {
                    content += "Sin rutas configuradas\n";
                }
                else
                {
                    foreach (var entry in entries)
                    {
                        content += $"{entry.DestinationNetwork}/{entry.GetPrefixLength(),-8} | {entry.NextHop,-14} | {entry.OutInterface,-7} | {entry.Metric,-7} | {entry.Protocol}\n";
                    }
                }

                content += "------------------------------------------------------------\n\n";
            }

            content += "\nACTUALIZAR = Refrescar tablas | ESC = Menú";

            tableText.text = content;
        }

        public List<NetworkNode> GetRouters()
        {
            return routers;
        }

        public string GetRouterTableSummary(int routerDiscId)
        {
            var router = topologyManager?.GetNode(routerDiscId);
            return RoutingSimulator.GetRoutingTableSummary(router);
        }
    }
}