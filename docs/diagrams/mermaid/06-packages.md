# DC-06: Diagrama de Paquetes (Namespaces)

> **Propósito**: Mostrar la organización del código en namespaces y sus dependencias.
>
> **Proyecto:** Simulador de Redes Tangible IDEUM
> **Autor:** Johan Sebastian Marin Rojas
> **Director:** Prof. Paulo Alonso Gaona Garcia
> **Grupo de Investigación:** Multimedia Interactiva
> **Universidad:** Universidad Distrital Francisco Jose de Caldas

Los diagramas se dividen en archivos individuales para facilitar su lectura:

| # | Diagrama | Archivo |
|:-:|----------|---------|
| 06a | Visión general de namespaces | [packages/06a-overview.md](packages/06a-overview.md) |
| 06b | Namespace Network | [packages/06b-network.md](packages/06b-network.md) |
| 06c | Namespace Tangible | [packages/06c-tangible.md](packages/06c-tangible.md) |
| 06d | Namespace Simulation | [packages/06d-simulation.md](packages/06d-simulation.md) |
| 06e | Namespace UI | [packages/06e-ui.md](packages/06e-ui.md) |
| 06f | Dependencias entre namespaces | [packages/06f-dependencies.md](packages/06f-dependencies.md) |

## Resumen de Namespaces

| Namespace | Archivos | Propósito |
|-----------|:--------:|-----------|
| `SimRedes.Network` | 10 | Núcleo de networking: nodos, enlaces, tablas, IP |
| `SimRedes.Tangible` | 5 | Integración con hardware IDEUM |
| `SimRedes.Simulation` | 23 | Actividades, escenarios, puntajes, protocolos |
| `SimRedes.UI` | 15 | Paneles, visualización, controladores de interacción |
| `SimRedes.Core` | 1 | Utilidades transversales (MemoryDiagnostics) |
| `Tests.EditMode.*` | 30 | Tests unitarios EditMode (Network, Tangible, Simulation, UI) |

## Archivos Relacionados

- `Assets/Scripts/` — Código fuente principal
- `Assets/Editor/Tests/` — Tests unitarios EditMode
- `Assets/TangibleEngine/` — SDK externo de TangibleEngine
