using System.Collections.Generic;
using UnityEngine;

namespace SimRedes.Network
{
    /// <summary>Acción de una regla ACL: Permit (permitir) o Deny (denegar).</summary>
    public enum ACLAction
    {
        Permit,
        Deny
    }

    /// <summary>Protocolo de red para una regla ACL.</summary>
    public enum ACLProtocol
    {
        Any,
        TCP,
        UDP,
        ICMP,
        IP
    }

    /// <summary>Regla individual de una lista de control de acceso (ACL).</summary>
    public class ACLRule
    {
        /// <summary>Número de secuencia para el orden de evaluación.</summary>
        public int Sequence { get; set; }
        /// <summary>Acción de la regla (Permit/Deny).</summary>
        public ACLAction Action { get; set; }
        /// <summary>Protocolo de la regla.</summary>
        public ACLProtocol Protocol { get; set; }
        /// <summary>Dirección IP de origen.</summary>
        public string SourceIP { get; set; }
        /// <summary>Máscara wildcard de origen.</summary>
        public string SourceMask { get; set; }
        /// <summary>Dirección IP de destino.</summary>
        public string DestIP { get; set; }
        /// <summary>Máscara wildcard de destino.</summary>
        public string DestMask { get; set; }
        /// <summary>Puerto de origen (opcional).</summary>
        public int? SourcePort { get; set; }
        /// <summary>Puerto de destino (opcional).</summary>
        public int? DestPort { get; set; }
        /// <summary>Descripción de la regla.</summary>
        public string Description { get; set; }
        /// <summary>Indica si la regla está habilitada.</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>Crea una regla ACL con valores por defecto (Any/Permit).</summary>
        public ACLRule()
        {
            Protocol = ACLProtocol.Any;
            Action = ACLAction.Permit;
            SourceMask = "0.0.0.0";
            DestMask = "0.0.0.0";
        }

        /// <summary>Verifica si un paquete coincide con esta regla ACL.</summary>
        /// <param name="srcIp">Dirección IP de origen.</param>
        /// <param name="dstIp">Dirección IP de destino.</param>
        /// <param name="srcPort">Puerto de origen (opcional).</param>
        /// <param name="dstPort">Puerto de destino (opcional).</param>
        /// <param name="protocol">Protocolo (tcp, udp, icmp).</param>
        /// <returns>True si el paquete cumple todos los criterios de la regla.</returns>
        public bool Matches(string srcIp, string dstIp, int? srcPort = null, int? dstPort = null, string protocol = "")
        {
            if (!MatchesIP(srcIp, SourceIP, SourceMask))
                return false;

            if (!MatchesIP(dstIp, DestIP, DestMask))
                return false;

            if (Protocol != ACLProtocol.Any && Protocol != ACLProtocol.IP)
            {
                bool protoMatch = protocol.ToLower() switch
                {
                    "tcp" => Protocol == ACLProtocol.TCP,
                    "udp" => Protocol == ACLProtocol.UDP,
                    "icmp" => Protocol == ACLProtocol.ICMP,
                    _ => false
                };
                if (!protoMatch) return false;
            }

            if (SourcePort.HasValue && srcPort.HasValue && SourcePort != srcPort)
                return false;

            if (DestPort.HasValue && dstPort.HasValue && DestPort != dstPort)
                return false;

            return true;
        }

        /// <summary>Verifica si una IP coincide con la IP y máscara wildcard de la regla.</summary>
        /// <param name="ip">Dirección IP a verificar.</param>
        /// <param name="ruleIp">IP definida en la regla.</param>
        /// <param name="mask">Máscara wildcard.</param>
        /// <returns>True si hay coincidencia.</returns>
        private bool MatchesIP(string ip, string ruleIp, string mask)
        {
            if (string.IsNullOrEmpty(ruleIp) || ruleIp == "any")
                return true;

            var ipParts = ip.Split('.');
            var ruleParts = ruleIp.Split('.');
            var maskParts = mask.Split('.');

            if (ipParts.Length != 4 || ruleParts.Length != 4 || maskParts.Length != 4)
                return false;

            for (int i = 0; i < 4; i++)
            {
                if (!int.TryParse(ipParts[i], out int ipByte) ||
                    !int.TryParse(ruleParts[i], out int ruleByte) ||
                    !int.TryParse(maskParts[i], out int maskByte))
                    return false;

                if ((ipByte & maskByte) != (ruleByte & maskByte))
                    return false;
            }

            return true;
        }
    }

    /// <summary>Administra listas de control de acceso (ACL) para filtrar tráfico de red.</summary>
    public class ACLManager
    {
        private Dictionary<string, List<ACLRule>> acls = new Dictionary<string, List<ACLRule>>();
        private int sequenceCounter = 10;

        /// <summary>Obtiene todas las reglas de todas las ACLs.</summary>
        public List<ACLRule> Rules
        {
            get
            {
                var allRules = new List<ACLRule>();
                foreach (var acl in acls.Values)
                {
                    allRules.AddRange(acl);
                }
                return allRules;
            }
        }

        /// <summary>Crea una nueva lista de control de acceso con el nombre indicado.</summary>
        /// <param name="name">Nombre de la ACL.</param>
        /// <returns>El nombre de la ACL creada.</returns>
        public string CreateACL(string name)
        {
            if (!acls.ContainsKey(name))
            {
                acls[name] = new List<ACLRule>();
                UnityEngine.Debug.Log($"[ACL] Created ACL: {name}");
            }
            return name;
        }

