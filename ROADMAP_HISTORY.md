# ROADMAP_HISTORY.md - Historial de Sesiones (Mayo 2026)

> Historial extraido de ROADMAP.md para reducir tokens en el system prompt.
> Las tareas completadas estan en ROADMAP.md; este archivo solo guarda el log historico.

---

### Sesion 1 — Mejora del sistema de agentes
- Rediseno completo del sistema de automatizacion:
  - ROADMAP.md creado como backlog persistente
  - main.md: ahora es autonomo, lee ROADMAP al inicio, toma iniciativa
  - Subagentes: corregido uso de `subagent_type=architect|programmer|reviewer|tester` (antes usaban `general`)
  - architect.md: integrado con skills, contexto de proyecto
  - programmer.md: auto-verificacion post-edit, convenciones reales
  - reviewer.md: checklist exhaustivo especifico del proyecto
  - tester.md: comandos Unity CLI reales
  - builder.md: nuevo agente de build/deploy
  - opencode.json: skills registrados, builder agent anadido
  - AGENTS.md: actualizado con referencia a ROADMAP y nuevo flujo

### Sesion 2 — Tests y persistencia de rutas
- A4: Ejecutados 50/50 tests (47 originales + 3 nuevos B1). Suite completa pasando.
- B1: Verificada persistencia de rutas de discos 7-18:
  - Creado TestRoutePersistence.cs con 3 tests nuevos
  - Confirmado: SelectActivity() NO destruye TopologyManager -> rutas persisten
  - Hallazgo: RefreshRoutingTables() mezcla rutas de discos con rutas de ejemplo sin limpiar

### Sesion 3 — B2 y B3
- B3: RoutingSimulator refactorizado a clase estatica:
  - Eliminado el routerTables interno (estado paralelo)
  - Metodos ahora trabajan directamente sobre router.RoutingTable
  - Actualizados: RoutingTablesActivity, StaticRoutingActivity, DynamicRoutingActivity
- B2: ShowConnectivityPanel ahora autosuficiente:
  - Nuevo metodo EnsureManagersForConnectivity() en ActivityLoader
  - Crea TopologyManager, NodeVisualizer y PingVisualizer si no existen
  - Funciona sin SceneSetup escena

### Sesion 4 — Fix pantalla blanca y limpieza de paneles
- Bug: Pantalla blanca al hacer Play. Causa: escena GetStarted_Scene.unity abierta en lugar de Main.unity. Fix: instruir al usuario abrir Assets/Main.unity. Adicional: desactivado Native Render Pass, SSAO y HDR en URP.
- Bug: Menu principal visible al fondo. Causa: StartSimulation() y otros no ocultaban el menu. Fix: DestroyMainMenu() llamado al inicio de cada callback, DestroyPreviousPanels() para limpiar.
- Revision de Actividades: analisis profundo de las 7 actividades (0-6). Actividad 1 (FindFault) rota.

### Sesion 5 — Fix panel de FindFaultActivity
- Activity 1 (FindFault) reparada: campos `[SerializeField]` -> `public`, OnSolveClicked -> public. Nueva CreateFindFaultPanel() en UIPanelFactory. ActivityLoader case 1 conecta boton RESOLVER.

### Sesion 6 — Fix boton VOLVER en panel COMO USAR
- Bug: Boton VOLVER no funcionaba. Causa raiz: conflicto de destruccion diferida entre DestroyMainMenu() y CreateMainMenu(). Fix: usar DestroyImmediate() en MainMenuManager.

### Sesion 7 — Documentacion completa del proyecto
- C5 + C6: 21 diagramas UML, 9 archivos API, 2 manuales, docs/README.md. Total: 33 archivos, ~4,200 lineas.

### Sesion 8 — Migracion Input.GetKeyDown -> Input System
- C1: Migradas 37 llamadas en 7 archivos a Keyboard.current.wasPressedThisFrame. Key.Alpha1..6 corregido a Key.Digit1..6. 50/50 tests.

### Sesion 9 — Tests de integracion Disco -> Tabla -> UI
- B4: 10 tests en TestDiscToRouteIntegration.cs (flujo completo, multiples rondas, IpRoute, protocol override, routers independientes, puente UI, FindBestRoute). 60/60 tests.

### Sesion 10 — C2/C3/C4: Tooltips, logging TE, animacion de enlaces
- C2: Panel "Leyenda de Discos" con 18 discos, 2 secciones, hint de uso.
- C3: Logging mejorado en TangibleBridge (timestamp/nivel, try-catch, coroutine periodica), TangibleDiscManager (validacion discType).
- C4: Animacion fade-in 0.3s en NodeVisualizer.CreateLinkLine().

### Sesion 11 — Simplificacion a 3 discos fisicos IDEUM
- Flujo IDEUM simplificado: solo Router(1), Switch(2), PC(3) como discos fisicos. TangibleBridge eliminados patrones 4-6. DiscEventHandler ignora discos 4-18 en mesa.

