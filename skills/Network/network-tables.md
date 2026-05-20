# Skill: Tablas de Red, Validación de IP y Ping Visual

## Archivos Principales

### IPValidation.cs
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

### PingVisualizer.cs
```csharp
// Ubicación: Assets/Scripts/UI/PingVisualizer.cs

// Animar ping entre dos nodos
var pingVis = FindObjectOfType<PingVisualizer>();
pingVis.AnimatePing(sourceDiscId, destDiscId, (success) => {
    Debug.Log($"Ping: {(success ? "OK" : "FALLO")}");
});
```

## Integración en NetworkNode

```csharp
public class NetworkNode
{
    public ARPTable ArpTable { get; private set; }
    public RoutingTable RoutingTable { get; private set; }

    public string GetDefaultGateway() { ... }
    public string GetNetworkAddress() { ... }
    public int GetPrefixLength() { ... }
    public bool IsValidConfiguration() { ... }
}
```

## Verificación de Conectividad en TopologyManager

```csharp
// Verificar si hay conectividad (considera IPs)
bool connected = topology.CheckConnectivity(sourceDiscId, destDiscId);

// Encontrar ruta entre nodos
var path = topology.FindPath(sourceDiscId, destDiscId);
// path = [Router_1, Switch_1, Router_2]

// Obtener información de red
string networkInfo = topology.GetIPNetworkInfo(discId); // "192.168.1.0/24"
```

## Mostrar Paneles ARP y Routing

En SceneSetup.cs, desde ShowIPConfigPanel():

```csharp
// Solo para Routers
if (node.Type == Network.DeviceType.Router)
{
    Button arpBtn = CreateMenuButton(..., "TABLA ARP", ...);
    arpBtn.onClick.AddListener(() => ShowARPPanel(node));

    Button routingBtn = CreateMenuButton(..., "TABLA RUTAS", ...);
    routingBtn.onClick.AddListener(() => ShowRoutingPanel(node));
}
```

## Panel de Tabla Routing Editable

```csharp
// ShowRoutingPanel(NetworkNode node)
// - Lista todas las rutas actuales
// - Botón X para eliminar cada ruta
// - Botón NUEVA RUTA para añadir

// ShowAddRoutePanel(NetworkNode node)
// - Campos: Red Destino, Mask, Next Hop
// - Selector de Interfaz
// - Validación de IPs antes de añadir
```

## Validación en Tiempo Real

```csharp
string validationInfo = "";
if (!IPValidation.IsValidIP(node.IpAddress))
    validationInfo = " [IP INVALIDA]";
else if (!IPValidation.IsValidSubnetMask(node.SubnetMask))
    validationInfo = " [MASK INVALIDA]";
else if (node.Type == Network.DeviceType.Router)
{
    string network = IPValidation.GetNetworkAddress(node.IpAddress, node.SubnetMask);
    int prefix = IPValidation.GetPrefixLength(node.SubnetMask);
    validationInfo = $"\nRed: {network}/{prefix}";
}
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

### Razones de Fallo
```csharp
"Origen sin IP"
"Destino sin IP"
"Origen sin mascara"
"Redes diferentes"
"Sin conexion"
```

## Ejemplo de Uso

```csharp
// Verificar y configurar conectividad
void ConfigureNode(NetworkNode node)
{
    if (IPValidation.IsValidIP(node.IpAddress) &&
        IPValidation.IsValidSubnetMask(node.SubnetMask))
    {
        Debug.Log($"Red: {node.GetNetworkAddress()}/{node.GetPrefixLength()}");
        Debug.Log($"Gateway: {node.GetDefaultGateway()}");
    }
}

// Añadir ruta estática
node.RoutingTable.AddStaticRoute("192.168.0.0", "255.255.255.0", "192.168.1.1", "G0/0");

// Ejecutar ping visual
void ExecutePing(int sourceId, int destId)
{
    var pingVis = FindObjectOfType<PingVisualizer>();
    pingVis.AnimatePing(sourceId, destId, (success) => {
        Debug.Log(success ? "Ping exitoso" : "Ping fallido");
    });
}
```

## Colores de Protocolo en Routing

```csharp
Color protoColor = entry.Protocol == "Static" ? Color.green :
                  (entry.Protocol == "RIP" ? Color.yellow : Color.cyan);
```

## Máscaras Válidas CIDR

```csharp
private static readonly string[] ValidMasks = new[]
{
    "0.0.0.0",
    "128.0.0.0", "192.168.3.11", "224.0.0.0", "240.0.0.0",
    "248.0.0.0", "252.0.0.0", "254.0.0.0", "255.0.0.0",
    "255.128.0.0", "255.192.0.0", "255.224.0.0", "255.240.0.0",
    "255.248.0.0", "255.252.0.0", "255.254.0.0", "255.255.0.0",
    "255.255.128.0", "255.255.192.0", "255.255.224.0", "255.255.240.0",
    "255.255.248.0", "255.255.252.0", "255.255.254.0", "255.255.255.0",
    "255.255.255.128", "255.255.255.192", "255.255.255.224", "255.255.255.240",
    "255.255.255.248", "255.255.255.252", "255.255.255.254", "255.255.255.255"
};
```