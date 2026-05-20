using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimRedes.Network;

namespace SimRedes.UI
{
    public class TopologyVisualizer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform nodeContainer;
        [SerializeField] private Transform linkContainer;
        [SerializeField] private LineRenderer lineRendererPrefab;

        [Header("Settings")]
        [SerializeField] private float nodeScale = 50f;
        [SerializeField] private Color linkColor = Color.white;
        [SerializeField] private Color activeLinkColor = Color.green;
        [SerializeField] private Color inactiveLinkColor = Color.red;

        private Dictionary<int, GameObject> nodeVisualObjects = new Dictionary<int, GameObject>();
        private List<LineRenderer> linkLines = new List<LineRenderer>();

        private void Start()
        {
            var topologyManager = FindObjectOfType<TopologyManager>();
            if (topologyManager != null)
            {
                topologyManager.OnNodeAdded += OnNodeAdded;
                topologyManager.OnNodeRemoved += OnNodeRemoved;
                topologyManager.OnLinkAdded += OnLinkAdded;
                topologyManager.OnTopologyChanged += OnTopologyChanged;
            }

            UpdateVisualization();
        }

        private void OnNodeAdded(NetworkNode node)
        {
            CreateNodeVisual(node);
        }

        private void OnNodeRemoved(NetworkNode node)
        {
            if (nodeVisualObjects.TryGetValue(node.DiscId, out var visual))
            {
                Destroy(visual);
                nodeVisualObjects.Remove(node.DiscId);
            }
            UpdateVisualization();
        }

        private void OnLinkAdded(NetworkLink link)
        {
            CreateLinkVisual(link);
        }

        private void OnTopologyChanged()
        {
            UpdateVisualization();
        }

        private void CreateNodeVisual(NetworkNode node)
        {
            if (nodeVisualObjects.ContainsKey(node.DiscId))
                return;

            var config = DiscConfiguration.GetConfiguration(node.DiscId);
            GameObject nodeObj = new GameObject($"Node_{node.DiscId}");
            nodeObj.transform.SetParent(nodeContainer);
            nodeObj.transform.localPosition = node.Position;

            var rectTransform = nodeObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(nodeScale, nodeScale);

            var image = nodeObj.AddComponent<Image>();
            image.color = config.DisplayColor;

            var text = nodeObj.AddComponent<Text>();
            text.text = config.Label;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 14;

            nodeVisualObjects.Add(node.DiscId, nodeObj);
        }

        private void CreateLinkVisual(NetworkLink link)
        {
            var line = Instantiate(lineRendererPrefab, linkContainer);
            line.positionCount = 2;
            line.startColor = activeLinkColor;
            line.endColor = activeLinkColor;
            linkLines.Add(line);
            UpdateLinkPositions(link, line);
        }

        private void UpdateVisualization()
        {
            var topologyManager = TopologyManager.Instance;
            if (topologyManager == null) return;

            foreach (var link in topologyManager.GetAllLinks())
            {
                bool linkExists = false;
                foreach (var line in linkLines)
                {
                    if (UpdateLinkPositions(link, line))
                    {
                        line.enabled = true;
                        line.startColor = link.IsFunctional() ? activeLinkColor : inactiveLinkColor;
                        line.endColor = link.IsFunctional() ? activeLinkColor : inactiveLinkColor;
                        linkExists = true;
                        break;
                    }
                }
            }
        }

        private bool UpdateLinkPositions(NetworkLink link, LineRenderer line)
        {
            if (link.SourceNode == null || link.DestinationNode == null)
                return false;

            line.SetPosition(0, link.SourceNode.Position);
            line.SetPosition(1, link.DestinationNode.Position);
            return true;
        }

        private void OnDestroy()
        {
            var topologyManager = FindObjectOfType<TopologyManager>();
            if (topologyManager != null)
            {
                topologyManager.OnNodeAdded -= OnNodeAdded;
                topologyManager.OnNodeRemoved -= OnNodeRemoved;
                topologyManager.OnLinkAdded -= OnLinkAdded;
                topologyManager.OnTopologyChanged -= OnTopologyChanged;
            }
        }
    }
}