# Plan de Trabajo — Correcciones Documentales, Memoria y Limpieza de Código

> **Generado:** 2026-09-28 · **Rama:** `main` @ `8beb623`
> **Fuentes:** revisión documental completa + auditoría de memoria (colapso) + auditoría de organización de código
> **Estado:** 🔄 En curso — A ✅ · B ✅ (B7 medición en mesa) · C0-C2 ✅ · C3 siguiente

---

## Resumen ejecutivo

Tres frentes, ejecutados en este orden. Cada tarea tiene ID estable, archivos afectados y criterio de aceptación.

| Frente | Qué | Tareas | Riesgo | Orden |
|--------|-----|:------:|--------|:-----:|
| **A — Documentación** | Corregir ~15 inconsistencias + commitear DOC1 pendiente | A1–A7 | Bajo | 1º |
| **B — Colapso de memoria** | Fugas de Sprite/Textura + churn de GC que tumban la app | B1–B6 | Medio | 2º |
| **C — Limpieza de código** | Refactor por fases, riesgo creciente | C0–C6 | Bajo→Alto | 3º |

**Fuera de alcance por ahora:** P2 (líneas de conexión invisibles en discos físicos) — sigue en el backlog de ROADMAP, no se toca en este plan.

---

## Verificación global

Se corre al cerrar cada frente y antes de cada commit:

```bash
UNITY=$(ls /Applications/Unity/Hub/Editor/6000.4.5f1/Unity.app/Contents/MacOS/Unity 2>/dev/null || echo "")
$UNITY -runTests -testPlatform EditMode \
  -projectPath /Users/sebastianmarin/Downloads/ProyectoSimRedes/SimuladorRedes \
  -testResults /tmp/test-results.xml -logFile /tmp/unity-test-log.txt -batchmode -quit
# Sin -quit si Unity 6000 mata los tests. Filtro: -testFilter TestFoo
```

- [ ] **328/328 tests en verde** (baseline: ninguna tarea puede bajar este número salvo que elimine tests de código muerto junto con su código — documentarlo)
- [ ] Compilación sin errores ni warnings nuevos en Editor
- [ ] Frente B: conteo de `Sprite`/`Texture2D` en `Resources.FindObjectsOfTypeAll` **no crece** tras 30 min de idle + 5 min de arrastre simulado
- [ ] Frente A: todos los conteos citados verificados contra disco (grep)

---

## Frente A — Corrección documental (DOC2)

> Objetivo: que toda la documentación diga la verdad verificable. Prioridad alta: son los documentos que ve el jurado.

| ID | Tarea | Archivos | Esfuerzo | Criterio de aceptación |
|----|-------|----------|:--------:|------------------------|
| **A1** | Unificar conteós globales con la realidad: **47** scripts (no 46), **21** suites (no 18), **20** skills (no 19), **14** HTML Archify (no 9), ~15K líneas; `opencode.json` "209 tests" → 328 | `AGENTS.md`, `CODE_INDEX.md`, `docs/README.md`, `docs/knowledge-graph/KNOWLEDGE-GRAPH.md`, `docs/knowledge-graph/README.md`, `opencode.json` | S | grep en cada archivo no deja números viejos |
| **A2** | Arreglar `requirements.md`: tabla de prioridades (real: **Alta=7, Media=8, Baja=4, Crítica=1**); texto corrupto línea 275 (`debe提供`); matriz de trazabilidad con **IDs de test reales** (solo existen: `Test_TopologyManager`, `Test_IPValidation`, `Test_RoutingTable`, `Test_ActivityLoader` — el resto se corrige o se marca N/A); `FaultDetectionActivity.cs` → `FindFaultActivity.cs`; `RoutingEntry.cs` → clase en `RoutingTable.cs`; RNF-05 (9→14 Archify); resolver checkbox vacío vs `Estado: Implementada` | `docs/requirements.md` | M | Cada test/archivo citado existe en disco; sin caracteres chinos; totales cuadran |
| **A3** | Arreglar `docs/user-stories.md`: misma matriz de trazabilidad (20 de 24 IDs falsos); `TopologyActivity.cs` → `BuildTopologyActivity.cs`; `FaultDetectionActivity.cs` → `FindFaultActivity.cs`; `ConnectivityChecker` → inexistente (corregir); `RoutingEntry.cs` → `RoutingTable.cs` | `docs/user-stories.md` | S | Ídem A2 |
| **A4** | Corregir matriz poder/interés de `stakeholders.md`: etiquetas intercambiadas (alto poder/bajo interés = **mantener satisfechos**; bajo poder/alto interés = **mantener informado**); reubicar Desarrollador (Influencia=Alto) e IDEUM según su propio poder | `docs/stakeholders.md` | S | Matriz consistente con la tabla 9-16 |
| **A5** | Unificar la historia de bugs: `KNOWLEDGE-GRAPH.md` dice "0 bugs activos", `docs/README.md` y `ROADMAP.md` dicen **P2 activo**. Realidad: P2 abierto, P1/P3 cerrados | `docs/knowledge-graph/KNOWLEDGE-GRAPH.md`, `ROADMAP.md` | S | Una sola versión de la verdad |
| **A6** | Reparar links/paths: `ROADMAP.md:20` → ruta completa `docs/knowledge-graph/05-bugs-history.md`; `.gitignore:72` (regla de `presentacion-contenido.md` muerta); `docs/proposal-context.md` está gitignored pero referenciado por AGENTS.md y docs/README (decidir: trackear o quitar referencias); listar los 5 HTML Archify sin mostrar en `docs/README.md` | `ROADMAP.md`, `.gitignore`, `AGENTS.md`, `docs/README.md` | S | Cero referencias a paths inexistentes |
| **A7** | **Commitear la reorganización DOC1 pendiente** (41 cambios: 29 borrados + 10 untracked + 2 modificados). ⚠️ Requiere confirmación explícita antes de hacer commit | repo completo | S | `git status` limpio tras commit conventional |

