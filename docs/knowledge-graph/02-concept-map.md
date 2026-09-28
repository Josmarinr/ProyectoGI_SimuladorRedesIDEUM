# 02 — Mapa de Conceptos del Dominio → Código

> **Propósito**: Mapear cada concepto de red/simulación a los archivos donde se implementa, se testea, y se documenta.

---

## Conceptos de Red

### Conectividad

| Aspecto | Archivo | Ubicación |
|---|---|---|
| **Implementación** | `TopologyManager.cs` | `CheckConnectivity()` — BFS + validación IP |
| **Validación IP** | `IPValidation.cs` | `IsInSameNetwork()`, `GetNetworkAddress()` |
| **Visualización** | `PingVisualizer.cs` | `AnimatePing()` — animación de paquete |
| **Panel UI** | `ConnectivityTestPanel.cs` | Panel de prueba de conectividad |
| **Test** | `TestTopologyManager.cs` | 4 tests de `CheckConnectivity()` |
| **Diagrama** | `11-sequence-connectivity.md` | Secuencia de ping |
| **Skill** | `network-tables` | IPValidation + PingVisualizer + CheckConnectivity |

### Routing Estático

| Aspecto | Archivo | Ubicación |
|---|---|---|
| **Tabla de rutas** | `RoutingTable.cs` | `AddStaticRoute()`, `FindBestRoute()` |
| **Actividad** | `StaticRoutingActivity.cs` | UI para agregar rutas manualmente |
| **Config por disco** | `RouteBuilderState.cs` | `ApplyToRouter()` → `AddStaticRoute()` |
| **Validación** | `IPValidation.cs` | `IsValidIP()`, `IsValidSubnetMask()` |
| **Test** | `TestRoutingTable.cs` | 20 tests de RoutingTable |
| **Test integración** | `TestDiscToRouteIntegration.cs` | 10 tests disco → ruta |
| **Diagrama** | `17-activity-static.md` | Flujo de actividad estática |
| **Skill** | `network-tables` | RoutingTable API |

### Routing Dinámico (RIP/OSPF/EIGRP)

| Aspecto | Archivo | Ubicación |
|---|---|---|
| **Protocolo** | `DynamicRoutingProtocol.cs` | `StartProtocol()`, `SimulateConvergence()` |
| **Simulador** | `RoutingProtocols.cs` | `SimulateRIPAdvertisement()`, etc. |
| **Actividad** | `DynamicRoutingActivity.cs` | UI para configurar protocolo |
| **Config por disco** | `DiscEventHandler.cs` | Discos 15-18 → neighbor, network, cost, BW |
| **Test** | `TestDynamicRoutingProtocol.cs` | 24 tests |
| **Diagrama** | `18-activity-dynamic.md` | Flujo de actividad dinámica |
| **Diagrama** | `20-state-protocol.md` | Estados del protocolo |
| **Skill** | `dynamic-routing` | DynamicRoutingProtocol API |

### Detección de Fallos

| Aspecto | Archivo | Ubicación |
|---|---|---|
| **Fault en nodo** | `NetworkNode.cs` | `IsActive`, `HasFault()` |
| **Fault en enlace** | `NetworkLink.cs` | `IsActive`, `SetFault()`, `ClearFault()` |
| **Set/Clear** | `TopologyManager.cs` | `SetNodeFault()`, `ClearNodeFault()`, `SetLinkFault()` |
| **Actividad** | `FindFaultActivity.cs` | 4 escenarios de fallos |
| **Test** | `TestTopologyManager.cs` | Tests de fault methods |
| **Diagrama** | `15-activity-faults.md` | Flujo de detección |
| **Skill** | `topology-detection` | Detección de topología |

### VLAN (Redes Virtuales)

| Aspecto | Archivo | Ubicación |
|---|---|---|
| **Implementación** | `VLANManager.cs` | `CreateVLAN()`, `AssignToVLAN()`, `CanCommunicate()` |
| **Nodo** | `NetworkNode.cs` | `VlanId` property |
| **Panel UI** | `ConfigPanelFactory.cs` | `CreateVLANPanel()` |
| **Test** | `TestVLANManager.cs` | 18 tests |
| **Diagrama** | `02-class-network.md` | DC-02c |
| **Skill** | `vlan-acl-nat` | VLANManager API |

### ACL (Listas de Acceso)

| Aspecto | Archivo | Ubicación |
|---|---|---|
| **Implementación** | `ACLManager.cs` | `AddRule()`, `CheckPacket()`, `DeleteACL()` |
| **Regla** | `ACLManager.cs` | `ACLRule.Matches()` |
| **Panel UI** | `ConfigPanelFactory.cs` | `CreateACLPanel()` |
| **Test** | `TestACLManager.cs` | 29 tests |
| **Diagrama** | `02-class-network.md` | DC-02c |
| **Skill** | `vlan-acl-nat` | ACLManager API |

