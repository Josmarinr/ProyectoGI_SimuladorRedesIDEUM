using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using IPValidation = SimRedes.Network.IPValidation;

namespace SimRedes.Network
{
    public class TopologyManager : MonoBehaviour
    {
        public static TopologyManager Instance { get; private set; }

        private Dictionary<int, NetworkNode> nodes = new Dictionary<int, NetworkNode>();
        private List<NetworkLink> links = new List<NetworkLink>();

        public event Action<NetworkNode> OnNodeAdded;
        public event Action<NetworkNode> OnNodeRemoved;
        public event Action<NetworkLink> OnLinkAdded;
        public event Action<NetworkLink> OnLinkRemoved;
        public event Action OnTopologyChanged;

        public int selectedNodeForLink = -1;

        public VLANManager VLAN { get; private set; }
        public ACLManager ACL { get; private set; }
        public NATManager NAT { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            VLAN = new VLANManager();
            ACL = new ACLManager();
            NAT = new NATManager();
        }

        public void AddNode(int discId, DeviceType type, Vector2 position)
        {
            if (!nodes.ContainsKey(discId))
            {
                var node = new NetworkNode(discId, type, position);
                nodes.Add(discId, node);
                OnNodeAdded?.Invoke(node);
            }
            else
            {
                nodes[discId].Position = position;
            }
            OnTopologyChanged?.Invoke();
        }

        public void RemoveNode(int discId)
        {
            if (nodes.TryGetValue(discId, out var node))
            {
                nodes.Remove(discId);
                links.RemoveAll(l => l.SourceNode.DiscId == discId || l.DestinationNode.DiscId == discId);
                OnNodeRemoved?.Invoke(node);
                UnityEngine.Debug.Log($"[Topology] Nodo eliminado: Disc {discId}");
            }
            OnTopologyChanged?.Invoke();
        }

        public void UpdateNodePosition(int discId, Vector2 position)
        {
            if (nodes.TryGetValue(discId, out var node))
            {
                node.Position = position;
                OnTopologyChanged?.Invoke();
            }
        }

        public void AddLink(int sourceDiscId, int destDiscId, string srcInterface = "G0/0", string dstInterface = "G0/0")
        {
            if (nodes.TryGetValue(sourceDiscId, out var source) && nodes.TryGetValue(destDiscId, out var dest))
            {
                var existingLink = links.FirstOrDefault(l =>
                    (l.SourceNode.DiscId == sourceDiscId && l.DestinationNode.DiscId == destDiscId) ||
                    (l.SourceNode.DiscId == destDiscId && l.DestinationNode.DiscId == sourceDiscId));

                if (existingLink == null)
                {
                    var link = new NetworkLink(source, dest, srcInterface, dstInterface);
                    links.Add(link);
                    OnLinkAdded?.Invoke(link);
                    UnityEngine.Debug.Log($"[Topology] Enlace creado: {source.Name} <-> {dest.Name}");
                    OnTopologyChanged?.Invoke();
                }
            }
        }

        public void RemoveLink(int sourceDiscId, int destDiscId)
        {
            var link = links.FirstOrDefault(l =>
                (l.SourceNode.DiscId == sourceDiscId && l.DestinationNode.DiscId == destDiscId) ||
                (l.SourceNode.DiscId == destDiscId && l.DestinationNode.DiscId == sourceDiscId));

            if (link != null)
            {
                links.Remove(link);
                OnLinkRemoved?.Invoke(link);
                UnityEngine.Debug.Log($"[Topology] Enlace eliminado: {link.SourceNode.Name} <-> {link.DestinationNode.Name}");
                OnTopologyChanged?.Invoke();
            }
        }

        public void SelectNodeForLink(int discId)
        {
            selectedNodeForLink = discId;
            UnityEngine.Debug.Log($"[Topology] Nodo seleccionado para enlace: {discId}");
        }

        public void CreateLinkBetweenSelected()
        {
            if (selectedNodeForLink == -1) return;

            var allNodes = GetAllNodes();
            if (allNodes.Count >= 2)
            {
                AddLink(selectedNodeForLink, allNodes[0].DiscId);
            }
            selectedNodeForLink = -1;
        }

        public NetworkNode GetNode(int discId)
        {
            return nodes.TryGetValue(discId, out var node) ? node : null;
        }

        public List<NetworkNode> FindNodesNear(Vector2 position, float radius)
        {
            var result = new List<NetworkNode>();
            foreach (var node in nodes.Values)
            {
                if (Vector2.Distance(node.Position, position) <= radius)
                    result.Add(node);
            }
            return result;
        }

        public List<NetworkNode> GetAllNodes()
        {
            return nodes.Values.ToList();
        }

        public List<NetworkLink> GetAllLinks()
        {
            return links;
        }

