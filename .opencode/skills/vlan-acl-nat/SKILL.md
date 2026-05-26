---
name: vlan-acl-nat
description: >-
  Use when configuring or modifying VLAN, ACL, or NAT functionality in
  SimuladorRedes IDEUM. Covers VLAN creation and assignment, ACL rule
  management, NAT translation (static/dynamic/PAT), and UI integration.
  Use for networking tasks beyond basic routing.
---

# Skill: VLAN, ACL y NAT

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

// Verificar comunicación
bool canCommunicate = vlanManager.CanCommunicate(source, dest);

// Obtener resumen
Debug.Log(vlanManager.GetVLANSummary());
```

## ACLs

```csharp
// Crear ACL
aclManager.CreateACL("MI_ACL");

// Regla estándar
var rule = ACLManager.CreateStandardRule("permit", "192.168.1.0", "Permit LAN");
aclManager.AddRule("MI_ACL", rule);

// Verificar paquete
bool allowed = aclManager.CheckPacket("MI_ACL", "192.168.1.10", "10.0.0.5", 80, 8080, "tcp");
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
```

## Errores a Evitar

- VLAN 1 es la VLAN por defecto, no se puede eliminar
- ACLs sin reglas permiten todo por defecto
- NAT necesita IP pública configurada primero
- Verificar compatibilidad de VLAN antes de comunicar nodos
