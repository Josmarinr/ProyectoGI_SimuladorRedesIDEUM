# DA-05: Diagrama de Actividad — Enrutamiento Dinámico

> **Propósito**: Mostrar el flujo de la Actividad 5 donde el estudiante selecciona un protocolo (RIP/OSPF) y observa la convergencia automática de las tablas de enrutamiento.

```mermaid
graph TB
    A([Inicio Actividad]) --> B[Estudiante coloca ≥2 routers<br/>con IPs configuradas]

    B --> C{Elige protocolo}
    C -->|RIP| D[Selecciona RIP<br/>Conteo de hops]
    C -->|OSPF| E[Selecciona OSPF<br/>Costo por enlace]

    D --> F[ActivityLoader.StartDynamicProtocol]
    E --> F

    F --> G[Verifica ≥2 routers<br/>con IPs válidas]
    G --> H{¿Válido?}
    H -->|No| I[❌ Mensaje: "Se necesitan<br/>al menos 2 routers"]
    H -->|Sí| J[Inicializa RouterAdvertState<br/>para cada router]

    J --> K[DynamicRoutingProtocol<br/>inicia coroutine RunProtocolLoop]

    K --> L[Espera advertisementInterval<br/>3 segundos]

    L --> M[Por cada router:<br/>anuncia sus redes conocidas<br/>a routers vecinos]

    M --> N{Router vecino<br/>aprende nueva red?}
    N -->|Sí| O[Agrega ruta a RoutingTable<br/>vía AddRipRoute/AddOspfRoute]
    N -->|No| P[Sin cambios en este ciclo]

    O --> Q{Hubo cambios<br/>en este ciclo?}
    P --> Q
    Q -->|Sí| L
    Q -->|No| R[Marca convergencia<br/>OnConvergence disparado]

    R --> S[✅ Panel muestra:<br/>"Convergencia alcanzada"<br/>Rutas compartidas: N]

    S --> T[Estudiante puede<br/>PROBAR CONECTIVIDAD<br/>con ping entre PCs]

    T --> U([Menú Actividades])

    L --> V{Iteraciones<br/>> 10?}
    V -->|Sí| W[⚠️ "Límite alcanzado<br/>sin convergencia total"]
    W --> T
```

## Detalle del Loop de Anuncios

```
RunProtocolLoop(routers):
  iteration = 0
  while iteration < 10 AND !isConverged:
    changed = false
    for each routerState in routerStates:
      for each neighbor in GetConnectedRouters(routerState.Router):
        for each knownNetwork in routerState.KnownNetworks:
          if !neighbor.KnownNetworks.Contains(knownNetwork):
            neighbor.KnownNetworks.Add(knownNetwork)
            if protocol == RIP:
              router.RoutingTable.AddRipRoute(network, nextHop, iface, hops)
            else: // OSPF
              router.RoutingTable.AddOspfRoute(network, nextHop, iface, cost)
            changed = true
    iteration++
    if !changed:
      isConverged = true
      OnConvergence()
    yield return WaitForSeconds(advertisementInterval)
```

## Archivos Relacionados

- `Assets/Scripts/Simulation/DynamicRoutingProtocol.cs` — Motor de protocolo
- `Assets/Scripts/Simulation/DynamicRoutingActivity.cs` — UI de actividad
- `Assets/Scripts/Simulation/ActivityLoader.cs` — Orquestación del protocolo
- `Assets/Scripts/Network/RoutingTable.cs` — `AddRipRoute()`, `AddOspfRoute()`
