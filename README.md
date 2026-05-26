# Simulador de Redes - Proyecto IDEUM

> 📚 **Documentación completa disponible en [`docs/README.md`](docs/README.md)** — 21 diagramas UML, 9 archivos de API, manuales de usuario y desarrollador.

## Descripción del Proyecto

Simulador de redes académicas para mesas táctiles **IDEUM 55"** utilizando **Unity 6000.4.5f1 (Unity 6)** con **TangibleEngine**. El sistema permite emular dispositivos de red (routers, switches, PCs, servidores) mediante 6 discos físicos (PUCs) para educación en networking.

## Especificaciones Técnicas

- **Unity Version**: 6000.4.5f1 (Unity 6)
- **Input System**: Input System Package (com.unity.inputsystem 1.19.0), activado en modo Both (`activeInputHandler=2`). El código legacy con `Input.GetKeyDown()` sigue funcionando junto al nuevo sistema.
- **Target Platform**: Windows 10
- **Display**: IDEUM 55" (1920x1080)
- **Physical Devices**: 6 PUCs (Physical Discs)
- **SDK**: TangibleEngine (provided in UnityTE folder)

## Características Principales

- **18 tipos de disco**: Router, Switch, PC, Enlace, Fallo, Protocolo + 12 de configuración de routing
- **7 actividades académicas**: Construye la Topología, Encuentra el Fallo, Tabla de Enrutamiento Tangible, **Simulación de Mejor Ruta**, Enrutamiento Estático Tangible, Protocolo de Enrutamiento Dinámico Tangible, Escenarios Preconfigurados
- **Sistema de puntajes**: Evaluación automática con bonos y penalizaciones, panel visible en tiempo real
- **Panel de puntaje**: Esquina inferior derecha muestra puntuación actual actualizada cada segundo
- **Validación de IP**: Validación en tiempo real de direcciones IP y máscaras de subred
- **Ping visual animado**: Paquetes que siguen la ruta real con animación de éxito/fallo
- **Tablas ARP y Routing**: Gestionables para routers
- **Escenarios preconfigurados**: 5 escenarios con diferentes niveles de complejidad
- **Menú de escenarios**: UI mejorada con items diferenciables y hover effect
- **VLANs**: Redes virtuales con aislamiento (1-4094 VLANs)
- **ACLs**: Listas de control de acceso con reglas Permit/Deny
- **NAT**: Traducción de direcciones de red (estática, dinámica, PAT)

## Arquitectura del Sistema

```
┌─────────────────────────────────────────────────────────────┐
│                    MESA IDEUM (Discos Físicos)              │
│   [Disco 1]   [Disco 2]   [Disco 3]   [Disco 4]   ...     │
│   (Router)   (Switch)     (PC)     (Enlace)              │
└────────────────────────────┬────────────────────────────────┘
                             │ TangibleEngine SDK
                             ▼
┌─────────────────────────────────────────────────────────────┐
│                    TangibleBridge.cs                        │
│  - Recibe eventos OnTangibleAdded/Removed/Updated         │
│  - Mapea PatternId → DiscType (1-6)                         │
│  - Convierte coordenadas físicas → posiciones en pantalla  │
└────────────────────────────┬────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────┐
│                 TangibleDiscManager.cs                     │
│  - Mantiene estado de discos activos (Dictionary)           │
│  - Genera IDs únicos (100, 101, 102...)                      │
│  - Dispara eventos: OnDiscPlaced, OnDiscMoved, OnDiscRemoved│
└────────────────────────────┬────────────────────────────────┘
                             │
              ┌──────────────┴──────────────┐
              ▼                             ▼
┌─────────────────────────┐    ┌─────────────────────────────┐
│    DebugDiscSimulator  │    │      Otros Managers         │
│  (Solo para testing)    │    │  • TopologyManager         │
│                         │    │  • ActivityLoader           │
│  Tecla 1 = Router      │    │  • SceneCleanupService      │
│  Tecla 2 = Switch      │    │  • BuildTopologyActivity    │
│  Tecla 3 = PC           │    │  • RoutingTablesActivity     │
│  Tecla 4 = Modo CONEXIÓN │    │  • StaticRoutingActivity     │
│  Tecla 5 = Fallo         │    │  • DynamicRoutingActivity    │
│                           │    │  • FindFaultActivity         │
│  Tecla C = Limpiar       │    │  • BestRouteActivity          │
│  Tecla P = Ping test     │    │  • ScoringSystem             │
│                           │    │  • IPValidation              │
│                           │    │  • PingVisualizer            │
└─────────────────────────┘    └─────────────────────────────┘
```

