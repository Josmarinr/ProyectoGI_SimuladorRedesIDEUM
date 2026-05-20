# Skill: Detección de Topología

## Descripción
Lógica para detectar automáticamente el tipo de topología de red.

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

## Topologias Soportadas

- Estrella: Un nodo central conectado a varios perifericos
- Bus: Linea lineal de nodos
- Anillo: Nodos en ciclo cerrado
- Arbol: Estructura jerarquica con routers y switches
- Malla: Todos los nodos conectados entre si

## Errores Comunes
- Division por cero en maxPossibleLinks
- No actualizar UI despues de detectar
- No suscribir a eventos OnLinkAdded
