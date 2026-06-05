using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using SimRedes.Network;

namespace SimRedes.UI
{
    public class NodeVisualizer : MonoBehaviour
    {
        [Header("References")]
        public Transform nodeContainer;
        public Transform linkContainer;

        private Dictionary<int, GameObject> nodeObjects = new Dictionary<int, GameObject>();
        private TopologyManager topology;
        private NodeInteractionController interactionController;

        private void Start()
        {
            topology = Object.FindAnyObjectByType<TopologyManager>();
            interactionController = Object.FindAnyObjectByType<NodeInteractionController>();

            if (topology != null)
            {
                topology.OnNodeAdded += OnNodeAdded;
                topology.OnNodeRemoved += OnNodeRemoved;
                topology.OnTopologyChanged += OnTopologyChanged;

                // Sincronizar nodos y enlaces existentes (por si se agregaron antes de que
                // este Start() se ejecutara, ej: FindFaultActivity.LoadScenario en Awake/ConnectUI)
                foreach (var node in topology.GetAllNodes())
                    OnNodeAdded(node);
                DrawLinks();
            }
        }

        private void OnNodeAdded(NetworkNode node)
        {
            CreateNodeVisual(node);
        }

        private void OnNodeRemoved(NetworkNode node)
        {
            if (nodeObjects.TryGetValue(node.DiscId, out var obj))
            {
                if (obj != null) Destroy(obj);
                nodeObjects.Remove(node.DiscId);
            }
            DrawLinks();
        }

        private void OnTopologyChanged()
        {
            DrawLinks();
        }

        private void CreateNodeVisual(NetworkNode node)
        {
            if (nodeObjects.ContainsKey(node.DiscId)) return;
            if (nodeContainer == null)
            {
                Debug.LogError("[NodeVisualizer] nodeContainer es null, no se puede crear nodo visual");
                return;
            }

            Color nodeColor = UIComponents.Colors.GetColorForDeviceType(node.Type);
            string labelText = GetLabelForDeviceType(node.Type);
            int capturedDiscId = node.DiscId;

            GameObject nodeObj = new GameObject($"Node_{node.Name}");
            nodeObj.transform.SetParent(nodeContainer, false);

            RectTransform rect = nodeObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = node.Position;
            rect.sizeDelta = new Vector2(80, 80);

            Image img = nodeObj.AddComponent<Image>();
            img.color = nodeColor;
            img.sprite = UIComponents.CreateDeviceIcon(64, nodeColor);
            img.type = Image.Type.Simple;

            var trigger = nodeObj.AddComponent<EventTrigger>();
            AddClickEvent(trigger, () => OnNodeClicked(capturedDiscId));

            Outline outline = nodeObj.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, 2);

            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(nodeObj.transform, false);
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            Text text = labelObj.AddComponent<Text>();
            text.text = labelText;
            text.color = Color.white;
            text.fontSize = 14;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = UIComponents.GetFont();

            nodeObjects[node.DiscId] = nodeObj;

            DrawLinks();
        }

        private string GetLabelForDeviceType(SimRedes.Network.DeviceType type)
        {
            switch (type)
            {
                case SimRedes.Network.DeviceType.Router:
                    return "Router";
                case SimRedes.Network.DeviceType.Switch:
                    return "Switch";
                case SimRedes.Network.DeviceType.PC:
                    return "PC";
                default:
                    return "Disco";
            }
        }

        public void DrawLinks()
        {
            if (linkContainer == null)
            {
                Debug.LogWarning("[NodeVisualizer] linkContainer es null, no se pueden dibujar enlaces");
                return;
            }
            foreach (Transform child in linkContainer)
            {
                if (child != null) Destroy(child.gameObject);
            }

            if (topology == null)
            {
                Debug.LogWarning("[NodeVisualizer] topology es null en DrawLinks");
                return;
            }

            var allLinks = topology.GetAllLinks();

            foreach (var link in allLinks)
            {
                if (link.SourceNode == null || link.DestinationNode == null)
                {
                    Debug.LogWarning("[NodeVisualizer] Enlace con nodo nulo detectado");
                    continue;
                }

                if (!nodeObjects.TryGetValue(link.SourceNode.DiscId, out var srcObj) ||
                    !nodeObjects.TryGetValue(link.DestinationNode.DiscId, out var dstObj))
                {
                    continue;
                }

                if (srcObj == null || dstObj == null) continue;

                CreateLinkLine(srcObj.GetComponent<RectTransform>(),
                              dstObj.GetComponent<RectTransform>(),
                              link.IsFunctional());
            }
        }

