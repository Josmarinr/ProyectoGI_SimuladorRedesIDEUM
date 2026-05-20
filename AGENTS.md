# AGENTS.md - Guía para Agentes

## Proyecto: Simulador de Redes IDEUM

### Estructura del Proyecto

```
Assets/
├── Scripts/
│   ├── Network/           # Core networking
│   │   ├── TopologyManager.cs
│   │   ├── NetworkNode.cs
│   │   ├── NetworkLink.cs
│   │   ├── RoutingTable.cs
│   │   ├── ARPTable.cs
│   │   ├── IPValidation.cs
│   │   ├── RoutingProtocols.cs
│   │   ├── DiscConfiguration.cs    # 18 tipos de disco (6 físicos + 12 routing)
│   │   ├── VLANManager.cs         # Redes virtuales
│   │   ├── ACLManager.cs          # Listas de acceso
│   │   └── NATManager.cs          # Traducción de direcciones
│   │
│   ├── Tangible/          # TangibleEngine integration
│   │   ├── TangibleDiscManager.cs
│   │   ├── TangibleBridge.cs
│   │   ├── DiscEventHandler.cs     # Asocia discos 7-18 a routers, compone rutas
│   │   ├── RouteBuilderState.cs    # Estado transitorio para ruta parcial
│   │   └── DebugDiscSimulator.cs
│   │
│   ├── Simulation/        # Actividades académicas + Setup
│   │   ├── SceneSetup.cs          # ORQUESTADOR DE ESCENA (~379L, refactorizado)
│   │   ├── ActivityLoader.cs      # Carga actividades + escenarios (~800L)
│   │   ├── SceneCleanupService.cs # Limpieza y GoBackToMainMenu (singleton)
│   │   ├── BuildTopologyActivity.cs
│   │   ├── FindFaultActivity.cs
│   │   ├── RoutingTablesActivity.cs
│   │   ├── BestRouteActivity.cs   # NUEVA: selección de mejor ruta
│   │   ├── StaticRoutingActivity.cs
│   │   ├── DynamicRoutingActivity.cs
│   │   ├── PredefinedScenarios.cs
│   │   ├── ScoringSystem.cs
│   │   ├── SimulationControls.cs
│   │   ├── RoutingProtocols.cs
│   │   └── GameManager.cs
│   │
│   ├── UI/                # Visualización
│   │   ├── NodeVisualizer.cs
│   │   ├── PingVisualizer.cs
│   │   ├── ConnectivityTestPanel.cs
│   │   ├── ActivityPanel.cs
│   │   ├── LinkModeController.cs     # NUEVO: Modo CONEXIÓN/DESCONEXIÓN
│   │   ├── PingModeController.cs     # NUEVO: Modo ping + selector de nodos
│   │   ├── IPConfigController.cs     # NUEVO: Panel IP (configuración de nodos)
│   │   ├── DevicePanelController.cs  # NUEVO: Panel dispositivos + score
│   │   ├── NodeInteractionController.cs # NUEVO: Click en nodos (fachada)
│   │   └── ...
│   │
│   └── Core/              # Utilidades
│       └── AppLogger.cs  # Logger centralizado (EnableLogging = false)
```

### Comandos de Debug (Keyboard)

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
| ESC | Volver / Cerrar panel |

### Para Testing sin Discos Físicos

- DebugDiscSimulator.cs tiene `enableSimulation = false` por defecto
- Para probar con teclado en PC normal: setear `enableSimulation = true`
- Teclas: 1=Router, 2=Switch, 3=PC, 4=Modo CONEXIÓN, 5=Fallo, C=Limpiar, P=Ping, R=Eliminar
- Para producción en mesa IDEUM: mantener `enableSimulation = false`

### Flujo de Datos de Discos (IDEUM)

```
Mesa IDEUM → TE Service → TangibleEngine → TangibleBridge → TangibleDiscManager → DiscEventHandler → TopologyManager → NodeVisualizer
```

### Mapeo tangibleId → uniqueId (TangibleBridge)

