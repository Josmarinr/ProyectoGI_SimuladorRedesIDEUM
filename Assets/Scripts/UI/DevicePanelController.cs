using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
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
        private NodeVisualizer visualizer;
        private Canvas canvas;

        private float scoreUpdateTimer = 0f;
        private float panelSyncTimer = 0f;

        // ---- Refresco coalescido del panel (evita rebuild por cada evento de topologia) ----
        // Los eventos de alta frecuencia (mover discos) solo marcan refreshDirty;
        // el consumo ocurre a razon limitada una vez por REFRESH_INTERVAL.
        private bool refreshDirty = false;
        private float refreshTimer = 0f;
        private const float REFRESH_INTERVAL = 0.35f;
        // Firma de la composicion de nodos con la que se construyo el panel actual.
        private string lastBuiltSignature = "";

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

            // Consumo acotado de refrescos pendientes: aunque lleguen decenas de
            // eventos de topologia por segundo, como mucho se evalua una vez
            // por REFRESH_INTERVAL y solo se reconstruye si cambio la composicion.
            if (refreshDirty)
            {
                refreshTimer += Time.deltaTime;
                if (refreshTimer >= REFRESH_INTERVAL)
                {
                    refreshTimer = 0f;
                    refreshDirty = false;
                    ConsumeRefreshRequest();
                }
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
                            // NOTA: No limpiar el primer extremo del modo CONEXION al hacer clic fuera del panel.
                            // Si lo hicieramos, el usuario nunca podria conectar dos dispositivos porque cada
                            // clic en un nodo visual (que esta fuera del DevicesPanel) resetea el estado.
                            // El modo CONEXION se cancela presionando CONECTAR/DESCONECTAR nuevamente.
                        }
                    }
                }
            }
        }

        private float lastDeviceClickTime = 0f;
        private int lastDeviceClickIndex = -1;

        public void OnDeviceItemClicked(int index)
        {
            // Ignorar eventos duplicados dentro de 200ms (TouchScript + InputSystem = doble clic)
            float now = Time.unscaledTime;
            if (now - lastDeviceClickTime < 0.2f && lastDeviceClickIndex == index)
                return;
            lastDeviceClickTime = now;
            lastDeviceClickIndex = index;

            if (TopologyManager.Instance == null) return;
            var nodes = TopologyManager.Instance.GetAllNodes();
            if (index >= nodes.Count) return;

            var node = nodes[index];

            // Si el modo CONEXION/DESCONEXION esta activo, priorizar enlace
            var linkCtrl = LinkModeController.Instance;
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
                ipConfigCtrl.CloseIPConfigPanel();
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

            if (TopologyManager.Instance != null)
            {
                TopologyManager.Instance.RemoveNode(selectedNodeForRemoval);
                Debug.Log($"[DevicePanelController] Nodo eliminado: {selectedNodeForRemoval}");
            }

            var ipConfigCtrl = Object.FindAnyObjectByType<IPConfigController>();
            if (ipConfigCtrl != null && ipConfigCtrl.IsIPConfigPanelOpen())
            {
                ipConfigCtrl.CloseIPConfigPanel();
            }

            selectedNodeForRemoval = -1;
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

        /// <summary>
        /// Marca el panel para un refresco diferido. Es el reemplazo barato del
        /// rebuild sincrono por evento: los eventos de topologia de alta frecuencia
        /// (mover discos) solo llaman a este metodo y el consumo real ocurre en
        /// Update() a razon limitada (ver REFRESH_INTERVAL).
        /// </summary>
        public void RequestRefresh()
        {
            refreshDirty = true;
        }

        /// <summary>
        /// Consume un refresco pendiente: reconstruye el panel solo si cambio la
        /// composicion de nodos (cantidad/ids/nombres) o si el panel no existe.
        /// Mover discos no altera el contenido del panel, asi que esos eventos
        /// terminan sin hacer nada.
        /// </summary>
        private void ConsumeRefreshRequest()
        {
            var tm = TopologyManager.Instance;
            if (tm == null)
            {
                // Topologia aun no disponible: re-marcar la solicitud, porque
                // Update() limpio refreshDirty antes de llamar aqui y sin este
                // reintento el refresco se perdia (L2).
                refreshDirty = true;
                return;
            }

            var nodes = tm.GetAllNodes();
            string signature = BuildNodesSignature(nodes);
            bool panelMissing = GameObject.Find("DevicesPanel") == null;
            if (!panelMissing && signature == lastBuiltSignature)
            {
                // Sin cambios de composicion: no reconstruir (eso crearia sprites).
                // Solo refrescar los contadores del HUD, que son texto y baratos.
                TopologyInfoPanelSync();
                return;
            }

            RefreshDevicesPanel();
        }

        /// <summary>
        /// Construye una firma de la composicion actual de nodos (cantidad + ids + nombres)
        /// para detectar altas/bajas de dispositivos sin reconstruir el panel.
        /// </summary>
        /// <param name="nodes">Lista actual de nodos de la topologia.</param>
        /// <returns>Firma compacta comparable por igualdad de string.</returns>
        private static string BuildNodesSignature(List<NetworkNode> nodes)
        {
            var sb = new System.Text.StringBuilder(nodes.Count * 16);
            sb.Append(nodes.Count);
            for (int i = 0; i < nodes.Count; i++)
                sb.Append(';').Append(nodes[i].DiscId).Append(':').Append(nodes[i].Name);
            return sb.ToString();
        }

        public void RefreshDevicesPanel()
        {
            // Siempre destruir el panel existente para recrearlo con items frescos
            // Usar DestroyImmediate para evitar que GameObject.Find encuentre el panel viejo
            var existingPanel = GameObject.Find("DevicesPanel");
            if (existingPanel != null)
            {
                // Liberar los Sprite del panel viejo (las texturas quedan en cache compartida)
                UIComp.SafeDestroyPanelSprites(existingPanel);
                DestroyImmediate(existingPanel);
            }

            var foundCanvas = Object.FindAnyObjectByType<Canvas>();
            if (foundCanvas == null)
            {
                Debug.LogWarning("[DevicePanelController] No se encontro Canvas para crear DevicesPanel");
                return;
            }

            Font arialFont = UIComp.GetFont();
            var tm = TopologyManager.Instance;
            if (tm == null) return;
            
            var allNodes = tm.GetAllNodes();
            int nodeCount = allNodes.Count;

            // Registrar con que composicion se construyo el panel y consumir el dirty
            lastBuiltSignature = BuildNodesSignature(allNodes);
            refreshDirty = false;

            GameObject panelObj = UIPanelFactory.CreateDevicesPanel(foundCanvas.transform,
                nodeCount, OnDeviceItemClicked);

            if (nodeCount > 0)
            {
                float startY = 220f;
                for (int i = 0; i < nodeCount; i++)
                {
                    int index = i;
                    float yPos = startY - (i * 65);
                    CreateDeviceItem(panelObj.transform, index, yPos, arialFont);
                }
            }

            UpdateDevicesList();
            TopologyInfoPanelSync();
        }

        /// <summary>
        /// Sincroniza los contadores del panel TopologyInfoPanel con el estado actual de la topologia.
        /// </summary>
        private void TopologyInfoPanelSync()
        {
            var tm = TopologyManager.Instance;
            if (tm == null) return;
            var nodes = tm.GetAllNodes();
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
            if (linkText != null) linkText.text = $"Enlaces: {tm.GetAllLinks().Count}";
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
            itemRect.sizeDelta = new Vector2(350, 65);

            var bgImg = itemObj.AddComponent<Image>();
            Texture2D bgTex = UIComp.CreateRoundedRectTexture(350, 65, 10, UIColors.surfaceElevated, UIColors.borderAccent, 1f);
            bgImg.sprite = Sprite.Create(bgTex, new Rect(0, 0, 350, 65), new Vector2(0.5f, 0.5f), 100);
            bgImg.type = Image.Type.Sliced;

            var btn = itemObj.AddComponent<Button>();
            btn.colors = UIComp.GetButtonColors(UIColors.surfaceElevated);

            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(itemObj.transform, false);
            var iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(35, 0);
            iconRect.sizeDelta = new Vector2(36, 36);

            var iconImg = iconObj.AddComponent<Image>();
            iconImg.color = Color.gray;

            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(itemObj.transform, false);
            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0.5f);
            nameRect.anchorMax = new Vector2(0f, 0.5f);
            nameRect.pivot = new Vector2(0f, 0.5f);
            nameRect.anchoredPosition = new Vector2(70, 0);
            nameRect.sizeDelta = new Vector2(220, 32);

            var nameText = nameObj.AddComponent<Text>();
            nameText.text = "";
            nameText.color = UIColors.textPrimary;
            nameText.fontSize = 20;
            nameText.alignment = TextAnchor.MiddleLeft;
            nameText.font = font;

            int capturedIndex = index;
            btn.onClick.AddListener(() => OnDeviceItemClicked(capturedIndex));
        }

        public void UpdateDevicesList()
        {
            var devicesPanel = GameObject.Find("DevicesPanel")?.transform;
            if (devicesPanel == null) return;

            var tm = TopologyManager.Instance;
            if (tm == null) return;

            var nodes = tm.GetAllNodes();
            int maxItems = nodes.Count;

            for (int i = 0; i < maxItems; i++)
            {
                var itemObj = devicesPanel.Find($"DeviceItem_{i}");
                if (itemObj == null) continue;

                var iconObj = itemObj.Find("Icon");
                var nameObj = itemObj.Find("Name");

                var node = nodes[i];

                // Determinar si este nodo es el primero seleccionado en modo CONEXION/DESCONEXION
                var linkCtrl = LinkModeController.Instance;
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
                    // B3: el icono viene de circleSpriteCache, que es la duena del sprite
                    // y su textura. Antes se hacia Destroy(img.sprite) cada 2 s, lo que
                    // invalidaba la cache y obligaba a recrear Texture2D en un bucle
                    // infinito (aun en idle). Ahora solo se reasigna si cambia.
                    img.color = UIColors.GetColorForDeviceType(node.Type);
                    Sprite circleIcon = CreateCircleIcon(24, UIColors.GetColorForDeviceType(node.Type));
                    if (img.sprite != circleIcon) img.sprite = circleIcon;
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

            var tm = TopologyManager.Instance;
            if (tm == null) return;

            var nodes = tm.GetAllNodes();

            for (int i = 0; i < nodes.Count; i++)
            {
                var itemObj = devicesPanel.Find($"DeviceItem_{i}");
                if (itemObj == null) continue;

                var node = nodes[i];
                bool isSelected = (selectedNodeForRemoval == node.DiscId);

                var linkCtrl = LinkModeController.Instance;
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

        // Cache de sprites circulares para device panel: key = "size_colorR_colorG_colorB"
        private static Dictionary<string, Sprite> circleSpriteCache = new Dictionary<string, Sprite>();

        private Sprite CreateCircleIcon(int size, Color color)
        {
            string cacheKey = $"{size}_{color.r:F4}_{color.g:F4}_{color.b:F4}";
            if (circleSpriteCache.TryGetValue(cacheKey, out var cached) && cached != null)
                return cached;

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
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            circleSpriteCache[cacheKey] = sprite;
            // La cache es duena del sprite y su textura: protegerlo del teardown de paneles
            UIComp.RegisterOwnedCachedSprite(sprite);
            return sprite;
        }

        /// <summary>
        /// Limpia el cache de sprites al destruirse para evitar texturas colgadas.
        /// </summary>
        private void OnDestroy()
        {
            foreach (var kvp in circleSpriteCache)
            {
                if (kvp.Value != null)
                {
                    UIComp.UnregisterOwnedCachedSprite(kvp.Value);
                    if (kvp.Value.texture != null)
                        Destroy(kvp.Value.texture);
                    Destroy(kvp.Value);
                }
            }
            circleSpriteCache.Clear();
        }
    }
}
