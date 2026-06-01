using System.Collections.Generic;
using UnityEngine;

namespace SimRedes.Network
{
    /// <summary>Tipo de traducción NAT: Static, Dynamic o PAT.</summary>
    public enum NATType
    {
        Static,
        Dynamic,
        PAT
    }

    /// <summary>Entrada individual en la tabla de traducción NAT.</summary>
    public class NATEntry
    {
        /// <summary>Dirección IP interna (privada).</summary>
        public string InternalIP { get; set; }
        /// <summary>Dirección IP externa (pública).</summary>
        public string ExternalIP { get; set; }
        /// <summary>Puerto interno (opcional, usado en PAT).</summary>
        public int? InternalPort { get; set; }
        /// <summary>Puerto externo (opcional, usado en PAT).</summary>
        public int? ExternalPort { get; set; }
        /// <summary>Protocolo de la traducción.</summary>
        public string Protocol { get; set; }
        /// <summary>Tipo de NAT (Static, Dynamic, PAT).</summary>
        public NATType Type { get; set; }

        /// <summary>Retorna una representación textual de la entrada NAT.</summary>
        /// <returns>Formato "IP:Puerto → IP:Puerto" o "IP → IP".</returns>
        public override string ToString()
        {
            if (InternalPort.HasValue && ExternalPort.HasValue)
                return $"{InternalIP}:{InternalPort} → {ExternalIP}:{ExternalPort}";
            return $"{InternalIP} → {ExternalIP}";
        }
    }

    /// <summary>Administra la traducción de direcciones de red (NAT) estática, dinámica y PAT.</summary>
    public class NATManager
    {
        private List<NATEntry> natTable = new List<NATEntry>();
        private string publicIP = "200.100.50.1";
        private string routerIP = "192.168.1.1";
        private int nextPort = 10000;
        /// <summary>Indica si la traducción NAT está habilitada.</summary>
        public bool IsEnabled { get; set; }

        /// <summary>Establece la dirección IP pública del router NAT.</summary>
        /// <param name="ip">Dirección IP pública.</param>
        public void SetPublicIP(string ip)
        {
            publicIP = ip;
            UnityEngine.Debug.Log($"[NAT] Public IP set to: {ip}");
        }

        /// <summary>Establece la dirección IP interna del router.</summary>
        /// <param name="ip">Dirección IP del router.</param>
        public void SetRouterIP(string ip)
        {
            routerIP = ip;
        }

        /// <summary>Obtiene la dirección IP interna del router.</summary>
        /// <returns>Dirección IP del router.</returns>
        public string GetRouterIP()
        {
            return routerIP;
        }

        /// <summary>Agrega una traducción NAT estática uno a uno.</summary>
        /// <param name="internalIP">Dirección IP interna.</param>
        /// <param name="externalIP">Dirección IP externa (pública).</param>
        public void AddStaticNAT(string internalIP, string externalIP)
        {
            var entry = new NATEntry
            {
                InternalIP = internalIP,
                ExternalIP = externalIP,
                Type = NATType.Static,
                Protocol = "IP"
            };

            natTable.Add(entry);
            UnityEngine.Debug.Log($"[NAT] Static: {internalIP} → {externalIP}");
        }

        /// <summary>Agrega una traducción NAT dinámica (asigna IP externa del pool).</summary>
        /// <param name="internalIP">Dirección IP interna a traducir.</param>
        public void AddDynamicNAT(string internalIP)
        {
            string externalIP = $"{publicIP.Split('.')[0]}.{publicIP.Split('.')[1]}.{publicIP.Split('.')[2]}.{nextPort % 255}";
            nextPort++;

            var entry = new NATEntry
            {
                InternalIP = internalIP,
                ExternalIP = externalIP,
                Type = NATType.Dynamic,
                Protocol = "IP"
            };

            natTable.Add(entry);
            UnityEngine.Debug.Log($"[NAT] Dynamic: {internalIP} → {externalIP}");
        }

        /// <summary>Agrega una traducción PAT (puerto a puerto) usando la IP pública.</summary>
        /// <param name="internalIP">Dirección IP interna.</param>
        /// <param name="internalPort">Puerto interno.</param>
        /// <param name="protocol">Protocolo (TCP por defecto).</param>
        public void AddPAT(string internalIP, int internalPort, string protocol = "TCP")
        {
            string externalIP = publicIP;
            int externalPort = nextPort++;

            var entry = new NATEntry
            {
                InternalIP = internalIP,
                ExternalIP = externalIP,
                InternalPort = internalPort,
                ExternalPort = externalPort,
                Type = NATType.PAT,
                Protocol = protocol
            };

            natTable.Add(entry);
            UnityEngine.Debug.Log($"[NAT] PAT: {internalIP}:{internalPort} → {externalIP}:{externalPort}");
        }

        /// <summary>Busca una entrada NAT por dirección IP interna.</summary>
        /// <param name="internalIP">Dirección IP interna.</param>
        /// <param name="port">Puerto interno (opcional).</param>
        /// <returns>Entrada NAT encontrada o null.</returns>
        public NATEntry LookupInternal(string internalIP, int? port = null)
        {
            foreach (var entry in natTable)
            {
                if (entry.InternalIP == internalIP)
                {
                    if (port.HasValue && entry.InternalPort.HasValue && entry.InternalPort == port)
                        return entry;
                    if (!port.HasValue)
                        return entry;
                }
            }
            return null;
        }