### Modos de Operación

| Modo | Componente Activo | Uso |
|------|------------------|-----|
| **Debug** | `DebugDiscSimulator` | Testing con teclado |
| **Producción** | `TangibleBridge` | Mesa IDEUM real |

## Estructura del Proyecto

```
Assets/
├── Scripts/
│   ├── Network/           # Core networking classes
│   │   ├── NetworkNode.cs         - Entidad de dispositivo de red
│   │   ├── NetworkLink.cs         - Conexiones entre dispositivos
│   │   ├── TopologyManager.cs     - Gestor de topología + AddLink/RemoveLink
│   │   ├── DiscConfiguration.cs   - Config de los 6 discos
│   │   ├── RoutingTable.cs        - Tablas de enrutamiento
│   │   ├── ARPTable.cs            - Tablas ARP (routers)
│   │   ├── IPValidation.cs        - Validación de IP y máscara
│   │   └── RoutingProtocols.cs    - Protocolos de enrutamiento
│   │
│   ├── Tangible/          # TangibleEngine integration
│   │   ├── TangibleDiscManager.cs - Gestor de discos (IDs únicos)
│   │   ├── TangibleBridge.cs      - Bridge a TangibleEngine SDK
│   │   ├── DiscEventHandler.cs    - Eventos de disco
│   │   └── DebugDiscSimulator.cs  - Simulador de debug (teclado)
│   │
│   ├── Simulation/        # Actividades académicas
│   │   ├── SceneSetup.cs           - Orquestador de escena (~379L, refactorizado)
│   │   ├── ActivityLoader.cs       - Carga actividades + escenarios (~800L)
│   │   ├── SceneCleanupService.cs  - Limpieza y GoBackToMainMenu (singleton)
│   │   ├── BuildTopologyActivity.cs - Detección de topología
│   │   ├── FindFaultActivity.cs    - Simulación de fallos
│   │   ├── RoutingTablesActivity.cs - Tablas de enrutamiento
│   │   ├── StaticRoutingActivity.cs - Enrutamiento estático
│   │   ├── DynamicRoutingActivity.cs - Enrutamiento dinámico (RIP/OSPF)
│   │   ├── PredefinedScenarios.cs  - 5 escenarios preconfigurados
│   │   ├── ScoringSystem.cs        - Sistema de puntajes
│   │   ├── SimulationControls.cs   - Control de ESC y botones
│   │   ├── PingVisualizer.cs       - Animación visual de ping
│   │   ├── GameManager.cs          - Gestor del juego
│   │   └── TouchScriptDisabler.cs  - Oculta cursores táctiles
│   │
│   └── UI/                # Interfaz de usuario
│       ├── MainMenuManager.cs      - Gestor del menú principal
│       ├── MenuNavigator.cs        - Navegación por teclado + mouse
│       ├── NodeVisualizer.cs       - Visualización de nodos (circulares)
│       ├── TopologyVisualizer.cs   - Visualizador de topología
│       ├── ConnectivityTestPanel.cs - Panel de ping
│       ├── ActivityPanel.cs        - Panel de actividades
│       ├── LinkModeController.cs   - Modo CONEXIÓN/DESCONEXIÓN (extraído de SceneSetup)
│       ├── PingModeController.cs   - Modo ping + selector de nodos
│       ├── IPConfigController.cs   - Panel IP (configuración de nodos)
│       ├── DevicePanelController.cs - Panel dispositivos + score
│       ├── NodeInteractionController.cs - Click en nodos (fachada)
│       ├── UIPanelFactory.cs       - Fábrica de paneles UI
│       ├── IDEUMConfigurator.cs    - Configuración de pantalla IDEUM
│       └── UIComponents.cs         - Sistema de colores profesional + helpers
│
├── Editor/                # Tests (EditMode)
│   └── Tests/
│       ├── Network/
│       │   ├── TestIPValidation.cs   - 18 tests: IP, máscara, subred
│       │   └── TestRoutingTable.cs   - 14 tests: rutas estáticas/dinámicas
│       └── Tangible/
│           └── TestRouteBuilderState.cs - 15 tests: estado transitorio de ruta
│
├── TangibleEngine/        # SDK original de TangibleEngine
│   └── Scripts/
│
└── UnityTE/               # Recursos de TangibleEngine
```

