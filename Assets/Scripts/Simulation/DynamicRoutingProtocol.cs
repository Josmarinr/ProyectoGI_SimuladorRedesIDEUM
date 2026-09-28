using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using SimRedes.Network;
using SimRedes.UI;
using DeviceType = SimRedes.Network.DeviceType;

namespace SimRedes.Simulation
{
    public class DynamicRoutingProtocol : MonoBehaviour
    {
        public enum ProtocolType { RIP, OSPF, EIGRP }

        [Header("Configuracion")]
        public ProtocolType protocol = ProtocolType.RIP;
        public float advertisementInterval = 3f;
        public bool autoStart = false;

        private TopologyManager topology;
        private bool isRunning = false;
        private bool isConverged = false;
        private int advertisementCount = 0;
        private List<RouterAdvertState> routerStates = new List<RouterAdvertState>();

        // Configuracion de discos virtuales 15-18
        private string manualNeighbor = "";
        private string manualNetwork = "";
        private int? customCost = null; // null = calcular desde BW, valor = override manual
        private int customBandwidth = 1000;

        // K values para EIGRP (metric = (K1*BW + K3*Delay) * 256)
        private int k1 = 1, k2 = 0, k3 = 1, k4 = 0, k5 = 0;

        public event Action<string> OnProtocolLog;
        public event Action OnConvergence;

        private class RouterAdvertState
        {
            public NetworkNode Router;
            public List<string> KnownNetworks = new List<string>();
            public int ConvergenceVersion = 0;
        }

        private void Awake()
        {
            topology = UnityEngine.Object.FindAnyObjectByType<TopologyManager>();
        }

        private void Start()
        {
            if (autoStart)
            {
                StartProtocol();
            }
        }

        public void StartProtocol()
        {
            if (isRunning) return;

            // Null guard: topology puede ser null si no se encontro TopologyManager
            // (ej: StartProtocol llamado antes de Awake, o TopologyManager no existe)
            if (topology == null)
            {
                Log("Error: TopologyManager no encontrado. No se puede iniciar el protocolo.");
                return;
            }

            var allNodes = topology.GetAllNodes();
            var routers = new List<NetworkNode>();
            foreach (var n in allNodes)
            {
                if (n.Type == DeviceType.Router)
                    routers.Add(n);
            }

            if (routers.Count < 2)
            {
                Log($"Se necesitan al menos 2 routers para {protocol}");
                return;
            }

            isRunning = true;
            isConverged = false;
            advertisementCount = 0;

            InitializeRouterStates(routers);

            Log($"Iniciando protocolo {protocol}...");
            StartCoroutine(RunProtocolLoop(routers));
        }

        public void StopProtocol()
        {
            isRunning = false;
            StopAllCoroutines();
            Log($"Protocolo {protocol} detenido");
        }

        public bool IsRunning() => isRunning;
        public bool IsConverged() => isConverged;
        public int GetAdvertisementCount() => advertisementCount;

        private void InitializeRouterStates(List<NetworkNode> routers)
        {
            routerStates.Clear();
            foreach (var router in routers)
            {
                var state = new RouterAdvertState { Router = router };
                if (IPValidation.IsValidIP(router.IpAddress) && IPValidation.IsValidSubnetMask(router.SubnetMask))
                {
                    string network = IPValidation.GetNetworkAddress(router.IpAddress, router.SubnetMask);
                    state.KnownNetworks.Add(network);
                    Log($"{router.Name} anuncia red: {network}");
                }

                // Disco 16 - AnunciarRed: Agregar red manual si esta configurada
                if (!string.IsNullOrEmpty(manualNetwork) && !state.KnownNetworks.Contains(manualNetwork))
                {
                    state.KnownNetworks.Add(manualNetwork);
                    Log($"{router.Name} anuncia red manual: {manualNetwork}");
                }

                routerStates.Add(state);
            }
        }

