# 03 — Puntos de Entrada (Entry Points)

> **Propósito**: Guía práctica — "Si quiero hacer X, debo modificar Y y probar Z".

---

## Modificar Comportamiento de Red

### "Quiero cambiar cómo se valida una IP"
- **Archivo**: `Assets/Scripts/Network/IPValidation.cs`
- **Método**: `IsValidIP()`, `ValidateIPField()`
- **Test**: `TestIPValidation.cs` (18 tests)
- **Skill**: `network-tables`

### "Quiero cambiar la lógica de routing (FindBestRoute)"
- **Archivo**: `Assets/Scripts/Network/RoutingTable.cs`
- **Método**: `FindBestRoute()` — longest prefix match + métrica
- **Test**: `TestRoutingTable.cs` (18 tests)
- **Skill**: `network-tables`

### "Quiero agregar un nuevo tipo de dispositivo"
- **Archivos**:
  1. `DiscConfiguration.cs` — agregar al enum `DiscType`
  2. `NetworkNode.cs` — propiedades específicas del tipo
  3. `UIComponents.cs` → `GetColorForDeviceType()` — color
  4. `NodeVisualizer.cs` → `CreateNodeVisual()` — icono
- **Test**: `TestTopologyManager.cs`
- **Skill**: `unity-ui-buttons`

### "Quiero modificar la conectividad (CheckConnectivity)"
- **Archivo**: `Assets/Scripts/Network/TopologyManager.cs`
- **Método**: `CheckConnectivity()` — BFS + validación IP
- **Test**: `TestTopologyManager.cs` (4 tests de conectividad)
- **Skill**: `network-tables`

### "Quiero agregar un nuevo protocolo de routing"
- **Archivos**:
  1. `DynamicRoutingProtocol.cs` — agregar al enum `ProtocolType` + lógica
  2. `RoutingProtocols.cs` — simular anuncio
  3. `DynamicRoutingActivity.cs` — UI del protocolo
- **Test**: `TestDynamicRoutingProtocol.cs` (24 tests)
- **Skill**: `dynamic-routing`

### "Quiero modificar VLAN/ACL/NAT"
- **Archivos**:
  - VLAN: `VLANManager.cs` → `TestVLANManager.cs`
  - ACL: `ACLManager.cs` → `TestACLManager.cs`
  - NAT: `NATManager.cs` → `TestNATManager.cs`
- **Panel UI**: `ConfigPanelFactory.cs` → `CreateVLANPanel()`, `CreateACLPanel()`, `CreateNATPanel()`
- **Skill**: `vlan-acl-nat`

---

## Modificar UI

### "Quiero agregar un panel nuevo"
1. Identificar dominio (navegación, actividad, configuración)
2. Agregar método en factory correspondiente:
   - Navegación/Info → `UIPanelFactory.cs`
   - Actividad → `ActivityPanelFactory.cs`
   - Configuración → `ConfigPanelFactory.cs`
3. Usar `UIComponents` para botones, textos, paneles
- **Test**: Agregar test en la suite correspondiente
- **Skill**: `unity-ui-buttons`, `unity-scene-setup`

### "Quiero cambiar el menú principal"
- **Archivos**: `UIPanelFactory.cs` → `CreateMainMenu()`, `MainMenuManager.cs`
- **Test**: `TestUIPanelFactory.cs`
- **Skill**: `menu-navigation`

### "Quiero cambiar colores de la UI"
- **Archivo**: `UIComponents.cs` → `Colors.*` (paleta centralizada)
- **Aliases**: `using UIColors = SimRedes.UI.UIComponents.Colors;`

### "Quiero cambiar tamaños de paneles"
- **Archivos**: Buscar el método `Create*Panel` en la factory correspondiente
- **Todos usan**: `UIComponents.CreateRoundedPanel()`, `CreateMenuButton()`, etc.

### "Quiero agregar un botón nuevo"
- **Archivo**: `UIComponents.cs` → `CreateMenuButton()`
- **Parámetros**: text, width, height, callback, fontSize, cornerRadius, borderWidth

### "Quiero cambiar fuentes"
- **Archivo**: `UIComponents.cs` → `GetFont()` (cache singleton)
- **SIEMPRE usar**: `UIComp.GetFont()` o `UIComponents.GetFont()`

---

## Modificar Tangible

### "Quiero cambiar cómo se detectan discos"
- **Archivos**:
  1. `TangibleBridge.cs` — mapeo tangibleId→uniqueId, coordenadas
  2. `TangibleDiscManager.cs` — estado de discos activos
  3. `DiscEventHandler.cs` — lógica al colocar/retirar disco
- **Test**: `TestTangibleBridge.cs`, `TestDiscEventHandler.cs`
- **Skill**: `ideum-integration`

### "Quiero cambiar el auto-connect de enlaces"
- **Archivo**: `DiscEventHandler.cs` → `autoConnectLinks=true`, `linkDistanceThreshold=300`
- **Test**: `TestDiscEventHandler.cs`

### "Quiero cambiar la configuración por discos (7-18)"
- **Archivos**:
  1. `DiscConfiguration.cs` — mapeo disco→tipo
  2. `DiscEventHandler.cs` → `HandleRoutingConfigDisc()`
  3. `RouteBuilderState.cs` — estado parcial
- **Test**: `TestDiscToRouteIntegration.cs`, `TestRouteBuilderState.cs`
- **Skill**: `manual-links`