- `TangibleBridge` mantiene `Dictionary<int,int> tangibleIdToUniqueId` que mapea `tangible.Id` (del servicio TE) → `uniqueId` (generado por TangibleDiscManager)
- `HandleTangibleAdded`: convierte coordenadas TE → canvas con `ConvertToCanvasPosition()`, llama a `SimulateDiscPlaced()`, guarda mapping
- `HandleTangibleUpdated`: busca por `tangible.Id` en el mapping, actualiza posición si se movió >5px
- `HandleTangibleRemoved`: busca por `tangible.Id`, remueve el disco
- Si el TE re-dispara `OnTangibleAdded` para un tangible ya conocido (reconexión), redirige a `HandleTangibleUpdated` sin duplicar

### Posiciones de Discos

- **Debug spawn positions**: X: 100-300, Y: 250-700
- **Escenarios**: offsetX: 100, offsetY: -50, viewW: 1000, viewH: 500
- **Coordenadas TE → Canvas**: `ConvertToCanvasPosition()` mapea coordenadas físicas (1920x1080) al canvas (4096x2160) usando `Display.main.systemWidth/Height`

### Paneles UI

- **TopologyInfoPanel**: Esquina superior derecha (anchor 1,1)
- **DevicesPanel**: Esquina superior izquierda (anchor 0,1)
- **ScorePanel**: Esquina inferior derecha (anchor 1,0)
- **ScenariosPanel**: Centro, 800x800px
- **VLANPanel/ACLPanel/NATPanel**: Centro, se cierra al hacer click fuera

### Build y Despliegue

- **Escena principal**: `Assets/Main.unity` (contiene SceneSetup)
- `Assets/Scenes/GetStarted_Scene.unity` NO tiene SceneSetup — **no usar como escena principal**
- **Build para IDEUM**: File > Build Settings > Windows x86_64, copiar a mesa IDEUM
- **TangibleEngine en Runtime**: Usa modo Service (TCP localhost:4949), requiere el servicio IDEUM corriendo
- **Build para PC normal**: Poner `DebugDiscSimulator.enableSimulation = true` para testing con teclado
- El TangibleEngine falla silenciosamente si no encuentra el servicio — no bloquea la app

### Errores Conocidos (Históricos - Mayoría Corregidos)

1. ~~ShowConnectivityPanel sin SetupManagers~~ → **FIXED**: Ahora llama SetupManagers() + CreateVisualizer() + SubscribeToTopologyEvents()
2. ~~FindFaultActivity.OnSolveClicked limpiaba toda la topología~~ → **FIXED**: Ahora llama FixFault() que repara el fallo específico sin borrar
3. ~~CheckConnectivity no manejaba Switches~~ → **FIXED**: Switch es L2 transparente, no requiere IP para pasar tráfico
4. ~~LoadScenario sin SetupManagers~~ → **FIXED**: Escenarios ahora tienen SetupManagers + SimulationControls + ScoringSystem
5. ~~RoutingTablesActivity/StaticRoutingActivity UI estática~~ → **FIXED**: Text references conectadas desde SceneSetup a las actividades
6. ~~Default Gateway Faltante era no-op~~ → **FIXED**: Ahora limpia IP del PC, el usuario debe configurarla
7. ~~Sin PingVisualizer en ConnectivityTestPanel~~ → **FIXED**: AnimatePing() delega a PingVisualizer si existe
8. Borrosidad: Usar CanvasScaler con modo Expand
9. UI pequeña al cambiar escenas: SetupCanvas() re-configura scaler
10. InputField.font no existe: usar InputField.textComponent.font

### Bugs Conocidos Actuales

*(No hay bugs críticos activos)*

### Flujo de Inicialización por Actividad

| Actividad | SetupManagers | CreateVisualizer | SimulationControls | Cómo entra |
|-----------|:---:|:---:|:---:|---|
| INICIAR SIMULACIÓN | ✅ | ✅ | ✅ | StartSimulation() |
| Actividad 0-6 | ✅ | ✅ | ✅ | SelectActivity() |
| Escenarios | ✅ | ✅ | ✅ | LoadScenario() (FIXED) |
| PRUEBAS Y CONEXIONES | ✅ | ✅ | ❌ | ShowConnectivityPanel() |

### Discos de Configuración de Routing (IDs 7-18)

