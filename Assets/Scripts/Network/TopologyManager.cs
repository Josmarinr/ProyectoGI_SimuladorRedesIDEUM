using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using IPValidation = SimRedes.Network.IPValidation;

namespace SimRedes.Network
{
    /// <summary>
    /// Gestiona la topologia de red: nodos, enlaces, VLAN, ACL y NAT.
    /// Expone eventos de cambio y mantiene el estado global de red como singleton.
    /// </summary>
    public class TopologyManager : MonoBehaviour
    {
        public static TopologyManager Instance { get; private set; }

        private Dictionary<int, NetworkNode> nodes = new Dictionary<int, NetworkNode>();
        private List<NetworkLink> links = new List<NetworkLink>();

        /// <summary>
        /// Se invoca cuando se agrega un nodo a la topologia.
        /// </summary>
        public event Action<NetworkNode> OnNodeAdded;
        /// <summary>
        /// Se invoca cuando se elimina un nodo de la topologia.
        /// </summary>
        public event Action<NetworkNode> OnNodeRemoved;
        /// <summary>
        /// Se invoca cuando se agrega un enlace entre dos nodos.
        /// </summary>
        public event Action<NetworkLink> OnLinkAdded;
        /// <summary>
        /// Se invoca cuando se elimina un enlace de la topologia.
        /// </summary>
        public event Action<NetworkLink> OnLinkRemoved;
        /// <summary>
        /// Se invoca ante cualquier cambio en la topologia (nodos o enlaces).
        /// </summary>
        public event Action OnTopologyChanged;

        /// <summary>
        /// ID del nodo seleccionado como origen para crear un enlace manual.
        /// -1 indica que no hay nodo seleccionado.
        /// </summary>
        public int selectedNodeForLink = -1;

        public VLANManager VLAN { get; private set; }
        public ACLManager ACL { get; private set; }
        public NATManager NAT { get; private set; }

        private HashSet<string> aclNames = new HashSet<string>();

        /// <summary>
        /// Inicializa el singleton y los managers de VLAN, ACL y NAT.
        /// Si ya existe una instancia, se destruye a si mismo.
        /// </summary>
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

        /// <summary>
        /// Agrega un nodo a la topologia. Si el disco ya existe, actualiza su posicion.
        /// </summary>
        /// <param name="discId">Identificador unico del disco (1-18).</param>
        /// <param name="type">Tipo de dispositivo (Router, Switch, PC).</param>
        /// <param name="position">Posicion en coordenadas del canvas.</param>
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

        /// <summary>
        /// Elimina un nodo de la topologia y todos los enlaces asociados.
        /// </summary>
        /// <param name="discId">Identificador del nodo a eliminar.</param>
        public void RemoveNode(int discId)
        {
            if (nodes.TryGetValue(discId, out var node))
            {
                nodes.Remove(discId);
                links.RemoveAll(l => l.SourceNode.DiscId == discId || l.DestinationNode.DiscId == discId);
                OnNodeRemoved?.Invoke(node);
            }
            OnTopologyChanged?.Invoke();
        }

        /// <summary>
        /// Actualiza la posicion de un nodo existente en la topologia.
        /// </summary>
        /// <param name="discId">Identificador del nodo a reposicionar.</param>
        /// <param name="position">Nueva posicion en coordenadas del canvas.</param>
        public void UpdateNodePosition(int discId, Vector2 position)
        {
            if (nodes.TryGetValue(discId, out var node))
            {
                node.Position = position;
                OnTopologyChanged?.Invoke();
            }
        }

        /// <summary>
        /// Crea un enlace entre dos nodos si ambos existen y no hay un enlace previo.
        /// </summary>
        /// <param name="sourceDiscId">Identificador del nodo origen.</param>
        /// <param name="destDiscId">Identificador del nodo destino.</param>
        /// <param name="srcInterface">Nombre de la interfaz en el nodo origen.</param>
        /// <param name="dstInterface">Nombre de la interfaz en el nodo destino.</param>
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
                    OnTopologyChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// Elimina el enlace entre dos nodos si existe.
        /// </summary>
        /// <param name="sourceDiscId">Identificador del nodo origen del enlace.</param>
        /// <param name="destDiscId">Identificador del nodo destino del enlace.</param>
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

