# Requerimientos del Sistema — SimuladorRedes IDEUM

> Requerimientos funcionales y no funcionales del sistema, con trazabilidad a código y tests.

---

## Requerimientos Funcionales

### RF-01: Detección de Discos Físicos

**Descripción:** El sistema debe detectar discos físicos colocados sobre la mesa IDEUM y crear nodos virtuales correspondientes.

**Prioridad:** Crítica
**Estado:** Implementada
**Archivo:** TangibleDiscManager.cs, TangibleBridge.cs
**Test:** Test_TangibleDiscManager

**Criterios de Verificación:**
- [ ] Detección de disco Router (ID 1)
- [ ] Detección de disco Switch (ID 2)
- [ ] Detección de disco PC (ID 3)
- [ ] Posicionamiento preciso en Canvas (4096x2160)
- [ ] Sistema de antirrebote activo

---

### RF-02: Creación Automática de Enlaces

**Descripción:** El sistema debe crear enlaces automáticamente cuando la distancia entre dispositivos es menor a un umbral configurado.

**Prioridad:** Alta
**Estado:** Implementada
**Archivo:** TopologyManager.cs, DiscEventHandler.cs
**Test:** Test_TopologyManager

**Criterios de Verificación:**
- [ ] Umbral de auto-conexión: 300px
- [ ] Visualización de enlaces como líneas
- [ ] Creación de enlaces bidireccionales
- [ ] Evitar duplicación de enlaces

---

### RF-03: Conexión Manual de Dispositivos

**Descripción:** El sistema debe permitir conectar dispositivos manualmente mediante el modo CONEXIÓN (tecla 4).

**Prioridad:** Media
**Estado:** Implementada
**Archivo:** LinkModeController.cs, TopologyManager.cs
**Test:** Test_LinkMode

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
**Test:** Test_IPValidation

**Criterios de Verificación:**
- [ ] Validación de formato IPv4
- [ ] Validación de rango (0-255 por octeto)
- [ ] Configuración de máscara de subred
- [ ] Asignación a PCs y interfaces de router

---

### RF-05: Ejecución de Ping

**Descripción:** El sistema debe permitir realizar pruebas de conectividad (ping) entre dispositivos con animación visual.

**Prioridad:** Alta
**Estado:** Implementada
**Archivo:** PingVisualizer.cs
**Test:** Test_Ping

**Criterios de Verificación:**
- [ ] Activación con tecla P
- [ ] Selección de origen y destino
- [ ] Animación de paquete viajando
- [ ] Resultado: éxito/fallo
- [ ] Actualización de tablas ARP
- [ ] Tiempo de respuesta simulado

---

### RF-06: Detección Automática de Topología

**Descripción:** El sistema debe detectar automáticamente el tipo de topología de red construida por el estudiante.

**Prioridad:** Alta
**Estado:** Implementada
**Archivo:** TopologyManager.cs
**Test:** Test_TopologyDetection

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
**Archivo:** FaultDetectionActivity.cs
**Test:** Test_FaultDetection

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
**Archivo:** RoutingTable.cs, RoutingEntry.cs
**Test:** Test_RoutingTable

**Criterios de Verificación:**
- [ ] Mostrar: destino, máscara, next-hop, métrica, interfaz
- [ ] Actualización en tiempo real
- [ ] Selección de router específico
- [ ] Mostrar rutas estáticas y dinámicas

---

### RF-09: Configuración de Rutas Estáticas

**Descripción:** El sistema debe permitir configurar rutas estáticas usando discos virtuales (IDs 4-18).

**Prioridad:** Media
**Estado:** Implementada
**Archivo:** RouteBuilderState.cs
**Test:** Test_StaticRouting

**Criterios de Verificación:**
- [ ] Discos virtuales disponibles (IDs 4-18)
- [ ] Creación de ruta al colocar disco sobre router
- [ ] Configuración de destino, máscara y next-hop
- [ ] Aparición en tabla de enrutamiento
- [ ] Eliminación de ruta estática

---

### RF-10: Enrutamiento Dinámico (RIP, OSPF, EIGRP)

**Descripción:** El sistema debe simular protocolos de enrutamiento dinámico con convergencia automática.

**Prioridad:** Alta
**Estado:** Implementada
**Archivo:** DynamicRoutingProtocol.cs
**Test:** Test_DynamicRouting

**Criterios de Verificación:**
- [ ] Selección de protocolo: RIP, OSPF o EIGRP
- [ ] Intercambio de información de enrutamiento
- [ ] Convergencia de rutas
- [ ] Actualización automática de tablas
- [ ] Métricas según protocolo (hop count, cost, bandwidth)

---

### RF-11: Configuración de VLAN

**Descripción:** El sistema debe permitir crear VLANs y asignar puertos a ellas.

**Prioridad:** Baja
**Estado:** Implementada
**Archivo:** VLANManager.cs
**Test:** Test_VLAN

**Criterios de Verificación:**
- [ ] Creación de VLAN con ID y nombre
- [ ] Asignación de puerto a VLAN
- [ ] Aislamiento entre VLANs
- [ ] Visualización de VLAN asignada

---

### RF-12: Configuración de ACL

**Descripción:** El sistema debe permitir crear listas de control de acceso para filtrar tráfico.

**Prioridad:** Baja
**Estado:** Implementada
**Archivo:** ACLManager.cs
**Test:** Test_ACL

**Criterios de Verificación:**
- [ ] Creación de reglas permit/deny
- [ ] Definición de origen, destino y protocolo
- [ ] Aplicación a interfaces de router
- [ ] Filtrado de tráfico

---

### RF-13: Configuración de NAT

