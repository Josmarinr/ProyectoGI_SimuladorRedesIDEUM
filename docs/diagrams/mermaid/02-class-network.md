# DC-02: Diagrama de Clases — Network Layer

> **Propósito**: Mostrar la estructura del núcleo de networking del simulador.
> Dividido en 3 sub-diagramas: Núcleo de Red, Tablas de Enrutamiento/ARP, y Gestión Avanzada.

---

## DC-02a: Núcleo de Red (TopologyManager, Nodos, Enlaces)

```mermaid
classDiagram
    class TopologyManager {
        +static TopologyManager Instance
        -List~NetworkNode~ nodes
        -List~NetworkLink~ links
        +event OnNodeAdded(NetworkNode)
        +event OnNodeRemoved(NetworkNode)
        +event OnLinkAdded(NetworkLink)
        +event OnLinkRemoved(NetworkLink)
        +event OnTopologyChanged()
        +AddNode(discId, type, pos) NetworkNode
        +RemoveNode(node) void
        +AddLink(node1, node2) NetworkLink
        +RemoveLink(link) void
        +FindPath(src, dst) List~NetworkNode~
        +CheckConnectivity(src, dst) bool
        +GetAllNodes() List~NetworkNode~
        +ClearTopology() void
    }

    class NetworkNode {
        +int DiscId
        +string Name
        +DeviceType Type
        +Vector2 Position
        +RoutingTable RoutingTable
        +ARPTable ArpTable
        +SetInterfaceIP(iface, ip, mask) void
    }

    class NetworkLink {
        +NetworkNode SourceNode
        +NetworkNode DestinationNode
        +string SourceInterface
        +string DestinationInterface
        +IsFunctional() bool
    }

    TopologyManager --> NetworkNode : contiene
    TopologyManager --> NetworkLink : contiene
    NetworkLink --> NetworkNode : origen
    NetworkLink --> NetworkNode : destino
```

---

## DC-02b: Tablas de Enrutamiento, ARP y Validación IP

```mermaid
classDiagram
    class RoutingTable {
        -List~RoutingEntry~ entries
        +AddStaticRoute(destNetwork, mask, nextHop, iface) void
        +AddRipRoute(destNetwork, nextHop, iface, hops) void
        +AddOspfRoute(destNetwork, nextHop, iface, cost) void
        +AddEigrpRoute(destNetwork, nextHop, iface, metric) void
        +FindBestRoute(destinationIP) RoutingEntry
        +GetAllEntries() List~RoutingEntry~
        +GetTableSummary() string
        +Clear() void
        +FormatRouteLine(entry) string$
    }

    class RoutingEntry {
        +string DestinationNetwork
        +string SubnetMask
        +string NextHop
        +string OutInterface
        +int Metric
        +string Protocol
        +GetPrefixLength() int
    }

    class ARPTable {
        -List~ARPEntry~ entries
        +AddEntry(ip, mac, iface) void
        +FindEntry(ip) ARPEntry
        +AgeEntries(deltaTime) void
        +GetAllEntries() List~ARPEntry~
        +Clear() void
    }

    class ARPEntry {
        +string IPAddress
        +string MACAddress
    }

    class IPValidation {
        +IsValidIP(ip) bool
        +IsValidSubnetMask(mask) bool
        +GetNetworkAddress(ip, mask) string
        +GetBroadcastAddress(ip, mask) string
        +IsInSameNetwork(ip1, mask1, ip2) bool
    }

    RoutingTable --> RoutingEntry : contiene
    ARPTable --> ARPEntry : contiene
```

---

## DC-02c: Gestión Avanzada (VLAN, ACL, NAT)

```mermaid
classDiagram
    class DiscConfiguration {
        +int DiscId
        +DiscType Type
        +string Label
        +string Description
        +static GetConfiguration(discId) DiscConfiguration
    }

    class VLANManager {
        +CreateVLAN(vlanId) int
        +DeleteVLAN(vlanId) void
        +AssignToVLAN(node, vlanId) void
        +GetNodeVLAN(node) int
        +GetNodesInVLAN(vlanId) List~NetworkNode~
        +GetAllVLANs() List~int~
        +CanCommunicate(src, dst) bool
        +GetVLANSummary() string
    }

    class ACLManager {
        +List~ACLRule~ Rules
        +CreateACL(name) string
        +AddRule(aclName, rule) void
        +RemoveRule(aclName, sequence) void
        +CheckPacket(aclName, srcIp, dstIp, srcPort, dstPort, protocol) bool
        +GetRules(aclName) List~ACLRule~
        +DeleteACL(name) void
    }

    class ACLRule {
        +int Sequence
        +ACLAction Action
        +string SourceIP
        +string DestIP
        +int? SourcePort
        +int? DestPort
        +ACLProtocol Protocol
    }

    class NATManager {
        -List~NATEntry~ natTable
        -string publicIP
        -string routerIP
        +SetPublicIP(ip) void
        +AddStaticNAT(internalIP, externalIP) void
        +AddDynamicNAT(internalIP) void
        +AddPAT(internalIP, internalPort) void
        +LookupInternal(internalIP, port) NATEntry
        +LookupExternal(externalIP, port) NATEntry
        +TranslatePacket(srcIP, dstIP, srcPort, dstPort) bool
        +RemoveEntry(entry) void
        +GetNATTable() List~NATEntry~
    }

    class NATEntry {
        +string InternalIP
        +string ExternalIP
        +int? InternalPort
        +int? ExternalPort
        +NATType Type
    }

    DiscConfiguration --> VLANManager
    ACLManager --> ACLRule : contiene
    NATManager --> NATEntry : contiene
```

---

## Archivos Relacionados

- `Assets/Scripts/Network/TopologyManager.cs` — Gestor de topología
- `Assets/Scripts/Network/NetworkNode.cs` — Nodo de red
- `Assets/Scripts/Network/NetworkLink.cs` — Enlace entre nodos
- `Assets/Scripts/Network/RoutingTable.cs` — Tabla de enrutamiento
- `Assets/Scripts/Network/ARPTable.cs` — Tabla ARP
- `Assets/Scripts/Network/IPValidation.cs` — Validación de direcciones IP
- `Assets/Scripts/Network/DiscConfiguration.cs` — Configuración de discos virtuales
- `Assets/Scripts/Network/VLANManager.cs` — Gestión de VLANs
- `Assets/Scripts/Network/ACLManager.cs` — Gestión de ACLs
- `Assets/Scripts/Network/NATManager.cs` — Gestión de NAT
