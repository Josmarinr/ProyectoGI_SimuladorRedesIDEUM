# Diagramas Archify — SimuladorRedes IDEUM

> Diagramas interactivos generados con [Archify](https://github.com/tt-a1i/archify)
> **Preset visual**: Blueprint | **Tema por defecto**: Light

---

## Diagramas Disponibles

### Architecture (Arquitectura)

| Diagrama | Archivo JSON | HTML Renderizado | Descripción |
|----------|--------------|------------------|-------------|
| Casos de Uso | [use-cases.architecture.json](architecture/use-cases.architecture.json) | [use-cases.architecture.html](rendered/use-cases.architecture.html) | Casos de uso principales del sistema |
| Despliegue | [deployment.architecture.json](architecture/deployment.architecture.json) | [deployment.architecture.html](rendered/deployment.architecture.html) | Despliegue en mesa IDEUM |
| Componentes | [components.architecture.json](architecture/components.architecture.json) | [components.architecture.html](rendered/components.architecture.html) | Componentes del sistema |
| Clases Network | [class-network.architecture.json](architecture/class-network.architecture.json) | [class-network.architecture.html](rendered/class-network.architecture.html) | TopologyManager, NetworkNode, RoutingTable |
| Clases UI | [class-ui.architecture.json](architecture/class-ui.architecture.json) | [class-ui.architecture.html](rendered/class-ui.architecture.html) | UIPanelFactory, MenuNavigator, UIComponents |

### Sequence (Secuencias)

| Diagrama | Archivo JSON | HTML Renderizado | Descripción |
|----------|--------------|------------------|-------------|
| Routing Config | [routing-config.sequence.json](sequence/routing-config.sequence.json) | [routing-config.sequence.html](rendered/routing-config.sequence.html) | Configuración de ruta estática con discos virtuales |

### Workflow (Flujos de Trabajo)

| Diagrama | Archivo JSON | HTML Renderizado | Descripción |
|----------|--------------|------------------|-------------|
| Actividad Topología | [activity-topology.workflow.json](workflow/activity-topology.workflow.json) | [activity-topology.workflow.html](rendered/activity-topology.workflow.html) | Construye la Topología |
| Actividad Fallos | [activity-faults.workflow.json](workflow/activity-faults.workflow.json) | [activity-faults.workflow.html](rendered/activity-faults.workflow.html) | Encuentra el Fallo |
| Actividad Dinámico | [activity-dynamic.workflow.json](workflow/activity-dynamic.workflow.json) | [activity-dynamic.workflow.html](rendered/activity-dynamic.workflow.html) | Enrutamiento Dinámico |

---

## Cómo Usar

### Abrir un diagrama HTML

```bash
# macOS
open docs/diagrams/archify/rendered/use-cases.architecture.html

# Linux
xdg-open docs/diagrams/archify/rendered/use-cases.architecture.html

# Windows
start docs/diagrams/archify/rendered/use-cases.architecture.html
```

### Atajos de Teclado (Viewer)

| Tecla | Acción |
|-------|--------|
| `?` | Abrir Diagram Guide |
| `/` | Buscar nodo |
| `R` | Trazar ruta |
| `L` | Comparar roles |
| `M` | Overview radar |
| `P` | Presentación |
| `S` | Cambiar preset visual |
| `T` | Alternar tema (dark/light) |
| `E` | Exportar |
| `+` / `-` / `0` | Zoom / Reset |

### Regenerar un diagrama

```bash
# Validar
node .opencode/skills/archify/bin/archify.mjs validate architecture docs/diagrams/archify/architecture/use-cases.architecture.json --quality showcase --json

# Renderizar
node .opencode/skills/archify/bin/archify.mjs deliver architecture docs/diagrams/archify/architecture/use-cases.architecture.json docs/diagrams/archify/rendered/use-cases.architecture.html --quality showcase --json
```

---

## Estructura

```
docs/diagrams/archify/
├── architecture/           # Diagramas de arquitectura (5 JSONs)
│   ├── use-cases.architecture.json
│   ├── deployment.architecture.json
│   ├── components.architecture.json
│   ├── class-network.architecture.json
│   └── class-ui.architecture.json
├── sequence/               # Diagramas de secuencia (1 JSON)
│   └── routing-config.sequence.json
├── workflow/               # Diagramas de flujo de trabajo (3 JSONs)
│   ├── activity-topology.workflow.json
│   ├── activity-faults.workflow.json
│   └── activity-dynamic.workflow.json
├── rendered/               # HTML generados (9 archivos)
│   ├── use-cases.architecture.html
│   ├── deployment.architecture.html
│   ├── components.architecture.html
│   ├── class-network.architecture.html
│   ├── class-ui.architecture.html
│   ├── routing-config.sequence.html
│   ├── activity-topology.workflow.html
│   ├── activity-faults.workflow.html
│   └── activity-dynamic.workflow.html
└── README.md               # Este archivo
```

---

## Relación con Diagramas Existentes (Mermaid)

| Archivo Mermaid | Equivalente Archify | Mejora |
|-----------------|---------------------|--------|
| `01-use-case.md` | `use-cases.architecture.html` | Interactivo, zoom, búsqueda |
| `07-component-architecture.md` | `components.architecture.html` | Visualización de capas |
| `08-deployment.md` | `deployment.architecture.html` | Despliegue interactivo |
| `02-class-network.md` | `class-network.architecture.html` | Clases con navegación |
| `05-class-ui.md` | `class-ui.architecture.html` | UI con zoom |
| `10-sequence-routing-config.md` | `routing-config.sequence.html` | Secuencia animada |
| `14-activity-topology.md` | `activity-topology.workflow.html` | Workflow interactivo |
| `15-activity-faults.md` | `activity-faults.workflow.html` | Workflow con branches |
| `18-activity-dynamic.md` | `activity-dynamic.workflow.html` | Routing dinámico |

---

## Preset Visual

Se usa **Blueprint** por defecto para todos los diagramas, ya que encaja con el tema de ingeniería de redes.

Para cambiar el preset, modifica `visual_preset` en el JSON:
- `classic` — Estilo neutro
- `signal-flow` — Artístico
- `blueprint` — Técnico/ingeniería (recomendado)
- `editorial` — Editorial/profesional
