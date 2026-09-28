# Historias de Usuario — SimuladorRedes IDEUM

> Historias de usuario con criterios de aceptación para cada funcionalidad del sistema.

---

## Formato de Historia

```
**US-XX: [Título]**
Como [rol], quiero [funcionalidad], para [beneficio].

Criterios de Aceptación:
- [ ] Criterio 1
- [ ] Criterio 2
- [ ] Criterio 3

Prioridad: [Alta|Media|Baja]
Estado: [Pendiente|En Progreso|Completada]
```

---

## Historias de Usuario

### US-01: Colocar Disco Físico

**Como** estudiante,
**Quiero** colocar un disco sobre la mesa IDEUM,
**Para** que el sistema detecte el tipo de dispositivo y lo muestre en pantalla.

**Criterios de Aceptación:**
- [ ] Al colocar disco Router (ID 1), aparece nodo Router en pantalla
- [ ] Al colocar disco Switch (ID 2), aparece nodo Switch en pantalla
- [ ] Al colocar disco PC (ID 3), aparece nodo PC en pantalla
- [ ] El nodo se posiciona exactamente donde está el disco físico
- [ ] Al retirar el disco, el nodo desaparece de la pantalla
- [ ] El sistema maneja antirrebote para evitar parpadeo

**Prioridad:** Alta
**Estado:** Completada

---

### US-02: Conectar Dispositivos Automáticamente

**Como** estudiante,
**Quiero** que los dispositivos cercanos se conecten automáticamente,
**Para** construir una topología de red sin configuración manual compleja.

**Criterios de Aceptación:**
- [ ] Si la distancia entre dos dispositivos es < 300px, se crea enlace automáticamente
- [ ] El enlace se muestra visualmente como línea entre los nodos
- [ ] Se puede desconectar un enlace existente
- [ ] El modo auto-conexión se puede activar/desactivar

**Prioridad:** Alta
**Estado:** Completada

---

### US-03: Conectar Dispositivos Manualmente

**Como** estudiante,
**Quiero** conectar dispositivos manualmente usando el modo CONEXIÓN,
**Para** tener control total sobre la topología de la red.

**Criterios de Aceptación:**
- [ ] Al presionar la tecla 4, se activa modo CONEXIÓN
- [ ] Al hacer clic en un dispositivo y luego en otro, se crea enlace
- [ ] Se muestra feedback visual del enlace creado
- [ ] Al presionar ESC, se cancela el modo conexión

**Prioridad:** Media
**Estado:** Completada

---

### US-04: Configurar Dirección IP

**Como** estudiante,
**Quiero** configurar la dirección IP de un PC o interfaz de router,
**Para** que los dispositivos puedan comunicarse en la red.

**Criterios de Aceptación:**
- [ ] Se puede asignar IP a cualquier PC
- [ ] Se puede asignar IP a interfaces de router
- [ ] Se valida que la IP sea correcta (formato, rango)
- [ ] Se muestra la IP configurada en el panel del dispositivo
- [ ] Se puede configurar máscara de subred

**Prioridad:** Alta
**Estado:** Completada

---

### US-05: Ejecutar Ping

**Como** estudiante,
**Quiero** hacer ping entre dos dispositivos,
**Para** verificar conectividad en la red.

**Criterios de Aceptación:**
- [ ] Al presionar la tecla P, se activa modo PING
- [ ] Se selecciona origen y destino
- [ ] Se anima el paquete viajando por la red
- [ ] Se muestra resultado (éxito/fallo)
- [ ] Se actualizan tablas ARP si es exitoso
- [ ] El ping falla si no hay ruta

**Prioridad:** Alta
**Estado:** Completada

---

### US-06: Identificar Topología Automáticamente

**Como** estudiante,
**Quiero** que el sistema detecte la topología de red,
**Para** aprender a identificar estructuras de red.

**Criterios de Aceptación:**
- [ ] Detecta topología Estrella
- [ ] Detecta topología Bus
- [ ] Detecta topología Anillo
- [ ] Detecta topología Árbol
- [ ] Detecta topología Malla
- [ ] Muestra el nombre de la topología detectada

**Prioridad:** Alta
**Estado:** Completada