        public void SetNodeFault(int discId, string faultType)
        {
            if (nodes.TryGetValue(discId, out var node))
            {
                node.ConfiguredFault = faultType;
                UnityEngine.Debug.Log($"[Topology] Fallo configurado en {node.Name}: {faultType}");
            }
        }

        public void ClearNodeFault(int discId)
        {
            if (nodes.TryGetValue(discId, out var node))
            {
                node.ConfiguredFault = null;
            }
        }

        public void SetLinkFault(int sourceDiscId, int destDiscId)
        {
            var link = links.FirstOrDefault(l =>
                (l.SourceNode.DiscId == sourceDiscId && l.DestinationNode.DiscId == destDiscId) ||
                (l.SourceNode.DiscId == destDiscId && l.DestinationNode.DiscId == sourceDiscId));

            if (link != null)
            {
                link.SetFault("linkdown");
                UnityEngine.Debug.Log($"[Topology] Enlace con fallo entre {sourceDiscId} y {destDiscId}");
            }
        }

        public void ClearLinkFault(int sourceDiscId, int destDiscId)
        {
            var link = links.FirstOrDefault(l =>
                (l.SourceNode.DiscId == sourceDiscId && l.DestinationNode.DiscId == destDiscId) ||
                (l.SourceNode.DiscId == destDiscId && l.DestinationNode.DiscId == sourceDiscId));

            if (link != null)
            {
                link.SetFault(null);
            }
        }

        public bool CheckConnectivity(int sourceDiscId, int destDiscId)
        {
            if (!nodes.ContainsKey(sourceDiscId) || !nodes.ContainsKey(destDiscId))
                return false;

            var sourceNode = nodes[sourceDiscId];
            var destNode = nodes[destDiscId];

            bool hasPath = HasPhysicalPath(sourceDiscId, destDiscId);
            if (!hasPath)
                return false;

            if (!ValidateVLAN(sourceNode, destNode))
                return false;

            if (!ValidateACL(sourceNode, destNode))
                return false;

            if (sourceNode.Type == DeviceType.Switch && destNode.Type == DeviceType.Switch)
            {
                return HasPhysicalPath(sourceDiscId, destDiscId);
            }

            if (sourceNode.Type == DeviceType.Switch || destNode.Type == DeviceType.Switch)
            {
                var nonSwitch = sourceNode.Type == DeviceType.Switch ? destNode : sourceNode;
                if (nonSwitch.Type == DeviceType.PC)
                {
                    if (!IPValidation.IsValidIP(nonSwitch.IpAddress) ||
                        !IPValidation.IsValidSubnetMask(nonSwitch.SubnetMask))
                        return false;
                }
                else if (nonSwitch.Type == DeviceType.Router)
                {
                    if (!IPValidation.IsValidIP(nonSwitch.IpAddress))
                        return false;
                }
                return true;
            }

            bool sourceHasIP = IPValidation.IsValidIP(sourceNode.IpAddress);
            bool destHasIP = IPValidation.IsValidIP(destNode.IpAddress);
            bool sourceHasMask = IPValidation.IsValidSubnetMask(sourceNode.SubnetMask);

            if (sourceNode.Type == DeviceType.PC && destNode.Type == DeviceType.PC)
            {
                if (!sourceHasIP || !destHasIP || !sourceHasMask)
                    return false;

                return IPValidation.IsInSameNetwork(sourceNode.IpAddress, sourceNode.SubnetMask, destNode.IpAddress);
            }

            if (sourceNode.Type == DeviceType.Router || destNode.Type == DeviceType.Router)
            {
                if (!sourceHasIP || !destHasIP || !sourceHasMask)
                    return false;

                if (sourceNode.Type == DeviceType.Router && destNode.Type == DeviceType.Router)
                {
                    if (!IPValidation.IsInSameNetwork(sourceNode.IpAddress, sourceNode.SubnetMask, destNode.IpAddress))
                        return false;
                }

                return true;
            }

            if (!sourceHasIP || !destHasIP || !sourceHasMask)
                return false;

            return true;
        }

        private bool HasPhysicalPath(int sourceDiscId, int destDiscId)
        {
            var visited = new HashSet<int>();
            var queue = new Queue<int>();
            queue.Enqueue(sourceDiscId);
            visited.Add(sourceDiscId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == destDiscId)
                    return true;

                var connectedNodes = links
                    .Where(l => l.IsFunctional() && l.SourceNode.DiscId == current)
                    .Select(l => l.DestinationNode.DiscId)
                    .Concat(links
                        .Where(l => l.IsFunctional() && l.DestinationNode.DiscId == current)
                        .Select(l => l.SourceNode.DiscId));

                foreach (var nodeId in connectedNodes)
                {
                    if (!visited.Contains(nodeId))
                    {
                        visited.Add(nodeId);
                        queue.Enqueue(nodeId);
                    }
                }
            }
            return false;
        }

