# CODE_INDEX — SimuladorRedes IDEUM

> Indice compacto de 46 scripts (~18K) para cargar bajo demanda y ahorrar tokens.
> Contiene: archivo → clases clave → métodos públicos principales.

---

## Core/ (1 archivo)

### AppLogger.cs (27L)
`SimRedes.Core` — **AppLogger** (static) — `Log()`, `LogWarning()`, `LogError()` — `EnableLogging=false`

---

## Network/ (10 archivos, ~2.5K total)

### TopologyManager.cs (718L)
`SimRedes.Network` — **TopologyManager** : MonoBehaviour (singleton `Instance`) — Singleton de red central.
- Events: OnNodeAdded/Removed, OnLinkAdded/Removed, OnTopologyChanged
- Métodos: AddNode, RemoveNode, UpdateNodePosition, AddLink, RemoveLink, SelectNodeForLink, CreateLinkBetweenSelected, GetNode, FindNodesNear, GetAllNodes, GetAllLinks, SetNodeFault, ClearNodeFault, SetLinkFault, ClearLinkFault, CheckConnectivity, GetIPNetworkInfo, FindPath, GetLinksOnPath, ClearTopology, GetTopologySummary, GetVLANSummary, GetACLSummary, GetNATSummary, CreateACL, AddACLRule, DeleteACL, TranslateSourceIP, TranslateDestIP

### NetworkNode.cs (130L)
`SimRedes.Network` — **DeviceType** enum (Router/Switch/PC/Unknown) — **NetworkNode**
- Props: Id, Name, Type, IpAddress, SubnetMask, Position, DiscId, IsActive, Interfaces, InterfaceIPs, VlanId, ArpTable, RoutingTable
- Métodos: SetInterfaceIP, GetInterfaceIP, HasFault, GetDefaultGateway, GetNetworkAddress, GetPrefixLength, IsValidConfiguration

### NetworkLink.cs (70L)
`SimRedes.Network` — **LinkType** enum (Ethernet/Serial/Wireless/Fiber) — **NetworkLink**
- Props: Id, SourceNode, DestNode, SourceInterface, DestInterface, Type, Bandwidth, Delay, Cost, IsActive
- Métodos: SetFault, ClearFault, IsFunctional

### RoutingTable.cs (187L)
`SimRedes.Network` — **RoutingEntry** (DestNetwork, SubnetMask, NextHop, OutInterface, Metric, Protocol) — **RoutingTable**
- Métodos: AddStaticRoute, AddRipRoute, AddOspfRoute, AddEigrpRoute, FindBestRoute, GetAllEntries, Clear, GetTableSummary

### ARPTable.cs (92L)
`SimRedes.Network` — **ARPEntry** (IPAddress, MACAddress, Interface, Type, Age) — **ARPTable**
- Métodos: AddEntry, FindEntry, GetAllEntries, Clear, AgeEntries, GetTableSummary

### IPValidation.cs (201L)
`SimRedes.Network` — **IPValidation** (static)
- Métodos: IsValidIP, IsValidSubnetMask, ValidateIPField, ValidateMaskField, GetPrefixLength, IsInSameNetwork, GetNetworkAddress, GetBroadcastAddress, GetGatewayFromIP

### DiscConfiguration.cs (108L)
`SimRedes.Network` — **DiscType** enum (Router=1, Switch=2, PC=3, Enlace=4, Fallo=5, Protocolo=6, RedDestino=7, Metrica=8, InterfazSalida=9, ModoEnrutamiento=10, IpRoute=11, Destino=12, Mascara=13, ProximoSalto=14, Vecino=15, AnunciarRed=16, Costo=17, BW=18)
- **DiscConfiguration** (static) — GetConfiguration(), DefaultConfiguration

### VLANManager.cs (146L)
`SimRedes.Network` — **VLANManager** — CreateVLAN, DeleteVLAN, AssignToVLAN, GetNodeVLAN, GetNodesInVLAN, CanCommunicate, GetAllVLANs

### ACLManager.cs (296L)
`SimRedes.Network` — **ACLAction** enum (Permit/Deny) — **ACLProtocol** enum (Any/TCP/UDP/ICMP/IP)
- **ACLRule** — Matches()
- **ACLManager** — CreateACL, AddRule, RemoveRule, CheckPacket, GetRules, DeleteACL

