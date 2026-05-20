using System;
using System.Collections.Generic;
using System.Linq;

namespace SimRedes.Network
{
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

        public static bool IsValidSubnetMask(string mask)
        {
            if (string.IsNullOrEmpty(mask)) return false;

            if (!IsValidIP(mask)) return false;

            return ValidMasks.Contains(mask);
        }

        public static string ValidateIPField(string ip)
        {
            if (string.IsNullOrEmpty(ip))
                return "IP requerida";

            if (!IsValidIP(ip))
                return "Formato invalido (ej: 192.168.1.1)";

            return null;
        }

        public static string ValidateMaskField(string mask)
        {
            if (string.IsNullOrEmpty(mask))
                return " mascara requerida";

            if (!IsValidSubnetMask(mask))
                return " mascara invalida (ej: 255.255.255.0)";

            return null;
        }

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

        public static string GetNetworkAddress(string ip, string mask)
        {
            if (!IsValidIP(ip) || !IsValidSubnetMask(mask))
                return null;

            var ipParts = ip.Split('.').Select(int.Parse).ToArray();
            var maskParts = mask.Split('.').Select(int.Parse).ToArray();

            return string.Join(".", ipParts.Select((p, i) => p & maskParts[i]).ToArray());
        }

        public static string GetBroadcastAddress(string ip, string mask)
        {
            if (!IsValidIP(ip) || !IsValidSubnetMask(mask))
                return null;

            var ipParts = ip.Split('.').Select(int.Parse).ToArray();
            var maskParts = mask.Split('.').Select(int.Parse).ToArray();

            return string.Join(".", ipParts.Select((p, i) => (p & maskParts[i]) | (~maskParts[i] & 255)).ToArray());
        }

        public static string GetGatewayFromIP(string ip)
        {
            if (!IsValidIP(ip)) return null;

            var parts = ip.Split('.').Select(int.Parse).ToArray();
            parts[3] = 1;
            return string.Join(".", parts);
        }
    }

}