### "Quiero activar el debug por teclado"
- **Archivo**: `DebugDiscSimulator.cs` → `enableSimulation=true`
- **Teclas**: 1=Router, 2=Switch, 3=PC, 4=CONEXION, 5=Fallo, C=Limpiar, P=Ping, R=Eliminar

---

## Modificar Actividades

### "Quiero agregar una actividad nueva"
1. Crear archivo en `Assets/Scripts/Simulation/`
2. Agregar en `ActivityLoader.cs` → `SelectActivity(n)` (case nuevo)
3. Crear panel en `ActivityPanelFactory.cs`
4. Agregar botón en `UIPanelFactory.cs` → `CreateActivitiesPanel()`
5. Agregar test en `TestActivityLoader.cs`

### "Quiero modificar FindFaultActivity"
- **Archivo**: `FindFaultActivity.cs` (455 líneas)
- **Escenarios**: `OnDiscButtonClicked()`, `OnNextClicked()`, `OnPrevClicked()`
- **Test**: `TestActivityLoader.cs` (indirecto)
- **Skill**: `topology-detection`

### "Quiero modificar DynamicRoutingActivity"
- **Archivo**: `DynamicRoutingActivity.cs` (278 líneas)
- **Config**: `SetProtocol()`, `StartProtocol()`, `StopProtocol()`
- **Test**: `TestDynamicRoutingProtocol.cs` (24 tests)
- **Skill**: `dynamic-routing`

### "Quiero modificar StaticRoutingActivity"
- **Archivo**: `StaticRoutingActivity.cs` (246 líneas)
- **Métodos**: `AddRoute()`, `ShowAddRoutePanel()`, `TestRouting()`
- **Skill**: `network-tables`

---

## Modificar Escenarios

### "Quiero agregar un escenario nuevo"
- **Archivo**: `PredefinedScenarios.cs` → `GetScenarios()`
- **Estructura**: `NetworkScenario` con devices, links, ipConfigs, faults, objectives, hints
- **Carga**: `ActivityLoader.cs` → `LoadScenario()`
- **Test**: `TestPredefinedScenarios.cs` (13 tests)
- **Skill**: `predefined-scenarios`

---

## Modificar Puntaje

### "Quiero cambiar la fórmula de puntaje"
- **Archivo**: `ScoringSystem.cs` → `GetCurrentScore()`, `GetGrade()`
- **Test**: `TestScoringSystem.cs` (19 tests)
- **Skill**: `scoring-system`

---

## Debugging

### "Hay un bug de UI"
1. Revisar `UIComponents.cs` (helpers base)
2. Revisar la factory correspondiente
3. Revisar `05-bugs-history.md` para bugs similares

### "Un panel no aparece"
1. Verificar que `SceneSetup.SetupCanvas()` creó el Canvas
2. Verificar que el EventSystem existe (`SetupEventSystem()`)
3. Verificar que `RefreshDevicesPanel()` fue llamado

### "Los discos no aparecen"
1. Verificar `TangibleBridge` (coordenadas TE→Canvas)
2. Verificar `TangibleDiscManager` (discos activos)
3. Verificar `DebugDiscSimulator` (`enableSimulation`)

### "El routing no funciona"
1. Verificar `RoutingTable.FindBestRoute()`
2. Verificar IPs y máscaras (`IPValidation`)
3. Verificar conectividad física (`CheckConnectivity`)

---

## Agregar un Test Nuevo

1. Identificar la clase a testear → `04-tests-map.md`
2. Crear archivo en `Assets/Editor/Tests/[Namespace]/TestMiClase.cs`
3. Usar patrón AAA (Arrange-Act-Assert)
4. Naming: `TestMiClase`, métodos: `Metodo_Escenario_Resultado`
5. Si la clase tiene `Awake()`, invocarlo manualmente en `[SetUp]`
6. Ejecutar: `unity -runTests -testPlatform EditMode -testFilter TestMiClase`
7. Verificar que no rompe tests existentes

**Skill**: `testing-guide`

---

## Agregar un Skill Nuevo

1. Crear directorio en `.opencode/skills/mi-skill/`
2. Crear `SKILL.md` con frontmatter YAML (name, description)
3. Agregar en `opencode.json` la sección `skills`
4. Actualizar `06-skills-map.md` con el nuevo skill
5. Referenciar archivos relevantes del proyecto

**Skill**: `customize-opencode`

---

## Flujo de Build para IDEUM

1. Verificar que `enableSimulation=true` en `DebugDiscSimulator` (para testing local)
2. En Unity: File → Build Settings → Windows x86_64
3. Scene: `Assets/Main.unity` (la única necesaria)
4. Build: clicking Build genera ejecutable en `Build/`
5. Para mesa IDEUM: copiar build a la máquina Windows
6. Verificar: TouchScript activo (no deshabilitado como en Editor)

**Skill**: `build-and-deploy`

---

## Flujo de un Cambio Típico

```
1. Identificar concepto → 02-concept-map.md
2. Encontrar archivos → 01-dependency-graph.md
3. Leer archivo fuente → Assets/Scripts/...
4. Modificar código (seguir unity-code-style)
5. Ejecutar test → 04-tests-map.md
6. Verificar no regresión → todos los tests relacionados
7. Actualizar documentación si aplica
8. Agregar /// <summary> si es método/clase nueva
```