### NATManager.cs (311L)
`SimRedes.Network` — **NATType** enum (Static/Dynamic/PAT) — **NATEntry**
- **NATManager** — SetPublicIP, SetRouterIP, AddStaticNAT, AddDynamicNAT, AddPAT, LookupInternal, LookupExternal, TranslatePacket, RemoveEntry, ClearNAT, GetNATTable, TranslateInternalToExternal, TranslateExternalToInternal

---

## Simulation/ (14 archivos, ~4.8K total)

### SceneSetup.cs (606L)
`SimRedes` — **SceneSetup** : MonoBehaviour
- SetupManagers, CreateVisualizer, SetupScene, SubscribeToTopologyEvents, CreateMainMenuPublic, ExitApplication, SelectActivity, ToggleLinkMode, IsLinkModeActive, IsPingModeActive, IsIPConfigPanelOpen, GetCurrentIPConfigNodeDiscId, CloseIPConfigPanelPublic, HandleNodeClick, ShowIPConfigPanel, RemoveSelectedNodePublic, ClearSelectedNode, RefreshDevicesPanel, UpdateScoreDisplay

### ActivityLoader.cs (843L)
`SimRedes.Simulation` — **ActivityLoader** : MonoBehaviour
- selectedProtocol (RIP/OSPF/EIGRP por defecto)
- SelectActivity(n), StartSimulation, ShowConnectivityPanel, StartDynamicProtocol, StopDynamicProtocol, ClearDynamicRoutes, ShowAllRouterRoutes

### SceneCleanupService.cs (115L)
`SimRedes.Simulation` — **SceneCleanupService** : MonoBehaviour (singleton `Instance`) — ClearSimulation, GoBackToMainMenu, ExitApplication

### ScoringSystem.cs (222L)
`SimRedes.Simulation` — **ScoringSystem** : MonoBehaviour (singleton `Instance`) — StartSession, AddTaskCompleted, AddFaultFound, AddRouteConfigured, AddPingSuccess, AddPenalty, GetCurrentScore, GetGrade, EndSession

### BuildTopologyActivity.cs (342L)
`SimRedes.Simulation` — **TopologyType** enum (Estrella/Bus/Anillo/Arbol/Malla) — **BuildTopologyActivity** : MonoBehaviour — GetCurrentTopology, IsFullyConnected

### FindFaultActivity.cs (455L)
`SimRedes.Simulation` — **FindFaultScenario** — **FindFaultActivity** : MonoBehaviour — LoadScenario(n), OnDiscButtonClicked, OnNextClicked, OnPrevClicked, Restart

### BestRouteActivity.cs (325L)
`SimRedes.Simulation` — **RouteScenario**, **RouteOption** — **BestRouteActivity** : MonoBehaviour — NextScenario

### RoutingTablesActivity.cs (159L)
`SimRedes.Simulation` — **RoutingTablesActivity** : MonoBehaviour — RefreshRoutingTables, GetRouters

### StaticRoutingActivity.cs (246L)
`SimRedes.Simulation` — **StaticRoutingActivity** : MonoBehaviour — AddRoute, ShowAddRoutePanel, AddSampleRoute, TestRouting

### DynamicRoutingActivity.cs (278L)
`SimRedes.Simulation` — **DynamicRoutingActivity** : MonoBehaviour — SetProtocol, StartProtocol, StopProtocol, ClearAllRoutes, ShowRoutes, GetCurrentProtocol, SetNeighborRouter, SetNetworkToAdvertise, SetLinkCost, SetBandwidth, SetKValue, ApplyDiscConfigToProtocol

### DynamicRoutingProtocol.cs (487L)
`SimRedes.Simulation` — **ProtocolType** enum (RIP/OSPF/EIGRP) — **DynamicRoutingProtocol** : MonoBehaviour
- Events: OnProtocolLog, OnConvergence
- StartProtocol, StopProtocol, IsRunning, IsConverged, GetAdvertisementCount, SimulateConvergence, GetProtocolStatus, GetRouters, GetRouterRoutesSummary, SetManualNeighbor, SetManualNetwork, SetCustomCost, SetCustomBandwidth, SetKValue, SetAllKValues, ClearAllRoutes

