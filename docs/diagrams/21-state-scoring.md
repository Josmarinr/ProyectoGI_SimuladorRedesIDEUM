# DE-03: Diagrama de Estados — Sesión de Puntaje

> **Propósito**: Mostrar el ciclo de vida de una sesión de puntuación: desde que se inicia una actividad hasta que se calcula la nota final.

```mermaid
stateDiagram-v2
    [*] --> NotStarted: App inicia
    note right of NotStarted: ScoringSystem existe como singleton<br/>pero sin sesión activa

    NotStarted --> InProgress: StartSession(activityName)
    note right of InProgress: Se registra:<br/>- activityName<br/>- startTime (para bono de tiempo)

    InProgress --> Scoring: Eventos del estudiante
    note right of Scoring: Cada acción del estudiante<br/>dispara un Add*()

    Scoring --> InProgress: Punto acumulado<br/>siguiente acción

    InProgress --> TimeBonusCheck: Pasa el tiempo
    TimeBonusCheck --> InProgress: Bono calculado<br/>según elapsed time

    InProgress --> Ended: EndSession()
    note right of Ended: Se calcula:<br/>- Puntaje total<br/>- Nota (1-5)

    Ended --> [*]: GetSessionSummary()<br/>GetGrade()

    Scoring --> Penalized: AddPenalty()
    Penalized --> InProgress: Penalización aplicada
```

## Sistema de Puntajes

| Evento | Puntos | Método |
|--------|:------:|--------|
| Tarea completada | 100 + bono tiempo | `AddTaskCompleted()` |
| Bono < 2 min | +50 | `CalculateTimeBonus()` |
| Bono 2-5 min | +25 | `CalculateTimeBonus()` |
| Bono > 5 min | 0 | `CalculateTimeBonus()` |
| Fallo encontrado | 120 | `AddFaultFound()` |
| Ruta configurada | +15 | `AddRouteConfigured()` |
| Ping exitoso | +10 | `AddPingSuccess()` |
| Penalización | -N | `AddPenalty(reason, N)` |

## Escala de Notas

| Puntaje Mínimo | Nota | Descripción |
|:--------------:|:----:|-------------|
| 500 | 5 | ⭐ Excelente |
| 400 | 4 | ✅ Bueno |
| 300 | 3 | 📘 Regular |
| 200 | 2 | ⚠️ Insuficiente |
| < 200 | 1 | ❌ Reprobado |

## Ejemplo de Sesión

```
StartSession("Mejor Ruta")
  → AddTaskCompleted() → 100 pts + bono tiempo
  → AddPingSuccess()   → +10 pts
  → AddPingSuccess()   → +10 pts
  → AddTaskCompleted() → 100 pts
  → EndSession()
  → Total: 220 pts → Nota: 2 (Insuficiente)
```

## Archivos Relacionados

- `Assets/Scripts/Simulation/ScoringSystem.cs` — Lógica de puntajes
- `Assets/Scripts/UI/DevicePanelController.cs` — `UpdateScoreDisplay()`
- `Assets/Scripts/UI/UIPanelFactory.cs` — `CreateScorePanel()`
