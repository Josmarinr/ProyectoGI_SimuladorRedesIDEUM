using System.Collections.Generic;
using UnityEngine;

namespace SimRedes.Network
{
    public class VLANManager
    {
        private Dictionary<int, List<NetworkNode>> vlans = new Dictionary<int, List<NetworkNode>>();
        private Dictionary<string, int> nodeVlanMap = new Dictionary<string, int>();
        private int nextVlanId = 10;

        public const int DEFAULT_VLAN = 1;
        public const int MAX_VLANS = 4094;

        public VLANManager()
        {
            CreateVLAN(DEFAULT_VLAN);
        }

        public int CreateVLAN(int vlanId = 0)
        {
            if (vlanId == 0)
            {
                vlanId = nextVlanId++;
            }

            if (!vlans.ContainsKey(vlanId))
            {
                vlans[vlanId] = new List<NetworkNode>();
                UnityEngine.Debug.Log($"[VLAN] Created VLAN {vlanId}");
            }
            return vlanId;
        }

        public void DeleteVLAN(int vlanId)
        {
            if (vlanId == DEFAULT_VLAN)
            {
                UnityEngine.Debug.LogWarning("[VLAN] Cannot delete default VLAN 1");
                return;
            }

            if (vlans.ContainsKey(vlanId))
            {
                foreach (var node in vlans[vlanId])
                {
                    if (nodeVlanMap.ContainsKey(node.Id))
                    {
                        AssignToVLAN(node, DEFAULT_VLAN);
                    }
                }
                vlans.Remove(vlanId);
                UnityEngine.Debug.Log($"[VLAN] Deleted VLAN {vlanId}");
            }
        }

        public void AssignToVLAN(NetworkNode node, int vlanId)
        {
            if (!vlans.ContainsKey(vlanId))
            {
                CreateVLAN(vlanId);
            }

            int currentVlan = GetNodeVLAN(node);
            if (currentVlan != DEFAULT_VLAN && vlans.ContainsKey(currentVlan))
            {
                vlans[currentVlan].Remove(node);
            }

            if (!vlans[vlanId].Contains(node))
            {
                vlans[vlanId].Add(node);
            }

            nodeVlanMap[node.Id] = vlanId;
            UnityEngine.Debug.Log($"[VLAN] Node {node.Name} assigned to VLAN {vlanId}");
        }

        public int GetNodeVLAN(NetworkNode node)
        {
            return nodeVlanMap.TryGetValue(node.Id, out int vlan) ? vlan : DEFAULT_VLAN;
        }

        public List<NetworkNode> GetNodesInVLAN(int vlanId)
        {
            if (vlans.ContainsKey(vlanId))
            {
                return new List<NetworkNode>(vlans[vlanId]);
            }
            return new List<NetworkNode>();
        }

        public bool CanCommunicate(NetworkNode source, NetworkNode dest)
        {
            int sourceVlan = GetNodeVLAN(source);
            int destVlan = GetNodeVLAN(dest);

            return sourceVlan == destVlan;
        }

        public List<int> GetAllVLANs()
        {
            return new List<int>(vlans.Keys);
        }

        public string GetVLANSummary()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("VLANs configured:");

            foreach (var kvp in vlans)
            {
                sb.AppendLine($"  VLAN {kvp.Key}: {kvp.Value.Count} nodes");
            }

            return sb.ToString();
        }
    }
}