### RoutingProtocols.cs (134L)
`SimRedes.Simulation` — **RoutingProtocol** enum (Static/RIP/OSPF/EIGRP) — **RoutingSimulator** (static) — SetProtocol, SimulateRIPAdvertisement, SimulateOSPFAdvertisement, SimulateEIGRPAdvertisement, SimulatePacketForwarding

### PredefinedScenarios.cs (333L)
`SimRedes.Simulation` — **PredefinedScenarios** : MonoBehaviour (singleton `Instance`) — **ScenarioDifficulty** enum — GetScenarios, GetScenariosByDifficulty, GetScenario, GetScenarioCount

### SimulationControls.cs (136L)
`SimRedes.Simulation` — **SimulationControls** : MonoBehaviour — GoBackToMainMenu

### TouchScriptDisabler.cs (37L) + PointerClickHandler.cs (20L)
`SimRedes` — **TouchScriptDisabler**, **PointerClickHandler** (IPointerClickHandler)

---

## Tangible/ (5 archivos, ~1.1K total)

### TangibleDiscManager.cs (169L)
`SimRedes.Tangible` — **TangibleDiscManager** : MonoBehaviour (singleton `Instance`)
- Events: OnDiscPlaced, OnDiscMoved, OnDiscRemoved
- SimulateDiscPlaced, UpdateDiscPosition, SimulateDiscRemoved, GetActiveDiscs, IsDiscActive, GetDiscPosition, GetDiscType, ClearAllDiscs

### TangibleBridge.cs (225L)
`SimRedes.Tangible` — **TangibleBridge** : MonoBehaviour — Mapea tangibleId→uniqueId, coordenadas TE→Canvas. Fallback silencioso.

### DiscEventHandler.cs (407L)
`SimRedes.Tangible` — **DiscEventHandler** : MonoBehaviour — autoConnectLinks=true, linkDistanceThreshold=300, routerProximityRadius

### DebugDiscSimulator.cs (205L)
`SimRedes.Tangible` — **DebugDiscSimulator** : MonoBehaviour — enableSimulation=false — ForceClearAll

### RouteBuilderState.cs (42L)
`SimRedes.Tangible` — **RouteBuilderState** — Props: RouterDiscId, DestNetwork, SubnetMask, NextHop, OutInterface, Protocol, IsComplete — ApplyToRouter, Reset

---

## UI/ (16 archivos, ~6.2K total)

### UIPanelFactory.cs (953L)
`SimRedes.UI` — **UIPanelFactory** (static)
- Métodos: CreateMainMenu, CreateActivitiesPanel, CreateScorePanel, CreateDevicesPanel, CreateConnectivityPanel, CreateInstructionsPanel, CreateDiscLegendPanel, ToggleTopologyExamplePanel
- Internal static helpers: GetFont, CreateNumericKeypad, CreateConfigField, CreateDropdown, UpdateStatusText

### ActivityPanelFactory.cs (857L)
`SimRedes.UI` — **ActivityPanelFactory** (static)
- CreateBuildTopologyInfoPanel, CreateBestRoutePanel, CreateFindFaultPanel, CreateRoutingTablesPanel, CreateStaticRoutingPanel, CreateDynamicRoutingPanel, CreateScenariosPanel, CreateScenarioInfoPanel

### ConfigPanelFactory.cs (882L)
`SimRedes.UI` — **ConfigPanelFactory** (static)
- CreateIPConfigPanel, CreateARPPanel, CreateRoutingPanel, CreateAddRoutePanel, CreateVLANPanel, CreateACLPanel, CreateNATPanel

### UIComponents.cs (907L)
`SimRedes.UI` — **UIComponents** (static)
- Colors (static): backgroundBase, surfacePanel, border, textPrimary, textSecondary, textAccent, buttonNormal, buttonHover, etc. + GetColorForDeviceType()
- Métodos: GetFont, GetButtonColors, CreateInfoText, CreateSimpleText, ApplyTitleStyle, CreateSmallInfoButton, CreateMenuPanel, CreateRoundedPanel, CreateMenuTitle, CreateMenuButton, CreateSmallButton, CreateClickOutsideToClose, CreateSimpleDropdown, SetupNumericInput, CreateInfoTextFull, CreateRoundedRectTexture, CreateRoundedRectSprite, CreateDeviceIcon, CreateRoundedButton, CreateInfoColumn, CreateInputField

