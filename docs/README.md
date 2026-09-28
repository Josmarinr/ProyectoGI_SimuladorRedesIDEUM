# Documentación — SimuladorRedes IDEUM

> Indice completo de documentacion tecnica, manuales y diagramas del proyecto.

---

## Estructura de Carpetas

```
docs/
├── README.md                    # Este archivo
├── stakeholders.md              # Análisis de stakeholders
├── user-stories.md              # 24 historias de usuario
├── requirements.md              # 20 RF + 10 RNF
│
├── api/                         # 9 archivos de documentación de APIs
├── manuals/                     # Manuales de usuario y desarrollador
├── config/                      # Configuración del proyecto
│   └── Config_TE.json           # Config TangibleEngine
│
├── diagrams/
│   ├── mermaid/                 # 21 diagramas UML (fuentes)
│   │   ├── packages/            # Subdiagramas de paquetes
│   │   └── *.md                 # Diagramas Mermaid
│   └── archify/                 # 14 diagramas interactivos (HTML)
│       ├── rendered/            # HTMLs renderizados
│       ├── architecture/        # JSONs de arquitectura
│       ├── sequence/            # JSONs de secuencia
│       └── workflow/            # JSONs de workflow
│
└── knowledge-graph/             # 9 archivos de grafo de conocimiento
```

---

## Documentos de Requerimientos

| Archivo | Contenido |
|---------|-----------|
| [`stakeholders.md`](stakeholders.md) | Stakeholders primarios/secundarios, intereses, matriz poder/interés |
| [`user-stories.md`](user-stories.md) | 24 historias de usuario con criterios de aceptación |
| [`requirements.md`](requirements.md) | 20 Requerimientos Funcionales (RF) + 10 No Funcionales (RNF) |

---

## Diagramas Archify (HTML Interactivo)

> Diagramas HTML autocontenidos con zoom, búsqueda, temas dark/light y exportación.
> Guía de uso: [`diagrams/archify/README.md`](diagrams/archify/README.md)

### Arquitectura
| Archivo | Descripción |
|---------|-------------|
| [`diagrams/archify/rendered/use-cases.architecture.html`](diagrams/archify/rendered/use-cases.architecture.html) | Casos de uso del sistema |
| [`diagrams/archify/rendered/deployment.architecture.html`](diagrams/archify/rendered/deployment.architecture.html) | Despliegue en mesa IDEUM |
| [`diagrams/archify/rendered/components.architecture.html`](diagrams/archify/rendered/components.architecture.html) | Componentes del sistema |
| [`diagrams/archify/rendered/class-network.architecture.html`](diagrams/archify/rendered/class-network.architecture.html) | Clases Network (Topology, Routing, IP) |
| [`diagrams/archify/rendered/class-ui.architecture.html`](diagrams/archify/rendered/class-ui.architecture.html) | Clases UI (Factories, Menu, Components) |
| [`diagrams/archify/rendered/simulador-redes.architecture.html`](diagrams/archify/rendered/simulador-redes.architecture.html) | Arquitectura general del sistema |
| [`diagrams/archify/rendered/network-namespace.architecture.html`](diagrams/archify/rendered/network-namespace.architecture.html) | Namespace Network (núcleo de red) |

### Secuencia
| Archivo | Descripción |
|---------|-------------|
| [`diagrams/archify/rendered/routing-config.sequence.html`](diagrams/archify/rendered/routing-config.sequence.html) | Configuración de ruta estática |
| [`diagrams/archify/rendered/disc-placement.sequence.html`](diagrams/archify/rendered/disc-placement.sequence.html) | Colocación de disco físico → nodo en topología |
| [`diagrams/archify/rendered/ping-connectivity.sequence.html`](diagrams/archify/rendered/ping-connectivity.sequence.html) | Prueba de conectividad (Ping) — BFS + IP |

### Workflow (Actividades)
| Archivo | Descripción |
|---------|-------------|
| [`diagrams/archify/rendered/activity-topology.workflow.html`](diagrams/archify/rendered/activity-topology.workflow.html) | Actividad: Construye la Topología |
| [`diagrams/archify/rendered/activity-faults.workflow.html`](diagrams/archify/rendered/activity-faults.workflow.html) | Actividad: Encuentra el Fallo |
| [`diagrams/archify/rendered/activity-dynamic.workflow.html`](diagrams/archify/rendered/activity-dynamic.workflow.html) | Actividad: Enrutamiento Dinámico |
| [`diagrams/archify/rendered/dynamic-routing.workflow.html`](diagrams/archify/rendered/dynamic-routing.workflow.html) | Routing dinámico — convergencia (RIP/OSPF/EIGRP) |

