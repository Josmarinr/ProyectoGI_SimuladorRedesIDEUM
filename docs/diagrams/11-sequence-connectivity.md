# DS-03: Secuencia — CheckConnectivity

> **Propósito**: Mostrar el flujo completo de validación de conectividad entre dos nodos, incluyendo pathfinding BFS, validación VLAN, ACL, NAT y animación de ping.

```mermaid
sequenceDiagram
    participant User as Estudiante
    participant SC as SimulationControls
    participant PV as PingVisualizer
    participant TM as TopologyManager
    participant VLAN as VLANManager
    participant ACL as ACLManager
    participant NAT as NATManager
    participant NV as NodeVisualizer

    User->>SC: Presiona tecla P (o botón PING)
    SC->>SC: Obtener nodo origen y destino seleccionados
    SC->>PV: AnimatePing(srcDiscId, dstDiscId)

    PV->>TM: FindPath(nodeSrc, nodeDst)
    Note right of TM: BFS desde src a dst

    TM->>TM: ValidateVLAN(nodeSrc, nodeDst)
    TM->>VLAN: CanCommunicate(src, dst)

    alt VLAN bloquea comunicación
        VLAN-->>TM: return false
        TM-->>PV: null (sin ruta)
        PV->>PV: Mostrar "VLAN no permite comunicación"
        PV-->>SC: callback fallo
    else VLAN permite
        VLAN-->>TM: return true
    end

    TM->>TM: ValidateACL(path)
    TM->>ACL: CheckPacket(acl, srcIP, dstIP)

    alt ACL bloquea
        ACL-->>TM: return false (deny)
        TM-->>PV: null (sin ruta)
        PV->>PV: Mostrar "ACL deniega el tráfico"
    else ACL permite
        ACL-->>TM: return true
    end

    TM->>TM: NAT? Aplica traducción si existe
    TM->>NAT: TranslateSourceIP(srcIP)
    TM->>NAT: TranslateDestIP(dstIP)

    Note right of TM: BFS normal por el grafo
    TM->>TM: BFS(nodos, enlaces)
    TM-->>PV: path[NetworkNode]

    alt Path encontrado
        PV->>PV: Animar paquete amarillo por cada enlace
        PV->>NV: Destacar nodos en la ruta
        Note right of PV: Paquete viaja de nodo en nodo<br/>siguiendo GetLinksOnPath()

        alt LLegada exitosa
            PV->>PV: Paquete → verde ✅
        else Fallo en el camino
            PV->>PV: Paquete → rojo ❌
        end

        PV-->>SC: callback con resultado
        SC->>SC: Actualiza estadísticas de ping
    else No hay ruta
        PV-->>SC: callback "Sin ruta disponible"
        SC->>SC: Muestra mensaje de error
    end
```

## Lógica de Pathfinding

```
CheckConnectivity(src, dst):
  1. Switch es L2 transparente (no requiere IP)
  2. Extremos no-Switch requieren IP + máscara válidas
  3. VLAN: CanCommunicate(src, dst)
  4. ACL: CheckPacket() en cada ACL aplicable
  5. NAT: TranslateSourceIP() + TranslateDestIP()
  6. BFS por TopologyManager.FindPath()
  7. Retorna true si hay ruta física + validaciones
```

## Archivos Relacionados

| Archivo | Rol |
|---------|-----|
| `Assets/Scripts/Network/TopologyManager.cs` | `FindPath()`, `CheckConnectivity()` |
| `Assets/Scripts/Network/VLANManager.cs` | `CanCommunicate()` |
| `Assets/Scripts/Network/ACLManager.cs` | `CheckPacket()` |
| `Assets/Scripts/Network/NATManager.cs` | `TranslateSourceIP()`, `TranslateDestIP()` |
| `Assets/Scripts/UI/PingVisualizer.cs` | `AnimatePing()` |
| `Assets/Scripts/UI/NodeVisualizer.cs` | Destacar nodos en ruta |
