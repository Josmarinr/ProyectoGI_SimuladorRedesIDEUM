# 04 — Mapa de Tests → Cubrimiento

> **Propósito**: Qué test cubre qué clase, qué métodos, y qué clases NO tienen test dedicado.

---

## Resumen

| Namespace | Suites | Tests | Cobertura |
|---|---|---|---|
| Network | 9 | 175 | Alta |
| Tangible | 3 | 48 | Alta |
| Simulation | 14 | 138 | Media |
| UI | 3 | 29 | Baja |
| **Total** | **29** | **390** | — |

---

## Network/ — 9 suites, 175 tests

### TestIPValidation.cs → `IPValidation` (18 tests)

| Método testado | Tests |
|---|---|
| `IsValidIP()` | 2 (válido + inválido) |
| `IsValidSubnetMask()` | 2 (válido + inválido) |
| `GetPrefixLength()` | 2 (conocido + desconocido) |
| `ValidateIPField()` | 2 (válido + inválido) |
| `ValidateMaskField()` | 2 (válido + inválido) |
| `IsInSameNetwork()` | 3 (misma, diferente, null) |
| `GetNetworkAddress()` | 2 (correcto + null) |
| `GetBroadcastAddress()` | 1 |
| `GetGatewayFromIP()` | 2 (correcto + null) |

### TestRoutingTable.cs → `RoutingTable`, `RoutingEntry` (20 tests)

| Método testado | Tests |
|---|---|
| `Constructor` | 1 |
| `AddStaticRoute()` | 2 (campos + múltiples) |
| `AddRipRoute()` | 1 |
| `AddOspfRoute()` | 1 |
| `AddEigrpRoute()` | 2 (uno + múltiples) |
| `FindBestRoute()` | 6 (exact, default, métrica, null, EIGRP, cross-protocol) |
| `GetPrefixLength()` | 2 (conocido + desconocido) |
| `GetAllEntries()` | 1 (copia defensiva) |
| `Clear()` | 1 |
| `GetTableSummary()` | 1 |
| `FormatRouteLine()` | 2 (static, RIP con prefijo/protocolo) |

### TestTopologyManager.cs → `TopologyManager` (39 tests)

| Método testado | Tests |
|---|---|
| `AddNode()` | 5 (Router, Switch, PC, duplicado, posición) |
| `RemoveNode()` | 3 (existe, no existe, cascade links) |
| `GetNode()` | 2 (existe, null) |
| `GetAllNodes()` | 1 (copia defensiva) |
| `AddLink()` | 3 (existe, duplicado, source inexistente) |
| `RemoveLink()` | 2 (existe, no existe) |
| `GetAllLinks()` | 1 |
| `UpdateNodePosition()` | 2 (existe, no existe) |
| `FindNodesNear()` | 2 (dentro radio, exacto) |
| `FindPath()` | 3 (conectado, desconectado, nodo único) |
| `CheckConnectivity()` | 4 (misma subred, diferente, sin link, switch transparente) |
| `GetRoutingTableSummary()` | 2 (con datos, vacío) |
| `FindLink()` | 7 (normal, invertido, sin enlace, null, por discId ×3) |
| `ClearTopology()` | 2 (con datos, vacío) |

### TestARPTable.cs → `ARPTable` (13 tests)

| Método testado | Tests |
|---|---|
| `Constructor` | 1 |
| `AddEntry()` | 4 (3 params, tipo dinámico, duplicado, 2 params) |
| `FindEntry()` | 3 (existe, null, vacío) |
| `GetAllEntries()` | 1 (copia defensiva) |
| `Clear()` | 2 (normal, vacío) |
| `AgeEntries()` | 1 |
| `GetTableSummary()` | 1 |

### TestVLANManager.cs → `VLANManager` (18 tests)

| Método testado | Tests |
|---|---|
| `Constructor` | 1 (VLAN 1 default) |
| `CreateVLAN()` | 4 (auto ID, incremento, específico, duplicado) |
| `DeleteVLAN()` | 4 (default, existente, con nodos, no existe) |
| `AssignToVLAN()` | 4 (asignado, auto-create, reasignar, múltiples) |
| `GetNodeVLAN()` | 1 (sin asignar) |
| `GetNodesInVLAN()` | 2 (no existe, copia) |
| `CanCommunicate()` | 2 (misma VLAN, diferente) |

### TestACLManager.cs → `ACLManager`, `ACLRule` (29 tests)