## Sistema de Menús

### Menú Principal
Al iniciar la aplicación se muestra un menú principal con 5 opciones:
1. **INICIAR SIMULACIÓN** - Entra directo a la simulación de redes
2. **ACTIVIDADES** - Panel con 7 actividades académicas
3. **PRUEBAS Y CONEXIONES** - Panel de tests de conectividad
4. **CÓMO USAR** - Instrucciones en 2 columnas
5. **SALIR** - Cierra la aplicación

### Navegación
- **Flechas arriba/abajo** o **W/S**: Navegar entre opciones
- **Enter** o **Espacio**: Seleccionar opción
- **Tecla 1-5**: Ir directamente a la opción
- **ESC**: Volver al menú anterior (en subpaneles)
- **Click mouse**: Funciona en todos los paneles
- **Toque táctil**: Funciona en mesa IDEUM

## Panel de Simulación (TopologyInfoPanel)

Cuando se inicia la simulación aparece un panel en la esquina superior derecha con:

### Contenido
- **Topología: [tipo]** - Detección automática (Estrella, Bus, Anillo, Árbol, Malla)
- **Enlaces: [ número ]** - Número de conexiones activas
- **Dispositivos:**
  - Routers: X
  - Switches: X
  - PCs: X

### Botones
| Botón | Función |
|-------|---------|
| **LIMPIAR** | Limpia todos los dispositivos |
| **PING** | Prueba de conectividad |
| **CONECTAR** | Crear enlace manual entre nodos |
| **DESCONECTAR** | Eliminar enlace manual |
| **VOLVER** | Vuelve al menú principal |

### Cómo Usar CONECTAR/DESCONECTAR

**CONECTAR (Crear enlace manual):**
1. Presionar **CONECTAR** (el botón se activa)
2. **Click en el primer nodo** (se marca en amarillo)
3. **Click en el segundo nodo** (se crea el enlace)
4. Presionar botón otra vez para **desactivar**

**DESCONECTAR (Eliminar enlace):**
1. Presionar **DESCONECTAR** (el botón se activa)
2. **Click en el primer nodo** del enlace a eliminar
3. **Click en el segundo nodo** (se elimina el enlace)
4. Presionar botón otra vez para **desactivar**

### Botón de Información (i)
- Botón blanco con letra "i" junto al título "Topología"
- Al presionarlo muestra panel con tipos de topología y cómo construir

## Configuración de Discos (PUCs)

| Disco | Tecla | Tipo | Color | Función |
|-------|-------|------|-------|---------|
| 1 | 1 | Router | Azul | Dispositivo de enrutamiento |
| 2 | 2 | Switch | Cyan | Dispositivo de conmutación |
| 3 | 3 | PC | Verde | Host final |
| 4 | 4 | (modo link) | Amarillo | Conexión entre dispositivos |
| 5 | 5 | Fallo | Rojo | Simulación de errores |
| 6 | - | Protocolo | Magenta | Protocolo de enrutamiento |
| 7-18 | - | RedDestino..BW | Varios | Configuración de routing |

## Panel de Configuración de IP

Al hacer clic en un nodo (excepto enlaces), se abre un panel de configuración con:
- **Teclado numérico compacto**: 0-9, punto (.), DEL
- **Campos separados**: IP y Máscara de subred
- **Validación en tiempo real**: Verifica formato y valores válidos
- **Info de red**: Muestra dirección de red para routers
- **Botones**: APLICAR | CANCELAR | TABLA ARP | TABLA RUTAS
- **TABLA ARP y TABLA RUTAS**: Solo aparecen en routers

## Actividades Académicas

