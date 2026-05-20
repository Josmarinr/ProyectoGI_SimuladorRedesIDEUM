# Skills - Simulador de Redes IDEUM

Guías de referencia rápida para el proyecto. Cada skill contiene código listo para copiar y patrones del proyecto.

## Estructura

```
skills/
├── UI/                        # Creación de interfaces
│   ├── unity-scene-setup.md   # SceneSetup.cs, paneles dinámicos
│   ├── unity-ui-buttons.md    # Botones redondeados, colores, paneles
│   ├── menu-navigation.md     # Navegación teclado/mouse
│   └── ui-performance.md      # CanvasScaler, borrosidad, optimización
│
├── Network/                   # Funcionalidades de red
│   ├── network-tables.md      # ARP, Routing, IP validation, Ping visual
│   ├── topology-detection.md  # Lógica de detección de topología
│   ├── dynamic-routing.md     # RIP/OSPF
│   ├── manual-links.md        # CONECTAR/DESCONECTAR
│   └── vlan-acl-nat.md        # VLANs, ACLs, NAT
│
├── Integration/               # Hardware IDEUM
│   └── ideum-integration.md   # TangibleBridge, DiscManager, DebugSim
│
├── Scoring/
│   └── scoring-system.md      # Puntajes y evaluación
│
├── Scenarios/
│   └── predefined-scenarios.md# Escenarios preconfigurados
│
├── Core/                      # Patrones y utilidades
│   ├── best-practices.md      # Singletons, eventos, caché, cleanup
│   ├── debugging.md           # Profiling, errores comunes
│   └── build-and-deploy.md    # ⚠ Build settings, pantalla negra, despliegue
│
└── README.md
```

## Cuándo cargar cada skill

| Tarea | Skill |
|-------|-------|
| SceneSetup.cs, crear paneles | `unity-scene-setup` |
| Botones, UI, colores | `unity-ui-buttons` |
| Menú principal, navegación | `menu-navigation` |
| UI lenta/borrosa | `ui-performance` |
| TopologyManager, nodos | `topology-detection` |
| ARP/Routing tables, IP | `network-tables` |
| RIP/OSPF | `dynamic-routing` |
| CONECTAR/DESCONECTAR | `manual-links` |
| VLAN, ACL, NAT | `vlan-acl-nat` |
| TangibleEngine, discos | `ideum-integration` |
| Puntajes | `scoring-system` |
| Escenarios | `predefined-scenarios` |
| Bugs, errores | `debugging` |
| Patrones, arquitectura | `best-practices` |
| **Build, pantalla negra, deploy** | **`build-and-deploy`** |

## Flujo de datos

```
Mesa IDEUM → TangibleBridge → TangibleDiscManager → TopologyManager → NodeVisualizer
```

## Buenas prácticas clave

1. **Cachear referencias** en Start(), no FindObjectOfType en Update()
2. **CleanupNullReferences()** antes de iterar diccionarios de objetos
3. **Capturar variables locales** en lambdas (`int captured = node.DiscId`)
4. **Unsubscribe** en OnDestroy() para evitar memory leaks
5. **Nombres únicos** en GameObjects UI para encontrarlos después
6. **Limpiar paneles existentes** antes de crear nuevos (`Destroy` si existe)
