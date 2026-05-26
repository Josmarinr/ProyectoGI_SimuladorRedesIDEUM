# API — ScoringSystem

> **Namespace**: `SimRedes.Simulation` · **Archivo**: `Assets/Scripts/Simulation/ScoringSystem.cs`
>
> Singleton que gestiona la puntuación de las actividades académicas.

---

## Propiedades

| Propiedad | Tipo | Default | Descripción |
|-----------|------|:-------:|-------------|
| `Instance` | `ScoringSystem` (static) | - | Acceso singleton |
| `basePoints` | `int` | 100 | Puntos por tarea completada |
| `timeBonusMax` | `int` | 50 | Bono máximo por tiempo |
| `faultPenalty` | `int` | -20 | Penalización por fallo |
| `routeBonus` | `int` | 15 | Bonus por ruta configurada |
| `pingSuccessBonus` | `int` | 10 | Bonus por ping exitoso |

## Métodos

### Ciclo de Sesión

```csharp
public void StartSession(string activityName)
```
Inicia una nueva sesión de puntuación. Resetea todos los contadores y registra el tiempo de inicio.

```csharp
public void EndSession()
```
Finaliza la sesión y calcula el puntaje final.

### Acumulación de Puntos

```csharp
public void AddTaskCompleted()
```
Suma `basePoints` + bono de tiempo calculado.

```csharp
public void AddFaultFound()
```
Suma `basePoints` + `Math.Abs(faultPenalty)` (120 pts).

```csharp
public void AddRouteConfigured()
```
Suma `routeBonus` (15 pts).

```csharp
public void AddPingSuccess()
```
Suma `pingSuccessBonus` (10 pts).

```csharp
public void AddPenalty(string reason, int points)
```
Resta puntos por penalización (valor negativo).

### Consultas

```csharp
public int GetCurrentScore()
```
Retorna el puntaje acumulado hasta el momento.

```csharp
public string GetSessionSummary()
```
Retorna resumen formateado de la sesión.

```csharp
public int GetGrade()
```
Retorna la nota calculada (1-5) según la escala:

| Puntaje Mínimo | Nota |
|:--------------:|:----:|
| 500 | 5 (Excelente) |
| 400 | 4 (Bueno) |
| 300 | 3 (Regular) |
| 200 | 2 (Insuficiente) |
| < 200 | 1 (Reprobado) |

## Cálculo de Bono de Tiempo

```csharp
int CalculateTimeBonus() {
    var elapsed = DateTime.Now - sessionStartTime;
    if (elapsed.TotalMinutes < 2) return timeBonusMax;       // +50
    if (elapsed.TotalMinutes < 5) return timeBonusMax / 2;   // +25
    return 0;
}
```

## Código de Ejemplo

```csharp
// Iniciar sesión
ScoringSystem.Instance.StartSession("Construir Topología");

// El estudiante completa tareas
ScoringSystem.Instance.AddTaskCompleted();  // 100 + bono tiempo

// Configura rutas
ScoringSystem.Instance.AddRouteConfigured(); // +15

// Hace ping exitoso
ScoringSystem.Instance.AddPingSuccess();     // +10

// Obtiene puntaje actual
int score = ScoringSystem.Instance.GetCurrentScore();
Debug.Log($"Puntaje actual: {score}");

// Finalizar y obtener nota
ScoringSystem.Instance.EndSession();
int grade = ScoringSystem.Instance.GetGrade();
Debug.Log($"Nota final: {grade}");
```
