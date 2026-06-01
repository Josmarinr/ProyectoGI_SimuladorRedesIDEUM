using System;
using System.Collections.Generic;
using System.Linq;

namespace SimRedes.Network
{
    /// <summary>Métodos de validación para direcciones IP, máscaras de subred y cálculos de red.</summary>
    public static class IPValidation
    {
        private static readonly string[] ValidMasks = new[]
        {
            "0.0.0.0",
            "128.0.0.0", "192.0.0.0", "224.0.0.0", "240.0.0.0",
            "248.0.0.0", "252.0.0.0", "254.0.0.0", "255.0.0.0",
            "255.128.0.0", "255.192.0.0", "255.224.0.0", "255.240.0.0",
            "255.248.0.0", "255.252.0.0", "255.254.0.0", "255.255.0.0",
            "255.255.128.0", "255.255.192.0", "255.255.224.0", "255.255.240.0",
            "255.255.248.0", "255.255.252.0", "255.255.254.0", "255.255.255.0",
            "255.255.255.128", "255.255.255.192", "255.255.255.224", "255.255.255.240",
            "255.255.255.248", "255.255.255.252", "255.255.255.254", "255.255.255.255"
        };

        /// <summary>Verifica si una cadena tiene el formato de una dirección IP IPv4 válida.</summary>
        /// <param name="ip">Dirección IP a validar.</param>
        /// <returns>True si la IP tiene 4 octetos entre 0 y 255.</returns>
        public static bool IsValidIP(string ip)
        {
            if (string.IsNullOrEmpty(ip)) return false;

            var parts = ip.Split('.');
            if (parts.Length != 4) return false;

            foreach (var part in parts)
            {
                if (!int.TryParse(part, out int num))
                    return false;
                if (num < 0 || num > 255)
                    return false;
            }

            return true;
        }

        /// <summary>Verifica si una cadena es una máscara de subred válida.</summary>
        /// <param name="mask">Máscara de subred a validar.</param>
        /// <returns>True si la máscara está en la lista de máscaras válidas.</returns>
        public static bool IsValidSubnetMask(string mask)
        {
            if (string.IsNullOrEmpty(mask)) return false;

            if (!IsValidIP(mask)) return false;

            return ValidMasks.Contains(mask);
        }

        /// <summary>Valida un campo de dirección IP y retorna un mensaje de error si es inválido.</summary>
        /// <param name="ip">Dirección IP a validar.</param>
        /// <returns>Mensaje de error o null si es válida.</returns>
        public static string ValidateIPField(string ip)
        {
            if (string.IsNullOrEmpty(ip))
                return "IP requerida";

            if (!IsValidIP(ip))
                return "Formato invalido (ej: 192.168.1.1)";

            return null;
        }

        /// <summary>Valida un campo de máscara de subred y retorna un mensaje de error si es inválido.</summary>
        /// <param name="mask">Máscara de subred a validar.</param>
        /// <returns>Mensaje de error o null si es válida.</returns>
        public static string ValidateMaskField(string mask)
        {
            if (string.IsNullOrEmpty(mask))
                return " mascara requerida";

            if (!IsValidSubnetMask(mask))
                return " mascara invalida (ej: 255.255.255.0)";

            return null;
        }

        /// <summary>Calcula la longitud del prefijo CIDR a partir de una máscara de subred.</summary>
        /// <param name="mask">Máscara de subred.</param>
        /// <returns>Longitud del prefijo (0-32).</returns>
        public static int GetPrefixLength(string mask)
        {
            if (!IsValidSubnetMask(mask)) return 0;

            int prefix = 0;
            var parts = mask.Split('.').Select(int.Parse).ToArray();

            for (int i = 0; i < parts.Length; i++)
            {
                int part = parts[i];
                while (part > 0)
                {
                    prefix += (part & 1);
                    part >>= 1;
                }
            }

            return prefix;
        }

