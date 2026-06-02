using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Linq;
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
        private float panelSyncTimer = 0f;

        private void Update()
        {
            scoreUpdateTimer += Time.deltaTime;
            if (scoreUpdateTimer >= 1f)
            {
                scoreUpdateTimer = 0f;
                UpdateScoreDisplay();
            }

            panelSyncTimer += Time.deltaTime;
            if (panelSyncTimer >= 2f)
            {
                panelSyncTimer = 0f;
                var panel = GameObject.Find("DevicesPanel");
                if (panel != null)
                    UpdateDevicesList();
                TopologyInfoPanelSync();
            }

            // Detectar toque/click para salir del panel (funciona con mouse y tactil)
            bool pointerPressed = (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
                                  (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame);
            if (pointerPressed)
            {
                var devicePanel = GameObject.Find("DevicesPanel");
                if (devicePanel != null)
                {
                    var rt = devicePanel.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        Vector2 pointerPos;
                        if (Mouse.current != null)
                            pointerPos = Mouse.current.position.ReadValue();
                        else
                            pointerPos = Touchscreen.current.primaryTouch.position.ReadValue();

                        Vector2 localPoint;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, pointerPos, null, out localPoint);

                        if (!rt.rect.Contains(localPoint))
                        {
                            // Click/touch fuera del panel de dispositivos
                            if (selectedNodeForRemoval != -1)
                            {
                                selectedNodeForRemoval = -1;
                                UpdateDevicesVisualState();
                            }
                            // Si hay modo CONEXION activo, deseleccionar primer extremo
                            var linkCtrl = Object.FindAnyObjectByType<LinkModeController>();
                            if (linkCtrl != null && linkCtrl.IsLinkModeActive() && linkCtrl.GetLinkModeFirstNode() != -1)
                            {
                                linkCtrl.ClearFirstNode();
                                UpdateDevicesList();
                            }
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

            // Si el modo CONEXION/DESCONEXION esta activo, priorizar enlace
            var linkCtrl = Object.FindAnyObjectByType<LinkModeController>();
            if (linkCtrl != null && linkCtrl.IsLinkModeActive())
            {
                // No marcar como seleccion de eliminacion, solo pasar a HandleNodeLinkClick
                selectedNodeForRemoval = -1;
                linkCtrl.HandleNodeLinkClick(node.DiscId);
                // Actualizar panel para reflejar el estado del primer extremo o el enlace completado
                UpdateDevicesList();
                return;
            }

            // Comportamiento normal: seleccion/eliminacion
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

            if (ipConfigCtrl != null)
            {
                ipConfigCtrl.ShowIPConfigPanel(node, node.DiscId);
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

        public void RefreshDevicesPanel()
        {
            // Siempre destruir el panel existente para recrearlo con items frescos
            var existingPanel = GameObject.Find("DevicesPanel");
            if (existingPanel != null) Destroy(existingPanel);

            var foundCanvas = Object.FindAnyObjectByType<Canvas>();
            if (foundCanvas == null)
            {
                Debug.LogWarning("[DevicePanelController] No se encontro Canvas para crear DevicesPanel");
                return;
            }

            if (topology == null) topology = Object.FindAnyObjectByType<TopologyManager>();

            Font arialFont = GetFont();
            int nodeCount = topology != null ? topology.GetAllNodes().Count : 0;

            GameObject panelObj = UIPanelFactory.CreateDevicesPanel(foundCanvas.transform,
                nodeCount, OnDeviceItemClicked);

            if (topology != null)
            {
                var nodes = topology.GetAllNodes();
                float startY = 200f;
                for (int i = 0; i < nodes.Count; i++)
                {
                    int index = i;
                    float yPos = startY - (i * 58);
                    CreateDeviceItem(panelObj.transform, index, yPos, arialFont);
                }
            }

            UpdateDevicesList();
            TopologyInfoPanelSync();
            Debug.Log("[DevicePanelController] DevicesPanel creado con " + (topology?.GetAllNodes().Count ?? 0) + " nodos");
        }

        /// <summary>
        /// Sincroniza los contadores del panel TopologyInfoPanel con el estado actual de la topologia.
        /// </summary>
        private void TopologyInfoPanelSync()
        {
            if (topology == null) return;
            var nodes = topology.GetAllNodes();
            int routers = nodes.Count(n => n.Type == Network.DeviceType.Router);
            int switches = nodes.Count(n => n.Type == Network.DeviceType.Switch);
            int pcs = nodes.Count(n => n.Type == Network.DeviceType.PC);

            // Actualizar textos del panel de topologia si existe
            var topoPanel = GameObject.Find("TopologyInfoPanel");
            if (topoPanel == null) return;

            var routerText = topoPanel.transform.Find("RouterCountText")?.GetComponent<Text>();
            var switchText = topoPanel.transform.Find("SwitchCountText")?.GetComponent<Text>();
            var pcText = topoPanel.transform.Find("PCCountText")?.GetComponent<Text>();
            var linkText = topoPanel.transform.Find("LinkCountText")?.GetComponent<Text>();

            if (routerText != null) routerText.text = $"  Routers: {routers}";
            if (switchText != null) switchText.text = $"  Switches: {switches}";
            if (pcText != null) pcText.text = $"  PCs: {pcs}";
            if (linkText != null) linkText.text = $"Enlaces: {topology.GetAllLinks().Count}";
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
            itemRect.sizeDelta = new Vector2(320, 58);

            var bgImg = itemObj.AddComponent<Image>();
            Texture2D bgTex = UIComp.CreateRoundedRectTexture(320, 58, 10, UIColors.surfaceElevated, UIColors.borderAccent, 1f);
            bgImg.sprite = Sprite.Create(bgTex, new Rect(0, 0, 320, 58), new Vector2(0.5f, 0.5f), 100);
            bgImg.type = Image.Type.Sliced;

            var btn = itemObj.AddComponent<Button>();
            btn.colors = GetButtonColors(UIColors.surfaceElevated);

            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(itemObj.transform, false);
            var iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(30, 0);
            iconRect.sizeDelta = new Vector2(32, 32);

            var iconImg = iconObj.AddComponent<Image>();
            iconImg.color = Color.gray;

            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(itemObj.transform, false);
            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0.5f);
            nameRect.anchorMax = new Vector2(0f, 0.5f);
            nameRect.pivot = new Vector2(0f, 0.5f);
            nameRect.anchoredPosition = new Vector2(60, 0);
            nameRect.sizeDelta = new Vector2(200, 28);

            var nameText = nameObj.AddComponent<Text>();
            nameText.text = "";
            nameText.color = UIColors.textPrimary;
            nameText.fontSize = 18;
            nameText.alignment = TextAnchor.MiddleLeft;
            nameText.font = font;

            int capturedIndex = index;
            btn.onClick.AddListener(() => OnDeviceItemClicked(capturedIndex));
        }

        public void UpdateDevicesList()
        {
            var devicesPanel = GameObject.Find("DevicesPanel")?.transform;
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

                // Determinar si este nodo es el primero seleccionado en modo CONEXION/DESCONEXION
                var linkCtrl = Object.FindAnyObjectByType<LinkModeController>();
                bool isLinkModeFirst = (linkCtrl != null && linkCtrl.IsLinkModeActive() && linkCtrl.GetLinkModeFirstNode() == node.DiscId);
                bool isSelectedForRemoval = (selectedNodeForRemoval == node.DiscId);

                if (nameObj != null)
                {
                    var text = nameObj.GetComponent<Text>();
                    text.text = node.Name;
                    if (isLinkModeFirst)
                        text.color = new Color(0.2f, 0.8f, 0.3f, 1f); // verde para primer extremo
                    else if (isSelectedForRemoval)
                        text.color = Color.red;
                    else
                        text.color = UIColors.GetColorForDeviceType(node.Type);
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
                    if (isLinkModeFirst)
                        bgImg.color = new Color(0.1f, 0.4f, 0.15f, 1f); // fondo verde oscuro
                    else if (isSelectedForRemoval)
                        bgImg.color = new Color(0.6f, 0.15f, 0.15f, 1f);
                    else
                        bgImg.color = UIColors.surfaceElevated;
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
            var devicesPanel = GameObject.Find("DevicesPanel")?.transform;
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

                var linkCtrl = Object.FindAnyObjectByType<LinkModeController>();
                bool isLinkModeFirst = (linkCtrl != null && linkCtrl.IsLinkModeActive() && linkCtrl.GetLinkModeFirstNode() == node.DiscId);

                var nameObj = itemObj.Find("Name");
                if (nameObj != null)
                {
                    var text = nameObj.GetComponent<Text>();
                    if (isLinkModeFirst)
                        text.color = new Color(0.2f, 0.8f, 0.3f, 1f);
                    else if (isSelected)
                        text.color = Color.red;
                    else
                        text.color = UIColors.GetColorForDeviceType(node.Type);
                }

                var bgImg = itemObj.GetComponent<UnityEngine.UI.Image>();
                if (bgImg != null)
                {
                    if (isLinkModeFirst)
                        bgImg.color = new Color(0.1f, 0.4f, 0.15f, 1f);
                    else if (isSelected)
                        bgImg.color = new Color(0.6f, 0.15f, 0.15f, 1f);
                    else
                        bgImg.color = UIColors.surfaceElevated;
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
            return UIComp.GetFont();
        }

        private ColorBlock GetButtonColors(Color c)
        {
            return UIComp.GetButtonColors(c);
        }
    }
}
