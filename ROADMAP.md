# ROADMAP - SimuladorRedes IDEUM

> Este archivo es el backlog persistente del proyecto. El agente `main` lo lee al inicio de cada sesion y lo usa para guiar el trabajo. Se actualiza automaticamente a medida que se completan tareas.

---

## Estado Actual: 328 tests, pruebas en mesa IDEUM completadas

> Tras 22 sesiones: **328 tests**. Sesion 23+24:
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
> - **P1 (CRITICO) — Error de RAM/Colapso**: App colapsa por falta de RAM tras uso prolongado o actividades demandantes. Posible fuga de memoria en creacion/destruccion de paneles o manejo de texturas.
> - **P2 (ALTO) — Lineas de conexion invisibles en discos fisicos**: Al crear enlaces entre dispositivos fisicos, la linea verde no se renderiza. Se cambio a RawImage (sin sprite), alpha=1, 10px, SetAsLastSibling. Persiste sin solucion.
> - **P3 (MEDIO) — Ajuste de tamanos UI**: Paneles y elementos necesitan ajuste fino para pantalla 4096x2160.
> - **Fix CRITICO: Sin EventSystem en escena**: `SetupCanvas()` creaba Canvas + GraphicRaycaster pero NUNCA creaba el EventSystem. Sin EventSystem, ningun click/toque funciona en UI (el input se pierde). Se agrego `SetupEventSystem()` que crea EventSystem + InputSystemUIInputModule (requerido por `activeInputHandler=2`).
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
| **P1** | Fix error de RAM/colapso por uso prolongado | **No iniciada** | A2 |
| **P2** | Fix lineas de conexion invisibles en discos fisicos | **No iniciada** | A2 |
| **P3** | Ajuste fino de tamanos UI para pantalla 4096x2160 | **No iniciada** | A2 |
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