**Decisión pendiente (marca cuando llegue):** agentes — `AGENTS.md`/`opencode.json` definen 7 (con `documenter`), en disco hay 6 (`documenter.md` no existe). Opción 1: crear el archivo faltante. Opción 2: sacarlo de los docs. Mismo problema con `ConfigPanelFactory.cs:189` (comentario `闭包` chino).

---

## Frente B — Colapso de memoria

> Objetivo: eliminar el crecimiento ilimitado de memoria. Evidencia: la sesión 25 (`8beb623`) solo tuneó cachés de textura; las fugas reales siguen abiertas.

### Hallazgos rankeados (de la auditoría)

1. **Rebuild completo del DevicesPanel en cada evento de topología** — mover un disco dispara `OnTopologyChanged → RefreshDevicesPanel()` que destruye y recrea el panel completo, creando sprites nuevos por evento (`SceneSetup.cs:335` ← `TopologyManager.cs:115` ← `DiscEventHandler.cs:321`). En la mesa, arrastre continuo = decenas de Hz.
2. **Loop de 2 segundos que fuga texturas** aunque nadie toque nada: `DevicePanelController.cs:31-39` → `:338-346` destruye el sprite pero no su `Texture2D` y la caché queda invalidada → recrea → repite.
3. **`Sprite.Create` sin teardown en 12 sitios** (solo 2 sitios destruyen); escena única, nunca se recicla nada, cero `Resources.UnloadUnusedAssets()`.
4. **Limpieza con nombres que no coinciden**: `SceneCleanupService` busca `"ClickOutsideBG"`, los reales son `ClickOutsideBG_VLANPanel/ACLPanel/NATPanel`; faltan 9 paneles en la lista de limpieza.
5. **Churn de GC**: `ConnectivityTestPanel.Update` (allocs por frame), `NodeVisualizer.DrawLinks` (destruye+recrea todos los enlaces por evento), 100+ `Debug.Log` sin gate en build.

| ID | Tarea | Archivos clave | Esfuerzo | Criterio de aceptación |
|----|-------|----------------|:--------:|------------------------|
| **B1** | Diagnóstico reproducible: logger temporal de `Resources.FindObjectsOfTypeAll<Sprite/Texture2D>().Length` cada 5 s + escenarios (30 min idle / arrastre simulado 10 Hz 60 s / abrir-cerrar VLAN 20×) | script temporal de diagnóstico | S | Curva de crecimiento medida antes de tocar código (baseline) |
| **B2** | Fix fuga #1: evitar rebuild total del DevicesPanel por evento — debounce + actualizar posiciones/elementos in-place en vez de destruir/recrear | `SceneSetup.cs`, `DevicePanelController.cs` | M | Mover discos 5 min no incrementa el conteo de sprites |
| **B3** | Fix fuga #2: loop de 2 s — destruir `Texture2D` junto al sprite, o dejar de romper la caché | `DevicePanelController.cs:31-39,338-346,422-449` | S | Idle 30 min → conteo plano |
| **B4** | Fix fuga #3: helper único `SafeDestroySprite(Sprite)` y aplicarlo a los 12 sitios de creación; limpiar `deviceIconCache` en `ClearTextureCache` | `UIComponents.cs` + factories | M | Todo sprite creado tiene su teardown |
| **B5** | Fix fuga #4: corregir nombre `ClickOutsideBG_*`, sumar los 9 paneles faltantes a la limpieza, parentear `PingPackets` a su componente | `SceneCleanupService.cs:85-90`, `ConfigPanelFactory.cs`, `PingVisualizer.cs:35` | S | Volver al menú 20× sin huérfanos |
| **B6** | Fix churn: throttling de `ConnectivityTestPanel.Update`, `DrawLinks` solo cuando cambia la topología (no la posición), gate de `Debug.Log` en builds | `ConnectivityTestPanel.cs:56-102`, `NodeVisualizer.cs:130-170`, múltiples | M | GC Alloc estable en Profiler |
| **B7** | Verificación final: re-correr B1 y comparar contra baseline + 328 tests | — | S | Gráfica antes/después + suite verde |

