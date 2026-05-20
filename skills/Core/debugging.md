# Skill: Unity Debugging & Profiling

## Descripción
Técnicas para debuggear y optimizar el proyecto.

## Debug en Editor

```csharp
// Logging estructurado
UnityEngine.Debug.Log($"[Scenarios] Creado: {deviceType} en {position}");
UnityEngine.Debug.LogError($"[Scenarios] Escenario {index} no encontrado");

// Log condicional
[Conditional("DEBUG")]
void LogDebug(string message) { ... }
```

## Encontrar Objetos en Escena

```csharp
// Buscar por nombre
GameObject panel = GameObject.Find("TopologyInfoPanel");
GameObject[] panels = GameObject.FindGameObjectsWithTag("UI");

// Buscar por tipo
var canvases = FindObjectsOfType<Canvas>();
var texts = FindObjectsOfType<UnityEngine.UI.Text>();
```

## Profiling de UI

```csharp
// Medir tiempo de operación
using (new ScopedTimer("BuildScenarioTopology"))
{
    BuildScenarioTopology(scenario);
}

// ScopedTimer helper
public class ScopedTimer : IDisposable
{
    System.Diagnostics.Stopwatch sw;
    string label;
    
    public ScopedTimer(string label)
    {
        this.label = label;
        sw = System.Diagnostics.Stopwatch.StartNew();
    }
    
    public void Dispose()
    {
        sw.Stop();
        UnityEngine.Debug.Log($"{label}: {sw.ElapsedMilliseconds}ms");
    }
}
```

## Errores Comunes

### MissingReference
```csharp
// Verificar antes de usar
if (topology != null) { topology.AddNode(...); }

// Cleanup automático
void CleanupNullReferences()
{
    var texts = FindObjectsOfType<Text>();
    foreach (var t in texts)
    {
        if (t == null) continue;
    }
}
```

### Lambdas Capturando Valores
```csharp
// Problema: closure captura referencia
btn.onClick.AddListener(() => OnNodeClicked(node)); // node puede ser null

// Solución: capturar valor
int capturedId = node.DiscId;
btn.onClick.AddListener(() => OnNodeClicked(capturedId));
```

## Build Errors

- Cerrar Unity
- Eliminar carpeta `Library`
- Abrir proyecto de nuevo
- Recompilar

## UI Debug

```csharp
// Mostrar orden de sorting
var canvas = GetComponent<Canvas>();
UnityEngine.Debug.Log($"Canvas sorting: {canvas.sortingOrder}");
UnityEngine.Debug.Log($"Canvas renderMode: {canvas.renderMode}");
```