### 1. Construir Topología ✓
Detecta automáticamente el tipo de topología:
- **Estrella**: Un nodo central conectado a múltiples nodos perifericos
- **Bus**: Línea lineal de nodos
- **Anillo**: Nodos en ciclo cerrado
- **Árbol**: Estructura jerárquica con routers y switches
- **Malla**: Conexiones completas entre nodos

Lógica de detección:
```
Nodos = 0 → Sin topología
Nodos = 1 → Estrella
Enlaces = n*(n-1)/2 → Malla (todos conectados)
Enlaces = nodos → Anillo (cada uno al siguiente)
1 switch + routers → Estrella
2+ routers + switches → Árbol
Por defecto → Bus
```

### 2. Encuentra el Fallo ✓
Genera fallos aleatorios para diagnóstico:
- Cable desconectado
- IP incorrecta
- Máscara de subred incorrecta
- Interfaz down (administrativamente)
- Default gateway faltante

### 3. Tabla de Enrutamiento Tangible ✓
- RoutingTable para cada NetworkNode
- Métricas y next hops
- Botón PING integrado
- Visualización de rutas configuradas

### 4. Simulación de Mejor Ruta ✓
Selección de la mejor ruta hacia un destino entre varias opciones:
- **4 escenarios** con diferentes combinaciones de prefijo/métrica
- El estudiante elige la ruta correcta (longest prefix match + lowest metric)
- Explicación detallada de la respuesta correcta
- Puntaje acumulado visible

### 5. Enrutamiento Estático Tangible ✓
- El estudiante define rutas estáticas
- Validación de conectividad
- Botón PING para verificar rutas

### 6. Protocolo de Enrutamiento Dinámico Tangible ✓
- **RIP**: Conteo de hops (máx 15)
- **OSPF**: Costo por enlace
- Advertisement automático cada 3 segundos
- Convergencia automática
- Botón PING para verificar convergencia

### 7. Escenarios Preconfigurados ✓
5 escenarios con diferentes niveles:
- **Estrella Simple** (Básico): 1 switch, 3 PCs
- **Dos Routers** (Intermedio): 2 routers, 1 switch, 2 PCs
- **Topología en Anillo** (Intermedio): 4 routers en anillo
- **Red en Árbol** (Avanzado): Router raíz, switches, PCs
- **Detectar Fallos** (Intermedio): Red con fallos preconfigurados

## Sistema de Puntajes

- **Puntos base**: 100 pts por tarea completada
- **Bono de tiempo**: +50 pts si completa en < 2 minutos
- **Penalizaciones**: -20 pts por fallos
- **Rutas configuradas**: +15 pts por ruta válida
- **Ping exitoso**: +10 pts
- **Grado final**: Escala 1-5 basada en puntuación total

## Ping Visual Animado

- Paquete amarillo que sigue la ruta real
- Animación de paquete moviéndose por cada enlace
- Paquete se vuelve verde (éxito) o rojo (fallo)
- Muestra tiempo de respuesta

## Validación de IP

- Valida formato de dirección IP (4 octetos, 0-255)
- Valida máscara de subred válida (solo máscaras CIDR válidas)
- Muestra dirección de red calculada
- Validación en tiempo real mientras el usuario escribe

## Tablas ARP y Routing

### Tabla ARP (solo Routers)
- IP Address
- MAC Address
- Interfaz de salida

### Tabla de Enrutamiento (solo Routers)
- Red destino
- Máscara
- Next hop
- Interfaz
- Métrica
- **Editable**: Añadir/eliminar rutas estáticas

## Cómo Probar sin Discos Físicos

El proyecto incluye **DebugDiscSimulator** para probar con teclado:

```
1 = Añadir Router      2 = Añadir Switch      3 = Añadir PC
4 = Modo CONEXIÓN      5 = Añadir Fallo       6 = Protocolo
C = Limpiar todo       P = Test de conectividad (ping)
R = Eliminar dispositivo seleccionado
ESC = Volver al menú
```

Presiona **Play** en Unity y usa las teclas del teclado numérico o teclas alfanuméricas.

## Configuración para Pruebas en Editor

### Resolución
- **1920 x 1080** (Full HD)

### Game View
- En el dropdown de Aspect, selecciona **"1920 x 1080"** o **"16:9"**
- Escala: **1x** (o 0.85x si los elementos se ven muy grandes)

