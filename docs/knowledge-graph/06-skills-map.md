# 06 — Mapa de Skills → Archivos Referenciados

> **Propósito**: Qué skill de opencode referencia qué archivos del proyecto, y cuándo usar cada skill.

---

## Skills del Proyecto (19)

### network-tables
**Cuándo usar**: IP validation, routing tables, ARP, ping visualization, connectivity checking.
**Archivos referenciados**:
- `Assets/Scripts/Network/IPValidation.cs`
- `Assets/Scripts/Network/RoutingTable.cs`
- `Assets/Scripts/Network/ARPTable.cs`
- `Assets/Scripts/UI/PingVisualizer.cs`
- `Assets/Scripts/Network/TopologyManager.cs` (`CheckConnectivity()`)
- `Assets/Scripts/Network/NetworkNode.cs`

---

### dynamic-routing
**Cuándo usar**: Implementar o modificar routing dinámico (RIP/OSPF/EIGRP).
**Archivos referenciados**:
- `Assets/Scripts/Simulation/DynamicRoutingProtocol.cs`
- `Assets/Scripts/Simulation/DynamicRoutingActivity.cs`
- `Assets/Scripts/Simulation/RoutingProtocols.cs`

---

### topology-detection
**Cuándo usar**: Detección de topología, BuildTopologyActivity.
**Archivos referenciados**:
- `Assets/Scripts/Simulation/BuildTopologyActivity.cs`
- `Assets/Scripts/Network/TopologyManager.cs`

---

### ideum-integration
**Cuándo usar**: Integración con mesa IDEUM, discos táctiles, TangibleEngine.
**Archivos referenciados**:
- `Assets/Scripts/Tangible/TangibleBridge.cs`
- `Assets/Scripts/Tangible/TangibleDiscManager.cs`
- `Assets/Scripts/Tangible/DiscEventHandler.cs`
- `Assets/Scripts/Tangible/DebugDiscSimulator.cs`
- `Assets/Scripts/Tangible/RouteBuilderState.cs`

---

### manual-links
**Cuándo usar**: Creación/eliminación de enlaces manuales, modo CONECTAR/DESCONECTAR.
**Archivos referenciados**:
- `Assets/Scripts/UI/LinkModeController.cs`
- `Assets/Scripts/Network/TopologyManager.cs` (`AddLink()`, `RemoveLink()`, `HandleNodeClick()`)
- `Assets/Scripts/UI/NodeVisualizer.cs`

---

### menu-navigation
**Cuándo usar**: Navegación de menú, paneles, ESC, GoBackToMainMenu.
**Archivos referenciados**:
- `Assets/Scripts/UI/MenuNavigator.cs`
- `Assets/Scripts/UI/MainMenuManager.cs`
- `Assets/Scripts/Simulation/SceneCleanupService.cs`

---

### scoring-system
**Cuándo usar**: Sistema de puntaje, evaluación, notas.
**Archivos referenciados**:
- `Assets/Scripts/Simulation/ScoringSystem.cs`
- `Assets/Scripts/UI/UIPanelFactory.cs` (`CreateScorePanel()`)

---

### predefined-scenarios
**Cuándo usar**: Escenarios predefinidos, carga de escenarios.
**Archivos referenciados**:
- `Assets/Scripts/Simulation/PredefinedScenarios.cs`
- `Assets/Scripts/Simulation/ActivityLoader.cs` (`LoadScenario()`)

---

### vlan-acl-nat
**Cuándo usar**: Configuración de VLAN, ACL, NAT.
**Archivos referenciados**:
- `Assets/Scripts/Network/VLANManager.cs`
- `Assets/Scripts/Network/ACLManager.cs`
- `Assets/Scripts/Network/NATManager.cs`
- `Assets/Scripts/UI/ConfigPanelFactory.cs` (`CreateVLANPanel()`, `CreateACLPanel()`, `CreateNATPanel()`)

---

