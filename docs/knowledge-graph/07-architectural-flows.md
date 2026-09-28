# 07 — Flujos de Datos Arquitectónicos

> **Propósito**: Mostrar cómo fluyen los datos entre capas del sistema en los escenarios más comunes.

---

## Flujo 1: Disco Físico → Nodo en Topología

```mermaid
sequenceDiagram
    participant DISC as Disco Físico
    participant TE as TangibleEngine
    participant TB as TangibleBridge
    participant TDM as TangibleDiscManager
    participant DEH as DiscEventHandler
    participant TM as TopologyManager
    participant NV as NodeVisualizer

    DISC->>TE: Colocar disco en mesa
    TE->>TB: OnTangibleAdded(tangibleId, x, y)
    TB->>TB: MapPatternToDiscType(pattern)
    TB->>TB: ConvertToCanvasPosition(x, y)
    TB->>TDM: AddDisc(uniqueId, type, canvasPos)
    TDM-->>DEH: OnDiscPlaced(uniqueId, type, pos)
    DEH->>DEH: ¿DiscId 1-3 (físico)?
    DEH->>TM: AddNode(discId, type, position)
    TM-->>NV: OnTopologyChanged
    NV->>NV: CreateNodeVisual(node)
```

---

## Flujo 2: Configuración de Routing por Discos (7-18)

```mermaid
sequenceDiagram
    participant DISC as Disco Routing (7-18)
    participant DEH as DiscEventHandler
    participant RBS as RouteBuilderState
    participant TM as TopologyManager
    participant RT as RoutingTable

    DISC->>DEH: OnDiscPlaced(discId=7, RedDestino)
    DEH->>DEH: ¿Es disco de configuración (7-18)?
    DEH->>TM: GetNode(nearbyRouterDiscId)
    DEH->>RBS: Crear/obtener builder para router
    DEH->>RBS: SetDestinationNetwork("192.168.1.0")

    DISC->>DEH: OnDiscPlaced(discId=8, Mascara)
    DEH->>RBS: SetSubnetMask("255.255.255.0")

    DISC->>DEH: OnDiscPlaced(discId=9, ProximoSalto)
    DEH->>RBS: SetNextHop("10.0.0.1")

    DISC->>DEH: OnDiscPlaced(discId=10, InterfazSalida)
    DEH->>RBS: SetOutInterface("eth0")

    DEH->>RBS: ¿IsComplete()?
    Note over RBS: 4 campos requeridos<br/>(Destination, Mask, NextHop, Interface)
    RBS-->>DEH: true
    DEH->>RBS: ApplyToRouter()
    RBS->>TM: GetNode(routerDiscId)
    RBS->>RT: AddStaticRoute(dest, mask, nextHop, iface)
```

---

## Flujo 3: Ping (Prueba de Conectividad)

```mermaid
sequenceDiagram
    participant USER as Usuario
    participant PM as PingModeController
    participant PV as PingVisualizer
    participant TM as TopologyManager
    participant IPV as IPValidation

    USER->>PM: TogglePingMode()
    PM->>PM: isPingModeActive = true
    USER->>TM: HandleNodeClick(srcDiscId)
    TM->>PM: HandlePingNodeClick(srcDiscId)
    PM->>PM: Primer nodo seleccionado
    USER->>TM: HandleNodeClick(dstDiscId)
    TM->>PM: HandlePingNodeClick(dstDiscId)
    PM->>TM: CheckConnectivity(src, dst)

    rect rgb(20, 30, 50)
        Note over TM: 1. BFS: ¿Hay path físico?
        Note over TM: 2. ¿Ambos tienen IP?
        Note over TM: 3. ¿Máscaras válidas?
        Note over TM: 4. ¿Misma subred?
    end

    TM-->>PM: bool connected
    PM->>PV: AnimatePing(src, dst, success)
    PV->>PV: Animación visual del paquete
```

---

## Flujo 4: Inicio de Actividad

```mermaid
sequenceDiagram
    participant USER as Usuario
    participant MMM as MainMenuManager
    participant AL as ActivityLoader (fachada → ActivityDispatcher)
    participant SS as SceneSetup
    participant UPF as UIPanelFactory
    participant APF as ActivityPanelFactory
    participant TM as TopologyManager

    USER->>MMM: OnActivitiesClicked()
    MMM->>UPF: CreateActivitiesPanel()
    USER->>MMM: SelectActivity(n)
    MMM->>AL: SelectActivity(n)

    alt Activity 0 (Build Topology)
        AL->>APF: CreateBuildTopologyInfoPanel()
        AL->>TM: AddNode() [escenario por defecto]
    else Activity 4 (Static Routing)
        AL->>APF: CreateStaticRoutingPanel()
    else Activity 5 (Dynamic Routing)
        AL->>APF: CreateDynamicRoutingPanel()
    else Activity 6 (Scenarios)
        AL->>APF: CreateScenariosPanel()
    end

    AL->>SS: ShowConnectivityPanel()
```

---

## Flujo 5: Routing Dinámico (RIP/OSPF/EIGRP)

