---
name: topology-detection
description: >-
  Use when implementing or modifying topology detection logic in
  SimuladorRedes IDEUM. Covers the detection algorithm, rules table,
  event subscriptions, and supported topology types (Star, Bus, Ring,
  Tree, Mesh). Use for BuildTopologyActivity or topology UI tasks.
---

# Skill: Detección de Topología

## Algoritmo de Detección

```csharp
private void DetectTopologyType()
{
    var nodes = topologyManager.GetAllNodes();
    var links = topologyManager.GetAllLinks();

    if (nodes.Count == 0)
        currentTopology = TopologyType.Ninguna;
    else if (nodes.Count == 1)
        currentTopology = TopologyType.Estrella;
    else
    {
        int routers = nodes.Count(n => n.Type == DeviceType.Router);
        int switches = nodes.Count(n => n.Type == DeviceType.Switch);
        int linkCount = links.Count;
        int perfectMeshLinks = nodes.Count * (nodes.Count - 1) / 2;

        if (linkCount == perfectMeshLinks)
            currentTopology = TopologyType.Malla;
        else if (linkCount == nodes.Count)
            currentTopology = TopologyType.Anillo;
        else if (switches > 0 && (routers == 1 || switches >= nodes.Count - 1))
            currentTopology = TopologyType.Estrella;
        else if (routers >= 2 && switches > 0)
            currentTopology = TopologyType.Arbol;
        else
            currentTopology = TopologyType.Bus;
    }
}
```

## Reglas de Detección

| Condicion | Topologia |
|-----------|-----------|
| Nodos = 0 | Sin topologia |
| Nodos = 1 | Estrella |
| Enlaces = n*(n-1)/2 | Malla |
| Enlaces = nodos | Anillo |
| 1 switch + routers | Estrella |
| 2+ routers + switches | Arbol |
| Por defecto | Bus |

## Suscripcion a Eventos

```csharp
private void Start()
{
    topologyManager = FindObjectOfType<TopologyManager>();
    if (topologyManager != null)
    {
        topologyManager.OnTopologyChanged += OnTopologyChanged;
        topologyManager.OnNodeAdded += (node) => { DetectTopologyType(); UpdateUI(); };
        topologyManager.OnNodeRemoved += (node) => { DetectTopologyType(); UpdateUI(); };
        topologyManager.OnLinkAdded += (link) => { DetectTopologyType(); UpdateUI(); };
    }

    FindUITexts();
    DetectTopologyType();
    UpdateUI();
}
```

## Errores Comunes
- Division por cero en maxPossibleLinks
- No actualizar UI despues de detectar
- No suscribir a eventos OnLinkAdded