### unity-ui-buttons
**Cuándo usar**: Crear botones, paneles, UI components.
**Archivos referenciados**:
- `Assets/Scripts/UI/UIComponents.cs`
- `Assets/Scripts/UI/UIPanelFactory.cs`
- `Assets/Scripts/UI/ActivityPanelFactory.cs`
- `Assets/Scripts/UI/ConfigPanelFactory.cs`

---

### unity-scene-setup
**Cuándo usar**: Crear/modificar SceneSetup, paneles, UI elements.
**Archivos referenciados**:
- `Assets/Scripts/Simulation/SceneSetup.cs`
- `Assets/Scripts/UI/UIPanelFactory.cs`
- `Assets/Scripts/UI/UIComponents.cs`

---

### unity-code-style
**Cuándo usar**: Escribir/revisar código C#, convenciones, naming.
**Archivos referenciados**: Todos los .cs del proyecto.

---

### ui-performance
**Cuándo usar**: Optimización de UI, blurry text, FPS, Canvas Scaler.
**Archivos referenciados**:
- `Assets/Scripts/UI/UIComponents.cs`
- `Assets/Scripts/UI/UIPanelFactory.cs`
- `Assets/Scripts/UI/NodeVisualizer.cs`
- `Assets/Scripts/UI/PingVisualizer.cs`

---

### debugging
**Cuándo usar**: Debug en Unity, logging, encontrar objetos, UI profiling.
**Archivos referenciados**: Todos los MonoBehaviours del proyecto.

---

### testing-guide
**Cuándo usar**: Escribir, modificar, ejecutar tests EditMode.
**Archivos referenciados**:
- `Assets/Editor/Tests/` (21 archivos)

---

### build-and-deploy
**Cuándo usar**: Build para IDEUM (Windows), build para PC testing.
**Archivos referenciados**:
- `ProjectSettings/` (configuración de build)

---

### best-practices
**Cuándo usar**: Patrones arquitectónicos, singletons, event system, UI patterns.
**Archivos referenciados**: Todos los archivos del proyecto (referencia general).

---

### agent-workflow
**Cuándo usar**: Coordinación de agentes, reportes, progreso.
**Archivos referenciados**: `AGENTS.md`, `ROADMAP.md`

---

## Tabla Cruzada: Archivo → Skills que lo Referencian

| Archivo | Skills |
|---|---|
| `TopologyManager.cs` | network-tables, manual-links, topology-detection, vlan-acl-nat |
| `NetworkNode.cs` | network-tables, unity-code-style |
| `IPValidation.cs` | network-tables |
| `RoutingTable.cs` | network-tables, dynamic-routing |
| `ARPTable.cs` | network-tables |
| `VLANManager.cs` | vlan-acl-nat |
| `ACLManager.cs` | vlan-acl-nat |
| `NATManager.cs` | vlan-acl-nat |
| `TangibleBridge.cs` | ideum-integration |
| `TangibleDiscManager.cs` | ideum-integration |
| `DiscEventHandler.cs` | ideum-integration, manual-links |
| `DynamicRoutingProtocol.cs` | dynamic-routing |
| `DynamicRoutingActivity.cs` | dynamic-routing |
| `BuildTopologyActivity.cs` | topology-detection |
| `FindFaultActivity.cs` | topology-detection |
| `PredefinedScenarios.cs` | predefined-scenarios |
| `ScoringSystem.cs` | scoring-system |
| `SceneCleanupService.cs` | menu-navigation |
| `UIComponents.cs` | unity-ui-buttons, ui-performance |
| `UIPanelFactory.cs` | unity-ui-buttons, unity-scene-setup, menu-navigation |
| `ActivityPanelFactory.cs` | unity-ui-buttons |
| `ConfigPanelFactory.cs` | unity-ui-buttons, vlan-acl-nat |
| `LinkModeController.cs` | manual-links |
| `PingVisualizer.cs` | network-tables, ui-performance |
| `NodeVisualizer.cs` | manual-links, ui-performance |
| `MenuNavigator.cs` | menu-navigation |
| `MainMenuManager.cs` | menu-navigation |
| `SceneSetup.cs` | unity-scene-setup, debugging |
