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
        +ARPTable ARPTable
        +ConfigureIP(ip, mask) void
    }

    class NetworkLink {
        +NetworkNode SourceNode
        +NetworkNode DestinationNode
        +string SourceInterface
        +string DestInterface
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
        +string RouterIP
        +string SubnetMask
        +AddEntry(destNet, mask, nextHop, interface, protocol) void
        +RemoveEntry(index) void
        +GetAllEntries() List~RoutingEntry~
        +FindLongestPrefixMatch(ip) RoutingEntry
        +Clear() void
    }

    class RoutingEntry {
        +string DestNetwork
        +string SubnetMask
        +string NextHop
        +string OutInterface
        +string Protocol
    }

    class ARPTable {
        -Dictionary~string,string~ entries
        +AddEntry(ip, mac) void
        +RemoveEntry(ip) void
        +Resolve(ip) string
        +GetAllEntries() Dictionary
        +Clear() void
    }

    class ARPEntry {
        +string IPAddress
        +string MACAddress
    }

    class IPValidation {
        +IsValidIP(ip) bool
        +IsValidMask(mask) bool
        +GetNetworkAddress(ip, mask) string
        +GetBroadcastAddress(ip, mask) string
        +IsSameSubnet(ip1, mask1, ip2, mask2) bool
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
        +string Type
        +Dictionary~string,string~ Config
        +SetConfig(key, value) void
        +GetConfig(key) string
    }

    class VLANManager {
        +int VlanId
        +string VlanName
        +List~int~ MemberPorts
        +CreateVLAN(id, name) void
        +AssignPort(vlanId, port) void
        +RemoveVLAN(id) void
        +GetVLANMembers(id) List~int~
    }

    class ACLManager {
        -List~ACLRule~ rules
        +AddRule(aclRule) void
        +RemoveRule(index) void
        +CheckTraffic(ip, port) bool
        +GetAllRules() List~ACLRule~
        +Clear() void
    }

    class ACLRule {
        +int RuleId
        +string Action
        +string SourceIP
        +string DestIP
        +int Port
        +string Protocol
    }

    class NATManager {
        -List~NATEntry~ translations
        +string InsideLocal
        +string InsideGlobal
        +string OutsideLocal
        +string OutsideGlobal
        +AddTranslation(entry) void
        +RemoveTranslation(index) void
        +Translate(ip, type) string
    }

    class NATEntry {
        +string InsideLocal
        +string InsideGlobal
        +string Type
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