### Device Simulator
- Ve a **Window > General > Device Simulator**
- Selecciona **"1920 x 1080"**
- **Importante**: Cerrar Device Simulator si interfiera con la UI

### TouchScript Cursors
- En editor aparecen cursores táctiles de TouchScript
- **Solución**: TouchScriptDisabler.cs los oculta automáticamente

## Escenas

| Escena | Contiene SceneSetup | Propósito |
|--------|-------------------|-----------|
| `Assets/Main.unity` | ✅ Sí | Escena principal — usada en el build |
| `Assets/Scenes/GetStarted_Scene.unity` | ❌ No | Escena de respaldo — no usar como principal |

**⚠ Importante**: `EditorBuildSettings.asset` debe tener `Assets/Main.unity` como única escena activa. Si solo está `GetStarted_Scene.unity`, el build mostrará pantalla negra porque `SceneSetup.Awake()` nunca se ejecuta.

## Auto-Configuración de Escena (Refactorizada)

`Assets/Scripts/Simulation/SceneSetup.cs` (~379L) es el orquestador que delega en:  
- **ActivityLoader.cs** (~800L): Carga actividades y escenarios
- **SceneCleanupService.cs** (singleton): Limpieza y retorno al menú principal
- **5 UI Controllers** extraídos:
  - `LinkModeController.cs` — Modo CONEXIÓN/DESCONEXIÓN
  - `PingModeController.cs` — Modo ping + selector de nodos
  - `IPConfigController.cs` — Panel IP y configuración de nodos
  - `DevicePanelController.cs` — Panel dispositivos + score
  - `NodeInteractionController.cs` — Click en nodos (fachada)
- **UIPanelFactory.cs** + **UIComponents.cs**: Creación de paneles y helpers visuales

Originalmente SceneSetup tenía ~4250 líneas; tras dos refactors quedó en ~379L con responsabilidades claras.

## IDEUMConfigurator

Script que configura automáticamente la pantalla para la mesa IDEUM:
- Resolución 1920x1080
- Cámara ortográfica
- Fondo oscuro profesional

## Sistema de Colores (Teoría del Color 2026)

Paleta de colores basada en investigación para interfaces de juegos de uso prolongado:

| Color | Hex | Uso |
|-------|-----|-----|
| Background Base | #0D1117 | Fondo principal oscuro |
| Surface Panel | #161B22 | Paneles y cards |
| Surface Elevated | #21262D | Elementos elevados |
| Border Accent | #58A6FF | Bordes azul suave |
| Text Primary | #E6EDF3 | Texto principal off-white |
| Text Secondary | #8B949E | Texto secundario gris |
| Text Accent | #58A6FF | Texto accent azul |
| Button Normal | #213F66 | Botón azul oscuro |
| Button Hover | #2D5066 | Botón hover |
| Button Selected | #38689E | Botón seleccionado |

