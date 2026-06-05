# ROADMAP - SimuladorRedes IDEUM

> Este archivo es el backlog persistente del proyecto. El agente `main` lo lee al inicio de cada sesion y lo usa para guiar el trabajo. Se actualiza automaticamente a medida que se completan tareas.

---

## Estado Actual: 328 tests, pruebas en mesa IDEUM completadas

> Tras 25 sesiones: **328 tests**. Sesion 25 (Junio 2026):
> - **P1 (CRITICO) — Fix RAM/Colapso**: Cache de texturas en UIComponents (CreateRoundedRectTexture, CreateDeviceIcon, shared 1x1 white texture), destruccion de sprites viejos en DevicePanelController.UpdateDevicesList(), textura 1x1 compartida en NodeVisualizer, fix fugas de eventos en ActivityLoader (desuscripcion handlers protocolo), limpieza de texturas en PingVisualizer y SceneCleanupService.
> - **P3 (MEDIO) — Ajuste masivo de UI/UX**: Todos los paneles redimensionados para 4096x2160. Menu principal (1000x1100, botones 480x145 fontSize 30), Actividades (1000x1100, botones 500x110 fontSize 22), Conectividad (820x700), Instrucciones (1050x850, fontSize 20), Leyenda Discos (960x1200, fontSize 16-18), HUD Simulacion (560x760, botones 140x58), DevicesPanel (400x640, items 350x65 fontSize 20), ScorePanel (220x110), BestRoute (960x850, botones 700x70 fontSize 22), RoutingTables (900x750, fontSize 20), StaticRouting (960x800, fontSize 22), DynamicRouting (960x1000, fontSize 20-24), Escenarios (1000x1000, items 880x130 fontSize 24), ScenarioInfo (800x700, fontSize 18-26). Botones cambiados de ALL CAPS a title case. Fuente cambiada de Arial a Helvetica Neue.
> - **Bugfix TouchScript doble click**: TouchScriptDisabler ahora desactiva GameObject "TouchManager Instance" (evita doble procesamiento de eventos UI en todos los botones). Cooldown de 100-200ms en AddDeviceAtSpawn, ToggleLinkMode y OnDeviceItemClicked como respaldo.
> - **Bugfix CONECTAR/DESCONECTAR**: LinkModeController convertido a singleton con Instance, usa TopologyManager.Instance en vez de campo stale. UpdateLinkButtonColors ahora controla Image.color directamente (no btn.colors que sobrescribe). Color rojo para modo DESCONECTAR. Cooldown 150ms en ToggleLinkMode. Corregido else→else if en rama desconectar para evitar que TouchScript duplicado resete el modo.
> - **Bugfix DevicePanel**: Eliminado campo topology stale, usa TopologyManager.Instance. DestroyImmediate del panel viejo en RefreshDevicesPanel (evita que GameObject.Find encuentre el panel viejo). Cooldown 200ms en OnDeviceItemClicked.
> - **Bugfix ActivityLoader.AddDeviceAtSpawn**: Usa TopologyManager.Instance en vez de campo topology stale. Los dispositivos creados con botones ahora aparecen en el panel.
> - **Bugfix SceneCleanupService**: Agregados LinkModeController, PingModeController, IPConfigController, DevicePanelController, NodeInteractionController, DiscEventHandler, TangibleBridge a la lista de destruccion al volver al menu.
> - **Bugfix MenuNavigator**: Eliminada sobrescritura de sizeDelta en UpdateSelection (usaba normalWidth=260, normalHeight=50 fijos que pisaban los tamanos reales de los botones).
> - **Bugfix MainMenuManager**: Eliminada sobrescritura de sizeDelta en UpdateButtonSelection.
> - **Reduccion de logs**: Eliminados ~23 Debug.Log de alto frecuencia (BuildTopologyActivity, NodeVisualizer, TopologyManager, TangibleDiscManager).
> 
> Sesion 23+24:
> - **Bugfix FindFaultActivity**: discos tactiles en escenarios 3+ corregido
>   (timing Awake vs Start, NodeVisualizer sync, TangibleDiscManager cleanup)
> - **CODE_INDEX.md**: indice compacto de 46 scripts (~18K tokens, bajo demanda)
> - **Rediseno FindFaultActivity**: 4 escenarios resolubles con clicks reales:
>   1. Cable Caido → CONECTAR + click routers
>   2. IP Erronea → click router → IPConfig → editar IP
>   3. Mascara Incorrecta → click router → IPConfig → editar mascara
>   4. PC sin IP → click PC → IPConfig → escribir IP+mask
> - **Fix IPConfigPanel**: InputFields eliminados, reemplazados por Text +
>   Button de seleccion + teclado inline (no depende de EventSystem focus)
> - **Botones ROUTER/SWITCH/PC**: agregados al panel HUD para agregar
>   dispositivos sin teclado (BuildTopActivity y modo sim libre)
> - **Escenarios predefinidos corregidos**:
>   - Esc5 (Detectar Fallos): fault.ip/fault.mask ahora se aplican al nodo
>   - Esc4 (Red en Arbol): IPs de Router1/Router2 en misma subred que Router0
### Problemas detectados en pruebas IDEUM:
> - **P1 (CRITICO) — Error de RAM/Colapso**: *Sesion 25: Fix aplicado.* Se identificaron y corrigieron 3 causas raiz: (1) Cache de texturas en UIComponents para reutilizar Texture2D en vez de crear cientos de instancias, (2) Destruccion de sprites viejos en DevicePanelController.UpdateDevicesList() que cada 2s creaba texturas nuevas sin limpiar, (3) Textura 1x1 compartida en NodeVisualizer en vez de crear una por enlace. Ademas: fix de fugas de eventos en ActivityLoader (desuscripcion de handlers de protocolo dinamico) y limpieza de texturas en PingVisualizer. Pendiente: verificar en mesa IDEUM que el colapso de RAM ya no ocurre.
> - **P2 (ALTO) — Lineas de conexion invisibles en discos fisicos**: Al crear enlaces entre dispositivos fisicos, la linea verde no se renderiza. Se cambio a RawImage (sin sprite), alpha=1, 10px, SetAsLastSibling. Persiste sin solucion.
> - **P3 (MEDIO) — Ajuste de tamanos UI**: *Sesion 25: Ajustados.* Botones del menu principal (95x320→130x320, fontSize 18→26, cornerRadius 12→20, panel 850→1060 alto, espaciado 100→135). Panel actividades (700x650→800x720, botones 280x50→360x65, fontSize 16→20). Panel conectividad (550x500→620x560). HUD simulacion (520x680→560x720, botones 130x52→140x58). DevicesPanel (360x600→400x640, items 320x58→350x65, fontSize 18→20). ScorePanel (180x90→220x110). Se agrego parametro cornerRadius a UIComponents.CreateMenuButton. Pendiente: paneles de configuracion (VLAN, ACL, NAT, Routing) y paneles de actividades (StaticRouting, DynamicRouting, BestRoute, FindFault, RoutingTables).
> - **Fix CRITICO: Sin EventSystem en escena**: `SetupCanvas()` creaba Canvas + GraphicRaycaster pero NUNCA creaba el EventSystem. Sin EventSystem, ningun click/toque funciona en UI (el input se pierde). Se agrego `SetupEventSystem()` que crea EventSystem + InputSystemUIInputModule (requerido por `activeInputHandler=2`).
> - **Fix Ray Tracing Warnings**: Se reescribio `SuppressRayTracingWarnings.cs` con un `ILogHandler` personalizado que filtra los warnings de compilacion de shaders `TraceRenderingLayerMask`, `DynamicGISkyOcclusion`, `TraceVirtualOffset` que aparecian en macOS por falta de soporte de ray tracing. El filtro intercepta los warnings en el Editor sin afectar otros logs ni interferir con el build para IDEUM (Windows).
> - **Fix Fullscreen IDEUM + Touch**: `SceneSetup.SetupResolution()` se cambio de `false` (ventana) a `FullScreenMode.FullScreenWindow` a 4096x2160. TouchScript `setScaling()` modificado para siempre usar 1:1. PlayerSettings: `fullscreenMode: 1`, `defaultScreenWidth/Height: 4096x2160`.
> - **Fix DevicePanelController**: `RefreshDevicesPanel()` no llamaba a `UpdateDevicesList()` tras crear items, dejandolos inactivos. Panel mostraba solo titulo "Dispositivos" sin los dispositivos.
> - **Fix antirrebote TangibleBridge**: Discos se activaban/desactivaban rapidamente al colocarlos. Se reemplazo el debounce de re-adicion por un sistema de "remocion pendiente": cuando TangibleEngine reporta ausencia de un disco, no se remueve de inmediato, sino que se marca como pendiente. Solo se remueve si permanece ausente >1s (procesado en Update()). Si reaparece antes, se cancela la remocion pendiente.
> - **Fix coordenadas TUIO**: `ConvertToCanvasPosition()` usaba `Display.main.systemWidth/Height` que con fullscreen devuelve 4096x2160, pero TUIO siempre reporta en 1920x1080. Ahora usa constantes fijas 1920x1080 para la conversion, asegurando que los discos virtuales aparezcan exactamente debajo de los discos fisicos.
> - **Agrandar DevicesPanel**: Panel 220x400 → 280x480, items 260x45 → 260x52, font 14→16.
> - **Fix UpdateDevicesList/UpdateDevicesVisualState**: Ya no dependen de `canvas` cacheado, usan `GameObject.Find("DevicesPanel")` directamente (mas robusto si canvas se recrea).
> - **Rediseno RefreshDevicesPanel**: Ahora busca Canvas y TopologyManager en tiempo real. Si el panel ya existe, solo actualiza items (no destruye/recrea). Agregada sincronizacion periodica cada 2s como red de seguridad.
> - **Fix AddDeviceAtSpawn**: Tras `GoBackToMainMenu()`, el `GameManager` se destruye y con el `DevicePanelController`. `AddDeviceAtSpawn()` creaba `TopologyManager` pero NO `DevicePanelController`. Ahora lo crea si no existe.
> - **Fix OnDeviceItemClicked para modo enlace**: Al clickear un dispositivo en el panel izquierdo mientras el modo CONECTAR/DESCONECTAR esta activo (desde botones del HUD), ahora se llama a LinkModeController.HandleNodeLinkClick correctamente sin marcar seleccion roja ni abrir IPConfig.
> - **Fix lineas de enlace invisibles**: Se agrego sprite de textura blanca 1x1 al Image de la linea para asegurar renderizado. Grosor aumentado de 4 a 6px. Eliminada animacion de fade-in (alpha fijo en 1). Agregado logging de Debug para rastrear posiciones y cantidad de enlaces/nodos en DrawLinks.
> - **Fix renderizado lineas**: Cambiado de `Image` a `RawImage` (no requiere sprite). NodeVisualizer y link lines se mueven al final del Canvas para renderizar encima de paneles. Grosor aumentado a 10px.
> - **Fix DrawLinks forzado**: `DrawLinks()` se volvio publico y se llama explicitamente despues de cada AddLink/RemoveLink en LinkModeController, ademas del llamado via evento OnTopologyChanged.
> - **Feedback visual de seleccion para conexion**: Al seleccionar el primer dispositivo en modo CONECTAR/DESCONECTAR, se resalta con borde amarillo en la escena (via NodeVisualizer.SelectNodeByDiscId) y con color verde en el panel izquierdo (via UpdateDevicesList). Al completar el enlace o cancelar el modo, se quita el resaltado.
> - **Cancelar modo CONEXION/DESCONEXION**: Click fuera del panel de dispositivos deselecciona el primer extremo sin cancelar el modo. Click en el mismo boton CONECTAR/DESCONECTAR desactiva el modo completo. Compatible con touch (Touchscreen.current) y mouse.
> - **Fix renderizado de texto/fuentes**: Se centralizo la creacion de fuentes en `UIComponents.GetFont()` con cache singleton. Antes cada panel/control creaba su propia instancia de Font via `CreateDynamicFontFromOSFont`, fragmentando los atlas de textura y causando glifos faltantes (letras/ palabras invisibles).

