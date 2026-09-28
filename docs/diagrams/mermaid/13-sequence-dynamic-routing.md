# DS-05: Secuencia — Enrutamiento Dinámico RIP/OSPF/EIGRP

> **Propósito**: Mostrar cómo funciona el protocolo de enrutamiento dinámico: inicio, anuncios periódicos, convergencia y visualización.

```mermaid
sequenceDiagram
    participant Student as Estudiante
    participant Act as DynamicRoutingActivity
    participant DRP as DynamicRoutingProtocol
    participant R1 as Router1.RoutingTable
    participant R2 as Router2.RoutingTable
    participant Panel as UI Panel

    Student->>Act: Selecciona protocolo RIP
    Act->>Act: StartProtocol(panel) — valida ≥2 routers en topología
    Act->>DRP: AddComponent DynamicRoutingProtocol (RIP)
    DRP->>DRP: InitializeRouterStates()
    Note right of DRP: Cada router calcula su red conocida<br/>Router1: 192.168.1.0/24<br/>Router2: 192.168.2.0/24
    DRP->>DRP: StartCoroutine(RunProtocolLoop(routers))

    loop Cada 3 segundos (advertisementInterval)
        DRP->>DRP: advertisementCount++
        DRP->>Panel: OnProtocolLog("Anuncio #N...")

        par Router1 anuncia
            DRP->>R1: GetConnectedRouters()
            DRP->>R2: AddRipRoute("192.168.1.0", ...)
            Note right of R2: R2 aprende red de R1<br/>hops = R1.KnownNetworks.Count
        and Router2 anuncia
            DRP->>R2: GetConnectedRouters()
            DRP->>R1: AddRipRoute("192.168.2.0", ...)
            Note right of R1: R1 aprende red de R2
        end

        alt Nuevas rutas aprendidas
            DRP->>DRP: changed = true
            DRP->>Panel: OnProtocolLog("Nuevas rutas aprendidas")
        else Sin cambios
            DRP->>DRP: isConverged = true
            DRP->>DRP: OnConvergence()
            DRP->>Panel: OnProtocolLog("Convergencia alcanzada")
            Note right of Panel: Panel muestra mensaje verde
        end
    end

    Note over DRP: Máximo 10 iteraciones

    alt Límite alcanzado sin convergencia
        DRP->>DRP: StopProtocol()
        DRP->>Panel: OnProtocolLog("Limite de anuncios alcanzado")
    end
```

## Variante: OSPF

```mermaid
sequenceDiagram
    participant DRP as DynamicRoutingProtocol
    participant R1 as Router1.RoutingTable
    participant R2 as Router2.RoutingTable

    Note over DRP: Mismo loop, diferente cálculo de métrica

    DRP->>DRP: InitializeRouterStates() para OSPF
    DRP->>R1: AddOspfRoute("192.168.2.0", nextHop, iface, cost=Random(10,50))
    DRP->>R2: AddOspfRoute("192.168.1.0", nextHop, iface, cost=Random(10,50))
    Note right of R1: Métrica = costo del enlace (10-50)<br/>No es conteo de hops
```

## Comparación RIP vs OSPF

| Característica | RIP | OSPF |
|---------------|:---:|:----:|
| Métrica | Conteo de hops | Costo por enlace (10-50) |
| Máxima métrica | 15 saltos | Ilimitado |
| Tipo | Distance Vector | Link State (simplificado) |
| Frecuencia anuncio | Cada 3s | Cada 3s |
| Método de anuncio | Vecinos reciben redes conocidas | Costo aleatorio por enlace |
| Convergencia | Cuando no hay nuevas rutas | Misma lógica |

## Estados de DynamicRoutingProtocol

```
IDLE → RUNNING → CONVERGED → (opcional) STOPPED
          ↑__________| (si no converge en 10 iteraciones)
```

## Archivos Relacionados

| Archivo | Rol |
|---------|-----|
| `Assets/Scripts/Simulation/DynamicRoutingProtocol.cs` | Motor de protocolo (coroutine) |
| `Assets/Scripts/Simulation/DynamicRoutingActivity.cs` | UI de la actividad |
| `Assets/Scripts/Simulation/DynamicRoutingActivity.cs` | `StartProtocol(panel)` / `StopProtocol(panel)` |
| `Assets/Scripts/Simulation/RoutingProtocols.cs` | `RoutingSimulator` estático |
| `Assets/Scripts/Network/RoutingTable.cs` | `AddRipRoute()` / `AddOspfRoute()` |
