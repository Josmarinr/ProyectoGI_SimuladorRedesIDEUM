# ROADMAP - SimuladorRedes IDEUM

> Este archivo es el backlog persistente del proyecto. El agente `main` lo lee al inicio de cada sesion y lo usa para guiar el trabajo. Se actualiza automaticamente a medida que se completan tareas.

---

## Estado Actual: 328 tests, bugfix FindFaultActivity (discos tactiles en escenarios 3+)

> Tras 22 sesiones: **328 tests**, EIGRP, Input migration, SUS, CODE_INDEX.md. Sesion 23:
> - **Fix FindFaultActivity**: discos tactiles no se actualizaban en escenarios 3+
>   - Raiz: Start() ejecutaba LoadScenario() antes de conectar referencias UI
>   - Fix: ConnectUI() llamada post-panel + FindUIReferences() fallback + restart en reingreso
>   - Fix: TangibleDiscManager.ClearAllDiscs() al cambiar de escenario
> - **CODE_INDEX.md**: indice compacto de 46 scripts (~18K tokens, bajo demanda)
> - **Pendiente A2**: Pruebas en mesa IDEUM real con discos fisicos

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
| **D1** | Migrar ultimos usos Input Manager (Mouse) | Completada | C1 |
| **D2** | Eliminar CreateStatusPanel obsoleto | Completada | Ninguna |
| **D3** | Tests ACLManager, NATManager, VLANManager, ARPTable | Completada | Ninguna |
| **D4** | Implementar protocolo EIGRP | Completada | Ninguna |
| **D5** | Instrumentos evaluacion usabilidad (SUS + pre/post-test) | Completada | Ninguna |
| **D6** | Fix layout DynamicRoutingPanel — campos config superpuestos | Completada | Ninguna |
| **D7** | Fix feedbackText — mensaje superpuesto con botones OSPF/EIGRP | Completada | Ninguna |
| **D8** | Fix feedbackText StaticRouting — mensaje superpuesto con botones AÑADIR MANUAL/RUTA/TEST | Completada | Ninguna |
| **D9** | Fix RoutingTablesActivity — Clear eliminado, header corregido, 3 mejoras menores | Completada | Ninguna |
| **E1** | Documentar codigo: TopologyManager.cs (29 metodos) | Completada | Ninguna |
| **E2** | Documentar codigo: SceneSetup.cs (19 metodos) | Completada | Ninguna |
| **E3** | Documentar codigo: 3 factories UI (UIPanel, ActivityPanel, ConfigPanel) | Completada | Ninguna |
| **E4** | Documentar codigo: UIComponents.cs | Completada | Ninguna |
| **E5** | Documentar codigo: ActivityLoader.cs | Completada | Ninguna |
| **E6** | Documentar codigo: clases de red (RoutingTable, IPValidation, NetworkNode, ACL, NAT, VLAN) | Completada | Ninguna |
| **E7** | Documentar codigo: actividades (BestRoute, BuildTopology, FindFault, StaticRouting, RoutingTables) | Completada | Ninguna |
| **E8** | Documentar codigo: modulo Tangible (TangibleBridge, DiscManager, DebugSimulator, DiscEventHandler) | Completada | Ninguna |
| **E9** | Documentar codigo: resto (ScoringSystem, SceneCleanup, MenuNavigator, RoutingProtocols) | Completada | Ninguna |

---

## Historial de Sesiones

Ver `ROADMAP_HISTORY.md` para el detalle completo de las 20 sesiones previas (Mayo 2026).