        private void CreateLinkLine(RectTransform from, RectTransform to, bool isActive)
        {
            GameObject lineObj = new GameObject("Link");
            lineObj.transform.SetParent(linkContainer, false);

            // Asegurar que la linea se renderice encima de otros elementos
            lineObj.transform.SetAsLastSibling();

            // Usar RawImage en lugar de Image - no necesita sprite y siempre renderiza
            RawImage lineImage = lineObj.AddComponent<RawImage>();
            Color lineColor = isActive ? Color.green : Color.red;
            lineImage.color = lineColor;

            // Usar textura blanca 1x1 compartida (evita crear cientos de Texture2D)
            lineImage.texture = UIComponents.GetSharedWhiteTexture();

            RectTransform rect = lineObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);

            Vector2 direction = to.anchoredPosition - from.anchoredPosition;
            float distance = direction.magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            rect.sizeDelta = new Vector2(Mathf.Max(distance, 2f), 10);
            rect.anchoredPosition = from.anchoredPosition + direction / 2f;
            rect.localEulerAngles = new Vector3(0, 0, angle);
        }

        private void AddClickEvent(EventTrigger trigger, UnityEngine.Events.UnityAction action)
        {
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener((data) => action());
            trigger.triggers.Add(entry);
        }

        private void OnNodeClicked(int discId)
        {
            CleanupNullReferences();

            UnityEngine.Debug.Log($"[NodeVisualizer] Nodo clickeado: {discId}");

            if (topology == null) return;

            var node = topology.GetNode(discId);
            if (node == null) return;

            HighlightSelectedNode(discId);

            if (interactionController != null)
            {
                if (interactionController.IsLinkModeActive())
                {
                    interactionController.CloseIPConfigPanelPublic();
                    interactionController.HandleNodeClick(discId);
                }
                else if (interactionController.IsPingModeActive())
                {
                    interactionController.CloseIPConfigPanelPublic();
                    interactionController.HandleNodeClick(discId);
                }
                else if (interactionController.IsIPConfigPanelOpen() && discId != interactionController.GetCurrentIPConfigNodeDiscId())
                {
                    interactionController.CloseIPConfigPanelPublic();
                    interactionController.ShowIPConfigPanel(node, discId);
                }
                else if (!interactionController.IsIPConfigPanelOpen())
                {
                    interactionController.ShowIPConfigPanel(node, discId);
                }
            }
        }

        private void HighlightSelectedNode(int discId)
        {
            CleanupNullReferences();

            foreach (var kvp in nodeObjects)
            {
                if (kvp.Value == null) continue;

                var outline = kvp.Value.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.effectColor = (kvp.Key == discId) ? Color.yellow : Color.black;
                    outline.effectDistance = (kvp.Key == discId) ? new Vector2(4, 4) : new Vector2(2, 2);
                }
            }
        }

        private void CleanupNullReferences()
        {
            var keysToRemove = new List<int>();
            foreach (var kvp in nodeObjects)
            {
                if (kvp.Value == null)
                    keysToRemove.Add(kvp.Key);
            }
            foreach (var key in keysToRemove)
            {
                nodeObjects.Remove(key);
            }
        }

        public void UpdateNodeLabel(int discId, string newLabel)
        {
            if (nodeObjects.TryGetValue(discId, out var nodeObj))
            {
                var labelObj = nodeObj.transform.Find("Label");
                if (labelObj != null)
                {
                    var text = labelObj.GetComponent<Text>();
                    if (text != null)
                    {
                        text.text = newLabel;
                        if (newLabel.Contains("\n"))
                        {
                            text.fontSize = 10;
                        }
                        else
                        {
                            text.fontSize = 14;
                        }
                    }

                    var labelRect = labelObj.GetComponent<RectTransform>();
                    if (labelRect != null)
                    {
                        labelRect.sizeDelta = new Vector2(120, newLabel.Contains("\n") ? 60 : 40);
                    }
                }
            }
        }

        public void SelectNodeByDiscId(int discId)
        {
            HighlightSelectedNode(discId);
        }

        private void OnDestroy()
        {
            if (topology != null)
            {
                topology.OnNodeAdded -= OnNodeAdded;
                topology.OnNodeRemoved -= OnNodeRemoved;
                topology.OnTopologyChanged -= OnTopologyChanged;
            }
        }
    }
}