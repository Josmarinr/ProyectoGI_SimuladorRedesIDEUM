using System;
using System.Collections.Generic;
using System.Linq;
using SimRedes.Network;
using UnityEngine;

namespace SimRedes.Simulation
{
    public enum RoutingProtocol
    {
        Static,
        RIP,
        OSPF,
        EIGRP
    }

    public class RoutingSimulator
    {
        private Dictionary<int, RoutingTable> routerTables = new Dictionary<int, RoutingTable>();
        private RoutingProtocol activeProtocol = RoutingProtocol.Static;

        public void SetProtocol(RoutingProtocol protocol)
        {
            activeProtocol = protocol;
            Debug.Log($"[RoutingSim] Protocolo cambiado a: {protocol}");
        }

        public void InitializeRouterTable(NetworkNode router)
        {
            if (!routerTables.ContainsKey(router.DiscId))
            {
                routerTables[router.DiscId] = new RoutingTable(router);
                Debug.Log($"[RoutingSim] Tabla de enrutamiento creada para {router.Name}");
            }
        }

        public void AddStaticRoute(int routerDiscId, string destNetwork, string mask, string nextHop, string iface)
        {
            if (routerTables.TryGetValue(routerDiscId, out var table))
            {
                table.AddStaticRoute(destNetwork, mask, nextHop, iface);
                Debug.Log($"[RoutingSim] Ruta estática añadida: {destNetwork} via {nextHop}");
            }
        }

        public void SimulateRIPAdvertisement(int routerDiscId, List<(string network, int hops)> advertisements)
        {
            if (routerTables.TryGetValue(routerDiscId, out var table))
            {
                foreach (var adv in advertisements)
                {
                    table.AddRipRoute(adv.network, "?", "G0/0", adv.hops + 1);
                }
                Debug.Log($"[RoutingSim] Anuncios RIP procesados para Router {routerDiscId}");
            }
        }

        public void SimulateOSPFAdvertisement(int routerDiscId, List<(string network, int cost)> advertisements)
        {
            if (routerTables.TryGetValue(routerDiscId, out var table))
            {
                foreach (var adv in advertisements)
                {
                    table.AddOspfRoute(adv.network, "?", "G0/0", adv.cost);
                }
                Debug.Log($"[RoutingSim] Anuncios OSPF procesados para Router {routerDiscId}");
            }
        }

        public bool SimulatePacketForwarding(int sourceRouterDiscId, string destinationIP)
        {
            if (routerTables.TryGetValue(sourceRouterDiscId, out var table))
            {
                var bestRoute = table.FindBestRoute(destinationIP);
                if (bestRoute != null)
                {
                    Debug.Log($"[RoutingSim] Paquete hacia {destinationIP}");
                    Debug.Log($"  -> Ruta encontrada: {bestRoute.DestinationNetwork}/{bestRoute.GetPrefixLength()}");
                    Debug.Log($"  -> Interfaz de salida: {bestRoute.OutInterface}");
                    Debug.Log($"  -> Siguiente salto: {bestRoute.NextHop}");
                    Debug.Log($"  -> Métrica: {bestRoute.Metric} ({bestRoute.Protocol})");
                    return true;
                }
                else
                {
                    Debug.Log($"[RoutingSim] No hay ruta hacia {destinationIP}");
                }
            }
            return false;
        }

        public string GetRoutingTableSummary(int routerDiscId)
        {
            if (routerTables.TryGetValue(routerDiscId, out var table))
            {
                var entries = table.GetAllEntries();
                string summary = $"=== Tabla de Enrutamiento Router {routerDiscId} ===\n";
                foreach (var entry in entries)
                {
                    summary += $"{entry.DestinationNetwork}/{entry.GetPrefixLength()} -> {entry.NextHop} via {entry.OutInterface} [Metrica: {entry.Metric}] ({entry.Protocol})\n";
                }
                return summary;
            }
            return "Sin tabla de enrutamiento";
        }

        public void ClearAllTables()
        {
            routerTables.Clear();
            Debug.Log("[RoutingSim] Todas las tablas limpiadas");
        }
    }

    public class PathCalculation
    {
        public static (int cost, List<string> path) CalculateShortestPath(
            List<(string node, int cost, string nextHop)> graph,
            string start,
            string destination)
        {
            var distances = new Dictionary<string, int>();
            var previous = new Dictionary<string, string>();
            var unvisited = new List<string>();

            foreach (var node in graph)
            {
                distances[node.node] = int.MaxValue;
                unvisited.Add(node.node);
            }

            distances[start] = 0;

            while (unvisited.Count > 0)
            {
                unvisited.Sort((a, b) => distances[a].CompareTo(distances[b]));
                string current = unvisited[0];
                unvisited.RemoveAt(0);

                if (current == destination)
                    break;

                var neighbors = graph.Where(n => n.node == current).ToList();
                foreach (var neighbor in neighbors)
                {
                    int alt = distances[current] + neighbor.cost;
                    if (alt < distances[neighbor.node])
                    {
                        distances[neighbor.node] = alt;
                        previous[neighbor.node] = current;
                    }
                }
            }

            var path = new List<string>();
            string curr = destination;
            while (previous.ContainsKey(curr))
            {
                path.Insert(0, curr);
                curr = previous[curr];
            }
            if (path.Count > 0 || curr == start)
            {
                path.Insert(0, start);
            }

            return (distances[destination], path);
        }

        public static bool IsLongestPrefixMatch(string ip, string dest1, string mask1, string dest2, string mask2)
        {
            var prefix1 = GetPrefixLength(mask1);
            var prefix2 = GetPrefixLength(mask2);
            return prefix1 > prefix2 && IsInNetwork(ip, dest1, mask1);
        }

        private static bool IsInNetwork(string ip, string network, string mask)
        {
            var ipParts = ip.Split('.').Select(int.Parse).ToArray();
            var netParts = network.Split('.').Select(int.Parse).ToArray();
            var maskParts = mask.Split('.').Select(int.Parse).ToArray();

            for (int i = 0; i < 4; i++)
            {
                if ((ipParts[i] & maskParts[i]) != (netParts[i] & maskParts[i]))
                    return false;
            }
            return true;
        }

        private static int GetPrefixLength(string mask)
        {
            if (mask == "255.255.255.255") return 32;
            if (mask == "255.255.255.252") return 30;
            if (mask == "255.255.255.0") return 24;
            if (mask == "255.255.0.0") return 16;
            if (mask == "255.0.0.0") return 8;
            return 0;
        }
    }
}