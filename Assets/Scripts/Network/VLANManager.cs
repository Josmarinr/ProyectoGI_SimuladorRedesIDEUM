using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimRedes.Network
{
    /// <summary>Administra la creación, asignación y comunicación de VLANs en la red.</summary>
    public class VLANManager
    {
        private Dictionary<int, List<NetworkNode>> vlans = new Dictionary<int, List<NetworkNode>>();
        private Dictionary<string, int> nodeVlanMap = new Dictionary<string, int>();
        private int nextVlanId = 10;

        /// <summary>ID de VLAN por defecto (1).</summary>
        public const int DEFAULT_VLAN = 1;
        /// <summary>Cantidad máxima de VLANs soportadas (4094).</summary>
        public const int MAX_VLANS = 4094;

        /// <summary>Crea el gestor de VLANs e inicializa la VLAN por defecto.</summary>
        public VLANManager()
        {
            CreateVLAN(DEFAULT_VLAN);
        }

        /// <summary>Crea una nueva VLAN con el ID especificado o uno auto-asignado.</summary>
        /// <param name="vlanId">ID de VLAN (0 para auto-asignado).</param>
        /// <returns>ID de la VLAN creada.</returns>
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

        /// <summary>Elimina una VLAN y reasigna sus nodos a la VLAN por defecto.</summary>
        /// <param name="vlanId">ID de la VLAN a eliminar.</param>
        public void DeleteVLAN(int vlanId)
        {
            if (vlanId == DEFAULT_VLAN)
            {
                UnityEngine.Debug.LogWarning("[VLAN] Cannot delete default VLAN 1");
                return;
            }

            if (vlans.ContainsKey(vlanId))
            {
                foreach (var node in vlans[vlanId].ToList())
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

        /// <summary>Asigna un nodo a una VLAN, moviéndolo desde su VLAN actual si es necesario.</summary>
        /// <param name="node">Nodo a asignar.</param>
        /// <param name="vlanId">ID de la VLAN destino.</param>
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

        /// <summary>Obtiene el ID de la VLAN a la que pertenece un nodo.</summary>
        /// <param name="node">Nodo a consultar.</param>
        /// <returns>ID de VLAN (por defecto 1 si no está asignado).</returns>
        public int GetNodeVLAN(NetworkNode node)
        {
            return nodeVlanMap.TryGetValue(node.Id, out int vlan) ? vlan : DEFAULT_VLAN;
        }

        /// <summary>Obtiene los nodos pertenecientes a una VLAN.</summary>
        /// <param name="vlanId">ID de la VLAN.</param>
        /// <returns>Lista de nodos o lista vacía si la VLAN no existe.</returns>
        public List<NetworkNode> GetNodesInVLAN(int vlanId)
        {
            if (vlans.ContainsKey(vlanId))
            {
                return new List<NetworkNode>(vlans[vlanId]);
            }
            return new List<NetworkNode>();
        }

        /// <summary>Verifica si dos nodos pueden comunicarse (deben estar en la misma VLAN).</summary>
        /// <param name="source">Nodo origen.</param>
        /// <param name="dest">Nodo destino.</param>
        /// <returns>True si ambos nodos están en la misma VLAN.</returns>
        public bool CanCommunicate(NetworkNode source, NetworkNode dest)
        {
            int sourceVlan = GetNodeVLAN(source);
            int destVlan = GetNodeVLAN(dest);

            return sourceVlan == destVlan;
        }

        /// <summary>Obtiene una lista de todos los IDs de VLAN configurados.</summary>
        /// <returns>Lista de IDs de VLAN.</returns>
        public List<int> GetAllVLANs()
        {
            return new List<int>(vlans.Keys);
        }

        /// <summary>Obtiene un resumen textual de todas las VLANs configuradas.</summary>
        /// <returns>Cadena con el resumen de VLANs.</returns>
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