using UnityEngine;
using UnityEngine.UI;
using SimRedes.Network;
using PingVis = SimRedes.UI.PingVisualizer;
using UIComp = SimRedes.UI.UIComponents;
using UIColors = SimRedes.UI.UIComponents.Colors;

namespace SimRedes.UI
{
    public class PingModeController : MonoBehaviour
    {
        private bool pingModeActive = false;
        private int pingSourceDiscId = -1;
        private Button pingBtn;
        private Text pingResultText;
        private TopologyManager topology;
        private Canvas canvas;

        private readonly Color activeButtonColor = new Color(0.2f, 0.7f, 0.3f, 1f);
        private readonly Color normalButtonColor = UIComponents.Colors.buttonNormal;

        private void Start()
        {
            topology = Object.FindAnyObjectByType<TopologyManager>();
            canvas = Object.FindAnyObjectByType<Canvas>();
        }

        public void StorePingReferences(Button btn, Text resultText)
        {
            pingBtn = btn;
            pingResultText = resultText;
        }

        public void TogglePingMode()
        {
            if (pingModeActive)
            {
                pingModeActive = false;
                pingSourceDiscId = -1;
                if (pingResultText != null)
                {
                    pingResultText.text = "Ping: Seleccionar origen";
                    pingResultText.color = UIColors.textSecondary;
                }
                ClosePingSelectionPanel();
                UpdatePingButtonColor();
                Debug.Log("[PingMode] Modo ping desactivado");
                return;
            }

            if (topology == null) topology = Object.FindAnyObjectByType<TopologyManager>();
            if (topology == null) return;

            var nodes = topology.GetAllNodes();
            if (nodes.Count < 2)
            {
                if (pingResultText != null)
                {
                    pingResultText.text = "Ping: Se necesitan 2+ nodos";
                    pingResultText.color = UIColors.textSecondary;
                }
                return;
            }

            pingModeActive = true;
            pingSourceDiscId = -1;
            if (pingResultText != null)
            {
                pingResultText.text = "Ping: Seleccionar origen";
                pingResultText.color = UIColors.textAccent;
            }
            ShowPingSelectionPanel();
            UpdatePingButtonColor();
            Debug.Log("[PingMode] Modo ping activado");
        }

        public bool IsPingModeActive()
        {
            return pingModeActive;
        }

        public void HandlePingNodeClick(NetworkNode node)
        {
            if (pingSourceDiscId == -1)
            {
                pingSourceDiscId = node.DiscId;
                if (pingResultText != null)
                {
                    pingResultText.text = $"Ping: Origen={node.Name}\nSelecciona DESTINO";
                    pingResultText.color = UIColors.textAccent;
                }
                Debug.Log($"[PingMode] Origen seleccionado: {node.Name}");
            }
            else if (pingSourceDiscId != node.DiscId)
            {
                int sourceId = pingSourceDiscId;
                int destDiscId = node.DiscId;
                string sourceName = node.Name;

                if (topology == null) topology = Object.FindAnyObjectByType<TopologyManager>();
                if (topology != null)
                {
                    var sourceNode = topology.GetNode(sourceId);
                    if (sourceNode != null) sourceName = sourceNode.Name;
                }

                if (pingResultText != null)
                {
                    pingResultText.text = $"Ping: Enviando {sourceName}->{node.Name}...";
                    pingResultText.color = UIColors.textAccent;
                }

                var pingVis = Object.FindAnyObjectByType<PingVis>();
                if (pingVis != null)
                {
                    pingVis.AnimatePing(sourceId, destDiscId, (success) => {
                        if (pingResultText != null)
                        {
                            pingResultText.text = success ? $"Ping: {sourceName}->{node.Name}: OK" : $"Ping: FALLO";
                            pingResultText.color = success ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.9f, 0.2f, 0.2f);
                        }
                    });
                }

                pingModeActive = false;
                pingSourceDiscId = -1;
                UpdatePingButtonColor();
                Debug.Log($"[PingMode] Ping: {sourceId} -> {destDiscId}");
            }
            else
            {
                if (pingResultText != null)
                {
                    pingResultText.text = "Ping: Mismo nodo, selecciona otro";
                    pingResultText.color = UIColors.textSecondary;
                }
            }
        }

        public void UpdatePingButtonColor()
        {
            if (pingBtn != null)
            {
                var img = pingBtn.GetComponent<Image>();
                if (img != null)
                {
                    img.color = pingModeActive ? activeButtonColor : normalButtonColor;
                }
            }
        }

        private Font GetFont()
        {
            return UIComp.GetFont();
        }