        /// <summary>
        /// Marca un nodo como origen para la creacion manual de un enlace.
        /// </summary>
        /// <param name="discId">Identificador del nodo a seleccionar.</param>
        public void SelectNodeForLink(int discId)
        {
            selectedNodeForLink = discId;
            UnityEngine.Debug.Log($"[Topology] Nodo seleccionado para enlace: {discId}");
        }

        /// <summary>
        /// Crea un enlace desde el nodo seleccionado hacia el primer nodo disponible.
        /// </summary>
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

        /// <summary>
        /// Obtiene un nodo por su identificador de disco.
        /// </summary>
        /// <param name="discId">Identificador del nodo buscado.</param>
        /// <returns>El nodo encontrado, o null si no existe.</returns>
        public NetworkNode GetNode(int discId)
        {
            return nodes.TryGetValue(discId, out var node) ? node : null;
        }

        /// <summary>
        /// Busca nodos dentro de un radio de distancia desde una posicion dada.
        /// </summary>
        /// <param name="position">Posicion central de busqueda.</param>
        /// <param name="radius">Radio maximo de distancia.</param>
        /// <returns>Lista de nodos dentro del radio especificado.</returns>
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

        /// <summary>
        /// Retorna una copia de la lista de todos los nodos en la topologia.
        /// </summary>
        /// <returns>Lista completa de nodos.</returns>
        public List<NetworkNode> GetAllNodes()
        {
            return nodes.Values.ToList();
        }

        /// <summary>
        /// Retorna la lista de todos los enlaces en la topologia.
        /// </summary>
        /// <returns>Lista completa de enlaces.</returns>
        public List<NetworkLink> GetAllLinks()
        {
            return links;
        }

        /// <summary>
        /// Configura un fallo en un nodo especifico.
        /// </summary>
        /// <param name="discId">Identificador del nodo.</param>
        /// <param name="faultType">Tipo de fallo a asignar.</param>
        public void SetNodeFault(int discId, string faultType)
        {
            if (nodes.TryGetValue(discId, out var node))
            {
                node.ConfiguredFault = faultType;
                UnityEngine.Debug.Log($"[Topology] Fallo configurado en {node.Name}: {faultType}");
            }
        }

        /// <summary>
        /// Elimina la configuracion de fallo de un nodo.
        /// </summary>
        /// <param name="discId">Identificador del nodo.</param>
        public void ClearNodeFault(int discId)
        {
            if (nodes.TryGetValue(discId, out var node))
            {
                node.ConfiguredFault = null;
            }
        }

        /// <summary>
        /// Marca un enlace como con fallo entre dos nodos.
        /// </summary>
        /// <param name="sourceDiscId">Identificador del nodo origen del enlace.</param>
        /// <param name="destDiscId">Identificador del nodo destino del enlace.</param>
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

        /// <summary>
        /// Elimina el fallo de un enlace entre dos nodos.
        /// </summary>
        /// <param name="sourceDiscId">Identificador del nodo origen del enlace.</param>
        /// <param name="destDiscId">Identificador del nodo destino del enlace.</param>
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

        /// <summary>
        /// Verifica conectividad extremo a extremo entre dos nodos considerando
        /// ruta fisica, VLAN, ACL, NAT y configuracion IP segun tipo de dispositivo.
        /// </summary>
        /// <param name="sourceDiscId">Identificador del nodo origen.</param>
        /// <param name="destDiscId">Identificador del nodo destino.</param>
        /// <param name="srcPort">Puerto origen (opcional, para evaluacion ACL).</param>
        /// <param name="dstPort">Puerto destino (opcional, para evaluacion ACL).</param>
        /// <param name="protocol">Protocolo de capa superior.</param>
        /// <returns>True si la comunicacion es posible, false en caso contrario.</returns>
        public bool CheckConnectivity(int sourceDiscId, int destDiscId,
            int? srcPort = null, int? dstPort = null, string protocol = "icmp")
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

            if (!ValidateACL(sourceNode, destNode, srcPort, dstPort, protocol))
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

