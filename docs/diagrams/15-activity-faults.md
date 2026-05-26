# DA-02: Diagrama de Actividad — Encuentra el Fallo

> **Propósito**: Mostrar el flujo de la Actividad 1 donde el sistema genera un fallo aleatorio y el estudiante debe diagnosticarlo y resolverlo.

```mermaid
graph TB
    A([Inicio Actividad]) --> B[Sistema elige fallo aleatorio]
    B --> C{Tipo de fallo}

    C -->|1. Cable Desconectado| D[Desactiva enlace<br/>link.SetFault"broken"]
    C -->|2. IP Incorrecta| E[Cambia IP del router<br/>a 10.0.0.1/24]
    C -->|3. Máscara Incorrecta| F[Cambia máscara<br/>a 255.0.0.0]
    C -->|4. Interfaz Down| G[Activa<br/>node.IsAdminDown = true]
    C -->|5. Gateway Faltante| H[Limpia IP del PC<br/>debe re-configurar]

    D --> I[Panel muestra pista<br/>y descripción del fallo]
    E --> I
    F --> I
    G --> I
    H --> I

    I --> J[Estudiante diagnostica<br/>e intenta resolver]

    J --> K[Presiona botón<br/>RESOLVER]

    K --> L[FindFaultActivity<br/>OnSolveClicked]
    L --> M[Llama FixFault según tipo]

    M --> N{Tipo de fallo}
    N -->|Cable| O[link.ClearFault en todos]
    N -->|IP/Máscara| P[Reset IP a 192.168.1.1/24]
    N -->|Interfaz Down| Q[node.IsAdminDown = false]
    N -->|Gateway| R[Reset PC a 192.168.1.10/24]

    O --> S[ValidateSolution<br/>verifica conectividad]
    P --> S
    Q --> S
    R --> S

    S --> T{¿Solución<br/>correcta?}
    T -->|Sí| U[✅ Fallo resuelto<br/>Suma puntos]
    T -->|No| V[❌ Fallo persiste<br/>Intenta de nuevo]

    U --> W[GenerateNewScenario<br/>o volver al menú]
    V --> J
```

## Tipos de Fallo

| Tipo | Cómo se aplica | Cómo se repara |
|------|----------------|----------------|
| Cable Desconectado | `link.SetFault("broken")` | `link.ClearFault()` |
| IP Incorrecta | Router → `10.0.0.1/24` | Router → `192.168.1.1/24` |
| Máscara Incorrecta | Router → `255.0.0.0` | Router → `255.255.255.0` |
| Interfaz Down | `node.IsAdminDown = true` | `node.IsAdminDown = false` |
| Gateway Faltante | PC sin IP | PC → `192.168.1.10/24` |

## Archivos Relacionados

- `Assets/Scripts/Simulation/FindFaultActivity.cs` — Lógica de fallos
- `Assets/Scripts/UI/UIPanelFactory.cs` — `CreateFindFaultPanel()`
- `Assets/Scripts/Network/NetworkNode.cs` — `ConfiguredFault`, `IsAdminDown`
- `Assets/Scripts/Network/NetworkLink.cs` — `SetFault()`, `ClearFault()`
