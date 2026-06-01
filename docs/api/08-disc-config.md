# API — DiscConfiguration

> **Namespace**: `SimRedes.Network` · **Archivo**: `Assets/Scripts/Network/DiscConfiguration.cs`

---

## DiscConfiguration

```csharp
public class DiscConfiguration
```

Configuración estática de los 18 tipos de disco del sistema. La descripción de cada disco es visible en el panel "Leyenda de Discos" del menú principal.

### Propiedades

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `DiscId` | `int` | Identificador del disco (1-18) |
| `Type` | `DiscType` | Tipo de disco |
| `Label` | `string` | Nombre visible en español |
| `DisplayColor` | `Color` | Color representativo |
| `Description` | `string` | Descripción funcional |

### Métodos

```csharp
public static DiscConfiguration[] DefaultConfiguration
```
Array de 18 configuraciones, una por cada disco.

```csharp
public static DiscConfiguration GetConfiguration(int discId)
```
Obtiene la configuración de un disco por su ID.

---

## DiscType (Enum)

```csharp
public enum DiscType
```

| Valor | ID | Uso |
|-------|:--:|-----|
| `Router` | 1 | Dispositivo de enrutamiento |
| `Switch` | 2 | Dispositivo de conmutación L2 |
| `PC` | 3 | Host final |
| `Enlace` | 4 | Conexión (activa modo CONEXIÓN) |
| `Fallo` | 5 | Simulación de errores |
| `Protocolo` | 6 | Protocolo de enrutamiento |
| `RedDestino` | 7 | Red de destino para ruta |
| `Metrica` | 8 | Valor de métrica |
| `InterfazSalida` | 9 | Interfaz de salida |
| `ModoEnrutamiento` | 10 | Modo de enrutamiento (Static/RIP/OSPF/EIGRP) |
| `IpRoute` | 11 | Comando ip route (ruta por defecto) |
| `Destino` | 12 | IP de destino (sinónimo de RedDestino) |
| `Mascara` | 13 | Máscara de subred |
| `ProximoSalto` | 14 | Siguiente salto |
| `Vecino` | 15 | Router vecino (config. desde Actividad 5) |
| `AnunciarRed` | 16 | Red a anunciar en RIP/OSPF/EIGRP (config. desde Actividad 5) |
| `Costo` | 17 | Costo OSPF o Delay EIGRP (config. desde Actividad 5) |
| `BW` | 18 | Ancho de banda en Mbps (config. desde Actividad 5) |

---

## Mapa de Colores

| ID | Tipo | Color | Hex |
|:--:|------|-------|:----:|
| 1 | Router | Azul | `#4A90D9` |
| 2 | Switch | Cyan | `#50E3C2` |
| 3 | PC | Verde | `#7ED321` |
| 4 | Enlace | Amarillo | `#F5A623` |
| 5 | Fallo | Rojo | `#D0021B` |
| 6 | Protocolo | Magenta | `#BD10E0` |
| 7 | RedDestino | Naranja | `#F5A623` |
| 8 | Métrica | Dorado | `#F8E71C` |
| 9 | InterfazSalida | Celeste | `#4A90D9` |
| 10 | ModoEnrutamiento | Rosa | `#E83C8A` |
| 11 | IpRoute | Verde claro | `#7ED321` |
| 12 | Destino | Púrpura | `#9013FE` |
| 13 | Máscara | Azul claro | `#4A90D9` |
| 14 | PróximoSalto | Verde azulado | `#50E3C2` |
| 15 | Vecino | Naranja claro | `#F5A623` |
| 16 | AnunciarRed | Rojo claro | `#D0021B` |
| 17 | Costo | Verde lima | `#7ED321` |
| 18 | Ancho de Banda | Azul cielo | `#4A90D9` |

---

## DeviceType (Enum)

```csharp
public enum DeviceType
```

| Valor | Descripción |
|-------|-------------|
| `Router` | Dispositivo de capa 3 con tabla de enrutamiento |
| `Switch` | Dispositivo de capa 2, transparente en pathfinding |
| `PC` | Host final sin forwarding |
| `Unknown` | Tipo no reconocido (no debería ocurrir) |

---

## Código de Ejemplo

```csharp
// Obtener configuración de un disco
var config = DiscConfiguration.GetConfiguration(1);
Debug.Log($"Disco {config.DiscId}: {config.Label} ({config.Type})");

// Iterar todos los discos
foreach (var disc in DiscConfiguration.DefaultConfiguration) {
    Debug.Log($"[{disc.DiscId}] {disc.Label} → {disc.Description}");
}

// Comparar tipo
if (discType == DiscType.Router || discType == DiscType.Switch || discType == DiscType.PC) {
    // Es un disco físico que crea nodo
}
```