### NAT (Traducción de Direcciones)

| Aspecto | Archivo | Ubicación |
|---|---|---|
| **Implementación** | `NATManager.cs` | `AddStaticNAT()`, `TranslatePacket()`, `TranslateInternalToExternal()` |
| **Tipos** | `NATManager.cs` | Static, Dynamic, PAT |
| **Panel UI** | `ConfigPanelFactory.cs` | `CreateNATPanel()` |
| **Test** | `TestNATManager.cs` | 25 tests |
| **Diagrama** | `02-class-network.md` | DC-02c |
| **Skill** | `vlan-acl-nat` | NATManager API |

### ARP (Address Resolution Protocol)

| Aspecto | Archivo | Ubicación |
|---|---|---|
| **Implementación** | `ARPTable.cs` | `AddEntry()`, `FindEntry()`, `AgeEntries()` |
| **Nodo** | `NetworkNode.cs` | `ArpTable` property |
| **Panel UI** | `ConfigPanelFactory.cs` | `CreateARPPanel()` |
| **Test** | `TestARPTable.cs` | 13 tests |
| **Skill** | `network-tables` | ARPTable operations |

### IP Validation

| Aspecto | Archivo | Ubicación |
|---|---|---|
| **Implementación** | `IPValidation.cs` | 11 métodos estáticos |
| **Test** | `TestIPValidation.cs` | 18 tests |
| **Skill** | `network-tables` | IPValidation API |

### Discos de Configuración (7-18)

| Aspecto | Archivo | Ubicación |
|---|---|---|
| **Mapeo disco→tipo** | `DiscConfiguration.cs` | `GetConfiguration()`, enum `DiscType` |
| **Handler** | `DiscEventHandler.cs` | `HandleRoutingConfigDisc()` |
| **Estado builder** | `RouteBuilderState.cs` | `IsComplete`, `ApplyToRouter()` |
| **Test** | `TestDiscToRouteIntegration.cs` | 10 tests |
| **Test** | `TestDiscEventHandler.cs` | 18 tests |
| **Test** | `TestRouteBuilderState.cs` | 15 tests |
| **Diagrama** | `19-state-routebuilder.md` | Estados del builder |

---

## Conceptos de UI

### Menú Principal

| Aspecto | Archivo | Ubicación |
|---|---|---|
| **Navegación** | `MenuNavigator.cs` | `SetupPanel()`, `ClearPanel()` |
| **Manager** | `MainMenuManager.cs` | `ShowMainMenu()`, `OnStartClicked()`, etc. |
| **Panel** | `UIPanelFactory.cs` | `CreateMainMenu()` |
| **Skill** | `menu-navigation` | MenuNavigator singleton |

### Paneles de Actividades

| Aspecto | Archivo | Ubicación |
|---|---|---|
| **Factory** | `ActivityPanelFactory.cs` | 8 métodos Create*Panel |
| **Componentes** | `UIComponents.cs` | `CreateMenuButton()`, `CreateInfoText()`, etc. |
| **Test** | `TestActivityPanelFactory.cs` | 12 tests |

### Controladores de Interacción

| Aspecto | Archivo | Ubicación |
|---|---|---|
| **Modo enlace** | `LinkModeController.cs` | `ToggleLinkMode()`, `HandleNodeLinkClick()` |
| **Modo ping** | `PingModeController.cs` | `TogglePingMode()`, `HandlePingNodeClick()` |
| **Config IP** | `IPConfigController.cs` | `ShowIPConfigPanel()`, `CloseIPConfigPanel()` |
| **Panel dispositivos** | `DevicePanelController.cs` | `OnDeviceItemClicked()`, `RefreshDevicesPanel()` |
| **Interacción** | `NodeInteractionController.cs` | `HandleNodeClick()` — dispatcher |

### Visualización de Nodos

| Aspecto | Archivo | Ubicación |
|---|---|---|
| **Crear visuales** | `NodeVisualizer.cs` | `CreateNodeVisual()`, `CreateLinkLine()` |
| **Animación ping** | `PingVisualizer.cs` | `AnimatePing()` |
| **Colores** | `UIComponents.cs` | `GetColorForDeviceType()`, `Colors.*` |

---

## Conceptos de Tangible

### Integración con Mesa IDEUM