            // --- NAT Check: si hay traducción, permite comunicación cross-network ---
            if (NAT != null)
            {
                bool sourceHasNAT = NAT.LookupInternal(sourceNode.IpAddress) != null;
                bool destHasNAT = NAT.LookupExternal(destNode.IpAddress) != null;

                if (sourceHasNAT || destHasNAT)
                {
                    // Solo aplicar NAT si source y dest están en redes diferentes
                    bool sameNetwork = IPValidation.IsValidIP(sourceNode.IpAddress) &&
                                       IPValidation.IsValidSubnetMask(sourceNode.SubnetMask) &&
                                       IPValidation.IsInSameNetwork(sourceNode.IpAddress, sourceNode.SubnetMask, destNode.IpAddress);

                    if (!sameNetwork)
                    {
                        // NAT maneja el ruteo entre redes diferentes
                        if (!IPValidation.IsValidIP(sourceNode.IpAddress) ||
                            !IPValidation.IsValidIP(destNode.IpAddress))
                            return false;

                        return true;
                    }
                    // Si están en la misma red, cae a las verificaciones normales (tráfico interno)
                }
            }
            // --- Fin NAT Check ---

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

        /// <summary>
        /// Determina si existe una ruta fisica entre dos nodos usando BFS.
        /// Solo considera enlaces funcionales (sin fallo).
        /// </summary>
        /// <param name="sourceDiscId">Identificador del nodo origen.</param>
        /// <param name="destDiscId">Identificador del nodo destino.</param>
        /// <returns>True si hay al menos un camino de enlaces funcionales.</returns>
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

        /// <summary>
        /// Obtiene la informacion de red de un nodo en formato CIDR.
        /// </summary>
        /// <param name="discId">Identificador del nodo.</param>
        /// <returns>Notacion CIDR (ej: "192.168.1.0/24"), o null si las IP/mascara no son validas.</returns>
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

        /// <summary>
        /// Encuentra la secuencia de nodos en la ruta mas corta entre dos nodos usando BFS.
        /// </summary>
        /// <param name="sourceDiscId">Identificador del nodo origen.</param>
        /// <param name="destDiscId">Identificador del nodo destino.</param>
        /// <returns>Lista ordenada de nodos desde origen a destino. Vacia si no hay ruta.</returns>
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

        /// <summary>
        /// Obtiene los enlaces que conectan una secuencia de nodos en una ruta.
        /// </summary>
        /// <param name="path">Lista ordenada de nodos que conforman la ruta.</param>
        /// <returns>Lista de enlaces entre los nodos consecutivos de la ruta.</returns>
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

        /// <summary>
        /// Elimina todos los nodos y enlaces de la topologia.
        /// Dispara <see cref="OnNodeRemoved"/> por cada nodo existente antes de limpiar,
        /// para que NodeVisualizer destruya los objetos visuales correspondientes.
        /// Dispara <see cref="OnTopologyChanged"/> al finalizar.
        /// </summary>
        public void ClearTopology()
        {
            foreach (var kvp in nodes)
            {
                OnNodeRemoved?.Invoke(kvp.Value);
            }
            nodes.Clear();
            links.Clear();
            OnTopologyChanged?.Invoke();
            UnityEngine.Debug.Log("[Topology] Topología limpiada");
        }

        /// <summary>
        /// Retorna un resumen textual de la cantidad de nodos y enlaces.
        /// </summary>
        /// <returns>String con conteo de nodos y enlaces.</returns>
        public string GetTopologySummary()
        {
            return $"Nodos: {nodes.Count}, Enlaces: {links.Count}";
        }

        /// <summary>
        /// Retorna un resumen de la configuracion VLAN actual.
        /// </summary>
        /// <returns>Resumen VLAN, o mensaje de no inicializado.</returns>
        public string GetVLANSummary()
        {
            return VLAN != null ? VLAN.GetVLANSummary() : "VLAN not initialized";
        }

        /// <summary>
        /// Retorna un resumen de las listas de control de acceso actuales.
        /// </summary>
        /// <returns>Resumen ACL, o mensaje de no inicializado.</returns>
        public string GetACLSummary()
        {
            return ACL != null ? ACL.GetACLSummary() : "ACL not initialized";
        }