**Descripción:** El sistema debe permitir configurar traducción de direcciones de red (estática, dinámica, PAT).

**Prioridad:** Baja
**Estado:** Implementada
**Archivo:** NATManager.cs
**Test:** Test_NAT

**Criterios de Verificación:**
- [ ] NAT estático (una a una)
- [ ] NAT dinámico (rango de IPs)
- [ ] PAT (overload)
- [ ] Mostrar traducciones activas
- [ ] Traducción correcta de tráfico

---

### RF-14: Sistema de Puntuación

**Descripción:** El sistema debe calcular y mostrar puntajes basados en acciones del estudiante.

**Prioridad:** Media
**Estado:** Implementada
**Archivo:** ScoringSystem.cs
**Test:** Test_Scoring

**Criterios de Verificación:**
- [ ] Puntaje acumulado en tiempo real
- [ ] Bonos por acciones correctas
- [ ] Calificación final (0-100)
- [ ] Desglose por categoría

---

### RF-15: Escenarios Predefinidos

**Descripción:** El sistema debe cargar topologías preconfiguradas con objetivos específicos.

**Prioridad:** Media
**Estado:** Implementada
**Archivo:** PredefinedScenarios.cs
**Test:** Test_Scenarios

**Criterios de Verificación:**
- [ ] 5 escenarios disponibles
- [ ] Nombre y descripción de cada uno
- [ ] Carga automática de topología
- [ ] Inclusión de dispositivos, enlaces y configuraciones
- [ ] Objetivos del escenario

---

### RF-16: Navegación de Menú

**Descripción:** El sistema debe提供 una interfaz de menú navegable con teclado.

**Prioridad:** Alta
**Estado:** Implementada
**Archivo:** MenuNavigator.cs, UIPanelFactory.cs
**Test:** Test_Menu

**Criterios de Verificación:**
- [ ] Menú con opciones claras
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
**Test:** Test_DiscSelection

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
**Test:** Test_NodeRemoval

**Criterios de Verificación:**
- [ ] Modo eliminar con tecla R
- [ ] Eliminación al hacer clic
- [ ] Eliminación de enlaces asociados
- [ ] Feedback de eliminación
- [ ] Cancelación con ESC

---

### RF-19: Simulación de Mejor Ruta

**Descripción:** El sistema debe presentar escenarios con rutas alternativas para que el estudiante seleccione la óptima.

**Prioridad:** Baja
**Estado:** Implementada
**Archivo:** BestRouteActivity.cs
**Test:** Test_BestRoute

**Criterios de Verificación:**
- [ ] Escenario con múltiples rutas
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
**Test:** Test_ActivityLoader

**Criterios de Verificación:**
- [ ] Carga de actividad de topología
- [ ] Carga de actividad de fallos
- [ ] Carga de actividad de mejor ruta
- [ ] Carga de actividad de routing estático
- [ ] Carga de actividad de routing dinámico

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
**Objetivo:** 328+ pruebas, todas pasando

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
**Objetivo:** 40+ archivos, 21 diagramas Mermaid, 9 diagramas Archify

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

| Requerimiento | Archivo(s) | Test(s) | Estado |
|---------------|------------|---------|--------|
| RF-01 | TangibleDiscManager.cs, TangibleBridge.cs | Test_TangibleDiscManager | ✅ |
| RF-02 | TopologyManager.cs, DiscEventHandler.cs | Test_TopologyManager | ✅ |
| RF-03 | LinkModeController.cs | Test_LinkMode | ✅ |
| RF-04 | IPValidation.cs, NetworkNode.cs | Test_IPValidation | ✅ |
| RF-05 | PingVisualizer.cs | Test_Ping | ✅ |
| RF-06 | TopologyManager.cs | Test_TopologyDetection | ✅ |
| RF-07 | FaultDetectionActivity.cs | Test_FaultDetection | ✅ |
| RF-08 | RoutingTable.cs, RoutingEntry.cs | Test_RoutingTable | ✅ |
| RF-09 | RouteBuilderState.cs | Test_StaticRouting | ✅ |
| RF-10 | DynamicRoutingProtocol.cs | Test_DynamicRouting | ✅ |
| RF-11 | VLANManager.cs | Test_VLAN | ✅ |
| RF-12 | ACLManager.cs | Test_ACL | ✅ |
| RF-13 | NATManager.cs | Test_NAT | ✅ |
| RF-14 | ScoringSystem.cs | Test_Scoring | ✅ |
| RF-15 | PredefinedScenarios.cs | Test_Scenarios | ✅ |
| RF-16 | MenuNavigator.cs, UIPanelFactory.cs | Test_Menu | ✅ |
| RF-17 | TangibleDiscManager.cs | Test_DiscSelection | ✅ |
| RF-18 | TopologyManager.cs | Test_NodeRemoval | ✅ |
| RF-19 | BestRouteActivity.cs | Test_BestRoute | ✅ |
| RF-20 | ActivityLoader.cs | Test_ActivityLoader | ✅ |

---

## Resumen de Requerimientos

| Tipo | Total | Crítica | Alta | Media | Baja |
|------|-------|---------|------|-------|------|
| Funcionales (RF) | 20 | 1 | 6 | 9 | 4 |
| No Funcionales (RNF) | 10 | - | 3 | 4 | 3 |
| **Total** | **30** | **1** | **9** | **13** | **7** |

---

## Notas

- El problema activo P2 afecta los RNF-02 (compatibilidad) y RNF-04 (estabilidad)
- El sistema actualmente soporta 18 tipos de discos (3 físicos + 15 virtuales)
- Todos los requerimientos funcionales están implementados y probados

---

*Documento generado automáticamente. Última actualización: 2026-09-07.*