---

### US-07: Diagnosticar Fallos

**Como** estudiante,
**Quiero** encontrar y corregir fallos en una red preconfigurada,
**Para** aprender a diagnosticar problemas de conectividad.

**Criterios de Aceptación:**
- [ ] Se cargan escenarios con fallos predefinidos
- [ ] Los fallos incluyen: cable desconectado, IP incorrecta, máscara inválida, PC sin IP
- [ ] El estudiante puede identificar cada fallo
- [ ] El estudiante puede corregir cada fallo
- [ ] Se muestra retroalimentación sobre correcciones

**Prioridad:** Media
**Estado:** Completada

---

### US-08: Ver Tablas de Enrutamiento

**Como** estudiante,
**Quiero** visualizar las tablas de enrutamiento de los routers,
**Para** entender cómo se aprenden y almacenan las rutas.

**Criterios de Aceptación:**
- [ ] Se muestra tabla de enrutamiento de cada router
- [ ] Se muestran: destino, máscara, next-hop, métrica, interfaz
- [ ] Se actualiza cuando cambian las rutas
- [ ] Se puede seleccionar qué router ver

**Prioridad:** Media
**Estado:** Completada

---

### US-09: Configurar Ruta Estática

**Como** estudiante,
**Quiero** configurar rutas estáticas en los routers usando discos virtuales,
**Para** aprender a definir caminos manuales en la red.

**Criterios de Aceptación:**
- [ ] Se muestran discos virtuales (IDs 4-18) para configuración
- [ ] Al colocar un disco virtual sobre un router, se crea ruta estática
- [ ] Se configura destino, máscara y next-hop
- [ ] La ruta aparece en la tabla de enrutamiento
- [ ] Se puede eliminar una ruta estática

**Prioridad:** Media
**Estado:** Completada

---

### US-10: Configurar Enrutamiento Dinámico (RIP/OSPF/EIGRP)

**Como** estudiante,
**Quiero** activar protocolos de enrutamiento dinámico,
**Para** ver cómo los routers aprenden rutas automáticamente.

**Criterios de Aceptación:**
- [ ] Se puede seleccionar protocolo: RIP, OSPF o EIGRP
- [ ] Los routers intercambian información de enrutamiento
- [ ] Se observa convergencia de rutas
- [ ] Las tablas de enrutamiento se actualizan automáticamente
- [ ] Se muestran métricas según el protocolo

**Prioridad:** Alta
**Estado:** Completada

---

### US-11: Configurar VLAN

**Como** estudiante,
**Quiero** crear y asignar VLANs a dispositivos,
**Para** aprender segmentación de red a nivel de capa 2.

**Criterios de Aceptación:**
- [ ] Se puede crear una VLAN con ID y nombre
- [ ] Se puede asignar un puerto a una VLAN
- [ ] Los dispositivos en diferentes VLANs no se comunican directamente
- [ ] Se muestra la VLAN asignada a cada interfaz

**Prioridad:** Baja
**Estado:** Completada

---

### US-12: Configurar ACL

**Como** estudiante,
**Quiero** configurar listas de control de acceso,
**Para** aprender a filtrar tráfico en la red.

**Criterios de Aceptación:**
- [ ] Se pueden crear reglas ACL (permit/deny)
- [ ] Se puede definir origen, destino y protocolo
- [ ] Las reglas se aplican a interfaces de router
- [ ] El tráfico filtrado no pasa por el router

**Prioridad:** Baja
**Estado:** Completada

---

### US-13: Configurar NAT

**Como** estudiante,
**Quiero** configurar traducción de direcciones de red,
**Para** aprender a traducir IPs privadas a públicas.

**Criterios de Aceptación:**
- [ ] Se puede configurar NAT estático (una a una)
- [ ] Se puede configurar NAT dinámico (rango de IPs)
- [ ] Se puede configurar PAT (overload)
- [ ] Se muestran traducciones activas
- [ ] El tráfico se traduce correctamente

**Prioridad:** Baja
**Estado:** Completada

---

### US-14: Ver Puntuación y Calificación

**Como** estudiante,
**Quiero** ver mi puntuación en tiempo real,
**Para** saber cómo voy en la actividad y motivarme a mejorar.

