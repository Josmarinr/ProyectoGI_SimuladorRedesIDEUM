# API — Catálogo de Eventos

> **Propósito**: Lista completa de eventos del sistema, publicadores y suscriptores.

---

## TopologyManager Events

| Evento | Tipo | Publicador | Suscriptores |
|--------|------|------------|--------------|
| `OnNodeAdded` | `Action<NetworkNode>` | `AddNode()` | `NodeVisualizer`, `BuildTopologyActivity`, `SceneSetup` |
| `OnNodeRemoved` | `Action<NetworkNode>` | `RemoveNode()` | `NodeVisualizer`, `BuildTopologyActivity` |
| `OnLinkAdded` | `Action<NetworkLink>` | `AddLink()` | `BuildTopologyActivity` |
| `OnLinkRemoved` | `Action<NetworkLink>` | `RemoveLink()` | — (no listeners directos) |
| `OnTopologyChanged` | `Action` | Todos los anteriores | `SceneSetup` → `DevicePanelController`, `NodeVisualizer`, `BuildTopologyActivity` |

## TangibleDiscManager Events

| Evento | Tipo | Publicador | Suscriptores |
|--------|------|------------|--------------|
| `OnDiscPlaced` | `Action<int, Vector2>` | `SimulateDiscPlaced()` | `DiscEventHandler` |
| `OnDiscMoved` | `Action<int, Vector2>` | `UpdateDiscPosition()` | `DiscEventHandler` |
| `OnDiscRemoved` | `Action<int>` | `SimulateDiscRemoved()` | `DiscEventHandler` |

## TangibleEngine Events (SDK Externo)

| Evento | Tipo | Publicador | Suscriptores |
|--------|------|------------|--------------|
| `TE.OnTangibleAdded` | `Action<Tangible>` | TangibleEngine Service | `TangibleBridge` |
| `TE.OnTangibleRemoved` | `Action<Tangible>` | TangibleEngine Service | `TangibleBridge` |
| `TE.OnTangibleUpdated` | `Action<Tangible>` | TangibleEngine Service | `TangibleBridge` |

> Estos eventos vienen del SDK externo `TangibleEngine` (en `Assets/TangibleEngine/`). El sistema se suscribe a ellos en el `Awake()` de `TangibleBridge`.

## DynamicRoutingProtocol Events

| Evento | Tipo | Publicador | Suscriptores |
|--------|------|------------|--------------|
| `OnProtocolLog` | `Action<string>` | `RunProtocolLoop()` | `DynamicRoutingActivity` (lambdas → panel UI) |
| `OnConvergence` | `Action` | `RunProtocolLoop()` | `DynamicRoutingActivity` (lambdas → panel UI) |

## Diagrama de Flujo de Eventos

```mermaid
graph LR
    subgraph "TangibleEngine SDK"
        TE_ADD[OnTangibleAdded]
        TE_UPD[OnTangibleUpdated]
        TE_REM[OnTangibleRemoved]
    end

    subgraph "TangibleDiscManager"
        DM_PLACE[OnDiscPlaced]
        DM_MOVE[OnDiscMoved]
        DM_REM[OnDiscRemoved]
    end

    subgraph "DiscEventHandler"
        DH_HANDLE[HandleDiscPlaced]
    end

    subgraph "TopologyManager"
        TM_ADD[OnNodeAdded]
        TM_REM[OnNodeRemoved]
        TM_LADD[OnLinkAdded]
        TM_LREM[OnLinkRemoved]
        TM_CHG[OnTopologyChanged]
    end

    subgraph "UI Layer"
        NV[NodeVisualizer]
        BT[BuildTopologyActivity]
        DP[DevicePanelController]
    end

    subgraph "DynamicRoutingProtocol"
        DRP_LOG[OnProtocolLog]
        DRP_CONV[OnConvergence]
    end

    TE_ADD --> TangibleBridge --> DM_PLACE
    TE_UPD --> TangibleBridge --> DM_MOVE
    TE_REM --> TangibleBridge --> DM_REM

    DM_PLACE --> DH_HANDLE
    DM_MOVE --> DH_HANDLE
    DM_REM --> DH_HANDLE

    DH_HANDLE --> TM_ADD
    DH_HANDLE --> TM_CHG

    TM_ADD --> NV
    TM_ADD --> BT

    TM_REM --> NV
    TM_REM --> BT

    TM_LADD --> BT

    TM_CHG --> DP
    TM_CHG --> NV
    TM_CHG --> BT

    DRP_LOG --> DynamicRoutingActivity --> PanelUI
    DRP_CONV --> DynamicRoutingActivity --> PanelUI
```

## Reglas de Subscripción

1. **Siempre desuscribirse en `OnDestroy()`** para evitar `MissingReferenceException`:

```csharp
private void OnDestroy() {
    if (topologyManager != null) {
        topologyManager.OnNodeAdded -= OnNodeAdded;
        topologyManager.OnTopologyChanged -= OnTopologyChanged;
    }
}
```

2. **Los lambdas capturan variables**: Usar variable local para evitar null:

```csharp
int capturedDiscId = node.DiscId;
AddClickEvent(trigger, () => OnNodeClicked(capturedDiscId));
```

3. **Eventos con `?.Invoke()`**: Todos los eventos usan el patrón seguro:

```csharp
OnNodeAdded?.Invoke(node);
OnTopologyChanged?.Invoke();
```
