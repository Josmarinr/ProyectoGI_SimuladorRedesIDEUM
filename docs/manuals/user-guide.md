# Manual de Usuario — SimuladorRedes IDEUM

> Simulador de redes académicas para mesas táctiles **IDEUM 55"**  
> Versión: Junio 2026 · Plataforma: Windows 10 · Resolución: 4096x2160

---

## Índice

1. [Introducción](#1-introducción)
2. [Requisitos del Sistema](#2-requisitos-del-sistema)
3. [Primeros Pasos](#3-primeros-pasos)
4. [Menú Principal](#4-menú-principal)
5. [Discos Físicos (PUCs)](#5-discos-físicos-pucs)
   - [Agregar Dispositivos sin Discos Físicos (Botones Táctiles)](#agregar-dispositivos-sin-discos-físicos-botones-táctiles)
   - [Panel de Dispositivos (Esquina Superior Izquierda)](#panel-de-dispositivos-esquina-superior-izquierda)
6. [Actividades Académicas](#6-actividades-académicas)
7. [Panel de Simulación](#7-panel-de-simulación)
8. [Configuración de IP](#8-configuración-de-ip)
9. [VLAN, ACL y NAT](#9-vlan-acl-y-nat)
10. [Ping y Conectividad](#10-ping-y-conectividad)
11. [Sistema de Puntajes](#11-sistema-de-puntajes)
12. [Solución de Problemas](#12-solución-de-problemas)

---

## 1. Introducción

**SimuladorRedes IDEUM** es una herramienta educativa para el aprendizaje de redes de computadoras. Utiliza una mesa táctil **IDEUM 55"** con **discos físicos (PUCs)** que representan dispositivos de red como routers, switches y PCs.

Los estudiantes pueden:

- **Construir topologías de red** colocando discos físicos sobre la mesa
- **Configurar direcciones IP** mediante un teclado numérico en pantalla
- **Crear rutas estáticas y dinámicas** (RIP/OSPF/EIGRP)
- **Diagnosticar y reparar fallos** de red
- **Visualizar el tráfico** con animaciones de ping
- **Configurar VLANs, ACLs y NAT**
- **Obtener una calificación** basada en su desempeño

> 💡 **Sin mesa IDEUM**: También funciona en PC normal con teclado, activando el modo Debug (ver Sección 3).

---

## 2. Requisitos del Sistema

### Para Mesa IDEUM (Producción)

| Componente | Especificación |
|------------|----------------|
| Mesa | IDEUM 55" con pantalla táctil |
| PC embebido | Windows 10, 4096x2160 |
| Servicio | TangibleEngine Service corriendo (TCP puerto 4949) |
| Discos | 3 PUCs físicos (Router, Switch, PC) + actividades para enlaces, fallos y routing |

### Para PC Normal (Testing/Desarrollo)

| Componente | Especificación |
|------------|----------------|
| SO | Windows 10 |
| Resolución | 4096x2160 (4K) |
| Teclado | Para simular discos (teclas 1-6) |
| Mouse/Táctil | Para interactuar con la UI |

### Para Desarrollo (Editor Unity)

| Componente | Especificación |
|------------|----------------|
| Unity | 6000.4.5f1 (Unity 6) |
| Plataforma | Windows, macOS (Editor) |
| Build Target | Windows x86_64 |
| Input System | Package 1.19.0 (modo Both) |

---

## 3. Primeros Pasos

### Abrir la Aplicación

1. Ejecutar `SimuladorRedes.exe`
2. Esperar a que cargue la escena principal
3. Verás el **Menú Principal** con 5 opciones

### Usar el Teclado (Modo Debug)

Si no tienes discos físicos, activa el modo de simulación por teclado:

| Tecla | Acción |
|:-----:|--------|
| **1** | Agregar Router |
| **2** | Agregar Switch |
| **3** | Agregar PC |
| **4** | Activar modo CONEXIÓN |
| **5** | Agregar Fallo |
| **C** | Limpiar simulación |
| **P** | Probar ping |
| **R** | Eliminar dispositivo seleccionado |
| **ESC** | Volver / Cerrar panel |

> ⚠️ En producción (mesa IDEUM), el modo teclado está desactivado. Usa los discos físicos.

### Navegar por los Menús

| Acción | Control |
|--------|---------|
| Navegar opciones | Flechas arriba/abajo o W/S |
| Seleccionar | Enter, Espacio, o Click |
| Ir directo a opción | Teclas 1-5 |
| Volver atrás | ESC o botón VOLVER |
| Click mouse | Funciona en todos los paneles |
| Toque táctil | Funciona en mesa IDEUM |

---

## 4. Menú Principal

Al iniciar la aplicación se muestran 5 opciones:

```
┌─────────────────────────────────────┐
│           SIMULADOR DE REDES        │
│                                     │
│  1.  INICIAR SIMULACIÓN             │
│  2.  ACTIVIDADES                    │
│  3.  PRUEBAS Y CONEXIONES           │
│  4.  CÓMO USAR                      │
│  5.  SALIR                          │
└─────────────────────────────────────┘
```

### Opción 1: INICIAR SIMULACIÓN

Entra directamente al modo de simulación libre. Aquí puedes:

- Colocar dispositivos (discos 1-3 o teclas 1-3)
- Conectar dispositivos (modo CONEXIÓN)
- Configurar IPs (click en nodo)
- Probar ping entre nodos
- Configurar VLANs, ACLs y NAT
- Ver la topología detectada automáticamente

### Opción 2: ACTIVIDADES

Panel con 7 actividades académicas numeradas (0-6):

| # | Actividad | Descripción |
|:-:|-----------|-------------|
| 0 | **Construye la Topología** | El sistema detecta automáticamente el tipo de topología que construyes |
| 1 | **Encuentra el Fallo** | El sistema genera un fallo, tú debes diagnosticarlo y repararlo |
| 2 | **Tabla de Enrutamiento Tangible** | Visualiza las tablas de routing de todos los routers |
| 3 | **Simulación de Mejor Ruta** | Elige la mejor ruta entre varias opciones (quiz de 4 escenarios) |
| 4 | **Enrutamiento Estático Tangible** | Configura rutas estáticas en los routers |
| 5 | **Protocolo de Enrutamiento Dinámico Tangible** | Simula RIP, OSPF o EIGRP y observa la convergencia |
| 6 | **Escenarios** | Carga escenarios preconfigurados con diferentes niveles |

### Opción 3: PRUEBAS Y CONEXIONES

Panel para pruebas rápidas de conectividad:

- Crea dispositivos con teclas 1-3
- Selecciona origen y destino
- Haz ping y ve los resultados

### Opción 4: LEYENDA DE DISCOS

Panel visual que lista los **18 discos** del sistema en 3 secciones:

1. **Dispositivos Físicos (1-3)**: Router, Switch, PC — los únicos que existen físicamente en la mesa IDEUM
2. **Actividades (4-6)**: Enlace, Fallo, Protocolo — manejados desde las actividades
3. **Configuración de Routing (7-18)**: 12 discos virtuales para rutas estáticas

Cada disco muestra su color, nombre y descripción. Incluye un hint sobre cómo configurar rutas usando los 4 campos básicos (Red Destino, Máscara, Next Hop, Interfaz).

### Opción 5: CÓMO USAR

Panel informativo con instrucciones detalladas en 2 columnas:

- Controles de teclado
- Descripción de actividades
- Conceptos de networking

### Opción 6: SALIR

Cierra la aplicación.

---

## 5. Discos Físicos (PUCs)

### Discos Físicos (IDs 1-3) y Virtuales (IDs 4-6)

**Solo 3 discos físicos** en la mesa IDEUM. Los IDs 4-6 se manejan desde las actividades:

| Disco | Tecla | Tipo | Color | Cómo usarlo |
|:-----:|:-----:|------|-------|-------------|
| 1 | 1 | **Router** 🟦 Azul | Colocar sobre la mesa para crear un router |
| 2 | 2 | **Switch** 🟩 Cyan | Colocar para crear un switch |
| 3 | 3 | **PC** 🟩 Verde | Colocar para crear un PC |
| 4 | 4 | **Enlace** 🟨 Amarillo | **Virtual**: se crean automáticamente al acercar nodos o manualmente en modo CONEXIÓN |
| 5 | 5 | **Fallo** 🟥 Rojo | **Virtual**: generado por Actividad 1 (Encuentra el Fallo) |
| 6 | - | **Protocolo** 🟪 Magenta | **Virtual**: seleccionado en Actividad 5 (Protocolo de Enrutamiento Dinámico Tangible) |

> 💡 En modo Debug (PC sin mesa IDEUM), las teclas 1-6 simulan todos los discos. En producción, solo los 3 físicos existen en la mesa.

### Agregar Dispositivos sin Discos Físicos (Botones Táctiles)

En el panel superior derecho (HUD de simulación), hay botones para agregar dispositivos sin necesidad de discos físicos:

| Botón | Qué hace |
|-------|----------|
| **ROUTER** | Agrega un router en una posición predefinida |
| **SWITCH** | Agrega un switch en una posición predefinida |
| **PC** | Agrega un PC en una posición predefinida |

> 💡 Útil cuando no tienes todos los discos físicos disponibles o para pruebas rápidas.

### Panel de Dispositivos (Esquina Superior Izquierda)

Muestra todos los dispositivos activos en la simulación:
- **Icono de color** según el tipo (Router, Switch, PC)
- **Nombre** del dispositivo
- **Click** en un dispositivo: abre configuración IP o inicia conexión
- **Click repetido**: elimina el dispositivo

### Auto-Conexión

Cuando colocas un disco cerca de otro (< 300px de distancia), el sistema los conecta automáticamente con un enlace.

### Discos de Configuración de Routing (IDs 7-18)

Estos discos **no existen físicamente** en la mesa IDEUM. Se configuran desde las actividades académicas mediante botones e inputs en la UI:

| ID | Dispo | Qué hace |
|:--:|-------|----------|
| 7 | **RedDestino** 🟧 | Define la red de destino de la ruta |
| 8 | **Métrica** 🟨 | Ajusta la métrica a 10 en la última ruta |
| 9 | **InterfazSalida** 🟦 | Define la interfaz de salida (G0/0-G0/3) |
| 10 | **ModoEnrutamiento** 🩷 | Cambia entre Static, RIP, OSPF y EIGRP |
| 11 | **IpRoute** 🟩 | Agrega ruta por defecto (0.0.0.0/0) |
| 12 | **Destino** 🟣 | Define la red de destino (alias de RedDestino) |
| 13 | **Máscara** 🟦 | Define la máscara de subred |
| 14 | **PróximoSalto** 🟦 | Define el next hop |
| 15 | **Vecino** 🟧 | Router vecino para enrutamiento dinámico (config. desde Actividad 5) |
| 16 | **AnunciarRed** 🟥 | Red personalizada a anunciar en RIP/OSPF/EIGRP (config. desde Actividad 5) |
| 17 | **Costo** 🟩 | Costo OSPF personalizado (config. desde Actividad 5, default=calculado por BW) |
| 18 | **BW** 🟦 | Ancho de banda para cálculo de costo OSPF (config. desde Actividad 5) |

> 💡 **Discos 15-18**: Son completamente virtuales. Se configuran desde la sección **"Config. Avanzada"** en el panel de Enrutamiento Dinámico (Actividad 5). No existen físicamente en la mesa IDEUM.

#### Cómo Configurar una Ruta (Actividad 4 — Enrutamiento Estático)

**Método 1 — AÑADIR MANUAL** (recomendado):
1. Abre la Actividad 4 (Enrutamiento Estático)
2. Presiona el botón **AÑADIR MANUAL**
3. Completa los 4 campos: Red Destino, Máscara, Next Hop, Interfaz
4. Presiona **AGREGAR**
5. La ruta aparece en la tabla de enrutamiento del router

**Método 2 — AÑADIR RUTA** (demo rápida):
1. Presiona **AÑADIR RUTA** para generar una ruta de ejemplo automática
2. La ruta se escribe directamente en el router seleccionado

---

## 6. Actividades Académicas

### 6.0 Construir Topología

El sistema detecta automáticamente qué tipo de topología estás construyendo:

| Tipo | Cómo identificarlo |
|------|--------------------|
| ⭐ **Estrella** | Un nodo central conectado a varios periféricos |
| 📏 **Bus** | Nodos en línea recta |
| ⭕ **Anillo** | Nodos conectados en ciclo cerrado |
| 🌳 **Árbol** | Estructura jerárquica con routers y switches |
| 🔁 **Malla** | Todos los nodos conectados entre sí |

El tipo aparece en el panel de información (esquina superior derecha).

### 6.1 Encuentra el Fallo

El sistema genera un fallo aleatorio. Debes diagnosticarlo y repararlo:

```
1. El sistema aplica un fallo (cable, IP, máscara, interfaz, gateway)
2. Aparece una pista en el panel de la actividad
3. Identifica el problema y presiona RESOLVER
4. El sistema verifica si lo reparaste correctamente
```

**Tipos de fallo:**

- **Cable Desconectado**: Un enlace está roto
- **IP Incorrecta**: Un router tiene IP equivocada
- **Máscara Incorrecta**: Un router tiene máscara inválida
- **Interfaz Down**: Una interfaz está administrativamente caída
- **Gateway Faltante**: Un PC no tiene gateway configurado

### 6.2 Tabla de Enrutamiento Tangible

Visualiza las tablas de routing de todos los routers en la topología:

- Muestra: Red destino, Máscara, Next Hop, Interfaz, Métrica, Protocolo
- Útil para verificar rutas configuradas con discos 7-18

### 6.3 Simulación de Mejor Ruta

Actividad tipo quiz con 4 escenarios:

```
Se muestra una IP de destino y varias rutas candidatas.
Tú debes seleccionar la MEJOR ruta según:
1. Longest Prefix Match (la más específica)
2. Lowest Metric (la de menor costo)
```

**Ejemplo**: Para llegar a `10.1.1.100`:

| Ruta | ¿Gana? | Razón |
|------|:------:|-------|
| `10.0.0.0/8` RIP 5 | ✗ | Menos específica |
| `10.1.0.0/16` OSPF 3 | ✗ | Menos específica |
| **`10.1.1.0/24` Static 1** | **✅** | **Más específica** |
| `0.0.0.0/0` Static 1 | ✗ | Default, menos prioridad |

### 6.4 Enrutamiento Estático

Configura rutas estáticas en los routers de la topología. Dos métodos disponibles:

**Método 1 — AÑADIR MANUAL** (rutas personalizadas):
1. Abre la Actividad 4 (AÑADIR RUTA abre panel lateral)
2. Presiona **AÑADIR MANUAL**
3. Ingresa: Red Destino, Máscara, Next Hop, Interfaz
4. Presiona **AGREGAR**

**Método 2 — AÑADIR RUTA** (demo rápida):
1. Presiona **AÑADIR RUTA** para generar una ruta de ejemplo
2. La ruta aparece automáticamente en el router

**Probar las rutas:**
1. Presiona **PROBAR CONECTIVIDAD**
2. El sistema busca una ruta hacia una IP aleatoria
3. Muestra éxito o fallo según la configuración

### 6.5 Enrutamiento Dinámico (RIP/OSPF/EIGRP)

Simula el intercambio de rutas entre routers:

1. Coloca al menos 2 routers con IPs configuradas
2. Selecciona **RIP** (conteo de hops) u **OSPF** (costo por enlace)
3. Presiona "INICIAR PROTOCOLO"
4. Cada 3 segundos los routers intercambian rutas
5. Cuando todos los routers conocen todas las redes → **Convergencia alcanzada** ✅
6. Prueba conectividad con PING

#### Configuración Avanzada (Discos Virtuales 15-18)

En la sección inferior del panel de Enrutamiento Dinámico encontrarás la sección **"Config. Avanzada (Discos Virtuales 15-18)"** con 4 opciones:

| Opción | Disco | Descripción |
|--------|:-----:|-------------|
| **Vecino** | 15 | Limita la propagación a un router vecino específico |
| **Anunciar Red** | 16 | Agrega una red personalizada a los anuncios RIP/OSPF/EIGRP |
| **Costo OSPF** | 17 | Sobrescribe el costo calculado por BW (default: 10) |
| **Ancho Banda** | 18 | Valor en Mbps para el cálculo de costo OSPF (fórmula: 100,000 / BW en Kbps) |

Para usar:
1. Completa el campo de texto con el valor deseado
2. Presiona **APLICAR** al lado del campo
3. Luego presiona **START** para iniciar el protocolo con la configuración
4. Los valores se aplican automáticamente al protocolo

### 6.6 Escenarios Preconfigurados

5 escenarios con niveles de dificultad creciente:

| # | Escenario | Dificultad | Descripción |
|:-:|-----------|:----------:|-------------|
| 0 | Estrella Simple | 🟢 Básico | 1 switch, 3 PCs |
| 1 | Dos Routers | 🟡 Intermedio | 2 routers, 1 switch, 2 PCs |
| 2 | Topología en Anillo | 🟡 Intermedio | 4 routers en anillo |
| 3 | Red en Árbol | 🔴 Avanzado | Router raíz + switches + PCs |
| 4 | Detectar Fallos | 🟡 Intermedio | Red con fallos preconfigurados |

---

## 7. Panel de Simulación

Cuando estás en una simulación, verás varios paneles:

### TopologyInfoPanel (Esquina superior derecha)

```
┌──────────────────────┐
│ 🌐 TOPOLOGÍA      [i] │
│────────────────────────│
│ Tipo: Estrella         │
│ Enlaces: 3             │
│                        │
│ Dispositivos:          │
│   Routers: 1           │
│   Switches: 1          │
│   PCs: 3               │
│                        │
│ [LIMPIAR] [PING]       │
│ [CONECTAR] [DESCONECT] │
│ [VLAN] [ACL] [NAT]     │
│ [VOLVER]               │
└──────────────────────┘
```

| Botón | Función |
|-------|---------|
| **LIMPIAR** | Elimina todos los dispositivos |
| **PING** | Activa modo ping (selecciona origen y destino) |
| **CONECTAR** | Activa modo conexión (click en 2 nodos para enlazar) |
| **DESCONECTAR** | Activa modo desconexión (click en 2 nodos del enlace a eliminar) |
| **VLAN** | Abre panel de configuración de VLANs |
| **ACL** | Abre panel de listas de acceso |
| **NAT** | Abre panel de traducción de direcciones |
| **VOLVER** | Vuelve al menú principal |
| **i** | Muestra panel con tipos de topología y ejemplos |

### DevicesPanel (Esquina superior izquierda)

Lista todos los dispositivos activos con su tipo, icono de color y nombre:

- **Click** en un dispositivo → abre configuración IP
- **Click repetido** en el mismo dispositivo → lo elimina (se marca en rojo)
- **Click fuera del panel** → deselecciona
- Cuando el modo **CONECTAR/DESCONECTAR** está activo:
  - El primer dispositivo seleccionado se marca en **verde** en el panel
  - En la escena, se resalta con un **borde amarillo**
  - Click en un segundo dispositivo completa la acción

### ScorePanel (Esquina inferior derecha)

Muestra la puntuación actualizada en tiempo real (cada 1 segundo).

### Cómo Usar CONECTAR/DESCONECTAR

**CONECTAR:**
1. Presiona el botón **CONECTAR** (se ilumina en verde)
2. Click en el **primer nodo** (se marca en amarillo)
3. Click en el **segundo nodo** → se crea el enlace
4. Presiona el botón otra vez para desactivar

**DESCONECTAR:**
1. Presiona el botón **DESCONECTAR**
2. Click en el **primer nodo** del enlace a eliminar
3. Click en el **segundo nodo** → se elimina el enlace
4. Presiona el botón otra vez para desactivar

---

## 8. Configuración de IP

Al hacer click en un nodo (excepto enlaces), se abre el panel de configuración IP:

```
┌────────────────────────────────┐
│   CONFIGURACIÓN IP             │
│                                │
│   Router R1                    │
│                                │
│   IP:   [192 . 168 . 1  . 1]  │
│   Mask: [255 . 255 . 255. 0]  │
│                                │
│   Red: 192.168.1.0/24          │
│                                │
│   [7] [8] [9]                  │
│   [4] [5] [6]                  │
│   [1] [2] [3]                  │
│   [0] [.] [DEL]               │
│                                │
│   [APLICAR]  [CANCELAR]        │
│   [TABLA ARP] [TABLA RUTAS]   │
└────────────────────────────────┘
```

### Funcionalidades

- **Teclado numérico compacto**: 0-9, punto (.), DEL
- **Validación en tiempo real**: El campo se valida mientras escribes
- **Info de red**: Muestra la dirección de red calculada
- **Botón TABLA ARP**: Solo aparece en **routers** — muestra la tabla ARP
- **Botón TABLA RUTAS**: Solo aparece en **routers** — muestra y permite editar la tabla de enrutamiento

### Cómo Configurar una IP

1. Click en el nodo deseado
2. Usa el teclado numérico para escribir la IP
3. Presiona TAB o click en el campo de máscara
4. Escribe la máscara
5. Presiona **APLICAR**
6. Verifica que la red calculada sea correcta

---

## 9. VLAN, ACL y NAT

### VLAN (Redes Virtuales)

Para aislar dispositivos en redes virtuales:

1. Presiona **VLAN** en el panel de topología
2. Crea una VLAN con ID (1-4094)
3. Asigna nodos a la VLAN
4. Los nodos en diferentes VLANs no pueden comunicarse

### ACL (Listas de Acceso)

Para controlar el tráfico:

1. Presiona **ACL** en el panel de topología
2. Crea una ACL con nombre
3. Agrega reglas: Permitir o Denegar tráfico por IP, puerto o protocolo
4. Las reglas se aplican automáticamente en `CheckConnectivity()`

### NAT (Traducción de Direcciones)

Para traducir IPs internas a externas:

1. Presiona **NAT** en el panel de topología
2. Configura la IP pública
3. Agrega reglas:
   - **Estática**: 1 IP interna → 1 IP externa
   - **Dinámica**: 1 IP interna → pool de IPs
   - **PAT**: Muchas internas → 1 externa (con puertos)

---

## 10. Ping y Conectividad

### Cómo Hacer Ping

Hay varias formas:

1. **Botón PING** en TopologyInfoPanel → selecciona origen y destino
2. **Tecla P** (modo debug) → prueba entre dos nodos
3. **Actividad Enrutamiento Estático** → botón PROBAR CONECTIVIDAD

### Animación de Ping

Cuando haces ping exitoso:

1. Un **paquete amarillo** aparece en el nodo origen
2. El paquete viaja por cada enlace siguiendo la ruta
3. Al llegar: el paquete se vuelve **verde** ✅ (éxito) o **rojo** ❌ (fallo)
4. Se muestra el tiempo de respuesta

### ¿Por qué Puede Fallar un Ping?

- Los nodos no están conectados físicamente
- Falta configuración IP en algún nodo
- Los nodos están en diferentes VLANs
- Una ACL bloquea el tráfico
- Falta ruta de retorno (para comunicación entre subredes)
- Una interfaz está administrativamente caída

---

## 11. Sistema de Puntajes

### Cómo se Ganan Puntos

| Acción | Puntos |
|--------|:------:|
| Tarea completada | 100 + bono de tiempo |
| Bono: menos de 2 minutos | +50 |
| Bono: entre 2 y 5 minutos | +25 |
| Bono: más de 5 minutos | 0 |
| Fallo encontrado y reparado | 120 |
| Ruta configurada | +15 cada una |
| Ping exitoso | +10 cada uno |
| Penalización | -N (variable) |

### Escala de Notas

| Puntaje Mínimo | Nota | Descripción |
|:--------------:|:----:|:-----------:|
| 500 | 5 | ⭐ Excelente |
| 400 | 4 | ✅ Bueno |
| 300 | 3 | 📘 Regular |
| 200 | 2 | ⚠️ Insuficiente |
| < 200 | 1 | ❌ Reprobado |

### Dónde Ver tu Puntaje

El puntaje aparece en:
- **ScorePanel** (esquina inferior derecha) — actualización cada 1 segundo
- **DevicePanel** (esquina superior izquierda) — en la parte inferior

---

## 12. Solución de Problemas

### Problemas Comunes

| Problema | Causa | Solución |
|----------|-------|----------|
| **Pantalla negra al abrir** | Escena incorrecta en build | Asegurar que `Main.unity` es la escena principal |
| **No puedo colocar discos** | Modo debug desactivado | En PC: activar `enableSimulation = true` en DebugDiscSimulator |
| **Los nodos no se conectan** | Distancia > 300px | Usar modo CONEXIÓN manual con botón CONECTAR |
| **No se ven las líneas de conexión** | Problema de renderizado conocido (P2) | En desarrollo — usar modo CONEXIÓN, la línea aparece pero puede no ser visible |
| **La app se cierra tras uso prolongado** | Fuga de memoria (P1) | En desarrollo — reiniciar la app si ocurre |
| **Textos sin letras/palabras incompletas** | Múltiples instancias de fuente | Ya corregido: las fuentes ahora se comparten (font cache) |
| **Ping siempre falla** | Sin ruta de retorno | Configurar rutas estáticas en ambos sentidos |
| **No veo el panel de IP** | Click en enlace (no en nodo) | Hacer click en Router, Switch o PC |
| **No aparecen TABLA ARP/RUTAS** | El nodo no es un router | Solo los routers tienen tablas |
| **Actividad no responde** | Panel bloqueado | Presionar ESC para volver y reintentar |
| **Error de compilación** | Library corrupta | Cerrar Unity, eliminar carpeta Library, reabrir |
| **UI borrosa** | Canvas Scaler incorrecto | Usar modo Expand, resolución referencia 4096x2160 |
| **Botones no responden al tacto** | Sin EventSystem en escena | Ya corregido en versión actual |
| **Discos parpadean al colocarlos** | Touch frame envía datos intermitentes | Ya corregido: antirrebote de 1s en TangibleBridge |

### Contacto y Soporte

Para reportar bugs o solicitar ayuda técnica, contacta al equipo de desarrollo.

---

> **Documentación generada:** Junio 2026  
> **Proyecto:** SimuladorRedes IDEUM · Unity 6000.4.5f1
