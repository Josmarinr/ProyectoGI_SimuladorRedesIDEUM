# Skill: VLAN, ACL y NAT

## Descripción
Configuración de VLANs, ACLs y NAT en el simulador.

## Acceso

```csharp
var topology = FindObjectOfType<TopologyManager>();
var vlanManager = topology.VLAN;
var aclManager = topology.ACL;
var natManager = topology.NAT;
```

## VLANs

```csharp
// Crear VLAN
int vlanId = vlanManager.CreateVLAN(10);

// Asignar nodo a VLAN
vlanManager.AssignToVLAN(node, vlanId);

// Ver VLAN de un nodo
int nodeVlan = vlanManager.GetNodeVer(node);

// Verificar comunicación
bool canCommunicate = vlanManager.CanCommunicate(source, dest);

// Obtener nodos en VLAN
var nodes = vlanManager.GetNodesInVLAN(vlanId);

// Eliminar VLAN
vlanManager.DeleteVLAN(vlanId);

// Resumen
Debug.Log(vlanManager.GetVLANSummary());
```

## ACLs

```csharp
// Crear ACL
aclManager.CreateACL("MI_ACL");

// Regla estándar
var rule = ACLManager.CreateStandardRule("permit", "192.168.1.0", "Permit LAN");
aclManager.AddRule("MI_ACL", rule);

// Regla extendida
var extRule = ACLManager.CreateExtendedRule("permit", "tcp", "192.168.1.0", "10.0.0.0", "HTTP");
aclManager.AddRule("MI_ACL", extRule);

// Verificar paquete
bool allowed = aclManager.CheckPacket("MI_ACL", "192.168.1.10", "10.0.0.5", 80, 8080, "tcp");

// Obtener reglas
var rules = aclManager.GetRules("MI_ACL");
```

## NAT

```csharp
// Configurar IP pública
natManager.SetPublicIP("200.100.50.1");

// NAT estática
natManager.AddStaticNAT("192.168.1.10", "200.100.50.10");

// NAT dinámica
natManager.AddDynamicNAT("192.168.1.20");

// PAT (Port Address Translation)
natManager.AddPAT("192.168.1.30", 80, "TCP");

// Traducir paquete
bool translated = natManager.TranslatePacket(sourceIP, destIP, sourcePort, destPort, isOutgoing);

// Ver tabla NAT
var table = natManager.GetNATTable();

// Limpiar NAT
natManager.ClearNAT();

// Resumen
Debug.Log(natManager.GetNATSummary());
```

## UI

Botones en TopologyInfoPanel:
- **VLAN**: Muestra configuración de VLANs
- **ACL**: Muestra configuración de ACLs
- **NAT**: Muestra configuración de NAT

Panel se destruye con GoBackToMainMenu().

## Errores a Evitar

- VLAN 1 es la VLAN por defecto, no se puede eliminar
- ACLs sin reglas permiten todo por defecto
- NAT necesita IP pública configurada primero
- Verificar compatibilidad de VLAN antes de comunicar nodos