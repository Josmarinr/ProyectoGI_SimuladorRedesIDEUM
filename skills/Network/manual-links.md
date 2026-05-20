# Skill: Sistema de Conexiones Manuales

## Descripción
Implementación de botones CONECTAR/DESCONECTAR para crear y eliminar enlaces entre nodos manualmente.

## Arquitectura

### TopologyManager (AddLink/RemoveLink)

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

### SceneSetup (ToggleLinkMode/HandleNodeClick)

```csharp
private string currentLinkMode = null;
private int linkModeFirstNode = -1;

public void ToggleLinkMode(string mode)
{
    var topology = FindObjectOfType<TopologyManager>();
    if (topology == null) return;

    if (currentLinkMode == mode)
    {
        currentLinkMode = null;
        linkModeFirstNode = -1;
        return;
    }

    currentLinkMode = mode;
    linkModeFirstNode = -1;
}

public void HandleNodeClick(int discId)
{
    var topology = FindObjectOfType<TopologyManager>();
    if (topology == null) return;

    if (currentLinkMode == "connect")
    {
        if (linkModeFirstNode == -1)
            linkModeFirstNode = discId;
        else if (linkModeFirstNode != discId)
        {
            topology.AddLink(linkModeFirstNode, discId);
            linkModeFirstNode = -1;
            currentLinkMode = null;
        }
    }
    else if (currentLinkMode == "disconnect")
    {
        if (linkModeFirstNode == -1)
            linkModeFirstNode = discId;
        else
        {
            topology.RemoveLink(linkModeFirstNode, discId);
            linkModeFirstNode = -1;
            currentLinkMode = null;
        }
    }
}
```

### NodeVisualizer (Click en Nodos)

```csharp
// En CreateNodeVisual() agregar:
var trigger = nodeObj.AddComponent<EventTrigger>();
AddClickEvent(trigger, () => OnNodeClicked(node.DiscId));

private void AddClickEvent(EventTrigger trigger, UnityEngine.Events.UnityAction action)
{
    var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
    entry.callback.AddListener((data) => action());
    trigger.triggers.Add(entry);
}

private void OnNodeClicked(int discId)
{
    HighlightSelectedNode(discId);
    var sceneSetup = FindObjectOfType<SimRedes.SceneSetup>();
    if (sceneSetup != null)
        sceneSetup.HandleNodeClick(discId);
}

private void HighlightSelectedNode(int discId)
{
    foreach (var kvp in nodeObjects)
    {
        var outline = kvp.Value.GetComponent<Outline>();
        if (outline != null)
        {
            outline.effectColor = (kvp.Key == discId) ? Color.yellow : Color.black;
            outline.effectDistance = (kvp.Key == discId) ? new Vector2(4, 4) : new Vector2(2, 2);
        }
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
- Olvidar marcar metodos como public
- No agregar EventTrigger a nodos
