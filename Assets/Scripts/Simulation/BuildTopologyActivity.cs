using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using SimRedes.Network;

namespace SimRedes.Simulation
{
    public enum TopologyType
    {
        Ninguna,
        Estrella,
        Bus,
        Anillo,
        Arbol,
        Malla
    }

    public class BuildTopologyActivity : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text topologyTypeText;
        [SerializeField] private Text linkCountText;
        [SerializeField] private Text routerCountText;
        [SerializeField] private Text switchCountText;
        [SerializeField] private Text pcCountText;
        [SerializeField] private Text instructionsText;

        [Header("Settings")]
        [SerializeField] private bool autoDetectTopology = true;

        private TopologyManager topologyManager;
        private TopologyType currentTopology = TopologyType.Ninguna;

        private void Start()
        {
            topologyManager = FindObjectOfType<TopologyManager>();
            if (topologyManager != null)
            {
                topologyManager.OnTopologyChanged += OnTopologyChanged;
                topologyManager.OnNodeAdded += (node) => { DetectTopologyType(); UpdateUI(); };
                topologyManager.OnNodeRemoved += (node) => { DetectTopologyType(); UpdateUI(); };
                topologyManager.OnLinkAdded += (link) => { DetectTopologyType(); UpdateUI(); };
            }

            FindUITexts();
            DetectTopologyType();
            UpdateUI();
        }

