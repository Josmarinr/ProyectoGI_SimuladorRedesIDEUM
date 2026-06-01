using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using SimRedes.Network;
using SimRedes.Simulation;
using UIColors = SimRedes.UI.UIComponents.Colors;
using UIComp = SimRedes.UI.UIComponents;

namespace SimRedes.UI
{
    public class DevicePanelController : MonoBehaviour
    {
        private int selectedNodeForRemoval = -1;
        private TopologyManager topology;
        private NodeVisualizer visualizer;
        private Canvas canvas;

        private float scoreUpdateTimer = 0f;

        private void Start()
        {
            topology = Object.FindAnyObjectByType<TopologyManager>();
            visualizer = Object.FindAnyObjectByType<NodeVisualizer>();
            canvas = Object.FindAnyObjectByType<Canvas>();
        }

        private void Update()
        {
            scoreUpdateTimer += Time.deltaTime;
            if (scoreUpdateTimer >= 1f)
            {
                scoreUpdateTimer = 0f;
                UpdateScoreDisplay();
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && selectedNodeForRemoval != -1)
            {
                var devicePanel = GameObject.Find("DevicesPanel");
                if (devicePanel != null)
                {
                    var rt = devicePanel.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        Vector2 mousePos = Mouse.current.position.ReadValue();
                        Vector2 localPoint;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, mousePos, null, out localPoint);

                        if (!rt.rect.Contains(localPoint))
                        {
                            selectedNodeForRemoval = -1;
                            UpdateDevicesVisualState();
                        }
                    }
                }
            }
        }

        public void OnDeviceItemClicked(int index)
        {
            if (topology == null) topology = Object.FindAnyObjectByType<TopologyManager>();
            if (topology == null) return;

            var nodes = topology.GetAllNodes();
            if (index >= nodes.Count) return;

            var node = nodes[index];

            if (selectedNodeForRemoval == node.DiscId)
            {
                RemoveSelectedNode();
                return;
            }

            var ipConfigCtrl = Object.FindAnyObjectByType<IPConfigController>();
            if (ipConfigCtrl != null && ipConfigCtrl.IsIPConfigPanelOpen())
            {
                ipConfigCtrl.CloseIPConfigPanelPublic();
            }

            selectedNodeForRemoval = node.DiscId;
            UpdateDevicesList();

            if (visualizer == null) visualizer = Object.FindAnyObjectByType<NodeVisualizer>();
            if (visualizer != null)
            {
                visualizer.SelectNodeByDiscId(node.DiscId);
            }

            var linkCtrl = Object.FindAnyObjectByType<LinkModeController>();
            if (linkCtrl != null && linkCtrl.IsLinkModeActive())
            {
                var pingCtrl = Object.FindAnyObjectByType<PingModeController>();
                if (pingCtrl != null && pingCtrl.IsPingModeActive())
                {
                    pingCtrl.HandlePingNodeClick(node);
                }
                else
                {
                    linkCtrl.HandleNodeLinkClick(node.DiscId);
                }
            }
            else
            {
                if (ipConfigCtrl != null)
                {
                    ipConfigCtrl.ShowIPConfigPanel(node, node.DiscId);
                }
            }
        }

        public void RemoveSelectedNode()
        {
            if (selectedNodeForRemoval == -1) return;

            if (topology == null) topology = Object.FindAnyObjectByType<TopologyManager>();
            if (topology != null)
            {
                topology.RemoveNode(selectedNodeForRemoval);
                Debug.Log($"[DevicePanelController] Nodo eliminado: {selectedNodeForRemoval}");
            }

            var ipConfigCtrl = Object.FindAnyObjectByType<IPConfigController>();
            if (ipConfigCtrl != null && ipConfigCtrl.IsIPConfigPanelOpen())
            {
                ipConfigCtrl.CloseIPConfigPanelPublic();
            }

            selectedNodeForRemoval = -1;
        }

        public void RemoveSelectedNodePublic()
        {
            RemoveSelectedNode();
        }

        public void ClearSelectedNode()
        {
            selectedNodeForRemoval = -1;
        }

        public void RefreshDevicesPanel()
        {
            var existingPanel = GameObject.Find("DevicesPanel");
            if (existingPanel != null)
            {
                Destroy(existingPanel);
            }

            if (canvas != null)
            {
                CreateDevicesPanel(canvas.transform);
            }
        }

        public void UpdateScoreDisplay()
        {
            var scorePanel = GameObject.Find("ScorePanel");
            if (scorePanel == null) return;

            var scoring = ScoringSystem.Instance;
            if (scoring == null) return;

            var scoreText = scorePanel.transform.Find("ScoreText")?.GetComponent<Text>();
            if (scoreText != null)
                scoreText.text = scoring.GetCurrentScore().ToString();
        }

        private void CreateDevicesPanel(Transform canvasTransform)
        {
            var existingPanel = GameObject.Find("DevicesPanel");
            if (existingPanel != null) Destroy(existingPanel);

            Font arialFont = GetFont();

            GameObject panelObj = UIPanelFactory.CreateDevicesPanel(canvasTransform,
                topology != null ? topology.GetAllNodes().Count : 0, OnDeviceItemClicked);

            if (topology != null)
            {
                var nodes = topology.GetAllNodes();
                float startY = 130f;
                for (int i = 0; i < nodes.Count; i++)
                {
                    int index = i;
                    float yPos = startY - (i * 45);
                    CreateDeviceItem(panelObj.transform, index, yPos, arialFont);
                }
            }
        }

        private void CreateDeviceItem(Transform parent, int index, float y, Font font)
        {
            var itemObj = new GameObject($"DeviceItem_{index}");
            itemObj.transform.SetParent(parent, false);
            itemObj.SetActive(false);

            var itemRect = itemObj.AddComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0.5f, 0.5f);
            itemRect.anchorMax = new Vector2(0.5f, 0.5f);
            itemRect.anchoredPosition = new Vector2(0, y);
            itemRect.sizeDelta = new Vector2(260, 45);

            var bgImg = itemObj.AddComponent<Image>();
            Texture2D bgTex = UIComp.CreateRoundedRectTexture(260, 45, 10, UIColors.surfaceElevated, UIColors.borderAccent, 1f);
            bgImg.sprite = Sprite.Create(bgTex, new Rect(0, 0, 260, 45), new Vector2(0.5f, 0.5f), 100);
            bgImg.type = Image.Type.Sliced;

            var btn = itemObj.AddComponent<Button>();
            btn.colors = GetButtonColors(UIColors.surfaceElevated);

            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(itemObj.transform, false);
            var iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(25, 0);
            iconRect.sizeDelta = new Vector2(24, 24);

            var iconImg = iconObj.AddComponent<Image>();
            iconImg.color = Color.gray;

            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(itemObj.transform, false);
            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0.5f);
            nameRect.anchorMax = new Vector2(0f, 0.5f);
            nameRect.pivot = new Vector2(0f, 0.5f);
            nameRect.anchoredPosition = new Vector2(50, 0);
            nameRect.sizeDelta = new Vector2(140, 20);

            var nameText = nameObj.AddComponent<Text>();
            nameText.text = "";
            nameText.color = UIColors.textPrimary;
            nameText.fontSize = 14;
            nameText.alignment = TextAnchor.MiddleLeft;
            nameText.font = font;

            var arrowObj = new GameObject("Arrow");
            arrowObj.transform.SetParent(itemObj.transform, false);
            var arrowRect = arrowObj.AddComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(1f, 0.5f);
            arrowRect.anchorMax = new Vector2(1f, 0.5f);
            arrowRect.pivot = new Vector2(1f, 0.5f);
            arrowRect.anchoredPosition = new Vector2(-25, 0);
            arrowRect.sizeDelta = new Vector2(20, 20);

            var arrowText = arrowObj.AddComponent<Text>();
            arrowText.text = ">";
            arrowText.color = UIColors.textSecondary;
            arrowText.fontSize = 18;
            arrowText.alignment = TextAnchor.MiddleCenter;
            arrowText.font = font;

            int capturedIndex = index;
            btn.onClick.AddListener(() => OnDeviceItemClicked(capturedIndex));
        }

        public void UpdateDevicesList()
        {
            var devicesPanel = canvas?.transform.Find("DevicesPanel");
            if (devicesPanel == null) devicesPanel = GameObject.Find("DevicesPanel")?.transform;
            if (devicesPanel == null) return;

            if (topology == null) topology = Object.FindAnyObjectByType<TopologyManager>();
            if (topology == null) return;

            var nodes = topology.GetAllNodes();
            int maxItems = nodes.Count;

            for (int i = 0; i < maxItems; i++)
            {
                var itemObj = devicesPanel.Find($"DeviceItem_{i}");
                if (itemObj == null) continue;

                var iconObj = itemObj.Find("Icon");
                var nameObj = itemObj.Find("Name");

                var node = nodes[i];
                if (nameObj != null)
                {
                    var text = nameObj.GetComponent<Text>();
                    text.text = node.Name;
                    bool isSelectedForRemoval = (selectedNodeForRemoval == node.DiscId);
                    text.color = isSelectedForRemoval ? Color.red : UIColors.GetColorForDeviceType(node.Type);
                }
                if (iconObj != null)
                {
                    var img = iconObj.GetComponent<Image>();
                    img.color = UIColors.GetColorForDeviceType(node.Type);
                    img.sprite = CreateCircleIcon(24, UIColors.GetColorForDeviceType(node.Type));
                    img.gameObject.SetActive(true);
                }

                itemObj.gameObject.SetActive(true);

                var bgImg = itemObj.GetComponent<UnityEngine.UI.Image>();
                if (bgImg != null)
                {
                    bgImg.color = (selectedNodeForRemoval == node.DiscId)
                        ? new Color(0.6f, 0.15f, 0.15f, 1f)
                        : UIColors.surfaceElevated;
                }
            }

            for (int i = nodes.Count; i < 6; i++)
            {
                var itemObj = devicesPanel.Find($"DeviceItem_{i}");
                if (itemObj != null)
                {
                    itemObj.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateDevicesVisualState()
        {
            var devicesPanel = canvas?.transform.Find("DevicesPanel");
            if (devicesPanel == null) devicesPanel = GameObject.Find("DevicesPanel")?.transform;
            if (devicesPanel == null) return;

            if (topology == null) topology = Object.FindAnyObjectByType<TopologyManager>();
            if (topology == null) return;

            var nodes = topology.GetAllNodes();

            for (int i = 0; i < nodes.Count; i++)
            {
                var itemObj = devicesPanel.Find($"DeviceItem_{i}");
                if (itemObj == null) continue;

                var node = nodes[i];
                bool isSelected = (selectedNodeForRemoval == node.DiscId);

                var nameObj = itemObj.Find("Name");
                if (nameObj != null)
                {
                    var text = nameObj.GetComponent<Text>();
                    text.color = isSelected ? Color.red : UIColors.GetColorForDeviceType(node.Type);
                }

                var bgImg = itemObj.GetComponent<UnityEngine.UI.Image>();
                if (bgImg != null)
                {
                    bgImg.color = isSelected ? new Color(0.6f, 0.15f, 0.15f, 1f) : UIColors.surfaceElevated;
                }
            }
        }

        private Sprite CreateCircleIcon(int size, Color color)
        {
            Texture2D tex = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            int center = size / 2;
            int radius = center - 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (dist <= radius)
                        pixels[y * size + x] = color;
                    else
                        pixels[y * size + x] = Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private Font GetFont()
        {
            Font font = Font.CreateDynamicFontFromOSFont("Arial", 14);
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font;
        }

        private ColorBlock GetButtonColors(Color c)
        {
            return UIComp.GetButtonColors(c);
        }
    }
}
