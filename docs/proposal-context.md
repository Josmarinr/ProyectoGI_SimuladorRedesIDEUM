# Contexto de Propuesta Académica

> ⚠️ Este archivo NO se carga automáticamente en las sesiones del agente.
> Leer SOLO cuando el usuario pregunte "qué falta de la propuesta" o similar.

---

## Título

**Modelo de interacción a través de reconocimiento de objetos tangibles para la simulación de procesos de conectividad física y lógica de dispositivos networking**

---

## Contexto General

- Mesas IDEUM en la Universidad (80 puntos táctiles)
- Integrar objetos físicos para emular equipos de comunicaciones
- Originalmente se consideraron bloques programables (FlowBlocks) pero se decidió usar discos/PUCs
- Referencias: FlowBlocks Harvard, IDEUM Planes of Fame
- Proyecto orientado a: Grupo de Trabajo, Grupo de Investigación, Propuesta de Grado
- 6 discos (PUCs) para cargar propiedades de Router y simular conexión

---

## 1. ACTIVIDAD Grupo de Trabajo

### 1a. "Construye la Topología"
- **Objetivo**: Comprender nodos y enlaces cargando propiedades de equipos a discos
- **Discos**: 2 Router, 1 Switch, 1 PC, 1 Enlace
- **Dinámica**: Colocar discos → mesa detecta posiciones → genera topología digital → muestra estado de conectividad
- **Competencias**: Estructura de red, nodos, medios de transmisión
- ✅ **Estado**: Implementado en `BuildTopologyActivity` (actividad 0)

### 1b. "Encuentra el Fallo" (Troubleshooting Tangible)
- **Objetivo**: Desarrollar habilidades de diagnóstico
- **Material**: Cable desconectado, IP mal configurada, Máscara incorrecta, Interfaz administrativamente down
- **Dinámica**: Mesa genera escenario con fallos → estudiante descubre causa → mesa valida solución → ping exitoso
- **Competencias**: Diagnóstico, reparación, análisis de rutas, conectividad
- ✅ **Estado**: Implementado en `FindFaultActivity` (actividad 1)

---

## 2. ACTIVIDAD Grupo de Investigación

### 2a. "Tabla de Enrutamiento Tangible"
- **Objetivo**: Entender selección de rutas por prefijos y métricas
- **Discos**: Red destino, Métrica, Interfaz de salida
- **Dinámica**: Armar entrada de tabla (Red destino → Métrica → Interfaz) → se representa en pantalla → mesa simula llegada de paquetes
- **Competencias**: Tablas de enrutamiento, prefijos, selección de salida
- ✅ **Estado**: Implementado en `RoutingTablesActivity` (actividad 2)

### 2b. "Simulación de Mejor Ruta"
- **Objetivo**: Aplicar lowest cost path / longest prefix match
- **Discos**: Rutas alternativas, Métrica/prefijo, Modo de enrutamiento
- **Dinámica**: Colocar rutas alternativas → asignar métricas/prefijos → mesa simula llegada de paquete → visualiza selección y explica por qué
- **Competencias**: Algoritmo de mejor ruta, prefijos CIDR, costo del enlace
- ✅ **Estado**: Implementado en `BestRouteActivity` (actividad 3)

### 2c. "Enrutamiento Estático Tangible"
- **Objetivo**: Crear rutas estáticas mediante objetos físicos
- **Discos**: ip route, Destino, Máscara, Próximo salto
- **Dinámica**: Armar bloque "ip route destino máscara next-hop" → mesa traduce a configuración real → simula tráfico → resalta errores comunes
- **Competencias**: Configuración estática, nexthop, validación de rutas
- ✅ **Estado**: Implementado en `StaticRoutingActivity` (actividad 4)

### 2d. "Protocolo de Enrutamiento Dinámico Tangible"
- **Objetivo**: Diferenciar protocolos dinámicos (RIP, OSPF)
- **Discos**: RIP/OSPF/EIGRP, Vecino, Anunciar Red, Costo (OSPF), Bandwidth (EIGRP)
- **Dinámica**: Seleccionar protocolo → agregar redes a anunciar → mesa simula intercambio de rutas → aparecen rutas aprendidas
- **Competencias**: Protocolos dinámicos, anuncios, cálculo de métricas, convergencia
- ✅ **Estado**: Implementado en `DynamicRoutingActivity` (actividad 5). RIP, OSPF y EIGRP disponibles.

---

## 3. ACTIVIDAD Trabajo de Grado

- Plantear propuesta de anteproyecto
- Montar escenario para interactuar con discos que emulen conectividad
- Determinar estrategias de usabilidad para despliegue
- ✅ **Estado**: Anteproyecto creado en `docs/anteproyecto-grado.md`

---

## Estado General vs. Propuesta

| Actividad | Estado | Notas |
|-----------|--------|-------|
| 1a. Construye la Topología | ✅ Implementado | BuildTopologyActivity |
| 1b. Encuentra el Fallo | ✅ Implementado | FindFaultActivity |
| 2a. Tabla de Enrutamiento Tangible | ✅ Implementado | RoutingTablesActivity |
| 2b. Simulación de Mejor Ruta | ✅ Implementado | BestRouteActivity |
| 2c. Enrutamiento Estático Tangible | ✅ Implementado | StaticRoutingActivity |
| 2d. Protocolo de Enrutamiento Dinámico Tangible | ✅ Implementado | RIP/OSPF/EIGRP |
| 3. Trabajo de Grado | ✅ Anteproyecto listo | docs/anteproyecto-grado.md |
| Pruebas en mesa IDEUM (A2) | 🟡 Pendiente | Build listo, falta probar físicamente |
| Evaluación de usabilidad (SUS + pre/post-test) | ✅ Instrumentos creados | docs/instrumentos-evaluacion-usabilidad.md |

---

## Referencias Originales

- FlowBlocks: https://scholar.harvard.edu/files/chiashen/files/flowblocks_sdr_harvard.pdf
- IDEUM Planes of Fame: https://ideum.com/portfolio/planes-of-fame
- Video referencia: https://www.youtube.com/watch?v=eVKZi-DURXw
