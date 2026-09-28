# SPEC - Simulador de Redes Académicas IDEUM

## 1. Información General

**Proyecto**: Simulador de redes académicas para mesas táctiles IDEUM 55"
**Engine**: Unity 6000.4.5f1 (Unity 6) con TangibleEngine
**Usuario**: Sebastián Marín
**Idioma**: Español

## 2. Funcionalidad Principal

El sistema permite a los estudiantes aprender networking colocando discos físicos (PUCs) que representan dispositivos de red y configuraciones de enrutamiento.

### Dispositivos (PUCs) — 18 tipos de disco

| ID | Tecla | Tipo | Color UI | Descripción |
|----|-------|------|----------|-------------|
| 1 | 1 | Router | Azul | Dispositivo de enrutamiento |
| 2 | 2 | Switch | Cyan | Dispositivo de conmutación |
| 3 | 3 | PC | Verde | Host final |
| 4 | 4 | Enlace | Amarillo | Conexión (físico) |
| 5 | 5 | Fallo | Rojo | Simulación de errores |
| 6 | 6 | Protocolo | Magenta | Protocolo de enrutamiento |
| 7 | - | Red Destino | Naranja | Red de destino para ruta |
| 8 | - | Métrica | Dorado | Valor de métrica |
| 9 | - | Interfaz Salida | Celeste | Interfaz de salida |
| 10 | - | Modo Enrutamiento | Rosa | Modo de enrutamiento |
| 11 | - | IP Route | Verde claro | Comando ip route |
| 12 | - | Destino | Púrpura | IP de destino |
| 13 | - | Máscara | Azul claro | Máscara de subred |
| 14 | - | Próximo Salto | Verde azulado | Siguiente salto |
| 15 | - | Vecino | Naranja claro | Router vecino |
| 16 | - | Anunciar Red | Rojo claro | Red a anunciar |
| 17 | - | Costo | Verde lima | Costo del enlace |
| 18 | - | Ancho Banda | Azul cielo | Ancho de banda |

Los discos 7-18 no crean nodos en la topología. Al colocarlos sobre un router, configuran su tabla de enrutamiento real (`router.RoutingTable`). Las rutas configuradas son visibles en las actividades StaticRoutingActivity, RoutingTablesActivity y DynamicRoutingActivity.

## 3. Features Completados

### Sistema de Menú
- Menú principal con 5 opciones: INICIAR SIMULACIÓN, ACTIVIDADES, PRUEBAS Y CONEXIONES, CÓMO USAR, SALIR
- Navegación por teclado (flechas, W/S, números 1-5)
- Click de mouse funciona en todos los paneles
- ESC para volver atrás
- Panel "Cómo Usar" en 2 columnas

### Panel Dispositivos (izquierda arriba)
- Lista de dispositivos con colores por tipo
- Selección visual: item se pone ROJO cuando está seleccionado para eliminar
- Click → abre panel de IP o maneja enlaces
- Click de nuevo → elimina el dispositivo seleccionado
- Sincronizado con eventos de TopologyManager

### Panel TopologyInfoPanel (derecha)
- Topología: [tipo]
- Enlaces: [número]
- Dispositivos: Routers, Switches, PCs
- Botones: LIMPIAR | PING | VOLVER | CONECTAR | DESCONECTAR

### Configuración de IP (Panel Compacto 480x650)
- Panel táctil con teclado numérico (0-9, ., DEL) compacto
- Campos separados: IP y Máscara
- Validación en tiempo real
- Info de red: muestra dirección de red para routers
- Botones: APLICAR | CANCELAR | TABLA ARP | TABLA RUTAS
- TABLA ARP y TABLA RUTAS solo aparecen en Routers

### 7 Actividades Académicas
1. Construir Topología
2. Encuentra el Fallo

3. Tabla de Enrutamiento Tangible

4. **Simulación de Mejor Ruta** (selección de mejor ruta por longest prefix match + métrica)
5. Enrutamiento Estático
6. Enrutamiento Dinámico (RIP/OSPF)
7. Escenarios Preconfigurados

### Enrutamiento Dinámico
- RIP: Conteo de hops (máx 15)
- OSPF: Costo por enlace
- Advertisement automático cada 3 segundos
- Convergencia automática

