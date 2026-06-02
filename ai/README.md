# SimuladorRedes IDEUM

## Proyecto

Simulador de redes tangible que funciona sobre las mesas interactivas IDEUM de la Universidad Distrital Francisco Jose de Caldas. Los estudiantes colocan discos fisicos (PUCs) sobre una pantalla tactil de 55" para representar routers, switches y PCs, y el sistema construye la topologia de red en tiempo real. Incluye 7 actividades academicas, enrutamiento estatico y dinamico (RIP, OSPF, EIGRP), deteccion automatica de topologias, y sistema de puntuacion.

---

## Directorios del Proyecto

```
Assets/
  Scripts/
    Network/      Logica de red: TopologyManager, RoutingTable, ARPTable, IPValidation, VLAN, ACL, NAT
    Tangible/     Integracion con discos fisicos: TangibleBridge, DiscManager, DiscEventHandler
    Simulation/   Actividades y escenas: SceneSetup, ActivityLoader, 7 actividades, ScoringSystem
    UI/           Interfaz de usuario: 3 factories, 5 controladores, visualizadores
    Core/         Utilidades: AppLogger (logging deshabilitado por defecto)
  Editor/Tests/   328 tests EditMode, 18 suites
  Main.unity      Escena principal del simulador
docs/
  api/            Documentacion de 9 APIs del sistema
  diagrams/       21 diagramas UML (casos de uso, clases, secuencia, actividad, estado, despliegue)
  manuals/        Manual de usuario y guia de desarrollo
  README.md       Indice de documentacion
```

---

## Arquitectura

### Capas del Sistema

```
Mesa IDEUM (discos fisicos + touch)
      |
TangibleEngine SDK (TUIO, TCP puerto 4949)
      |
TangibleBridge (mapea tangibleId -> uniqueId, convierte coordenadas 1920x1080 -> 4096x2160)
      |
TopologyManager (singleton central: nodos, enlaces, pathfinding, eventos)
      |
  +-------+--------+
  |                 |
Actividades (7)    UI (3 factories + 5 controladores)
  |                 |
RoutingTables      Paneles: Devices, Score, TopologyInfo, IPConfig, VLAN, ACL, NAT
Protocolos Dinamicos (RIP/OSPF/EIGRP)
```

### Eventos (Pub/Sub)

TopologyManager expone eventos que el resto del sistema consume:
- `OnNodeAdded`, `OnNodeRemoved`
- `OnLinkAdded`, `OnLinkRemoved`
- `OnTopologyChanged`

### Discos

| ID | Tipo | Descripcion |
|:--:|------|-------------|
| 1 | Router | Disco fisico - router |
| 2 | Switch | Disco fisico - switch |
| 3 | PC | Disco fisico - PC |
| 4-6 | Enlace, Fallo, Protocolo | Virtuales para actividades |
| 7-14 | RedDestino, Metrica, InterfazSalida, etc. | Virtuales - configuracion routing |
| 15-18 | Vecino, AnunciarRed, Costo, BW | Virtuales - routing dinamico |

### Paneles UI

- **TopologyInfo** (top-right): Informacion de la topologia actual
- **Devices** (top-left): Lista de dispositivos con interaccion
- **Score** (bottom-right): Puntaje y nota
- **Centrales** (centro): Scenarios, VLAN, ACL, NAT, DiscLegend
- **IPConfig** (modal): Configuracion de IP por dispositivo

### 3 Factories

| Factory | Responsabilidad |
|---------|-----------------|
| UIPanelFactory | Navegacion, menus, informacion general |
| ActivityPanelFactory | Paneles especificos de cada actividad |
| ConfigPanelFactory | Paneles de configuracion de red (IP, routing, VLAN, ACL, NAT) |

### 5 Controladores

| Controlador | Funcion |
|-------------|---------|
| LinkModeController | Modo CONECTAR/DESCONECTAR entre dispositivos |
| PingModeController | Modo PING para probar conectividad |
| IPConfigController | Panel de configuracion IP |
| DevicePanelController | Panel de dispositivos, seleccion y eliminacion |
| NodeInteractionController | Manejo unificado de clicks en nodos |

---

## 7 Actividades Academicas

| # | Actividad | Competencia |
|:-:|-----------|-------------|
| 0 | Construye la Topologia | Identificar topologias (estrella, bus, anillo, arbol, malla) |
| 1 | Encuentra el Fallo | Diagnosticar y reparar: cable caido, IP erronea, mascara incorrecta, PC sin IP |
| 2 | Tabla de Enrutamiento | Visualizar y comprender tablas de enrutamiento |
| 3 | Mejor Ruta | Elegir ruta optima (longest prefix match, metricas) |
| 4 | Enrutamiento Estatico | Configurar rutas manualmente |
| 5 | Enrutamiento Dinamico | Simular RIP, OSPF y EIGRP |
| 6 | Escenarios | 5 topologias preconfiguradas con objetivos |

