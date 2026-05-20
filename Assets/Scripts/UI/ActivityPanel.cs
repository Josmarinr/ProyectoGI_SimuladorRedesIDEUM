using UnityEngine;
using UnityEngine.UI;
using SimRedes.Network;
using SimRedes.Simulation;

namespace SimRedes.UI
{
    public enum ActivityMode
    {
        BuildTopology,
        FindFault,
        RoutingTable,
        BestRoute,
        StaticRouting,
        DynamicProtocol
    }

    public class ActivityPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text statusText;
        [SerializeField] private Text topologySummaryText;
        [SerializeField] private Text routingTableText;
        [SerializeField] private Button resetButton;

        [Header("Activity Settings")]
        [SerializeField] private ActivityMode currentMode = ActivityMode.BuildTopology;

        private void Start()
        {
            if (resetButton != null)
                resetButton.onClick.AddListener(OnResetClicked);

            UpdateUI();
        }

        private void Update()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            var topology = FindObjectOfType<TopologyManager>();

            if (topologySummaryText != null && topology != null)
            {
                topologySummaryText.text = topology.GetTopologySummary();
            }

            if (routingTableText != null && currentMode == ActivityMode.RoutingTable)
            {
                ShowRoutingTable();
            }
        }

        private void ShowRoutingTable()
        {
            var gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null) return;

            var nodes = gameManager.GetTopologyManager().GetAllNodes();
            string tableContent = "=== Tabla de Enrutamiento ===\n";

            foreach (var node in nodes)
            {
                if (node.Type == SimRedes.Network.DeviceType.Router)
                {
                    var sim = gameManager.GetRoutingSimulator();
                    sim.InitializeRouterTable(node);
                    tableContent += sim.GetRoutingTableSummary(node.DiscId) + "\n";
                }
            }

            routingTableText.text = tableContent;
        }

        public void SetMode(ActivityMode mode)
        {
            currentMode = mode;
            if (statusText != null)
                statusText.text = $"Modo: {mode}";

            UnityEngine.Debug.Log($"[Activity] Modo cambiado a: {mode}");
        }

        private void OnResetClicked()
        {
            var gameManager = FindObjectOfType<GameManager>();
            gameManager?.ResetSimulation();
        }

        public void SimulatePing(int sourceDiscId, int destDiscId)
        {
            var topology = FindObjectOfType<TopologyManager>();
            if (topology == null) return;

            bool connected = topology.CheckConnectivity(sourceDiscId, destDiscId);

            if (statusText != null)
            {
                statusText.text = connected ? "Ping: EXITOSO" : "Ping: FALLIDO";
            }

            UnityEngine.Debug.Log($"[Activity] Ping {sourceDiscId} -> {destDiscId}: {(connected ? "OK" : "FAIL")}");
        }
    }
}