### Sesion 12 — Boton automatico + inputs manuales en StaticRoutingActivity
- Coexisten ambos modos: ANIADIR RUTA (automatico) + ANIADIR MANUAL (4 inputs via CreateAddRoutePanel()). AddRoute() acepta outInterface opcional.

### Sesion 13 — DiscLegendPanel: circulos, layout horizontal y espaciado
- De cuadrados a circulos reales (CreateRoundedRectSprite, cornerRadius=8). Layout horizontal reorganizado. 3 secciones. Panel 820x1060. Tester agent reescrito. testing-guide skill actualizado a 60 tests.

### Sesion 14 — Auditoria null safety en actividades y controladores
- 15 archivos auditados. 6 bugs corregidos: FindFaultActivity (4 NPEs), RoutingTablesActivity (rutas duplicadas), BestRouteActivity (RectTransform), BuildTopologyActivity (link null), DynamicRoutingActivity (sin guards).

### Sesion 15 — Eliminacion de ~188 warnings de compilacion
- Migracion masiva CS0618: ~203 llamadas FindObjectOfType -> FindAnyObjectByType. 157 calls en 24 archivos. 6 campos CS0414 eliminados. A1: Build generado en Build/SimuladorRedes.exe.

### Sesion 16 — Fix 6 tests fallando por Awake() no ejecutado en EditMode
- 6 tests de Simulation/ fallaban en EditMode porque AddComponent no ejecuta Awake(). Fix: invocar Awake() via reflexion en SetUp. 108/108 tests.

### Sesion 17 — Discos virtuales 15-18 + Bug #1 fix
- B5: Vecino(15), AnunciarRed(16), Costo(17), BW(18) implementados como configuracion virtual desde DynamicRoutingActivity.
- Bug #1: DynamicRoutingProtocol.StartProtocol() ya no invoca OnConvergence prematuramente.

### Sesion 18 — Bug #2 + Bug #3 + 34 tests nuevos
- Bug #2: CalculateOSPFCost() usaba sentinela fragil (customCost != bwCost). Cambiado a int? customCost = null.
- Bug #3: TestRoutePersistence no limpiaba TopologyManager -> contaminaba tests. Agregado DestroyImmediate.
- 34 tests nuevos (16 DynamicRoutingProtocol + 18 DiscEventHandler). Total: 142 tests.

### Sesion 19 — Bug #4 + 67 tests nuevos + documentacion
- Bug #4: DynamicRoutingProtocol.StartProtocol() crasheaba con NRE (topology en Start en vez de Awake). Movido a Awake() + null guards.
- 67 tests nuevos (32 TopologyManager + 20 ActivityLoader + 15 TangibleBridge).
- Documentacion actualizada a 209 tests. Total: 209 tests.

### Sesion 20 — Refactor UIPanelFactory + documentacion
- UIPanelFactory.cs dividido en 3 archivos: 2,302 -> 834 lineas (-64%).
  - ActivityPanelFactory.cs (694L) — 8 metodos de actividades 0-6
  - ConfigPanelFactory.cs (790L) — 7 metodos de configuracion red
  - UIPanelFactory.cs (834L) — navegacion + info + helpers
- 16 llamadas actualizadas en ActivityLoader, IPConfigController, StaticRoutingActivity.
- 4 helpers cambiados de private a internal static.
- Best-practices skill: nueva seccion "Arquitectura de Factories (3 capas)".
- 209/209 tests pasando.

### Sesion 21 — Textos alineados con propuesta + tests UI factories + anteproyecto de grado
- Item 2: Textos de actividades alineados con propuesta academica en 4 archivos:
  - UIPanelFactory.cs: menu actividades + panel instrucciones + legend hint
  - ActivityPanelFactory.cs: titulos e info texts de las 6 actividades
  - ActivityLoader.cs: 6 Debug.Log actualizados
  - FindFaultActivity.cs: statusText actualizado
- Item 3: 22 tests nuevos de UI factories (3 suites):
  - TestUIPanelFactory.cs: 7 tests (menu, actividades, conectividad, instrucciones, legend, keypad)
  - TestActivityPanelFactory.cs: 8 tests (todas las actividades 0-6 + scenarios)
  - TestConfigPanelFactory.cs: 7 tests (IP, ARP, routing, VLAN, ACL, NAT)
  - Bug corregido en ConfigPanelFactory: NRE en InputFields de VLAN/ACL/NAT
  - Total: 231 tests pasando
- Item 4: Anteproyecto de Trabajo de Grado creado en docs/anteproyecto-grado.md
  - 12 secciones: problema, objetivos, marco teorico, metodologia, cronograma, etc.
- Commit: pendiente (sesion activa)

### Sesion 22 — Input migration final, EIGRP, SUS instruments + 85 tests nuevos
- **T2**: Migrados ultimos 5 usos de Input Manager (Mouse.current) en MenuNavigator.cs y DevicePanelController.cs
  - `Input.GetMouseButtonDown(0)` → `Mouse.current.leftButton.wasPressedThisFrame`
  - `Input.mousePosition` → `Mouse.current.position.ReadValue()`
  - Agregados null checks para Mouse.current
