# API — Capa Tangible

> **Namespaces**: `SimRedes.Tangible` · **Archivos**: `Assets/Scripts/Tangible/`

---

## TangibleDiscManager

> **Archivo**: `Assets/Scripts/Tangible/TangibleDiscManager.cs`

Singleton que gestiona el estado de los discos activos en la mesa IDEUM.

Incluye validación de discType (rango 1-18) y detección de suscriptores ausentes en eventos OnDiscPlaced/OnDiscRemoved.

### Eventos

| Evento | Firma | Descripción |
|--------|-------|-------------|
| `OnDiscPlaced` | `Action<int, Vector2>` | Disco colocado (uniqueId, posición) |
| `OnDiscMoved` | `Action<int, Vector2>` | Disco movido (uniqueId, posición) |
| `OnDiscRemoved` | `Action<int>` | Disco retirado (uniqueId) |

### Métodos

```csharp
public int SimulateDiscPlaced(DiscType discType, Vector2 position)
```
Simula la colocación de un disco. Genera un `uniqueId` auto-incremental (BASE_DISC_ID + counter).
- **Retorna**: `uniqueId` del disco creado

```csharp
public void UpdateDiscPosition(int uniqueId, Vector2 newPosition)
```
Actualiza la posición de un disco existente.

```csharp
public void SimulateDiscRemoved(int uniqueId)
```
Elimina un disco. Dispara `OnDiscRemoved`.

```csharp
public Dictionary<int, Vector2> GetActiveDiscs()
```
Retorna todos los discos activos con sus posiciones.

```csharp
public Vector2 GetDiscPosition(int uniqueId)
```
Retorna la posición de un disco específico.

```csharp
public DiscType GetDiscType(int uniqueId)
```
Retorna el tipo de un disco.

```csharp
public void ClearAllDiscs()
```
Limpia todos los discos activos.

---

## TangibleBridge

> **Archivo**: `Assets/Scripts/Tangible/TangibleBridge.cs`

Puente entre el SDK de TangibleEngine y el sistema interno. Mantiene un `Dictionary<int,int>` que mapea `tangible.Id` (del TE) → `uniqueId` (del manager).

**Solo reconoce discos físicos 1-3 (Router, Switch, PC).** Discos 4-18 se ignoran porque enlaces, fallos y configuración de routing se manejan desde las actividades académicas (actividades, no discos físicos).

Incluye logging mejorado con timestamp/nivel, try-catch en los 3 handlers de eventos, y una coroutine de chequeo periódico (cada 5s por 30s) que detecta si TE no está conectado al servicio IDEUM.

**Sistema de antirrebote**: Cuando TE reporta la ausencia de un disco, no se remueve inmediatamente. Se marca como "pendiente de remoción" y solo se remueve si permanece ausente >1s (procesado en Update()). Si reaparece antes, se cancela la remoción pendiente. Esto evita parpadeo cuando el touch frame deja de detectar el disco por frames sueltos.

### Métodos

```csharp
public void HandleTangibleAdded(Tangible tangible)
```
Recibe un evento del TE, mapea el patrón a `DiscType`, convierte coordenadas, llama a `SimulateDiscPlaced()` y guarda el mapping.

```csharp
public void HandleTangibleUpdated(Tangible tangible)
```
Actualiza la posición si el disco se movió >5px.

```csharp
public void HandleTangibleRemoved(Tangible tangible)
```
Remueve el disco usando el mapping `tangibleIdToUniqueId`.

```csharp
public DiscType MapPatternToDiscType(int patternId)
```
Mapea PatternId (1-6) a tipos de disco.

```csharp
public Vector2 ConvertToCanvasPosition(Vector2 screenPosition)
```
Convierte coordenadas TUIO (1920x1080) a coordenadas del canvas (4096x2160).
Usa constantes fijas `TUIO_WIDTH=1920`, `TUIO_HEIGHT=1080` en lugar de `Display.main.systemWidth/Height` (que con fullscreen devuelve 4096x2160 en vez de la resolución del touch frame).

---

## DiscEventHandler

> **Archivo**: `Assets/Scripts/Tangible/DiscEventHandler.cs`

Procesa los eventos de discos y decide si crean nodos (IDs 1-6) o configuran rutas (IDs 7-18).

### Métodos

```csharp
public void HandleDiscPlaced(int uniqueId, Vector2 position)
```
Para discos 1-6: crea nodo en TopologyManager. Para discos 7-18: llama `HandleRoutingConfigDisc()`.

```csharp
public void HandleRoutingConfigDisc(int discId, Vector2 position, DiscConfiguration config, TopologyManager topology)
```
Busca un router cerca de la posición y actualiza su `RouteBuilderState` según el tipo de disco de routing.

```csharp
public bool IsRoutingConfigDisc(DiscType discType)
```
Retorna `true` si el disco es de configuración de routing (IDs 7-18).

```csharp
public void TryAddRoute(NetworkNode router, RouteBuilderState builder, TopologyManager topology)
```
Si `builder.IsComplete` → llama `ApplyToRouter()` y resetea el builder.

---

## RouteBuilderState

> **Archivo**: `Assets/Scripts/Tangible/RouteBuilderState.cs`

Estado transitorio para construir rutas parciales con discos de routing.

### Propiedades

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `RouterDiscId` | `int` | ID del router al que pertenece |
| `DestinationNetwork` | `string` | Red destino (set por disco 7/12) |
| `SubnetMask` | `string` | Máscara (set por disco 13) |
| `NextHop` | `string` | Siguiente salto (set por disco 14) |
| `OutInterface` | `string` | Interfaz de salida (set por disco 9) |
| `Protocol` | `string` | Protocolo (set por disco 10) |
| `IsComplete` | `bool` | `true` si los 4 campos están seteados |

### Métodos

```csharp
public void ApplyToRouter(TopologyManager topology)
```
Ejecuta `router.RoutingTable.AddStaticRoute(dest, mask, nextHop, iface)` y asigna el protocolo.

```csharp
public void Reset()
```
Limpia campos de ruta pero preserva `Protocol` (para mantener RIP/OSPF/EIGRP entre ciclos).

---

## DebugDiscSimulator

> **Archivo**: `Assets/Scripts/Tangible/DebugDiscSimulator.cs`

Simula discos con teclado para testing sin hardware IDEUM.

### Propiedades

| Propiedad | Tipo | Default | Descripción |
|-----------|------|:-------:|-------------|
| `enableSimulation` | `bool` | `false` | Activar simulación por teclado |

### Teclas

| Tecla | Acción |
|:-----:|--------|
| 1 | Router |
| 2 | Switch |
| 3 | PC |
| 4 | Modo CONEXIÓN |
| 5 | Fallo |
| C | Limpiar |
| P | Ping |
| R | Eliminar seleccionado |

### Métodos

```csharp
public void SimulateDiscAt(DiscType discType, Vector2 position)
```
Simula colocación de disco en una posición.

```csharp
public void ClearAllDiscs()
```
Limpia discos y topología.

```csharp
public void ForceClearAll()
```
Limpieza forzada (ignora null checks).
