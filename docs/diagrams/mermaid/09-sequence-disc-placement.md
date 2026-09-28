# DS-01: Secuencia — Colocar Disco Físico

> **Propósito**: Mostrar el flujo completo desde que un estudiante coloca un disco físico en la mesa IDEUM hasta que el nodo aparece visualizado en pantalla.

```mermaid
sequenceDiagram
    participant Estudiante
    participant Mesa as Mesa IDEUM
    participant TE as TangibleEngine SDK
    participant TB as TangibleBridge
    participant TDM as TangibleDiscManager
    participant DEH as DiscEventHandler
    participant TM as TopologyManager
    participant NV as NodeVisualizer
    participant DP as DevicePanelController

    Estudiante->>Mesa: Coloca disco Router (ID 1) en posición (500,400)
    Mesa->>TE: Señal RFID/patrón detectado
    TE->>TB: OnTangibleAdded(tangible)
    Note right of TB: tangible.Id = 42<br/>tangible.PatternId = 1<br/>tangible.Position = (500,400)

    TB->>TB: MapPatternToDiscType(1)
    Note right of TB: PatternId 1 → DiscType.Router

    TB->>TB: ConvertToCanvasPosition((500,400))
    Note right of TB: Escala 1920x1080 → 4096x2160<br/>Resultado: (1067, 800)

    TB->>TDM: SimulateDiscPlaced(DiscType.Router, (1067,800))
    Note right of TDM: uniqueId = BASE_DISC_ID + counter<br/>= 100 + 0 = 100

    TDM-->>TB: return 100
    TB->>TB: tangibleIdToUniqueId[42] = 100

    TDM->>DEH: OnDiscPlaced(100, (1067,800))

    DEH->>DEH: IsRoutingConfigDisc(Router)
    Note right of DEH: Router es ID 1, NO es routing → procede

    DEH->>TM: AddNode(discId=100, DeviceType.Router, pos=(1067,800))

    TM->>TM: Crear NetworkNode
    Note right of TM: node.Id = auto<br/>node.DiscId = 100<br/>node.Type = Router<br/>node.Position = (1067,800)

    TM-->>DEH: return node
    TM->>NV: OnNodeAdded(node)

    NV->>NV: CreateNodeVisual(node)
    Note right of NV: Crea GameObject circular<br/>Color: Azul (DeviceType.Router)

    TM->>TM: OnTopologyChanged()
    TM->>DP: RefreshDevicesPanel()

    DEH->>DEH: autoConnectLinks?
    Note right of DEH: ¿Hay otro disco a <300px?

    alt Disco cercano encontrado
        DEH->>TM: AddLink(node1, node2)
        TM->>NV: OnLinkAdded(link)
        NV->>NV: DrawLinks()
    end

    Note over Estudiante,DP: Nodo visible en pantalla
```

## Variante: Debug sin Discos Físicos

```mermaid
sequenceDiagram
    participant Dev as Desarrollador
    participant DDS as DebugDiscSimulator
    participant TDM as TangibleDiscManager
    participant DEH as DiscEventHandler
    participant TM as TopologyManager
    participant NV as NodeVisualizer

    Dev->>DDS: Presiona tecla 1
    DDS->>DDS: SimulateDiscAt(DiscType.Router, randomPos)
    DDS->>TDM: SimulateDiscPlaced(Router, pos)
    TDM->>DEH: OnDiscPlaced(id, pos)
    DEH->>TM: AddNode(...)
    TM->>NV: OnNodeAdded(node)
    Note over NV: Misma lógica que disco físico
```

## Archivos Relacionados

| Archivo | Rol en el flujo |
|---------|----------------|
| `Assets/Scripts/Tangible/TangibleBridge.cs` | Puente TE → DiscManager |
| `Assets/Scripts/Tangible/TangibleDiscManager.cs` | IDs únicos, estado de discos |
| `Assets/Scripts/Tangible/DiscEventHandler.cs` | Enrutar disco a nodo o routing |
| `Assets/Scripts/Tangible/DebugDiscSimulator.cs` | Simulación por teclado |
| `Assets/Scripts/Network/TopologyManager.cs` | Crear nodo en topología |
| `Assets/Scripts/UI/NodeVisualizer.cs` | Visualizar nodo en canvas |
