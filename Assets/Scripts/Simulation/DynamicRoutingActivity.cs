using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
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
        private RoutingProtocol currentProtocol = RoutingProtocol.RIP;
        private DynamicRoutingProtocol dynProtocol;

        // Configuracion virtual de discos 15-18
        [Header("Configuracion de Red Virtual (Discos 15-18)")]
        public string neighborRouter = "";       // Vecino (15)
        public string networkToAdvertise = "";   // AnunciarRed (16)
        public int? linkCost = null;              // Costo (17) — null = no configurado, usar calculo por BW
        public int bandwidth = 1000;             // BW (18) en Mbps

        private void Start()
        {
            topologyManager = ActivityStartup.ResolveTopologyManager();
            UpdateUI();
        }

        public void SetProtocol(RoutingProtocol protocol)
        {
            currentProtocol = protocol;
            ShowFeedback($"Protocolo cambiado a: {protocol}");
            UpdateProtocolDisplay();
            UnityEngine.Debug.Log($"[DynamicRouting] Protocolo: {protocol}");
        }

        public void StartProtocol(GameObject panel)
        {
            if (topologyManager == null) return;
            var routers = topologyManager.GetAllNodes().FindAll(n => n.Type == SimRedes.Network.DeviceType.Router);
            if (routers.Count < 2) { ShowFeedback("Se necesitan al menos 2 routers"); return; }

            if (dynProtocol != null) { dynProtocol.StopProtocol(); Destroy(dynProtocol); }

            dynProtocol = gameObject.AddComponent<DynamicRoutingProtocol>();
            if (currentProtocol == RoutingProtocol.RIP)
                dynProtocol.protocol = DynamicRoutingProtocol.ProtocolType.RIP;
            else if (currentProtocol == RoutingProtocol.EIGRP)
                dynProtocol.protocol = DynamicRoutingProtocol.ProtocolType.EIGRP;
            else
                dynProtocol.protocol = DynamicRoutingProtocol.ProtocolType.OSPF;

            // Aplicar configuracion virtual de discos 15-18 antes de iniciar
            ApplyDiscConfigToProtocol(dynProtocol);

            dynProtocol.StartProtocol();
            ShowFeedback($"Protocolo {currentProtocol} iniciado con {routers.Count} routers");

            var rightText = panel.transform.Find("RightPanel")?.GetComponent<UnityEngine.UI.Text>();
            dynProtocol.OnProtocolLog += (msg) => {
                if (rightText != null) rightText.text = $"[{currentProtocol}] {msg}";
                ShowFeedback(msg);
            };
            dynProtocol.OnConvergence += () => {
                if (rightText != null)
                {
                    rightText.text = $"[{currentProtocol}] CONVERGENCIA ALCANZADA\n\nTodas las rutas han sido intercambiadas.";
                    rightText.color = new Color(0.2f, 0.8f, 0.2f);
                }
                ShowFeedback("Convergencia alcanzada");
            };
        }

        public void StopProtocol(GameObject panel)
        {
            if (dynProtocol != null) dynProtocol.StopProtocol();
            ShowFeedback("Protocolo detenido");
        }

        public void ClearAllRoutes(GameObject panel)
        {
            if (dynProtocol != null) dynProtocol.ClearAllRoutes();
            if (topologyManager != null)
            {
                foreach (var r in topologyManager.GetAllNodes().FindAll(n => n.Type == SimRedes.Network.DeviceType.Router))
                    r.RoutingTable.Clear();
            }
            ShowFeedback("Rutas limpiadas");
        }

        public void ShowRoutes(GameObject panel)
        {
            var rightText = panel.transform.Find("RightPanel")?.GetComponent<UnityEngine.UI.Text>();
            if (rightText == null) return;
            var routers = topologyManager?.GetAllNodes().FindAll(n => n.Type == SimRedes.Network.DeviceType.Router) ?? new System.Collections.Generic.List<NetworkNode>();
            string info = "TABLAS DE RUTAS:\n\n";
            foreach (var r in routers)
            {
                var entries = r.RoutingTable.GetAllEntries();
                info += $"{r.Name}:\n";
                if (entries.Count == 0) info += "  Sin rutas aprendidas\n\n";
                else { foreach (var e in entries) info += $"  {RoutingTable.FormatRouteLine(e)}\n"; info += "\n"; }
            }
            rightText.text = info;
            rightText.fontSize = 11;
        }

        private void UpdateUI()
        {
            if (infoText != null)
            {
                infoText.text = "ENRUTAMIENTO DINÁMICO\n\n" +
                    "Simula protocolos de enrutamiento\n" +
                    "dinámico: RIP, OSPF y EIGRP.\n\n" +
                    "BOTONES:\n" +
                    "  RIP / OSPF / EIGRP = Elegir\n" +
                    "  START = Simular advertisement\n" +
                    "  STOP = Detener simulacion\n" +
                    "  VER RUTAS = Mostrar tablas";
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
            if (topologyManager == null) return;

            string content = "=== ENRUTAMIENTO DINÁMICO ===\n\n";
            content += "Protocolo actual: " + currentProtocol + "\n\n";

            var routers = topologyManager.GetAllNodes().FindAll(n => n.Type == SimRedes.Network.DeviceType.Router);
            if (routers.Count < 2)
            {
                content += "Necesitas al menos 2 routers.\n" +
                           "Usa tecla 1 o coloca discos\n" +
                           "para añadir routers.\n\n" +
                           "Luego conectalos con enlaces\n" +
                           "(tecla 4 o modo CONEXION).";
            }
            else
            {
                content += "Routers detectados: " + routers.Count + "\n\n";
                content += "Presiona START para comenzar\n";
                content += "la simulacion de advertisements.\n\n";
                content += "Diferencias RIP vs OSPF vs EIGRP:\n";
                content += "- RIP: conteo de hops\n";
                content += "- OSPF: costo de enlace\n";
                content += "- EIGRP: metrica compuesta (BW, Delay)\n";
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

        // ============================================================
        // Configuracion virtual de discos 15-18 (Vecino, AnunciarRed, Costo, BW)
        // Estos valores se configuran desde la UI de la actividad
        // y se aplican al protocolo de enrutamiento dinamico.
        // ============================================================

        public void SetNeighborRouter(string routerName)
        {
            neighborRouter = routerName;
            ShowFeedback($"Vecino configurado: {routerName}");
        }

        public void SetNetworkToAdvertise(string network)
        {
            networkToAdvertise = network;
            ShowFeedback($"Red a anunciar: {network}");
        }

        public void SetLinkCost(int cost)
        {
            linkCost = Mathf.Max(1, cost);
            ShowFeedback($"Costo de enlace OSPF: {linkCost}");
        }

        public void SetBandwidth(int bw)
        {
            bandwidth = Mathf.Max(1, bw);
            ShowFeedback($"Ancho de banda: {bandwidth} Mbps");
        }

        public void SetKValue(int kIndex, int value)
        {
            if (dynProtocol != null)
            {
                dynProtocol.SetKValue(kIndex, value);
                ShowFeedback($"K{kIndex} establecido a {value}");
            }
            else
            {
                ShowFeedback("Inicia el protocolo primero antes de configurar K values");
            }
        }

        /// <summary>
        /// Aplica la configuracion de discos 15-18 al protocolo antes de iniciar.
        /// Si se configuro un vecino manual, filtra los routers conectados.
        /// Si se configuro una red a anunciar, la agrega a las redes conocidas.
        /// </summary>
        public void ApplyDiscConfigToProtocol(DynamicRoutingProtocol protocol)
        {
            if (protocol == null) return;

            // Si hay un vecino configurado manualmente, establecerlo
            if (!string.IsNullOrEmpty(neighborRouter))
            {
                protocol.SetManualNeighbor(neighborRouter);
                UnityEngine.Debug.Log($"[DynamicRouting] Vecino manual: {neighborRouter}");
            }

            // Si hay una red a anunciar, establecerla
            if (!string.IsNullOrEmpty(networkToAdvertise))
            {
                protocol.SetManualNetwork(networkToAdvertise);
                UnityEngine.Debug.Log($"[DynamicRouting] Red manual a anunciar: {networkToAdvertise}");
            }

            // Costo OSPF personalizado (solo si el usuario lo configuro explicitamente)
            if (linkCost.HasValue)
            {
                protocol.SetCustomCost(linkCost.Value);
                UnityEngine.Debug.Log($"[DynamicRouting] Costo OSPF manual: {linkCost.Value}");
            }

            // BW se registra para calculo OSPF basado en ancho de banda
            protocol.SetCustomBandwidth(bandwidth);

            string costStr = linkCost.HasValue ? linkCost.Value.ToString() : "(calculado por BW)";
            ShowFeedback($"Configuracion aplicada: Vecino={neighborRouter}, Red={networkToAdvertise}, Costo={costStr}, BW={bandwidth}");
        }

        private void OnDestroy()
        {
            if (dynProtocol != null)
            {
                dynProtocol.StopProtocol();
            }
        }
    }
}