---

## Frente C — Limpieza de código (fases por riesgo)

> Objetivo: "código mucho más limpio y organizado" sin romper los 328 tests. Cada fase es un work-unit commit separado.

| ID | Fase | Alcance | Riesgo | Esfuerzo |
|----|------|---------|:------:|:--------:|
| **C0** | **Borrar código muerto** (~300 líneas): `ActivityLoader.cs:806-967` (4 métodos sin llamadas), 5 helpers sin uso en `UIComponents`, `CreateNumericKeypad` + su test por reflejo, `DynamicRoutingProtocol.FindLink`/`pingVis`, `ExitApplication` (solo lo llama un test); decisión sobre `Core/AppLogger.cs` (0 referencias — conectar o borrar) | Bajo | S |
| **C1** | **Helpers puros**: `TopologyManager.FindLink(a,b)` reemplaza 5 copias del predicado; un renderizador de tablas de rutas reemplaza 5 duplicados; alinear namespaces (`SceneSetup`, `TouchScriptDisabler`, `PointerClickHandler` usan `SimRedes` en vez de `SimRedes.Simulation`); eliminar wrappers `*Public` | Bajo | M |
| **C2** | **Consolidar primitivas UI**: keypad ×2 → 1, dropdown ×2 → 1 (borrar el sin uso), botones ×3 → 1. **Primero** agregar tests de comportamiento (hoy solo assert `DoesNotThrow`) | Medio | M |
| **C3** | **Scaffolding de actividades**: extraer `Start()` duplicado de 4 activities (elimina el fallback `new GameObject("TopologyManager")` que crea singletons fantasma); sacar los 125 líneas de datos de `FindFaultActivity.InitializeScenarios`; consolidar las **4 rutas de limpieza** en `SceneCleanupService` | Medio | M |
| **C4** | **Unificar input**: loop de teclado duplicado (`MenuNavigator` vs `MainMenuManager`) + resolver colisiones reales: `R` = refrescar tablas **vs** `R` = eliminar nodo; `P` = ping en dos controladores | Medio | S |
| **C5** | **Dividir god classes**: `ActivityLoader.cs` (969 líneas, 5 responsabilidades → HUD factory + scenario loader + dispatch) y `SceneSetup.cs` (626 → composition root vs navegación vs fachada) | Alto | L |
| **C6** | **Singletons/eventos**: reducir ~60 accesos a `.Instance`, arreglar lambdas huérfanas de `BuildTopologyActivity:51-53`. ⚠️ **No tocar `NodeVisualizer.CreateLinkLine`** mientras P2 esté abierto | Alto | L |

**Cobertura de tests (paralelo a C2+):** los archivos sin ningún test hoy: `SceneSetup` (626 ln), `FindFaultActivity`, `DevicePanelController`, `LinkModeController`, `PingVisualizer`, `NodeVisualizer`, entre otros. Agregar tests antes de refactorizar cada uno de ellos.

---

## Registro de progreso

- [x] **A1** conteos globales
- [x] **A2** requirements.md
- [x] **A3** user-stories.md
- [x] **A4** stakeholders.md
- [x] **A5** historia de bugs
- [x] **A6** links/paths
- [x] **A7** commit DOC1 (pendiente de confirmación)
- [x] **B1** diagnóstico baseline
- [x] **B2** rebuild DevicesPanel
- [x] **B3** loop 2 s texturas
- [x] **B4** teardown de sprites
- [x] **B5** limpieza de paneles
- [x] **B6** churn GC
- [x] **B7** verificación final
- [x] **C0** código muerto
- [x] **C1** helpers puros
- [x] **C2** primitivas UI
- [x] **C3** scaffolding actividades
- [x] **C4** input unificado
- [x] **C5** god classes
- [ ] **C6** singletons/eventos

---

## Criterios globales de aceptación

- [ ] 328/328 tests verdes (o menos solo si se elimina test de código muerto en la misma tarea, documentado)
- [ ] Cero inconsistencias de conteo en la documentación (verificado por grep)
- [ ] Memoria plana en idle 30 min y en arrastre simulado (B1 vs B7)
- [ ] Cada tarea cierra con un commit conventional en la rama
- [ ] Este documento y su espejo Engram (`odd/plan-de-trabajo/tasks`) se actualizan tras cada tarea