### Sistema de Puntajes
- Puntos base por tareas completadas (100 pts)
- Bono de tiempo (< 2 min = +50 pts)
- Penalizaciones por fallos (-20 pts)
- Rutas configuradas (+15 pts)
- Ping exitoso (+10 pts)
- Grade final (1-5)

### Escenarios Preconfigurados
- Estrellas Simple (Básico)
- Dos Routers (Intermedio)
- Topología en Anillo (Intermedio)
- Red en Árbol (Avanzado)
- Detectar Fallos (Intermedio - con fallos preconfigurados)
- Fallos: badip, badmask, noip

### Tablas de Red (Routers Only)
- Tabla ARP: IP, MAC Address, Interfaz
- Tabla de Enrutamiento: Editable (añadir/eliminar rutas)
- Validación de IPs

### Ping Visual Animado
- Paquete amarillo que sigue la ruta real
- Paquete se vuelve verde (éxito) o rojo (fallo)

### Conexiones Manuales
- CONECTAR: click en dos nodos para crear enlace
- DESCONECTAR: click en dos nodos del enlace para eliminar

### VLANs (Redes Virtuales)
- Crear/eliminar VLANs (ID 1-4094)
- Asignar nodos a VLANs
- Aislamiento entre VLANs
- Comunicación solo dentro de misma VLAN
- Panel de configuración UI con botones CREAR VLAN/ACL/NAT
- **Lógica aplicada**: ValidateVLAN() en CheckConnectivity

### ACLs (Access Control Lists)
- ACLs estándar y extendidas
- Reglas: Permit/Deny
- Filtrado por IP origen/destino
- Filtrado por protocolo (TCP/UDP/ICMP)
- Filtrado por puerto
- Validación de paquetes contra reglas
- Panel UI para crear ACLs y agregar reglas
- **Lógica aplicada**: ValidateACL() en CheckConnectivity

### NAT (Network Address Translation)
- NAT estática: IP interna → IP externa fija
- NAT dinámica: IP interna → IP del pool
- PAT (Port Address Translation):many-to-one con puertos
- Tabla NAT visible
- Public IP configurable
- Panel UI para configurar NAT
- **Lógica aplicada**: TranslateSourceIP()/TranslateDestIP() disponibles

### UI Mejorada
- Botones VLAN/ACL/NAT en TopologyInfoPanel
- Paneles de configuración para cada功能
- Click fuera del panel cierra la ventana
- Selección de dispositivo se limpia al hacer click fuera

### TangibleBridge (mapeo tangibleId → uniqueId)

- `TangibleBridge` mantiene `Dictionary<int,int> tangibleIdToUniqueId` que mapea `tangible.Id` (instancia del TE) → `uniqueId` (101, 102...)
- `HandleTangibleAdded`: convierte coordenadas TE → canvas vía `ConvertToCanvasPosition()`, llama a `SimulateDiscPlaced()`, guarda mapping
- `HandleTangibleUpdated`: busca por `tangible.Id`, actualiza si se movió >5px  
- `HandleTangibleRemoved`: busca por `tangible.Id`, remueve correctamente
- Reconexión del TE: si `OnTangibleAdded` se dispara para un `tangible.Id` ya conocido, se redirige a update en vez de duplicar

### Coordenadas TE → Canvas

- TE reporta en píxeles de pantalla física (1920x1080 en IDEUM)
- Canvas/cámara configurados en 4096x2160 (SceneSetup)
- `ConvertToCanvasPosition()` escala coordenadas usando `Display.main.systemWidth/Height`
- Ej: toque al centro (960,540) → canvas (2048,1080)

## 4. Controles de Debug

| Tecla | Acción |
|-------|--------|
| 1 | Añadir Router |
| 2 | Añadir Switch |
| 3 | Añadir PC |
| 4 | Activar modo CONEXIÓN |
| 5 | Añadir Fallo |
| C | Limpiar simulación |
| P | Test ping |
| R | Eliminar dispositivo seleccionado |
| ESC | Volver al menú / Cerrar panel IP |

