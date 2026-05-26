# API — IPValidation

> **Namespace**: `SimRedes.Network` · **Archivo**: `Assets/Scripts/Network/IPValidation.cs`
>
> Clase estática con métodos de validación y cálculo de redes IP.

---

## Métodos

### Validación

```csharp
public static bool IsValidIP(string ip)
```
Valida que una IP tenga 4 octetos, cada uno entre 0-255, sin caracteres inválidos.
- **Retorna**: `true` si es una IPv4 válida

```csharp
public static bool IsValidSubnetMask(string mask)
```
Valida que una máscara sea CIDR válida (33 máscaras conocidas de 128.0.0.0 a 255.255.255.255).
- **Retorna**: `true` si la máscara es válida

```csharp
public static string ValidateIPField(string ip)
```
Valida campo IP para UI. Similar a `IsValidIP` pero retorna mensaje de error.
- **Retorna**: `null` si es válida, o un string con el error

```csharp
public static string ValidateMaskField(string mask)
```
Valida campo de máscara para UI.
- **Retorna**: `null` si es válida, o un string con el error

### Cálculos

```csharp
public static int GetPrefixLength(string subnetMask)
```
Convierte máscara de subred a prefijo CIDR.
- Ej: `"255.255.255.0"` → `24`
- Ej: `"255.0.0.0"` → `8`
- Retorna `0` si la máscara es inválida

```csharp
public static bool IsInSameNetwork(string ip1, string subnetMask1, string ip2)
```
Verifica si dos IPs están en la misma red, dada la máscara de la primera.
- **Retorna**: `true` si ambas IPs comparten dirección de red

```csharp
public static string GetNetworkAddress(string ip, string subnetMask)
```
Calcula la dirección de red aplicando la máscara a la IP.
- Ej: `("192.168.1.100", "255.255.255.0")` → `"192.168.1.0"`

```csharp
public static string GetBroadcastAddress(string ip, string subnetMask)
```
Calcula la dirección de broadcast.
- Ej: `("192.168.1.100", "255.255.255.0")` → `"192.168.1.255"`

```csharp
public static string GetGatewayFromIP(string ip)
```
Genera la dirección de gateway por defecto: último octeto = 1.
- Ej: `"192.168.1.100"` → `"192.168.1.1"`

---

## Tabla de Máscaras Válidas (CIDR)

| Prefijo | Máscara |
|:-------:|---------|
| /0 | 0.0.0.0 |
| /1 | 128.0.0.0 |
| /2 | 192.0.0.0 |
| /3 | 224.0.0.0 |
| ... | ... |
| /24 | 255.255.255.0 |
| /30 | 255.255.255.252 |
| /31 | 255.255.255.254 |
| /32 | 255.255.255.255 |

> Solo 33 máscaras son válidas (potencias de 2 en binario).

## Código de Ejemplo

```csharp
// Validar IP y máscara
if (IPValidation.IsValidIP("192.168.1.1") && IPValidation.IsValidSubnetMask("255.255.255.0")) {
    Debug.Log("IP y máscara válidas");
}

// Verificar si dos IPs están en la misma red
bool sameNetwork = IPValidation.IsInSameNetwork("192.168.1.10", "255.255.255.0", "192.168.1.20");
Debug.Log(sameNetwork ? "Misma red" : "Redes diferentes");

// Calcular dirección de red
string network = IPValidation.GetNetworkAddress("192.168.1.100", "255.255.255.0");
// → "192.168.1.0"

// Obtener gateway
string gateway = IPValidation.GetGatewayFromIP("192.168.1.100");
// → "192.168.1.1"
```