### Principios Aplicados
- **No usar negro puro** (#000000) - causa halación y fatiga visual
- **Off-white para texto** (#E6EDF3) - menos contraste agresivo
- **Azules desaturados** - profesionales para aplicaciones de networking
- **Contraste WCAG 4.5:1** - legible sin cansar la vista

## Problemas Conocidos y Soluciones

### Corregidos Recientemente (Mayo 2026)

| # | Bug | Síntoma | Fix |
|---|-----|---------|-----|
| 1 | **PRUEBAS Y CONEXIONES no funcionaba** | Al entrar, las teclas 1/2/3 no creaban dispositivos. No se podía hacer ping. | `ShowConnectivityPanel()` ahora llama `SetupManagers()` + `CreateVisualizer()` + `SubscribeToTopologyEvents()` |
| 2 | **Encuentra el Fallo limpiaba topología** | Al resolver un fallo, se borraban todos los nodos → la actividad se estancaba | `OnSolveClicked()` ya no llama `ClearTopology()`. Nuevo método `FixFault()` repara el fallo específico sin borrar la red |
| 3 | **Ping fallaba con Switches** | Switch no tiene IP, pero el código requería IP en ambos extremos → ping siempre fallaba | `CheckConnectivity()` ahora reconoce Switch como L2 transparente |
| 4 | **Escenarios sin controles** | ESC/P/R no funcionaban en escenarios preconfigurados | `LoadScenario()` ahora llama `SetupManagers()` + crea `SimulationControls` + `ScoringSystem` |
| 5 | **UI estática en Actividades 3 y 4** | Paneles de RoutingTables y StaticRouting mostraban texto hardcodeado | Conectar referencias `tableText`/`routesText`/`infoText` a los componentes de actividad desde SceneSetup |
| 6 | **Default Gateway Faltante no-op** | Aplicar el fallo asignaba IP válida (no rompía nada) | `ApplyFault()` ahora limpia IP del PC. `ValidateSolution()` verifica IP+máscara. `FixFault()` restaura configuración |
| 7 | **Sin PingVisualizer en ConnectivityTestPanel** | Solo texto, sin animación visual del paquete | `AnimatePing()` delega a `PingVisualizer.AnimatePing()` si existe, fallback a texto |
| 8 | **Doble P key** | Dos handlers para P → doble ping | P unificado en SimulationControls |
| 9 | **Doble ESC** | Dos handlers para ESC → conflicto | ESC unificado en SimulationControls |
| 10 | **Routing faltante escenarios 2-4** | Pings entre subredes fallaban sin rutas | Rutas estáticas auto-configuradas en BuildScenarioTopology |
| 11 | **Tecla 4 creaba Unknown** | Key 4 creaba nodo "Enlace" sin tipo válido | Cambiado a ToggleLinkMode("connect") |
| 12 | **BestRoute activity faltante** | No existía clase ni panel para actividad "Simulación de Mejor Ruta" | Nuevo BestRouteActivity.cs + panel |
| 13 | **Discos routing sin registro** | IDs 7-18 no estaban en DefaultConfiguration | Agregados con nombres y colores |

### Conocidos Actuales

*(No hay bugs críticos activos)*

### Históricos
- Errores de compilación: Cerrar Unity, eliminar carpeta `Library`, abrir de nuevo
- Fuentes no aparecen: Usar fallback a LegacyRuntime.ttf
- Nodos se sobreponían: IDs únicos (100, 101, 102...)
- TouchScript cursores en editor: TouchScriptDisabler.cs
- Device Simulator overlaying UI: Cerrar Device Simulator
- Panel de topología pequeño: Aumentado a 380x420
- Menú no funcionaba después de volver: Destruir MenuNavigator y MainMenuManager
- OnGUI de Debug visible: Eliminado
- Texto duplicado de Enlaces: Eliminado "Estado:" y ConnectivityText
- Lambdas null: `int capturedDiscId = node.DiscId`
- MissingReference: CleanupNullReferences()

## Estado Actual del Proyecto

### Completado ✓
- Sistema de menú principal con navegación por teclado + mouse (sin numeración)
- **7 actividades académicas completas**:
  1. Construye la Topología ✓
  2. Encuentra el Fallo ✓ (FIXED: ya no limpia topología al resolver)
  3. Tabla de Enrutamiento Tangible ✓
  4. Simulación de Mejor Ruta ✓
  5. Enrutamiento Estático Tangible ✓
  6. Protocolo de Enrutamiento Dinámico Tangible (RIP/OSPF) ✓
  7. Escenarios Preconfigurados ✓ (FIXED: ahora con controles completos)
- Panel de instrucciones de uso (Cómo Usar) con 2 columnas
- Panel de pruebas de conectividad (ConnectivityTestPanel) (FIXED: ahora con managers activos)
- **Botón PING** en TopologyInfoPanel
- **Botones CONECTAR/DESCONECTAR** para gestión manual de enlaces
- Detección de topología con lógica mejorada
- Nodos clicables con highlight amarillo en selección
- Botón "i" de información con panel de ejemplos de topología
- Navegación por teclado (flechas, números, ESC) y click de mouse
- Visualización de nodos como círculos con colores diferenciados
- UI premium con colores profesionales basados en teoría del color
- Sistema de TangibleBridge para integración con TangibleEngine
- DebugDiscSimulator para testing sin discos físicos
- TouchScriptDisabler para ocultar cursores táctiles en editor
- **Panel de configuración de IP** con teclado numérico compacto
- **Tablas ARP** para routers
- **Tabla de Enrutamiento Tangible** editable para routers
- **Validación de IP** en tiempo real
- **Ping Visual Animado** con paquete siguiendo la ruta real
- **CheckConnectivity** con soporte para Switches (L2 transparentes) (FIXED)
- **Sistema de Puntajes** con bonos y penalizaciones
- **5 Escenarios Preconfigurados** con diferentes niveles
- **IDEUMConfigurator** para configuración automática de pantalla
- Sin GUI de debug (OnGUI eliminado)
- **Refactor completo de SceneSetup** (~4250L → ~379L): 5 UI controllers extraídos, ActivityLoader (~800L), SceneCleanupService (singleton), UIPanelFactory + UIComponents

### Completado Recientemente (Mayo 2026 - continuación)
- **Integración disco-actividades**: StaticRoutingActivity, RoutingTablesActivity y DynamicRoutingActivity ahora leen/escriben sobre el `router.RoutingTable` real (no solo el `RoutingSimulator` separado). Las rutas configuradas con discos 7-18 aparecen en la UI de las actividades.
- **Fix disco Destino (ID 12)**: Ahora tiene case propio en `HandleRoutingConfigDisc()` — antes caía al default (no-op).
- **ShowConnectivityPanel unificado**: ActivityLoader ahora usa `UIPanelFactory.CreateConnectivityPanel()` (struct `ConnectivityPanelRefs`), eliminando ~125 líneas de código duplicado de creación de UI.
- **CreateStatusPanel deprecado**: Marcado `[Obsolete]` — era código muerto sin callers.
- **Tests unitarios**: 47 tests EditMode para `IPValidation`, `RoutingTable` y `RouteBuilderState` en `Assets/Editor/Tests/`.

### Pendiente 🔄
- Testing con discos f├¡sicos en IDEUM
- Pruebas de usabilidad en IDEUM real

## Cómo Buildear

### Build para mesa IDEUM (producción con discos físicos)
1. `DebugDiscSimulator.enableSimulation = false` (ya está por defecto)
2. TangibleEngine usará modo Service (TCP localhost:4949)
3. **File > Build Settings > Windows x86_64**
4. Verificar que `Assets/Main.unity` esté como escena activa (índice 0)
5. Build y copiar carpeta a la mesa IDEUM
6. En la mesa debe estar corriendo el TangibleEngine Windows service

### Build para PC normal (testing con teclado)
1. `DebugDiscSimulator.enableSimulation = true`
2. **File > Build Settings > Windows x86_64**
3. TangibleEngine falla silenciosamente — no afecta la app
4. Usar teclas **1** (Router), **2** (Switch), **3** (PC) para agregar dispositivos

### Pantalla negra al abrir la app
**Causa**: La escena incorrecta está en Build Settings.  
**Solución**: `Assets/Main.unity` debe ser la primera escena en `EditorBuildSettings.asset`.  
Si solo está `Assets/Scenes/GetStarted_Scene.unity`, el componente `SceneSetup` nunca se ejecuta y no se crea la UI.

## Cómo Ejecutar

1. Abrir proyecto en Unity 6000.4.5f1 (Unity 6)
2. Ejecutar en Editor (Play) o hacer Build para Windows
3. Navegar con teclado, mouse o tocar en mesa IDEUM
4. Usar teclas 1-6 para simular discos (en modo debug)
5. Usar botones CONECTAR/DESCONECTAR para crear/eliminar enlaces
6. Click en nodo para configurar IP y ver tablas ARP/Routing
7. Usar botones VLAN/ACL/NAT en panel de topología para configurar这些功能

## Skills (opencode)

El proyecto incluye **18 skills** en formato opencode (`.opencode/skills/`).  
Los agentes cargan automáticamente el skill relevante con el tool `skill` según la tarea.

```
.opencode/skills/
├── best-practices/          # Arquitectura, patrones, checklist
├── build-and-deploy/        # Build IDEUM/PC, pantalla negra fix
├── debugging/               # Logging, profiling, errores comunes
├── dynamic-routing/         # RIP/OSPF
├── ideum-integration/       # TangibleEngine, discos, bridge
├── manual-links/            # CONECTAR/DESCONECTAR
├── menu-navigation/         # MenuNavigator, ESC, flechas
├── network-tables/          # IP, routing, ARP, ping
├── predefined-scenarios/    # 5 escenarios académicos
├── scoring-system/          # Puntajes, notas, evaluación
├── topology-detection/      # Detección automática de topología
├── ui-performance/          # Canvas, borrosidad, optimización
├── unity-scene-setup/       # Paneles, textos, FindUITexts
├── unity-ui-buttons/        # Botones, colores, textos
├── vlan-acl-nat/            # Redes virtuales, acceso, traducción
├── unity-code-style/        # Naming, Unity lifecycle, convenciones
├── testing-guide/           # Cómo escribir y ejecutar tests
└── agent-workflow/          # Comunicación y flujo entre agentes
```

Cada skill contiene código listo para copiar y explicaciones de la arquitectura.
Los agentes los usan automáticamente como referencia durante implementación, revisión y testing.

## Notas para Desarrolladores

- El namespace principal es `SimRedes` seguido de sub-namespaces: `Network`, `Tangible`, `Simulation`, `UI`
- Todos los scripts usan `UnityEngine.Debug.Log` para logging
- SceneSetup (~379L) se ejecuta automáticamente al hacer Play y delega en ActivityLoader, SceneCleanupService y 5 controllers
- El código está diseñado para Spanish-speaking users (mensajes en español)
- MenuNavigator.cs maneja la navegación por teclado y mouse de todos los paneles
- SimulationControls.cs maneja ESC, P (ping) y el botón de volver al menú
- TangibleDiscManager genera IDs únicos (BASE_DISC_ID + counter) para evitar colisiones
- TangibleBridge conecta con TangibleEngine para detectar discos físicos en IDEUM
- BuildTopologyActivity incluye FindUITexts() para auto-encontrar referencias UI por nombre
- NodeVisualizer usa EventTrigger para clicks en nodos y HighlightSelectedNode para marcar selección
- TopologyManager tiene métodos AddLink/RemoveLink para gestión manual de enlaces
- IPValidation valida IPs y máscaras de subred en tiempo real
- PingVisualizer anima el paquete siguiendo la ruta calculada por TopologyManager
- ScoringSystem核算a puntuación con bonos de tiempo y penalizaciones
- PredefinedScenarios carga 5 escenarios preconfigurados para práctica
- VLANManager maneja VLANs (crear, asignar nodos, aislamiento)
- ACLManager maneja ACLs (reglas permit/deny, filtrado por IP/puerto)
- NATManager maneja NAT (estática, dinámica, PAT)
- TopologyManager expone VLAN, ACL, NAT como propiedades públicas
- **Lógica aplicada**: ValidateVLAN() y ValidateACL() en CheckConnectivity()
- **NAT**: TranslateSourceIP() y TranslateDestIP() disponibles para traducción
- **Controllers de UI (extraídos de SceneSetup)**:
  - `LinkModeController` — maneja el estado CONECTAR/DESCONECTAR y la selección de 2 nodos
  - `PingModeController` — maneja el modo ping con selector origen/destino
  - `IPConfigController` — maneja el panel de configuración IP y teclado numérico
  - `DevicePanelController` — maneja el panel de dispositivos y el score visible
  - `NodeInteractionController` — fachada que orquesta clicks en nodos según modo activo
- **ActivityLoader** centraliza la lógica de carga de 7 actividades + 5 escenarios (~800L)
- **SceneCleanupService** es un singleton que maneja la destrucción ordenada al salir
- **Input System**: Activo en modo Both (`activeInputHandler = 2`). El código legacy con `Input.GetKeyDown()` funciona junto con el nuevo Input System Package. Para código nuevo, usar `UnityEngine.InputSystem`.
- **Tests**: 47 EditMode tests en `Assets/Editor/Tests/`. Ejecutar desde Test Runner → EditMode → Run All.
- **Código eliminado**: `ActivityPanel.cs` (sin callers), `GameManager.routingSimulator` y `GetRoutingSimulator()` (sin callers).

---

**Fecha de última actualización**: Mayo 2026
**Autor**: Sebastián Marín