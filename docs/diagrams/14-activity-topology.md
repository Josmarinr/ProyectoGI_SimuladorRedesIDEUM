# DA-01: Diagrama de Actividad — Construir Topología

> **Propósito**: Mostrar el flujo de la Actividad 0 donde el estudiante construye una topología y el sistema la detecta automáticamente.

```mermaid
graph TB
    A([Inicio]) --> B[Estudiante coloca discos<br/>en la mesa IDEUM]
    B --> C{Sistema detecta<br/>disco físico?}
    C -->|Sí| D[DiscEventHandler procesa<br/>y crea NetworkNode]
    C -->|No| E[DebugDiscSimulator<br/>simula disco por teclado]
    D --> F[NodeVisualizer muestra<br/>nodo en pantalla]
    E --> F
    F --> G{Auto-conectar<br/>a <300px?}
    G -->|Sí| H[Crear enlace automático<br/>entre nodos cercanos]
    G -->|No| I[Estudiante usa modo<br/>CONECTAR manual]
    H --> J[BuildTopologyActivity<br/>detecta tipo de topología]
    I --> J
    J --> K[Analiza grado de nodos<br/>y número de enlaces]
    K --> L{¿Qué topología es?}

    L -->|1 nodo| M[⭐ Estrella]
    L -->|Enlaces = n*(n-1)/2| N[🔁 Malla]
    L -->|Enlaces = nodos| O[⭕ Anillo]
    L -->|1 switch + routers| M
    L -->|2+ routers + switches| P[🌳 Árbol]
    L -->|Default| Q[📏 Bus]

    M --> R[Actualiza TopologyInfoPanel<br/>con tipo detectado]
    N --> R
    O --> R
    P --> R
    Q --> R

    R --> S{¿Estudiante<br/>quiere continuar?}
    S -->|Sí, más nodos| B
    S -->|No, pasar a otra actividad| T([Menú Actividades])
    S -->|VOLVER| U([Menú Principal])
```

## Lógica de Detección

```
Nodos = 0          → "Sin topología"
Nodos = 1          → "Estrella" (nodo único)
Enlaces = n*(n-1)/2 → "Malla" (todos conectados a todos)
Enlaces = nodos     → "Anillo" (cada nodo al siguiente)
1 switch + routers  → "Estrella" (switch central)
2+ routers + sw     → "Árbol" (jerarquía)
Default             → "Bus" (línea lineal)
```

## Archivos Relacionados

- `Assets/Scripts/Simulation/BuildTopologyActivity.cs` — Lógica de detección
- `Assets/Scripts/UI/UIPanelFactory.cs` — `CreateBuildTopologyInfoPanel()`
- `Assets/Scripts/UI/TopologyVisualizer.cs` — Visualización de enlaces
