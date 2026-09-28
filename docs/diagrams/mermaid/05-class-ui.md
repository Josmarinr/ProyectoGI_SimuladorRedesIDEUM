# DC-05: Diagrama de Clases — UI Layer

> **Propósito**: Mostrar la estructura de la capa de interfaz de usuario: factories, controladores, visualizadores y navegación.
> Dividido en 4 sub-diagramas: Factories, Controladores, Visualización, y Navegación/Utilidades.

---

## DC-05a: Factories UI

```mermaid
classDiagram
    class UIPanelFactory {
        +CreateMainMenu() GameObject
        +CreateActivitiesPanel() GameObject
        +CreateScorePanel() GameObject
        +CreateDevicesPanel() GameObject
        +CreateConnectivityPanel() ConnectivityPanelRefs
        +CreateInstructionsPanel() GameObject
        +CreateDiscLegendPanel() GameObject
        +static ToggleTopologyExamplePanel(canvas, font) void
    }

    class ActivityPanelFactory {
        +CreateBuildTopologyInfoPanel() GameObject
        +CreateBestRoutePanel() GameObject
        +CreateFindFaultPanel() GameObject
        +CreateRoutingTablesPanel() GameObject
        +CreateStaticRoutingPanel() GameObject
        +CreateDynamicRoutingPanel() GameObject
        +CreateScenariosPanel() GameObject
    }

    class ConfigPanelFactory {
        +CreateIPConfigPanel() GameObject
        +CreateARPPanel() GameObject
        +CreateRoutingPanel() GameObject
        +CreateAddRoutePanel() GameObject
        +CreateVLANPanel() GameObject
        +CreateACLPanel() GameObject
        +CreateNATPanel() GameObject
    }

    class UIComponents {
        +GetFont() Font
        +CreateMenuPanel() GameObject
        +CreateMenuTitle() Text
        +CreateMenuButton() Button
        +CreateInfoText() Text
        +CreateRoundedPanel() GameObject
        +CreateRoundedRectTexture() Texture2D
        +static class Colors
    }

    UIPanelFactory --> UIComponents
    ActivityPanelFactory --> UIComponents
    ConfigPanelFactory --> UIComponents
```

---

## DC-05b: Controladores de Interacción

```mermaid
classDiagram
    class LinkModeController {
        +ToggleLinkMode(mode) void
        +IsLinkModeActive() bool
        +HandleNodeLinkClick(discId) void
        +GetLinkModeFirstNode() int
        +ClearFirstNode() void
    }

    class PingModeController {
        +TogglePingMode() void
        +IsPingModeActive() bool
        +HandlePingNodeClick(node) void
        +StorePingReferences(btn, text) void
    }

    class IPConfigController {
        +ShowIPConfigPanel(node, discId) void
        +IsIPConfigPanelOpen() bool
        +CloseIPConfigPanel() void
        +GetCurrentIPConfigNodeDiscId() int
    }

    class DevicePanelController {
        +RefreshDevicesPanel() void
        +UpdateDevicesList() void
        +OnDeviceItemClicked(index) void
        +RemoveSelectedNode() void
    }

    class NodeInteractionController {
        +HandleNodeClick(discId) void
        +IsLinkModeActive() bool
        +IsPingModeActive() bool
    }

    LinkModeController --> DevicePanelController
    NodeInteractionController --> LinkModeController
    NodeInteractionController --> PingModeController
    NodeInteractionController --> IPConfigController
```

---

## DC-05c: Visualización de Red

```mermaid
classDiagram
    class NodeVisualizer {
        -Dictionary~int,GameObject~ nodeObjects
        +Transform nodeContainer
        +Transform linkContainer
        +static CreateContainer(name, parent) RectTransform
        +ResetVisuals() void
        +DrawLinks() void
        +UpdateNodeLabel(discId, newLabel) void
        +SelectNodeByDiscId(discId) void
        -CreateNodeVisual(node) void
        -CreateLinkLine(from, to) void
    }

    class PingVisualizer {
        +static Instance
        +AnimatePing(srcDiscId, destDiscId, onComplete) void
        +IsAnimating() bool
    }

    NodeVisualizer --> PingVisualizer
```

---

## DC-05d: Navegación y Utilidades

```mermaid
classDiagram
    class MenuNavigator {
        +static Instance
        +SetupPanel(panel, onBack) void
        +ClearPanel() void
        -SelectAndInvoke(index) void
    }

    class MainMenuManager {
        +GameObject mainMenuPanel
        +ShowMainMenu() void
        -HideAllPanels() void
    }

    class ConnectivityTestPanel {
        +Initialize(source, dest, result, icon, pingBtn, status) void
        -ExecutePing() void
        +ResetStats() void
    }

    class IDEUMConfigurator {
        -int screenWidth
        -int screenHeight
        +SetResolution(width, height) void
    }

    MenuNavigator --> MainMenuManager
    MainMenuManager --> ConnectivityTestPanel
```

---

## Archivos Relacionados

- `Assets/Scripts/UI/UIPanelFactory.cs` — Factory de paneles de navegación/info
- `Assets/Scripts/UI/ActivityPanelFactory.cs` — Factory de paneles de actividades
- `Assets/Scripts/UI/ConfigPanelFactory.cs` — Factory de paneles de configuración
- `Assets/Scripts/UI/UIComponents.cs` — Componentes UI reutilizables
- `Assets/Scripts/UI/NodeVisualizer.cs` — Visualizador de nodos
- `Assets/Scripts/UI/PingVisualizer.cs` — Visualizador de pings
- `Assets/Scripts/UI/LinkModeController.cs` — Control de modo enlace
- `Assets/Scripts/UI/PingModeController.cs` — Control de modo ping
- `Assets/Scripts/UI/IPConfigController.cs` — Control de configuración IP
- `Assets/Scripts/UI/DevicePanelController.cs` — Panel de dispositivos
- `Assets/Scripts/UI/NodeInteractionController.cs` — Interacción con nodos
- `Assets/Scripts/UI/MenuNavigator.cs` — Navegación por menú
- `Assets/Scripts/UI/MainMenuManager.cs` — Gestor del menú principal
- `Assets/Scripts/UI/ConnectivityTestPanel.cs` — Panel de prueba de conectividad
- `Assets/Scripts/UI/IDEUMConfigurator.cs` — Configuración de pantalla IDEUM
