# DE-01: Diagrama de Estados — RouteBuilderState

> **Propósito**: Mostrar el ciclo de vida del estado transitorio que acumula campos de ruta cuando se colocan discos de routing (IDs 7-18) sobre un router.

```mermaid
stateDiagram-v2
    [*] --> Empty: Nuevo RouteBuilderState<br/>creado para router

    Empty --> Partial_1Field: Disco RedDestino (ID 7)<br/>o Destino (ID 12)<br/>→ DestinationNetwork set
    Empty --> Partial_1Field_IF: Disco InterfazSalida (ID 9)<br/>→ OutInterface set

    Partial_1Field --> Partial_2Fields: Disco Mascara (ID 13)<br/>→ SubnetMask set
    Partial_1Field_IF --> Partial_2Fields_IF: Disco RedDestino<br/>→ DestinationNetwork set

    Partial_2Fields --> Partial_3Fields: Disco ProximoSalto (ID 14)<br/>→ NextHop set
    Partial_2Fields_IF --> Partial_3Fields: Disco Mascara<br/>→ SubnetMask set

    Partial_3Fields --> Complete: Disco InterfazSalida (ID 9)<br/>→ OutInterface set<br/>IsComplete = true ✅

    Complete --> Applied: TryAddRoute → IsComplete<br/>→ ApplyToRouter()
    Applied --> Empty: Reset()<br/>(Protocol se conserva)

    note right of Complete: 4 campos requeridos:<br/>1. DestinationNetwork<br/>2. SubnetMask<br/>3. NextHop<br/>4. OutInterface

    note right of Applied: router.RoutingTable.AddStaticRoute()<br/>ejecutado con los 4 campos

    Empty --> ModoRouting: Disco ModoEnrutamiento (ID 10)<br/>Toggle Static ↔ RIP ↔ OSPF ↔ EIGRP
    ModoRouting --> Empty: Siguiente disco

    state Empty {
        [*] --> WaitingForDiscs
        WaitingForDiscs --> FieldReceived
    }

    state Complete {
        ReadyToApply --> Applied
    }
```

## Campos del RouteBuilderState

| Campo | Set por disco | Requerido para IsComplete |
|-------|:------------:|:-------------------------:|
| `DestinationNetwork` | 7 (RedDestino), 12 (Destino) | ✅ Sí |
| `SubnetMask` | 13 (Mascara) | ✅ Sí |
| `NextHop` | 14 (ProximoSalto) | ✅ Sí |
| `OutInterface` | 9 (InterfazSalida) | ✅ Sí |
| `Protocol` | 10 (ModoEnrutamiento) | ❌ No (default "Static") |

## Reglas de Transición

1. **IsComplete** = `true` cuando los 4 campos (Dest, Mask, NextHop, IF) son non-null y no vacíos
2. **ApplyToRouter** llama `router.RoutingTable.AddStaticRoute(dest, mask, nextHop, iface)` y luego asigna el Protocol
3. **Reset** limpia los 4 campos pero preserva `Protocol` (para mantener RIP/OSPF/EIGRP entre ciclos)
4. **Protocol** default es `"Static"`, modificable con disco ModoEnrutamiento (ID 10) o seleccionando RIP/OSPF/EIGRP

## Archivos Relacionados

- `Assets/Scripts/Tangible/RouteBuilderState.cs` — Implementación del estado
- `Assets/Scripts/Tangible/DiscEventHandler.cs` — `HandleRoutingConfigDisc()` + `TryAddRoute()`
- `Assets/Scripts/Network/RoutingTable.cs` — `AddStaticRoute()`
