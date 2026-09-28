# 05 — Historial de Bugs → Archivos Modificados

> **Propósito**: Registro de bugs históricos con los archivos que fueron modificados para resolverlos.
> Útil para encontrar patrones de bugs y archivos propensos a problemas.

---

## Bugs por Severidad

### P1 — CRÍTICO: Error de RAM/Colapso

**Sesión 25 (Junio 2026)** — Causa raíz identificada y corregida.

| Causa | Archivo modificado | Fix |
|---|---|---|
| Cache de texturas | `UIComponents.cs` | `CreateRoundedRectTexture()`, `CreateDeviceIcon()`, textura 1x1 compartida |
| Sprites sin destruir | `DevicePanelController.cs` | `DestroyImmediate()` de sprites viejos en `UpdateDevicesList()` |
| Texturas por enlace | `NodeVisualizer.cs` | Textura 1x1 compartida en vez de una por enlace |
| Fugas de eventos | `ActivityLoader.cs` | Desuscripción de handlers de protocolo dinámico |
| Texturas en ping | `PingVisualizer.cs` | Limpieza de texturas |
| Limpieza general | `SceneCleanupService.cs` | Destrucción de todos los componentes |

**Archivos afectados**: UIComponents, DevicePanelController, NodeVisualizer, ActivityLoader, PingVisualizer, SceneCleanupService

---

### P2 — ALTO: Líneas de Conexión Invisibles

**Estado**: Persiste sin solución completa.

| Intento | Archivo modificado | Cambio |
|---|---|---|
| Sprite 1x1 | `NodeVisualizer.cs` | Agregar sprite de textura blanca 1x1 al Image |
| Grosor 4→6px | `NodeVisualizer.cs` | Aumentar grosor de línea |
| Sin fade-in | `NodeVisualizer.cs` | Alpha fijo en 1 |
| Cambiar a RawImage | `NodeVisualizer.cs` | RawImage no requiere sprite |
| Mover al final | `NodeVisualizer.cs` | `SetAsLastSibling()` para renderizar encima |
| Grosor 10px | `NodeVisualizer.cs` | Aumentar grosor máximo |
| DrawLinks forzado | `LinkModeController.cs` | Llamar `DrawLinks()` explícitamente |

**Archivos afectados**: NodeVisualizer, LinkModeController

---

### P3 — MEDIO: Ajuste de Tamaños UI

**Sesión 25 (Junio 2026)** — Ajustados para 4096x2160.

| Panel | Archivo | Cambios |
|---|---|---|
| Menú principal | `UIPanelFactory.cs` | 1000x1100, botones 480x145, fontSize 30 |
| Actividades | `UIPanelFactory.cs` | 1000x1100, botones 500x110, fontSize 22 |
| Conectividad | `UIPanelFactory.cs` | 820x700 |
| Instrucciones | `UIPanelFactory.cs` | 1050x850, fontSize 20 |
| Leyenda Discos | `UIPanelFactory.cs` | 960x1200, fontSize 16-18 |
| HUD Simulación | `UIPanelFactory.cs` | 560x760, botones 140x58 |
| DevicesPanel | `UIPanelFactory.cs` | 400x640, items 350x65, fontSize 20 |
| ScorePanel | `UIPanelFactory.cs` | 220x110 |
| BestRoute | `ActivityPanelFactory.cs` | 960x850, botones 700x70, fontSize 22 |
| RoutingTables | `ActivityPanelFactory.cs` | 900x750, fontSize 20 |
| StaticRouting | `ActivityPanelFactory.cs` | 960x800, fontSize 22 |
| DynamicRouting | `ActivityPanelFactory.cs` | 960x1000, fontSize 20-24 |
| Escenarios | `ActivityPanelFactory.cs` | 1000x1000, items 880x130, fontSize 24 |
| ScenarioInfo | `ActivityPanelFactory.cs` | 800x700, fontSize 18-26 |

**Archivos afectados**: UIPanelFactory, ActivityPanelFactory, UIComponents

---

### Bugfix: TouchScript Doble Click

**Problema**: TouchScript procesaba eventos UI dos veces.

| Archivo | Fix |
|---|---|
| `TouchScriptDisabler.cs` | Desactiva GameObject "TouchManager Instance" en Editor |
| `AddDeviceAtSpawn` (ActivityLoader) | Cooldown 100-200ms |
| `ToggleLinkMode` (LinkModeController) | Cooldown 150ms |
| `OnDeviceItemClicked` (DevicePanelController) | Cooldown 200ms |

---

### Bugfix: CONECTAR/DESCONECTAR

**Problema**: LinkModeController usaba campo stale y color incorrecto.

