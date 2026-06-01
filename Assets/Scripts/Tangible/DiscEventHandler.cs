using System.Collections.Generic;
using UnityEngine;
using SimRedes.Network;
using SimRedes.Simulation;

namespace SimRedes.Tangible
{
    /// <summary>
    /// Maneja los eventos de TangibleDiscManager (OnDiscPlaced, OnDiscMoved, OnDiscRemoved)
    /// y los traduce a operaciones de topologia: crear/actualizar/eliminar nodos,
    /// crear enlaces automaticos por proximidad y gestionar discos de configuracion de rutas.
    /// </summary>
    public class DiscEventHandler : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool autoConnectLinks = true;
        [SerializeField] private float linkDistanceThreshold = 300f;
        [SerializeField] private float routerProximityRadius = 150f;

        private Dictionary<int, RouteBuilderState> routeBuilders = new Dictionary<int, RouteBuilderState>();

        /// <summary>
        /// Busca TangibleDiscManager y se suscribe a sus eventos de ciclo de vida de discos.
        /// </summary>
        private void Start()
        {
            var tangibleManager = Object.FindAnyObjectByType<TangibleDiscManager>();
            if (tangibleManager != null)
            {
                tangibleManager.OnDiscPlaced += HandleDiscPlaced;
                tangibleManager.OnDiscMoved += HandleDiscMoved;
                tangibleManager.OnDiscRemoved += HandleDiscRemoved;
            }
        }

        /// <summary>
        /// Procesa la colocacion de un disco: crea el nodo en TopologyManager si es
        /// Router/Switch/PC, y opcionalmente crea enlaces automaticos por proximidad.
        /// Los discos de configuracion de routing se delegan a HandleRoutingConfigDisc.
        /// </summary>
        /// <param name="discId">Identificador unico del disco colocado.</param>
        /// <param name="position">Posicion en coordenadas Canvas.</param>
        private void HandleDiscPlaced(int discId, Vector2 position)
        {
            var topologyManager = Object.FindAnyObjectByType<TopologyManager>();
            if (topologyManager == null) return;

            var discManager = Object.FindAnyObjectByType<TangibleDiscManager>();
            int discType = discManager != null ? discManager.GetDiscType(discId) : 1;

            var config = DiscConfiguration.GetConfiguration(discType);

            // Solo discos fisicos 1-3 (Router/Switch/PC) crean nodos.
            // Enlaces, fallos y configuracion de routing se manejan
            // desde las actividades (LinkModeController, FindFaultActivity,
            // StaticRoutingActivity, etc.)
            if (config.Type != DiscType.Router &&
                config.Type != DiscType.Switch &&
                config.Type != DiscType.PC)
            {
                UnityEngine.Debug.Log($"[DiscEvent] Disco '{config.Label}' (ID {discType}) ignorado — solo Router/Switch/PC son discos fisicos. El resto se maneja desde las actividades.");
                return;
            }

            SimRedes.Network.DeviceType deviceType = ConvertToDeviceType(config.Type);
            topologyManager.AddNode(discId, deviceType, position);
            if (autoConnectLinks)
                CheckAndCreateLinks(discId, position);

            UnityEngine.Debug.Log($"[DiscEvent] Disco colocado: {config.Label} (uniqueId={discId}) en posicion {position}");
        }

        /// <summary>
        /// Procesa discos de configuracion de routing (IDs 7-18): asocia valores
        /// al RouteBuilderState del router mas cercano, y cuando el builder esta completo
        /// aplica la ruta estatica o delega a DynamicRoutingActivity segun el tipo de disco.
        /// </summary>
        /// <param name="discId">Identificador unico del disco de configuracion.</param>
        /// <param name="position">Posicion del disco en coordenadas Canvas.</param>
        /// <param name="config">Configuracion del disco (tipo, etiqueta).</param>
        /// <param name="topology">Instancia de TopologyManager para operaciones de red.</param>
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