        private IEnumerator RunProtocolLoop(List<NetworkNode> routers)
        {
            int maxIterations = 10;
            int iteration = 0;

            while (!isConverged && iteration < maxIterations)
            {
                yield return new WaitForSeconds(advertisementInterval);

                bool changed = false;

                foreach (var state in routerStates)
                {
                    var connectedRouters = GetConnectedRouters(state.Router);
                    foreach (var connected in connectedRouters)
                    {
                        var connState = routerStates.Find(s => s.Router == connected);
                        if (connState == null) continue;

                        foreach (var network in state.KnownNetworks)
                        {
                            if (!connState.KnownNetworks.Contains(network))
                            {
                                connState.KnownNetworks.Add(network);
                                changed = true;

                                int metric;
                                if (protocol == ProtocolType.RIP)
                                    metric = state.KnownNetworks.Count;
                                else if (protocol == ProtocolType.EIGRP)
                                    metric = CalculateEIGRPMetric(state.Router, connected);
                                else
                                    metric = CalculateOSPFCost(state.Router, connected);

                                if (protocol == ProtocolType.RIP)
                                {
                                    connState.Router.RoutingTable.AddRipRoute(network, GetNextHop(state.Router, connected), GetInterface(state.Router, connected), state.KnownNetworks.Count);
                                }
                                else if (protocol == ProtocolType.EIGRP)
                                {
                                    connState.Router.RoutingTable.AddEigrpRoute(network, GetNextHop(state.Router, connected), GetInterface(state.Router, connected), metric);
                                }
                                else
                                {
                                    connState.Router.RoutingTable.AddOspfRoute(network, GetNextHop(state.Router, connected), GetInterface(state.Router, connected), metric);
                                }

                                Log($"{state.Router.Name} -> {connState.Router.Name}: {network}");
                            }
                        }
                    }
                }

                advertisementCount++;

                if (!changed)
                {
                    isConverged = true;
                    Log($"Convergencia alcanzada en {advertisementCount} anuncios");
                    OnConvergence?.Invoke();
                }

                iteration++;
            }

            if (!isConverged)
            {
                Log($"Max iterations ({maxIterations}) reached - possible loop");
            }
        }

        private List<NetworkNode> GetConnectedRouters(NetworkNode router)
        {
            var connected = new List<NetworkNode>();
            var links = topology.GetAllLinks();

            foreach (var link in links)
            {
                if (!link.IsFunctional()) continue;

                NetworkNode neighbor = null;
                if (link.SourceNode == router && link.DestinationNode.Type == DeviceType.Router)
                    neighbor = link.DestinationNode;
                else if (link.DestinationNode == router && link.SourceNode.Type == DeviceType.Router)
                    neighbor = link.SourceNode;

                if (neighbor == null) continue;

                // Disco 15 - Vecino: Si hay un vecino manual configurado, solo conectar a ese
                if (!string.IsNullOrEmpty(manualNeighbor))
                {
                    if (neighbor.Name == manualNeighbor || neighbor.DiscId.ToString() == manualNeighbor)
                        connected.Add(neighbor);
                }
                else
                {
                    connected.Add(neighbor);
                }
            }

            return connected;
        }

        private string GetNextHop(NetworkNode from, NetworkNode to)
        {
            if (IPValidation.IsValidIP(to.IpAddress))
                return to.IpAddress;

            var link = topology.FindLink(from, to);
            if (link != null)
            {
                if (IPValidation.IsValidIP(link.SourceNode == from ? link.DestinationNode.IpAddress : link.SourceNode.IpAddress))
                {
                    return link.SourceNode == from ? link.DestinationNode.IpAddress : link.SourceNode.IpAddress;
                }
            }

            return "0.0.0.0";
        }

        private string GetInterface(NetworkNode from, NetworkNode to)
        {
            var links = topology.GetAllLinks();
            foreach (var link in links)
            {
                if (link.SourceNode == from && link.DestinationNode == to)
                    return link.SourceInterface;
                if (link.SourceNode == to && link.DestinationNode == from)
                    return link.DestinationInterface;
            }
            return "G0/0";
        }

        private int CalculateOSPFCost(NetworkNode from, NetworkNode to)
        {
            // Disco 18 - BW: Calcular costo segun ancho de banda (formula OSPF estandar)
            // cost = referenceBandwidth / interfaceBandwidth (reference=100 Mbps)
            float referenceBW = 100000f; // 100 Mbps en Kbps
            float interfaceBW = customBandwidth * 1000f; // Mbps a Kbps
            int bwCost = Mathf.Max(1, Mathf.RoundToInt(referenceBW / interfaceBW));

            // Disco 17 - Costo: Si el usuario configuro un costo manual, usarlo (nullable)
            if (customCost.HasValue)
            {
                Log($"Usando costo personalizado {customCost.Value} (BW={customBandwidth} Mbps -> costo calculado={bwCost})");
                return customCost.Value;
            }

            return bwCost;
        }

        private int CalculateEIGRPMetric(NetworkNode from, NetworkNode to)
        {
            int minBW_kbps = customBandwidth * 1000;
            int totalDelay = 10;
            if (customCost.HasValue)
                totalDelay = customCost.Value;

            float bwComponent = (10000000f / Mathf.Max(1, minBW_kbps)) * 256f;
            float delayComponent = totalDelay * 256f;
            float metric = (k1 * bwComponent) + (k3 * delayComponent);
            return Mathf.Max(1, Mathf.RoundToInt(metric));
        }

        public void SimulateConvergence(System.Action onComplete)
        {
            StartCoroutine(SimulateConvergenceCoroutine(onComplete));
        }