**Criterios de Aceptación:**
- [ ] Se muestra puntaje acumulado
- [ ] Se muestran bonos por acciones correctas
- [ ] Se muestra la calificación final (puntaje total + nota 1-5)
- [ ] Se muestra desglose de puntos por categoría

**Prioridad:** Media
**Estado:** Completada

---

### US-15: Seleccionar Escenario Predefinido

**Como** estudiante,
**Quiero** cargar un escenario preconfigurado,
**Para** practicar con topologías específicas sin construirla desde cero.

**Criterios de Aceptación:**
- [ ] Se muestran 5 escenarios disponibles
- [ ] Cada escenario tiene nombre y descripción
- [ ] Al seleccionar uno, se carga la topología automáticamente
- [ ] Se incluyen dispositivos, enlaces y configuraciones
- [ ] Se muestran objetivos del escenario

**Prioridad:** Media
**Estado:** Completada

---

### US-16: Navegar Menú Principal

**Como** estudiante,
**Quiero** navegar por el menú principal fácilmente,
**Para** acceder a diferentes funcionalidades del sistema.

**Criterios de Aceptación:**
- [ ] Se muestra menú con opciones claras
- [ ] Se puede navegar con teclado (flechas + números)
- [ ] Cada opción lleva a la funcionalidad correspondiente
- [ ] Se puede volver al menú con ESC
- [ ] Los paneles se cierran correctamente

**Prioridad:** Alta
**Estado:** Completada

---

### US-17: Seleccionar Tipo de Disco

**Como** estudiante,
**Quiero** presionar teclas para seleccionar tipo de disco,
**Para** elegir qué dispositivo voy a colocar en la mesa.

**Criterios de Aceptación:**
- [ ] Tecla 1: Selecciona Router
- [ ] Tecla 2: Selecciona Switch
- [ ] Tecla 3: Selecciona PC
- [ ] Se muestra indicador del tipo seleccionado
- [ ] El siguiente disco colocado será del tipo seleccionado

**Prioridad:** Alta
**Estado:** Completada

---

### US-18: Eliminar Dispositivo

**Como** estudiante,
**Quiero** eliminar un dispositivo de la red,
**Para** corregir errores o modificar la topología.

**Criterios de Aceptación:**
- [ ] Al presionar R, se entra en modo eliminar
- [ ] Al hacer clic en un dispositivo, se elimina
- [ ] Se eliminan todos los enlaces asociados
- [ ] Se muestra feedback de la eliminación
- [ ] ESC cancela el modo eliminación

**Prioridad:** Media
**Estado:** Completada

---

### US-19: Simular Mejor Ruta

**Como** estudiante,
**Quiero** responder preguntas sobre cuál es la mejor ruta,
**Para** aprender a evaluar métricas de enrutamiento.

**Criterios de Aceptación:**
- [ ] Se presenta un escenario con rutas alternativas
- [ ] Se muestran métricas de cada ruta
- [ ] El estudiante selecciona la ruta óptima
- [ ] Se valida si la respuesta es correcta
- [ ] Se muestra explicación de la respuesta correcta

**Prioridad:** Baja
**Estado:** Completada

---

### US-20: Cargar Actividad de Topología

**Como** estudiante,
**Quiero** cargar la actividad "Construye la Topología",
**Para** practicar construyendo redes desde cero.

**Criterios de Aceptación:**
- [ ] Se carga vacía (sin dispositivos preexistentes)
- [ ] El estudiante coloca discos para construir la red
- [ ] El sistema identifica la topología resultante
- [ ] Se muestra retroalimentación sobre la topología construida

**Prioridad:** Media
**Estado:** Completada

---

### US-21: Cargar Actividad de Fallos

**Como** estudiante,
**Quiero** cargar la actividad "Encuentra el Fallo",
**Para** practicar diagnosticando problemas de red.

**Criterios de Aceptación:**
- [ ] Se carga una red con fallos preconfigurados
- [ ] Los fallos son: cable, IP, máscara, PC sin IP
- [ ] El estudiante identifica cada fallo
- [ ] El estudiante corrige cada fallo
- [ ] Se muestra puntaje por fallos corregidos