---

## Diagramas UML (Mermaid)

> Diagramas en formato Markdown con código Mermaid. Se renderizan en GitHub y Obsidian.

### Diagramas de Casos de Uso (5 sub-diagramas)
| Archivo | Sub-diagramas |
|---------|--------------|
| [`diagrams/mermaid/01-use-case.md`](diagrams/mermaid/01-use-case.md) | 01a: Gestión Simulación · 01b: Actividades · 01c: Enrutamiento · 01d: Config. Avanzada · 01e: Evaluación |

### Diagramas de Clases y Estructura (7 archivos, 20 sub-diagramas)
| Archivo | Sub-diagramas |
|---------|--------------|
| [`diagrams/mermaid/02-class-network.md`](diagrams/mermaid/02-class-network.md) | 02a: Núcleo Red · 02b: Tablas/ARP/IP · 02c: VLAN/ACL/NAT |
| [`diagrams/mermaid/03-class-tangible.md`](diagrams/mermaid/03-class-tangible.md) | Tangible Layer (managers, bridge, debug) |
| [`diagrams/mermaid/04-class-simulation.md`](diagrams/mermaid/04-class-simulation.md) | 04a: Núcleo Simulación · 04b: Actividades · 04c: Protocolo Dinámico · 04d: Puntajes |
| [`diagrams/mermaid/05-class-ui.md`](diagrams/mermaid/05-class-ui.md) | 05a: Factories · 05b: Controladores · 05c: Visualización · 05d: Navegación |
| [`diagrams/mermaid/06-packages.md`](diagrams/mermaid/06-packages.md) | 06a-06f: Paquetes/Namespaces (en [`packages/`](diagrams/mermaid/packages/)) |
| [`diagrams/mermaid/07-component-architecture.md`](diagrams/mermaid/07-component-architecture.md) | Componentes y capas |
| [`diagrams/mermaid/08-deployment.md`](diagrams/mermaid/08-deployment.md) | Despliegue IDEUM |

### Diagramas de Secuencia
| Archivo | Descripción |
|---------|-------------|
| [`diagrams/mermaid/09-sequence-disc-placement.md`](diagrams/mermaid/09-sequence-disc-placement.md) | Colocar disco físico → nodo visible |
| [`diagrams/mermaid/10-sequence-routing-config.md`](diagrams/mermaid/10-sequence-routing-config.md) | Configurar ruta con discos 7-18 |
| [`diagrams/mermaid/11-sequence-connectivity.md`](diagrams/mermaid/11-sequence-connectivity.md) | CheckConnectivity (BFS + VLAN + ACL + NAT) |
| [`diagrams/mermaid/12-sequence-menu-navigation.md`](diagrams/mermaid/12-sequence-menu-navigation.md) | Navegación de menús (ciclo completo) |
| [`diagrams/mermaid/13-sequence-dynamic-routing.md`](diagrams/mermaid/13-sequence-dynamic-routing.md) | Enrutamiento dinámico RIP/OSPF/EIGRP |

### Diagramas de Actividad
| Archivo | Descripción |
|---------|-------------|
| [`diagrams/mermaid/14-activity-topology.md`](diagrams/mermaid/14-activity-topology.md) | Actividad: Construye la Topología |
| [`diagrams/mermaid/15-activity-faults.md`](diagrams/mermaid/15-activity-faults.md) | Actividad: Encuentra el Fallo |
| [`diagrams/mermaid/16-activity-bestroute.md`](diagrams/mermaid/16-activity-bestroute.md) | Actividad: Simulación de Mejor Ruta |
| [`diagrams/mermaid/17-activity-static.md`](diagrams/mermaid/17-activity-static.md) | Actividad: Enrutamiento Estático Tangible |
| [`diagrams/mermaid/18-activity-dynamic.md`](diagrams/mermaid/18-activity-dynamic.md) | Actividad: Protocolo de Enrutamiento Dinámico Tangible |

### Diagramas de Estado
| Archivo | Descripción |
|---------|-------------|
| [`diagrams/mermaid/19-state-routebuilder.md`](diagrams/mermaid/19-state-routebuilder.md) | RouteBuilderState — ciclo de vida |
| [`diagrams/mermaid/20-state-protocol.md`](diagrams/mermaid/20-state-protocol.md) | DynamicRoutingProtocol — estados |
| [`diagrams/mermaid/21-state-scoring.md`](diagrams/mermaid/21-state-scoring.md) | ScoringSystem — sesión de puntaje |

