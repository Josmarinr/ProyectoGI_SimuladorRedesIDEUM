# Skill: Integración IDEUM y TangibleEngine

Flujo: `Mesa IDEUM → TE Service → TangibleEngine → TangibleBridge → TangibleDiscManager → DiscEventHandler → TopologyManager → NodeVisualizer`

## TangibleBridge (mapeo tangibleId → uniqueId)

```csharp
public class TangibleBridge : MonoBehaviour
{
    [Header("Mapping - PatternId to DiscType")]
    [SerializeField] private int routerPatternId = 1;
    [SerializeField] private int switchPatternId = 2;
    [SerializeField] private int pcPatternId = 3;
    [SerializeField] private int enlacePatternId = 4;
    [SerializeField] private int falloPatternId = 5;
    [SerializeField] private int protocoloPatternId = 6;

    [Header("Canvas Reference Resolution (de SceneSetup)")]
    [SerializeField] private float canvasWidth = 4096f;
    [SerializeField] private float canvasHeight = 2160f;

    // Mapea tangible.Id (del servicio TE) → uniqueId (generado por TangibleDiscManager)
    private Dictionary<int, int> tangibleIdToUniqueId = new Dictionary<int, int>();

    void Start() {
        TE.TangibleEngine.OnTangibleAdded += HandleTangibleAdded;
        TE.TangibleEngine.OnTangibleRemoved += HandleTangibleRemoved;
        TE.TangibleEngine.OnTangibleUpdated += HandleTangibleUpdated;
    }

    void HandleTangibleAdded(TE.Tangible tangible) {
        if (tangibleIdToUniqueId.ContainsKey(tangible.Id)) {
            HandleTangibleUpdated(tangible); // reconexión, no duplicar
            return;
        }
        int discType = MapPatternToDiscType(tangible.PatternId);
        Vector2 pos = ConvertToCanvasPosition(new Vector2(tangible.X, tangible.Y));
        int uniqueId = FindObjectOfType<TangibleDiscManager>().SimulateDiscPlaced(discType, pos);
        tangibleIdToUniqueId[tangible.Id] = uniqueId;
    }

    void HandleTangibleUpdated(TE.Tangible tangible) {
        Vector2 pos = ConvertToCanvasPosition(new Vector2(tangible.X, tangible.Y));
        var manager = FindObjectOfType<TangibleDiscManager>();
        if (tangibleIdToUniqueId.TryGetValue(tangible.Id, out int uid)) {
            var existing = manager.GetDiscPosition(uid);
            if (existing.HasValue && Vector2.Distance(existing.Value, pos) > 5f)
                manager.UpdateDiscPosition(uid, pos);
        }
    }

    void HandleTangibleRemoved(TE.Tangible tangible) {
        if (tangibleIdToUniqueId.TryGetValue(tangible.Id, out int uid)) {
            FindObjectOfType<TangibleDiscManager>().SimulateDiscRemoved(uid);
            tangibleIdToUniqueId.Remove(tangible.Id);
        }
    }

    Vector2 ConvertToCanvasPosition(Vector2 screenPos) {
        float dw = Display.main.systemWidth;
        float dh = Display.main.systemHeight;
        if (dw <= 0 || dh <= 0) { dw = Screen.currentResolution.width; dh = Screen.currentResolution.height; }
        if (dw <= 0) dw = 1920;
        if (dh <= 0) dh = 1080;
        return new Vector2(
            screenPos.x * (canvasWidth / dw),
            screenPos.y * (canvasHeight / dh)
        );
    }
}
```

## TangibleDiscManager (estado central)

```csharp
public static TangibleDiscManager Instance { get; private set; }
public event Action<int, Vector2> OnDiscPlaced, OnDiscMoved;
public event Action<int> OnDiscRemoved;

Dictionary<int, Vector2> activeDiscs = new();
Dictionary<int, int> discTypeToDiscId = new();
int instanceCounter = 1;
const int BASE_DISC_ID = 100;

public int SimulateDiscPlaced(int discType, Vector2 pos) {
    int id = BASE_DISC_ID + instanceCounter++;
    activeDiscs[id] = pos;
    discTypeToDiscId[id] = discType;
    OnDiscPlaced?.Invoke(id, pos);
    return id; // retorna uniqueId para que TangibleBridge lo mapee
}

public void UpdateDiscPosition(int uniqueId, Vector2 pos) {
    if (activeDiscs.ContainsKey(uniqueId)) {
        activeDiscs[uniqueId] = pos;
        OnDiscMoved?.Invoke(uniqueId, pos);
    }
}
```

## DebugDiscSimulator (testing con teclado)

- `enableSimulation = false` por defecto (producción)
- Para testing en PC: `enableSimulation = true`
- Teclas: **1**=Router, **2**=Switch, **3**=PC, **4**=Enlace, **5**=Fallo, **C**=Limpiar, **P**=Ping

```csharp
void Update() {
    if (!enableSimulation) return;
    if (Input.GetKeyDown(KeyCode.Alpha1)) SimulateDiscAt(1, GetNextPosition());
}
void SimulateDiscAt(int discType, Vector2 pos) {
    FindObjectOfType<TangibleDiscManager>()?.SimulateDiscPlaced(discType, pos);
}
```

## DiscEventHandler (conexión automática)

```csharp
[SerializeField] bool autoConnectLinks = true;
[SerializeField] float linkDistanceThreshold = 300f;

void HandleDiscPlaced(int discId, Vector2 pos) {
    // crea nodo en TopologyManager
    if (autoConnectLinks) CheckAndCreateLinks(discId, pos);
}

void CheckAndCreateLinks(int discId, Vector2 pos) {
    foreach (var kvp in activeDiscs) {
        if (kvp.Key == discId) continue;
        if (Vector2.Distance(pos, kvp.Value) <= linkDistanceThreshold)
            topology.AddLink(discId, kvp.Key);
    }
}
```

## Build & TangibleEngine Runtime

- En **editor**: TangibleEngine usa `EditorProperties` → Modo Simulator (software)
- En **build**: TangibleEngine usa `RuntimeProperties` → Modo Service (TCP localhost:4949)
- Si el servicio no existe: falla silenciosamente (no bloquea la app)
- Para probar en PC normal sin IDEUM: `DebugDiscSimulator.enableSimulation = true`
