using SimRedes.Network;
using System.Collections.Generic;
using UnityEngine;

namespace SimRedes.Simulation
{
    /// <summary>
    /// Protocolos de enrutamiento disponibles en el simulador.
    /// </summary>
    public enum RoutingProtocol
    {
        /// <summary>Ruta estatica configurada manualmente.</summary>
        Static,
        /// <summary>Routing Information Protocol (RIP) basado en saltos.</summary>
        RIP,
        /// <summary>Open Shortest Path First (OSPF) basado en costo.</summary>
        OSPF,
        /// <summary>Enhanced Interior Gateway Routing Protocol (EIGRP) basado en metrica compuesta.</summary>
        EIGRP
    }

    /// <summary>
    /// Simulador de protocolos de enrutamiento. Permite cambiar el protocolo activo,
    /// procesar anuncios (RIP, OSPF, EIGRP) y simular el reenvio de paquetes.
    /// </summary>
    public static class RoutingSimulator
    {
        /// <summary>
        /// Protocolo de enrutamiento activo actualmente. Por defecto es Static.
        /// </summary>
        public static RoutingProtocol ActiveProtocol { get; set; } = RoutingProtocol.Static;

        /// <summary>
        /// Cambia el protocolo de enrutamiento activo.
        /// </summary>
        /// <param name="protocol">Nuevo protocolo a usar.</param>
        public static void SetProtocol(RoutingProtocol protocol)
        {
            ActiveProtocol = protocol;
            Debug.Log($"[RoutingSim] Protocolo cambiado a: {protocol}");
        }

        /// <summary>
        /// Simula la recepcion de anuncios RIP y agrega rutas con metrica de saltos incrementada en 1.
        /// </summary>
        /// <param name="router">Router que recibe los anuncios.</param>
        /// <param name="advertisements">Lista de tuplas (red, saltos) con las rutas anunciadas.</param>
        public static void SimulateRIPAdvertisement(NetworkNode router, List<(string network, int hops)> advertisements)
        {
            if (router == null || router.RoutingTable == null) return;
            foreach (var adv in advertisements)
            {
                router.RoutingTable.AddRipRoute(adv.network, "?", "G0/0", adv.hops + 1);
            }
            Debug.Log($"[RoutingSim] Anuncios RIP procesados para Router {router.DiscId}");
        }

        /// <summary>
        /// Simula la recepcion de anuncios OSPF y agrega rutas con el costo especificado.
        /// </summary>
        /// <param name="router">Router que recibe los anuncios.</param>
        /// <param name="advertisements">Lista de tuplas (red, costo) con las rutas anunciadas.</param>
        public static void SimulateOSPFAdvertisement(NetworkNode router, List<(string network, int cost)> advertisements)
        {
            if (router == null || router.RoutingTable == null) return;
            foreach (var adv in advertisements)
            {
                router.RoutingTable.AddOspfRoute(adv.network, "?", "G0/0", adv.cost);
            }
            Debug.Log($"[RoutingSim] Anuncios OSPF procesados para Router {router.DiscId}");
        }

        /// <summary>
        /// Simula la recepcion de anuncios EIGRP y agrega rutas con la metrica especificada.
        /// </summary>
        /// <param name="router">Router que recibe los anuncios.</param>
        /// <param name="advertisements">Lista de tuplas (red, metrica) con las rutas anunciadas.</param>
        public static void SimulateEIGRPAdvertisement(NetworkNode router, List<(string network, int metric)> advertisements)
        {
            if (router == null || router.RoutingTable == null) return;
            foreach (var adv in advertisements)
            {
                router.RoutingTable.AddEigrpRoute(adv.network, "?", "G0/0", adv.metric);
            }
            Debug.Log($"[RoutingSim] Anuncios EIGRP procesados para Router {router.DiscId}");
        }

        /// <summary>
        /// Simula el reenvio de un paquete hacia una IP destino.
        /// Busca la mejor ruta en la tabla y registra el resultado.
        /// </summary>
        /// <param name="router">Router que realiza el reenvio.</param>
        /// <param name="destinationIP">Direccion IP de destino.</param>
        /// <returns>True si se encontro una ruta, false si no hay ruta disponible.</returns>
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

        /// <summary>
        /// Genera un resumen formateado de la tabla de enrutamiento de un router.
        /// </summary>
        /// <param name="router">Router del cual obtener la tabla.</param>
        /// <returns>Texto con todas las entradas de la tabla de enrutamiento.</returns>
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