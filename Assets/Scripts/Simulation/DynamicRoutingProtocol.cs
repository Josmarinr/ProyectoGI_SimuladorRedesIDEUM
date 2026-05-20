using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SimRedes.Network;
using SimRedes.UI;
using DeviceType = SimRedes.Network.DeviceType;

namespace SimRedes.Simulation
{
    public class DynamicRoutingProtocol : MonoBehaviour
    {
        public enum ProtocolType { RIP, OSPF }

        [Header("Configuracion")]
        public ProtocolType protocol = ProtocolType.RIP;
        public float advertisementInterval = 3f;
        public bool autoStart = false;

        private TopologyManager topology;
        private PingVisualizer pingVis;
        private bool isRunning = false;
        private bool isConverged = false;
        private int advertisementCount = 0;
        private List<RouterAdvertState> routerStates = new List<RouterAdvertState>();

        public event Action<string> OnProtocolLog;
        public event Action OnConvergence;

        private class RouterAdvertState
        {
            public NetworkNode Router;
            public List<string> KnownNetworks = new List<string>();
            public int ConvergenceVersion = 0;
        }

        private void Start()
        {
            topology = FindObjectOfType<TopologyManager>();
            pingVis = FindObjectOfType<PingVisualizer>();

            if (autoStart)
            {
                StartProtocol();
            }
        }

        public void StartProtocol()
        {
            if (isRunning) return;

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

            OnConvergence?.Invoke();
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

                                int metric = protocol == ProtocolType.RIP ? state.KnownNetworks.Count : CalculateOSPFCost(state.Router, connected);

                                if (protocol == ProtocolType.RIP)
                                {
                                    connState.Router.RoutingTable.AddRipRoute(network, GetNextHop(state.Router, connected), GetInterface(state.Router, connected), state.KnownNetworks.Count);
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

                if (link.SourceNode == router && link.DestinationNode.Type == DeviceType.Router)
                {
                    connected.Add(link.DestinationNode);
                }
                else if (link.DestinationNode == router && link.SourceNode.Type == DeviceType.Router)
                {
                    connected.Add(link.SourceNode);
                }
            }

            return connected;
        }

        private string GetNextHop(NetworkNode from, NetworkNode to)
        {
            if (IPValidation.IsValidIP(to.IpAddress))
                return to.IpAddress;

            var links = topology.GetAllLinks();
            foreach (var link in links)
            {
                if ((link.SourceNode == from && link.DestinationNode == to) ||
                    (link.SourceNode == to && link.DestinationNode == from))
                {
                    if (IPValidation.IsValidIP(link.SourceNode == from ? link.DestinationNode.IpAddress : link.SourceNode.IpAddress))
                    {
                        return link.SourceNode == from ? link.DestinationNode.IpAddress : link.SourceNode.IpAddress;
                    }
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
            return UnityEngine.Random.Range(10, 50);
        }

        public void AnimateAdvertisement(NetworkNode from, NetworkNode to, string network)
        {
            if (pingVis == null) pingVis = FindObjectOfType<PingVisualizer>();
            if (pingVis == null) return;

            int fromId = from.DiscId;
            int toId = to.DiscId;

            var path = topology.FindPath(fromId, toId);
            if (path.Count >= 2)
            {
                StartCoroutine(AnimateAdvertCoroutine(path, network));
            }
        }

        private IEnumerator AnimateAdvertCoroutine(List<NetworkNode> path, string network)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                var from = path[i];
                var to = path[i + 1];

                var nodeVis = FindObjectOfType<NodeVisualizer>();
                if (nodeVis != null)
                {
                    nodeVis.SelectNodeByDiscId(from.DiscId);
                }

                yield return new WaitForSeconds(0.3f);
            }
        }

        public void SimulateConvergence(System.Action onComplete)
        {
            StartCoroutine(SimulateConvergenceCoroutine(onComplete));
        }

        private IEnumerator SimulateConvergenceCoroutine(System.Action onComplete)
        {
            var allNodes = topology.GetAllNodes();
            var routers = new List<NetworkNode>();
            foreach (var n in allNodes)
            {
                if (n.Type == DeviceType.Router)
                    routers.Add(n);
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
                summary.Add($"{entry.DestinationNetwork}/{entry.GetPrefixLength()} via {entry.NextHop} ({entry.Protocol})");
                count++;
            }
            return string.Join("\n", summary);
        }

        private void Log(string message)
        {
            UnityEngine.Debug.Log($"[{protocol}] {message}");
            OnProtocolLog?.Invoke(message);
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
            Log("Todas las rutas han sido eliminadas");
        }
    }
}