| Aspecto | Archivo | Ubicación |
|---|---|---|
| **Puente** | `TangibleBridge.cs` | `tangibleId→uniqueId`, coordenadas TE→Canvas |
| **Estado discos** | `TangibleDiscManager.cs` | `OnDiscPlaced/Moved/Removed` |
| **Eventos** | `DiscEventHandler.cs` | Auto-conectar enlaces, routing config |
| **Debug** | `DebugDiscSimulator.cs` | `enableSimulation=false` (producción) |
| **Test** | `TestTangibleBridge.cs` | 15 tests |
| **Test** | `TestDiscEventHandler.cs` | 18 tests |
| **Skill** | `ideum-integration` | TangibleBridge + DiscManager |

---

## Conceptos de Simulación

### Actividades (7)

| # | Actividad | Archivo | Test |
|---|---|---|---|
| 0 | Build Topology | `BuildTopologyActivity.cs` | TestActivityLoader |
| 1 | Find Faults | `FindFaultActivity.cs` | TestActivityLoader |
| 2 | Routing Tables | `RoutingTablesActivity.cs` | TestActivityLoader |
| 3 | Best Route | `BestRouteActivity.cs` | TestBestRouteActivity |
| 4 | Static Routing | `StaticRoutingActivity.cs` | TestActivityLoader |
| 5 | Dynamic Routing | `DynamicRoutingActivity.cs` | TestActivityLoader |
| 6 | Scenarios | `PredefinedScenarios.cs` | TestPredefinedScenarios |

### Escenarios Predefinidos

| Aspecto | Archivo | Ubicación |
|---|---|---|
| **Datos** | `PredefinedScenarios.cs` | 5 escenarios (Estrella, Dos Routers, Anillo, Árbol, Fallos) |
| **Carga** | `ScenarioLoader.cs` | `LoadScenario()` |
| **Test** | `TestPredefinedScenarios.cs` | 13 tests |
| **Skill** | `predefined-scenarios` | Scenario structure |

### Sistema de Puntaje

| Aspecto | Archivo | Ubicación |
|---|---|---|
| **Implementación** | `ScoringSystem.cs` | `StartSession()`, `AddTaskCompleted()`, `GetGrade()` |
| **Panel UI** | `UIPanelFactory.cs` | `CreateScorePanel()` |
| **Test** | `TestScoringSystem.cs` | 19 tests |
| **Skill** | `scoring-system` | ScoringSession API |

### Limpieza de Escena

| Aspecto | Archivo | Ubicación |
|---|---|---|
| **Implementación** | `SceneCleanupService.cs` | `ClearSimulation()`, `GoBackToMainMenu()` |
| **Test** | `TestSceneCleanupService.cs` | 3 tests |

---

## Tabla Cruzada: Concepto → Todos los Recursos

| Concepto | Archivos | Tests | Diagramas | Skills |
|---|---|---|---|---|
| Conectividad | TopologyManager, IPValidation, PingVisualizer | TestTopologyManager | 11 | network-tables |
| Routing Estático | RoutingTable, StaticRoutingActivity, RouteBuilderState | TestRoutingTable, TestDiscToRoute | 17 | network-tables |
| Routing Dinámico | DynamicRoutingProtocol, RoutingProtocols, DynamicRoutingActivity | TestDynamicRoutingProtocol | 18, 20 | dynamic-routing |
| Fallos | NetworkNode, NetworkLink, TopologyManager, FindFaultActivity | TestTopologyManager | 15 | topology-detection |
| VLAN | VLANManager, NetworkNode | TestVLANManager | 02-class-network | vlan-acl-nat |
| ACL | ACLManager, ACLRule | TestACLManager | 02-class-network | vlan-acl-nat |
| NAT | NATManager, NATEntry | TestNATManager | 02-class-network | vlan-acl-nat |
| ARP | ARPTable, NetworkNode | TestARPTable | 02-class-network | network-tables |
| IP Validation | IPValidation | TestIPValidation | 02-class-network | network-tables |
| Tangible | TangibleBridge, TangibleDiscManager, DiscEventHandler | TestTangibleBridge, TestDiscEventHandler | 09 | ideum-integration |
| Route Building | RouteBuilderState, DiscEventHandler | TestRouteBuilderState, TestDiscToRoute | 19 | manual-links |
| Puntaje | ScoringSystem | TestScoringSystem | 21 | scoring-system |
| Escenarios | PredefinedScenarios | TestPredefinedScenarios | — | predefined-scenarios |
| Menú | MenuNavigator, MainMenuManager, UIPanelFactory | TestUIPanelFactory | 12 | menu-navigation |
| UI Components | UIComponents, 3 Factories | TestUIPanelFactory, TestActivity/ConfigPanelFactory | 07 | unity-ui-buttons |
| Cleanup | SceneCleanupService | TestSceneCleanupService | — | — |
