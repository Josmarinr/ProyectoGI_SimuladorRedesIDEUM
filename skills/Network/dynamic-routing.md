# Skill: Enrutamiento Dinámico RIP/OSPF

## DynamicRoutingProtocol.cs

Ubicación: `Assets/Scripts/Simulation/DynamicRoutingProtocol.cs`

### Uso
```csharp
// Agregar componente
var dynProtocol = gameManager.AddComponent<DynamicRoutingProtocol>();

// Configurar protocolo
dynProtocol.protocol = DynamicRoutingProtocol.ProtocolType.RIP;
// o
dynProtocol.protocol = DynamicRoutingProtocol.ProtocolType.OSPF;

// Iniciar simulación
dynProtocol.StartProtocol();

// Suscribirse a eventos
dynProtocol.OnProtocolLog += (msg) => Debug.Log(msg);
dynProtocol.OnConvergence += () => Debug.Log("Convergencia alcanzada");

// Detener
dynProtocol.StopProtocol();

// Limpiar rutas
dynProtocol.ClearAllRoutes();
```

### Configuración
- `advertisementInterval`: Tiempo entre anuncios (default 3s)
- `protocol`: RIP u OSPF

### Protocolo RIP
- Cuenta hops (máximo 15)
- Métrica = número de routers en la ruta

### Protocolo OSPF
- Costo por enlace
- Métrica = costo acumulado de la ruta

### Eventos
- `OnProtocolLog(string)`: Cada mensaje de log del protocolo
- `OnConvergence()`: Cuando la red converge

## Integration en SceneSetup
```csharp
private void StartDynamicProtocol(GameObject panel)
{
    var dynProtocol = gameManager.AddComponent<DynamicRoutingProtocol>();
    dynProtocol.protocol = selectedProtocol == "RIP" ?
        DynamicRoutingProtocol.ProtocolType.RIP :
        DynamicRoutingProtocol.ProtocolType.OSPF;

    dynProtocol.OnProtocolLog += (msg) => { /* actualizar UI */ };
    dynProtocol.OnConvergence += () => { /* mostrar éxito */ };

    dynProtocol.StartProtocol();
}
```

## Panel UI
Botones en panel de Enrutamiento Dinámico:
- **RIP**: Selecciona protocolo RIP
- **OSPF**: Selecciona protocolo OSPF
- **START**: Inicia la simulación
- **STOP**: Detiene la simulación
- **LIMPIAR**: Borra todas las rutas
- **VER RUTAS**: Muestra tablas de todos los routers