| Método testado | Tests |
|---|---|
| `ACLRule.DefaultValues` | 1 |
| `ACLRule.Matches()` | 12 (wildcard src/dst, src específico, protocolo, puerto src/dst, null) |
| `CreateACL()` | 2 (nuevo, duplicado) |
| `AddRule()` | 3 (auto-create, secuencia, incremento) |
| `RemoveRule()` | 1 |
| `CheckPacket()` | 6 (no ACL, vacío, deny, permit, primer match, implicit deny) |
| `DeleteACL()` | 1 |
| `CreateStandardRule()` | 1 |
| `CreateExtendedRule()` | 1 |
| `Rules` | 1 |

### TestNATManager.cs → `NATManager` (25 tests)

| Método testado | Tests |
|---|---|
| `Constructor` | 1 |
| `SetPublicIP()` | 1 |
| `SetRouterIP()` | 1 |
| `AddStaticNAT()` | 1 |
| `AddDynamicNAT()` | 1 |
| `AddPAT()` | 1 |
| `LookupInternal()` | 3 (con puerto, sin puerto, no existe) |
| `LookupExternal()` | 2 (existe, no existe) |
| `TranslatePacket()` | 5 (outgoing match/miss, incoming static/PAT/miss) |
| `RemoveEntry()` | 1 |
| `ClearNAT()` | 1 |
| `GetNATTable()` | 1 |
| `TranslateInternalToExternal()` | 4 (disabled, static, PAT, dynamic) |
| `TranslateExternalToInternal()` | 2 (disabled, static) |

### TestDiscToRouteIntegration.cs → `RouteBuilderState` + `TopologyManager` + `RoutingTable` (10 tests)

| Escenario testado | Tests |
|---|---|
| Disco incompleto → no ruta | 1 |
| Secuencia completa 4 discos → ruta | 1 |
| Múltiples rondas → múltiples rutas | 1 |
| Disco IpRoute → ruta por defecto | 1 |
| Protocol override | 1 |
| Múltiples routers → rutas independientes | 1 |
| Resumen incluye rutas | 1 |
| Resumen todas las rondas | 1 |
| FindBestRoute funciona | 1 |
| Longest prefix match | 1 |

### TestRoutePersistence.cs → `RouteBuilderState` + `TopologyManager` + `RoutingTable` (3 tests)

| Escenario | Tests |
|---|---|
| Ruta persiste tras refrescar actividad | 1 |
| Múltiples rondas persisten | 1 |
| `SelectActivity()` no destruye topología | 1 |

---

## Tangible/ — 3 suites, 48 tests

### TestDiscEventHandler.cs → `DiscEventHandler` (18 tests)

| Método testado | Tests |
|---|---|
| `HandleDiscPlaced()` | 4 (Router, Switch, PC, disco no físico) |
| `HandleDiscRemoved()` | 2 (existe, no existe) |
| `HandleRoutingConfigDisc()` | 12 (RedDestino, 4 discos completos, IpRoute, ModoEnrutamiento, Métrica, Destino, Vecino sin activity, Vecino con activity, AnunciarRed, Costo, BW, Sin router cercano) |

### TestRouteBuilderState.cs → `RouteBuilderState` (15 tests)

| Método testado | Tests |
|---|---|
| `IsComplete` | 7 (todos campos, falta destino/mask/nextHop/interface, strings vacíos, protocolo opcional) |
| `ApplyToRouter()` | 6 (completo, protocol override OSPF, incompleto, router inexistente, múltiples llamadas) |
| `Reset()` | 2 (limpia campos, preserva protocolo) |

### TestTangibleBridge.cs → `TangibleBridge` (15 tests)

| Método testado | Tests |
|---|---|
| `MapPatternToDiscType()` | 6 (patrones 1-4, desconocido) |
| `ConvertToCanvasPosition()` | 3 (fallback, screen→canvas, bordes) |
| `tangibleIdToUniqueId` | 1 (inicia vacío) |
| `HandleTangibleAdded()` | 3 (con DiscManager, reuso, sin DiscManager) |
| `HandleTangibleRemoved()` | 1 (sin DiscManager) |
| `HandleTangibleUpdated()` | 1 (sin DiscManager) |

---

## Simulation/ — 14 suites, 138 tests

### TestScoringSystem.cs → `ScoringSystem` (19 tests)

| Método testado | Tests |
|---|---|
| `StartSession()` | 1 |
| `AddTaskCompleted()` | 1 |
| `AddFaultFound()` | 1 |
| `AddRouteConfigured()` | 1 |
| `AddPingSuccess()` | 1 |
| `AddPenalty()` | 1 |
| Score floor | 1 |
| `GetGrade()` | 10 (parámetros: 0, 20, 40, 50, 60, 70, 80, 90, 95, 100) |
| `GetEvents()` | 1 |
| `GetSessionSummary()` | 1 |

### TestBestRouteActivity.cs → `BestRouteActivity` (12 tests)