---

## Datos Clave

### Tests (328 EditMode, 18 suites)

| Suite | Tests | Suite | Tests |
|-------|:-----:|-------|:-----:|
| TestIPValidation | 18 | TestRoutingTable | 18 |
| TestRouteBuilderState | 15 | TestRoutePersistence | 3 |
| TestDiscToRouteIntegration | 10 | TestTopologyManager | 32 |
| TestDynamicRoutingProtocol | 24 | TestActivityLoader | 20 |
| TestDiscEventHandler | 18 | TestTangibleBridge | 15 |
| TestScoringSystem | 19 | TestBestRouteActivity | 12 |
| TestSceneCleanupService | 4 | TestPredefinedScenarios | 13 |
| TestARPTable | 13 | TestVLANManager | 18 |
| TestACLManager | 29 | TestNATManager | 25 |

### Comandos Debug (Keyboard)

| Tecla | Accion | Tecla | Accion |
|-------|--------|-------|--------|
| 1 | Router | 2 | Switch |
| 3 | PC | 4 | Modo CONEXION |
| 5 | Fallo | C | Limpiar |
| P | Ping | R | Eliminar |
| ESC | Volver/Cerrar panel | | |

### Stack Tecnologico

| Componente | Especificacion |
|------------|----------------|
| Motor | Unity 6000.4.5f1 (Unity 6) |
| Lenguaje | C# (46 scripts, ~11,847 lineas) |
| Input | Input System Package 1.19.0 (migracion completa) |
| Mesa | IDEUM TangibleEngine SDK (TCP, puerto 4949) |
| Tests | NUnit + Unity Test Framework |

---

## Bugs Activos

| ID | Problema | Severidad |
|:--:|----------|:---------:|
| P1 | Fuga de RAM en uso prolongado - posible creacion/destruccion de paneles | Critica |
| P2 | Lineas de conexion invisibles entre discos fisicos (RawImage, 10px, alpha=1 no funciona) | Alta |
| P3 | Ajuste fino de tamanos UI para resolucion 4096x2160 | Media |

---

## Convenciones de Codigo

- **Nombres**: PascalCase en clases y metodos, camelCase en variables locales y parametros
- **Campos privados**: `_tipoCamello` con guion bajo
- **Inspector de Unity**: `[SerializeField] private` en vez de `public` para variables del inspector
- **Metodos Unity**: Awake > OnEnable > Start > Update en ese orden
- **Documentacion**: XML comments (`/// <summary>`) obligatorios en metodos nuevos o modificados
- **Fuentes**: Usar siempre `UIComponents.GetFont()` (cache singleton, no crear nuevas instancias)
- **Eventos**: Sistema Pub/Sub centralizado en TopologyManager

---

## Flujo de Trabajo con Agentes

1. `main` (coordinador) lee este README + ROADMAP.md al iniciar
2. Para tareas de 2+ archivos: main -> architect (plan) -> programmer (codigo) -> reviewer (revision) -> tester (tests)
3. Tareas simples (1-2 tool calls): las ejecuta main directamente
4. Skills disponibles en `ai/skills/` para contexto tecnico especifico

---

## Archivos Clave

| Archivo | Lineas | Rol |
|---------|:------:|-----|
| Assets/Scripts/Network/TopologyManager.cs | ~718 | Singleton central de red |
| Assets/Scripts/Simulation/SceneSetup.cs | ~606 | Inicializacion de escena y UI global |
| Assets/Scripts/Simulation/ActivityLoader.cs | ~843 | Cargador de actividades y protocolos |
| Assets/Scripts/UI/UIPanelFactory.cs | ~953 | Creacion de paneles de navegacion |
| Assets/Scripts/UI/ActivityPanelFactory.cs | ~857 | Paneles de actividades |
| Assets/Scripts/UI/ConfigPanelFactory.cs | ~882 | Paneles de configuracion |
| Assets/Scripts/UI/UIComponents.cs | ~907 | Componentes UI reutilizables |
| Assets/Scripts/Tangible/TangibleBridge.cs | ~225 | Puente con mesa IDEUM |
| Assets/Scripts/Tangible/DiscEventHandler.cs | ~407 | Eventos de discos y auto-conexion |
| Assets/Scripts/Simulation/DynamicRoutingProtocol.cs | ~487 | Protocolos dinamicos RIP/OSPF/EIGRP |
| Assets/Scripts/UI/DevicePanelController.cs | ~387 | Panel de dispositivos |
| Assets/Scripts/UI/NodeVisualizer.cs | ~311 | Visualizacion de nodos y enlaces |
