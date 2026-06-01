using System.Collections.Generic;
using UnityEngine;

namespace SimRedes.Network
{
    /// <summary>Tipo de dispositivo de red (Router, Switch, PC, Unknown).</summary>
    public enum DeviceType
    {
        Router,
        Switch,
        PC,
        Unknown
    }

    /// <summary>Representa un nodo de red con IP, interfaces, tabla ARP y tabla de enrutamiento.</summary>
    public class NetworkNode
    {
        /// <summary>Identificador único del nodo.</summary>
        public string Id { get; set; }
        /// <summary>Nombre del nodo (ej: Router_1).</summary>
        public string Name { get; set; }
        /// <summary>Tipo de dispositivo.</summary>
        public DeviceType Type { get; set; }
        /// <summary>Dirección IP asignada al nodo.</summary>
        public string IpAddress { get; set; }
        /// <summary>Máscara de subred del nodo.</summary>
        public string SubnetMask { get; set; }
        /// <summary>Posición en el canvas.</summary>
        public Vector2 Position { get; set; }
        /// <summary>Identificador del disco tangible asociado.</summary>
        public int DiscId { get; set; }
        /// <summary>Indica si el nodo está activo.</summary>
        public bool IsActive { get; set; }
        /// <summary>Lista de nombres de interfaces del nodo.</summary>
        public List<string> Interfaces { get; set; }
        /// <summary>Diccionario de IPs por interfaz.</summary>
        public Dictionary<string, string> InterfaceIPs { get; set; }
        /// <summary>Indica si la interfaz está administrativamente deshabilitada.</summary>
        public bool IsAdminDown { get; set; }
        /// <summary>Falla configurada en el nodo (para actividades de detección).</summary>
        public string ConfiguredFault { get; set; }
        /// <summary>ID de VLAN asignada al nodo.</summary>
        public int VlanId { get; set; }

        /// <summary>Tabla ARP del nodo.</summary>
        public ARPTable ArpTable { get; private set; }
        /// <summary>Tabla de enrutamiento del nodo.</summary>
        public RoutingTable RoutingTable { get; private set; }

        /// <summary>Crea un nodo con valores por defecto.</summary>
        public NetworkNode()
        {
            Interfaces = new List<string> { "G0/0", "G0/1", "G0/2", "G0/3" };
            InterfaceIPs = new Dictionary<string, string>();
            IsActive = true;
            IsAdminDown = false;

            ArpTable = new ARPTable(this);
            RoutingTable = new RoutingTable(this);
        }

        /// <summary>Crea un nodo con identificador de disco, tipo y posición.</summary>
        /// <param name="discId">Identificador del disco tangible.</param>
        /// <param name="type">Tipo de dispositivo.</param>
        /// <param name="position">Posición en el canvas.</param>
        public NetworkNode(int discId, DeviceType type, Vector2 position) : this()
        {
            DiscId = discId;
            Type = type;
            Position = position;
            Name = $"{type}_{discId}";
            Id = System.Guid.NewGuid().ToString();
        }

        /// <summary>Asigna una dirección IP a una interfaz del nodo.</summary>
        /// <param name="interfaceName">Nombre de la interfaz (ej: G0/0).</param>
        /// <param name="ip">Dirección IP.</param>
        /// <param name="mask">Máscara de subred.</param>
        public void SetInterfaceIP(string interfaceName, string ip, string mask)
        {
            if (Interfaces.Contains(interfaceName))
            {
                InterfaceIPs[interfaceName] = ip;
            }
        }

        /// <summary>Obtiene la IP asignada a una interfaz específica.</summary>
        /// <param name="interfaceName">Nombre de la interfaz.</param>
        /// <returns>Dirección IP o null si no está configurada.</returns>
        public string GetInterfaceIP(string interfaceName)
        {
            return InterfaceIPs.TryGetValue(interfaceName, out string ip) ? ip : null;
        }

        /// <summary>Indica si el nodo tiene una falla configurada.</summary>
        /// <returns>True si ConfiguredFault no es nulo o vacío.</returns>
        public bool HasFault()
        {
            return !string.IsNullOrEmpty(ConfiguredFault);
        }

        /// <summary>Obtiene la dirección del gateway por defecto basada en la IP del nodo.</summary>
        /// <returns>Dirección del gateway.</returns>
        public string GetDefaultGateway()
        {
            return IPValidation.GetGatewayFromIP(IpAddress);
        }

        /// <summary>Obtiene la dirección de red del nodo.</summary>
        /// <returns>Dirección de red.</returns>
        public string GetNetworkAddress()
        {
            return IPValidation.GetNetworkAddress(IpAddress, SubnetMask);
        }

        /// <summary>Obtiene la longitud del prefijo CIDR de la máscara del nodo.</summary>
        /// <returns>Longitud del prefijo (0-32).</returns>
        public int GetPrefixLength()
        {
            return IPValidation.GetPrefixLength(SubnetMask);
        }

        /// <summary>Verifica si la configuración IP del nodo es válida.</summary>
        /// <returns>True si la IP y la máscara son válidas.</returns>
        public bool IsValidConfiguration()
        {
            return IPValidation.IsValidIP(IpAddress) && IPValidation.IsValidSubnetMask(SubnetMask);
        }
    }
}