        /// <summary>Busca una entrada NAT por dirección IP externa.</summary>
        /// <param name="externalIP">Dirección IP externa.</param>
        /// <param name="port">Puerto externo (opcional).</param>
        /// <returns>Entrada NAT encontrada o null.</returns>
        public NATEntry LookupExternal(string externalIP, int? port = null)
        {
            foreach (var entry in natTable)
            {
                if (entry.ExternalIP == externalIP)
                {
                    if (port.HasValue && entry.ExternalPort.HasValue && entry.ExternalPort == port)
                        return entry;
                    if (!port.HasValue)
                        return entry;
                }
            }
            return null;
        }

        /// <summary>Verifica si un paquete puede ser traducido según las reglas NAT configuradas.</summary>
        /// <param name="sourceIP">Dirección IP de origen.</param>
        /// <param name="destIP">Dirección IP de destino.</param>
        /// <param name="sourcePort">Puerto de origen (opcional).</param>
        /// <param name="destPort">Puerto de destino (opcional).</param>
        /// <param name="isOutgoing">True si es tráfico saliente, false si es entrante.</param>
        /// <returns>True si el paquete tiene una traducción válida.</returns>
        public bool TranslatePacket(string sourceIP, string destIP, int? sourcePort = null, int? destPort = null, bool isOutgoing = true)
        {
            if (isOutgoing)
            {
                var entry = LookupInternal(sourceIP, sourcePort);
                if (entry != null)
                {
                    return true;
                }
            }
            else
            {
                if (destPort.HasValue)
                {
                    var entry = LookupExternal(destIP, destPort);
                    if (entry != null)
                    {
                        return true;
                    }
                }
                else
                {
                    foreach (var entry in natTable)
                    {
                        if (entry.ExternalIP == destIP && entry.Type == NATType.Static)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>Elimina una entrada específica de la tabla NAT.</summary>
        /// <param name="entry">Entrada a eliminar.</param>
        public void RemoveEntry(NATEntry entry)
        {
            natTable.Remove(entry);
            UnityEngine.Debug.Log($"[NAT] Removed entry: {entry}");
        }

        /// <summary>Limpia todas las entradas de la tabla NAT.</summary>
        public void ClearNAT()
        {
            natTable.Clear();
            UnityEngine.Debug.Log("[NAT] Table cleared");
        }

        /// <summary>Obtiene una copia de la tabla NAT completa.</summary>
        /// <returns>Lista de entradas NAT.</returns>
        public List<NATEntry> GetNATTable()
        {
            return new List<NATEntry>(natTable);
        }

        /// <summary>Obtiene un resumen textual de la configuración NAT.</summary>
        /// <returns>Cadena con IPs y entradas de la tabla NAT.</returns>
        public string GetNATSummary()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"NAT Configuration:");
            sb.AppendLine($"  Public IP: {publicIP}");
            sb.AppendLine($"  Router IP: {routerIP}");
            sb.AppendLine($"  Entries: {natTable.Count}");

            if (natTable.Count > 0)
            {
                sb.AppendLine("  NAT Table:");
                foreach (var entry in natTable)
                {
                    sb.AppendLine($"    {entry.Type}: {entry}");
                }
            }

            return sb.ToString();
        }

        /// <summary>Traduce una dirección IP interna a su equivalente externo.</summary>
        /// <param name="internalIP">Dirección IP interna.</param>
        /// <returns>Dirección externa traducida o la misma IP si NAT está deshabilitado.</returns>
        public string TranslateInternalToExternal(string internalIP)
        {
            if (!IsEnabled)
                return internalIP;

            foreach (var entry in natTable)
            {
                if (entry.InternalIP == internalIP)
                {
                    if (entry.Type == NATType.Static)
                        return entry.ExternalIP;

                    if (entry.Type == NATType.PAT && entry.ExternalPort.HasValue)
                        return $"{publicIP}:{entry.ExternalPort}";
                }
            }

            var dynamicEntry = new NATEntry
            {
                InternalIP = internalIP,
                ExternalIP = publicIP,
                ExternalPort = nextPort++,
                Type = NATType.Dynamic
            };
            natTable.Add(dynamicEntry);
            return $"{publicIP}:{dynamicEntry.ExternalPort}";
        }

        /// <summary>Traduce una dirección IP externa a su equivalente interno.</summary>
        /// <param name="externalIP">Dirección IP externa (puede incluir puerto).</param>
        /// <returns>Dirección interna traducida o la misma IP si NAT está deshabilitado.</returns>
        public string TranslateExternalToInternal(string externalIP)
        {
            if (!IsEnabled)
                return externalIP;

            string ipOnly = externalIP.Contains(":") ? externalIP.Split(':')[0] : externalIP;
            int? port = externalIP.Contains(":") ? int.Parse(externalIP.Split(':')[1]) : (int?)null;

            foreach (var entry in natTable)
            {
                if (entry.Type == NATType.Static && entry.ExternalIP == ipOnly)
                    return entry.InternalIP;

                if (entry.Type == NATType.PAT && entry.ExternalPort == port)
                    return entry.InternalIP;

                if (entry.Type == NATType.Dynamic && entry.ExternalIP == ipOnly)
                    return entry.InternalIP;
            }

            return externalIP;
        }
    }
}