### DebugDiscSimulator
- `enableSimulation = false` por defecto
- Para testing con teclado en PC: setear `enableSimulation = true`
- En producción (mesa IDEUM real): mantener `enableSimulation = false`
- Posiciones de spawn: X: 100-300, Y: 250-700
- Tecla 4 activa `ToggleLinkMode("connect")` en SceneSetup (en vez de crear nodo Unknown)

## 5. Paleta de Colores

```csharp
Colors.backgroundBase   // #0D1117 - Fondo base
Colors.surfacePanel     // #161B22 - Paneles elevados
Colors.surfaceElevated  // #21262D - Elementos elevados
Colors.borderAccent     // #58A6FF - Bordes azul suave
Colors.textPrimary      // #E6EDF3 - Texto off-white
Colors.textSecondary    // #8B949E - Texto gris suave
Colors.buttonNormal     // #334D66 - Botón azul oscuro
Colors.buttonHover       // #4A6B8C - Botón hover
Colors.activeLinkMode   // Verde cuando CONNECTAR/DESCONECTAR está activo
```

## 6. Archivos Principales

| Archivo | Descripción |
|---------|-------------|
| `SceneSetup.cs` | Orquestador de escena (~379L, refactorizado) |
| `ActivityLoader.cs` | Carga actividades + escenarios (~800L, NUEVO) |
| `SceneCleanupService.cs` | Limpieza y GoBackToMainMenu (singleton, NUEVO) |
| `LinkModeController.cs` | Modo CONEXIÓN/DESCONEXIÓN (NUEVO) |
| `PingModeController.cs` | Modo ping + selector de nodos (NUEVO) |
| `IPConfigController.cs` | Panel IP y configuración de nodos (NUEVO) |
| `DevicePanelController.cs` | Panel dispositivos + score (NUEVO) |
| `NodeInteractionController.cs` | Click en nodos — fachada (NUEVO) |
| `UIPanelFactory.cs` | Fábrica de paneles UI |
| `UIComponents.cs` | Helpers visuales (CreateCircleIcon, GetColor) |
| `TopologyManager.cs` | Nodos, enlaces, conectividad, FindPath |
| `NodeVisualizer.cs` | Visualización de nodos en pantalla |
| `IPValidation.cs` | Validación de IP, máscara, subred |
| `PingVisualizer.cs` | Animación de ping por ruta real |
| `RoutingTable.cs` | Gestión de rutas estáticas |
| `ARPTable.cs` | Gestión de entradas ARP |
| `NetworkNode.cs` | Modelo de nodo |
| `NetworkLink.cs` | Modelo de enlace |
| `DynamicRoutingProtocol.cs` | Simulación RIP/OSPF |
| `ScoringSystem.cs` | Sistema de puntajes |
| `PredefinedScenarios.cs` | 5 escenarios preconfigurados |
| `SimulationControls.cs` | Controles de teclado |
| `Simulation/DebugDiscSimulator.cs` | Simulación de discos en editor |
| `VLANManager.cs` | Gestión de VLANs (crear, asignar, eliminar) |
| `ACLManager.cs` | Gestión de ACLs (reglas permit/deny) |
| `NATManager.cs` | Gestión de NAT (estática, dinámica, PAT) |

## 7. Build y Despliegue

### Escenas
- **Escena principal**: `Assets/Main.unity` — contiene `SceneSetup` adjunto, se ejecuta `Awake()` → `SetupScene()`
- `Assets/Scenes/GetStarted_Scene.unity` — NO tiene `SceneSetup`, solo es escena de respaldo. **No usar como escena principal en build**.

### Build Settings
El `EditorBuildSettings.asset` debe tener `Assets/Main.unity` como primera escena (índice 0). No incluir `GetStarted_Scene.unity` como escena activa.

### Build para mesa IDEUM (producción)
1. File > Build Settings > Windows x86_64
2. Asegurar que TangibleEngine Windows service corra en la máquina destino (TCP puerto 4949)
3. `DebugDiscSimulator.enableSimulation = false`
4. Build y copiar carpeta a la mesa IDEUM

