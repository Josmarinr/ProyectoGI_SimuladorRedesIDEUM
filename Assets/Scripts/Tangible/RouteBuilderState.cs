using System.Collections.Generic;
using SimRedes.Network;

namespace SimRedes.Tangible
{
    public class RouteBuilderState
    {
        public int RouterDiscId { get; set; }
        public string DestinationNetwork { get; set; }
        public string SubnetMask { get; set; }
        public string NextHop { get; set; }
        public string OutInterface { get; set; }
        public string Protocol { get; set; }

        public bool IsComplete => !string.IsNullOrEmpty(DestinationNetwork)
            && !string.IsNullOrEmpty(SubnetMask)
            && !string.IsNullOrEmpty(NextHop)
            && !string.IsNullOrEmpty(OutInterface);

        public void ApplyToRouter(TopologyManager topology)
        {
            if (!IsComplete) return;

            var router = topology.GetNode(RouterDiscId);
            if (router == null) return;

            string proto = string.IsNullOrEmpty(Protocol) ? "Static" : Protocol;
            router.RoutingTable.AddStaticRoute(DestinationNetwork, SubnetMask, NextHop, OutInterface);
            router.RoutingTable.GetAllEntries()[^1].Protocol = proto;

            UnityEngine.Debug.Log($"[RouteBuilder] Ruta agregada a {router.Name}: {DestinationNetwork}/{SubnetMask} -> {NextHop} via {OutInterface} ({proto})");
        }

        public void Reset()
        {
            DestinationNetwork = null;
            SubnetMask = null;
            NextHop = null;
            OutInterface = null;
        }
    }
}
