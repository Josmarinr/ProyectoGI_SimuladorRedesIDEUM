# Requerimientos del Sistema — SimuladorRedes IDEUM

> Requerimientos funcionales y no funcionales del sistema, con trazabilidad a código y tests.
>
> **Convención de verificación:** una casilla marcada indica un criterio cubierto por al menos un test unitario existente (ver Matriz de Trazabilidad); las casillas sin marcar están pendientes de verificación manual.

---

## Requerimientos Funcionales

### RF-01: Detección de Discos Físicos

**Descripción:** El sistema debe detectar discos físicos colocados sobre la mesa IDEUM y crear nodos virtuales correspondientes.

**Prioridad:** Crítica
**Estado:** Implementada
**Archivo:** TangibleDiscManager.cs, TangibleBridge.cs
**Test:** TestTangibleBridge, TestDiscEventHandler

**Criterios de Verificación:**
- [x] Detección de disco Router (ID 1)
- [x] Detección de disco Switch (ID 2)
- [x] Detección de disco PC (ID 3)
- [ ] Posicionamiento preciso en Canvas (4096x2160)
- [ ] Sistema de antirrebote activo

---

### RF-02: Creación Automática de Enlaces

**Descripción:** El sistema debe crear enlaces automáticamente cuando la distancia entre dispositivos es menor a un umbral configurado.

**Prioridad:** Alta
**Estado:** Implementada
**Archivo:** TopologyManager.cs, DiscEventHandler.cs
**Test:** TestTopologyManager

**Criterios de Verificación:**
- [ ] Umbral de auto-conexión: 300px
- [ ] Visualización de enlaces como líneas
- [ ] Creación de enlaces bidireccionales
- [x] Evitar duplicación de enlaces

---

### RF-03: Conexión Manual de Dispositivos

**Descripción:** El sistema debe permitir conectar dispositivos manualmente mediante el modo CONEXIÓN (tecla 4).

**Prioridad:** Media
**Estado:** Implementada
**Archivo:** LinkModeController.cs, TopologyManager.cs
**Test:** —

**Criterios de Verificación:**
- [ ] Activación con tecla 4
- [ ] Selección de origen y destino
- [ ] Creación de enlace entre seleccionados
- [ ] Cancelación con ESC

---

### RF-04: Configuración de Dirección IP

**Descripción:** El sistema debe permitir configurar direcciones IP en PCs e interfaces de router.

**Prioridad:** Alta
**Estado:** Implementada
**Archivo:** IPValidation.cs, NetworkNode.cs
**Test:** TestIPValidation

**Criterios de Verificación:**
- [x] Validación de formato IPv4
- [x] Validación de rango (0-255 por octeto)
- [x] Configuración de máscara de subred
- [ ] Asignación a PCs y interfaces de router

---

### RF-05: Ejecución de Ping

**Descripción:** El sistema debe permitir realizar pruebas de conectividad (ping) entre dispositivos con animación visual.

**Prioridad:** Alta
**Estado:** Implementada
**Archivo:** PingVisualizer.cs
**Test:** TestTopologyManager

**Criterios de Verificación:**
- [ ] Activación con tecla P
- [ ] Selección de origen y destino
- [ ] Animación de paquete viajando
- [x] Resultado: éxito/fallo
- [ ] Actualización de tablas ARP
- [ ] Tiempo de respuesta simulado

---

### RF-06: Detección Automática de Topología

**Descripción:** El sistema debe detectar automáticamente el tipo de topología de red construida por el estudiante.

**Prioridad:** Alta
**Estado:** Implementada
**Archivo:** TopologyManager.cs
**Test:** —

**Criterios de Verificación:**
- [ ] Detección de topología Estrella
- [ ] Detección de topología Bus
- [ ] Detección de topología Anillo
- [ ] Detección de topología Árbol
- [ ] Detección de topología Malla
- [ ] Mostrar nombre de topología detectada

---

### RF-07: Diagnóstico de Fallos

**Descripción:** El sistema debe cargar escenarios con fallos predefinidos y permitir al estudiante diagnosticarlos y corregirlos.

**Prioridad:** Media
**Estado:** Implementada
**Archivo:** FindFaultActivity.cs
**Test:** —

**Criterios de Verificación:**
- [ ] Fallos: cable, IP incorrecta, máscara inválida, PC sin IP
- [ ] Identificación de cada fallo por el estudiante
- [ ] Corrección de cada fallo
- [ ] Retroalimentación sobre correcciones

---

### RF-08: Visualización de Tablas de Enrutamiento