### Build para PC normal (testing con teclado)
1. `DebugDiscSimulator.enableSimulation = true`
2. File > Build Settings > Windows x86_64
3. El TangibleEngine falla silenciosamente al no encontrar el servicio — no afecta la app
4. Usar teclas 1/2/3 para agregar dispositivos

## 8. Problemas Conocidos

### FIXED (Corregidos en Mayo 2026)

| Bug | Síntoma | Fix |
|-----|---------|-----|
| ShowConnectivityPanel sin SetupManagers | Al entrar a PRUEBAS Y CONEXIONES, no se podían colocar dispositivos. Teclas 1/2/3 no respondían. | Agregar `SetupManagers()` + `CreateVisualizer()` + `SubscribeToTopologyEvents()` al inicio de `ShowConnectivityPanel()` |
| FindFaultActivity limpiaba toda la topología | Al resolver un fallo, se llamaba `ClearTopology()` eliminando todos los nodos. `GenerateNewFault()` ya no podía aplicar fallos nuevos → actividad se estancaba. | Reemplazar `ClearTopology()` por `FixFault()` que repara el fallo específico (cable, interfaz, IP) sin borrar la topología |
| CheckConnectivity no manejaba Switches | Ping entre PC → Switch o Switch → PC siempre fallaba porque el Switch no tiene IP y el código requería IPs válidas en ambos extremos. | Agregar caso temprano para Switches: son L2 transparentes. Solo requieren ruta física; IP solo exigida al extremo no-Switch |
| LoadScenario sin SetupManagers | Escenarios no tenían SimulationControls (ESC/P/R), PingVisualizer ni ScoringSystem. ESC no volvía al menú. | Agregar `SetupManagers()` + `SubscribeToTopologyEvents()` + `SimulationControls` + `ScoringSystem` en `LoadScenario()` |

### HISTÓRICOS (Corregidos anteriormente)

- Pantalla negra en build: `Main.unity` debe ser escena principal (no GetStarted_Scene.unity)
- Duplicación de nodos en IDEUM: mapeo `tangibleIdToUniqueId` en TangibleBridge
- Nodos en esquina: `ConvertToCanvasPosition()` escala coordenadas TE → canvas
- Borrosidad UI: CanvasScaler modo Expand
- UI pequeña: SetupCanvas() re-configura scaler
- InputField.font: usar `textComponent.font`

### FIXED (Batch 1 - Corregidos anteriormente)

| Bug | Síntoma | Fix |
|-----|---------|-----|
| RoutingTablesActivity/StaticRoutingActivity UI estática | Paneles de actividades 3 y 4 mostraban texto hardcodeado en vez de datos de RoutingSimulator | Conectar referencias `tableText`/`routesText`/`infoText`/`feedbackText` desde SceneSetup a los componentes de actividad |
| Default Gateway Faltante no-op | Al aplicar el fallo, se asignaba IP válida (no rompía nada). `ValidateSolution()` retornaba siempre true | `ApplyFault()` ahora limpia IP del PC. `ValidateSolution()` verifica que exista PC con IP+máscara válida. `FixFault()` restaura 192.168.1.10/24 |
| Sin PingVisualizer en ConnectivityTestPanel | El panel de pruebas solo mostraba texto sin animación visual | `AnimatePing()` ahora delega a `PingVisualizer.AnimatePing()` si existe, con fallback a texto |

### FIXED (Batch 2 - Mayo 2026)

