# API — RoutingTable, ARPTable, y clases asociadas

> **Namespace**: `SimRedes.Network` · **Archivos**: `Assets/Scripts/Network/RoutingTable.cs`, `Assets/Scripts/Network/ARPTable.cs`

---

## RoutingTable

```csharp
public class RoutingTable
```

Tabla de enrutamiento asociada a cada `NetworkNode` (solo routers la usan realmente).

### Métodos

**Agregar Rutas Estáticas**

```csharp
public void AddStaticRoute(string destNetwork, string subnetMask, string nextHop, string outInterface)
```
Agrega una ruta estática con métrica 0 y protocolo "Static".

**Agregar Rutas Dinámicas**

```csharp
public void AddRipRoute(string destNetwork, string nextHop, string outInterface, int hops)
```
Agrega ruta aprendida por RIP. Asigna máscara /24 automáticamente si no se especifica.

```csharp
public void AddOspfRoute(string destNetwork, string nextHop, string outInterface, int cost)
```
Agrega ruta aprendida por OSPF. Asigna máscara /24 automáticamente si no se especifica.

```csharp
public void AddEigrpRoute(string destNetwork, string nextHop, string outInterface, int compositeMetric)
```
Agrega ruta aprendida por EIGRP con métrica compuesta. Asigna máscara /24 automáticamente.

**Búsqueda**

```csharp
public RoutingEntry FindBestRoute(string destinationIP)
```
Encuentra la mejor ruta para una IP destino usando:
1. **Longest prefix match** (máscara más específica)
2. **Lowest metric** (menor métrica como desempate)

**Consultas**

```csharp
public List<RoutingEntry> GetAllEntries()
```
Retorna **copia** de la lista interna (seguridad para evitar mutación externa).

```csharp
public void Clear()
```
Elimina todas las entradas de la tabla.

```csharp
public string GetTableSummary()
```
Retorna resumen formateado: número de entradas.

---

## RoutingEntry

```csharp
public class RoutingEntry
```

Entrada individual en la tabla de enrutamiento.

### Propiedades

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `DestinationNetwork` | `string` | Red destino (ej: "192.168.1.0") |
| `SubnetMask` | `string` | Máscara de subred (ej: "255.255.255.0") |
| `NextHop` | `string` | IP del siguiente salto |
| `OutInterface` | `string` | Interfaz de salida (ej: "G0/0") |
| `Metric` | `int` | Métrica de la ruta |
| `Protocol` | `string` | Protocolo de origen ("Static", "RIP", "OSPF", "EIGRP") |

### Métodos

```csharp
public int GetPrefixLength()
```
Calcula el prefijo CIDR desde la máscara. Ej: "255.255.255.0" → 24.

---

## ARPTable

```csharp
public class ARPTable
```

Tabla ARP asociada a cada `NetworkNode` (solo routers la usan).

### Métodos

```csharp
public void AddEntry(string ipAddress, string macAddress, string interfaceName)
```
Agrega entrada ARP con MAC explícita.

```csharp
public void AddEntry(string ipAddress, string interfaceName)
```
Agrega entrada ARP con MAC auto-generada.

```csharp
public ARPEntry FindEntry(string ipAddress)
```
Busca entrada por IP. Retorna `null` si no existe.

```csharp
public List<ARPEntry> GetAllEntries()
```
Retorna copia de todas las entradas.

```csharp
public void Clear()
```
Limpia la tabla.

---

## ARPEntry

```csharp
public class ARPEntry
```

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `IPAddress` | `string` | Dirección IP |
| `MACAddress` | `string` | Dirección MAC |
| `Interface` | `string` | Interfaz de red |
| `Type` | - | Tipo de entrada |
| `Age` | `float` | Edad en segundos |

---

## Código de Ejemplo

```csharp
// Obtener tabla de enrutamiento de un router
var router = topology.GetNode(100);
var table = router.RoutingTable;

// Agregar ruta estática
table.AddStaticRoute("192.168.2.0", "255.255.255.0", "192.168.1.254", "G0/0");

// Buscar mejor ruta
var best = table.FindBestRoute("192.168.2.50");
if (best != null) {
    Debug.Log($"Mejor ruta: {best.DestinationNetwork}/{best.GetPrefixLength()}");
}

// Agregar entrada ARP
router.ArpTable.AddEntry("192.168.1.1", "AA:BB:CC:DD:EE:FF", "G0/0");

// Listar entradas
foreach (var entry in table.GetAllEntries()) {
    Debug.Log($"{entry.DestinationNetwork}/{entry.GetPrefixLength()} → {entry.NextHop} [{entry.Protocol}]");
}
```
