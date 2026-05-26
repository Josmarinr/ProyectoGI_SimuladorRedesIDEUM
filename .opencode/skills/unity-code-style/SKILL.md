---
name: unity-code-style
description: >-
  Use when writing or reviewing C# code for SimuladorRedes IDEUM. Covers
  naming conventions, Unity lifecycle rules, project-specific code
  standards, file organization, and anti-patterns to avoid. Every agent
  should load this before writing code or reviewing changes.
---

# Skill: Unity Code Style & Conventions

## Naming Conventions

| Elemento | Regla | Ejemplo |
|----------|-------|---------|
| Clases públicas | PascalCase | `TopologyManager`, `NetworkNode` |
| Métodos públicos | PascalCase | `AddNode()`, `FindBestRoute()` |
| Métodos privados | PascalCase | `SetupManagers()`, `DetectTopology()` |
| Variables privadas | camelCase | `currentTopology`, `linkModeFirstNode` |
| Variables públicas/prop | PascalCase | `Instance`, `OnNodeAdded` |
| Parámetros | camelCase | `sourceDiscId`, `destDiscId` |
| Constantes | UPPER_SNAKE / Pascal | `BASE_DISC_ID`, `MaxHops` |
| Namespaces | PascalCase | `SimRedes.Network`, `SimRedes.UI` |
| Interfaces | I + Pascal | `IDiscHandler`, `IRoutingProtocol` |
| Archivos .cs | PascalCase, match class | `TopologyManager.cs` |

## Unity Lifecycle Rules

### Orden correcto de ejecución
```csharp
private void Awake()   // 1er: inicializar referencias propias
private void Start()    // 2do: buscar referencias externas
private void Update()   // frames: solo lógica que cambia por frame
private void OnDestroy() // cleanup: Unsubscribe, null references
```

### Awake vs Start
```csharp
// Awake: inicializar estado propio (no depende de otros)
private void Awake() {
    Instance = this;
    nodes = new Dictionary<int, NetworkNode>();
}

// Start: buscar referencias externas (otros managers)
private void Start() {
    topology = FindObjectOfType<TopologyManager>();
    if (topology == null) return; // SIEMPRE verificar null
}
```

### Cleanup obligatorio en OnDestroy
```csharp
private void OnDestroy() {
    if (topology != null) {
        topology.OnNodeAdded -= OnNodeAdded;
        topology.OnNodeRemoved -= OnNodeRemoved;
        topology.OnTopologyChanged -= OnTopologyChanged;
    }
}
```

## Anti-patrones a evitar

### ❌ FindObjectOfType en Update
```csharp
// MAL: busca cada frame
void Update() {
    var topology = FindObjectOfType<TopologyManager>();
}

// BIEN: cachear en Start
private TopologyManager topology;
void Start() { topology = FindObjectOfType<TopologyManager>(); }
void Update() { if (topology != null) topology.DoSomething(); }
```

### ❌ Destroy en lugar de DestroyImmediate (y viceversa)
```csharp
// En runtime: usar Destroy siempre
Destroy(gameObject);

// En Editor-only code: DestroyImmediate es aceptable
#if UNITY_EDITOR
DestroyImmediate(gameObject);
#endif
```

### ❌ Object ambiguo
```csharp
// MAL: Object es ambiguo (System vs UnityEngine)
Object.Destroy(gameObject);

// BIEN: calificar siempre
UnityEngine.Object.Destroy(gameObject);
```

### ❌ DeviceType ambiguo
```csharp
// MAL: puede ser SimRedes.Network.DeviceType o UnityEngine.DeviceType
DeviceType type = DeviceType.Router;

// BIEN: namespace explícito
SimRedes.Network.DeviceType type = SimRedes.Network.DeviceType.Router;
```

## Project-Specific Rules

| Regla | Estándar |
|-------|----------|
| **Idioma** | Clases/métodos en inglés, strings UI en español |
| **Input** | `activeInputHandler=2` (Both). Legacy `Input.GetKeyDown()` funciona. Código nuevo puede usar `UnityEngine.InputSystem`. |
| **Target** | Windows 10, 1920x1080 |
| **UI** | Code-only (sin prefabs). Canvas Expand. Paneles con `UIPanelFactory`. |
| **Logger** | Usar `AppLogger.LogWarning/LogError`. No `Debug.Log` directo. |
| **Tests** | 47 EditMode en `Assets/Editor/Tests/` — no romperlos |

## Estructura de archivos recomendada

```csharp
using UnityEngine;
using SimRedes.Network;  // namespaces del proyecto

namespace SimRedes.UI      // namespace acorde a la carpeta
{
    public class MiComponente : MonoBehaviour
    {
        // 1. Singleton / static
        public static MiComponente Instance { get; private set; }

        // 2. Serialized fields (exposed in Inspector)
        [SerializeField] private float threshold = 300f;

        // 3. Public events
        public event System.Action OnSomething;

        // 4. Private fields
        private TopologyManager topology;

        // 5. Unity lifecycle
        private void Awake() { ... }
        private void Start() { ... }
        private void OnDestroy() { ... }

        // 6. Public methods
        public void DoSomething() { ... }

        // 7. Private methods
        private void HandleEvent() { ... }
    }
}
```

## Debug.Log vs AppLogger

```csharp
// Usar estos (EnableLogging controla si se muestran):
AppLogger.LogWarning("TopologyManager", "Nodo null ignorado");
AppLogger.LogError("SceneSetup", "Panel no encontrado");

// NO usar directamente:
Debug.Log("mensaje"); // AppLogger ya encapsula Debug
```

## Formato de respuesta del agente

Cuando reportes cambios de código, usa este formato:

```markdown
## Cambios realizados

### Modificados
- `archivo.cs`: [qué cambió]

### Creados
- `NuevoArchivo.cs`: [propósito]

### Convenciones aplicadas
- ✅ Naming PascalCase
- ✅ FindObjectOfType cacheado en Start
- ✅ OnDestroy limpia eventos
- ✅ strings UI en español
```