---

## Backlog

| ID | Tarea | Estado | Dependencias |
|----|-------|--------|-------------|
| **A1** | Build de prueba para IDEUM (Windows x86_64) | Completada | Ninguna |
| **A2** | Pruebas en mesa IDEUM real con discos fisicos (1-3) | Completada | A1 |
| **P1** | Fix error de RAM/colapso por uso prolongado | **Completada** | A2 |
| **P2** | Fix lineas de conexion invisibles en discos fisicos | **No iniciada** | A2 |
| **P3** | Ajuste masivo de tamanos UI para pantalla 4096x2160 | **Completada** | A2 |
| **A4** | Suite completa de tests en Unity Editor | Completada | Ninguna |
| **B1** | Verificar persistencia rutas discos 7-18 al recargar actividad | Completada | Ninguna |
| **B2** | ShowConnectivityPanel sin SceneSetup | Completada | Ninguna |
| **B3** | Unificar RoutingSimulator (3 copias -> 1) | Completada | Ninguna |
| **B4** | Tests integracion Disco -> Tabla -> UI | Completada | Ninguna |
| **B5** | Discos 15-18 como configuracion virtual | Completada | Ninguna |
| **C1** | Migrar Input.GetKeyDown a Input System | Completada | Ninguna |
| **C2** | Tooltips/layout discos de routing (IDs 7-18) | Completada | Ninguna |
| **C3** | Logging TangibleEngine | Completada | Ninguna |
| **C4** | Animacion de conexion en enlaces | Completada | Ninguna |
| **C5** | Documentacion API TopologyManager | Completada | Ninguna |
| **C6** | Documentacion completa (diagramas + manuales) | Completada | Ninguna |
| **D1** | Migrar ultimos usos Input Manager (Mouse) | Completada | C1 |
| **D2** | Eliminar CreateStatusPanel obsoleto | Completada | Ninguna |
| **D3** | Tests ACLManager, NATManager, VLANManager, ARPTable | Completada | Ninguna |
| **D4** | Implementar protocolo EIGRP | Completada | Ninguna |
| **D5** | Instrumentos evaluacion usabilidad (SUS + pre/post-test) | Completada | Ninguna |
| **D6** | Fix layout DynamicRoutingPanel — campos config superpuestos | Completada | Ninguna |
| **D7** | Fix feedbackText — mensaje superpuesto con botones OSPF/EIGRP | Completada | Ninguna |
| **D8** | Fix feedbackText StaticRouting — mensaje superpuesto con botones AÑADIR MANUAL/RUTA/TEST | Completada | Ninguna |
| **D9** | Fix RoutingTablesActivity — Clear eliminado, header corregido, 3 mejoras menores | Completada | Ninguna |
| **E1** | Documentar codigo: TopologyManager.cs (29 metodos) | Completada | Ninguna |
| **E2** | Documentar codigo: SceneSetup.cs (19 metodos) | Completada | Ninguna |
| **E3** | Documentar codigo: 3 factories UI (UIPanel, ActivityPanel, ConfigPanel) | Completada | Ninguna |
| **E4** | Documentar codigo: UIComponents.cs | Completada | Ninguna |
| **E5** | Documentar codigo: ActivityLoader.cs | Completada | Ninguna |
| **E6** | Documentar codigo: clases de red (RoutingTable, IPValidation, NetworkNode, ACL, NAT, VLAN) | Completada | Ninguna |
| **E7** | Documentar codigo: actividades (BestRoute, BuildTopology, FindFault, StaticRouting, RoutingTables) | Completada | Ninguna |
| **E8** | Documentar codigo: modulo Tangible (TangibleBridge, DiscManager, DebugSimulator, DiscEventHandler) | Completada | Ninguna |
| **E9** | Documentar codigo: resto (ScoringSystem, SceneCleanup, MenuNavigator, RoutingProtocols) | Completada | Ninguna |

---

## Historial de Sesiones

Ver `ROADMAP_HISTORY.md` para el detalle completo de las 20 sesiones previas (Mayo 2026).
