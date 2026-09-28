# DE-02: Diagrama de Estados — DynamicRoutingProtocol

> **Propósito**: Mostrar los estados del motor de enrutamiento dinámico (RIP/OSPF/EIGRP) durante su ciclo de vida: desde la creación hasta la convergencia o detención.

```mermaid
stateDiagram-v2
    [*] --> Idle: DynamicRoutingProtocol<br/>creado por ActivityLoader

    Idle --> Initializing: StartProtocol()
    note right of Idle: Protocolo (RIP/OSPF/EIGRP) ya asignado

    Initializing --> Running: RouterAdvertState<br/>inicializado para cada router
    note right of Initializing: Cada router calcula su red conocida<br/>a partir de IP + máscara

    Running --> Advertising: advertisementInterval<br/>(3 segundos) transcurrido
    note right of Running: Coroutine RunProtocolLoop<br/>activa cada N segundos

    Advertising --> Checking: Anuncios enviados a todos<br/>los routers vecinos
    note right of Advertising: RIP: AddRipRoute<br/>OSPF: AddOspfRoute

    Checking --> Running: Hubo cambios (changed = true)<br/>iteration++
    note right of Checking: Se aprendieron nuevas rutas<br/>en este ciclo

    Checking --> Converged: Sin cambios (changed = false)<br/>→ OnConvergence disparado

    Converged --> [*] : StopProtocol()<br/>o cambio de actividad

    Running --> MaxIterations: iteration >= 10<br/>sin converger

    MaxIterations --> [*]: StopProtocol()<br/>"Límite alcanzado"

    Running --> Stopped: StopProtocol()<br/>(manual o cambio de actividad)
    MaxIterations --> Stopped
    Converged --> Stopped

    Stopped --> [*]: Destruido por<br/>ActivityLoader
```

## Eventos del Protocolo

| Evento | Disparador | Suscriptores |
|--------|-----------|--------------|
| `OnProtocolLog(string)` | Cada anuncio, cada cambio | Panel UI (actualiza texto de estado) |
| `OnConvergence()` | Cuando `changed == false` en un ciclo completo | Panel UI (muestra mensaje verde) |

## Constantes

| Parámetro | Valor | Descripción |
|-----------|:-----:|-------------|
| `advertisementInterval` | 3s | Tiempo entre anuncios |
| `maxIterations` | 10 | Máximo de ciclos antes de timeout |
| RIP métrica base | 1 | Hop count inicial |
| OSPF costo mínimo | 10 | `Random.Range(10, 50)` |

## Archivos Relacionados

- `Assets/Scripts/Simulation/DynamicRoutingProtocol.cs` — Motor del protocolo
- `Assets/Scripts/Simulation/ActivityLoader.cs` — Creación y ciclo de vida
- `Assets/Scripts/Simulation/DynamicRoutingActivity.cs` — UI asociada
