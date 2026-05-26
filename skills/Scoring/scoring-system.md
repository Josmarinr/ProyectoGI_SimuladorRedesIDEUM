# Skill: Sistema de Puntajes

## ScoringSystem.cs

Ubicación: `Assets/Scripts/Simulation/ScoringSystem.cs`

### Uso
```csharp
// Iniciar sesión de evaluación
ScoringSystem.Instance.StartSession("Construye la Topología");

// Agregar puntos por tareas
ScoringSystem.Instance.AddTaskCompleted("Configurar router");
ScoringSystem.Instance.AddTaskCompleted("Conectar PCs");

// Puntos por fallos encontrados
ScoringSystem.Instance.AddFaultFound("IP incorrecta");

// Puntos por rutas configuradas
ScoringSystem.Instance.AddRouteConfigured("192.168.1.0/24 via 10.0.0.1");

// Puntos por ping exitoso
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

### Cálculo de Puntuación
- **Tarea completada**: 100 pts + bono de tiempo
- **Bono de tiempo**: +50 si < 2 min, +25 si < 5 min
- **Fallo encontrado**: +20 pts
- **Ruta configurada**: +15 pts
- **Ping exitoso**: +10 pts

### Cálculo de Nota
| Puntuación | Nota |
|------------|------|
| >= 500 | 5.0 |
| >= 400 | 4.0 |
| >= 300 | 3.0 |
| >= 200 | 2.0 |
| < 200 | 1.0 |

### Resumen de Sesión
```csharp
string summary = scoring.GetSessionSummary();
// Retorna:
// Puntuacion Final: 350
// Tareas: 3
// Fallos: 2
// Rutas: 5
// Pings: 4
// Tiempo: 3m 45s
```

### Eventos en Escenarios
```csharp
private void LoadScenario(int scenarioIndex)
{
    var scenario = PredefinedScenarios.Instance.GetScenario(scenarioIndex);
    ScoringSystem.Instance.StartSession(scenario.name);
}

private void OnPingSuccess(string source, string dest)
{
    ScoringSystem.Instance.AddPingSuccess(source, dest);
}

private void OnTaskComplete(string description)
{
    ScoringSystem.Instance.AddTaskCompleted(description);
}
```