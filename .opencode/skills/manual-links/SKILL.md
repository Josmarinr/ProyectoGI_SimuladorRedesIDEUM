---
name: manual-links
description: >-
  Use when implementing or modifying the manual link connection system
  in SimuladorRedes IDEUM. Covers CONNECT/DISCONNECT mode in
  LinkModeController, HandleNodeClick in TopologyManager, AddLink/RemoveLink
  API, EventTrigger setup on nodes, and highlighting. Use for any task
  involving link creation or deletion.
---

# Skill: Sistema de Conexiones Manuales

## TopologyManager (AddLink/RemoveLink)

```csharp
public void AddLink(int sourceDiscId, int destDiscId, string srcInterface = "G0/0", string dstInterface = "G0/0")
{
    if (nodes.TryGetValue(sourceDiscId, out var source) && nodes.TryGetValue(destDiscId, out var dest))
    {
        var existingLink = links.FirstOrDefault(l =>
            (l.SourceNode.DiscId == sourceDiscId && l.DestinationNode.DiscId == destDiscId) ||
            (l.SourceNode.DiscId == destDiscId && l.DestinationNode.DiscId == sourceDiscId));

        if (existingLink == null)
        {
            var link = new NetworkLink(source, dest, srcInterface, dstInterface);
            links.Add(link);
            OnLinkAdded?.Invoke(link);
            OnTopologyChanged?.Invoke();
        }
    }
}

public void RemoveLink(int sourceDiscId, int destDiscId)
{
    var link = links.FirstOrDefault(l =>
        (l.SourceNode.DiscId == sourceDiscId && l.DestinationNode.DiscId == destDiscId) ||
        (l.SourceNode.DiscId == destDiscId && l.DestinationNode.DiscId == sourceDiscId));

    if (link != null)
    {
        links.Remove(link);
        OnLinkRemoved?.Invoke(link);
        OnTopologyChanged?.Invoke();
    }
}
```

## Cómo Funciona

### CONECTAR:
1. Usuario presiona boton CONECTAR
2. Click en primer nodo (se marca amarillo)
3. Click en segundo nodo (se crea enlace)
4. Modo se desactiva

### DESCONECTAR:
1. Usuario presiona boton DESCONECTAR
2. Click en primer nodo del enlace
3. Click en segundo nodo (se elimina enlace)
4. Modo se desactiva

## Errores Comunes
- No usar FindObjectOfType en listeners
- No agregar EventTrigger a nodos
