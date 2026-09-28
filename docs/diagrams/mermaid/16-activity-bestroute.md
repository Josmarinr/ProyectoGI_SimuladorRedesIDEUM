# DA-03: Diagrama de Actividad — Simulación de Mejor Ruta

> **Propósito**: Mostrar el flujo de la Actividad 3 (Simulación de Mejor Ruta) donde el estudiante debe seleccionar la mejor ruta hacia un destino entre varias opciones.

```mermaid
graph TB
    A([Inicio Actividad]) --> B[Cargar escenario 1 de 4]

    B --> C[BestRouteActivity muestra:]
    C --> C1[IP de destino: 10.1.1.100]
    C --> C2[3-4 rutas candidatas<br/>con red/máscara/nextHop/metrica]
    C --> C3[Estudiante hace clic<br/>en la ruta que considera mejor]

    C3 --> D[OnRouteSelected(index)]

    D --> E[Sistema valida con<br/>RoutingTable.FindBestRoute]

    E --> F{Criterios:<br/>1. Longest prefix match<br/>2. Lowest metric}

    F --> G[Compara respuesta<br/>del estudiante con la correcta]

    G --> H{¿Es correcta?}

    H -->|Sí| I[+1 punto<br/>Muestra explicación]
    H -->|No| J[Muestra respuesta correcta<br/>con explicación detallada]

    I --> K[Siguiente escenario<br/>o resultados finales]
    J --> K

    K --> L{¿Quedan más<br/>escenarios?}
    L -->|Sí| B
    L -->|No| M[Muestra puntaje final<br/>X/4 correctas]

    M --> N([Menú Actividades])
```

## Escenarios

| # | Destino | Rutas | Ganadora |
|:-:|:-------:|-------|:--------:|
| 1 | `10.1.1.100` | /8 RIP 5, /16 OSPF 3, **/24 Static 1**, /0 Static 1 | **/24** (más específica) |
| 2 | `172.16.5.50` | /16 RIP 3, **/24 OSPF 2**, /0 Static 1 | **/24** (más específica) |
| 3 | `192.168.1.10` | **/24 Static 1**, /16 RIP 2, /0 Static 1 | **/24** (misma métrica, más específica) |
| 4 | `10.20.30.1` | /8 RIP 5, /16 OSPF 3, **/24 Static 2**, /0 Static 1 | **/24** (más específica) |

## Lógica de Selección (`FindBestRoute`)

```
1. Filtrar rutas que contienen la IP destino (longest prefix match)
2. Si múltiples coinciden, elegir la de menor métrica
3. Si empate en métrica, elegir la ruta más específica (mayor prefix)
4. Si no hay match, retornar null (o default route si existe)
```

## Archivos Relacionados

- `Assets/Scripts/Simulation/BestRouteActivity.cs` — Lógica y escenarios
- `Assets/Scripts/Network/RoutingTable.cs` — `FindBestRoute()` con longest prefix + metric
