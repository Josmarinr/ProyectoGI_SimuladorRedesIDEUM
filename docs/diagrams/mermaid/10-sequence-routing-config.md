# DS-02: Secuencia — Configurar Ruta con Discos 7-18

> **Propósito**: Mostrar cómo un estudiante configura una ruta estática en un router colocando discos de routing (IDs 7-18) que construyen un RouteBuilderState parcial hasta completar los 4 campos necesarios.

```mermaid
sequenceDiagram
    participant Est as Estudiante
    participant DEH as DiscEventHandler
    participant RBS as RouteBuilderState[router100]
    participant TM as TopologyManager
    participant Router as NetworkNode(router100).RoutingTable

    Note over Est,Router: === Paso 1: Colocar disco RedDestino (ID 7) ===
    Est->>DEH: HandleRoutingConfigDisc(7, pos, config, topology)
    DEH->>TM: FindNodesNear(pos, radius=150)
    TM-->>DEH: [router100, ...]
    DEH->>DEH: router = router100
    DEH->>RBS: DestinationNetwork = "192.168.1.0"
    DEH->>RBS: TryAddRoute(router, builder, topology)
    Note right of RBS: IsComplete? Destino=✓, Mask=✗, NextHop=✗, IF=✗ → false
    RBS-->>DEH: Log estado parcial

    Note over Est,Router: === Paso 2: Colocar disco Mascara (ID 13) ===
    Est->>DEH: HandleRoutingConfigDisc(13, pos, config, topology)
    DEH->>RBS: SubnetMask = "255.255.255.0"
    DEH->>RBS: TryAddRoute(router, builder, topology)
    Note right of RBS: IsComplete? Destino=✓, Mask=✓, NextHop=✗, IF=✗ → false

    Note over Est,Router: === Paso 3: Colocar disco ProximoSalto (ID 14) ===
    Est->>DEH: HandleRoutingConfigDisc(14, pos, config, topology)
    DEH->>RBS: NextHop = "192.168.1.254"
    DEH->>RBS: TryAddRoute(router, builder, topology)
    Note right of RBS: IsComplete? Destino=✓, Mask=✓, NextHop=✓, IF=✗ → false

    Note over Est,Router: === Paso 4: Colocar disco InterfazSalida (ID 9) ===
    Est->>DEH: HandleRoutingConfigDisc(9, pos, config, topology)
    DEH->>RBS: OutInterface = "G0/0"
    DEH->>RBS: TryAddRoute(router, builder, topology)
    Note right of RBS: IsComplete? Todos ✓ → true!

    RBS->>RBS: ApplyToRouter(topology)
    RBS->>Router: AddStaticRoute("192.168.1.0", "255.255.255.0", "192.168.1.254", "G0/0")
    Note right of Router: Entrada agregada con Protocol="Static"
    RBS->>RBS: Reset()
    Note right of RBS: Campos limpios, Protocol preservado

    Note over Est,Router: Ruta configurada ✓
```

## Variante: Disco IpRoute (ID 11) — Ruta por Defecto

```mermaid
sequenceDiagram
    participant Est as Estudiante
    participant DEH as DiscEventHandler
    participant Router as NetworkNode.RoutingTable

    Est->>DEH: HandleRoutingConfigDisc(11, pos, config, topology)
    Note right of DEH: IpRoute → default route
    DEH->>Router: AddStaticRoute("0.0.0.0", "0.0.0.0", "192.168.1.254", "G0/0")
    Note right of Router: Ruta por defecto agregada
```

## Estados de RouteBuilderState

| Estado | DestNetwork | SubnetMask | NextHop | OutInterface | IsComplete |
|--------|:-----------:|:----------:|:-------:|:------------:|:----------:|
| Vacío | ✗ | ✗ | ✗ | ✗ | false |
| RedDestino puesto | ✓ | ✗ | ✗ | ✗ | false |
| + Mascara | ✓ | ✓ | ✗ | ✗ | false |
| + ProximoSalto | ✓ | ✓ | ✓ | ✗ | false |
| + InterfazSalida | Si | Si | Si | Si | **true** |
| Tras ApplyToRouter | Limpio | Limpio | Limpio | Limpio | false |

## Archivos Relacionados

| Archivo | Rol |
|---------|-----|
| `Assets/Scripts/Tangible/DiscEventHandler.cs` | `HandleRoutingConfigDisc()` + `TryAddRoute()` |
| `Assets/Scripts/Tangible/RouteBuilderState.cs` | Estado transitorio de 4 campos + protocolo |
| `Assets/Scripts/Network/TopologyManager.cs` | `FindNodesNear()` para detectar router cercano |
| `Assets/Scripts/Network/RoutingTable.cs` | `AddStaticRoute()` destino final |
