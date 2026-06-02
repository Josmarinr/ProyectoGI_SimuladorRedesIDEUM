# DC-06: Diagrama de Paquetes (Namespaces)

> **Propósito**: Mostrar la organización del código en namespaces y sus dependencias.
>
> **Proyecto:** Simulador de Redes Tangible IDEUM
> **Autor:** Johan Sebastian Marin Rojas
> **Director:** Prof. Paulo Alonso Gaona Garcia
> **Grupo de Investigación:** Multimedia Interactiva
> **Universidad:** Universidad Distrital Francisco Jose de Caldas

```mermaid
graph TB
    subgraph S1["SimRedes"]
        SceneSetup
        GameManager
        PointerClickHandler
        TouchScriptDisabler
    end

    subgraph S2["SimRedes.Network"]
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

    subgraph S3["SimRedes.Tangible"]
        TangibleDiscManager
        TangibleBridge
        DiscEventHandler
        RouteBuilderState
        DebugDiscSimulator
    end

    subgraph S4["SimRedes.Simulation"]
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

    subgraph S5["SimRedes.UI"]
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

    subgraph S6["SimRedes.Core"]
        AppLogger
    end

    subgraph S7["External"]
        TE[TangibleEngine SDK]
        NUnit[NUnit 3.x]
    end

    subgraph S8["Tests.EditMode"]
        TestIPValidation
        TestRoutingTable
        TestRouteBuilderState
        TestRoutePersistence
    end

    S1 --> S2
    S1 --> S5
    S1 --> S4
    S1 --> S3
    S1 --> S6

    S3 --> S2
    S3 --> TE

    S4 --> S2
    S4 --> S5
    S4 --> S3

    S5 --> S2

    S6 --> S1

    S8 --> S2
    S8 --> S3
    S8 --> NUnit
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
