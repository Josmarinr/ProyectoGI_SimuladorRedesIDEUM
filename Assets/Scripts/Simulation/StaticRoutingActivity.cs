using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimRedes.Network;

namespace SimRedes.Simulation
{
    public class StaticRoutingActivity : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] public Text infoText;
        [SerializeField] public Text routesText;
        [SerializeField] public Text feedbackText;

        private TopologyManager topologyManager;
        private RoutingSimulator routingSimulator;
        private NetworkNode selectedRouter;

        private string pendingDestNetwork = "";
        private string pendingMask = "";
        private string pendingNextHop = "";

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
            if (Input.GetKeyDown(KeyCode.A))
            {
                AddSampleRoute();
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                TestRouting();
            }
        }

        public void AddRoute(string destNetwork, string mask, string nextHop)
        {
            if (selectedRouter == null)
            {
                var routers = topologyManager.GetAllNodes().FindAll(n => n.Type == SimRedes.Network.DeviceType.Router);
                if (routers.Count > 0)
                {
                    selectedRouter = routers[0];
                    routingSimulator.InitializeRouterTable(selectedRouter);
                }
            }

            if (selectedRouter != null)
            {
                routingSimulator.AddStaticRoute(selectedRouter.DiscId, destNetwork, mask, nextHop, "G0/0");
                ShowFeedback($"Ruta añadida: {destNetwork}/{GetPrefixLength(mask)} -> {nextHop}");
                UpdateRoutesDisplay();
            }
        }

        private void AddSampleRoute()
        {
            if (selectedRouter == null)
            {
                var routers2 = topologyManager.GetAllNodes().FindAll(n => n.Type == SimRedes.Network.DeviceType.Router);
                if (routers2.Count > 0)
                {
                    selectedRouter = routers2[0];
                    routingSimulator.InitializeRouterTable(selectedRouter);
                }
            }

            if (selectedRouter != null)
            {
                string dest = $"10.{Random.Range(1, 255)}.0.0";
                string mask = "255.0.0.0";
                string nextHop = $"192.168.{selectedRouter.DiscId}.254";
                routingSimulator.AddStaticRoute(selectedRouter.DiscId, dest, mask, nextHop, "G0/0");
                ShowFeedback($"Ruta estática añadida: {dest}/8 -> {nextHop}");
                UpdateRoutesDisplay();
            }
            else
            {
                ShowFeedback("Añade un router primero (tecla 1)");
            }
        }

        private void TestRouting()
        {
            if (selectedRouter == null)
            {
                ShowFeedback("Selecciona un router primero");
                return;
            }

            string testIP = $"10.{Random.Range(1, 255)}.{Random.Range(1, 255)}.1";
            bool found = routingSimulator.SimulatePacketForwarding(selectedRouter.DiscId, testIP);

            if (found)
            {
                ShowFeedback($"Tráfico hacia {testIP} enrutado correctamente");
            }
            else
            {
                ShowFeedback($"No hay ruta hacia {testIP}");
            }
        }

        private void UpdateUI()
        {
            if (infoText != null)
            {
                infoText.text = "ENRUTAMIENTO ESTÁTICO\n\n" +
                    "Configura rutas estáticas en los\n" +
                    "routers para dirigir el tráfico.\n\n" +
                    "COMANDOS:\n" +
                    "  A = Añadir ruta de ejemplo\n" +
                    "  T = Test de enrutamiento\n" +
                    "  R = Ver tabla de rutas";
            }

            UpdateRoutesDisplay();
        }

        private void UpdateRoutesDisplay()
        {
            if (routesText == null) return;

            string content = "=== RUTAS ESTÁTICAS ===\n\n";

            var routers = topologyManager.GetAllNodes().FindAll(n => n.Type == SimRedes.Network.DeviceType.Router);
            if (routers.Count == 0)
            {
                content += "No hay routers configurados.\n" +
                           "Usa la tecla 1 para añadir routers.";
            }
            else
            {
                foreach (var router in routers)
                {
                    if (selectedRouter == null) selectedRouter = router;

                    content += $"Router {router.DiscId}:\n";
                    content += "-----------------------------\n";
                    content += "ip route 10.0.0.0 255.0.0.0 192.168." + router.DiscId + ".254\n";
                    content += "ip route 172.16.0.0 255.240.0.0 172.16.0.1\n";
                    content += "ip route 0.0.0.0 0.0.0.0 192.168.1.254\n";
                    content += "-----------------------------\n\n";
                }
            }

            content += "\nA = Añadir ruta | T = Test | C = Limpiar | ESC = Menú";

            routesText.text = content;
        }

        private void ShowFeedback(string message)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
                UnityEngine.Debug.Log("[StaticRouting] " + message);
            }
        }

        private string GetPrefixLength(string mask)
        {
            if (mask == "255.255.255.255") return "32";
            if (mask == "255.255.255.252") return "30";
            if (mask == "255.255.255.248") return "29";
            if (mask == "255.255.255.240") return "28";
            if (mask == "255.255.255.224") return "27";
            if (mask == "255.255.255.192") return "26";
            if (mask == "255.255.255.128") return "25";
            if (mask == "255.255.255.0") return "24";
            if (mask == "255.255.254.0") return "23";
            if (mask == "255.255.252.0") return "22";
            if (mask == "255.255.248.0") return "21";
            if (mask == "255.255.240.0") return "20";
            if (mask == "255.255.224.0") return "19";
            if (mask == "255.255.192.0") return "18";
            if (mask == "255.255.128.0") return "17";
            if (mask == "255.255.0.0") return "16";
            if (mask == "255.254.0.0") return "15";
            if (mask == "255.252.0.0") return "14";
            if (mask == "255.248.0.0") return "13";
            if (mask == "255.240.0.0") return "12";
            if (mask == "255.224.0.0") return "11";
            if (mask == "255.192.0.0") return "10";
            if (mask == "255.128.0.0") return "9";
            if (mask == "255.0.0.0") return "8";
            return "0";
        }
    }
}