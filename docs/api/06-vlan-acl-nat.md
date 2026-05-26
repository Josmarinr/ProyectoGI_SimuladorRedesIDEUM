# API — VLAN, ACL y NAT

> **Namespace**: `SimRedes.Network` · **Archivos**: `Assets/Scripts/Network/VLANManager.cs`, `ACLManager.cs`, `NATManager.cs`

---

## VLANManager

```csharp
public class VLANManager
```

Gestión de redes virtuales (VLANs). Instancia única en `TopologyManager.VLAN`.

### Constantes

| Constante | Valor |
|-----------|:-----:|
| `DEFAULT_VLAN` | 1 |
| `MAX_VLANS` | 4094 |

### Métodos

```csharp
public void CreateVLAN(int vlanId)
```
Crea una VLAN con el ID especificado (1-4094).

```csharp
public void DeleteVLAN(int vlanId)
```
Elimina una VLAN y desasigna todos sus nodos.

```csharp
public void AssignToVLAN(NetworkNode node, int vlanId)
```
Asigna un nodo a una VLAN.

```csharp
public int GetNodeVLAN(NetworkNode node)
```
Retorna el ID de la VLAN del nodo (default 1).

```csharp
public bool CanCommunicate(NetworkNode src, NetworkNode dst)
```
Verifica si dos nodos pueden comunicarse (misma VLAN).
- **Retorna**: `true` si ambos están en la misma VLAN

```csharp
public List<NetworkNode> GetNodesInVLAN(int vlanId)
```
Retorna todos los nodos asignados a una VLAN.

```csharp
public List<int> GetAllVLANs()
```
Retorna lista de IDs de VLANs existentes.

```csharp
public string GetVLANSummary()
```
Retorna resumen de VLANs.

---

## ACLManager

```csharp
public class ACLManager
```

Gestión de listas de control de acceso. Instancia única en `TopologyManager.ACL`.

### Métodos

```csharp
public void CreateACL(string name)
```
Crea una ACL con nombre.

```csharp
public void AddRule(string aclName, ACLRule rule)
```
Agrega una regla a una ACL existente.

```csharp
public void RemoveRule(string aclName, int sequence)
```
Elimina una regla por número de secuencia.

```csharp
public bool CheckPacket(string aclName, string srcIP, string dstIP, ...)
```
Verifica si un paquete es permitido según las reglas de la ACL.

```csharp
public List<ACLRule> GetRules(string aclName)
```
Retorna las reglas de una ACL.

```csharp
public void DeleteACL(string name)
```
Elimina una ACL completa.

```csharp
public string GetACLSummary()
```
Retorna resumen de ACLs configuradas.

---

## ACLRule

```csharp
public class ACLRule
```

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `Sequence` | `int` | Número de orden |
| `Action` | `ACLAction` | `Permit` o `Deny` |
| `Protocol` | `ACLProtocol` | `Any`, `TCP`, `UDP`, `ICMP`, `IP` |
| `SourceIP` | `string` | IP origen |
| `SourceMask` | `string` | Máscara origen |
| `DestIP` | `string` | IP destino |
| `DestMask` | `string` | Máscara destino |
| `SourcePort` | `int` | Puerto origen |
| `DestPort` | `int` | Puerto destino |
| `IsEnabled` | `bool` | Regla activa o no |

---

## NATManager

```csharp
public class NATManager
```

Gestión de traducción de direcciones de red. Instancia única en `TopologyManager.NAT`.

### Propiedades

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `IsEnabled` | `bool` | NAT activo o no |

### Métodos

```csharp
public void SetPublicIP(string ip)
```
Configura la IP pública del NAT.

```csharp
public string GetRouterIP()
```
Retorna la IP del router configurado.

```csharp
public void AddStaticNAT(string internalIP, string externalIP)
```
Agrega traducción NAT estática (1:1).

```csharp
public void AddDynamicNAT(string internalIP, string poolIP)
```
Agrega traducción NAT dinámica (pool).

```csharp
public void AddPAT(string internalIP, int port)
```
Agrega traducción PAT (many-to-one con puerto).

```csharp
public string TranslatePacket(string ip)
```
Aplica traducción a un paquete.

```csharp
public string TranslateSourceIP(string internalIP)
```
Traduce IP origen interna → externa.

```csharp
public string TranslateDestIP(string externalIP)
```
Traduce IP destino externa → interna.

```csharp
public List<NATEntry> GetNATTable()
```
Retorna la tabla NAT completa.

```csharp
public string GetNATSummary()
```
Retorna resumen de configuraciones NAT.

---

## NATEntry

```csharp
public class NATEntry
```

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `InternalIP` | `string` | IP interna |
| `ExternalIP` | `string` | IP externa |
| `InternalPort` | `int` | Puerto interno |
| `ExternalPort` | `int` | Puerto externo |
| `Type` | `NATType` | `Static`, `Dynamic`, `PAT` |

---

## Código de Ejemplo

```csharp
var topology = TopologyManager.Instance;

// VLAN
topology.VLAN.CreateVLAN(10);
topology.VLAN.AssignToVLAN(pc1, 10);
topology.VLAN.AssignToVLAN(pc2, 10);
bool canComm = topology.VLAN.CanCommunicate(pc1, pc2);

// ACL
topology.ACL.CreateACL("BLOCK_SSH");
var rule = new ACLRule {
    Action = ACLAction.Deny,
    Protocol = ACLProtocol.TCP,
    DestPort = 22,
    IsEnabled = true
};
topology.ACL.AddRule("BLOCK_SSH", rule);

// NAT
topology.NAT.SetPublicIP("200.100.50.1");
topology.NAT.AddStaticNAT("192.168.1.10", "200.100.50.10");
string translated = topology.NAT.TranslateSourceIP("192.168.1.10");
```