| Bug | Síntoma | Fix |
|-----|---------|-----|
| Doble P key | ConnectivityTestPanel y SimulationControls capturaban P → doble ping al presionar una vez | Eliminado `Input.GetKeyDown(KeyCode.P)` de ConnectivityTestPanel.Update() |
| Doble ESC | SceneSetup destruía ScenarioInfoPanel, SimulationControls volvía al menú → conflicto al presionar ESC | Removido handler de SceneSetup.Update(). ESC unificado en SimulationControls: cierra info panel → IP config → menú |
| Routing faltante escenarios 2-4 | Pings entre subredes diferentes fallaban porque no había rutas estáticas configuradas | Rutas estáticas auto-configuradas en `BuildScenarioTopology()` para Dos Routers, Anillo, Árbol |
| Tecla 4 creaba nodo Unknown | KeyCode.Alpha4 creaba un nodo "Enlace" que se mapeaba a DeviceType.Unknown | Cambiado a `LinkModeController.ToggleLinkMode("connect")` en DebugDiscSimulator |
| Actividad 0 sin panel | Construir Topología no tenía UI instructiva propia | Nuevo `BuildTopologyInfoPanel` con instrucciones y tipos de topología |
| BestRoute activity faltante | ActivityMode.BestRoute existía en enum pero no había clase ni panel | Nueva `BestRouteActivity.cs` con 4 escenarios, panel, puntaje |
| 12 discos routing sin registro | DiscType.RedDestino..BW nunca agregados a DefaultConfiguration | Agregados IDs 7-18 con nombres, colores y descripciones |
| Routing config discs creaban nodos | Discos 7-18 pasaban por AddNode como Unknown | `IsRoutingConfigDisc()` en DiscEventHandler evita creación de nodos |

### COMPLETADO RECIENTEMENTE

| Feature | Descripción |
|---------|-------------|
| **Integración discos 7-18 → actividades** | StaticRoutingActivity, RoutingTablesActivity, DynamicRoutingActivity ahora escriben/leen del router.RoutingTable real. Discos de routing configuran routers y las rutas aparecen en la UI de actividades. |
| **Fix disco Destino (ID 12)** | Case agregado en HandleRoutingConfigDisc() — antes caía al default silencioso. |
| **ShowConnectivityPanel unificado** | ActivityLoader delega en UIPanelFactory.CreateConnectivityPanel() con struct ConnectivityPanelRefs. ~125 líneas de código duplicado eliminadas. |
| **CreateStatusPanel deprecado** | Código muerto marcado [Obsolete]. Sin callers. |
| **Tests unitarios (53 EditMode)** | IPValidation (18), RoutingTable (20), RouteBuilderState (15). En Assets/Editor/Tests/. |
| **Input System Both** | activeInputHandler cambiado de 0 (Old) a 2 (Both). El Input System Package 1.19.0 ahora está activo junto con el código legacy. |
| **Dead code eliminado** | ActivityPanel.cs removido (sin callers). GameManager.cs limpiado (RoutingSimulator muerto removido). |

### CONOCIDOS ACTUALES

1. **Errores de compilación**: Cerrar Unity, eliminar carpeta Library, abrir de nuevo
2. **MissingReference en nodos destruidos**: CleanupNullReferences() + null checks
3. **Lambdas capturando variables null**: Usar `int capturedDiscId = node.DiscId`

## 11. Backlog

### Alta Prioridad ✓ (Completado)
- Núcleo (discos, topologías, enlaces)
- Refactor SceneSetup (~4250L → ~379L) con 5 controllers, ActivityLoader, SceneCleanupService, UIPanelFactory
- Red (Router, Switch, ARP, Routing, Ping)
- UI (paneles, estados visuales)
- IP Validation
- Ping Visual
- Tablas ARP/Routing editables
- Enrutamiento Dinámico (RIP/OSPF)
- Sistema de Puntajes con panel visible
- Escenarios Preconfigurados
- VLANs (Redes Virtuales)
- ACLs (Access Control Lists)
- NAT (Network Address Translation)

### Media Prioridad
- Exportar/Importar topologías

### Baja Prioridad
- Métricas de desempeño detalladas

## 9. Nueva Actividad: "Simulación de Mejor Ruta" (Mayo 2026)

Actividad académica #4 donde el estudiante debe seleccionar la mejor ruta hacia un destino entre varias opciones.

### Funcionamiento
- Se muestra una IP de destino y 3-4 rutas candidatas
- Cada ruta muestra: red destino, máscara, next hop, protocolo, métrica
- El estudiante hace clic en la ruta que considera mejor
- El sistema valida usando `RoutingTable.FindBestRoute()` (longest prefix match + lowest metric)
- Muestra explicación de por qué es la correcta
- Puntaje acumulado: correctas / totales

