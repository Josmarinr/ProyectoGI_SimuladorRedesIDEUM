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
| Clases publicas | PascalCase | TopologyManager, NetworkNode |
| Metodos publicos | PascalCase | AddNode(), FindBestRoute() |
| Metodos privados | PascalCase | SetupManagers(), DetectTopology() |
| Variables privadas | camelCase | currentTopology, linkModeFirstNode |
| Variables publicas/prop | PascalCase | Instance, OnNodeAdded |
| Parametros | camelCase | sourceDiscId, destDiscId |
| Constantes | UPPER_SNAKE / Pascal | BASE_DISC_ID, MaxHops |
| Namespaces | PascalCase | SimRedes.Network, SimRedes.UI |
| Interfaces | I + Pascal | IDiscHandler, IRoutingProtocol |
| Archivos .cs | PascalCase, match class | TopologyManager.cs |

## Unity Lifecycle Rules

### Orden correcto
```csharp
private void Awake()     // 1ro: inicializar referencias propias
private void Start()     // 2do: buscar referencias externas
private void Update()    // frames: solo logica que cambia por frame
private void OnDestroy() // cleanup: Unsubscribe, null references
```

### Awake vs Start
```csharp
// Awake: estado propio (no depende de otros)
private void Awake() { Instance = this; nodes = new Dictionary<int, NetworkNode>(); }

// Start: referencias externas (otros managers)
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
    }
}
```

## Anti-patrones a evitar

### FindObjectOfType en Update
```csharp
// MAL: busca cada frame
void Update() { var topology = FindObjectOfType<TopologyManager>(); }
// BIEN: cachear en Start
private TopologyManager topology;
void Start() { topology = FindObjectOfType<TopologyManager>(); }
```

### Destroy vs DestroyImmediate
```csharp
// Runtime: usar Destroy siempre
UnityEngine.Object.Destroy(gameObject);
// Editor-only: DestroyImmediate aceptable con #if UNITY_EDITOR
```

### Object ambiguo
```csharp
// MAL: Object ambiguo (System vs UnityEngine)
Object.Destroy(gameObject);
// BIEN: calificar siempre
UnityEngine.Object.Destroy(gameObject);
```

### DeviceType ambiguo
```csharp
// MAL: puede ser SimRedes.Network.DeviceType o UnityEngine.DeviceType
// BIEN: namespace explicito
SimRedes.Network.DeviceType type = SimRedes.Network.DeviceType.Router;
```

## Project-Specific Rules

| Regla | Estandar |
|-------|----------|
| Idioma | Clases/metodos en ingles, strings UI en espanol |
| Input | activeInputHandler=2 (Both). Usar UnityEngine.InputSystem. |
| Target | Windows 10, 1920x1080 |
| UI | Code-only (sin prefabs). Canvas Expand. Paneles con factories. |
| Logger | AppLogger.LogWarning/LogError. No Debug.Log directo. |
| Tests | 209 EditMode en Assets/Editor/Tests/ -- no romperlos |

## Estructura de archivos recomendada

```csharp
using UnityEngine;
using SimRedes.Network;

namespace SimRedes.UI
{
    public class MiComponente : MonoBehaviour
    {
        // 1. Singleton / static
        public static MiComponente Instance { get; private set; }
        // 2. Serialized fields
        [SerializeField] private float threshold = 300f;
        // 3. Public events
        public event System.Action OnSomething;
        // 4. Private fields
        private TopologyManager topology;
        // 5. Unity lifecycle
        private void Awake() { }
        private void Start() { }
        private void OnDestroy() { }
        // 6. Public methods
        public void DoSomething() { }
        // 7. Private methods
        private void HandleEvent() { }
    }
}
```

## Debug.Log vs AppLogger

```csharp
// Usar estos (EnableLogging controla si se muestran):
AppLogger.LogWarning("TopologyManager", "Nodo null ignorado");
AppLogger.LogError("SceneSetup", "Panel no encontrado");
// NO usar Debug.Log directo -- AppLogger ya encapsula Debug
```
