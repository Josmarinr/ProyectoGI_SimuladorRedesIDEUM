# AGENTS.md - Guia para Agentes

## Proyecto: Simulador de Redes IDEUM

### Estructura del Proyecto

Assets/
  Scripts/
    Network/   TopologyManager, NetworkNode, NetworkLink, RoutingTable, ARPTable, IPValidation, RoutingProtocols, DiscConfiguration (18 discos), VLANManager, ACLManager, NATManager
    Tangible/  TangibleDiscManager, TangibleBridge, DiscEventHandler, RouteBuilderState, DebugDiscSimulator
    Simulation/ SceneSetup(~480L), ActivityLoader(~800L), SceneCleanupService, 7 actividades (BuildTopology, FindFault, RoutingTables, BestRoute, StaticRouting, DynamicRouting), PredefinedScenarios, ScoringSystem, SimulationControls, GameManager
    UI/        UIPanelFactory(~830L), ActivityPanelFactory(~690L), ConfigPanelFactory(~790L), UIComponents(~680L), NodeVisualizer, PingVisualizer, ConnectivityTestPanel, 5 controllers (LinkMode, PingMode, IPConfig, DevicePanel, NodeInteraction)
    Core/      AppLogger (EnableLogging=false)
  Editor/Tests/ (209 tests EditMode, 14 suites)

### Tests Unitarios (209 EditMode)

| Suite | Tests | Suite | Tests |
|-------|:-----:|-------|:-----:|
| TestIPValidation | 18 | TestRoutingTable | 14 |
| TestRouteBuilderState | 15 | TestRoutePersistence | 3 |
| TestDiscToRouteIntegration | 10 | TestTopologyManager | 32 |
| TestDynamicRoutingProtocol | 16 | TestActivityLoader | 20 |
| TestDiscEventHandler | 18 | TestTangibleBridge | 15 |
| TestScoringSystem | 19 | TestBestRouteActivity | 12 |
| TestSceneCleanupService | 4 | TestPredefinedScenarios | 13 |

### Comandos Debug (Keyboard)

| Tecla | Accion | Tecla | Accion |
|-------|--------|-------|--------|
| 1 | Router | 2 | Switch |
| 3 | PC | 4 | Modo CONEXION |
| 5 | Fallo | C | Limpiar |
| P | Ping | R | Eliminar |
| ESC | Volver/Cerrar panel | | |

### Datos Clave de Arquitectura

- **Discos**: Solo 3 fisicos (Router=1, Switch=2, PC=3). IDs 4-18 virtuales (actividades). Routing config (7-18) via botones en StaticRoutingActivity/DynamicRoutingActivity.
- **DebugDiscSimulator**: `enableSimulation=false` (produccion). Activar para testing por teclado.
- **TangibleBridge**: Mapea tangibleId->uniqueId (Dictionary). Coordenadas TE (1920x1080) -> Canvas (4096x2160). Fallback silencioso si no hay servicio TE.
- **Input System**: Package 1.19.0, modo Both. Migracion completa -- 0 usos `Input.GetKeyDown()`.
- **RouteBuilderState**: Estado transitorio (DestNetwork, SubnetMask, NextHop, OutInterface, Protocol) por router. IsComplete(4 campos) -> ApplyToRouter() -> AddStaticRoute.
- **Paneles UI**: TopologyInfo (top-right), Devices (top-left), Score (bottom-right). Centrales: Scenarios/VLAN/ACL/NAT/DiscLegend.
- **Escena principal**: Assets/Main.unity. NO usar GetStarted_Scene.unity (sin SceneSetup).
- **Build**: Windows x86_64 (IDEUM). Testing: enableSimulation=true + build local.
- **ActivityLoader.SelectActivity()**: NO destruye TopologyManager -- rutas persisten entre actividades.
- **3 Factories**: UIPanelFactory (navegacion/info), ActivityPanelFactory (actividades), ConfigPanelFactory (config red). 4 helpers internal static compartidos.
- **0 bugs criticos activos**. Roadmap en ROADMAP.md, historial en ROADMAP_HISTORY.md.
- **autoConnectLinks=true**: discos se conectan si estan a <300px.

### Sistema de Agentes

| Agente | Rol | Permisos |
|--------|-----|----------|
| `main` | Coordinador autonomo (primary) | task, edit, bash |
| `architect` | Disena planes de impl. (read-only) | edit: deny |
| `programmer` | Implementa codigo | edit, bash |
| `reviewer` | Revisa codigo (read-only) | edit: deny |
| `tester` | Ejecuta tests | bash |
| `builder` | Build & deploy | edit, bash |

Flujo: main -> architect -> programmer -> reviewer -> tester (tareas complejas).
main tiene iniciativa propia: lee ROADMAP.md al iniciar, propone trabajo.

### Contexto de Propuesta (bajo demanda)
`docs/proposal-context.md` contiene el texto completo de la propuesta academica.
NO se carga automaticamente. Leer solo cuando el usuario pregunte "que falta de la propuesta".
