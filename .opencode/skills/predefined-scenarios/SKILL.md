---
name: predefined-scenarios
description: >-
  Use when loading or creating predefined network scenarios in
  SimuladorRedes IDEUM. Covers scenario structure (devices, links, IP
  configs, faults, objectives), the 5 available scenarios (Star, Two
  Routers, Ring, Tree, Fault Detection), scenario loading/building
  pipeline, and fault types. Use for PredefinedScenarios, BuildScenarioTopology,
  or any task involving scenario management.
---

# Skill: Escenarios Preconfigurados

## PredefinedScenarios.cs

Ubicación: `Assets/Scripts/Simulation/PredefinedScenarios.cs`

### Uso
```csharp
// Obtener instancia
var scenarios = PredefinedScenarios.Instance.GetScenarios();

// Obtener por índice
var scenario = PredefinedScenarios.Instance.GetScenario(0);

// Filtrar por dificultad
var basic = PredefinedScenarios.Instance.GetScenariosByDifficulty(
    PredefinedScenarios.ScenarioDifficulty.Basico);
```

### Estructura de Escenario
```csharp
public class NetworkScenario
{
    public string name;                      // Nombre del escenario
    public string description;               // Descripción
    public ScenarioDifficulty difficulty;    // Basico/Intermedio/Avanzado
    public List<DeviceConfig> devices;       // Dispositivos
    public List<LinkConfig> links;           // Enlaces
    public List<IPConfig> ipConfigurations;  // IPs preconfiguradas
    public List<FaultConfig> faults;         // Fallos
    public string[] objectives;              // Objetivos
    public string[] hints;                   // Ayudas
}
```

### Escenarios Disponibles

1. **Estrella Simple** (Basico)
   - 1 Router + 3 PCs, IPs en 192.168.1.0/24

2. **Dos Routers** (Intermedio)
   - 2 Routers + 4 PCs, enrutamiento estático necesario

3. **Topologia en Anillo** (Intermedio)
   - 3 Routers + 1 Switch + 2 PCs, protocolo RIP

4. **Red en Arbol** (Avanzado)
   - 3 Routers + 4 Switches + 4 PCs, jerarquía compleja

5. **Detectar Fallos** (Intermedio)
   - 2 Routers + 1 Switch + 2 PCs, fallos preconfigurados

### Tipos de Fallos
- `badip` - IP inválida
- `badmask` - Máscara incorrecta
- `noip` - Sin IP configurada
