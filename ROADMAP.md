# ROADMAP - SimuladorRedes IDEUM

> Este archivo es el backlog persistente del proyecto. El agente `main` lo lee al inicio de cada sesion y lo usa para guiar el trabajo. Se actualiza automaticamente a medida que se completan tareas.

---

## Estado Actual: 231 tests pasando, textos alineados con propuesta, anteproyecto de grado creado

> SceneSetup ~480L, **231 tests pasando** (209 originales + 22 nuevos de UI factories). Textos de actividades alineados con propuesta academica. Anteproyecto de Trabajo de Grado generado. Bug corregido en ConfigPanelFactory (NRE en InputFields de VLAN/ACL/NAT). **Pendiente A2** (pruebas en mesa IDEUM real con discos fisicos).

---

## Backlog

| ID | Tarea | Estado | Dependencias |
|----|-------|--------|-------------|
| **A1** | Build de prueba para IDEUM (Windows x86_64) | Completada | Ninguna |
| **A2** | Pruebas en mesa IDEUM real con discos fisicos (1-3) | **No iniciada** | A1 |
| **A4** | Suite completa de tests en Unity Editor | Completada | Ninguna |
| **B1** | Verificar persistencia rutas discos 7-18 al recargar actividad | Completada | Ninguna |
| **B2** | ShowConnectivityPanel sin SceneSetup | Completada | Ninguna |
| **B3** | Unificar RoutingSimulator (3 copias -> 1) | Completada | Ninguna |
| **B4** | Tests integracion Disco -> Tabla -> UI | Completada | Ninguna |
| **B5** | Discos 15-18 como configuracion virtual | Completada | Ninguna |
| **C1** | Migrar Input.GetKeyDown a Input System | Completada | Ninguna |
| **C2** | Tooltips/layout discos de routing (IDs 7-18) | Completada | Ninguna |
| **C3** | Logging TangibleEngine | Completada | Ninguna |
| **C4** | Animacion de conexion en enlaces | Completada | Ninguna |
| **C5** | Documentacion API TopologyManager | Completada | Ninguna |
| **C6** | Documentacion completa (diagramas + manuales) | Completada | Ninguna |

---

## Historial de Sesiones

Ver `ROADMAP_HISTORY.md` para el detalle completo de las 20 sesiones previas (Mayo 2026).