| Método testado | Tests |
|---|---|
| `GetPrefixLength()` | 8 (todas las máscaras estándar) |
| Escenarios inicializados | 1 |
| `NextScenario()` | 1 (cicla) |
| Rango de índices | 1 |
| Mínimo 2 opciones | 1 |

### TestPredefinedScenarios.cs → `PredefinedScenarios` (13 tests)

| Método testado | Tests |
|---|---|
| `GetScenarioCount()` | 1 |
| `GetScenario()` | 2 (válido, inválido) |
| `GetScenariosByDifficulty()` | 3 (Básico, Intermedio, Avanzado) |
| Validación de datos | 6 (nombre, dispositivos, links, IPs, objetivos) |
| `GetScenarios()` | 1 (copia defensiva) |

### TestActivityLoader.cs → `ActivityLoader` (20 tests)

| Método testado | Tests |
|---|---|
| `SelectActivity(0-6)` | 7 (cada actividad crea su panel) |
| `SelectActivity()` inválido | 1 |
| `SelectActivity(0)` componentes | 2 (BuildTopologyActivity, SimulationControls) |
| `ShowConnectivityPanel()` | 3 (panel, TopologyManager, NodeVisualizer) |
| `ShowConnectivityPanel()` sin canvas | 1 |
| `LoadScenario(0-4)` | 5 (cada escenario) |
| `LoadScenario()` inválido | 1 |

### TestDynamicRoutingProtocol.cs → `DynamicRoutingProtocol` (24 tests)

| Método testado | Tests |
|---|---|
| `StartProtocol()` | 5 (2 routers, <2 routers, sin topology, EIGRP) |
| `StopProtocol()` | 1 |
| `ProtocolType` | 3 (default RIP, OSPF, EIGRP) |
| `GetProtocolStatus()` | 3 (stopped, running, EIGRP stopped) |
| `CalculateEIGRPMetric()` | 3 (default K, custom BW, custom delay) |
| `SetKValue()` | 2 (índice válido, bulk) |
| `GetRouterRoutesSummary()` | 2 (con rutas, sin rutas) |
| Configuración virtual | 4 (SetManualNeighbor, SetManualNetwork, SetCustomCost, SetCustomBandwidth) |
| `ClearAllRoutes()` | 1 |

### TestSceneCleanupService.cs → `SceneCleanupService` (3 tests)

| Método testado | Tests |
|---|---|
| Singleton | 1 |
| `ClearSimulation()` null params | 1 |
| `ClearSimulation()` parcial null | 1 |

### TestActivityLoaderSplit.cs → `ActivityLoader` (fachada), `ActivityDispatcher`, `ActivityHudFactory`, `ScenarioLoader` (8 tests)

| Método testado | Tests |
|---|---|
| Fachada `SetSelectedProtocol()` / `StartSimulation()` | 2 |
| `ActivityHudFactory` (panel HUD, debounce de clicks) | 2 |
| `ActivityDispatcher` (SelectActivity, protocolo desde botón RIP) | 2 |
| `ScenarioLoader.BuildScenarioTopology()` (nodos/links/IPs/rutas, falla en nodo) | 2 |

### TestActivityStartup.cs → `ActivityStartup` (5 tests)

| Método testado | Tests |
|---|---|
| `ResolveTopologyManager()` (sin manager, con manager, nunca crea GameObject fantasma) | 3 |
| `Start()` de actividades sin manager (no falla ni crea fantasma) | 1 |
| `Start()` con manager existente (lo asigna) | 1 |

### TestBuildTopologyActivity.cs → `BuildTopologyActivity` (3 tests)

| Método testado | Tests |
|---|---|
| `Start()` suscribe los 4 handlers de topología | 1 |
| `OnDestroy()` desuscribe todos | 1 |
| `OnDestroy()` sin `Start()` no lanza ni deja suscripciones | 1 |

### TestFindFaultScenarios.cs → `FindFaultScenarios`, `FindFaultActivity` (5 tests)

| Escenario testado | Tests |
|---|---|
| `Create()` devuelve 4 escenarios | 1 |
| Nombres y tipos de falla esperados | 1 |
| Datos de reparación clave vs dataset original | 1 |
| Instancias independientes por llamada | 1 |
| `FindFaultActivity.Awake()` carga el catálogo | 1 |

### TestInputUnification.cs → input unificado (`MenuNavigator`, `SimulationControls`, `DebugDiscSimulator`, `DynamicRoutingActivity`) (16 tests)

