using System;
using System.Collections.Generic;
using System.Linq;

namespace SimRedes.Network
{
    /// <summary>Representa una entrada individual en la tabla de enrutamiento.</summary>
    public class RoutingEntry
    {
        /// <summary>Red de destino de la ruta.</summary>
        public string DestinationNetwork { get; set; }
        /// <summary>Máscara de subred de la ruta.</summary>
        public string SubnetMask { get; set; }
        /// <summary>Next hop (siguiente salto) hacia el destino.</summary>
        public string NextHop { get; set; }
        /// <summary>Interfaz de salida para esta ruta.</summary>
        public string OutInterface { get; set; }
        /// <summary>Métrica asociada a la ruta.</summary>
        public int Metric { get; set; }
        /// <summary>Protocolo de enrutamiento que originó la ruta (Static, RIP, OSPF, EIGRP).</summary>
        public string Protocol { get; set; }

        /// <summary>Crea una nueva entrada de enrutamiento.</summary>
        /// <param name="dest">Red de destino.</param>
        /// <param name="mask">Máscara de subred.</param>
        /// <param name="nextHop">Siguiente salto.</param>
        /// <param name="iface">Interfaz de salida.</param>
        /// <param name="metric">Métrica de la ruta.</param>
        /// <param name="proto">Protocolo de enrutamiento.</param>
        public RoutingEntry(string dest, string mask, string nextHop, string iface, int metric, string proto)
        {
            DestinationNetwork = dest;
            SubnetMask = mask;
            NextHop = nextHop;
            OutInterface = iface;
            Metric = metric;
            Protocol = proto;
        }

        /// <summary>Obtiene la longitud del prefijo CIDR a partir de la máscara de subred.</summary>
        /// <returns>Longitud del prefijo (0-32).</returns>
        public int GetPrefixLength()
        {
            if (SubnetMask == "255.255.255.255") return 32;
            if (SubnetMask == "255.255.255.254") return 31;
            if (SubnetMask == "255.255.255.252") return 30;
            if (SubnetMask == "255.255.255.248") return 29;
            if (SubnetMask == "255.255.255.240") return 28;
            if (SubnetMask == "255.255.255.224") return 27;
            if (SubnetMask == "255.255.255.192") return 26;
            if (SubnetMask == "255.255.255.128") return 25;
            if (SubnetMask == "255.255.255.0") return 24;
            if (SubnetMask == "255.255.254.0") return 23;
            if (SubnetMask == "255.255.252.0") return 22;
            if (SubnetMask == "255.255.248.0") return 21;
            if (SubnetMask == "255.255.240.0") return 20;
            if (SubnetMask == "255.255.224.0") return 19;
            if (SubnetMask == "255.255.192.0") return 18;
            if (SubnetMask == "255.255.128.0") return 17;
            if (SubnetMask == "255.255.0.0") return 16;
            if (SubnetMask == "255.254.0.0") return 15;
            if (SubnetMask == "255.252.0.0") return 14;
            if (SubnetMask == "255.248.0.0") return 13;
            if (SubnetMask == "255.240.0.0") return 12;
            if (SubnetMask == "255.224.0.0") return 11;
            if (SubnetMask == "255.192.0.0") return 10;
            if (SubnetMask == "255.128.0.0") return 9;
            if (SubnetMask == "255.0.0.0") return 8;
            return 0;
        }
    }

    /// <summary>Tabla de enrutamiento de un nodo de red. Almacena y consulta rutas estáticas y dinámicas.</summary>
    public class RoutingTable
    {
        private List<RoutingEntry> entries = new List<RoutingEntry>();
        private NetworkNode router;

        /// <summary>Crea una tabla de enrutamiento asociada a un nodo.</summary>
        /// <param name="routerNode">Nodo de red al que pertenece esta tabla.</param>
        public RoutingTable(NetworkNode routerNode)
        {
            router = routerNode;
        }

        /// <summary>Agrega una ruta estática a la tabla.</summary>
        /// <param name="destNetwork">Red de destino.</param>
        /// <param name="mask">Máscara de subred.</param>
        /// <param name="nextHop">Siguiente salto.</param>
        /// <param name="iface">Interfaz de salida.</param>
        public void AddStaticRoute(string destNetwork, string mask, string nextHop, string iface)
        {
            entries.Add(new RoutingEntry(destNetwork, mask, nextHop, iface, 0, "Static"));
        }