        public string GetIPNetworkInfo(int discId)
        {
            if (!nodes.TryGetValue(discId, out var node))
                return null;

            if (!IPValidation.IsValidIP(node.IpAddress) || !IPValidation.IsValidSubnetMask(node.SubnetMask))
                return null;

            string network = IPValidation.GetNetworkAddress(node.IpAddress, node.SubnetMask);
            int prefix = IPValidation.GetPrefixLength(node.SubnetMask);

            return $"{network}/{prefix}";
        }

        public List<NetworkNode> FindPath(int sourceDiscId, int destDiscId)
        {
            var path = new List<NetworkNode>();

            if (!nodes.ContainsKey(sourceDiscId) || !nodes.ContainsKey(destDiscId))
                return path;

            var visited = new Dictionary<int, int>();
            var queue = new Queue<int>();
            queue.Enqueue(sourceDiscId);
            visited[sourceDiscId] = -1;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current == destDiscId)
                    break;

                var connectedLinks = links
                    .Where(l => l.IsFunctional() && l.SourceNode.DiscId == current)
                    .Select(l => new { Node = l.DestinationNode, From = current })
                    .Concat(links
                        .Where(l => l.IsFunctional() && l.DestinationNode.DiscId == current)
                        .Select(l => new { Node = l.SourceNode, From = current }))
                    .ToList();

                foreach (var conn in connectedLinks)
                {
                    if (!visited.ContainsKey(conn.Node.DiscId))
                    {
                        visited[conn.Node.DiscId] = conn.From;
                        queue.Enqueue(conn.Node.DiscId);
                    }
                }
            }

            if (!visited.ContainsKey(destDiscId))
                return path;

            var reversePath = new List<int>();
            int currentNode = destDiscId;
            while (currentNode != -1)
            {
                reversePath.Add(currentNode);
                currentNode = visited[currentNode];
            }

            reversePath.Reverse();

            foreach (var nodeId in reversePath)
            {
                if (nodes.TryGetValue(nodeId, out var node))
                    path.Add(node);
            }

            return path;
        }

        public List<NetworkLink> GetLinksOnPath(List<NetworkNode> path)
        {
            var pathLinks = new List<NetworkLink>();

            for (int i = 0; i < path.Count - 1; i++)
            {
                var fromNode = path[i];
                var toNode = path[i + 1];

                var link = links.FirstOrDefault(l =>
                    (l.SourceNode.DiscId == fromNode.DiscId && l.DestinationNode.DiscId == toNode.DiscId) ||
                    (l.SourceNode.DiscId == toNode.DiscId && l.DestinationNode.DiscId == fromNode.DiscId));

                if (link != null)
                    pathLinks.Add(link);
            }

            return pathLinks;
        }

        public void ClearTopology()
        {
            nodes.Clear();
            links.Clear();
            OnTopologyChanged?.Invoke();
            UnityEngine.Debug.Log("[Topology] Topología limpiada");
        }

        public string GetTopologySummary()
        {
            return $"Nodos: {nodes.Count}, Enlaces: {links.Count}";
        }

        public string GetVLANSummary()
        {
            return VLAN != null ? VLAN.GetVLANSummary() : "VLAN not initialized";
        }

        public string GetACLSummary()
        {
            return ACL != null ? ACL.GetACLSummary() : "ACL not initialized";
        }

        public string GetNATSummary()
        {
            return NAT != null ? NAT.GetNATSummary() : "NAT not initialized";
        }

        private bool ValidateVLAN(NetworkNode source, NetworkNode dest)
        {
            if (VLAN == null)
                return true;

            return VLAN.CanCommunicate(source, dest);
        }

        private bool ValidateACL(NetworkNode source, NetworkNode dest)
        {
            if (ACL == null || ACL.Rules.Count == 0)
                return true;

            string sourceIP = source.IpAddress;
            string destIP = dest.IpAddress;

            foreach (var rule in ACL.Rules)
            {
                if (!rule.IsEnabled)
                    continue;

                if (rule.Matches(sourceIP, destIP))
                {
                    return rule.Action == ACLAction.Permit;
                }
            }

            return true;
        }

        public string TranslateSourceIP(string internalIP)
        {
            if (NAT == null || !NAT.IsEnabled)
                return internalIP;

            return NAT.TranslateInternalToExternal(internalIP);
        }

        public string TranslateDestIP(string externalIP)
        {
            if (NAT == null || !NAT.IsEnabled)
                return externalIP;

            return NAT.TranslateExternalToInternal(externalIP);
        }
    }
}