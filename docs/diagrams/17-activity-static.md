# DA-04: Diagrama de Actividad — Enrutamiento Estático

> **Propósito**: Mostrar el flujo de la Actividad 4 donde el estudiante configura rutas estáticas manualmente en los routers.

```mermaid
graph TB
    A([Inicio Actividad]) --> B[Estudiante coloca al menos 2 routers<br/>y configura IPs en cada interfaz]

    B --> C{Abre panel de<br/>Enrutamiento Estático}

    C --> D[StaticRoutingActivity muestra:<br/>tabla de rutas actual del router]

    D --> E[Estudiante elige:<br/>AÑADIR RUTA ESTÁTICA]

    E --> F[Completa campos:<br/>- Red destino<br/>- Máscara de subred<br/>- Next Hop<br/>- Interfaz de salida]

    F --> G[Presiona AGREGAR RUTA]

    G --> H[StaticRoutingActivity.AddRoute<br/>→ router.RoutingTable.AddStaticRoute]

    H --> I[Verificación visual:<br/>ruta aparece en tabla de rutas]

    I --> J{¿Configurar<br/>más rutas?}
    J -->|Sí| E
    J -->|No| K[Presiona PROBAR CONECTIVIDAD]

    K --> L[Busca PC origen y PC destino<br/>en la topología]
    L --> M[Ejecuta TopologyManager.CheckConnectivity]

    M --> N{¿Conectividad<br/>exitosa?}
    N -->|Sí| O[✅ Ping exitoso<br/>+10 puntos por ruta]
    N -->|No| P[❌ Sin conectividad<br/>revisar configuración]

    O --> Q{¿Rutas estáticas<br/>completas?}
    Q -->|Sí| R([🏁 Actividad completada])
    Q -->|No| J

    P --> J
```

## Flujo Alternativo: Discos de Routing (IDs 7-14)

Los discos de routing pueden usarse en lugar del panel táctil:

```
1. Colocar disco RedDestino (ID 7) sobre un router
2. Colocar disco Mascara (ID 13) sobre el mismo router
3. Colocar disco ProximoSalto (ID 14) sobre el mismo router
4. Colocar disco InterfazSalida (ID 9) sobre el mismo router
→ Ruta completa automáticamente (RouteBuilderState.IsComplete → ApplyToRouter)
```

## Archivos Relacionados

- `Assets/Scripts/Simulation/StaticRoutingActivity.cs` — UI y lógica de rutas estáticas
- `Assets/Scripts/Network/RoutingTable.cs` — `AddStaticRoute()` con campos completos
- `Assets/Scripts/Tangible/RouteBuilderState.cs` — Construcción de ruta con discos
- `Assets/Scripts/Tangible/DiscEventHandler.cs` — `HandleRoutingConfigDisc()`
