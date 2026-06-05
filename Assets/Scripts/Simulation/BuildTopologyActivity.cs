using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using SimRedes.Network;

namespace SimRedes.Simulation
{
    /// <summary>
    /// Enumera los tipos de topologia de red detectables: Estrella, Bus, Anillo, Arbol y Malla.
    /// </summary>
    public enum TopologyType
    {
        Ninguna,
        Estrella,
        Bus,
        Anillo,
        Arbol,
        Malla
    }

    /// <summary>
    /// Actividad que detecta y muestra la topologia de red actual basada en los nodos y enlaces presentes.
    /// Se suscribe a eventos de TopologyManager para reaccionar a cambios en tiempo real.
    /// </summary>
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

        /// <summary>
        /// Inicializa la actividad: localiza TopologyManager, se suscribe a eventos de topologia, encuentra los textos UI y realiza la deteccion inicial.
        /// </summary>
        private void Start()
        {
            topologyManager = Object.FindAnyObjectByType<TopologyManager>();
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

        /// <summary>
        /// Busca y asigna las referencias a los textos UI dentro del panel TopologyInfoPanel por nombre de GameObject.
        /// </summary>
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

        /// <summary>
        /// Se invoca cuando la topologia cambia. Redetecta el tipo de topologia y actualiza la UI.
        /// </summary>
        private void OnTopologyChanged()
        {
            UnityEngine.Debug.Log("[BuildTopology] Topologia cambiada, detectando...");
            if (autoDetectTopology)
            {
                DetectTopologyType();
            }
            UpdateUI();
        }

        /// <summary>
        /// Detecta el tipo de topologia actual analizando el grado de conexion de cada nodo y la cantidad de enlaces.
        /// Aplica reglas secuenciales: malla completa, estrella, anillo, bus lineal, arbol y malla parcial.
        /// </summary>
        private void DetectTopologyType()
        {
            var nodes = topologyManager.GetAllNodes();
            var links = topologyManager.GetAllLinks();

            if (nodes.Count < 2)
            {
                currentTopology = TopologyType.Ninguna;
                return;
            }

            int n = nodes.Count;
            int l = links.Count;

            var degree = new Dictionary<int, int>();
            foreach (var node in nodes) degree[node.DiscId] = 0;
            foreach (var link in links)
            {
                if (link.SourceNode == null || link.DestinationNode == null) continue;
                if (degree.ContainsKey(link.SourceNode.DiscId))
                    degree[link.SourceNode.DiscId]++;
                if (degree.ContainsKey(link.DestinationNode.DiscId))
                    degree[link.DestinationNode.DiscId]++;
            }

            int degree1 = degree.Values.Count(d => d == 1);
            int degree2 = degree.Values.Count(d => d == 2);
            int degreeNminus1 = degree.Values.Count(d => d == n - 1);

            // 1. MALLA completa
            int perfectMeshLinks = n * (n - 1) / 2;
            if (l == perfectMeshLinks)
            {
                currentTopology = TopologyType.Malla;
                return;
            }

            // 2. ESTRELLA
            if (degreeNminus1 == 1 && degree1 == n - 1)
            {
                currentTopology = TopologyType.Estrella;
                return;
            }

            // 3. ANILLO
            if (l == n && degree2 == n)
            {
                currentTopology = TopologyType.Anillo;
                return;
            }

            // 4. BUS lineal
            if (l == n - 1 && degree1 == 2 && degree2 == n - 2)
            {
                currentTopology = TopologyType.Bus;
                return;
            }

            // 5. ARBOL
            if (l == n - 1)
            {
                currentTopology = TopologyType.Arbol;
                return;
            }

            // 6. MALLA parcial
            if (l > n - 1)
            {
                currentTopology = TopologyType.Malla;
                    return;
            }

            // 7. Por defecto
            currentTopology = TopologyType.Bus;
        }

        /// <summary>
        /// Actualiza todos los textos UI con la topologia detectada, conteo de enlaces y dispositivos por tipo.
        /// </summary>
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

        /// <summary>
        /// Retorna el texto de instrucciones para el usuario sobre como construir la topologia.
        /// </summary>
        private string GetInstructions()
        {
            return "Instructions:\n" +
                   "1. Coloca los discos en la mesa\n" +
                   "2. Acerca los dispositivos para conectar\n" +
                   "3. Observa la topología detectada";
        }

        /// <summary>
        /// Convierte el valor del enum TopologyType a su nombre legible en espanol.
        /// </summary>
        /// <param name="type">Tipo de topologia a mostrar.</param>
        /// <returns>Cadena legible con el nombre de la topologia.</returns>
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

        /// <summary>
        /// Retorna el tipo de topologia actualmente detectado.
        /// </summary>
        /// <returns>Tipo de topologia vigente.</returns>
        public TopologyType GetCurrentTopology()
        {
            return currentTopology;
        }

        /// <summary>
        /// Verifica si todos los enlaces de la topologia son funcionales y existe al menos un enlace.
        /// </summary>
        /// <returns>True si todos los enlaces son funcionales y hay al menos uno.</returns>
        public bool IsFullyConnected()
        {
            if (topologyManager == null) return false;
            var nodes = topologyManager.GetAllNodes();
            if (nodes.Count < 2) return false;

            var links = topologyManager.GetAllLinks();
            return links.All(l => l.IsFunctional()) && links.Count > 0;
        }

        /// <summary>
        /// Limpia la suscripcion al evento OnTopologyChanged al destruirse el componente.
        /// </summary>
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