**Prioridad:** Media
**Estado:** Completada

---

### US-22: Cargar Actividad de Mejor Ruta

**Como** estudiante,
**Quiero** cargar la actividad "Mejor Ruta",
**Para** practicar evaluando rutas alternativas.

**Criterios de Aceptación:**
- [ ] Se presenta un escenario con múltiples rutas
- [ ] Se muestran métricas de cada ruta
- [ ] El estudiante selecciona la mejor
- [ ] Se valida la respuesta
- [ ] Se muestra explicación

**Prioridad:** Baja
**Estado:** Completada

---

### US-23: Cargar Actividad de Routing Estático

**Como** estudiante,
**Quiero** cargar la actividad "Enrutamiento Estático Tangible",
**Para** practicar configurando rutas estáticas con discos.

**Criterios de Aceptación:**
- [ ] Se carga una red sin rutas estáticas
- [ ] El estudiante usa discos virtuales para crear rutas
- [ ] Se verifica conectividad después de configurar
- [ ] Se muestra puntaje por rutas configuradas

**Prioridad:** Media
**Estado:** Completada

---

### US-24: Cargar Actividad de Routing Dinámico

**Como** estudiante,
**Quiero** cargar la actividad "Enrutamiento Dinámico",
**Para** practicar con protocolos RIP, OSPF y EIGRP.

**Criterios de Aceptación:**
- [ ] Se carga una red con múltiples routers
- [ ] Se activa un protocolo de enrutamiento dinámico
- [ ] Se observa convergencia de rutas
- [ ] Se verifican tablas de enrutamiento
- [ ] Se compara rendimiento entre protocolos

**Prioridad:** Alta
**Estado:** Completada

---

## Matriz de Trazabilidad

| Historia | Archivo(s) Relevante(s) | Test(s) |
|----------|-------------------------|---------|
| US-01 | TangibleDiscManager.cs, TangibleBridge.cs | TestTangibleBridge, TestDiscEventHandler |
| US-02 | TopologyManager.cs, DiscEventHandler.cs | TestTopologyManager |
| US-03 | LinkModeController.cs, TopologyManager.cs | — |
| US-04 | IPValidation.cs, NetworkNode.cs | TestIPValidation |
| US-05 | PingVisualizer.cs, TopologyManager.cs (CheckConnectivity) | TestTopologyManager |
| US-06 | TopologyManager.cs | — |
| US-07 | FindFaultActivity.cs | — |
| US-08 | RoutingTable.cs (clase RoutingEntry) | TestRoutingTable |
| US-09 | RouteBuilderState.cs | TestRouteBuilderState, TestDiscToRouteIntegration |
| US-10 | DynamicRoutingProtocol.cs | TestDynamicRoutingProtocol |
| US-11 | VLANManager.cs | TestVLANManager |
| US-12 | ACLManager.cs | TestACLManager |
| US-13 | NATManager.cs | TestNATManager |
| US-14 | ScoringSystem.cs | TestScoringSystem |
| US-15 | PredefinedScenarios.cs | TestPredefinedScenarios, TestActivityLoader |
| US-16 | MenuNavigator.cs, UIPanelFactory.cs | TestUIPanelFactory |
| US-17 | TangibleDiscManager.cs | — |
| US-18 | TopologyManager.cs | TestTopologyManager |
| US-19 | BestRouteActivity.cs | TestBestRouteActivity |
| US-20 | BuildTopologyActivity.cs | TestActivityLoader |
| US-21 | FindFaultActivity.cs | TestActivityLoader, TestScoringSystem |
| US-22 | BestRouteActivity.cs | TestActivityLoader, TestBestRouteActivity |
| US-23 | StaticRoutingActivity.cs | TestActivityLoader, TestDiscToRouteIntegration |
| US-24 | DynamicRoutingActivity.cs | TestActivityLoader, TestDynamicRoutingProtocol |

Los IDs de test citados corresponden a clases reales de `Assets/Editor/Tests/`. `—` indica que ningún test existente cubre la historia (verificación manual pendiente).

---

*Documento generado automáticamente. Última actualización: 2026-09-28.*
