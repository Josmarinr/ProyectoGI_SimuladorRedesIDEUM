using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimRedes.Network;

namespace SimRedes.Simulation
{
    public class DynamicRoutingActivity : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] public Text infoText;
        [SerializeField] public Text tablesText;
        [SerializeField] public Text protocolText;
        [SerializeField] public Text feedbackText;

        private TopologyManager topologyManager;
        private RoutingSimulator routingSimulator;
        private RoutingProtocol currentProtocol = RoutingProtocol.RIP;
        private List<NetworkNode> routers = new List<NetworkNode>();
        private int simulationStep = 0;

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
                SetProtocol(RoutingProtocol.RIP);
            }
            else if (Input.GetKeyDown(KeyCode.O))
            {
                SetProtocol(RoutingProtocol.OSPF);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                SimulateAdvertisement();
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                SimulateConvergence();
            }
        }

        public void SetProtocol(RoutingProtocol protocol)
        {
            currentProtocol = protocol;
            routingSimulator.SetProtocol(protocol);
            ShowFeedback($"Protocolo cambiado a: {protocol}");
            UpdateProtocolDisplay();
            UnityEngine.Debug.Log($"[DynamicRouting] Protocolo: {protocol}");
        }

        private void SimulateAdvertisement()
        {
            routers.Clear();
            var allNodes = topologyManager.GetAllNodes();

            foreach (var node in allNodes)
            {
                if (node.Type == SimRedes.Network.DeviceType.Router)
                {
                    routers.Add(node);
                    routingSimulator.InitializeRouterTable(node);
                }
            }

            if (routers.Count < 2)
            {
                ShowFeedback("Se necesitan al menos 2 routers para advertisements");
                return;
            }

            simulationStep++;
            string content = $"=== {currentProtocol} SIMULATION ===\n\n";
            content += $"Paso {simulationStep}: Advertisement\n\n";

            if (currentProtocol == RoutingProtocol.RIP)
            {
                foreach (var router in routers)
                {
                    var advertisements = new List<(string network, int hops)>();
                    advertisements.Add(("192.168." + router.DiscId + ".0", 0));
                    advertisements.Add(("10." + router.DiscId + ".0.0", 0));
                    routingSimulator.SimulateRIPAdvertisement(router.DiscId, advertisements);
                    content += $"Router {router.DiscId} envía:\n";
                    content += $"  192.168.{router.DiscId}.0/24 (hops=1)\n";
                    content += $"  10.{router.DiscId}.0.0/8 (hops=1)\n\n";
                }
            }
            else if (currentProtocol == RoutingProtocol.OSPF)
            {
                foreach (var router in routers)
                {
                    var advertisements = new List<(string network, int cost)>();
                    advertisements.Add(("192.168." + router.DiscId + ".0", 10));
                    advertisements.Add(("10." + router.DiscId + ".0.0", 20));
                    routingSimulator.SimulateOSPFAdvertisement(router.DiscId, advertisements);
                    content += $"Router {router.DiscId} envía LSA:\n";
                    content += $"  192.168.{router.DiscId}.0/24 (cost=10)\n";
                    content += $"  10.{router.DiscId}.0.0/8 (cost=20)\n\n";
                }
            }

            content += "Presiona T para ver convergencia";
            tablesText.text = content;
        }

        private void SimulateConvergence()
        {
            if (routers.Count == 0)
            {
                ShowFeedback("Ejecuta advertisement primero (S)");
                return;
            }

            string content = $"=== CONVERGENCIA {currentProtocol} ===\n\n";
            content += "Tablas después de convergencia:\n\n";

            foreach (var router in routers)
            {
                content += $"--- Router {router.DiscId} ---\n";
                content += routingSimulator.GetRoutingTableSummary(router.DiscId);
                content += "\n";
            }

            content += "\nS = Nuevo advertisement | C = Limpiar | ESC = Menú";
            tablesText.text = content;
            ShowFeedback("Convergencia completada");
        }

        private void UpdateUI()
        {
            if (infoText != null)
            {
                infoText.text = "ENRUTAMIENTO DINÁMICO\n\n" +
                    "Simula protocolos de enrutamiento\n" +
                    "dinámico: RIP y OSPF.\n\n" +
                    "COMANDOS:\n" +
                    "  R = Protocolo RIP\n" +
                    "  O = Protocolo OSPF\n" +
                    "  S = Simular advertisement\n" +
                    "  T = Ver convergencia";
            }

            UpdateProtocolDisplay();
            UpdateTablesDisplay();
        }

        private void UpdateProtocolDisplay()
        {
            if (protocolText != null)
            {
                protocolText.text = $"Protocolo: {currentProtocol}";
            }
        }

        private void UpdateTablesDisplay()
        {
            if (tablesText == null) return;

            string content = "=== ENRUTAMIENTO DINÁMICO ===\n\n";
            content += "Protocolo actual: " + currentProtocol + "\n\n";

            var routers = topologyManager.GetAllNodes().FindAll(n => n.Type == SimRedes.Network.DeviceType.Router);
            if (routers.Count < 2)
            {
                content += "Necesitas al menos 2 routers.\n" +
                           "Usa la tecla 1 para añadir routers\n" +
                           "y 4 para conectar con enlaces.";
            }
            else
            {
                content += "Routers detectados: " + routers.Count + "\n\n";
                content += "Presiona S para comenzar la\n";
                content += "simulación de advertisements.\n\n";
                content += "Diferencias RIP vs OSPF:\n";
                content += "- RIP: usa conteo de hops\n";
                content += "- OSPF: usa costo de enlace\n";
            }

            tablesText.text = content;
        }

        private void ShowFeedback(string message)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
            }
            UnityEngine.Debug.Log("[DynamicRouting] " + message);
        }

        public RoutingProtocol GetCurrentProtocol()
        {
            return currentProtocol;
        }
    }
}