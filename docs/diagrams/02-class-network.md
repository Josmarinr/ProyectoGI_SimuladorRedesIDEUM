# DC-02: Diagrama de Clases — Network Layer

> **Propósito**: Mostrar la estructura del núcleo de networking: nodos, enlaces, tablas de enrutamiento, y managers de red.

```mermaid
classDiagram
    class TopologyManager {
        +static TopologyManager Instance
        -List~NetworkNode~ nodes
        -List~NetworkLink~ links
        +VLANManager VLAN
        +ACLManager ACL
        +NATManager NAT
        +event OnNodeAdded(NetworkNode)
        +event OnNodeRemoved(NetworkNode)
        +event OnLinkAdded(NetworkLink)
        +event OnLinkRemoved(NetworkLink)
        +event OnTopologyChanged()
        +AddNode(discId, type, pos) NetworkNode
        +RemoveNode(node) void
        +AddLink(node1, node2) NetworkLink
        +RemoveLink(link) void
        +FindNodesNear(pos, radius) List~NetworkNode~
        +FindPath(src, dst) List~NetworkNode~
        +CheckConnectivity(src, dst) bool
        +GetAllNodes() List~NetworkNode~
        +GetAllLinks() List~NetworkLink~
        +ClearTopology() void
    }

    class NetworkNode {
        +int Id
        +string Name
        +DeviceType Type
        +string IpAddress
        +string SubnetMask
        +Vector2 Position
        +int DiscId
        +bool IsActive
        +bool IsAdminDown
        +string ConfiguredFault
        +int VlanId
        +RoutingTable RoutingTable
        +ARPTable ArpTable
        +SetInterfaceIP(ip, mask) void
        +GetDefaultGateway() string
        +GetNetworkAddress() string
        +HasFault() bool
        +IsValidConfiguration() bool
    }

    class NetworkLink {
        +int Id
        +NetworkNode SourceNode
        +NetworkNode DestinationNode
        +string SourceInterface
        +string DestinationInterface
        +LinkType Type
        +int Bandwidth
        +int Delay
        +int Cost
        +bool IsActive
        +bool IsConnected
        +SetFault(type) void
        +ClearFault() void
        +IsFunctional() bool
    }

    class RoutingTable {
        -List~RoutingEntry~ entries
        +AddStaticRoute(dest, mask, nextHop, iface) void
        +AddRipRoute(dest, nextHop, iface, hops) void
        +AddOspfRoute(dest, nextHop, iface, cost) void
        +FindBestRoute(destIP) RoutingEntry
        +GetAllEntries() List~RoutingEntry~
        +Clear() void
        +GetTableSummary() string
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
        +GetAllEntries() List~ARPEntry~
        +Clear() void
    }

    class ARPEntry {
        +string IPAddress
        +string MACAddress
        +string Interface
        +float Age
    }

    class IPValidation {
        +static IsValidIP(ip) bool
        +static IsValidSubnetMask(mask) bool
        +static GetPrefixLength(mask) int
        +static ValidateIPField(ip) string
        +static IsInSameNetwork(ip1, mask1, ip2) bool
        +static GetNetworkAddress(ip, mask) string
        +static GetBroadcastAddress(ip, mask) string
        +static GetGatewayFromIP(ip) string
    }

    class DiscConfiguration {
        +int DiscId
        +DiscType Type
        +string Label
        +Color DisplayColor
        +string Description
        +static GetConfiguration(discId) DiscConfiguration
    }

    class VLANManager {
        +CreateVLAN(id) void
        +DeleteVLAN(id) void
        +AssignToVLAN(node, vlanId) void
        +GetNodeVLAN(node) int
        +CanCommunicate(src, dst) bool
        +GetAllVLANs() List~int~
    }

    class ACLManager {
        +CreateACL(name) void
        +AddRule(acl, rule) void
        +RemoveRule(acl, seq) void
        +CheckPacket(acl, src, dst) bool
        +GetRules(acl) List~ACLRule~
    }

    class ACLRule {
        +int Sequence
        +ACLAction Action
        +ACLProtocol Protocol
        +string SourceIP
        +string DestIP
        +int SourcePort
        +int DestPort
        +bool IsEnabled
    }

    class NATManager {
        +bool IsEnabled
        +SetPublicIP(ip) void
        +AddStaticNAT(inIP, outIP) void
        +AddDynamicNAT(inIP, pool) void
        +AddPAT(inIP, port) void
        +TranslatePacket(inIP) string
        +GetNATTable() List~NATEntry~
    }

    class NATEntry {
        +string InternalIP
        +string ExternalIP
        +int InternalPort
        +int ExternalPort
        +NATType Type
    }

    TopologyManager "1" *-- "many" NetworkNode : contiene
    TopologyManager "1" *-- "many" NetworkLink : contiene
    TopologyManager "1" *-- "1" VLANManager
    TopologyManager "1" *-- "1" ACLManager
    TopologyManager "1" *-- "1" NATManager
    NetworkNode "1" *-- "1" RoutingTable
    NetworkNode "1" *-- "1" ARPTable
    RoutingTable "1" *-- "many" RoutingEntry
    ARPTable "1" *-- "many" ARPEntry
    NetworkLink "2" --> "1" NetworkNode : conecta
    ACLManager "1" *-- "many" ACLRule
    NATManager "1" *-- "many" NATEntry
```

## Archivos Relacionados

| Archivo | Namespace |
|---------|-----------|
| `Assets/Scripts/Network/TopologyManager.cs` | `SimRedes.Network` |
| `Assets/Scripts/Network/NetworkNode.cs` | `SimRedes.Network` |
| `Assets/Scripts/Network/NetworkLink.cs` | `SimRedes.Network` |
| `Assets/Scripts/Network/RoutingTable.cs` | `SimRedes.Network` |
| `Assets/Scripts/Network/ARPTable.cs` | `SimRedes.Network` |
| `Assets/Scripts/Network/IPValidation.cs` | `SimRedes.Network` |
| `Assets/Scripts/Network/DiscConfiguration.cs` | `SimRedes.Network` |
| `Assets/Scripts/Network/VLANManager.cs` | `SimRedes.Network` |
| `Assets/Scripts/Network/ACLManager.cs` | `SimRedes.Network` |
| `Assets/Scripts/Network/NATManager.cs` | `SimRedes.Network` |