| Comportamiento testado | Tests |
|---|---|
| Ciclo de teclado: único owner (`MenuNavigator`) | 2 |
| `MenuNavigator.SelectAndInvoke()` (invoca botón, índice fuera de rango) | 2 |
| `ShouldRemoveSelectedNodeOnR()` (panel visible/oculto, sin actividad) | 3 |
| `IsRefreshPanelVisible()` | 1 |
| `DebugDiscSimulator` sin binding de tecla P | 1 |
| `SimulationControls` como owner de P | 1 |
| Barrido de menú (`ShouldSkipMainMenuSweep`, `GoBackToMainMenu` sin canvas) | 2 |
| `DynamicRoutingActivity` sin topología (4 guards) | 4 |

### TestScenarioLoaderGuards.cs → `ScenarioLoader` (1 test)

| Escenario testado | Tests |
|---|---|
| `LoadScenario()` sin `PredefinedScenarios`: LogError y no lanza | 1 |

### TestSceneCleanupRoutes.cs → `SceneCleanupService` (4 tests)

| Método testado | Tests |
|---|---|
| `GetOrCreate()` (crea y reutiliza instancia única) | 2 |
| `DestroyPreviousPanels()` (escena vacía, panel + backdrop) | 2 |

### TestSceneSetupSplit.cs → `SceneSetup` (fachada), `SceneBootstrap`, `SceneNavigation` (5 tests)

| Método testado | Tests |
|---|---|
| Fachada pública de `SceneSetup` callable en escena vacía | 1 |
| `SceneBootstrap.SetupScene()` (CanvasScaler + EventSystem) | 1 |
| `SetupManagers()` crea GameManager con todos los managers | 1 |
| `CreateMainMenu()` construye panel de menú | 1 |
| `SceneNavigation.ShowActivities()` reemplaza menú por actividades | 1 |

---

## UI/ — 3 suites, 29 tests

### TestUIPanelFactory.cs → `UIPanelFactory` (7 tests)

| Método testado | Tests |
|---|---|
| `GetFont()` | 1 |
| `CreateMainMenu()` | 1 (6 botones) |
| `CreateActivitiesPanel()` | 1 (7 botones) |
| `CreateConnectivityPanel()` | 1 |
| `CreateInstructionsPanel()` | 1 |
| `CreateDiscLegendPanel()` | 1 |
| `CreateConnectivityPanel()` estructura de hijos | 1 |

### TestActivityPanelFactory.cs → `ActivityPanelFactory` (12 tests)

| Método testado | Tests |
|---|---|
| `CreateBuildTopologyInfoPanel()` | 1 |
| `CreateFindFaultPanel()` | 1 |
| `CreateBestRoutePanel()` | 1 |
| `CreateRoutingTablesPanel()` | 1 |
| `CreateStaticRoutingPanel()` | 1 |
| `CreateDynamicRoutingPanel()` | 1 |
| `CreateScenariosPanel()` | 1 |
| `CreateScenarioInfoPanel()` | 1 |
| Estructura y colores de items | 4 (ScenarioItem colores/hijos, DynamicRouting inputs, BestRoute back button) |

### TestConfigPanelFactory.cs → `ConfigPanelFactory` (10 tests)

| Método testado | Tests |
|---|---|
| `CreateIPConfigPanel()` | 1 |
| `CreateARPPanel()` | 1 |
| `CreateRoutingPanel()` | 1 |
| `CreateAddRoutePanel()` | 1 |
| `CreateVLANPanel()` | 1 |
| `CreateACLPanel()` | 1 |
| `CreateNATPanel()` | 1 |
| Estructura de inputs | 3 (AddRoute config fields, VLAN input, ACL name input) |

---

## Clases SIN Test Dedicado

| Clase | Archivo | Notas |
|---|---|---|
| `SceneSetup` | Simulation/ | Probado por TestSceneSetupSplit (fachada, bootstrap, navegación) |
| `NodeVisualizer` | UI/ | Probado indirectamente por TopologyManager tests |
| `PingVisualizer` | UI/ | Probado indirectamente por connectivity tests |
| `LinkModeController` | UI/ | Sin tests directos |
| `PingModeController` | UI/ | Sin tests directos |
| `IPConfigController` | UI/ | Sin tests directos |
| `DevicePanelController` | UI/ | Sin tests directos |
| `NodeInteractionController` | UI/ | Sin tests directos |
| `MenuNavigator` | UI/ | Probado vía TestInputUnification |
| `MainMenuManager` | UI/ | Probado vía TestInputUnification |
| `ConnectivityTestPanel` | UI/ | Sin tests directos |
| `IDEUMConfigurator` | UI/ | Leaf node, trivial |
| `MemoryDiagnostics` | Core/ | Leaf node, trivial |
| `RoutingProtocols` | Simulation/ | Probado vía DynamicRoutingProtocol tests |
| `SimulationControls` | Simulation/ | Probado vía TestInputUnification |
| `TouchScriptDisabler` | SimRedes/ | Leaf node, trivial |
| `PointerClickHandler` | SimRedes/ | Leaf node, trivial |