                case DiscType.Destino:
                    if (!routeBuilders.ContainsKey(router.DiscId))
                        routeBuilders[router.DiscId] = new RouteBuilderState { RouterDiscId = router.DiscId, OutInterface = "G0/0" };
                    routeBuilders[router.DiscId].DestinationNetwork = "192.168.1.0";
                    TryAddRoute(router, routeBuilders[router.DiscId], topology);
                    break;

                case DiscType.Vecino:
                {
                    // Disco 15 - Vecino: Configurar router vecino para enrutamiento dinamico
                    var dynAct = UnityEngine.Object.FindAnyObjectByType<DynamicRoutingActivity>();
                    if (dynAct != null)
                    {
                        string neighborName = $"Router{router.DiscId + 1}";
                        dynAct.SetNeighborRouter(neighborName);
                        UnityEngine.Debug.Log($"[DiscEvent] Vecino configurado virtualmente: {neighborName}");
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"[DiscEvent] Disco Vecino (15) requiere DynamicRoutingActivity activa");
                    }
                    break;
                }
                case DiscType.AnunciarRed:
                {
                    // Disco 16 - AnunciarRed: Configurar red a anunciar en rutas dinamicas
                    var dynAct = UnityEngine.Object.FindAnyObjectByType<DynamicRoutingActivity>();
                    if (dynAct != null)
                    {
                        string network = "10.0.0.0/8";
                        dynAct.SetNetworkToAdvertise(network);
                        UnityEngine.Debug.Log($"[DiscEvent] Red a anunciar configurada virtualmente: {network}");
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"[DiscEvent] Disco AnunciarRed (16) requiere DynamicRoutingActivity activa");
                    }
                    break;
                }
                case DiscType.Costo:
                {
                    // Disco 17 - Costo: Configurar costo OSPF personalizado
                    var dynAct = UnityEngine.Object.FindAnyObjectByType<DynamicRoutingActivity>();
                    if (dynAct != null)
                    {
                        int cost = 15;
                        dynAct.SetLinkCost(cost);
                        UnityEngine.Debug.Log($"[DiscEvent] Costo OSPF configurado virtualmente: {cost}");
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"[DiscEvent] Disco Costo (17) requiere DynamicRoutingActivity activa");
                    }
                    break;
                }
                case DiscType.BW:
                {
                    // Disco 18 - BW: Configurar ancho de banda para calculo OSPF
                    var dynAct = UnityEngine.Object.FindAnyObjectByType<DynamicRoutingActivity>();
                    if (dynAct != null)
                    {
                        int bw = 100;
                        dynAct.SetBandwidth(bw);
                        UnityEngine.Debug.Log($"[DiscEvent] Ancho de banda configurado virtualmente: {bw} Mbps");
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"[DiscEvent] Disco BW (18) requiere DynamicRoutingActivity activa");
                    }
                    break;
                }
                case DiscType.Protocolo:
                {
                    var dynAct = UnityEngine.Object.FindAnyObjectByType<DynamicRoutingActivity>();
                    if (dynAct != null)
                    {
                        dynAct.SetProtocol(RoutingProtocol.EIGRP);
                        UnityEngine.Debug.Log($"[DiscEvent] Protocolo EIGRP seleccionado via disco virtual");
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Si el RouteBuilderState del router esta completo (destino, mascara, nextHop,
        /// interfaz), aplica la ruta estatica al router y resetea el builder.
        /// En caso contrario, registra el estado parcial en consola.
        /// </summary>
        /// <param name="router">Nodo router al que asociar la ruta.</param>
        /// <param name="builder">Estado transitorio de construccion de ruta.</param>
        /// <param name="topology">Instancia de TopologyManager.</param>
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

        /// <summary>
        /// Agrega una ruta por defecto (0.0.0.0/0 -> 192.168.1.254) al router.
        /// Corresponde al disco de tipo IpRoute.
        /// </summary>
        /// <param name="router">Router destino de la ruta por defecto.</param>
        /// <param name="topology">Instancia de TopologyManager.</param>
        private void HandleIpRouteDisc(NetworkNode router, TopologyManager topology)
        {
            router.RoutingTable.AddStaticRoute("0.0.0.0", "0.0.0.0", "192.168.1.254", "G0/0");
            UnityEngine.Debug.Log($"[DiscEvent] Ruta por defecto agregada a {router.Name}: 0.0.0.0/0 -> 192.168.1.254");
        }

        /// <summary>
        /// Obtiene la siguiente interfaz disponible para un router basado en
        /// las entradas de ruta existentes, rotando entre las interfaces disponibles.
        /// </summary>
        /// <param name="router">Router del cual obtener la interfaz.</param>
        /// <returns>Nombre de la interfaz (ej: "G0/0", "G0/1").</returns>
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

        /// <summary>
        /// Determina si un tipo de disco corresponde a un disco de configuracion
        /// de enrutamiento (IDs 7-18) vs un disco fisico (1-3).
        /// </summary>
        /// <param name="type">Tipo de disco a evaluar.</param>
        /// <returns>True si es un disco de configuracion de routing.</returns>
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
                case DiscType.Protocolo:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Actualiza la posicion de un nodo en TopologyManager y recrea enlaces
        /// automaticos si autoConnectLinks esta activado.
        /// </summary>
        /// <param name="discId">Identificador unico del disco movido.</param>
        /// <param name="position">Nueva posicion en coordenadas Canvas.</param>
        private void HandleDiscMoved(int discId, Vector2 position)
        {
            var topologyManager = Object.FindAnyObjectByType<TopologyManager>();
            if (topologyManager != null)
            {
                topologyManager.UpdateNodePosition(discId, position);

                if (autoConnectLinks)
                {
                    CheckAndCreateLinks(discId, position);
                }
            }
        }

        /// <summary>
        /// Elimina el nodo asociado al disco de TopologyManager.
        /// </summary>
        /// <param name="discId">Identificador unico del disco removido.</param>
        private void HandleDiscRemoved(int discId)
        {
            var topologyManager = Object.FindAnyObjectByType<TopologyManager>();
            if (topologyManager != null)
            {
                topologyManager.RemoveNode(discId);
            }

            UnityEngine.Debug.Log($"[DiscEvent] Disco eliminado: {discId}");
        }

        /// <summary>
        /// Convierte un DiscType (Router/Switch/PC) al DeviceType correspondiente
        /// del namespace SimRedes.Network.
        /// </summary>
        /// <param name="discType">Tipo de disco desde DiscConfiguration.</param>
        /// <returns>DeviceType equivalente, o Unknown si no hay mapeo.</returns>
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

        /// <summary>
        /// Busca discos activos dentro del umbral de distancia (linkDistanceThreshold)
        /// y crea enlaces automaticos entre ellos en TopologyManager.
        /// </summary>
        /// <param name="discId">Disco de referencia para medir distancias.</param>
        /// <param name="position">Posicion del disco de referencia.</param>
        private void CheckAndCreateLinks(int discId, Vector2 position)
        {
            var topologyManager = Object.FindAnyObjectByType<TopologyManager>();
            if (topologyManager == null) return;

            var activeDiscs = Object.FindAnyObjectByType<TangibleDiscManager>().GetActiveDiscs();

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

        /// <summary>
        /// Desuscribe los eventos de TangibleDiscManager para evitar fugas de memoria.
        /// </summary>
        private void OnDestroy()
        {
            var tangibleManager = Object.FindAnyObjectByType<TangibleDiscManager>();
            if (tangibleManager != null)
            {
                tangibleManager.OnDiscPlaced -= HandleDiscPlaced;
                tangibleManager.OnDiscMoved -= HandleDiscMoved;
                tangibleManager.OnDiscRemoved -= HandleDiscRemoved;
            }
        }
    }
}