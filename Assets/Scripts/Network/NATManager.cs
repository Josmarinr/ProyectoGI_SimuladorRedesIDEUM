using System.Collections.Generic;
using UnityEngine;

namespace SimRedes.Network
{
    public enum NATType
    {
        Static,
        Dynamic,
        PAT
    }

    public class NATEntry
    {
        public string InternalIP { get; set; }
        public string ExternalIP { get; set; }
        public int? InternalPort { get; set; }
        public int? ExternalPort { get; set; }
        public string Protocol { get; set; }
        public NATType Type { get; set; }

        public override string ToString()
        {
            if (InternalPort.HasValue && ExternalPort.HasValue)
                return $"{InternalIP}:{InternalPort} → {ExternalIP}:{ExternalPort}";
            return $"{InternalIP} → {ExternalIP}";
        }
    }

    public class NATManager
    {
        private List<NATEntry> natTable = new List<NATEntry>();
        private string publicIP = "200.100.50.1";
        private string routerIP = "192.168.1.1";
        private int nextPort = 10000;
        public bool IsEnabled { get; set; }

        public void SetPublicIP(string ip)
        {
            publicIP = ip;
            UnityEngine.Debug.Log($"[NAT] Public IP set to: {ip}");
        }

        public void SetRouterIP(string ip)
        {
            routerIP = ip;
        }

        public string GetRouterIP()
        {
            return routerIP;
        }

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

        public void RemoveEntry(NATEntry entry)
        {
            natTable.Remove(entry);
            UnityEngine.Debug.Log($"[NAT] Removed entry: {entry}");
        }

        public void ClearNAT()
        {
            natTable.Clear();
            UnityEngine.Debug.Log("[NAT] Table cleared");
        }

        public List<NATEntry> GetNATTable()
        {
            return new List<NATEntry>(natTable);
        }

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