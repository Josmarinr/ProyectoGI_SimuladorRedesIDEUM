using System.Collections.Generic;
using UnityEngine;

namespace SimRedes.Network
{
    public enum ACLAction
    {
        Permit,
        Deny
    }

    public enum ACLProtocol
    {
        Any,
        TCP,
        UDP,
        ICMP,
        IP
    }

    public class ACLRule
    {
        public int Sequence { get; set; }
        public ACLAction Action { get; set; }
        public ACLProtocol Protocol { get; set; }
        public string SourceIP { get; set; }
        public string SourceMask { get; set; }
        public string DestIP { get; set; }
        public string DestMask { get; set; }
        public int? SourcePort { get; set; }
        public int? DestPort { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; } = true;

        public ACLRule()
        {
            Protocol = ACLProtocol.Any;
            Action = ACLAction.Permit;
            SourceMask = "0.0.0.0";
            DestMask = "0.0.0.0";
        }

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

        private bool MatchesIP(string ip, string ruleIp, string mask)
        {
            if (string.IsNullOrEmpty(ruleIp) || ruleIp == "any")
                return true;

            var ipParts = ip.Split('.');
            var ruleParts = ruleIp.Split('.');
            var maskParts = mask.Split('.');

            if (ipParts.Length != 4 || ruleParts.Length != 4 || maskParts.Length != 4)
                return true;

            for (int i = 0; i < 4; i++)
            {
                int ipByte = int.Parse(ipParts[i]);
                int ruleByte = int.Parse(ruleParts[i]);
                int maskByte = int.Parse(maskParts[i]);

                if ((ipByte & maskByte) != (ruleByte & maskByte))
                    return false;
            }

            return true;
        }
    }

    public class ACLManager
    {
        private Dictionary<string, List<ACLRule>> acls = new Dictionary<string, List<ACLRule>>();
        private int sequenceCounter = 10;

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

        public string CreateACL(string name)
        {
            if (!acls.ContainsKey(name))
            {
                acls[name] = new List<ACLRule>();
                UnityEngine.Debug.Log($"[ACL] Created ACL: {name}");
            }
            return name;
        }

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

        public void RemoveRule(string aclName, int sequence)
        {
            if (acls.ContainsKey(aclName))
            {
                acls[aclName].RemoveAll(r => r.Sequence == sequence);
            }
        }

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

        public List<ACLRule> GetRules(string aclName)
        {
            if (acls.ContainsKey(aclName))
            {
                return new List<ACLRule>(acls[aclName]);
            }
            return new List<ACLRule>();
        }

        public void DeleteACL(string name)
        {
            if (acls.ContainsKey(name))
            {
                acls.Remove(name);
                UnityEngine.Debug.Log($"[ACL] Deleted ACL: {name}");
            }
        }

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

        public static ACLRule CreateStandardRule(string action, string srcIp, string description = "")
        {
            return new ACLRule
            {
                Action = action.ToLower() == "permit" ? ACLAction.Permit : ACLAction.Deny,
                SourceIP = srcIp,
                Description = description
            };
        }

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