using SimRedes.Network;
using System.Collections.Generic;
using UnityEngine;

namespace SimRedes.Simulation
{
    public enum RoutingProtocol
    {
        Static,
        RIP,
        OSPF
    }

    public static class RoutingSimulator
    {
        public static RoutingProtocol ActiveProtocol { get; set; } = RoutingProtocol.Static;

        public static void SetProtocol(RoutingProtocol protocol)
        {
            ActiveProtocol = protocol;
            Debug.Log($"[RoutingSim] Protocolo cambiado a: {protocol}");
        }

        public static void SimulateRIPAdvertisement(NetworkNode router, List<(string network, int hops)> advertisements)
        {
            if (router == null || router.RoutingTable == null) return;
            foreach (var adv in advertisements)
            {
                router.RoutingTable.AddRipRoute(adv.network, "?", "G0/0", adv.hops + 1);
            }
            Debug.Log($"[RoutingSim] Anuncios RIP procesados para Router {router.DiscId}");
        }

        public static void SimulateOSPFAdvertisement(NetworkNode router, List<(string network, int cost)> advertisements)
        {
            if (router == null || router.RoutingTable == null) return;
            foreach (var adv in advertisements)
            {
                router.RoutingTable.AddOspfRoute(adv.network, "?", "G0/0", adv.cost);
            }
            Debug.Log($"[RoutingSim] Anuncios OSPF procesados para Router {router.DiscId}");
        }

        public static bool SimulatePacketForwarding(NetworkNode router, string destinationIP)
        {
            if (router == null || router.RoutingTable == null) return false;
            var bestRoute = router.RoutingTable.FindBestRoute(destinationIP);
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
            return false;
        }

        public static string GetRoutingTableSummary(NetworkNode router)
        {
            if (router == null || router.RoutingTable == null) return "Sin tabla de enrutamiento";
            var entries = router.RoutingTable.GetAllEntries();
            string summary = $"=== Tabla de Enrutamiento Router {router.DiscId} ({router.Name}) ===\n";
            foreach (var entry in entries)
            {
                summary += $"{entry.DestinationNetwork}/{entry.GetPrefixLength()} -> {entry.NextHop} via {entry.OutInterface} [Metrica: {entry.Metric}] ({entry.Protocol})\n";
            }
            return summary;
        }
    }


}