### MenuNavigator.cs (352L)
`SimRedes.UI` — **MenuNavigator** : MonoBehaviour (singleton `Instance`) — SetupPanel, ClearPanel

### MainMenuManager.cs (316L)
`SimRedes.UI` — **MainMenuManager** : MonoBehaviour (singleton `Instance`) — ShowMainMenu, OnStartClicked, OnActivitiesClicked, OnConnectivityClicked, OnInstructionsClicked, OnExitClicked, SelectActivity

### NodeVisualizer.cs (311L)
`SimRedes.UI` — **NodeVisualizer** : MonoBehaviour — CreateNodeVisual, CreateLinkLine, UpdateNodeLabel, SelectNodeByDiscId

### PingVisualizer.cs (355L)
`SimRedes.UI` — **PingVisualizer** : MonoBehaviour (singleton `Instance`) — AnimatePing, IsAnimating

### ConnectivityTestPanel.cs (270L)
`SimRedes.UI` — **ConnectivityTestPanel** : MonoBehaviour — Initialize, ResetStats

### LinkModeController.cs (126L)
`SimRedes.UI` — **LinkModeController** : MonoBehaviour — ToggleLinkMode, IsLinkModeActive, HandleNodeLinkClick, ClearState

### PingModeController.cs (319L)
`SimRedes.UI` — **PingModeController** : MonoBehaviour — TogglePingMode, IsPingModeActive, HandlePingNodeClick

### IPConfigController.cs (222L)
`SimRedes.UI` — **IPConfigController** : MonoBehaviour — IsIPConfigPanelOpen, ShowIPConfigPanel, CloseIPConfigPanel

### DevicePanelController.cs (387L)
`SimRedes.UI` — **DevicePanelController** : MonoBehaviour — OnDeviceItemClicked, RemoveSelectedNode, ClearSelectedNode, RefreshDevicesPanel, UpdateDevicesList, UpdateScoreDisplay

### NodeInteractionController.cs (58L)
`SimRedes.UI` — **NodeInteractionController** : MonoBehaviour — IsLinkModeActive, IsPingModeActive, IsIPConfigPanelOpen, HandleNodeClick, ShowIPConfigPanel

### IDEUMConfigurator.cs (43L)
`SimRedes.UI` — **IDEUMConfigurator** : MonoBehaviour — SetResolution

---

## Tests (21 archivos, 338 tests)

### Network/ (9 suites, ~190 tests)
- TestIPValidation (18), TestRoutingTable (18+EIGRP), TestRoutePersistence (3), TestDiscToRouteIntegration (10), TestTopologyManager (32), TestARPTable (13), TestVLANManager (18), TestACLManager (29), TestNATManager (25)

### Tangible/ (3 suites, 48 tests)
- TestRouteBuilderState (15), TestDiscEventHandler (18), TestTangibleBridge (15)

### Simulation/ (6 suites, 92 tests)
- TestScoringSystem (19), TestBestRouteActivity (12), TestSceneCleanupService (4), TestPredefinedScenarios (13), TestDynamicRoutingProtocol (24), TestActivityLoader (20)

### UI/ (3 suites, 22 tests)
- TestUIPanelFactory (7), TestActivityPanelFactory (8), TestConfigPanelFactory (7)

---

## Datos Clave de Arquitectura

| Concepto | Valor |
|----------|-------|
| Discos físicos | Solo 3: Router=1, Switch=2, PC=3 |
| Discos virtuales | 4-18 (actividades). Routing config 7-18 vía botones |
| DebugDiscSimulator | enableSimulation=false (prod) |
| Coordenadas TE | 1920×1080 → Canvas 4096×2160 |
| Input System | 100% migrado. 0 usos de API vieja |
| autoConnectLinks | true, <300px |
| Scene principal | Assets/Main.unity |
| Escena alterna | GetStarted_Scene.unity (sin SceneSetup) |
| Build target | Windows x86_64 (IDEUM) |
| Tests | 338 EditMode, 21 suites |
| Bugs críticos | 0 activos |