- 12 discos de configuración (RedDestino, Métrica, InterfazSalida, ModoEnrutamiento, IpRoute, Destino, Mascara, ProximoSalto, Vecino, AnunciarRed, Costo, BW)
- No crean nodos en la topología (filtrados por `DiscEventHandler.IsRoutingConfigDisc()`)
- **DiscEventHandler.HandleRoutingConfigDisc()**: colocar sobre un router → modifica su tabla de enrutamiento
  - `RedDestino`, `Mascara`, `ProximoSalto`, `InterfazSalida`: componen ruta parcial vía `RouteBuilderState` (por router). Cuando los 4 campos están completos → `ApplyToRouter()` ejecuta `AddStaticRoute`
  - `IpRoute`: agrega ruta por defecto `0.0.0.0/0 → 192.168.1.254`
  - `ModoEnrutamiento`: toggle `Static ↔ RIP/OSPF` en el builder
  - `Metrica`: ajusta métrica a 10 en la última ruta del router
  - `Vecino`, `AnunciarRed`, `Costo`, `BW`: logging "soporte próximamente" (requieren protocolo dinámico)
- `RouteBuilderState.cs`: mantiene `DestinationNetwork, SubnetMask, NextHop, OutInterface, Protocol` por router. `IsComplete` true cuando los 4 campos están seteados. `ApplyToRouter()` llama `AddStaticRoute` + asigna Protocol posteriormente

### Bugs Corregidos (Batch 2 - Mayo 2026)

| Bug | Fix |
|-----|-----|
| Doble P key (ConnectivityTestPanel + SimulationControls) | P unificado en SimulationControls |
| Doble ESC (SceneSetup + SimulationControls) | ESC unificado en SimulationControls: cierra info panel → IP config → menú |
| Routing faltante en escenarios 2-4 | Rutas estáticas auto-configuradas en BuildScenarioTopology |
| Tecla 4 creaba nodo Unknown | Ahora activa ToggleLinkMode("connect") |
| Actividad 0 sin panel | Nuevo BuildTopologyInfoPanel con instrucciones y tipos de topología |
| BestRoute activity faltante | Nueva BestRouteActivity + panel con 4 escenarios y puntaje |
| 12 discos de routing sin registro | Agregados a DefaultConfiguration con nombres y colores |
| SceneSetup 4246→2573 líneas | 16 paneles extraídos a UIPanelFactory.cs (1775 líneas), helpers a UIComponents.cs (558 líneas) |
| SceneSetup 2573→379 líneas (Mayo 2026 v2) | 5 controllers extraídos (LinkMode, PingMode, IPConfig, DevicePanel, NodeInteraction) + ActivityLoader (800L) + SceneCleanupService |
| Discos 7-18 no configuraban routers | Nuevo RouteBuilderState + HandleRoutingConfigDisc en DiscEventHandler; TopologyManager.FindNodesNear() |

### Notas Importantes

- Input System: Usar Input System Package (com.unity.inputsystem 1.19.0), NO Input Manager (Old)
- Target: Windows 10, 1920x1080
- No usar logarithmicGamma en CanvasScaler (no está disponible en todas las versiones)
- VLAN/ACL/NAT accesibles via topology.VLAN, topology.ACL, topology.NAT
- Lógica de VLAN/ACL/NAT aplicada en CheckConnectivity()
- translateSourceIP() y translateDestIP() disponibles para NAT
- NetworkNode tiene propiedad VlanId para asignación a VLANs
- La UI de simulación usa 5 controllers extraídos de SceneSetup: LinkModeController, PingModeController, IPConfigController, DevicePanelController, NodeInteractionController
- ActivityLoader centraliza la carga de actividades y escenarios (~800L)
- SceneCleanupService es un singleton que maneja limpieza y retorno al menú principal
- UIPanelFactory crea todos los paneles UI; UIComponents provee helpers (CreateCircleIcon, GetColor, etc.)
- 15 skills organizados en 6 carpetas para referencia rápida
- **Skill crítico**: `build-and-deploy` contiene el fix de pantalla negra y cómo buildeear para IDEUM vs PC
- `autoConnectLinks = true` en DiscEventHandler: los discos se conectan automáticamente si están a <300px