```mermaid
sequenceDiagram
    participant DRA as DynamicRoutingActivity
    participant DRP as DynamicRoutingProtocol
    participant TM as TopologyManager
    participant RT as RoutingTable
    participant PV as PingVisualizer

    DRA->>DRP: SetProtocol(RIP/OSPF/EIGRP)
    DRA->>DRP: StartProtocol()
    DRP->>TM: GetAllNodes() [routers]

    loop Cada advertisementInterval (3s)
        DRP->>TM: GetAllLinks()
        alt RIP
            DRP->>DRP: SimulateRIPAdvertisement()
            Note over DRP: Métrica = hops
        else OSPF
            DRP->>DRP: SimulateOSPFAdvertisement()
            Note over DRP: Métrica = costo acumulado
        else EIGRP
            DRP->>DRP: SimulateEIGRPAdvertisement()
            Note over DRP: Métrica = f(BW, delay, K-values)
        end
        DRP->>RT: AddRipRoute/AddOspfRoute/AddEigrpRoute
        DRP-->>DRA: OnProtocolLog(msg)
    end

    DRP->>DRP: ¿Converged?
    DRP-->>DRA: OnConvergence()
```

---

## Flujo 6: Limpieza al Volver al Menú

```mermaid
sequenceDiagram
    participant USER as Usuario
    participant SIMC as SimulationControls
    participant SCS as SceneCleanupService
    participant TM as TopologyManager
    participant NV as NodeVisualizer
    participant PV as PingVisualizer
    participant LM as LinkModeController
    participant PM as PingModeController
    participant IPC as IPConfigController
    participant DPC as DevicePanelController

    USER->>SIMC: GoBackToMainMenu()
    SIMC->>SCS: ClearSimulation()

    par Destruir componentes
        SCS->>NV: Destroy()
        SCS->>PV: Destroy()
        SCS->>LM: Destroy()
        SCS->>PM: Destroy()
        SCS->>IPC: Destroy()
        SCS->>DPC: Destroy()
    and Limpiar topología
        SCS->>TM: ClearTopology()
    and Resetear estado
        SCS->>SCS: Destroy() [self]
    end

    SCS-->>USER: MainMenu visible
```

---

## Flujo 7: Auto-Conexión de Enlaces (Tangible)

```mermaid
sequenceDiagram
    participant TDM as TangibleDiscManager
    participant DEH as DiscEventHandler
    participant TM as TopologyManager

    TDM-->>DEH: OnDiscPlaced(discId, type, pos)
    DEH->>TM: GetAllNodes()
    DEH->>DEH: FindNodesNear(pos, linkDistanceThreshold=300)

    alt Hay nodo cercano compatible
        DEH->>TM: AddLink(existingNode, newNode)
        Note over DEH: Auto-connect si:<br/>1. Distancia < 300px<br/>2. Tipos compatibles<br/>3. No ya conectados
    else No hay nodo cercano
        Note over DEH: Solo crear nodo, sin enlace
    end
```

---

## Flujo 8: Build y Deploy

```mermaid
sequenceDiagram
    participant DEV as Desarrollador
    participant UNITY as Unity Editor
    participant BS as Build Settings
    participant EXE as Ejecutable

    DEV->>UNITY: Abrir proyecto (Assets/Main.unity)
    UNITY->>UNITY: Verificar escena activa = Main
    DEV->>UNITY: File → Build Settings
    BS->>BS: Platform = Windows x86_64
    BS->>BS: Scenes = Assets/Main.unity
    DEV->>BS: Build
    BS->>EXE: Generar ejecutable en Build/
    EXE-->>DEV: Ejecutable listo

    Note over DEV,EXE: Para testing local:<br/>DebugDiscSimulator.enableSimulation=true<br/>Teclas 1-5 para crear dispositivos

    Note over DEV,EXE: Para mesa IDEUM:<br/>Copiar Build/ a máquina Windows<br/>TouchScript habilitado (runtime)<br/>TangibleEngine service activo
```

---

## Diagrama de Capas con Dependencias

```mermaid
graph LR
    subgraph "Física"
        DISC[Discos IDEUM]
        TE[TangibleEngine]
    end

    subgraph "Integración"
        TB[TangibleBridge]
        TDM[TangibleDiscManager]
        DEH[DiscEventHandler]
        RBS[RouteBuilderState]
        DDS[DebugDiscSimulator]
    end

    subgraph "Red"
        TM[TopologyManager]
        NN[NetworkNode]
        NL[NetworkLink]
        RT[RoutingTable]
        IPV[IPValidation]
        VLAN[VLANManager]
        ACL[ACLManager]
        NAT[NATManager]
    end

    subgraph "Simulación"
        AL[ActivityLoader]
        DRP[DynamicRoutingProtocol]
        SC[ScoringSystem]
        PRED[PredefinedScenarios]
        ACT[7 Actividades]
    end

    subgraph "UI"
        UPF[UIPanelFactory]
        APF[ActivityPanelFactory]
        CPF[ConfigPanelFactory]
        UIC[UIComponents]
        NV[NodeVisualizer]
        PV[PingVisualizer]
        CTRL[5 Controllers]
    end

    DISC --> TE --> TB --> TDM --> DEH --> RBS
    DEH --> TM
    TM --> NN --> RT
    TM --> NL
    TM --> VLAN & ACL & NAT
    AL --> TM & DRP & SC & PRED & ACT
    UPF & APF & CPF --> UIC
    NV & PV --> TM
    CTRL --> TM
```
