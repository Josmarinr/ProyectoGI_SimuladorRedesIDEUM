using System;
using System.Collections.Generic;
using System.Linq;

namespace SimRedes.Network
{
    public class RoutingEntry
    {
        public string DestinationNetwork { get; set; }
        public string SubnetMask { get; set; }
        public string NextHop { get; set; }
        public string OutInterface { get; set; }
        public int Metric { get; set; }
        public string Protocol { get; set; }

        public RoutingEntry(string dest, string mask, string nextHop, string iface, int metric, string proto)
        {
            DestinationNetwork = dest;
            SubnetMask = mask;
            NextHop = nextHop;
            OutInterface = iface;
            Metric = metric;
            Protocol = proto;
        }

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

    public class RoutingTable
    {
        private List<RoutingEntry> entries = new List<RoutingEntry>();
        private NetworkNode router;

        public RoutingTable(NetworkNode routerNode)
        {
            router = routerNode;
        }

        public void AddStaticRoute(string destNetwork, string mask, string nextHop, string iface)
        {
            entries.Add(new RoutingEntry(destNetwork, mask, nextHop, iface, 0, "Static"));
        }

        public void AddRipRoute(string destNetwork, string nextHop, string iface, int hops)
        {
            entries.Add(new RoutingEntry(destNetwork, "255.255.255.0", nextHop, iface, hops, "RIP"));
        }

        public void AddOspfRoute(string destNetwork, string nextHop, string iface, int cost)
        {
            entries.Add(new RoutingEntry(destNetwork, "255.255.255.0", nextHop, iface, cost, "OSPF"));
        }

        public RoutingEntry FindBestRoute(string destinationIP)
        {
            var matchingEntries = entries
                .Where(e => IsInNetwork(destinationIP, e.DestinationNetwork, e.SubnetMask))
                .OrderByDescending(e => e.GetPrefixLength())
                .ThenBy(e => e.Metric)
                .ToList();

            return matchingEntries.FirstOrDefault();
        }

        private bool IsInNetwork(string ip, string network, string mask)
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

        public List<RoutingEntry> GetAllEntries()
        {
            return new List<RoutingEntry>(entries);
        }

        public void Clear()
        {
            entries.Clear();
        }

        public string GetTableSummary()
        {
            return $"Entradas: {entries.Count}";
        }
    }
}