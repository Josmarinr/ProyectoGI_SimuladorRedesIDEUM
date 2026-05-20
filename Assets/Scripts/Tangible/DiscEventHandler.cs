using System.Collections.Generic;
using UnityEngine;
using SimRedes.Network;

namespace SimRedes.Tangible
{
    public class DiscEventHandler : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool autoConnectLinks = true;
        [SerializeField] private float linkDistanceThreshold = 300f;
        [SerializeField] private float routerProximityRadius = 150f;

        private Dictionary<int, RouteBuilderState> routeBuilders = new Dictionary<int, RouteBuilderState>();

        private void Start()
        {
            var tangibleManager = FindObjectOfType<TangibleDiscManager>();
            if (tangibleManager != null)
            {
                tangibleManager.OnDiscPlaced += HandleDiscPlaced;
                tangibleManager.OnDiscMoved += HandleDiscMoved;
                tangibleManager.OnDiscRemoved += HandleDiscRemoved;
            }
        }

        private void HandleDiscPlaced(int discId, Vector2 position)
        {
            var discManager = FindObjectOfType<TangibleDiscManager>();
            int discType = discManager != null ? discManager.GetDiscType(discId) : 1;

            var config = DiscConfiguration.GetConfiguration(discType);
            SimRedes.Network.DeviceType deviceType = ConvertToDeviceType(config.Type);

            var topologyManager = FindObjectOfType<TopologyManager>();
            if (topologyManager == null) return;

            if (IsRoutingConfigDisc(config.Type))
            {
                HandleRoutingConfigDisc(discId, position, config, topologyManager);
                return;
            }

            topologyManager.AddNode(discId, deviceType, position);
            if (autoConnectLinks)
                CheckAndCreateLinks(discId, position);

            UnityEngine.Debug.Log($"[DiscEvent] Disco colocado: {config.Label} (uniqueId={discId}) en posicion {position}");
        }

        private void HandleRoutingConfigDisc(int discId, Vector2 position, DiscConfiguration config, TopologyManager topology)
        {
            var nearbyRouters = topology.FindNodesNear(position, routerProximityRadius);
            var router = nearbyRouters.Find(n => n.Type == SimRedes.Network.DeviceType.Router);

            if (router == null)
            {
                UnityEngine.Debug.Log($"[DiscEvent] Disco de configuracion '{config.Label}' debe colocarse cerca de un router.");
                return;
            }

            UnityEngine.Debug.Log($"[DiscEvent] Disco '{config.Label}' asociado a router {router.Name} (discId={router.DiscId})");

            if (!routeBuilders.ContainsKey(router.DiscId))
                routeBuilders[router.DiscId] = new RouteBuilderState { RouterDiscId = router.DiscId, OutInterface = "G0/0" };

            var builder = routeBuilders[router.DiscId];

            switch (config.Type)
            {
                case DiscType.RedDestino:
                    builder.DestinationNetwork = "192.168.1.0";
                    TryAddRoute(router, builder, topology);
                    break;

                case DiscType.Mascara:
                    builder.SubnetMask = "255.255.255.0";
                    TryAddRoute(router, builder, topology);
                    break;

                case DiscType.ProximoSalto:
                    builder.NextHop = "192.168.1.254";
                    TryAddRoute(router, builder, topology);
                    break;

                case DiscType.InterfazSalida:
                    builder.OutInterface = GetNextInterface(router);
                    TryAddRoute(router, builder, topology);
                    break;

                case DiscType.IpRoute:
                    HandleIpRouteDisc(router, topology);
                    break;

                case DiscType.ModoEnrutamiento:
                    builder.Protocol = builder.Protocol == "OSPF" ? "RIP" : "OSPF";
                    UnityEngine.Debug.Log($"[DiscEvent] Modo enrutamiento cambiado a {builder.Protocol} en {router.Name}");
                    break;

                case DiscType.Metrica:
                    if (router.RoutingTable.GetAllEntries().Count > 0)
                    {
                        var lastEntry = router.RoutingTable.GetAllEntries()[^1];
                        lastEntry.Metric = 10;
                        UnityEngine.Debug.Log($"[DiscEvent] Metrica ajustada a 10 en ultima ruta de {router.Name}");
                    }
                    break;

                case DiscType.Vecino:
                case DiscType.AnunciarRed:
                case DiscType.Costo:
                case DiscType.BW:
                    UnityEngine.Debug.Log($"[DiscEvent] Disco '{config.Label}' requiere protocolo dinamico (RIP/OSPF). Soporte proximamente.");
                    break;
            }
        }

