# Skill: Escenarios Preconfigurados

## PredefinedScenarios.cs

Ubicación: `Assets/Scripts/Simulation/PredefinedScenarios.cs`

### Uso
```csharp
// Obtener instancia (se crea automáticamente en Awake)
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
    public string name;           // Nombre del escenario
    public string description;    // Descripción
    public ScenarioDifficulty difficulty;  // Basico/Intermedio/Avanzado
    public List<DeviceConfig> devices;     // Dispositivos
    public List<LinkConfig> links;        // Enlaces entre dispositivos
    public List<IPConfig> ipConfigurations; // IPs preconfiguradas
    public List<FaultConfig> faults;      // Fallos a configurar
    public string[] objectives;   // Objetivos del estudiante
    public string[] hints;         // Ayudas
}
```

### Escenarios Disponibles

1. **Estrella Simple** (Basico)
   - 1 Router + 3 PCs
   - IPs en subred 192.168.1.0/24
   - Objetivos: Configurar IPs, verificar ping, revisar ARP

2. **Dos Routers** (Intermedio)
   - 2 Routers + 4 PCs
   - Enrutamiento estático necesario
   - Objetivos: Configurar IPs, agregar rutas, verificar ping entre redes

3. **Topologia en Anillo** (Intermedio)
   - 3 Routers + 1 Switch + 2 PCs
   - Protocolo RIP
   - Objetivos: Configurar IPs, iniciar RIP, verificar convergencia

4. **Red en Arbol** (Avanzado)
   - 3 Routers + 4 Switches + 4 PCs
   - Jerarquía compleja
   - Objetivos: Configurar IPs, enrutamiento, verificar conectividad

5. **Detectar Fallos** (Intermedio)
   - 2 Routers + 1 Switch + 2 PCs
   - Fallos preconfigurados (badmask, badip)
   - Objetivos: Encontrar y corregir fallos

### Cargar Escenario
```csharp
// BuildScenarioTopology crea la topología automáticamente
var scenario = PredefinedScenarios.Instance.GetScenario(index);
BuildScenarioTopology(scenario);
// Crea nodos, enlaces, configura IPs y fallos
```

### Tipos de Fallos
- `badip` - IP inválida
- `badmask` - Máscara incorrecta
- `noip` - Sin IP configurada

## Integration en SceneSetup

### Panel de Escenarios
```csharp
private void CreateScenariosPanel(Transform canvasTransform)
{
    // Crear PredefinedScenarios si no existe
    if (gameManager.GetComponent<PredefinedScenarios>() == null)
        gameManager.AddComponent<PredefinedScenarios>();

    var scenarios = PredefinedScenarios.Instance.GetScenarios();

    // Crear items para cada escenario
    for (int i = 0; i < scenarios.Count; i++)
    {
        CreateScenarioItem(scenarios[i], i);
    }
}
```

### Cargar y Construir
```csharp
private void LoadScenario(int scenarioIndex)
{
    var scenario = PredefinedScenarios.Instance.GetScenario(scenarioIndex);
    ShowScenarioInfo(scenario);  // Muestra objetivos y ayudas
}

private void BuildScenarioTopology(NetworkScenario scenario)
{
    // 1. Limpiar topología existente
    topology.ClearTopology();

    // 2. Crear dispositivos
    foreach (var dev in scenario.devices)
    {
        int discId = createdNodes.Count + 1;
        topology.AddNode(discId, devType, position);
    }

    // 3. Crear enlaces
    foreach (var link in scenario.links)
    {
        topology.AddLink(...);
    }

    // 4. Configurar IPs
    foreach (var ipConfig in scenario.ipConfigurations)
    {
        node.IpAddress = ipConfig.ip;
        node.SubnetMask = ipConfig.mask;
    }

    // 5. Configurar fallos
    foreach (var fault in scenario.faults)
    {
        topology.SetNodeFault(node.DiscId, fault.faultType);
    }
}
```