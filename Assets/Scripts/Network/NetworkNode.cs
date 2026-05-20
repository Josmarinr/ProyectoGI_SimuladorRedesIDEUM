using System.Collections.Generic;
using UnityEngine;

namespace SimRedes.Network
{
    public enum DeviceType
    {
        Router,
        Switch,
        PC,
        Unknown
    }

    public class NetworkNode
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DeviceType Type { get; set; }
        public string IpAddress { get; set; }
        public string SubnetMask { get; set; }
        public Vector2 Position { get; set; }
        public int DiscId { get; set; }
        public bool IsActive { get; set; }
        public List<string> Interfaces { get; set; }
        public Dictionary<string, string> InterfaceIPs { get; set; }
        public bool IsAdminDown { get; set; }
        public string ConfiguredFault { get; set; }
        public int VlanId { get; set; }

        public ARPTable ArpTable { get; private set; }
        public RoutingTable RoutingTable { get; private set; }

        public NetworkNode()
        {
            Interfaces = new List<string> { "G0/0", "G0/1", "G0/2", "G0/3" };
            InterfaceIPs = new Dictionary<string, string>();
            IsActive = true;
            IsAdminDown = false;

            ArpTable = new ARPTable(this);
            RoutingTable = new RoutingTable(this);
        }

        public NetworkNode(int discId, DeviceType type, Vector2 position) : this()
        {
            DiscId = discId;
            Type = type;
            Position = position;
            Name = $"{type}_{discId}";
            Id = System.Guid.NewGuid().ToString();
        }

        public void SetInterfaceIP(string interfaceName, string ip, string mask)
        {
            if (Interfaces.Contains(interfaceName))
            {
                InterfaceIPs[interfaceName] = ip;
            }
        }

        public string GetInterfaceIP(string interfaceName)
        {
            return InterfaceIPs.TryGetValue(interfaceName, out string ip) ? ip : null;
        }

        public bool HasFault()
        {
            return !string.IsNullOrEmpty(ConfiguredFault);
        }

        public string GetDefaultGateway()
        {
            return IPValidation.GetGatewayFromIP(IpAddress);
        }

        public string GetNetworkAddress()
        {
            return IPValidation.GetNetworkAddress(IpAddress, SubnetMask);
        }

        public int GetPrefixLength()
        {
            return IPValidation.GetPrefixLength(SubnetMask);
        }

        public bool IsValidConfiguration()
        {
            return IPValidation.IsValidIP(IpAddress) && IPValidation.IsValidSubnetMask(SubnetMask);
        }
    }
}