        /// <summary>Agrega una ruta aprendida por RIP.</summary>
        /// <param name="destNetwork">Red de destino.</param>
        /// <param name="nextHop">Siguiente salto.</param>
        /// <param name="iface">Interfaz de salida.</param>
        /// <param name="hops">Cantidad de saltos (métrica RIP).</param>
        public void AddRipRoute(string destNetwork, string nextHop, string iface, int hops)
        {
            entries.Add(new RoutingEntry(destNetwork, "255.255.255.0", nextHop, iface, hops, "RIP"));
        }

        /// <summary>Agrega una ruta aprendida por OSPF.</summary>
        /// <param name="destNetwork">Red de destino.</param>
        /// <param name="nextHop">Siguiente salto.</param>
        /// <param name="iface">Interfaz de salida.</param>
        /// <param name="cost">Costo OSPF de la ruta.</param>
        public void AddOspfRoute(string destNetwork, string nextHop, string iface, int cost)
        {
            entries.Add(new RoutingEntry(destNetwork, "255.255.255.0", nextHop, iface, cost, "OSPF"));
        }

        /// <summary>Agrega una ruta aprendida por EIGRP.</summary>
        /// <param name="destNetwork">Red de destino.</param>
        /// <param name="nextHop">Siguiente salto.</param>
        /// <param name="iface">Interfaz de salida.</param>
        /// <param name="compositeMetric">Métrica compuesta EIGRP.</param>
        public void AddEigrpRoute(string destNetwork, string nextHop, string iface, int compositeMetric)
        {
            entries.Add(new RoutingEntry(destNetwork, "255.255.255.0", nextHop, iface, compositeMetric, "EIGRP"));
        }

        /// <summary>Encuentra la mejor ruta para una IP de destino usando longest prefix match y menor métrica.</summary>
        /// <param name="destinationIP">Dirección IP de destino.</param>
        /// <returns>La entrada de ruta con mayor prefijo y menor métrica, o null si no hay coincidencia.</returns>
        public RoutingEntry FindBestRoute(string destinationIP)
        {
            var matchingEntries = entries
                .Where(e => IsInNetwork(destinationIP, e.DestinationNetwork, e.SubnetMask))
                .OrderByDescending(e => e.GetPrefixLength())
                .ThenBy(e => e.Metric)
                .ToList();

            return matchingEntries.FirstOrDefault();
        }

        /// <summary>Verifica si una IP pertenece a una red aplicando la máscara de subred.</summary>
        /// <param name="ip">Dirección IP a verificar.</param>
        /// <param name="network">Dirección de red.</param>
        /// <param name="mask">Máscara de subred.</param>
        /// <returns>True si la IP está dentro de la red.</returns>
        private bool IsInNetwork(string ip, string network, string mask)
        {
            var ipParts = ip.Split('.');
            var netParts = network.Split('.');
            var maskParts = mask.Split('.');

            if (ipParts.Length != 4 || netParts.Length != 4 || maskParts.Length != 4)
                return false;

            for (int i = 0; i < 4; i++)
            {
                if (!int.TryParse(ipParts[i], out int ipByte) ||
                    !int.TryParse(netParts[i], out int netByte) ||
                    !int.TryParse(maskParts[i], out int maskByte))
                    return false;

                if ((ipByte & maskByte) != (netByte & maskByte))
                    return false;
            }
            return true;
        }

        /// <summary>Obtiene una copia de todas las entradas de la tabla.</summary>
        /// <returns>Lista de entradas de enrutamiento.</returns>
        public List<RoutingEntry> GetAllEntries()
        {
            return new List<RoutingEntry>(entries);
        }

        /// <summary>Elimina todas las entradas de la tabla de enrutamiento.</summary>
        public void Clear()
        {
            entries.Clear();
        }

        /// <summary>Obtiene un resumen textual de la tabla de enrutamiento.</summary>
        /// <returns>Cadena con la cantidad de entradas.</returns>
        public string GetTableSummary()
        {
            return $"Entradas: {entries.Count}";
        }
    }
}