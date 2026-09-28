# API — Actividades Académicas

> **Namespace**: `SimRedes.Simulation` · **Archivos**: `Assets/Scripts/Simulation/`

---

## ActivityLoader

> **Archivo**: `Assets/Scripts/Simulation/ActivityLoader.cs` (114L)

Fachada central de actividades (split C5). La usa `SceneSetup`; conserva la superficie pública histórica y delega la implementación en `ActivityDispatcher.cs` (despacho, 299L), `ActivityHudFactory.cs` (HUD, 286L) y `ScenarioLoader.cs` (escenarios, 290L).

```csharp
public void SetSelectedProtocol(string protocol)
```
Almacena el protocolo de enrutamiento dinámico seleccionado ("RIP", "OSPF" o "EIGRP").

```csharp
public void SelectActivity(int index)
```
Delega en `ActivityDispatcher.SelectActivity(index)`. Despacha la actividad según el índice:
- `0` → `BuildTopologyActivity` — Detección de topología
- `1` → `FindFaultActivity` — Encontrar y reparar fallos
- `2` → `RoutingTablesActivity` — Visualizar tablas de enrutamiento
- `3` → `BestRouteActivity` — Seleccionar mejor ruta (quiz)
- `4` → `StaticRoutingActivity` — Configurar rutas estáticas
- `5` → `DynamicRoutingActivity` — Enrutamiento dinámico (RIP/OSPF/EIGRP)
- `6` → `PredefinedScenarios` — 5 escenarios preconfigurados

```csharp
public void StartSimulation(Transform canvasTransform)
```
Delega en `ActivityDispatcher.StartSimulation()`: crea el HUD de simulación y los componentes base (`BuildTopologyActivity`, `SimulationControls`, `ScoringSystem`).

```csharp
public void ShowConnectivityPanel(Transform canvasTransform)
```
Delega en `ActivityDispatcher.ShowConnectivityPanel()`: crea los gestores si no existen (su helper privado `EnsureManagersForConnectivity()`) y configura el `ConnectivityTestPanel`.

```csharp
// Implementación real: ScenarioLoader.cs
public void LoadScenario(int scenarioIndex)
```
Delegado en `ScenarioLoader.LoadScenario()`. Carga un escenario preconfigurado (0-4): gestores, HUD, sesión de puntuación, nodos, enlaces, IPs y fallos.

### Métodos de Enrutamiento Dinámico

El ciclo de UI del protocolo vive en `DynamicRoutingActivity.cs` — `SetProtocol()`, `StartProtocol(GameObject panel)`, `StopProtocol(GameObject panel)`, `ClearAllRoutes(GameObject panel)`, `ShowRoutes()` — y el despacho lo hace `ActivityDispatcher`. El motor (`DynamicRoutingProtocol.cs`) expone `StartProtocol()`, `StopProtocol()`, `GetProtocolStatus()`, `GetRouterRoutesSummary()`, etc.

> Los métodos `StartDynamicProtocol` / `StopDynamicProtocol` / `ClearDynamicRoutes` / `ShowAllRouterRoutes` ya no existen: fueron eliminados en el split C5 (fachada + dispatcher).

---

## BuildTopologyActivity

> **Archivo**: `Assets/Scripts/Simulation/BuildTopologyActivity.cs`

Detecta automáticamente el tipo de topología basado en grado de nodos y número de enlaces.

```csharp
public TopologyType GetCurrentTopology()
```
Retorna el tipo actual de topología: `Estrella`, `Bus`, `Anillo`, `Arbol`, `Malla`, `Ninguna`.

```csharp
public void UpdateUI()
```
Actualiza el panel con el tipo de topología detectado y las estadísticas.

```csharp
public bool IsFullyConnected()
```
Verifica si la topología es completa.

### Lógica de Detección

```
0 nodos            → Ninguna
1 nodo             → Estrella
Enlaces = n*(n-1)/2 → Malla
Enlaces = nodos    → Anillo
1 switch + routers → Estrella
2+ routers + sw    → Árbol
Default            → Bus
```

---

## FindFaultActivity

> **Archivo**: `Assets/Scripts/Simulation/FindFaultActivity.cs`

Genera fallos aleatorios que el estudiante debe diagnosticar y resolver.

```csharp
public void GenerateRandomFault()
```
Selecciona un fallo aleatorio entre 5 tipos y lo aplica a la topología.