        private void FindUITexts()
        {
            var panel = GameObject.Find("TopologyInfoPanel");
            if (panel != null)
            {
                var texts = panel.GetComponentsInChildren<Text>();
                foreach (var text in texts)
                {
                    UnityEngine.Debug.Log($"[FindUITexts] Found text: name={text.gameObject.name}, text={text.text}");

                    if (text.gameObject.name == "TopologyTypeText")
                    {
                        topologyTypeText = text;
                        UnityEngine.Debug.Log("[FindUITexts] Assigned topologyTypeText");
                    }
                    else if (text.gameObject.name == "LinkCountText")
                    {
                        linkCountText = text;
                    }
                    else if (text.gameObject.name == "RouterCountText")
                    {
                        routerCountText = text;
                    }
                    else if (text.gameObject.name == "SwitchCountText")
                    {
                        switchCountText = text;
                    }
                    else if (text.gameObject.name == "PCCountText")
                    {
                        pcCountText = text;
                    }
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("[FindUITexts] TopologyInfoPanel not found!");
            }
        }

        private void OnTopologyChanged()
        {
            UnityEngine.Debug.Log("[BuildTopology] Topologia cambiada, detectando...");
            if (autoDetectTopology)
            {
                DetectTopologyType();
            }
            UpdateUI();
        }

        private void DetectTopologyType()
        {
            var nodes = topologyManager.GetAllNodes();
            var links = topologyManager.GetAllLinks();

            UnityEngine.Debug.Log($"[Detect] Nodos: {nodes.Count}, Enlaces: {links.Count}");

            if (nodes.Count == 0)
            {
                currentTopology = TopologyType.Ninguna;
            }
            else if (nodes.Count == 1)
            {
                currentTopology = TopologyType.Estrella;
            }
            else
            {
                int routers = nodes.Count(n => n.Type == SimRedes.Network.DeviceType.Router);
                int switches = nodes.Count(n => n.Type == SimRedes.Network.DeviceType.Switch);
                int pcs = nodes.Count(n => n.Type == SimRedes.Network.DeviceType.PC);

                int linkCount = links.Count;
                int maxPossibleLinks = (nodes.Count * (nodes.Count - 1)) / 2;
                double connectivityRatio = maxPossibleLinks > 0 ? (double)linkCount / maxPossibleLinks : 0;

                UnityEngine.Debug.Log($"[Detect] Routers:{routers} Switches:{switches} PCs:{pcs} Links:{linkCount} Ratio:{connectivityRatio}");

                int perfectMeshLinks = nodes.Count * (nodes.Count - 1) / 2;

                if (linkCount == perfectMeshLinks)
                {
                    currentTopology = TopologyType.Malla;
                }
                else if (linkCount == nodes.Count)
                {
                    currentTopology = TopologyType.Anillo;
                }
                else if (switches > 0 && (routers == 1 || switches >= nodes.Count - 1))
                {
                    currentTopology = TopologyType.Estrella;
                }
                else if (routers >= 2 && switches > 0)
                {
                    currentTopology = TopologyType.Arbol;
                }
                else if (routers > 1 && linkCount < nodes.Count)
                {
                    currentTopology = TopologyType.Bus;
                }
                else
                {
                    currentTopology = TopologyType.Bus;
                }
            }

            UnityEngine.Debug.Log($"[Detect] Topologia detectada: {currentTopology}");
        }

        private void UpdateUI()
        {
            if (topologyManager == null)
            {
                UnityEngine.Debug.LogWarning("[UpdateUI] topologyManager is null");
                return;
            }

            var nodes = topologyManager.GetAllNodes();
            var links = topologyManager.GetAllLinks();

            UnityEngine.Debug.Log($"[UpdateUI] Updating UI - Topology: {currentTopology}, Nodes: {nodes.Count}, Links: {links.Count}");

            if (topologyTypeText != null)
            {
                topologyTypeText.text = $"Topologia: {GetTopologyDisplayName(currentTopology)}";
                UnityEngine.Debug.Log($"[UpdateUI] Updated topologyTypeText to: {topologyTypeText.text}");
            }
            else
            {
                UnityEngine.Debug.LogWarning("[UpdateUI] topologyTypeText is null!");
            }

            if (linkCountText != null)
            {
                linkCountText.text = $"Enlaces: {links.Count}";
            }

            if (routerCountText != null)
            {
                routerCountText.text = $"  Routers: {nodes.Count(n => n.Type == SimRedes.Network.DeviceType.Router)}";
            }

            if (switchCountText != null)
            {
                switchCountText.text = $"  Switches: {nodes.Count(n => n.Type == SimRedes.Network.DeviceType.Switch)}";
            }

            if (pcCountText != null)
            {
                pcCountText.text = $"  PCs: {nodes.Count(n => n.Type == SimRedes.Network.DeviceType.PC)}";
            }

            if (instructionsText != null)
            {
                instructionsText.text = GetInstructions();
            }
        }

        private string GetInstructions()
        {
            return "Instructions:\n" +
                   "1. Coloca los discos en la mesa\n" +
                   "2. Acerca los dispositivos para conectar\n" +
                   "3. Observa la topología detectada";
        }

        private string GetTopologyDisplayName(TopologyType type)
        {
            switch (type)
            {
                case TopologyType.Estrella: return "Estrella";
                case TopologyType.Bus: return "Bus";
                case TopologyType.Anillo: return "Anillo";
                case TopologyType.Arbol: return "Arbol";
                case TopologyType.Malla: return "Malla";
                default: return "Sin topologia";
            }
        }

        public TopologyType GetCurrentTopology()
        {
            return currentTopology;
        }

        public bool IsFullyConnected()
        {
            if (topologyManager == null) return false;
            var nodes = topologyManager.GetAllNodes();
            if (nodes.Count < 2) return false;

            var links = topologyManager.GetAllLinks();
            return links.All(l => l.IsFunctional()) && links.Count > 0;
        }

        private void OnDestroy()
        {
            if (topologyManager != null)
            {
                topologyManager.OnTopologyChanged -= OnTopologyChanged;
            }
        }

        private float updateInterval = 0.5f;
        private float lastUpdateTime = 0f;

        private void Update()
        {
            if (topologyTypeText == null || linkCountText == null ||
                routerCountText == null || switchCountText == null || pcCountText == null)
            {
                FindUITexts();
            }

            if (Time.time - lastUpdateTime >= updateInterval)
            {
                lastUpdateTime = Time.time;
                UpdateUI();
            }
        }
    }
}