**Descripción:** El sistema debe mostrar las tablas de enrutamiento de todos los routers en la topología.

**Prioridad:** Media
**Estado:** Implementada
**Archivo:** RoutingTable.cs (clase RoutingEntry)
**Test:** TestRoutingTable

**Criterios de Verificación:**
- [x] Mostrar: destino, máscara, next-hop, métrica, interfaz
- [ ] Actualización en tiempo real
- [ ] Selección de router específico
- [x] Mostrar rutas estáticas y dinámicas

---

### RF-09: Configuración de Rutas Estáticas

**Descripción:** El sistema debe permitir configurar rutas estáticas usando discos virtuales (IDs 4-18).

**Prioridad:** Media
**Estado:** Implementada
**Archivo:** RouteBuilderState.cs
**Test:** TestRouteBuilderState, TestDiscToRouteIntegration

**Criterios de Verificación:**
- [ ] Discos virtuales disponibles (IDs 4-18)
- [x] Creación de ruta al colocar disco sobre router
- [x] Configuración de destino, máscara y next-hop
- [x] Aparición en tabla de enrutamiento
- [ ] Eliminación de ruta estática

---

### RF-10: Enrutamiento Dinámico (RIP, OSPF, EIGRP)

**Descripción:** El sistema debe simular protocolos de enrutamiento dinámico con convergencia automática.

**Prioridad:** Alta
**Estado:** Implementada
**Archivo:** DynamicRoutingProtocol.cs
**Test:** TestDynamicRoutingProtocol, TestRoutingTable

**Criterios de Verificación:**
- [x] Selección de protocolo: RIP, OSPF o EIGRP
- [ ] Intercambio de información de enrutamiento
- [ ] Convergencia de rutas
- [ ] Actualización automática de tablas
- [x] Métricas según protocolo (hop count, cost, bandwidth)

---

### RF-11: Configuración de VLAN

**Descripción:** El sistema debe permitir crear VLANs y asignar puertos a ellas.

**Prioridad:** Baja
**Estado:** Implementada
**Archivo:** VLANManager.cs
**Test:** TestVLANManager

**Criterios de Verificación:**
- [x] Creación de VLAN con ID y nombre
- [x] Asignación de puerto a VLAN
- [x] Aislamiento entre VLANs
- [ ] Visualización de VLAN asignada

---

### RF-12: Configuración de ACL

**Descripción:** El sistema debe permitir crear listas de control de acceso para filtrar tráfico.

**Prioridad:** Baja
**Estado:** Implementada
**Archivo:** ACLManager.cs
**Test:** TestACLManager

**Criterios de Verificación:**
- [x] Creación de reglas permit/deny
- [x] Definición de origen, destino y protocolo
- [ ] Aplicación a interfaces de router
- [x] Filtrado de tráfico

---

### RF-13: Configuración de NAT

**Descripción:** El sistema debe permitir configurar traducción de direcciones de red (estática, dinámica, PAT).

**Prioridad:** Baja
**Estado:** Implementada
**Archivo:** NATManager.cs
**Test:** TestNATManager

**Criterios de Verificación:**
- [x] NAT estático (una a una)
- [x] NAT dinámico (rango de IPs)
- [x] PAT (overload)
- [x] Mostrar traducciones activas
- [x] Traducción correcta de tráfico

---

### RF-14: Sistema de Puntuación

**Descripción:** El sistema debe calcular y mostrar puntajes basados en acciones del estudiante.

**Prioridad:** Media
**Estado:** Implementada
**Archivo:** ScoringSystem.cs
**Test:** TestScoringSystem

**Criterios de Verificación:**
- [x] Puntaje acumulado en tiempo real
- [x] Bonos por acciones correctas
- [ ] Calificación final (puntaje total + nota 1-5)
- [x] Desglose por categoría

---

### RF-15: Escenarios Predefinidos

**Descripción:** El sistema debe cargar topologías preconfiguradas con objetivos específicos.

**Prioridad:** Media
**Estado:** Implementada
**Archivo:** PredefinedScenarios.cs
**Test:** TestPredefinedScenarios, TestActivityLoader

**Criterios de Verificación:**
- [x] 5 escenarios disponibles
- [x] Nombre y descripción de cada uno
- [x] Carga automática de topología
- [x] Inclusión de dispositivos, enlaces y configuraciones
- [x] Objetivos del escenario

---

### RF-16: Navegación de Menú

**Descripción:** El sistema debe proveer una interfaz de menú navegable con teclado.

