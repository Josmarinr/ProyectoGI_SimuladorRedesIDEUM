# Skill: Buenas Prácticas y Arquitectura

## Arquitectura del Proyecto

### Estructura de Carpetas

```
Assets/Scripts/
├── Network/                    # Modelos de red
│   ├── NetworkNode.cs
│   ├── NetworkLink.cs
│   ├── TopologyManager.cs     # Gestor central + eventos
│   ├── DiscConfiguration.cs
│   └── RoutingTable.cs
│
├── Tangible/                   # Integración mesa IDEUM
│   ├── TangibleBridge.cs
│   ├── TangibleDiscManager.cs  # Estado central discos
│   ├── DiscEventHandler.cs
│   └── DebugDiscSimulator.cs
│
├── Simulation/                 # Lógica de simulación
│   ├── SceneSetup.cs          # Creación dinámica de UI
│   ├── SimulationControls.cs
│   ├── PointerClickHandler.cs  # Manejo de clicks táctiles
│   └── Activities/
│
├── UI/                         # Componentes de UI
│   ├── UIComponents.cs         # Factory de elementos
│   ├── NodeVisualizer.cs
│   └── ConnectivityTestPanel.cs
│
└── Debug/                      # Herramientas debug
```

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

### 4. Limpieza de Nodos Nulos

```csharp
private void CleanupNullReferences()
{
    var keysToRemove = new List<int>();
    foreach (var kvp in nodeObjects)
    {
        if (kvp.Value == null)
            keysToRemove.Add(kvp.Key);
    }
    foreach (var key in keysToRemove)
    {
        nodeObjects.Remove(key);
    }
}
```

## Buenas Prácticas UI

### 1. Alias para Namespaces

```csharp
using UIComp = SimRedes.UI.UIComponents;
using UIColors = SimRedes.UI.UIComponents.Colors;
```

### 2. Factory para Elementos UI

```csharp
public static class UIComponents
{
    public static GameObject CreateRoundedPanel(Transform parent, Vector2 size,
        int cornerRadius, Color backgroundColor, Color borderColor)
    {
        GameObject panel = new GameObject("RoundedPanel");
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;

        // ... resto
        return panel;
    }
}
```

### 3. Limpiar Paneles Antes de Crear

```csharp
private void CreateMyPanel(Transform parent)
{
    var existing = GameObject.Find("MyPanel");
    if (existing != null) Destroy(existing);

    var panel = UIComp.CreateRoundedPanel(parent, ...);
}
```

### 4. Crear Textos con Nombres Únicos

```csharp
var topologyTypeText = CreateInfoText(panel, "Topologia: Sin topologia", ...);
topologyTypeText.gameObject.name = "TopologyTypeText";
```

**Encontrar después:**
```csharp
var text = GameObject.Find("TopologyTypeText").GetComponent<Text>();
```

### 5. Posicionamiento de Paneles

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
    // ...
}
```

### 2. Lambdas Capturando Variables

**MAL:**
```csharp
AddClickEvent(trigger, () => OnNodeClicked(node.DiscId));
// node puede ser null cuando se ejecuta
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

### 4. Verificar Referencias Antes de Usar

```csharp
var topology = FindObjectOfType<TopologyManager>();
if (topology == null) return; // Siempre verificar
```

### 5. FindObjectOfType en Update (Performance)

**MAL:**
```csharp
void Update()
{
    var topology = FindObjectOfType<TopologyManager>(); // Costoso
}
```

**BIEN:**
```csharp
private TopologyManager topology;

void Start()
{
    topology = FindObjectOfType<TopologyManager>();
}

void Update()
{
    // Usar topology directamente
}
```

## Patrones de Flujo de Datos

### Mesa IDEUM → Visualización

```
Mesa IDEUM (Discos Físicos)
    ↓ TangibleEngine SDK
TangibleBridge.cs → PatternId → DiscType
    ↓
TangibleDiscManager.cs (OnDiscPlaced event)
    ↓
DiscEventHandler.cs → TopologyManager (AddNode)
    ↓
TopologyManager (OnNodeAdded event)
    ↓
NodeVisualizer (CreateNodeVisual)
```

### Click en Nodo → Panel

```
OnNodeClicked(discId)
    ↓
CleanupNullReferences()
    ↓
¿Modo CONECTAR activo?
    → Sí: CloseIPConfigPanel → HandleNodeClick
    → No: ¿Panel IP abierto?
        → Sí: ¿Otro nodo?
            → Sí: Cerrar, abrir nuevo
        → No: ShowIPConfigPanel
```

### Panel IP con Fondo Oscuro

```
ShowIPConfigPanel():
    1. Crear fondo oscuro (semi-transparente)
    2. Click en fondo → CloseIPConfigPanelPublic
    3. Crear panel principal
    4. Botones APLICAR/CANCELAR → CloseIPConfigPanelPublic
```

## Consejos de Performance

### 1. Cachear Referencias

```csharp
private TopologyManager topology;
private NodeVisualizer visualizer;
private SceneSetup sceneSetup;

void Start()
{
    topology = FindObjectOfType<TopologyManager>();
    visualizer = FindObjectOfType<NodeVisualizer>();
    sceneSetup = FindObjectOfType<SceneSetup>();
}
```

### 2. Evitar FindObjectOfType en Update

```csharp
void Update()
{
    // NO: FindObjectOfType en cada frame
    // SÍ: Cachear en Start/Awake
}
```

### 3. Limpiar Contenedores Antes de Redibujar

```csharp
private void DrawLinks()
{
    foreach (Transform child in linkContainer)
    {
        if (child != null) Destroy(child.gameObject);
    }
    // ... crear nuevos
}
```

## Estructura de NetworkNode

```csharp
public class NetworkNode
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DeviceType Type { get; set; }
    public string IpAddress { get; set; }
    public string SubnetMask { get; set; }
    public Vector2 Position { get; set; }
    public int DiscId { get; set; }
    public bool IsActive { get; set; }
    public List<string> Interfaces { get; set; }
    public Dictionary<string, string> InterfaceIPs { get; set; }
}
```

## Paleta de Colores (Usar Desde UIColors)

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

## Teclado Numérico para IP

```csharp
private InputField activeInputField;

private void OnKeyPressed(string key)
{
    if (activeInputField == null) return;
    string currentText = activeInputField.text;

    if (key == "DEL")
    {
        if (currentText.Length > 0)
            activeInputField.text = currentText.Substring(0, currentText.Length - 1);
    }
    else if (key == ".")
    {
        char lastChar = currentText.Length > 0 ? currentText[currentText.Length - 1] : ' ';
        if (lastChar != '.' && lastChar != ' ')
            activeInputField.text = currentText + key;
    }
    else
    {
        if (currentText.Length < 18)
            activeInputField.text = currentText + key;
    }
}
```

## CheckList Antes de Commit

- [ ] ¿References cacheadas en Start()?
- [ ] ¿CleanupNullReferences() llamado?
- [ ] ¿Unsubscribe en OnDestroy?
- [ ] ¿Lambdas usan captured variables?
- [ ] ¿Paneles existentes se limpian?
- [ ] ¿Nombres únicos en textos?
- [ ] ¿Verificado que no hay FindObjectOfType en Update?