- **T3**: Eliminado metodo obsoleto `CreateStatusPanel` de UIPanelFactory.cs (lineas 814-822)
  - Confirmado: 0 referencias en todo el proyecto
- **T4**: 85 tests nuevos de red (4 suites):
  - TestARPTable.cs: 13 tests (entradas, duplicados, envejecimiento, busqueda)
  - TestVLANManager.cs: 18 tests (creacion, asignacion, comunicacion entre VLANs)
  - TestACLManager.cs: 29 tests (13 ACLRule + 16 ACLManager; wildcards, protocolos, puertos, orden)
  - TestNATManager.cs: 25 tests (static/dynamic/PAT, traduccion bidireccional, lookup)
- **T5**: Protocolo EIGRP implementado:
  - RoutingProtocols.cs: `EIGRP` al enum + `SimulateEIGRPAdvertisement()`
  - RoutingTable.cs: `AddEigrpRoute()` con protocolo "EIGRP"
  - DynamicRoutingProtocol.cs: `ProtocolType.EIGRP`, metrica compuesta (BW+Delay)*256, K values K1-K5
  - DynamicRoutingActivity.cs: mapeo EIGRP + SetKValue()
  - ActivityPanelFactory.cs: boton EIGRP en UI (4 columnas RIP/OSPF/EIGRP/START)
  - ActivityLoader.cs: case 5 con onSelectEIGRP
  - DiscEventHandler.cs: disco 6 (Protocolo) selecciona EIGRP
  - 12 tests nuevos (4 RoutingTable + 8 DynamicRoutingProtocol)
- **T6**: Instrumentos evaluacion usabilidad creados en docs/instrumentos-evaluacion-usabilidad.md
  - Pre-test (datos demograficos, autoevaluacion 9 temas, expectativas)
  - SUS estandar 10 preguntas adaptado al contexto tangible
  - Post-test (re-evaluacion, satisfaccion, preguntas abiertas)
- Total: ~316+ tests (85 nuevos T4 + 12 nuevos T5 + 231 existentes)
- **Pendiente**: A2 (pruebas en mesa IDEUM real)

### Sesion 23 — Bugfix FindFaultActivity + CODE_INDEX.md
- **Bug FindFaultActivity**: discos tactiles no se actualizaban en escenarios 3+
  - Raiz #1: Start() ejecutaba LoadScenario(0) antes de que ActivityPanelFactory conectara
    las referencias UI. Fix: nuevo metodo ConnectUI() llamado por CreateFindFaultPanel()
    DESPUES de crear los elementos graficos. Start() ya no carga escenarios.
  - Raiz #2: Al reingresar a la actividad, si FindFaultActivity ya existia en el GameManager,
    Start() no se volvia a ejecutar. Fix: ActivityLoader llama Restart() explicitamente.
  - Raiz #3: TangibleDiscManager acumulaba discos fantasma entre escenarios.
    Fix: ClearAllDiscs() al inicio de cada LoadScenario().
  - Safety net: FindUIReferences() busca componentes UI por nombre como fallback,
    + Update() reintenta conectar cada 1s.
- **CODE_INDEX.md**: indice compacto de los 46 scripts del proyecto (~18K tokens, bajo demanda)
- **328/328 tests pasando**

### Sesion 24 — Rediseno FindFaultActivity + IPConfig fix + botones pantalla + escenarios
- **Rediseno FindFaultActivity**: 4 escenarios ahora resolubles con interacciones UI reales:
  - Esc1 (Cable Caido): no se crea enlace, usuario usa CONECTAR en panel + click routers
  - Esc2 (IP Erronea): router con IP incorrecta, usuario la corrige via IPConfig
  - Esc3 (Mascara Incorrecta): reemplaza Interfaz Apagada, usuario corrige mascara via IPConfig
  - Esc4 (PC sin IP): usuario asigna IP via IPConfig
  - Boton VERIFICAR solo valida estado actual (no repara magicamente)
  - Eliminado RepairFault(), ValidateSolution() verifica estado real del dispositivo
- **Fix IPConfigPanel**: InputFields eliminados completamente
  - Reemplazados por Text + Button de seleccion + teclado inline por闭包
  - Cero dependencia del EventSystem focus (solucionaba problemas de click en InputField)
  - Teclado numerico inline, sin Button component (evita robo de foco)
- **Botones ROUTER/SWITCH/PC**: agregados al panel HUD Topologia
  - BuildTopologyActivity y modo simulacion libre ahora funcionan sin teclado
  - Posiciones predefinidas, discId unico (200+)
- **Escenarios predefinidos corregidos**:
  - Esc4 (Red en Arbol): Router1=10.0.0.2, Router2=10.0.0.3 (misma subred que Router0)
  - Esc5 (Detectar Fallos): fault.ip y fault.mask ahora se aplican al nodo
    (SetNodeFault solo guardaba etiqueta, no modificaba configuracion real)
- **328/328 tests pasando****
