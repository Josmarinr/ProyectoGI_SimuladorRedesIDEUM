# Manual de Desarrollador — SimuladorRedes IDEUM

> Guía técnica para contribuir, modificar y extender el proyecto.
> Unity 6000.4.5f1 · Target: Windows 10 · Input System Package (activeInputHandler=2)

---

## Índice

1. [Configuración del Entorno](#1-configuración-del-entorno)
2. [Arquitectura General](#2-arquitectura-general)
3. [Estructura del Proyecto](#3-estructura-del-proyecto)
4. [Convenciones de Código](#4-convenciones-de-código)
5. [Sistema de Eventos](#5-sistema-de-eventos)
6. [Cómo Agregar una Actividad Nueva](#6-cómo-agregar-una-actividad-nueva)
7. [Cómo Agregar un Tipo de Disco Nuevo](#7-cómo-agregar-un-tipo-de-disco-nuevo)
8. [Cómo Modificar la UI](#8-cómo-modificar-la-ui)
9. [Integración con TangibleEngine](#9-integración-con-tangibleengine)
10. [Tests](#10-tests)
11. [Build y Despliegue](#11-build-y-despliegue)
12. [Debugging](#12-debugging)
13. [Arquitectura del Sistema de Agentes (opencode)](#13-arquitectura-del-sistema-de-agentes-opencode)

---

## 1. Configuración del Entorno

### Requisitos

| Herramienta | Versión |
|-------------|---------|
| Unity Hub | Última estable |
| Unity Editor | **6000.4.5f1 (Unity 6)** |
| Build Support | Windows (x86_64) |
| IDE | Cualquier editor C# (VS, Rider, VS Code) |
| Git | Cualquier versión reciente |

### Clonar y Abrir

```bash
git clone <repo-url> SimuladorRedes
cd SimuladorRedes
```

1. Abre **Unity Hub**
2. Agrega el proyecto desde la carpeta clonada
3. Selecciona la versión **6000.4.5f1** (si no la tienes, instálala desde Unity Hub)
4. Abre el proyecto

### Verificar Configuración

Al abrir el proyecto por primera vez:

1. **Escena principal**: Abre `Assets/Main.unity` (NO `GetStarted_Scene.unity`)
2. **Input System**: Project Settings → Player → Active Input Handling → **Input System Package (New)**
3. **Build Target**: File → Build Settings → **Windows x86_64**
4. **Resolución**: Game View → 4096x2160 (canvas reference) o 1920x1080 para testing

### Solución a Problemas de Compilación

Si hay errores de compilación:

```bash
# 1. Cierra Unity
# 2. Elimina la caché de Library
rm -rf Library/
# 3. Vuelve a abrir Unity (reimportará todo)
```

---

## 2. Arquitectura General

### Capas del Sistema

```
┌─────────────────────────────────────────────────────────────┐
│                    CAPA FÍSICA (Mesa IDEUM)                 │
│   Discos PUCs → TangibleEngine Service → TCP localhost:4949 │
└────────────────────────────────┬────────────────────────────┘
                                 │
┌────────────────────────────────▼────────────────────────────┐
│              CAPA DE INTEGRACIÓN (Tangible)                  │
│   TangibleBridge → TangibleDiscManager → DiscEventHandler   │
└────────────────────────────────┬────────────────────────────┘
                                 │
┌────────────────────────────────▼────────────────────────────┐
│                CAPA DE RED (Network)                         │
│   TopologyManager → NetworkNode → RoutingTable → VLAN/ACL   │
└────────────────────────────────┬────────────────────────────┘
                                 │
┌────────────────────────────────▼────────────────────────────┐
│           CAPA DE ACTIVIDADES (Simulation)                   │
│   SceneSetup → ActivityLoader → 7 Activities → Scoring      │
└────────────────────────────────┬────────────────────────────┘
                                 │
┌────────────────────────────────▼────────────────────────────┐
│              CAPA DE PRESENTACIÓN (UI)                       │
│   UIPanelFactory → NodeVisualizer → Controllers → Paneles   │
└─────────────────────────────────────────────────────────────┘
```

### Namespaces

| Namespace | Contenido |
|-----------|-----------|
| `SimRedes` | SceneSetup, GameManager, PointerClickHandler |
| `SimRedes.Network` | TopologyManager, NetworkNode, RoutingTable, VLAN, ACL, NAT |
| `SimRedes.Tangible` | TangibleDiscManager, TangibleBridge, DiscEventHandler |
| `SimRedes.Simulation` | ActivityLoader, 7 Activities, DynamicRoutingProtocol, Scoring |
| `SimRedes.UI` | UIPanelFactory, ActivityPanelFactory, ConfigPanelFactory, UIComponents, 5 Controllers, Visualizers |
| `SimRedes.Core` | AppLogger |

### Patrón Singleton

Los managers principales usan singleton:

```csharp
public class TopologyManager : MonoBehaviour {
    public static TopologyManager Instance { get; private set; }
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }
}
```

> **Excepción**: `SceneCleanupService` usa `DontDestroyOnLoad` para persistir entre cambios de escena.

---

## 3. Estructura del Proyecto

```
Assets/
├── Scripts/
│   ├── Network/              # ~11 archivos — Núcleo de networking
│   │   ├── TopologyManager.cs    # Singleton, nodos, enlaces, pathfinding
│   │   ├── NetworkNode.cs        # Dispositivos de red
│   │   ├── NetworkLink.cs        # Conexiones entre dispositivos
│   │   ├── RoutingTable.cs       # Tablas de enrutamiento
│   │   ├── ARPTable.cs           # Tablas ARP
│   │   ├── IPValidation.cs       # Validación de IPs (estático)
│   │   ├── DiscConfiguration.cs  # Config de 18 tipos de disco
│   │   ├── VLANManager.cs        # Redes virtuales
│   │   ├── ACLManager.cs         # Listas de acceso
│   │   └── NATManager.cs         # Traducción de direcciones
│   │
│   ├── Tangible/             # ~5 archivos — Integración hardware
│   │   ├── TangibleDiscManager.cs    # Estado de discos activos
│   │   ├── TangibleBridge.cs         # Puente TE → sistema
│   │   ├── DiscEventHandler.cs       # Procesamiento de discos
│   │   ├── RouteBuilderState.cs      # Construcción parcial de rutas
│   │   └── DebugDiscSimulator.cs     # Simulación por teclado
│   │
│   ├── Simulation/           # ~15 archivos — Actividades y orquestación
│   │   ├── SceneSetup.cs           # Orquestador (~623L, refactorizado desde ~4,246L)
│   │   ├── ActivityLoader.cs       # Dispatcher (~900L)
│   │   ├── SceneCleanupService.cs  # Singleton de limpieza
│   │   ├── BuildTopologyActivity.cs    # Actividad 0
│   │   ├── FindFaultActivity.cs        # Actividad 1
│   │   ├── RoutingTablesActivity.cs    # Actividad 2
│   │   ├── BestRouteActivity.cs        # Actividad 3
│   │   ├── StaticRoutingActivity.cs    # Actividad 4
│   │   ├── DynamicRoutingActivity.cs   # Actividad 5
│   │   ├── PredefinedScenarios.cs      # Actividad 6
│   │   ├── DynamicRoutingProtocol.cs   # Motor RIP/OSPF/EIGRP
│   │   ├── ScoringSystem.cs            # Puntajes
│   │   ├── SimulationControls.cs       # ESC, P, R
│   │   └── RoutingProtocols.cs         # RoutingSimulator estático + enum
│   │
│   ├── UI/                   # ~16 archivos — Interfaces de usuario
│   │   ├── UIPanelFactory.cs        # Fábrica base: navegación + info (~834L)
│   │   ├── ActivityPanelFactory.cs  # Fábrica de paneles de actividades 0-6 (~694L)
│   │   ├── ConfigPanelFactory.cs    # Fábrica de configuración de red (~790L)
│   │   ├── UIComponents.cs          # Helpers y paleta de colores (~679L)
│   │   ├── NodeVisualizer.cs        # Visualización de nodos
│   │   ├── PingVisualizer.cs        # Animación de ping
│   │   ├── TopologyVisualizer.cs    # Visualización de enlaces
│   │   ├── MenuNavigator.cs         # Navegación por teclado
│   │   ├── MainMenuManager.cs       # Menú principal
│   │   ├── LinkModeController.cs    # Modo CONEXIÓN/DESCONEXIÓN
│   │   ├── PingModeController.cs    # Modo ping
│   │   ├── IPConfigController.cs    # Panel de IP
│   │   ├── DevicePanelController.cs # Panel de dispositivos
│   │   ├── NodeInteractionController.cs # Fachada de clicks
│   │   └── ConnectivityTestPanel.cs # Panel de pruebas
│   │
│   └── Core/                  # ~1 archivo — Utilidades
│       └── AppLogger.cs            # Logger centralizado
│
├── Editor/Tests/              # Tests unitarios EditMode
│   ├── Network/
│   │   ├── TestIPValidation.cs           # 18 tests
│   │   ├── TestRoutingTable.cs           # 14 tests
│   │   ├── TestRoutePersistence.cs       # 3 tests
│   │   └── TestDiscToRouteIntegration.cs # 10 tests
│   ├── Simulation/
│   │   ├── TestScoringSystem.cs          # 19 tests
│   │   ├── TestBestRouteActivity.cs      # 12 tests
│   │   ├── TestSceneCleanupService.cs    # 4 tests
│   │   └── TestPredefinedScenarios.cs    # 13 tests
│   └── Tangible/
│       └── TestRouteBuilderState.cs      # 15 tests
│
└── .opencode/                 # Configuración de agentes IA
    ├── agents/                # 6 agentes
    └── skills/                # 18 skills
```

---

## 4. Convenciones de Código

### Nomenclatura

| Elemento | Estilo | Ejemplo |
|----------|--------|---------|
| Clases | PascalCase | `TopologyManager` |
| Métodos | PascalCase | `AddStaticRoute()` |
| Propiedades públicas | PascalCase | `Instance` |
| Variables privadas | camelCase | `routerTables` |
| Parámetros | camelCase | `destinationIP` |
| Constantes | PascalCase | `MAX_VLANS` |
| Eventos | PascalCase prefijo On | `OnNodeAdded` |
| Namespaces | `SimRedes.*` | `SimRedes.Network` |

### Reglas Específicas

- **Idioma**: Clases/métodos en **inglés**. Strings de UI en **español**.
- **Input**: `activeInputHandler=2` (Input System Package). Migración completa — 0 usos del API viejo (`Input.GetKeyDown`, `Input.GetMouseButton`, `Input.mousePosition`).
- **UI**: Todo code-only (sin prefabs). Canvas modo Expand.
- **Destrucción**: Usar `UnityEngine.Object.Destroy` (calificar con namespace para evitar ambigüedad).
- **Tipos ambiguos**: `DeviceType` → usar `SimRedes.Network.DeviceType` (no `UnityEngine.DeviceType`).
- **Logger**: Usar `AppLogger.LogWarning()` / `AppLogger.LogError()` en lugar de `Debug.Log` directo.

### Anti-patrones a Evitar

```csharp
// ❌ MAL: FindAnyObjectByType (antes FindObjectOfType) en Update
void Update() {
    var tm = FindAnyObjectByType<TopologyManager>(); // NO
}

// ✅ BIEN: Cachear referencia en Start
TopologyManager tm;
void Start() { tm = FindAnyObjectByType<TopologyManager>(); }

// ❌ MAL: Lambda capturando variable mutable
for (int i = 0; i < 10; i++) {
    button.onClick.AddListener(() => DoSomething(i)); // i cambia!
}

// ✅ BIEN: Capturar en variable local
for (int i = 0; i < 10; i++) {
    int captured = i;
    button.onClick.AddListener(() => DoSomething(captured));
}

// ❌ MAL: No desuscribirse en OnDestroy
void OnDestroy() {
    // topology.OnNodeAdded -= OnNodeAdded; // FALTA!
}

// ✅ BIEN: Siempre desuscribir
void OnDestroy() {
    if (topology != null)
        topology.OnNodeAdded -= OnNodeAdded;
}
```

### Null Safety

```csharp
// Verificar null en objetos Unity destruidos
if (nodeObject != null) { // Equivalente a (bool)nodeObject
    nodeObject.SetActive(true);
}

// Eventos con null check
OnNodeAdded?.Invoke(node);

// Limpieza de referencias rotas
void CleanupNullReferences() {
    var toRemove = nodeObjects
        .Where(kvp => kvp.Value == null)
        .Select(kvp => kvp.Key)
        .ToList();
    foreach (var key in toRemove) nodeObjects.Remove(key);
}
```

---

## 5. Sistema de Eventos

### Publicadores y Suscriptores

| Evento | Publicador | Suscriptores |
|--------|------------|--------------|
| `OnNodeAdded` | `TopologyManager.AddNode()` | `NodeVisualizer`, `BuildTopologyActivity` |
| `OnNodeRemoved` | `TopologyManager.RemoveNode()` | `NodeVisualizer`, `BuildTopologyActivity` |
| `OnTopologyChanged` | Varios | `DevicePanelController`, `NodeVisualizer` |
| `OnDiscPlaced` | `TangibleDiscManager` | `DiscEventHandler` |
| `OnDiscMoved` | `TangibleDiscManager` | `DiscEventHandler` |
| `OnProtocolLog` | `DynamicRoutingProtocol` | `ActivityLoader` → Panel UI |
| `OnConvergence` | `DynamicRoutingProtocol` | `ActivityLoader` → Panel UI |

### Patrón de Subscripción

```csharp
public class MyComponent : MonoBehaviour {
    private TopologyManager topology;

    void Start() {
        topology = TopologyManager.Instance;
        topology.OnNodeAdded += OnNodeAdded;
        topology.OnTopologyChanged += OnTopologyChanged;
    }

    void OnDestroy() {
        if (topology != null) {
            topology.OnNodeAdded -= OnNodeAdded;
            topology.OnTopologyChanged -= OnTopologyChanged;
        }
    }

    private void OnNodeAdded(NetworkNode node) { }
    private void OnTopologyChanged() { }
}
```

---

## 6. Cómo Agregar una Actividad Nueva

### Paso a Paso

Supongamos que quieres agregar una **Actividad 7: "Subnetting"**:

#### 1. Crear la Clase de Actividad

`Assets/Scripts/Simulation/SubnettingActivity.cs`:

```csharp
using UnityEngine;
using SimRedes.Network;

namespace SimRedes.Simulation {
    public class SubnettingActivity : MonoBehaviour {
        public Text titleText;
        public Text descriptionText;
        public Button checkButton;
        public Text feedbackText;

        private TopologyManager topology;

        void Start() {
            topology = TopologyManager.Instance;
            SetupActivity();
        }

        public void SetupActivity() {
            // Inicializar la actividad
        }

        public void OnCheckClicked() {
            // Validar la respuesta del estudiante
        }
    }
}
```

#### 2. Crear el Panel UI en la Factory correspondiente

Según el dominio del panel, usar `ActivityPanelFactory`, `ConfigPanelFactory` o `UIPanelFactory`. Agregar el método estático:

```csharp
public static GameObject CreateSubnettingPanel(Transform parent) {
    // Usar CreateRoundedPanel, CreateMenuButton, CreateInfoText...
}
```

#### 3. Registrar en ActivityLoader

En `ActivityLoader.SelectActivity()`, agregar el case:

```csharp
case 7: // Subnetting
    var subnetting = gameObject.AddComponent<SubnettingActivity>();
    var subnetPanel = ActivityPanelFactory.CreateSubnettingPanel(canvasTransform, onBack);   // o ConfigPanelFactory según dominio
    subnetting.checkButton.onClick.AddListener(subnetting.OnCheckClicked);
    break;
```

#### 4. Agregar al Menú de Actividades

En `UIPanelFactory.CreateActivitiesPanel()`, agregar un botón más.

#### 5. Escribir Tests

En `Assets/Editor/Tests/`, crear `TestSubnettingActivity.cs`.

---

## 7. Cómo Agregar un Tipo de Disco Nuevo

### Paso a Paso

Supongamos que quieres agregar un **Disco 19: "Servidor Web"**:

#### 1. Agregar al Enum `DiscType`

En `DiscConfiguration.cs`:

```csharp
public enum DiscType {
    Router, Switch, PC, Enlace, Fallo, Protocolo,
    RedDestino, Metrica, InterfazSalida, ModoEnrutamiento, IpRoute,
    Destino, Mascara, ProximoSalto, Vecino, AnunciarRed, Costo, BW,
    ServidorWeb  // ← NUEVO
}
```

#### 2. Agregar Configuración por Defecto

En `DiscConfiguration.DefaultConfiguration`:

```csharp
new DiscConfiguration {
    DiscId = 19,
    Type = DiscType.ServidorWeb,
    Label = "Servidor Web",
    DisplayColor = Color.gray,
    Description = "Servidor web HTTP"
}
```

#### 3. Agregar Comportamiento en DiscEventHandler

En `DiscEventHandler.HandleDiscPlaced()`:

```csharp
if (config.Type == DiscType.ServidorWeb) {
    // Comportamiento especial para servidores web
}
```

---

## 8. Cómo Modificar la UI

### Creación de Paneles

Todos los paneles se crean desde las 3 factories (`UIPanelFactory`, `ActivityPanelFactory`, `ConfigPanelFactory`) usando helpers de `UIComponents.cs`:

```csharp
// Ejemplo: crear un panel simple
var panel = UIComp.CreateRoundedPanel(parent, 400, 300, UIColors.surfacePanel);
var title = UIComp.CreateInfoText(panel, "Título", font, 24, UIColors.textPrimary);
var button = UIComp.CreateMenuButton(panel, "btnAceptar", "ACEPTAR", pos, size, font, 14);
```

### Colores y Estilos

Usar la paleta definida en `UIComponents.Colors`:

| Color | Hex | Uso |
|-------|:----:|-----|
| `backgroundBase` | `#0D1117` | Fondo |
| `surfacePanel` | `#161B22` | Paneles |
| `borderAccent` | `#58A6FF` | Bordes |
| `textPrimary` | `#E6EDF3` | Texto principal |
| `buttonNormal` | `#213F66` | Botones |

### Añadir un Botón Nuevo al TopologyInfoPanel

En `ActivityLoader.StartSimulation()` o en el método de creación del HUD:

```csharp
var newButton = UIComponents.CreateMenuButton(panel, "MI BOTÓN", 180, 40);
newButton.GetComponent<Button>().onClick.AddListener(() => {
    // Acción
});
```

### Controladores de Interacción

Los 5 controladores extraídos de SceneSetup gestionan la interacción:

| Controlador | Responsabilidad |
|-------------|----------------|
| `LinkModeController` | Modo CONEXIÓN/DESCONEXIÓN |
| `PingModeController` | Modo ping + selector |
| `IPConfigController` | Panel IP + teclado |
| `DevicePanelController` | Dispositivos + score |
| `NodeInteractionController` | Fachada: redirige clicks |

Para agregar un nuevo modo de interacción:

1. Crear un nuevo controller (ej: `VLANAssignmentController`)
2. Agregarlo en `SceneSetup.SetupManagers()`
3. Integrarlo en `NodeInteractionController.HandleNodeClick()` si es necesario

---

## 9. Integración con TangibleEngine

### Flujo de Datos

```
Mesa IDEUM → TE Service (TCP:4949)
  → TangibleEngine SDK → OnTangibleAdded(tangible)
    → TangibleBridge.HandleTangibleAdded()
      → MapPatternToDiscType(patternId)  # pattern 1-6 → DiscType
      → ConvertToCanvasPosition(x, y)     # 1920x1080 → 4096x2160
      → TangibleDiscManager.SimulateDiscPlaced(type, pos)
        → OnDiscPlaced(uniqueId, pos)
          → DiscEventHandler.HandleDiscPlaced()
```

### Mapeo tangibleId → uniqueId

`TangibleBridge` mantiene un `Dictionary<int,int>` que mapea:

- **Key**: `tangible.Id` (ID que asigna el TE a cada instancia física)
- **Value**: `uniqueId` (ID interno, 100, 101, 102...)

```csharp
// HandleTangibleAdded: Crear mapping
int uniqueId = discManager.SimulateDiscPlaced(discType, canvasPos);
tangibleIdToUniqueId[tangible.Id] = uniqueId;

// HandleTangibleUpdated: Usar mapping
if (tangibleIdToUniqueId.TryGetValue(tangible.Id, out int uniqueId)) {
    discManager.UpdateDiscPosition(uniqueId, canvasPos);
}

// HandleTangibleRemoved: Usar mapping
if (tangibleIdToUniqueId.TryGetValue(tangible.Id, out int uniqueId)) {
    discManager.SimulateDiscRemoved(uniqueId);
    tangibleIdToUniqueId.Remove(tangible.Id);
}
```

### Conversión de Coordenadas

```csharp
// TE/TUIO siempre reporta en 1920x1080 (resolucion del touch frame IDEUM)
const float TUIO_WIDTH = 1920f;
const float TUIO_HEIGHT = 1080f;
return new Vector2(
    screenPosition.x * (canvasWidth / TUIO_WIDTH),
    screenPosition.y * (canvasHeight / TUIO_HEIGHT)
);
```

> **Nota**: Anteriormente se usaba `Display.main.systemWidth/Height`, pero esto devolvía la resolución de pantalla (4096x2160) en lugar de la resolución TUIO (1920x1080), causando que los discos virtuales aparecieran en posiciones incorrectas al usar pantalla completa.

### Debug sin Hardware

En `DebugDiscSimulator.cs`:

```csharp
public bool enableSimulation = false; // false = producción, true = debug
```

Para testing en PC:

1. Abre `DebugDiscSimulator.cs`
2. Cambia `enableSimulation = true`
3. Usa teclas 1-6, C, P, R para simular discos
4. Al terminar, revierte a `false`

---

## 10. Tests

### Estructura

Los tests están en `Assets/Editor/Tests/` y son **328 tests EditMode** (18 suites):

| Suite | Tests | Suite | Tests |
|-------|:-----:|-------|:-----:|
| TestIPValidation | 18 | TestRoutingTable | 18 |
| TestRouteBuilderState | 15 | TestRoutePersistence | 3 |
| TestDiscToRouteIntegration | 10 | TestTopologyManager | 32 |
| TestDynamicRoutingProtocol | 24 | TestActivityLoader | 20 |
| TestDiscEventHandler | 18 | TestTangibleBridge | 15 |
| TestScoringSystem | 19 | TestBestRouteActivity | 12 |
| TestSceneCleanupService | 4 | TestPredefinedScenarios | 13 |
| TestARPTable | 13 | TestVLANManager | 18 |
| TestACLManager | 29 | TestNATManager | 25 |

### Cómo Ejecutar

**Desde Unity Editor:**

1. Window → General → Test Runner
2. Pestaña **EditMode**
3. Run All (o seleccionar suite específica)

**Desde CLI (si Unity está disponible):**

```bash
/Applications/Unity/Hub/Editor/6000.4.5f1/Unity.app/Contents/MacOS/Unity \
  -runTests -testPlatform EditMode \
  -projectPath /ruta/al/proyecto \
  -logFile - \
  -testResults /tmp/test-results.xml
```

### Cómo Escribir un Test Nuevo

Patrón AAA (Arrange-Act-Assert):

```csharp
using NUnit.Framework;
using SimRedes.Network;

namespace Tests.EditMode.Network {
    [TestFixture]
    public class TestMyFeature {
        [Test]
        public void MyTest_ExpectedBehavior() {
            // Arrange
            var table = new RoutingTable();

            // Act
            table.AddStaticRoute("10.0.0.0", "255.0.0.0", "10.0.0.1", "G0/0");

            // Assert
            Assert.AreEqual(1, table.GetAllEntries().Count);
            Assert.AreEqual("Static", table.GetAllEntries()[0].Protocol);
        }
    }
}
```

### Cobertura Actual (328 tests, 18 suites)

Todas las suites principales tienen cobertura completa. Las 18 suites cubren:
- **Network**: IPValidation, RoutingTable, RoutePersistence, DiscToRouteIntegration, TopologyManager, ARPTable, VLANManager, ACLManager, NATManager
- **Tangible**: RouteBuilderState, DiscEventHandler, TangibleBridge
- **Simulation**: ScoringSystem, BestRouteActivity, SceneCleanupService, PredefinedScenarios, DynamicRoutingProtocol, ActivityLoader
---

## 11. Build y Despliegue

### Build para Mesa IDEUM (Producción)

```yaml
DebugDiscSimulator.enableSimulation: false
TangibleEngine: Service mode (TCP localhost:4949)
Escena principal: Assets/Main.unity
Target: Windows x86_64
```

Pasos:

1. Verificar que `Assets/Main.unity` es la única escena en Build Settings
2. File → Build Settings → Windows → Build
3. Copiar la carpeta generada a la mesa IDEUM

### Build para PC Normal (Testing)

```yaml
DebugDiscSimulator.enableSimulation: true
TangibleEngine: Fallo silencioso (no afecta la app)
Escena principal: Assets/Main.unity
Target: Windows x86_64
```

Pasos:

1. Cambiar `DebugDiscSimulator.enableSimulation = true`
2. Build normalmente
3. Usar teclas 1-6 para simular discos
4. **Reversión**: Volver a poner `enableSimulation = false` después del build

### Solución a Pantalla Negra

Si el build abre en negro:

1. Abrir File → Build Settings
2. Verificar que `Assets/Main.unity` está en Scenes In Build (índice 0)
3. Eliminar `Assets/Scenes/GetStarted_Scene.unity` de la lista
4. Re-buildear

---

## 12. Debugging

### Logging

Usar `AppLogger` en lugar de `Debug.Log` directo:

```csharp
AppLogger.LogWarning("TopologyManager", "Nodo duplicado: " + discId);
AppLogger.LogError("DiscEventHandler", "No se encontró router cerca");
```

> `EnableLogging = false` en producción. Para ver logs en desarrollo, cambiar a `true`.

### Errores Comunes y Soluciones

| Error | Causa | Solución |
|-------|-------|----------|
| `MissingReferenceException` | GameObject destruido pero referencia viva | Agregar null checks + `CleanupNullReferences()` |
| `NullReferenceException` en evento | Suscriptor no se desuscribió en OnDestroy | Siempre usar `-=` en OnDestroy |
| UI borrosa | CanvasScaler mal configurado | Usar modo Expand, resolución referencia 4096x2160 |
| Sin respuesta en botones UI | EventSystem no existe | SceneSetup.SetupEventSystem() crea EventSystem + InputSystemUIInputModule |
| Nodos se sobreponen | Mismo uniqueId | TangibleDiscManager genera IDs únicos (100, 101...) |
| Disco de routing no funciona | Router no encontrado en radio | Aumentar `routerProximityRadius` (default 150) |

### Profiling en Unity

1. Window → Analysis → Profiler
2. Enfocarse en: CPU Usage, Rendering, UI
3. Buscar picos en `NodeVisualizer.DrawLinks()` (llamado en cada `OnTopologyChanged`)

---

## 13. Arquitectura del Sistema de Agentes (opencode)

### Agentes Disponibles

| Agente | Rol | Tools |
|--------|-----|-------|
| `main` | Coordinador autónomo | task, edit, bash |
| `architect` | Diseña planes | read-only |
| `programmer` | Implementa código | edit, bash |
| `reviewer` | Revisa cambios | read-only |
| `tester` | Ejecuta tests | bash |
| `builder` | Build & deploy | edit, bash |

### Skills (18 disponibles)

Los skills están en `.opencode/skills/` y proveen guías de referencia para tareas específicas:

| Skill | Cuándo usarlo |
|-------|---------------|
| `unity-code-style` | Escribir o revisar código C# |
| `testing-guide` | Escribir o ejecutar tests |
| `build-and-deploy` | Buildear o desplegar |
| `ideum-integration` | Trabajar con discos o TangibleEngine |
| `dynamic-routing` | Implementar RIP/OSPF/EIGRP |
| `network-tables` | IP, routing, conectividad |
| `menu-navigation` | Menús, ESC, navegación |
| `unity-ui-buttons` | Botones, paneles, colores |

### Skills y su Propósito (Lista Completa)

| Skill | Archivo |
|-------|---------|
| `agent-workflow` | Flujo de trabajo entre agentes |
| `best-practices` | Patrones de diseño y anti-patrones |
| `build-and-deploy` | Build para IDEUM/PC, pantalla negra fix |
| `debugging` | Logging, profiling, errores comunes |
| `dynamic-routing` | RIP/OSPF/EIGRP, convergencia |
| `ideum-integration` | TangibleEngine, discos, bridge |
| `manual-links` | CONECTAR/DESCONECTAR |
| `menu-navigation` | MenuNavigator, ESC, flechas |
| `network-tables` | IP, routing, ARP, ping |
| `predefined-scenarios` | 5 escenarios académicos |
| `scoring-system` | Puntajes, notas, evaluación |
| `testing-guide` | Cómo escribir y ejecutar tests |
| `topology-detection` | Detección automática de topología |
| `ui-performance` | Canvas, borrosidad, optimización |
| `unity-code-style` | Naming, convenciones, anti-patrones |
| `unity-scene-setup` | Paneles, textos, FindUITexts |
| `unity-ui-buttons` | Botones, colores, textos |
| `vlan-acl-nat` | VLANs, ACLs, NAT |

### Flujo de Trabajo

```
Usuario → main → architect → programmer → reviewer → tester → main → Usuario
         ↑______________________________________________________|
```

Para tareas simples (1-2 tool calls), `main` las hace directamente.
Para tareas complejas (2+ archivos), usa el flujo completo.

---

## Apéndice A: Paleta de Colores

```csharp
UIComponents.Colors.backgroundBase   // #0D1117 — Fondo base
UIComponents.Colors.surfacePanel     // #161B22 — Superficie de paneles
UIComponents.Colors.surfaceElevated  // #21262D — Elementos elevados
UIComponents.Colors.borderAccent     // #58A6FF — Borde azul acento
UIComponents.Colors.textPrimary      // #E6EDF3 — Texto principal off-white
UIComponents.Colors.textSecondary    // #8B949E — Texto secundario
UIComponents.Colors.textAccent       // #58A6FF — Texto de acento
UIComponents.Colors.buttonNormal     // #213F66 — Botón normal
UIComponents.Colors.buttonHover      // #2D5066 — Botón hover
```

## Apéndice B: Constantes Importantes

| Constante | Valor | Ubicación |
|-----------|:-----:|-----------|
| `BASE_DISC_ID` | 100 | `TangibleDiscManager.cs` |
| `linkDistanceThreshold` | 300px | `DiscEventHandler.cs` |
| `routerProximityRadius` | 150px | `DiscEventHandler.cs` |
| `advertisementInterval` | 3s | `DynamicRoutingProtocol.cs` |
| `maxDynamicIterations` | 10 | `DynamicRoutingProtocol.cs` |
| `DEFAULT_VLAN` | 1 | `VLANManager.cs` |
| `MAX_VLANS` | 4094 | `VLANManager.cs` |
| Resolución Canvas | 4096x2160 | `SceneSetup.cs` |

---

> **Documentación generada:** Junio 2026  
> **Proyecto:** SimuladorRedes IDEUM · Unity 6000.4.5f1