### Escenarios
1. **10.1.1.100**: Compite 10.0.0.0/8 (RIP, métrica 5) vs 10.1.0.0/16 (OSPF, 3) vs 10.1.1.0/24 (Static, 1) vs 0.0.0.0/0 (Static, 1) → gana /24
2. **172.16.5.50**: /16 (RIP, 3) vs /24 (OSPF, 2) vs 0.0.0.0/0 (Static, 1) → gana /24
3. **192.168.1.10**: /24 (Static, 1) vs /16 (RIP, 2) vs 0.0.0.0/0 (Static, 1) → gana /24 (mismo metric pero más específica)
4. **10.20.30.1**: /8 (RIP, 5) vs /16 (OSPF, 3) vs /24 (Static, 2) vs 0.0.0.0/0 (Static, 1) → gana /24

### Archivos
- `Assets/Scripts/Simulation/BestRouteActivity.cs`: Lógica de actividad y escenarios
- Panel creado en `UIPanelFactory.CreateBestRoutePanel()` (invocado desde ActivityLoader)

## 10. Actualizaciones Recientes

### Mayo 2026 — Refactor Completo de SceneSetup

SceneSetup.cs pasó de ~2600L a ~379L mediante la extracción de 8 nuevos archivos:

| Antes (SceneSetup) | Ahora | Archivo |
|-------------------|-------|---------|
| ~100L — Modo CONEXIÓN/DESCONEXIÓN | `LinkModeController.cs` | UI/ |
| ~200L — Modo ping + selector | `PingModeController.cs` | UI/ |
| ~200L — Panel IP (configuración) | `IPConfigController.cs` (usa UIPanelFactory) | UI/ |
| ~300L — Panel dispositivos + score | `DevicePanelController.cs` | UI/ |
| ~70L — Click en nodos (routing) | `NodeInteractionController.cs` (fachada) | UI/ |
| ~400L — Actividades + escenarios | `ActivityLoader.cs` | Simulation/ |
| ~120L — Limpieza + GoBackToMainMenu | `SceneCleanupService.cs` (singleton) | Simulation/ |
| Paneles UI (creación) | `UIPanelFactory.cs` unificado | UI/ |
| CreateCircleIcon + GetColor duplicados | `UIComponents.cs` (único) | UI/ |

#### Nuevos archivos creados
1. `Assets/Scripts/UI/LinkModeController.cs` — Maneja el estado CONECTAR/DESCONECTAR
2. `Assets/Scripts/UI/PingModeController.cs` — Maneja el modo ping con selector origen/destino
3. `Assets/Scripts/UI/IPConfigController.cs` — Panel de configuración IP y teclado numérico
4. `Assets/Scripts/UI/DevicePanelController.cs` — Panel de dispositivos + score
5. `Assets/Scripts/UI/NodeInteractionController.cs` — Fachada que orquesta clicks en nodos
6. `Assets/Scripts/Simulation/ActivityLoader.cs` — Carga actividades y escenarios (~800L)
7. `Assets/Scripts/Simulation/SceneCleanupService.cs` — Singleton para limpieza
8. `Assets/Scripts/UI/UIComponents.cs` — Helpers visuales compartidos

#### Archivos modificados
- `SceneSetup.cs` — Reducido de ~2600L a ~379L, ahora es orquestador que llama a los controllers
- `SimulationControls.cs` — Actualizado para trabajar con los nuevos controllers
- `UIPanelFactory.cs` — Unificado como fábrica central de paneles

#### Beneficios
- **Mantenibilidad**: Cada controller tiene una responsabilidad única (< 400L cada uno)
- **Testeabilidad**: Los controllers son independientes y pueden probarse por separado
- **Legibilidad**: SceneSetup ahora se lee en minutos, no en horas
- **Extensibilidad**: Agregar nuevo modo UI = nuevo controller, no tocar SceneSetup

### Mayo 2026 - Batch 2: Correcciones y Nueva Actividad

#### FIX: ShowConnectivityPanel sin managers
- `ShowConnectivityPanel()` no llamaba `SetupManagers()` → TopologyManager, DebugDiscSimulator, TangibleDiscManager, NodeVisualizer, PingVisualizer no existían en "PRUEBAS Y CONEXIONES"
- **Fix**: Agregar `SetupManagers()`, `CreateVisualizer()`, `SubscribeToTopologyEvents()` al inicio del método

