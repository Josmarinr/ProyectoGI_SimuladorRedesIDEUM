# DC-06: Diagrama de Paquetes (Namespaces)

> **Propósito**: Mostrar la organización del código en namespaces y sus dependencias.

```mermaid
graph TB
    subgraph SimRedes
        SceneSetup
        GameManager
        PointerClickHandler
        TouchScriptDisabler
    end

    subgraph SimRedes.Network
        TopologyManager
        NetworkNode
        NetworkLink
        RoutingTable
        ARPTable
        IPValidation
        DiscConfiguration
        VLANManager
        ACLManager
        NATManager
    end

    subgraph SimRedes.Tangible
        TangibleDiscManager
        TangibleBridge
        DiscEventHandler
        RouteBuilderState
        DebugDiscSimulator
    end

    subgraph SimRedes.Simulation
        ActivityLoader
        SceneCleanupService
        BuildTopologyActivity
        FindFaultActivity
        RoutingTablesActivity
        BestRouteActivity
        StaticRoutingActivity
        DynamicRoutingActivity
        DynamicRoutingProtocol
        PredefinedScenarios
        ScoringSystem
        SimulationControls
        RoutingSimulator
        PathCalculation
    end

    subgraph SimRedes.UI
        UIPanelFactory
        UIComponents
        NodeVisualizer
        PingVisualizer
        TopologyVisualizer
        MenuNavigator
        MainMenuManager
        LinkModeController
        PingModeController
        IPConfigController
        DevicePanelController
        NodeInteractionController
        ConnectivityTestPanel
        IDEUMConfigurator
    end

    subgraph SimRedes.Core
        AppLogger
    end

    subgraph "External"
        TE[TangibleEngine SDK]
        NUnit[NUnit 3.x]
    end

    subgraph "Tests.EditMode"
        TestIPValidation
        TestRoutingTable
        TestRouteBuilderState
        TestRoutePersistence
    end

    SimRedes --> SimRedes.Network
    SimRedes --> SimRedes.UI
    SimRedes --> SimRedes.Simulation
    SimRedes --> SimRedes.Tangible
    SimRedes --> SimRedes.Core

    SimRedes.Tangible --> SimRedes.Network
    SimRedes.Tangible --> TE

    SimRedes.Simulation --> SimRedes.Network
    SimRedes.Simulation --> SimRedes.UI
    SimRedes.Simulation --> SimRedes.Tangible

    SimRedes.UI --> SimRedes.Network

    SimRedes.Core --> SimRedes

    Tests.EditMode --> SimRedes.Network
    Tests.EditMode --> SimRedes.Tangible
    Tests.EditMode --> NUnit
```

## Resumen de Namespaces

| Namespace | Clases | Propósito |
|-----------|--------|-----------|
| `SimRedes` | 4 | Raíz: SceneSetup, GameManager, helpers |
| `SimRedes.Network` | 11 | Núcleo de networking: nodos, enlaces, tablas, IP |
| `SimRedes.Tangible` | 5 | Integración con hardware IDEUM |
| `SimRedes.Simulation` | 14 | Actividades, escenarios, puntajes, protocolos |
| `SimRedes.UI` | 14 | Paneles, visualización, controladores de interacción |
| `SimRedes.Core` | 1 | Utilidades transversales (AppLogger) |
| `Tests.EditMode.*` | 4 | Tests unitarios EditMode |

## Archivos Relacionados

- `Assets/Scripts/` — Código fuente principal
- `Assets/Editor/Tests/` — Tests unitarios EditMode
- `Assets/TangibleEngine/` — SDK externo de TangibleEngine