        private void TryAddRoute(NetworkNode router, RouteBuilderState builder, TopologyManager topology)
        {
            if (builder.IsComplete)
            {
                builder.ApplyToRouter(topology);
                builder.Reset();
                builder.OutInterface = "G0/0";
            }
            else
            {
                string status = $"Estado ruta {router.Name}: dest={builder.DestinationNetwork ?? "?"} mask={builder.SubnetMask ?? "?"} nextHop={builder.NextHop ?? "?"} iface={builder.OutInterface ?? "?"}";
                UnityEngine.Debug.Log($"[DiscEvent] {status}");
            }
        }

        private void HandleIpRouteDisc(NetworkNode router, TopologyManager topology)
        {
            router.RoutingTable.AddStaticRoute("0.0.0.0", "0.0.0.0", "192.168.1.254", "G0/0");
            UnityEngine.Debug.Log($"[DiscEvent] Ruta por defecto agregada a {router.Name}: 0.0.0.0/0 -> 192.168.1.254");
        }

        private string GetNextInterface(NetworkNode router)
        {
            var interfaces = router.Interfaces;
            if (interfaces == null || interfaces.Count == 0)
                return "G0/0";

            int usedCount = 0;
            foreach (var entry in router.RoutingTable.GetAllEntries())
            {
                if (entry.OutInterface != null && entry.OutInterface.StartsWith("G"))
                    usedCount++;
            }
            int ifaceIndex = usedCount % interfaces.Count;
            return interfaces[ifaceIndex];
        }

        private bool IsRoutingConfigDisc(DiscType type)
        {
            switch (type)
            {
                case DiscType.RedDestino:
                case DiscType.Metrica:
                case DiscType.InterfazSalida:
                case DiscType.ModoEnrutamiento:
                case DiscType.IpRoute:
                case DiscType.Destino:
                case DiscType.Mascara:
                case DiscType.ProximoSalto:
                case DiscType.Vecino:
                case DiscType.AnunciarRed:
                case DiscType.Costo:
                case DiscType.BW:
                    return true;
                default:
                    return false;
            }
        }

        private void HandleDiscMoved(int discId, Vector2 position)
        {
            var topologyManager = FindObjectOfType<TopologyManager>();
            if (topologyManager != null)
            {
                topologyManager.UpdateNodePosition(discId, position);

                if (autoConnectLinks)
                {
                    CheckAndCreateLinks(discId, position);
                }
            }
        }

        private void HandleDiscRemoved(int discId)
        {
            var topologyManager = FindObjectOfType<TopologyManager>();
            if (topologyManager != null)
            {
                topologyManager.RemoveNode(discId);
            }

            UnityEngine.Debug.Log($"[DiscEvent] Disco eliminado: {discId}");
        }

        private SimRedes.Network.DeviceType ConvertToDeviceType(DiscType discType)
        {
            switch (discType)
            {
                case DiscType.Router:
                    return SimRedes.Network.DeviceType.Router;
                case DiscType.Switch:
                    return SimRedes.Network.DeviceType.Switch;
                case DiscType.PC:
                    return SimRedes.Network.DeviceType.PC;
                default:
                    return SimRedes.Network.DeviceType.Unknown;
            }
        }

        private void CheckAndCreateLinks(int discId, Vector2 position)
        {
            var topologyManager = FindObjectOfType<TopologyManager>();
            if (topologyManager == null) return;

            var activeDiscs = FindObjectOfType<TangibleDiscManager>().GetActiveDiscs();

            foreach (var kvp in activeDiscs)
            {
                if (kvp.Key == discId) continue;

                var otherPos = kvp.Value;
                float distance = Vector2.Distance(position, otherPos);

                if (distance <= linkDistanceThreshold)
                {
                    topologyManager.AddLink(discId, kvp.Key);
                }
            }
        }

        private void OnDestroy()
        {
            var tangibleManager = FindObjectOfType<TangibleDiscManager>();
            if (tangibleManager != null)
            {
                tangibleManager.OnDiscPlaced -= HandleDiscPlaced;
                tangibleManager.OnDiscMoved -= HandleDiscMoved;
                tangibleManager.OnDiscRemoved -= HandleDiscRemoved;
            }
        }
    }
}