#### FIX: FindFaultActivity limpiaba topología
- `OnSolveClicked()` llamaba `topologyManager.ClearTopology()` al resolver un fallo → borraba toda la red
- `GenerateNewFault()` fallaba porque `GetAllNodes()` retornaba 0
- **Fix**: Nuevo método `FixFault()` que repara el fallo específico según su tipo:
  - Cable Desconectado → `link.SetFault("")` en todos los enlaces
  - Interfaz Down → `node.IsAdminDown = false` en todos los nodos
  - IP/Máscara incorrecta → reset IP a `192.168.1.1/24` en routers
  - Gateway faltante → no-op (gateway auto-calculado)

#### FIX: CheckConnectivity para Switches
- Switch no tiene IP → código anterior requería IP válida en todos los casos
- **Fix**: Switch es L2 transparente. Nuevo caso en `CheckConnectivity()`:
  - Switch ↔ Switch: solo requiere ruta física
  - Switch ↔ PC: PC necesita IP/máscara válida
  - Switch ↔ Router: Router necesita IP válida

#### FIX: LoadScenario sin managers
- `LoadScenario()` no llamaba `SetupManagers()` → escenarios sin SimulationControls, PingVisualizer, ScoringSystem
- **Fix**: Llamar `SetupManagers()` + `SubscribeToTopologyEvents()` + crear `SimulationControls` + crear `ScoringSystem` si no existe

### Anteriores

#### Panel de Puntaje
- Panel visible en esquina inferior derecha
- Muestra puntuación actual en tiempo real
- Actualización cada 1 segundo

#### Menú de Escenarios
- Panel de 800x800px
- Items con recuadros diferenciables
- Hover effect en items
- Botón VOLVER funcionando
- Destroy de paneles al seleccionar escenario
- ESC cierra panel de info de escenario

#### Posiciones de Discos (Debug)
- Spawn en área centrada-izquierda de la pantalla
- X: 100-300, Y: 250-700
- Lejos del panel de topología

#### Canvas Scaler
- Modo Expand para mejor nitidez
- Sorting order alto para UI siempre visible

#### VLAN, ACL y NAT
- Botones en TopologyInfoPanel para cada funcionalidad
- Paneles de configuración con resumen
- VLANManager: crear VLANs (1-4094), asignar nodos, aislamiento
- ACLManager: reglas estándar/extendidas, filtrado por IP/puerto/protocolo
- NATManager: NAT estática, dinámica, PAT con puertos
- **Lógica aplicada**: CheckConnectivity() valida VLAN/ACL, NAT traduce IPs

#### DebugDiscSimulator
- enableSimulation = true por defecto (testing con teclado 1/2/3)
- Para producción en mesa IDEUM real: setear enableSimulation = false
- MemoryDiagnostics.cs en Core/ (diagnóstico de memoria en runtime, tecla F10)

#### FIX: Duplicación de nodos en IDEUM (TangibleBridge)
- TangibleBridge usaba `PatternId` (1-6) para buscar discos, pero TangibleDiscManager generaba uniqueId (101, 102...)
- HandleTangibleUpdated/Removed nunca encontraban discos → acumulación de nodos
- **Fix**: `Dictionary<int, int> tangibleIdToUniqueId` mapea `tangible.Id` del TE → `uniqueId` del manager
- `SimulateDiscPlaced()` ahora retorna `int uniqueId`
- Nuevo método `UpdateDiscPosition(uniqueId, position)` en TangibleDiscManager
- Guarda en HandleTangibleAdded: si tangible.Id ya existe, redirige a update (evita duplicados por reconexión)

#### FIX: Coordenadas TE → Canvas (ConvertToCanvasPosition)
- TangibleBridge usaba coordenadas del TE (píxeles de pantalla física, 1920x1080 en IDEUM) directamente
- El canvas/cámara está configurado para 4096x2160 → nodos aparecían en cuadrante inferior-izquierdo
- **Fix**: nuevo método `ConvertToCanvasPosition()` escala usando `Display.main.systemWidth/Height`
- `canvasWidth` y `canvasHeight` serializados (default 4096x2160)
- Aplicado en HandleTangibleAdded y HandleTangibleUpdated