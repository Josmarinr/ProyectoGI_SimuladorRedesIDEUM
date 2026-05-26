# API — TopologyManager

> **Namespace**: `SimRedes.Network` · **Archivo**: `Assets/Scripts/Network/TopologyManager.cs`
>
> Singleton que orquesta todos los elementos de red: nodos, enlaces, pathfinding, y managers de VLAN/ACL/NAT.

---

## Propiedades

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `Instance` | `TopologyManager` (static) | Acceso singleton global |
| `VLAN` | `VLANManager` | Gestor de redes virtuales |
| `ACL` | `ACLManager` | Gestor de listas de acceso |
| `NAT` | `NATManager` | Gestor de traducción de direcciones |

## Eventos

| Evento | Firma | Descripción |
|--------|-------|-------------|
| `OnNodeAdded` | `Action<NetworkNode>` | Se dispara cuando se agrega un nodo |
| `OnNodeRemoved` | `Action<NetworkNode>` | Se dispara cuando se elimina un nodo |
| `OnLinkAdded` | `Action<NetworkLink>` | Se dispara cuando se crea un enlace |
| `OnLinkRemoved` | `Action<NetworkLink>` | Se dispara cuando se elimina un enlace |
| `OnTopologyChanged` | `Action` | Se dispara cuando cambia la topología |

## Métodos Públicos

### Gestión de Nodos

```csharp
public NetworkNode AddNode(int discId, DeviceType type, Vector2 position)
```
Crea y agrega un nuevo nodo a la topología. Dispara `OnNodeAdded` y `OnTopologyChanged`.
- `discId`: Identificador único del disco (100, 101...)
- `type`: Tipo de dispositivo (Router, Switch, PC, Unknown)
- `position`: Posición en el canvas
- **Retorna**: El `NetworkNode` creado

```csharp
public void RemoveNode(NetworkNode node)
```
Elimina un nodo y todos sus enlaces asociados. Dispara `OnNodeRemoved` y `OnTopologyChanged`.

```csharp
public NetworkNode GetNode(int discId)
```
Busca un nodo por su `DiscId`.
- **Retorna**: El nodo o `null` si no existe

```csharp
public List<NetworkNode> FindNodesNear(Vector2 position, float radius)
```
Busca todos los nodos dentro de un radio desde una posición.
- **Retorna**: Lista de nodos en el radio (vacía si no hay)

```csharp
public List<NetworkNode> GetAllNodes()
```
Retorna una copia de la lista completa de nodos.

### Gestión de Enlaces

```csharp
public NetworkLink AddLink(NetworkNode source, NetworkNode destination)
```
Crea un enlace entre dos nodos. Dispara `OnLinkAdded` y `OnTopologyChanged`.

```csharp
public void RemoveLink(NetworkLink link)
```
Elimina un enlace. Dispara `OnLinkRemoved` y `OnTopologyChanged`.

```csharp
public List<NetworkLink> GetAllLinks()
```
Retorna una copia de la lista completa de enlaces.

### Pathfinding y Conectividad

```csharp
public List<NetworkNode> FindPath(NetworkNode source, NetworkNode destination)
```
Encuentra la ruta más corta entre dos nodos usando BFS (Breadth-First Search).

```csharp
public List<NetworkLink> GetLinksOnPath(List<NetworkNode> path)
```
Obtiene los enlaces que conectan los nodos en una ruta.

```csharp
public bool CheckConnectivity(NetworkNode source, NetworkNode destination)
```
Valida conectividad completa: VLAN + ACL + NAT + pathfinding BFS.

### Fallos y Limpieza

```csharp
public void SetNodeFault(NetworkNode node, string faultType)
```
Aplica un fallo a un nodo.

```csharp
public void ClearNodeFault(NetworkNode node)
```
Limpia el fallo de un nodo.

```csharp
public void ClearTopology()
```
Elimina todos los nodos y enlaces.

### Resumen y Reports

```csharp
public string GetTopologySummary()
```
Retorna resumen de la topología: número de routers, switches, PCs y enlaces.

```csharp
public string GetVLANSummary()
```
Delega en `VLANManager.GetVLANSummary()`.

```csharp
public string GetACLSummary()
```
Delega en `ACLManager.GetACLSummary()`.

```csharp
public string GetNATSummary()
```
Delega en `NATManager.GetNATSummary()`.

## Código de Ejemplo

```csharp
// Acceder al singleton
var topology = TopologyManager.Instance;

// Crear nodos
var router = topology.AddNode(100, DeviceType.Router, new Vector2(500, 400));
var pc = topology.AddNode(101, DeviceType.PC, new Vector2(800, 400));

// Conectarlos
var link = topology.AddLink(router, pc);

// Buscar ruta
var path = topology.FindPath(router, pc);
if (path != null) {
    Debug.Log($"Ruta encontrada: {path.Count} nodos");
}

// Suscribirse a eventos
topology.OnNodeAdded += node => Debug.Log($"Nodo agregado: {node.Name}");
topology.OnTopologyChanged += () => Debug.Log("Topología cambiada");
```