| Archivo | Fix |
|---|---|
| `LinkModeController.cs` | Convertido a singleton con `Instance` |
| `LinkModeController.cs` | Usa `TopologyManager.Instance` en vez de campo stale |
| `LinkModeController.cs` | `UpdateLinkButtonColors` controla `Image.color` directamente |
| `LinkModeController.cs` | Color rojo para modo DESCONECTAR |

---

### Bugfix: DevicePanel

**Problema**: Panel no mostraba dispositivos.

| Archivo | Fix |
|---|---|
| `DevicePanelController.cs` | Eliminado campo topology stale, usa `TopologyManager.Instance` |
| `DevicePanelController.cs` | `DestroyImmediate` del panel viejo en `RefreshDevicesPanel` |
| `DevicePanelController.cs` | Cooldown 200ms en `OnDeviceItemClicked` |

---

### Bugfix: EventSystem

**Problema**: Sin EventSystem, ningún click/toque funciona en UI.

| Archivo | Fix |
|---|---|
| `SceneSetup.cs` | Agregado `SetupEventSystem()` que crea EventSystem + InputSystemUIInputModule |

---

### Bugfix: Coordenadas TUIO

**Problema**: Discos virtuales no aparecían debajo de los físicos.

| Archivo | Fix |
|---|---|
| `TangibleBridge.cs` | `ConvertToCanvasPosition()` usa constantes fijas 1920x1080 en vez de `Display.main.systemWidth/Height` |

---

### Bugfix: DevicePanelController.RefreshDevicesPanel

**Problema**: Panel no mostraba dispositivos tras crear con botones.

| Archivo | Fix |
|---|---|
| `DevicePanelController.cs` | `RefreshDevicesPanel()` ahora llama a `UpdateDevicesList()` explícitamente |
| `DevicePanelController.cs` | Busca Canvas y TopologyManager en tiempo real |

---

### Bugfix: AddDeviceAtSpawn

**Problema**: Dispositivos creados con botones no aparecían en panel.

| Archivo | Fix |
|---|---|
| `ActivityLoader.cs` | `AddDeviceAtSpawn()` ahora crea `DevicePanelController` si no existe |

---

### Bugfix: MenuNavigator sobrescritura

**Problema**: `UpdateSelection()` sobrescribía tamaños de botones.

| Archivo | Fix |
|---|---|
| `MenuNavigator.cs` | Eliminada sobrescritura de `sizeDelta` en `UpdateSelection` |
| `MainMenuManager.cs` | Eliminada sobrescritura de `sizeDelta` en `UpdateButtonSelection` |

---

### Bugfix: Fullscreen IDEUM + Touch

**Problema**: TouchScript escalaba incorrectamente en fullscreen.

| Archivo | Fix |
|---|---|
| `SceneSetup.cs` | `SetupResolution()` cambiado a `FullScreenMode.FullScreenWindow` |
| TouchScript SDK | `setScaling()` modificado para siempre usar 1:1 |
| ProjectSettings | `fullscreenMode: 1`, `defaultScreenWidth/Height: 4096x2160` |

---

### Bugfix: Fuentes

**Problema**: Glifos faltantes por fragmentación de atlas de textura.

| Archivo | Fix |
|---|---|
| `UIComponents.cs` | `GetFont()` con cache singleton, fuente cambiada a Helvetica Neue |
| Todos los paneles | Ahora usan `UIComp.GetFont()` en vez de crear instancias propias |

---

## Archivos Más Modificados (por bugs)

| Archivo | Bugs que lo afectaron |
|---|---|
| `TopologyManager.cs` | RAM, stale references, multiplex |
| `UIComponents.cs` | RAM (texturas), fuentes, tamaños |
| `DevicePanelController.cs` | RAM, stale references, panel invisible |
| `LinkModeController.cs` | Doble click, color, stale reference |
| `SceneSetup.cs` | EventSystem, fullscreen, coordenadas |
| `NodeVisualizer.cs` | Líneas invisibles, RAM |
| `ActivityLoader.cs` | RAM, dispositivos no aparecen |
| `TangibleBridge.cs` | Coordenadas TUIO |
| `MenuNavigator.cs` | Sobrescritura de tamaños |
| `PingVisualizer.cs` | RAM (texturas) |

---

## Patrones de Bugs Comunes

| Patrón | Ejemplo | Prevención |
|---|---|---|
| **Campo stale** | LinkModeController, DevicePanelController | Usar `Singleton.Instance` en vez de cachear |
| **Sin cleanup** | RAM, texturas, sprites | `DestroyImmediate` en cada ciclo, cache de texturas |
| **Sin EventSystem** | UI no responde | Verificar `SetupEventSystem()` en SceneSetup |
| **Doble procesamiento** | TouchScript | Deshabilitar en Editor via TouchScriptDisabler |
| **Coordenadas incorrectas** | TUIO → Canvas | Usar constantes fijas, no `Display.main` |
| **Atlas fragmentado** | Fuentes, glifos | Cache singleton en `GetFont()` |