```csharp
public void GenerateNewScenario()
```
Limpia fallos anteriores y genera uno nuevo.

```csharp
public void FixFault()
```
Repara el fallo actual según su tipo específico (sin limpiar toda la topología).

```csharp
public bool ValidateSolution()
```
Verifica que el fallo fue correctamente reparado.

```csharp
public void OnSolveClicked()
```
Callback del botón RESOLVER. Valida la solución y actualiza puntaje.

---

## RoutingTablesActivity

> **Archivo**: `Assets/Scripts/Simulation/RoutingTablesActivity.cs`

Muestra las tablas de enrutamiento de todos los routers.

```csharp
public void RefreshRoutingTables()
```
Actualiza la visualización de todas las tablas de enrutamiento.

```csharp
public string GetRouterTableSummary(NetworkNode router)
```
Retorna las rutas de un router formateadas para display.

```csharp
public void GenerateSampleRoutes(NetworkNode router)
```
Agrega rutas de ejemplo a un router para demostración.

---

## BestRouteActivity

> **Archivo**: `Assets/Scripts/Simulation/BestRouteActivity.cs`

Actividad tipo quiz con 4 escenarios de selección de mejor ruta.

```csharp
public void ShowScenario(int index)
```
Muestra un escenario con IP destino y rutas candidatas.

```csharp
public void OnRouteSelected(int optionIndex)
```
Valida la selección del estudiante contra `FindBestRoute()`.

```csharp
public void NextScenario()
```
Avanza al siguiente escenario o muestra resultados finales.

---

## StaticRoutingActivity

> **Archivo**: `Assets/Scripts/Simulation/StaticRoutingActivity.cs`

Configuración manual de rutas estáticas con dos modos: botón **AÑADIR RUTA** (demo automática) y botón **AÑADIR MANUAL** (inputs personalizados).

```csharp
public void AddRoute(string destNetwork, string mask, string nextHop, string outInterface = "G0/0")
```
Agrega ruta estática al router seleccionado. El parámetro `outInterface` es opcional (default `"G0/0"`).

```csharp
public void ShowAddRoutePanel()
```
Abre panel con 4 inputs (Red Destino, Máscara, Next Hop, Interfaz) para rutas personalizadas. Requiere al menos un router en la topología.

```csharp
public void AddSampleRoute()
```
Agrega una ruta de ejemplo (`10.X.0.0/8 → 192.168.X.254`) para demostración rápida.

```csharp
public void TestRouting()
```
Prueba conectividad hacia una IP aleatoria usando el router seleccionado.

---

## DynamicRoutingActivity

> **Archivo**: `Assets/Scripts/Simulation/DynamicRoutingActivity.cs`

Interfaz de usuario para el protocolo de enrutamiento dinámico.

```csharp
public void SetProtocol(DynamicRoutingProtocol.ProtocolType type)
```
Selecciona RIP, OSPF o EIGRP.

```csharp
public void SimulateAdvertisement()
```
Dispara un anuncio manual entre routers.

```csharp
public void SimulateConvergence()
```
Verifica si hay convergencia en la red.

---

## PredefinedScenarios

> **Archivo**: `Assets/Scripts/Simulation/PredefinedScenarios.cs`

5 escenarios preconfigurados con diferentes niveles de dificultad.

```csharp
public NetworkScenario GetScenario(int index)
```
- `0` → Estrella Simple (Básico) — 1 switch, 3 PCs
- `1` → Dos Routers (Intermedio) — 2 routers, 1 switch, 2 PCs
- `2` → Topología en Anillo (Intermedio) — 4 routers en anillo
- `3` → Red en Árbol (Avanzado) — Router raíz, switches, PCs
- `4` → Detectar Fallos (Intermedio) — Red con fallos preconfigurados

### Tipos Anidados

```csharp
public class NetworkScenario {
    public string Name;
    public string Description;
    public ScenarioDifficulty Difficulty;
    public List<DeviceConfig> Devices;
    public List<LinkConfig> Links;
}

public class DeviceConfig {
    public DeviceType Type;
    public Vector2 Position;
    public string IPAddress;
    public string SubnetMask;
}

public class LinkConfig {
    public int SourceIndex;
    public int DestIndex;
}

public enum ScenarioDifficulty { Basico, Intermedio, Avanzado }
```
