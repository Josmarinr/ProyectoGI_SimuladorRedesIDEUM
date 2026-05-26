---
name: scoring-system
description: >-
  Use when working with the scoring system in SimuladorRedes IDEUM.
  Covers ScoringSession API, point calculation (tasks, faults, routes,
  pings, penalties), grade lookup table, session summary. Use for any
  task involving evaluation, grading, or score display.
---

# Skill: Sistema de Puntajes

## ScoringSystem.cs

Ubicación: `Assets/Scripts/Simulation/ScoringSystem.cs`

### Uso
```csharp
// Iniciar sesión de evaluación
ScoringSystem.Instance.StartSession("Construir Topologia");

// Agregar puntos por tareas
ScoringSystem.Instance.AddTaskCompleted("Configurar router");
ScoringSystem.Instance.AddFaultFound("IP incorrecta");
ScoringSystem.Instance.AddRouteConfigured("192.168.1.0/24 via 10.0.0.1");
ScoringSystem.Instance.AddPingSuccess("PC1", "PC2");

// Penalizaciones
ScoringSystem.Instance.AddPenalty("Tiempo excedido", -10);

// Obtener resultados
int score = ScoringSystem.Instance.GetCurrentScore();
int grade = ScoringSystem.Instance.GetGrade(); // 1-5
string summary = ScoringSystem.Instance.GetSessionSummary();

// Terminar sesión
ScoringSystem.Instance.EndSession();
```

### Configuración
```csharp
[Header("Configuracion")]
public int basePoints = 100;        // Puntos base por tarea
public int timeBonusMax = 50;       // Bono por tiempo (< 2 min)
public int faultPenalty = -20;      // Penalización por fallo
public int routeBonus = 15;         // Puntos por ruta
public int pingSuccessBonus = 10;   // Puntos por ping
```

### Cálculo de Nota
| Puntuación | Nota |
|------------|------|
| >= 500 | 5.0 |
| >= 400 | 4.0 |
| >= 300 | 3.0 |
| >= 200 | 2.0 |
| < 200 | 1.0 |