**Prioridad:** Alta
**Estado:** Implementada
**Archivo:** MenuNavigator.cs, UIPanelFactory.cs
**Test:** TestUIPanelFactory

**Criterios de Verificación:**
- [x] Menú con opciones claras
- [ ] Navegación con flechas y números
- [ ] Acceso a funcionalidades
- [ ] Volver al menú con ESC
- [ ] Cierre correcto de paneles

---

### RF-17: Selección de Tipo de Disco

**Descripción:** El sistema debe permitir seleccionar el tipo de disco a colocar mediante teclas.

**Prioridad:** Alta
**Estado:** Implementada
**Archivo:** TangibleDiscManager.cs
**Test:** —

**Criterios de Verificación:**
- [ ] Tecla 1: Router
- [ ] Tecla 2: Switch
- [ ] Tecla 3: PC
- [ ] Indicador del tipo seleccionado
- [ ] Disco colocado del tipo seleccionado

---

### RF-18: Eliminación de Dispositivos

**Descripción:** El sistema debe permitir eliminar dispositivos de la red.

**Prioridad:** Media
**Estado:** Implementada
**Archivo:** TopologyManager.cs
**Test:** TestTopologyManager

**Criterios de Verificación:**
- [ ] Modo eliminar con tecla R
- [ ] Eliminación al hacer clic
- [x] Eliminación de enlaces asociados
- [ ] Feedback de eliminación
- [ ] Cancelación con ESC

---

### RF-19: Simulación de Mejor Ruta

**Descripción:** El sistema debe presentar escenarios con rutas alternativas para que el estudiante seleccione la óptima.

**Prioridad:** Baja
**Estado:** Implementada
**Archivo:** BestRouteActivity.cs
**Test:** TestBestRouteActivity

**Criterios de Verificación:**
- [x] Escenario con múltiples rutas
- [ ] Mostrar métricas de cada ruta
- [ ] Selección de ruta óptima
- [ ] Validación de respuesta
- [ ] Explicación de respuesta correcta

---

### RF-20: Carga de Actividades

**Descripción:** El sistema debe permitir cargar diferentes actividades académicas (topología, fallos, mejor ruta, routing estático, routing dinámico).

**Prioridad:** Media
**Estado:** Implementada
**Archivo:** ActivityLoader.cs, *.cs (activities)
**Test:** TestActivityLoader

**Criterios de Verificación:**
- [x] Carga de actividad de topología
- [x] Carga de actividad de fallos
- [x] Carga de actividad de mejor ruta
- [x] Carga de actividad de routing estático
- [x] Carga de actividad de routing dinámico

---

## Requerimientos No Funcionales

### RNF-01: Tiempo de Respuesta

**Descripción:** El sistema debe responder a interacciones del usuario en menos de 100ms.

**Categoría:** Rendimiento
**Métrica:** Tiempo entre input del usuario y respuesta visual
**Objetivo:** < 100ms para colocación de discos, < 200ms para ping

---

### RNF-02: Compatibilidad con Hardware IDEUM

**Descripción:** El sistema debe funcionar correctamente en mesas interactivas IDEUM de 55 pulgadas.

**Categoría:** Compatibilidad
**Métrica:** Resolución soportada: 4096x2160 (4K)
**Objetivo:** Funcionalidad completa en hardware IDEUM

---

### RNF-03: Cobertura de Pruebas

**Descripción:** El sistema debe mantener una cobertura de pruebas unitarias completa.

**Categoría:** Calidad
**Métrica:** Número de pruebas y cobertura de código
**Objetivo:** 399+ pruebas, todas pasando

---

### RNF-04: Estabilidad en Sesiones Prolongadas

**Descripción:** El sistema debe operar estable durante sesiones de 45-60 minutos sin caídas.

**Categoría:** Confiabilidad
**Métrica:** Tiempo de operación sin crashes
**Objetivo:** 60 minutos sin incidents

---

### RNF-05: Documentación Técnica

**Descripción:** El sistema debe tener documentación completa de código, APIs y arquitectura.

**Categoría:** Mantenibilidad
**Métrica:** Archivos de documentación, diagramas UML
**Objetivo:** 40+ archivos, 21 diagramas Mermaid, 14 diagramas Archify

---

### RNF-06: Modularidad del Código

**Descripción:** El código debe estar organizado en namespaces claros con responsabilidades definidas.

**Categoría:** Mantenibilidad
**Métrica:** Namespaces, acoplamiento, cohesión
**Objetivo:** 6 namespaces, responsabilidades claras

---

### RNF-07: Portabilidad de Build

