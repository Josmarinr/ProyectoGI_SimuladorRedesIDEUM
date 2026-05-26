---
name: network-tables
description: >-
  Use when working with IP validation, routing tables, ARP tables, ping
  visualization, or connectivity checking in SimuladorRedes IDEUM. Covers
  IPValidation API, PingVisualizer, RoutingTable operations, NetworkNode
  properties, and CheckConnectivity logic. Essential for any task
  involving network verification or routing configuration.
---

# Skill: Tablas de Red, Validación de IP y Ping Visual

## IPValidation.cs

```csharp
// Ubicación: Assets/Scripts/Network/IPValidation.cs

// Validaciones
IPValidation.IsValidIP("192.168.1.1")          // true/false
IPValidation.IsValidSubnetMask("255.255.255.0") // true/false

// Utilidades
IPValidation.GetPrefixLength("255.255.255.0")   // 24
IPValidation.GetNetworkAddress("192.168.1.1", "255.255.255.0")  // "192.168.1.0"
IPValidation.GetBroadcastAddress("192.168.1.1", "255.255.255.0") // "192.168.1.255"
IPValidation.GetGatewayFromIP("192.168.1.100")  // "192.168.1.1"
IPValidation.IsInSameNetwork("192.168.1.10", "255.255.255.0", "192.168.1.20") // true
```

## PingVisualizer.cs

```csharp
// Ubicación: Assets/Scripts/UI/PingVisualizer.cs

// Animar ping entre dos nodos
var pingVis = FindObjectOfType<PingVisualizer>();
pingVis.AnimatePing(sourceDiscId, destDiscId, (success) => {
    Debug.Log($"Ping: {(success ? "OK" : "FALLO")}");
});
```

## Verificación de Conectividad en TopologyManager

```csharp
// Verificar si hay conectividad (considera IPs)
bool connected = topology.CheckConnectivity(sourceDiscId, destDiscId);

// Encontrar ruta entre nodos
var path = topology.FindPath(sourceDiscId, destDiscId);
```

## Lógica de Conectividad en CheckConnectivity

### Verifica en orden:
1. **Conexión física** - ¿Hay path entre nodos? (BFS)
2. **IPs válidas** - ¿Ambos nodos tienen IP configurada?
3. **Máscara válida** - ¿La máscara es válida?
4. **Misma subred** - ¿PCs/Routers están en la misma red IP?

### Tabla de Verificación

| Origen | Destino | Verificación |
|--------|---------|-------------|
| PC | PC | IP + Mask + misma subred |
| Router | Router | IP + Mask + misma subred |
| Router | PC | IP + Mask (ambos) |
| PC | Router | IP + Mask (ambos) |
| Switch | Cualquiera | Solo conexión física |

## Colores de Protocolo en Routing

```csharp
Color protoColor = entry.Protocol == "Static" ? Color.green :
                  (entry.Protocol == "RIP" ? Color.yellow : Color.cyan);
```