        /// <summary>Agrega una regla a una ACL. Si la ACL no existe, la crea.</summary>
        /// <param name="aclName">Nombre de la ACL.</param>
        /// <param name="rule">Regla a agregar.</param>
        public void AddRule(string aclName, ACLRule rule)
        {
            if (!acls.ContainsKey(aclName))
            {
                CreateACL(aclName);
            }

            if (rule.Sequence == 0)
            {
                rule.Sequence = sequenceCounter;
                sequenceCounter += 10;
            }

            acls[aclName].Add(rule);
            UnityEngine.Debug.Log($"[ACL] Added rule {rule.Sequence} to {aclName}");
        }

        /// <summary>Elimina una regla de una ACL por su número de secuencia.</summary>
        /// <param name="aclName">Nombre de la ACL.</param>
        /// <param name="sequence">Número de secuencia de la regla a eliminar.</param>
        public void RemoveRule(string aclName, int sequence)
        {
            if (acls.ContainsKey(aclName))
            {
                acls[aclName].RemoveAll(r => r.Sequence == sequence);
            }
        }

        /// <summary>Evalúa un paquete contra las reglas de una ACL en orden de secuencia.</summary>
        /// <param name="aclName">Nombre de la ACL.</param>
        /// <param name="srcIp">Dirección IP de origen.</param>
        /// <param name="dstIp">Dirección IP de destino.</param>
        /// <param name="srcPort">Puerto de origen (opcional).</param>
        /// <param name="dstPort">Puerto de destino (opcional).</param>
        /// <param name="protocol">Protocolo (tcp, udp, icmp).</param>
        /// <returns>True si el paquete es permitido, false si es denegado o no hay reglas.</returns>
        public bool CheckPacket(string aclName, string srcIp, string dstIp, int? srcPort = null, int? dstPort = null, string protocol = "")
        {
            if (!acls.ContainsKey(aclName) || acls[aclName].Count == 0)
                return true;

            var rules = acls[aclName];
            rules.Sort((a, b) => a.Sequence.CompareTo(b.Sequence));

            foreach (var rule in rules)
            {
                if (rule.Matches(srcIp, dstIp, srcPort, dstPort, protocol))
                {
                    return rule.Action == ACLAction.Permit;
                }
            }

            return false;
        }

        /// <summary>Obtiene una copia de las reglas de una ACL.</summary>
        /// <param name="aclName">Nombre de la ACL.</param>
        /// <returns>Lista de reglas o lista vacía si no existe.</returns>
        public List<ACLRule> GetRules(string aclName)
        {
            if (acls.ContainsKey(aclName))
            {
                return new List<ACLRule>(acls[aclName]);
            }
            return new List<ACLRule>();
        }

        /// <summary>Elimina una ACL y todas sus reglas.</summary>
        /// <param name="name">Nombre de la ACL a eliminar.</param>
        public void DeleteACL(string name)
        {
            if (acls.ContainsKey(name))
            {
                acls.Remove(name);
                UnityEngine.Debug.Log($"[ACL] Deleted ACL: {name}");
            }
        }

        /// <summary>Obtiene un resumen textual de todas las ACLs configuradas.</summary>
        /// <returns>Cadena con el resumen de ACLs.</returns>
        public string GetACLSummary()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("ACLs configured:");

            foreach (var kvp in acls)
            {
                sb.AppendLine($"  {kvp.Key}: {kvp.Value.Count} rules");
            }

            return sb.ToString();
        }

        /// <summary>Crea una regla ACL estándar (solo IP de origen).</summary>
        /// <param name="action">Acción: "permit" o "deny".</param>
        /// <param name="srcIp">Dirección IP de origen.</param>
        /// <param name="description">Descripción opcional.</param>
        /// <returns>Nueva regla ACL estándar.</returns>
        public static ACLRule CreateStandardRule(string action, string srcIp, string description = "")
        {
            return new ACLRule
            {
                Action = action.ToLower() == "permit" ? ACLAction.Permit : ACLAction.Deny,
                SourceIP = srcIp,
                Description = description
            };
        }

        /// <summary>Crea una regla ACL extendida (protocolo, origen y destino).</summary>
        /// <param name="action">Acción: "permit" o "deny".</param>
        /// <param name="protocol">Protocolo: "tcp", "udp", "icmp", "ip".</param>
        /// <param name="srcIp">Dirección IP de origen.</param>
        /// <param name="dstIp">Dirección IP de destino.</param>
        /// <param name="description">Descripción opcional.</param>
        /// <returns>Nueva regla ACL extendida.</returns>
        public static ACLRule CreateExtendedRule(string action, string protocol, string srcIp, string dstIp, string description = "")
        {
            return new ACLRule
            {
                Action = action.ToLower() == "permit" ? ACLAction.Permit : ACLAction.Deny,
                Protocol = protocol.ToLower() switch
                {
                    "tcp" => ACLProtocol.TCP,
                    "udp" => ACLProtocol.UDP,
                    "icmp" => ACLProtocol.ICMP,
                    "ip" => ACLProtocol.IP,
                    _ => ACLProtocol.Any
                },
                SourceIP = srcIp,
                DestIP = dstIp,
                Description = description
            };
        }
    }
}