        /// <summary>Verifica si dos direcciones IP están en la misma red aplicando la máscara indicada.</summary>
        /// <param name="ip1">Primera dirección IP.</param>
        /// <param name="mask1">Máscara de subred.</param>
        /// <param name="ip2">Segunda dirección IP.</param>
        /// <returns>True si ambas IPs pertenecen a la misma red.</returns>
        public static bool IsInSameNetwork(string ip1, string mask1, string ip2)
        {
            if (!IsValidIP(ip1) || !IsValidIP(ip2) || !IsValidSubnetMask(mask1))
                return false;

            var ip1Parts = ip1.Split('.').Select(int.Parse).ToArray();
            var ip2Parts = ip2.Split('.').Select(int.Parse).ToArray();
            var maskParts = mask1.Split('.').Select(int.Parse).ToArray();

            for (int i = 0; i < 4; i++)
            {
                if ((ip1Parts[i] & maskParts[i]) != (ip2Parts[i] & maskParts[i]))
                    return false;
            }
            return true;
        }

        /// <summary>Calcula la dirección de red aplicando la máscara a una IP.</summary>
        /// <param name="ip">Dirección IP.</param>
        /// <param name="mask">Máscara de subred.</param>
        /// <returns>Dirección de red o null si los parámetros son inválidos.</returns>
        public static string GetNetworkAddress(string ip, string mask)
        {
            if (!IsValidIP(ip) || !IsValidSubnetMask(mask))
                return null;

            var ipParts = ip.Split('.').Select(int.Parse).ToArray();
            var maskParts = mask.Split('.').Select(int.Parse).ToArray();

            return string.Join(".", ipParts.Select((p, i) => p & maskParts[i]).ToArray());
        }

        /// <summary>Calcula la dirección de broadcast a partir de una IP y su máscara.</summary>
        /// <param name="ip">Dirección IP.</param>
        /// <param name="mask">Máscara de subred.</param>
        /// <returns>Dirección de broadcast o null si los parámetros son inválidos.</returns>
        public static string GetBroadcastAddress(string ip, string mask)
        {
            if (!IsValidIP(ip) || !IsValidSubnetMask(mask))
                return null;

            var ipParts = ip.Split('.').Select(int.Parse).ToArray();
            var maskParts = mask.Split('.').Select(int.Parse).ToArray();

            return string.Join(".", ipParts.Select((p, i) => (p & maskParts[i]) | (~maskParts[i] & 255)).ToArray());
        }

        /// <summary>Obtiene la dirección del gateway por defecto a partir de una IP y máscara opcional.</summary>
        /// <param name="ip">Dirección IP.</param>
        /// <param name="mask">Máscara de subred (opcional, por defecto /24).</param>
        /// <returns>Dirección del gateway (primera IP usable de la red).</returns>
        public static string GetGatewayFromIP(string ip, string mask = null)
        {
            if (!IsValidIP(ip)) return null;

            if (!string.IsNullOrEmpty(mask) && IsValidSubnetMask(mask))
            {
                var ipParts = ip.Split('.').Select(int.Parse).ToArray();
                var maskParts = mask.Split('.').Select(int.Parse).ToArray();

                // Calcular direccion de red
                var networkParts = ipParts.Select((p, i) => p & maskParts[i]).ToArray();

                // Gateway = direccion de red + 1 (primera usable)
                var gatewayParts = new int[4];
                for (int i = 3; i >= 0; i--)
                {
                    if (i == 3)
                    {
                        gatewayParts[i] = networkParts[i] + 1;
                        if (gatewayParts[i] <= 255) break;
                        gatewayParts[i] = 0; // overflow
                    }
                    else
                    {
                        gatewayParts[i] = networkParts[i];
                    }
                }

                return string.Join(".", gatewayParts);
            }

            // Fallback: asumir /24 y usar .1
            var parts = ip.Split('.').Select(int.Parse).ToArray();
            parts[3] = 1;
            return string.Join(".", parts);
        }
    }

}