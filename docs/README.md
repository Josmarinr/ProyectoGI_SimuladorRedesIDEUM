# Documentación — SimuladorRedes IDEUM

> Indice completo de documentacion tecnica, manuales y diagramas del proyecto.

---

## Diagramas UML

### Diagramas de Casos de Uso (5 sub-diagramas)
| Archivo | Sub-diagramas |
|---------|--------------|
| [`diagrams/01-use-case.md`](diagrams/01-use-case.md) | 01a: Gestión Simulación · 01b: Actividades · 01c: Enrutamiento · 01d: Config. Avanzada · 01e: Evaluación |

### Diagramas de Clases (13 sub-diagramas)
| Archivo | Sub-diagramas |
|---------|--------------|
| [`diagrams/02-class-network.md`](diagrams/02-class-network.md) | 02a: Núcleo Red · 02b: Tablas/ARP/IP · 02c: VLAN/ACL/NAT |
| [`diagrams/03-class-tangible.md`](diagrams/03-class-tangible.md) | Tangible Layer (managers, bridge, debug) |
| [`diagrams/04-class-simulation.md`](diagrams/04-class-simulation.md) | 04a: Núcleo Simulación · 04b: Actividades · 04c: Protocolo Dinámico · 04d: Puntajes |
| [`diagrams/05-class-ui.md`](diagrams/05-class-ui.md) | 05a: Factories · 05b: Controladores · 05c: Visualización · 05d: Navegación |
| [`diagrams/06-packages.md`](diagrams/06-packages.md) | Paquetes/Namespaces |
| [`diagrams/07-component-architecture.md`](diagrams/07-component-architecture.md) | Componentes y capas |
| [`diagrams/08-deployment.md`](diagrams/08-deployment.md) | Despliegue IDEUM |

### Diagramas de Secuencia
| Archivo | Descripción |
|---------|-------------|
| [`diagrams/09-sequence-disc-placement.md`](diagrams/09-sequence-disc-placement.md) | Colocar disco físico → nodo visible |
| [`diagrams/10-sequence-routing-config.md`](diagrams/10-sequence-routing-config.md) | Configurar ruta con discos 7-18 |
| [`diagrams/11-sequence-connectivity.md`](diagrams/11-sequence-connectivity.md) | CheckConnectivity (BFS + VLAN + ACL + NAT) |
| [`diagrams/12-sequence-menu-navigation.md`](diagrams/12-sequence-menu-navigation.md) | Navegación de menús (ciclo completo) |
| [`diagrams/13-sequence-dynamic-routing.md`](diagrams/13-sequence-dynamic-routing.md) | Enrutamiento dinámico RIP/OSPF/EIGRP |

### Diagramas de Actividad
| Archivo | Descripción |
|---------|-------------|
| [`diagrams/14-activity-topology.md`](diagrams/14-activity-topology.md) | Actividad: Construye la Topología |
| [`diagrams/15-activity-faults.md`](diagrams/15-activity-faults.md) | Actividad: Encuentra el Fallo |
| [`diagrams/16-activity-bestroute.md`](diagrams/16-activity-bestroute.md) | Actividad: Simulación de Mejor Ruta |
| [`diagrams/17-activity-static.md`](diagrams/17-activity-static.md) | Actividad: Enrutamiento Estático Tangible |
| [`diagrams/18-activity-dynamic.md`](diagrams/18-activity-dynamic.md) | Actividad: Protocolo de Enrutamiento Dinámico Tangible |

### Diagramas de Estado
| Archivo | Descripción |
|---------|-------------|
| [`diagrams/19-state-routebuilder.md`](diagrams/19-state-routebuilder.md) | RouteBuilderState — ciclo de vida |
| [`diagrams/20-state-protocol.md`](diagrams/20-state-protocol.md) | DynamicRoutingProtocol — estados |
| [`diagrams/21-state-scoring.md`](diagrams/21-state-scoring.md) | ScoringSystem — sesión de puntaje |

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

---

## Estadisticas del Proyecto

| Métrica | Valor |
|---------|:-----:|
| Archivos de documentación | 33 |
| Diagramas UML | 21 (Mermaid) |
| API endpoints documentados | 9 archivos |
| Manuales | 2 (usuario + dev) |
| Tests EditMode | **108** (9 suites: 4 Network + 4 Simulation + 1 Tangible) |
| Skills (opencode) | 18 |
| Agentes | 6 (main, architect, programmer, reviewer, tester, builder) |
| Líneas de código fuente | ~11,847 |
| Líneas de documentación | ~4,200 |
| SceneSetup | ~480 líneas (refactorizado desde ~4,246) |
| Warnings de compilación eliminados | ~188 |
| Bugs corregidos (auditoría null safety) | 6 |
| Build generado | `Build/SimuladorRedes.exe` (Windows x86_64) |

---

## Enlaces Rapidos

- [README principal](../README.md) — Descripción general del proyecto
- [ROADMAP](../ROADMAP.md) — Backlog de tareas pendientes y completadas
- [AGENTS](../AGENTS.md) — Guía para agentes opencode
- [SPEC](../SPEC.md) — Especificaciones técnicas detalladas