        private IEnumerator SimulateConvergenceCoroutine(System.Action onComplete)
        {
            var routers = new List<NetworkNode>();
            if (topology != null)
            {
                var allNodes = topology.GetAllNodes();
                foreach (var n in allNodes)
                {
                    if (n.Type == DeviceType.Router)
                        routers.Add(n);
                }
            }

            yield return new WaitForSeconds(1f);

            foreach (var router in routers)
            {
                RouterAdvertState foundState = null;
                foreach (var s in routerStates)
                {
                    if (s.Router == router)
                    {
                        foundState = s;
                        break;
                    }
                }
                if (foundState != null)
                {
                    Log($"{router.Name} tiene {foundState.KnownNetworks.Count} redes en su tabla");
                }

                yield return new WaitForSeconds(0.5f);
            }

            onComplete?.Invoke();
        }

        public string GetProtocolStatus()
        {
            if (!isRunning) return $"{protocol}: Detenido";
            if (isConverged) return $"{protocol}: Convergido ({advertisementCount} ads)";
            return $"{protocol}: Ejecutando ({advertisementCount} ads)";
        }

        public List<NetworkNode> GetRouters()
        {
            var routers = new List<NetworkNode>();
            if (topology == null) return routers;
            var allNodes = topology.GetAllNodes();
            foreach (var n in allNodes)
            {
                if (n.Type == DeviceType.Router)
                    routers.Add(n);
            }
            return routers;
        }

        public string GetRouterRoutesSummary(NetworkNode router)
        {
            var entries = router.RoutingTable.GetAllEntries();
            if (entries.Count == 0) return "Sin rutas";

            var summary = new List<string>();
            int count = 0;
            foreach (var entry in entries)
            {
                if (count >= 5) break;
                summary.Add(RoutingTable.FormatRouteLine(entry));
                count++;
            }
            return string.Join("\n", summary);
        }

        private void Log(string message)
        {
            UnityEngine.Debug.Log($"[{protocol}] {message}");
            OnProtocolLog?.Invoke(message);
        }

        // ============================================================
        // Configuracion virtual de discos 15-18
        // Estos metodos son llamados desde DynamicRoutingActivity.ApplyDiscConfigToProtocol()
        // ============================================================

        /// <summary>
        /// Disco 15 - Vecino: Establece un router vecino manual para filtrar conexiones.
        /// </summary>
        public void SetManualNeighbor(string routerName)
        {
            manualNeighbor = routerName;
            Log($"Vecino manual configurado: {routerName}");
        }

        /// <summary>
        /// Disco 16 - AnunciarRed: Establece una red especifica a anunciar.
        /// </summary>
        public void SetManualNetwork(string network)
        {
            manualNetwork = network;
            Log($"Red manual a anunciar: {network}");
        }

        /// <summary>
        /// Disco 17 - Costo: Establece el costo personalizado para OSPF.
        /// </summary>
        public void SetCustomCost(int cost)
        {
            customCost = cost;
            Log($"Costo OSPF personalizado: {cost} (desde disco 17/UI)");
        }

        /// <summary>
        /// Disco 18 - BW: Establece el ancho de banda personalizado.
        /// </summary>
        public void SetCustomBandwidth(int bw)
        {
            customBandwidth = Mathf.Max(1, bw);
            Log($"Ancho de banda personalizado: {customBandwidth} Mbps");
        }

        /// <summary>
        /// Establece un K value individual para EIGRP (kIndex 1-5).
        /// </summary>
        public void SetKValue(int kIndex, int value)
        {
            switch (kIndex)
            {
                case 1: k1 = Mathf.Max(0, value); break;
                case 2: k2 = Mathf.Max(0, value); break;
                case 3: k3 = Mathf.Max(0, value); break;
                case 4: k4 = Mathf.Max(0, value); break;
                case 5: k5 = Mathf.Max(0, value); break;
                default:
                    Log($"Indice K invalido: {kIndex}. Usar 1-5.");
                    return;
            }
            Log($"K{kIndex} establecido a {value}");
        }

        /// <summary>
        /// Establece todos los K values para EIGRP simultaneamente.
        /// </summary>
        public void SetAllKValues(int k1, int k2, int k3, int k4, int k5)
        {
            this.k1 = Mathf.Max(0, k1);
            this.k2 = Mathf.Max(0, k2);
            this.k3 = Mathf.Max(0, k3);
            this.k4 = Mathf.Max(0, k4);
            this.k5 = Mathf.Max(0, k5);
            Log($"K values establecidos: K1={this.k1}, K2={this.k2}, K3={this.k3}, K4={this.k4}, K5={this.k5}");
        }

        public void ClearAllRoutes()
        {
            var routers = GetRouters();
            foreach (var router in routers)
            {
                router.RoutingTable.Clear();
            }
            routerStates.Clear();
            isRunning = false;
            isConverged = false;
            advertisementCount = 0;
            k1 = 1; k2 = 0; k3 = 1; k4 = 0; k5 = 0;
            Log("Todas las rutas han sido eliminadas");
        }
    }
}