**Descripción:** El sistema debe generar builds ejecutables para Windows x86_64 (IDEUM) y PC normal.

**Categoría:** Portabilidad
**Métrica:** Plataformas soportadas
**Objetivo:** Build para IDEUM (Windows x86_64) + build para testing local

---

### RNF-08: Integración con TangibleEngine

**Descripción:** El sistema debe comunicarse correctamente con el TangibleEngine SDK via TCP.

**Categoría:** Integración
**Métrica:** Conexión TCP, protocolo TUIO
**Objetivo:** Comunicación estable en puerto 4949

---

### RNF-09: Accesibilidad de Interfaz

**Descripción:** La interfaz debe ser clara y fácil de interpretar para estudiantes sin experiencia previa.

**Categoría:** Usabilidad
**Métrica:** SUS score > 70 (Bueno)
**Objetivo:** Interfaz intuitiva, minimal capacitación

---

### RNF-10: Escalabilidad de Actividades

**Descripción:** El sistema debe facilitar la adición de nuevas actividades académicas.

**Categoría:** Extensibilidad
**Métrica:** Facilidad para agregar nuevas activities
**Objetivo:** Arquitectura basada en factories, fácil extensión

---

## Matriz de Trazabilidad: Requerimiento → Código → Test

Los IDs de test citados corresponden a clases reales de `Assets/Editor/Tests/`. `—` indica que ningún test existente cubre el requerimiento (verificación manual pendiente).

| Requerimiento | Archivo(s) | Test(s) | Verificación |
|---------------|------------|---------|--------------|
| RF-01 | TangibleDiscManager.cs, TangibleBridge.cs | TestTangibleBridge, TestDiscEventHandler | Cobertura parcial |
| RF-02 | TopologyManager.cs, DiscEventHandler.cs | TestTopologyManager | Cobertura parcial |
| RF-03 | LinkModeController.cs | — | Pendiente verificación manual |
| RF-04 | IPValidation.cs, NetworkNode.cs | TestIPValidation | Cobertura parcial |
| RF-05 | PingVisualizer.cs | TestTopologyManager | Cobertura parcial |
| RF-06 | TopologyManager.cs | — | Pendiente verificación manual |
| RF-07 | FindFaultActivity.cs | — | Pendiente verificación manual |
| RF-08 | RoutingTable.cs (clase RoutingEntry) | TestRoutingTable | Cobertura parcial |
| RF-09 | RouteBuilderState.cs | TestRouteBuilderState, TestDiscToRouteIntegration | Cobertura parcial |
| RF-10 | DynamicRoutingProtocol.cs | TestDynamicRoutingProtocol, TestRoutingTable | Cobertura parcial |
| RF-11 | VLANManager.cs | TestVLANManager | Cobertura parcial |
| RF-12 | ACLManager.cs | TestACLManager | Cobertura parcial |
| RF-13 | NATManager.cs | TestNATManager | Cubierta por test |
| RF-14 | ScoringSystem.cs | TestScoringSystem | Cobertura parcial |
| RF-15 | PredefinedScenarios.cs | TestPredefinedScenarios, TestActivityLoader | Cubierta por test |
| RF-16 | MenuNavigator.cs, UIPanelFactory.cs | TestUIPanelFactory | Cobertura parcial |
| RF-17 | TangibleDiscManager.cs | — | Pendiente verificación manual |
| RF-18 | TopologyManager.cs | TestTopologyManager | Cobertura parcial |
| RF-19 | BestRouteActivity.cs | TestBestRouteActivity | Cobertura parcial |
| RF-20 | ActivityLoader.cs | TestActivityLoader | Cubierta por test |

---

## Resumen de Requerimientos

| Tipo | Total | Crítica | Alta | Media | Baja |
|------|-------|---------|------|-------|------|
| Funcionales (RF) | 20 | 1 | 7 | 8 | 4 |
| No Funcionales (RNF) | 10 | - | - | - | - |
| **Total** | **30** | **1** | **7** | **8** | **4** |

Los RNF no definen prioridad en este documento; las columnas de prioridad reflejan únicamente los RF (RF-01 a RF-20).

---

## Notas

- El problema activo P2 afecta los RNF-02 (compatibilidad) y RNF-04 (estabilidad)
- El sistema actualmente soporta 18 tipos de discos (3 físicos + 15 virtuales)
- Todos los requerimientos funcionales están implementados; la verificación automatizada es parcial y los criterios sin casilla marcada están pendientes de verificación manual (ver Matriz de Trazabilidad)

---

*Documento generado automáticamente. Última actualización: 2026-09-28.*