        /// <summary>
        /// Retorna un resumen de la configuracion NAT actual.
        /// </summary>
        /// <returns>Resumen NAT, o mensaje de no inicializado.</returns>
        public string GetNATSummary()
        {
            return NAT != null ? NAT.GetNATSummary() : "NAT not initialized";
        }

        /// <summary>
        /// Verifica si dos nodos pueden comunicarse segun las reglas VLAN configuradas.
        /// </summary>
        /// <param name="source">Nodo origen.</param>
        /// <param name="dest">Nodo destino.</param>
        /// <returns>True si la comunicacion VLAN esta permitida o no hay VLAN configurada.</returns>
        private bool ValidateVLAN(NetworkNode source, NetworkNode dest)
        {
            if (VLAN == null)
                return true;

            return VLAN.CanCommunicate(source, dest);
        }

        /// <summary>
        /// Crea una nueva lista de control de acceso con el nombre especificado.
        /// </summary>
        /// <param name="name">Nombre unico para la ACL.</param>
        public void CreateACL(string name)
        {
            if (ACL == null) return;
            ACL.CreateACL(name);
            aclNames.Add(name);
        }

        /// <summary>
        /// Agrega una regla a una ACL existente.
        /// </summary>
        /// <param name="aclName">Nombre de la ACL destino.</param>
        /// <param name="rule">Regla a agregar (permiso/denegacion, IPs, puertos).</param>
        public void AddACLRule(string aclName, ACLRule rule)
        {
            if (ACL == null) return;
            aclNames.Add(aclName);
            ACL.AddRule(aclName, rule);
        }

        /// <summary>
        /// Elimina una ACL y su nombre del registro interno.
        /// </summary>
        /// <param name="name">Nombre de la ACL a eliminar.</param>
        public void DeleteACL(string name)
        {
            if (ACL != null)
                ACL.DeleteACL(name);
            aclNames.Remove(name);
        }

        /// <summary>
        /// Evalua todas las ACLs configuradas para determinar si el trafico entre
        /// dos nodos esta permitido. Retorna true solo si todas las ACLs permiten el paso.
        /// </summary>
        /// <param name="source">Nodo origen.</param>
        /// <param name="dest">Nodo destino.</param>
        /// <param name="srcPort">Puerto origen (opcional).</param>
        /// <param name="dstPort">Puerto destino (opcional).</param>
        /// <param name="protocol">Protocolo de capa superior.</param>
        /// <returns>True si el trafico esta permitido por todas las ACLs.</returns>
        private bool ValidateACL(NetworkNode source, NetworkNode dest,
            int? srcPort = null, int? dstPort = null, string protocol = "icmp")
        {
            if (ACL == null || aclNames.Count == 0)
                return true;

            string sourceIP = source.IpAddress;
            string destIP = dest.IpAddress;

            // Sin IPs válidas no se puede evaluar ACL — permitir paso
            if (string.IsNullOrEmpty(sourceIP) || string.IsNullOrEmpty(destIP))
                return true;

            // Evaluar cada ACL por separado usando CheckPacket
            foreach (var aclName in aclNames)
            {
                if (!ACL.CheckPacket(aclName, sourceIP, destIP, srcPort, dstPort, protocol))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Traduce una IP interna a su correspondiente IP externa segun NAT.
        /// Si NAT no esta habilitado, retorna la IP original sin cambios.
        /// </summary>
        /// <param name="internalIP">Direccion IP interna a traducir.</param>
        /// <returns>IP externa traducida, o la misma IP si NAT no esta activo.</returns>
        public string TranslateSourceIP(string internalIP)
        {
            if (NAT == null || !NAT.IsEnabled)
                return internalIP;

            return NAT.TranslateInternalToExternal(internalIP);
        }

        /// <summary>
        /// Traduce una IP externa a su correspondiente IP interna segun NAT.
        /// Si NAT no esta habilitado, retorna la IP original sin cambios.
        /// </summary>
        /// <param name="externalIP">Direccion IP externa a traducir.</param>
        /// <returns>IP interna traducida, o la misma IP si NAT no esta activo.</returns>
        public string TranslateDestIP(string externalIP)
        {
            if (NAT == null || !NAT.IsEnabled)
                return externalIP;

            return NAT.TranslateExternalToInternal(externalIP);
        }
    }
}