---

## API Reference

| Archivo | Contenido |
|---------|-----------|
| [`api/01-topologymanager.md`](api/01-topologymanager.md) | TopologyManager — nodos, enlaces, pathfinding, eventos |
| [`api/02-routing.md`](api/02-routing.md) | RoutingTable, ARPTable, RoutingEntry, ARPEntry |
| [`api/03-ipvalidation.md`](api/03-ipvalidation.md) | IPValidation — validación y cálculos de red |
| [`api/04-scoring.md`](api/04-scoring.md) | ScoringSystem — puntajes, bonos, notas |
| [`api/05-tangible.md`](api/05-tangible.md) | TangibleDiscManager, TangibleBridge, RouteBuilderState, DebugDiscSimulator |
| [`api/06-vlan-acl-nat.md`](api/06-vlan-acl-nat.md) | VLANManager, ACLManager, NATManager |
| [`api/07-activities.md`](api/07-activities.md) | 7 actividades académicas — métodos públicos |
| [`api/08-disc-config.md`](api/08-disc-config.md) | DiscConfiguration, DiscType, DeviceType — 18 tipos de disco |
| [`api/09-events.md`](api/09-events.md) | Catálogo completo de eventos del sistema |

---

## Manuales

| Archivo | Audiencia | Contenido |
|---------|-----------|-----------|
| [`manuals/user-guide.md`](manuals/user-guide.md) | **Estudiantes** | Cómo usar el simulador: discos, actividades, IPs, puntajes |
| [`manuals/dev-guide.md`](manuals/dev-guide.md) | **Desarrolladores** | Arquitectura, convenciones, cómo extender, tests, build |
| [`manuals/Tangibles-3D-Printing.pdf`](manuals/Tangibles-3D-Printing.pdf) | **Hardware** | Guía de impresión 3D de discos tangibles |

---

## Knowledge Graph

| Archivo | Contenido |
|---------|-----------|
| [`knowledge-graph/README.md`](knowledge-graph/README.md) | Índice y guía de uso del grafo de conocimiento |
| [`knowledge-graph/KNOWLEDGE-GRAPH.md`](knowledge-graph/KNOWLEDGE-GRAPH.md) | Grafo de conocimiento global del proyecto |
| [`knowledge-graph/01-dependency-graph.md`](knowledge-graph/01-dependency-graph.md) | Dependencias entre archivos |
| [`knowledge-graph/02-concept-map.md`](knowledge-graph/02-concept-map.md) | Mapa de conceptos → archivos, tests, skills |
| [`knowledge-graph/03-entry-points.md`](knowledge-graph/03-entry-points.md) | Puntos de entrada por funcionalidad |
| [`knowledge-graph/04-tests-map.md`](knowledge-graph/04-tests-map.md) | Cobertura de tests por archivo |
| [`knowledge-graph/05-bugs-history.md`](knowledge-graph/05-bugs-history.md) | Historial de bugs corregidos |
| [`knowledge-graph/06-skills-map.md`](knowledge-graph/06-skills-map.md) | Skills → archivos referenciados |
| [`knowledge-graph/07-architectural-flows.md`](knowledge-graph/07-architectural-flows.md) | Flujos de datos (Mermaid) |

---

## Configuración

| Archivo | Contenido |
|---------|-----------|
| [`config/Config_TE.json`](config/Config_TE.json) | Configuración del TangibleEngine SDK |

---

## Estadisticas del Proyecto

| Métrica | Valor |
|---------|:-----:|
| Archivos de documentación | ~40 |
| Diagramas Archify | 14 (HTML interactivo) |
| Diagramas UML | 21 (Mermaid) |
| Documentos de requerimientos | 3 (stakeholders, user stories, requirements) |
| API endpoints documentados | 9 archivos |
| Manuales | 3 (usuario + dev + hardware) |
| Tests EditMode | **328** (21 suites: 9 Network + 6 Simulation + 3 Tangible + 3 UI) |
| Skills (opencode) | 20 |
| Agentes | 7 (main, architect, programmer, reviewer, tester, builder, documenter) |
| Líneas de código fuente | ~15,000 |
| Bugs activos | **1 (P2)** — Líneas de conexión invisibles en discos físicos |

---

## Enlaces Rapidos

- [README principal](../README.md) — Descripción general del proyecto
- [ROADMAP](../ROADMAP.md) — Backlog de tareas pendientes y completadas
- [AGENTS](../AGENTS.md) — Guía para agentes opencode
- [SPEC](../SPEC.md) — Especificaciones técnicas detalladas
