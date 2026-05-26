# DCMP-01: Diagrama de Componentes

> **Propósito**: Mostrar la arquitectura del sistema en capas, sus componentes y las dependencias entre ellos.

```mermaid
graph TB
    subgraph "Capa Física - Mesa IDEUM"
        DISC[Discos Físicos PUCs<br/>6 discos táctiles + 12 routing]
        TE_SVC[TangibleEngine Service<br/>TCP localhost:4949]
    end

    subgraph "Capa de Integración - Tangible"
        TE[TangibleEngine SDK<br/>Eventos OnTangible*]
        TB[TangibleBridge<br/>Mapeo tangibleId→uniqueId<br/>Conversión coordenadas]
        TDM[TangibleDiscManager<br/>Estado discos activos<br/>Eventos OnDisc*]
        DEH[DiscEventHandler<br/>Routing: 7-18 vs Físicos: 1-6<br/>Auto-conectar enlaces]
        RBS[RouteBuilderState<br/>Estado parcial de ruta<br/>IsComplete→ApplyToRouter]
        DDS[DebugDiscSimulator<br/>Teclado: 1,2,3,4,5,C,P,R<br/>enableSimulation toggle]
    end

    subgraph "Capa de Red - Network"
        TM[TopologyManager<br/>Singleton<br/>Nodos + Enlaces + Pathfinding]
        NN[NetworkNode<br/>Router/Switch/PC<br/>RoutingTable + ARPTable]
        NL[NetworkLink<br/>Conexiones<br/>Faults]
        RT[RoutingTable<br/>FindBestRoute<br/>Static/RIP/OSPF]
        IPV[IPValidation<br/>Estático<br/>Validación + Cálculo]
        VLAN[VLANManager<br/>Redes Virtuales<br/>Aislamiento]
        ACL[ACLManager<br/>Listas de Acceso<br/>Permit/Deny]
        NAT[NATManager<br/>Traducción<br/>Static/Dynamic/PAT]
    end

    subgraph "Capa de Actividades - Simulation"
        SS[SceneSetup<br/>Orquestador<br/>~379L]
        AL[ActivityLoader<br/>Dispatcher<br/>~800L]
        SCS[SceneCleanupService<br/>Singleton DontDestroyOnLoad]
        ACT[7 Actividades<br/>Topologia/Fallos/Rutas/Estatico/Dinamico/MejorRuta/Escenarios]
        DRP[DynamicRoutingProtocol<br/>RIP/OSPF<br/>Coroutine convergence]
        SC[ScoringSystem<br/>Singleton<br/>Puntajes + Notas]
        PRED[PredefinedScenarios<br/>5 escenarios]
    end

    subgraph "Capa de Presentación - UI"
        UPF[UIPanelFactory<br/>Fábrica de Paneles<br/>20+ métodos]
        UIC[UIComponents<br/>Helpers visuales<br/>Paleta de colores]
        NV[NodeVisualizer<br/>GameObjects nodos/enlaces]
        PV[PingVisualizer<br/>Animación paquetes]
        CTRL[5 Controladores<br/>LinkMode/PingMode/IPConfig/<br/>DevicePanel/NodeInteraction]
        MN[MENU<br/>MenuNavigator + MainMenuManager]
    end

    subgraph "Capa de Pruebas"
        TESTS[50 Tests EditMode<br/>IPValidation: 18<br/>RoutingTable: 14<br/>RouteBuilderState: 15<br/>RoutePersistence: 3]
    end

    subgraph "Herramientas - opencode"
        AGENTS[6 Agentes<br/>main/architect/programmer/<br/>reviewer/tester/builder]
        SKILLS[18 Skills<br/>Workflow, testing, UI,<br/>network, tangible, etc.]
    end

    %% Flujo físico
    DISC --> TE_SVC
    TE_SVC --> TE
    TE --> TB
    TB --> TDM
    TDM --> DEH
    DEH --> RBS
    DDS --> TDM

    %% Flujo de red
    DEH --> TM
    RBS --> RT
    TM --> NN
    TM --> NL
    TM --> VLAN
    TM --> ACL
    TM --> NAT

    %% Capa de actividades
    SS --> AL
    SS --> SCS
    AL --> ACT
    ACT --> DRP
    AL --> SC
    AL --> PRED

    %% Capa UI
    SS --> UPF
    SS --> CTRL
    UPF --> UIC
    NV --> UIC
    PV --> TM
    CTRL --> TM

    %% Pruebas
    TESTS --> IPV
    TESTS --> RT
    TESTS --> RBS
    TESTS --> TM

    %% opencode
    AGENTS --> SKILLS
```

## Capas del Sistema

| Capa | Tecnología | Responsabilidad |
|------|-----------|-----------------|
| **Física** | Mesa IDEUM 55" + PUCs | Input táctil, discos físicos |
| **Integración** | TangibleEngine SDK (C#) | Traducir eventos físicos a eventos de software |
| **Red** | C# plain classes + MonoBehaviour | Simulación de red: nodos, enlaces, routing |
| **Actividades** | MonoBehaviour | 7 actividades académicas + sistema de evaluación |
| **Presentación** | Unity Canvas + uGUI | Interfaz de usuario, visualización, animación |
| **Pruebas** | NUnit 3.x (EditMode) | 50 tests unitarios |
| **Herramientas** | opencode + skills | Automatización de desarrollo con agentes IA |

## Singleton Patterns

| Componente | Patrón | Persistencia |
|-----------|--------|-------------|
| `SceneCleanupService` | Singleton | `DontDestroyOnLoad` |
| `TopologyManager` | Singleton | Destruido al salir |
| `TangibleDiscManager` | Singleton | Destruido al salir |
| `PingVisualizer` | Singleton | Destruido al salir |
| `ScoringSystem` | Singleton | Destruido al salir |
| `MenuNavigator` | Singleton | Destruido al salir |
