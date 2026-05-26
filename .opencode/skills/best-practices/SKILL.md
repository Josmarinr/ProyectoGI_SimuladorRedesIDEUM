---
name: best-practices
description: >-
  Reference for architectural patterns and best practices used in
  SimuladorRedes IDEUM. Covers singleton managers, event system (Pub/Sub),
  centralized state, UI patterns, performance tips, null safety, lambda
  capture, data flow diagrams, and color palette. Use when writing new
  code or refactoring existing code.
---

# Skill: Buenas Prácticas y Arquitectura

## Patrones de Diseño

### 1. Singleton para Managers

```csharp
public class TopologyManager : MonoBehaviour
{
    public static TopologyManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }
}
```

### 2. Event System (Pub/Sub)

```csharp
public class TopologyManager : MonoBehaviour
{
    public event Action<NetworkNode> OnNodeAdded;
    public event Action<NetworkNode> OnNodeRemoved;
    public event Action OnTopologyChanged;

    public void AddNode(NetworkNode node)
    {
        nodes.Add(node);
        OnNodeAdded?.Invoke(node);
        OnTopologyChanged?.Invoke();
    }
}
```

**Usar en listeners:**
```csharp
private void OnDestroy()
{
    if (topologyManager != null)
    {
        topologyManager.OnNodeAdded -= OnNodeAdded;
    }
}
```

### 3. Estado Centralizado

```csharp
public class TangibleDiscManager
{
    public static TangibleDiscManager Instance { get; private set; }
    private Dictionary<int, Vector2> activeDiscs = new Dictionary<int, Vector2>();
    public event Action<int, Vector2> OnDiscPlaced;
}
```

## Buenas Prácticas UI

### 0. Arquitectura de Factories (3 capas)

Los paneles se organizan en 3 factories según su dominio, todas en namespace `SimRedes.UI`:

| Factory | Responsabilidad | Métodos |
|---------|----------------|---------|
| `UIPanelFactory` | Navegación + Información | CreateMainMenu, CreateActivitiesPanel, CreateScorePanel, CreateDevicesPanel, CreateConnectivityPanel, CreateInstructionsPanel, CreateDiscLegendPanel |
| `ActivityPanelFactory` | Paneles de actividades (0-6) | CreateBuildTopologyInfoPanel, CreateBestRoutePanel, CreateFindFaultPanel, CreateRoutingTablesPanel, CreateStaticRoutingPanel, CreateDynamicRoutingPanel, CreateScenariosPanel, CreateScenarioInfoPanel |
| `ConfigPanelFactory` | Configuración de red | CreateIPConfigPanel, CreateARPPanel, CreateRoutingPanel, CreateAddRoutePanel, CreateVLANPanel, CreateACLPanel, CreateNATPanel |

**Para agregar un panel nuevo:**
1. Identificar el dominio (actividad, configuración o navegación/info)
2. Agregar el método en la factory correspondiente
3. Usar `UIComponents` para botones, textos y paneles base
4. Los helpers compartidos (GetFont, CreateNumericKeypad, CreateConfigField, CreateDropdown) están en `UIPanelFactory` como `internal static`

### 1. Alias para Namespaces

```csharp
using UIComp = SimRedes.UI.UIComponents;
using UIColors = SimRedes.UI.UIComponents.Colors;
```

### 2. Limpiar Paneles Antes de Crear

```csharp
private void CreateMyPanel(Transform parent)
{
    var existing = GameObject.Find("MyPanel");
    if (existing != null) Destroy(existing);

    var panel = UIComp.CreateRoundedPanel(parent, ...);
}
```

### 3. Posicionamiento de Paneles

**Centro de pantalla:**
```csharp
rect.anchorMin = new Vector2(0.5f, 0.5f);
rect.anchorMax = new Vector2(0.5f, 0.5f);
rect.anchoredPosition = Vector2.zero;
```

**Esquina inferior izquierda:**
```csharp
rect.anchorMin = new Vector2(0f, 0f);
rect.anchorMax = new Vector2(0f, 0f);
rect.pivot = new Vector2(0f, 0f);
rect.anchoredPosition = new Vector2(20, 20);
```

**Esquina superior derecha:**
```csharp
rect.anchorMin = new Vector2(1f, 1f);
rect.anchorMax = new Vector2(1f, 1f);
rect.pivot = new Vector2(1f, 1f);
rect.anchoredPosition = new Vector2(-20, -20);
```

## Evitar Errores Comunes

### 1. MissingReference - Nodos Destruidos

**MAL:**
```csharp
var outline = kvp.Value.GetComponent<Outline>(); // kvp.Value puede ser null
```

**BIEN:**
```csharp
foreach (var kvp in nodeObjects)
{
    if (kvp.Value == null) continue;
    var outline = kvp.Value.GetComponent<Outline>();
}
```

### 2. Lambdas Capturando Variables

**MAL:**
```csharp
AddClickEvent(trigger, () => OnNodeClicked(node.DiscId));
```

**BIEN:**
```csharp
int capturedDiscId = node.DiscId;
AddClickEvent(trigger, () => OnNodeClicked(capturedDiscId));
```

### 3. No Unsubscribe en OnDestroy

```csharp
private void OnDestroy()
{
    if (topology != null)
    {
        topology.OnNodeAdded -= OnNodeAdded;
        topology.OnNodeRemoved -= OnNodeRemoved;
    }
}
```

## Paleta de Colores

```csharp
Colors.backgroundBase   // #0D1117
Colors.surfacePanel     // #161B22
Colors.surfaceElevated  // #21262D
Colors.borderAccent     // #58A6FF
Colors.textPrimary      // #E6EDF3
Colors.textSecondary    // #8B949E
Colors.textAccent       // #58A6FF
Colors.buttonNormal     // #334D66
Colors.buttonHover      // #4A6B8C
Colors.buttonPressed    // #264066
```

## CheckList Antes de Commit

- [ ] ¿References cacheadas en Start()?
- [ ] ¿CleanupNullReferences() llamado?
- [ ] ¿Unsubscribe en OnDestroy?
- [ ] ¿Lambdas usan captured variables?
- [ ] ¿Paneles existentes se limpian?
- [ ] ¿Nombres únicos en textos?
- [ ] ¿Verificado que no hay FindObjectOfType en Update?
