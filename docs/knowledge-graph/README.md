# Knowledge Graph — SimuladorRedes IDEUM

> Grafo de conocimiento del proyecto. Navegable en GitHub y Obsidian.

## Archivos

| # | Archivo | Propósito |
|---|---------|-----------|
| 0 | [KNOWLEDGE-GRAPH.md](KNOWLEDGE-GRAPH.md) | Nodo raíz — grafo global + tabla de búsqueda |
| 1 | [01-dependency-graph.md](01-dependency-graph.md) | Imports reales entre archivos (Mermaid) |
| 2 | [02-concept-map.md](02-concept-map.md) | Conceptos del dominio → código, tests, skills |
| 3 | [03-entry-points.md](03-entry-points.md) | "Si quiero X, debo tocar Y" |
| 4 | [04-tests-map.md](04-tests-map.md) | 399 tests → qué cubren |
| 5 | [05-bugs-history.md](05-bugs-history.md) | Bugs históricos → archivos modificados |
| 6 | [06-skills-map.md](06-skills-map.md) | 20 skills → archivos referenciados |
| 7 | [07-architectural-flows.md](07-architectural-flows.md) | 8 flujos de datos (Mermaid sequence) |

## Cómo Usar

### En GitHub
Navega los archivos directamente. Los links markdown funcionan entre archivos.

### En Obsidian
Abre la carpeta `docs/knowledge-graph/` como vault. Los links `[texto](archivo.md)` navegan entre archivos. El grafo Mermaid se renderiza nativamente.

## Navegación Rápida

```
¿Qué es esto?          → KNOWLEDGE-GRAPH.md
¿Qué depende de qué?   → 01-dependency-graph.md
¿Dónde está el código?  → 02-concept-map.md
¿Por dónde empiezo?    → 03-entry-points.md
¿Qué probar?           → 04-tests-map.md
¿Bugs similares?       → 05-bugs-history.md
¿Qué skill uso?        → 06-skills-map.md
¿Cómo fluyen datos?    → 07-architectural-flows.md
```