        private void ShowPingSelectionPanel()
        {
            ClosePingSelectionPanel();

            if (canvas == null) canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            Font arialFont = GetFont();

            GameObject panelObj = UIComp.CreateRoundedPanel(canvas.transform, new Vector2(280, 280), 15,
                UIColors.surfacePanel,
                UIColors.borderAccent);
            panelObj.name = "PingSelectionPanel";

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;

            UIComp.CreateMenuTitle(panelObj.transform, "PING", 22, new Vector2(0, 100), arialFont);

            string originLabel = pingSourceDiscId == -1 ? "ORIGEN" : topology.GetNode(pingSourceDiscId)?.Name ?? "ORIGEN";
            UIComp.CreateMenuButton(panelObj.transform, "OriginBtn", originLabel, new Vector2(0, 55), new Vector2(220, 40), arialFont, 14)
                .onClick.AddListener(() => ShowNodeListForSelection(true));

            UIComp.CreateMenuButton(panelObj.transform, "DestBtn", "DESTINO", new Vector2(0, 5), new Vector2(220, 40), arialFont, 14)
                .onClick.AddListener(() => ShowNodeListForSelection(false));

            UIComp.CreateMenuButton(panelObj.transform, "ExecutePingBtn", "HACER PING", new Vector2(0, -55), new Vector2(180, 45), arialFont, 16)
                .onClick.AddListener(() => ExecutePingFromSelection());

            UIComp.CreateMenuButton(panelObj.transform, "CancelPingBtn", "CERRAR", new Vector2(0, -105), new Vector2(120, 35), arialFont, 13)
                .onClick.AddListener(() => TogglePingMode());
        }

        private void ShowNodeListForSelection(bool isOrigin)
        {
            ClosePingSelectionPanel();

            if (canvas == null) canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            Font arialFont = GetFont();

            string titleText = isOrigin ? "SELECCIONAR ORIGEN" : "SELECCIONAR DESTINO";

            var nodes = topology.GetAllNodes();
            int maxItems = Mathf.Min(nodes.Count, 6);
            float panelHeight = 50 + (maxItems * 40) + 50;

            GameObject panelObj = UIComp.CreateRoundedPanel(canvas.transform, new Vector2(260, panelHeight), 15,
                UIColors.surfacePanel,
                UIColors.borderAccent);
            panelObj.name = "PingSelectionPanel";

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;

            UIComp.CreateMenuTitle(panelObj.transform, titleText, 18, new Vector2(0, panelHeight / 2 - 35), arialFont);

            float startY = panelHeight / 2 - 70;
            for (int i = 0; i < maxItems; i++)
            {
                int nodeIndex = i;
                Button itemBtn = UIComp.CreateMenuButton(panelObj.transform, $"NodeItem_{i}", nodes[i].Name, new Vector2(0, startY - (i * 40)), new Vector2(200, 35), arialFont, 13);
                itemBtn.onClick.AddListener(() => {
                    if (isOrigin)
                        SelectPingSource(nodeIndex);
                    else
                        SelectPingDest(nodeIndex);
                });
            }

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "VOLVER", new Vector2(0, -panelHeight / 2 + 30), new Vector2(100, 30), arialFont, 12);
            backBtn.onClick.AddListener(() => ShowPingSelectionPanel());
        }

        private void ClosePingSelectionPanel()
        {
            var panel = GameObject.Find("PingSelectionPanel");
            if (panel != null)
            {
                // B4: liberar los Sprite del panel antes de destruirlo
                UIComp.SafeDestroyPanelSprites(panel);
                Destroy(panel);
            }
        }

        private void SelectPingSource(int nodeIndex)
        {
            var nodes = topology.GetAllNodes();
            if (nodeIndex >= nodes.Count) return;

            pingSourceDiscId = nodes[nodeIndex].DiscId;
            if (pingResultText != null)
            {
                pingResultText.text = $"Ping: Origen={nodes[nodeIndex].Name}";
                pingResultText.color = UIColors.textAccent;
            }

            ShowPingSelectionPanel();
            Debug.Log($"[PingMode] Origen seleccionado: {nodes[nodeIndex].Name}");
        }

        private void SelectPingDest(int nodeIndex)
        {
            var nodes = topology.GetAllNodes();
            if (nodeIndex >= nodes.Count) return;

            if (pingSourceDiscId == nodes[nodeIndex].DiscId)
            {
                if (pingResultText != null)
                {
                    pingResultText.text = "Ping: Mismo nodo, selecciona otro";
                    pingResultText.color = UIColors.textSecondary;
                }
                return;
            }

            int sourceId = pingSourceDiscId;
            int destId = nodes[nodeIndex].DiscId;
            string sourceName = topology.GetNode(sourceId)?.Name ?? "Origen";

            if (pingResultText != null)
            {
                pingResultText.text = $"Ping: {sourceName}->{nodes[nodeIndex].Name}...";
                pingResultText.color = UIColors.textAccent;
            }

            var pingVis = Object.FindAnyObjectByType<PingVis>();
            if (pingVis != null)
            {
                pingVis.AnimatePing(sourceId, destId, (success) => {
                    if (pingResultText != null)
                    {
                        pingResultText.text = success ? $"Ping: OK" : "Ping: FALLO";
                        pingResultText.color = success ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.9f, 0.2f, 0.2f);
                    }
                });
            }

            TogglePingMode();
            Debug.Log($"[PingMode] Ping: {sourceId} -> {destId}");
        }

        private void ExecutePingFromSelection()
        {
            if (pingSourceDiscId == -1)
            {
                if (pingResultText != null)
                {
                    pingResultText.text = "Ping: Selecciona ORIGEN primero";
                    pingResultText.color = UIColors.textSecondary;
                }
                return;
            }

            ShowNodeListForSelection(false);
        }
    }
}
