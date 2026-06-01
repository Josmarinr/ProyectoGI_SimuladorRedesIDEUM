using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using SimRedes.Network;
using UIComp = SimRedes.UI.UIComponents;
using UIColors = SimRedes.UI.UIComponents.Colors;

namespace SimRedes.UI
{
    /// <summary>
    /// Fabrica de paneles de configuracion de red: IP, ARP, rutas, VLAN, ACL y NAT.
    /// Proporciona metodos estaticos para crear cada tipo de panel de configuracion.
    /// </summary>
    public static class ConfigPanelFactory
    {
        /// <summary>
        /// Crea el panel de configuracion IP para un nodo. Incluye campos de IP y mascara,
        /// teclado numerico virtual, validacion, y botones opcionales de ARP y tabla de rutas.
        /// </summary>
        /// <param name="canvas">Canvas raiz.</param>
        /// <param name="nodeName">Nombre del nodo a configurar.</param>
        /// <param name="nodeIP">IP actual del nodo.</param>
        /// <param name="nodeMask">Mascara de red actual.</param>
        /// <param name="onIPChanged">Callback cuando cambia la IP.</param>
        /// <param name="onMaskChanged">Callback cuando cambia la mascara.</param>
        /// <param name="onApply">Callback al presionar APLICAR.</param>
        /// <param name="onARP">Callback para abrir tabla ARP (puede ser null).</param>
        /// <param name="onRouting">Callback para abrir tabla de rutas (puede ser null).</param>
        /// <param name="onCancel">Callback al presionar CANCELAR.</param>
        /// <returns>Texto de estado de validacion.</returns>
        public static Text CreateIPConfigPanel(Transform canvas, string nodeName, string nodeIP, string nodeMask,
            Action<string> onIPChanged, Action<string> onMaskChanged,
            Action onApply, Action onARP, Action onRouting, Action onCancel)
        {
            Font font = UIPanelFactory.GetFont();
            Font bigFont = UIPanelFactory.GetFont(18);
            Font smallFont = UIPanelFactory.GetFont(12);

            bool showAdvanced = (onARP != null || onRouting != null);
            float panelHeight = showAdvanced ? 700f : 650f;

            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(480, panelHeight), 20,
                UIColors.surfacePanel, UIColors.borderAccent, "IPConfigPanel");

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(0f, 0f);
            panelRect.pivot = new Vector2(0f, 0f);
            panelRect.anchoredPosition = new Vector2(20, 20);

            UIComp.CreateMenuTitle(panelObj.transform, "Configurar " + nodeName, 22, new Vector2(0, panelHeight / 2 - 35), bigFont);
            Text validationText = null;

            float labelX = -110;
            float inputX = 70;
            float fieldY = panelHeight / 2 - 95;

            // ─── Etiqueta IP ───
            var ipLabelObj = new GameObject("IPLabel");
            ipLabelObj.transform.SetParent(panelObj.transform, false);
            var ipLabelRect = ipLabelObj.AddComponent<RectTransform>();
            ipLabelRect.anchorMin = new Vector2(0.5f, 0.5f);
            ipLabelRect.anchorMax = new Vector2(0.5f, 0.5f);
            ipLabelRect.anchoredPosition = new Vector2(labelX, fieldY);
            ipLabelRect.sizeDelta = new Vector2(80, 30);
            var ipLabelText = ipLabelObj.AddComponent<Text>();
            ipLabelText.text = "IP:";
            ipLabelText.color = UIColors.textSecondary;
            ipLabelText.fontSize = 18;
            ipLabelText.alignment = TextAnchor.MiddleRight;
            ipLabelText.font = bigFont;

            // ─── Campo IP (sin InputField - solo Text + Button de seleccion) ───
            string ipValue = nodeIP;
            bool editingIP = true; // IP seleccionado por defecto

            var ipFieldObj = new GameObject("IPField");
            ipFieldObj.transform.SetParent(panelObj.transform, false);
            var ipFieldRect = ipFieldObj.AddComponent<RectTransform>();
            ipFieldRect.anchorMin = new Vector2(0.5f, 0.5f);
            ipFieldRect.anchorMax = new Vector2(0.5f, 0.5f);
            ipFieldRect.anchoredPosition = new Vector2(inputX, fieldY);
            ipFieldRect.sizeDelta = new Vector2(260, 68);

            var ipFieldImg = ipFieldObj.AddComponent<Image>();
            Texture2D ipFieldTex = UIComp.CreateRoundedRectTexture(260, 68, 14, UIColors.surfaceElevated, UIColors.borderAccent, 2f);
            ipFieldImg.sprite = Sprite.Create(ipFieldTex, new Rect(0, 0, 260, 68), new Vector2(0.5f, 0.5f), 100);
            ipFieldImg.type = Image.Type.Sliced;

            var ipFieldTextObj = new GameObject("Text");
            ipFieldTextObj.transform.SetParent(ipFieldObj.transform, false);
            var ipFieldTextRect = ipFieldTextObj.AddComponent<RectTransform>();
            ipFieldTextRect.anchorMin = Vector2.zero;
            ipFieldTextRect.anchorMax = Vector2.one;
            ipFieldTextRect.offsetMin = new Vector2(15, 2);
            ipFieldTextRect.offsetMax = new Vector2(-15, -2);
            var ipDisplayText = ipFieldTextObj.AddComponent<Text>();
            ipDisplayText.text = ipValue;
            ipDisplayText.color = UIColors.textPrimary;
            ipDisplayText.fontSize = 20;
            ipDisplayText.alignment = TextAnchor.MiddleCenter;
            ipDisplayText.font = bigFont;
            ipDisplayText.raycastTarget = false;

            fieldY -= 65;

            // ─── Etiqueta Mascara ───
            var maskLabelObj = new GameObject("MaskLabel");
            maskLabelObj.transform.SetParent(panelObj.transform, false);
            var maskLabelRect = maskLabelObj.AddComponent<RectTransform>();
            maskLabelRect.anchorMin = new Vector2(0.5f, 0.5f);
            maskLabelRect.anchorMax = new Vector2(0.5f, 0.5f);
            maskLabelRect.anchoredPosition = new Vector2(labelX, fieldY);
            maskLabelRect.sizeDelta = new Vector2(80, 30);
            var maskLabelText = maskLabelObj.AddComponent<Text>();
            maskLabelText.text = "Mascara:";
            maskLabelText.color = UIColors.textSecondary;
            maskLabelText.fontSize = 18;
            maskLabelText.alignment = TextAnchor.MiddleRight;
            maskLabelText.font = bigFont;

            // ─── Campo Mascara (sin InputField) ───
            string maskValue = nodeMask;

            var maskFieldObj = new GameObject("MaskField");
            maskFieldObj.transform.SetParent(panelObj.transform, false);
            var maskFieldRect = maskFieldObj.AddComponent<RectTransform>();
            maskFieldRect.anchorMin = new Vector2(0.5f, 0.5f);
            maskFieldRect.anchorMax = new Vector2(0.5f, 0.5f);
            maskFieldRect.anchoredPosition = new Vector2(inputX, fieldY);
            maskFieldRect.sizeDelta = new Vector2(260, 68);

            var maskFieldImg = maskFieldObj.AddComponent<Image>();
            Texture2D maskFieldTex = UIComp.CreateRoundedRectTexture(260, 68, 14, UIColors.surfaceElevated, UIColors.borderAccent, 2f);
            maskFieldImg.sprite = Sprite.Create(maskFieldTex, new Rect(0, 0, 260, 68), new Vector2(0.5f, 0.5f), 100);
            maskFieldImg.type = Image.Type.Sliced;

            var maskFieldTextObj = new GameObject("Text");
            maskFieldTextObj.transform.SetParent(maskFieldObj.transform, false);
            var maskFieldTextRect = maskFieldTextObj.AddComponent<RectTransform>();
            maskFieldTextRect.anchorMin = Vector2.zero;
            maskFieldTextRect.anchorMax = Vector2.one;
            maskFieldTextRect.offsetMin = new Vector2(15, 2);
            maskFieldTextRect.offsetMax = new Vector2(-15, -2);
            var maskDisplayText = maskFieldTextObj.AddComponent<Text>();
            maskDisplayText.text = maskValue;
            maskDisplayText.color = UIColors.textPrimary;
            maskDisplayText.fontSize = 20;
            maskDisplayText.alignment = TextAnchor.MiddleCenter;
            maskDisplayText.font = bigFont;
            maskDisplayText.raycastTarget = false;

            // Botones de seleccion (ambos aqui para evitar CS0841: deben declararse las Image antes de usarlas en lambdas)
            CreateFieldSelectButton(ipFieldObj, () => {
                editingIP = true;
                ipFieldImg.color = UIColors.textAccent;
                maskFieldImg.color = UIColors.surfaceElevated;
            });
            CreateFieldSelectButton(maskFieldObj, () => {
                editingIP = false;
                maskFieldImg.color = UIColors.textAccent;
                ipFieldImg.color = UIColors.surfaceElevated;
            });

            // Highlight inicial: IP seleccionado
            ipFieldImg.color = UIColors.textAccent;
            maskFieldImg.color = UIColors.surfaceElevated;

            fieldY -= 65;

            var vsObj = new GameObject("ValidationStatus");
            vsObj.transform.SetParent(panelObj.transform, false);
            var vsRect = vsObj.AddComponent<RectTransform>();
            vsRect.anchorMin = new Vector2(0.5f, 0.5f);
            vsRect.anchorMax = new Vector2(0.5f, 0.5f);
            vsRect.anchoredPosition = new Vector2(0, fieldY + 20);
            vsRect.sizeDelta = new Vector2(400, 40);
            validationText = vsObj.AddComponent<Text>();
            validationText.text = "";
            validationText.color = UIColors.textSecondary;
            validationText.fontSize = 12;
            validationText.alignment = TextAnchor.MiddleCenter;
            validationText.font = smallFont;

            float keypadY = fieldY - 30;
            // Teclado numerico inline: usa editingIP, ipValue, maskValue directamente
            // (sin InputField - todo el estado se maneja con variables de闭包)
            string[] keys = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", ".", "DEL" };
            float kSize = 54, kSpacing = 62, kStartX = -(kSpacing * 1.5f);
            for (int ki = 0; ki < keys.Length; ki++)
            {
                int row = ki / 3, col = ki % 3;
                float kx = kStartX + (col * kSpacing), ky = keypadY - (row * kSpacing);
                var keyObj = new GameObject("Key_" + keys[ki]);
                keyObj.transform.SetParent(panelObj.transform, false);
                var kRect = keyObj.AddComponent<RectTransform>();
                kRect.anchorMin = new Vector2(0.5f, 0.5f);
                kRect.anchorMax = new Vector2(0.5f, 0.5f);
                kRect.anchoredPosition = new Vector2(kx, ky);
                kRect.sizeDelta = new Vector2(kSize, kSize);
                var kImg = keyObj.AddComponent<Image>();
                Texture2D kTex = UIComp.CreateRoundedRectTexture((int)kSize, (int)kSize, 10, UIColors.buttonNormal, UIColors.borderAccent, 1f);
                kImg.sprite = Sprite.Create(kTex, new Rect(0, 0, (int)kSize, (int)kSize), new Vector2(0.5f, 0.5f), 100);
                kImg.type = Image.Type.Sliced;
                var kTextObj = new GameObject("Text");
                kTextObj.transform.SetParent(keyObj.transform, false);
                var kTextRect = kTextObj.AddComponent<RectTransform>();
                kTextRect.anchorMin = Vector2.zero;
                kTextRect.anchorMax = Vector2.one;
                kTextRect.offsetMin = Vector2.zero;
                kTextRect.offsetMax = Vector2.zero;
                var kText = kTextObj.AddComponent<Text>();
                kText.text = keys[ki];
                kText.color = UIColors.textPrimary;
                kText.fontSize = keys[ki] == "DEL" ? 10 : 20;
                kText.alignment = TextAnchor.MiddleCenter;
                kText.font = keys[ki] == "DEL" ? font : bigFont;
                kText.raycastTarget = false;

                string captured = keys[ki];
                var kTrigger = keyObj.AddComponent<EventTrigger>();
                var kEntry = new EventTrigger.Entry();
                kEntry.eventID = EventTriggerType.PointerDown;
                kEntry.callback.AddListener((data) => {
                    string txt = editingIP ? ipValue : maskValue;
                    if (captured == "DEL") {
                        if (txt.Length > 0) txt = txt.Substring(0, txt.Length - 1);
                    } else if (captured == ".") {
                        if (txt.Length > 0 && txt[txt.Length-1] != '.') txt += ".";
                    } else {
                        if (txt.Length < 18) txt += captured;
                    }
                    if (editingIP) {
                        ipValue = txt; ipDisplayText.text = txt; onIPChanged?.Invoke(txt);
                    } else {
                        maskValue = txt; maskDisplayText.text = txt; onMaskChanged?.Invoke(txt);
                    }
                });
                kTrigger.triggers.Add(kEntry);
            }

            float btnY = keypadY - (4 * 62) - 20;

            if (showAdvanced)
            {
                Button arpBtn = UIComp.CreateMenuButton(panelObj.transform, "ARPBtn", "TABLA ARP", new Vector2(-90, btnY), new Vector2(160, 38), font, 14);
                arpBtn.onClick.AddListener(() => onARP?.Invoke());

                Button routingBtn = UIComp.CreateMenuButton(panelObj.transform, "RoutingBtn", "TABLA RUTAS", new Vector2(90, btnY), new Vector2(160, 38), font, 14);
                routingBtn.onClick.AddListener(() => onRouting?.Invoke());

                btnY -= 55;
            }

            Button applyBtn = UIComp.CreateMenuButton(panelObj.transform, "ApplyBtn", "APLICAR", new Vector2(-60, btnY), new Vector2(160, 40), font, 16);
            applyBtn.onClick.AddListener(() => onApply?.Invoke());

            btnY -= 50;

            Button cancelBtn = UIComp.CreateMenuButton(panelObj.transform, "CancelBtn", "CANCELAR", new Vector2(-60, btnY), new Vector2(160, 40), font, 16);
            cancelBtn.onClick.AddListener(() => onCancel?.Invoke());

            // Asegurar que el panel este al frente del canvas (sobre el background semi-transparente)
            panelObj.transform.SetAsLastSibling();

            return validationText;
        }

        /// <summary>
        /// Crea el panel de visualizacion de la tabla ARP de un nodo.
        /// Muestra direcciones IP, MAC e interfaz de cada entrada.
        /// Si la tabla esta vacia, agrega entradas de ejemplo.
        /// </summary>
        /// <param name="canvas">Canvas raiz.</param>
        /// <param name="node">Nodo cuya tabla ARP se mostrara.</param>
        /// <returns>Objeto del panel creado.</returns>
        public static GameObject CreateARPPanel(Transform canvas, NetworkNode node)
        {
            Font font = UIPanelFactory.GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(550, 500), 20,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "ARPPanel";

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;

            UIComp.CreateMenuTitle(panelObj.transform, $"Tabla ARP - {node.Name}", 22, new Vector2(0, 205), font);

            var entries = node.ArpTable.GetAllEntries();
            if (entries.Count == 0)
            {
                for (int i = 0; i < 3; i++)
                    node.ArpTable.AddEntry($"192.168.{i}.{(node.DiscId % 255) + 1}", $"00:{(node.DiscId % 16):X}:{(i * 17):X2}:AB:CD:{i:X2}", "G0/0");
                entries = node.ArpTable.GetAllEntries();
            }

            float startY = 155, rowHeight = 35;
            UIComp.CreateInfoText(panelObj.transform, "Direccion IP", new Vector2(-160, startY), font, 13, UIColors.textSecondary, true);
            UIComp.CreateInfoText(panelObj.transform, "MAC Address", new Vector2(60, startY), font, 13, UIColors.textSecondary, true);
            UIComp.CreateInfoText(panelObj.transform, "Interfaz", new Vector2(200, startY), font, 13, UIColors.textSecondary, true);

            for (int i = 0; i < entries.Count && i < 10; i++)
            {
                float y = startY - ((i + 1) * rowHeight);
                UIComp.CreateInfoText(panelObj.transform, entries[i].IPAddress, new Vector2(-160, y), font, 12, UIColors.textPrimary, false);
                UIComp.CreateInfoText(panelObj.transform, entries[i].MACAddress, new Vector2(60, y), font, 12, Color.cyan, false);
                UIComp.CreateInfoText(panelObj.transform, entries[i].Interface, new Vector2(200, y), font, 12, UIColors.textAccent, false);
            }

            Button closeBtn = UIComp.CreateMenuButton(panelObj.transform, "CloseBtn", "CERRAR", new Vector2(0, -195), new Vector2(120, 40), font, 14);
            closeBtn.onClick.AddListener(() => UnityEngine.Object.Destroy(panelObj));
            return panelObj;
        }

        /// <summary>
        /// Crea el panel de la tabla de enrutamiento de un nodo.
        /// Muestra las entradas con opciones para eliminar, anadir rutas y cerrar.
        /// </summary>
        /// <param name="canvas">Canvas raiz.</param>
        /// <param name="node">Nodo cuya tabla de enrutamiento se mostrara.</param>
        /// <param name="onDeleteRoute">Callback con el indice de la ruta a eliminar.</param>
        /// <param name="onAddRoute">Callback con el indice donde anadir ruta (-1 = nueva).</param>
        /// <param name="onClose">Callback al cerrar el panel.</param>
        /// <returns>Objeto del panel creado.</returns>
        public static GameObject CreateRoutingPanel(Transform canvas, NetworkNode node,
            Action<int> onDeleteRoute, Action<int> onAddRoute, Action onClose)
        {
            Font font = UIPanelFactory.GetFont();
            var entries = node.RoutingTable.GetAllEntries();
            int entryCount = entries.Count;
            int displayEntries = Mathf.Max(entryCount, 5);
            float panelHeight = 380 + (displayEntries * 35);

            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(700, panelHeight), 20,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "RoutingPanel";

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;

            UIComp.CreateMenuTitle(panelObj.transform, $"Tabla de Enrutamiento - {node.Name}", 20, new Vector2(0, panelHeight / 2 - 35), font);

            float startY = panelHeight / 2 - 75, rowHeight = 35;
            UIComp.CreateInfoText(panelObj.transform, "Red Destino", new Vector2(-170, startY), font, 12, UIColors.textSecondary, true);
            UIComp.CreateInfoText(panelObj.transform, "Mask", new Vector2(-10, startY), font, 12, UIColors.textSecondary, true);
            UIComp.CreateInfoText(panelObj.transform, "Next Hop", new Vector2(140, startY), font, 12, UIColors.textSecondary, true);
            UIComp.CreateInfoText(panelObj.transform, "Intf", new Vector2(240, startY), font, 12, UIColors.textSecondary, true);

            for (int i = 0; i < displayEntries; i++)
            {
                float y = startY - ((i + 1) * rowHeight);
                int capturedIdx = i;

                if (i < entryCount)
                {
                    var entry = entries[i];
                    Color protoColor = entry.Protocol == "Static" ? Color.green : (entry.Protocol == "RIP" ? Color.yellow : Color.cyan);

                    UIComp.CreateInfoText(panelObj.transform, entry.DestinationNetwork, new Vector2(-170, y), font, 11, UIColors.textPrimary, false);
                    UIComp.CreateInfoText(panelObj.transform, entry.SubnetMask, new Vector2(-10, y), font, 11, UIColors.textSecondary, false);
                    UIComp.CreateInfoText(panelObj.transform, entry.NextHop, new Vector2(140, y), font, 11, UIColors.textAccent, false);
                    UIComp.CreateInfoText(panelObj.transform, entry.OutInterface, new Vector2(240, y), font, 11, protoColor, false);

                    Button delBtn = UIComp.CreateSmallButton(panelObj.transform, $"DelRoute_{i}", "X", new Vector2(285, y), 24, 24, font);
                    delBtn.onClick.AddListener(() => onDeleteRoute?.Invoke(capturedIdx));
                }
                else
                {
                    Button addBtn = UIComp.CreateSmallButton(panelObj.transform, $"AddRoute_{i}", "+", new Vector2(0, y), 260, 30, font);
                    addBtn.onClick.AddListener(() => onAddRoute?.Invoke(capturedIdx));
                }
            }

            float btnY = -panelHeight / 2 + 55;
            Button addNewBtn = UIComp.CreateMenuButton(panelObj.transform, "AddNewRouteBtn", "NUEVA RUTA", new Vector2(-90, btnY), new Vector2(150, 40), font, 13);
            addNewBtn.onClick.AddListener(() => onAddRoute?.Invoke(-1));

            Button closeBtn = UIComp.CreateMenuButton(panelObj.transform, "CloseBtn", "CERRAR", new Vector2(90, btnY), new Vector2(120, 40), font, 14);
            closeBtn.onClick.AddListener(() => { onClose?.Invoke(); UnityEngine.Object.Destroy(panelObj); });
            return panelObj;
        }

        /// <summary>
        /// Crea el panel para agregar una nueva ruta estatica a un nodo.
        /// Incluye campos para red destino, mascara, next hop, y botones de interfaz.
        /// </summary>
        /// <param name="canvas">Canvas raiz.</param>
        /// <param name="node">Nodo al que se agregara la ruta.</param>
        /// <param name="onAddRoute">Callback con (destino, mascara, nextHop, interfaz).</param>
        /// <param name="onCancel">Callback al cancelar.</param>
        /// <returns>Objeto del panel creado.</returns>
        public static GameObject CreateAddRoutePanel(Transform canvas, NetworkNode node,
            Action<string, string, string, string> onAddRoute, Action onCancel)
        {
            Font font = UIPanelFactory.GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(420, 480), 20,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "AddRoutePanel";

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;

            UIComp.CreateMenuTitle(panelObj.transform, "Nueva Ruta Estatica", 20, new Vector2(0, 195), font);

            float fieldY = 155, labelX = -130, inputX = 60;
            UIComp.CreateInfoText(panelObj.transform, "Red Destino:", new Vector2(labelX, fieldY), font, 13, UIColors.textSecondary, false);
            var destInput = UIPanelFactory.CreateConfigField(panelObj.transform, "192.168.0.0", inputX - 30, inputX + 90, fieldY, font);

            fieldY -= 65;
            UIComp.CreateInfoText(panelObj.transform, "Mask:", new Vector2(labelX, fieldY), font, 13, UIColors.textSecondary, false);
            var maskInput = UIPanelFactory.CreateConfigField(panelObj.transform, "255.255.255.0", inputX - 30, inputX + 90, fieldY, font);

            fieldY -= 65;
            UIComp.CreateInfoText(panelObj.transform, "Next Hop:", new Vector2(labelX, fieldY), font, 13, UIColors.textSecondary, false);
            var nextHopInput = UIPanelFactory.CreateConfigField(panelObj.transform, "192.168.1.1", inputX - 30, inputX + 90, fieldY, font);

            fieldY -= 65;
            UIComp.CreateInfoText(panelObj.transform, "Interfaz:", new Vector2(labelX, fieldY), font, 13, UIColors.textSecondary, false);

            string[] interfaces = node.Interfaces.ToArray();
            float interfaceY = fieldY - 40;
            int maxInterfaces = Mathf.Min(interfaces.Length, 4);

            for (int i = 0; i < maxInterfaces; i++)
            {
                int ifaceIndex = i;
                Button ifaceBtn = UIComp.CreateMenuButton(panelObj.transform, $"Interface_{i}", interfaces[i], new Vector2(0, interfaceY - (i * 40)), new Vector2(200, 35), font, 13);
                ifaceBtn.onClick.AddListener(() => {
                    string dest = destInput.text;
                    string mask = maskInput.text;
                    string nextHop = nextHopInput.text;
                    string iface = interfaces[ifaceIndex];
                    onAddRoute?.Invoke(dest, mask, nextHop, iface);
                });
            }

            float btnY = -185;
            Button cancelBtn = UIComp.CreateMenuButton(panelObj.transform, "CancelAddRouteBtn", "CANCELAR", new Vector2(-80, btnY), new Vector2(130, 45), font, 14);
            cancelBtn.onClick.AddListener(() => { onCancel?.Invoke(); UnityEngine.Object.Destroy(panelObj); });

            Button addBtn = UIComp.CreateMenuButton(panelObj.transform, "ConfirmAddRouteBtn", "AGREGAR", new Vector2(80, btnY), new Vector2(130, 45), font, 14);
            addBtn.onClick.AddListener(() => {
                string dest = destInput.text;
                string mask = maskInput.text;
                string nextHop = nextHopInput.text;
                string iface = interfaces.Length > 0 ? interfaces[0] : "G0/0";
                onAddRoute?.Invoke(dest, mask, nextHop, iface);
            });
            return panelObj;
        }

        /// <summary>
        /// Crea el panel de configuracion VLAN. Permite crear VLANs, asignar nodos
        /// y visualizar la lista de VLANs configuradas.
        /// </summary>
        /// <param name="canvas">Canvas raiz.</param>
        /// <param name="topology">Topologia activa con el gestor VLAN.</param>
        public static void CreateVLANPanel(Transform canvas, TopologyManager topology)
        {
            var existingPanel = GameObject.Find("VLANPanel");
            if (existingPanel != null) UnityEngine.Object.Destroy(existingPanel);

            Font font = UIPanelFactory.GetFont();
            GameObject bgObj = UIComp.CreateClickOutsideToClose(canvas, "VLANPanel");
            if (bgObj == null) return;

            GameObject panelObj = UIComp.CreateRoundedPanel(bgObj.transform, new Vector2(600, 550), 20,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "VLANPanel";

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;

            UIComp.CreateMenuTitle(panelObj.transform, "Configuracion VLAN", 24, new Vector2(0, 220), font);
            UIComp.CreateInfoText(panelObj.transform, "CREAR NUEVA VLAN:", new Vector2(-180, 165), font, 16, UIColors.textAccent, false);

            var vlanInputObj = new GameObject("VLANInput");
            vlanInputObj.transform.SetParent(panelObj.transform, false);
            var vlanInputRect = vlanInputObj.AddComponent<RectTransform>();
            vlanInputRect.anchorMin = new Vector2(0.5f, 0.5f);
            vlanInputRect.anchorMax = new Vector2(0.5f, 0.5f);
            vlanInputRect.anchoredPosition = new Vector2(50, 135);
            vlanInputRect.sizeDelta = new Vector2(150, 30);
            var vlanInput = vlanInputObj.AddComponent<InputField>();
            vlanInput.text = "10";
            vlanInput.characterLimit = 4;
            vlanInput.contentType = InputField.ContentType.IntegerNumber;
            CreateInputFieldText(vlanInput, font, 16, UIColors.textPrimary);

            Button createVlanBtn = UIComp.CreateMenuButton(panelObj.transform, "CreateVlanBtn", "CREAR", new Vector2(160, 135), new Vector2(80, 35), font, 14);
            createVlanBtn.onClick.AddListener(() => {
                if (int.TryParse(vlanInput.text, out int vlanId) && vlanId > 1 && vlanId <= 4094)
                {
                    topology.VLAN.CreateVLAN(vlanId);
                    UpdateVLANListDisplay(panelObj.transform, topology, font);
                }
            });

            UIComp.CreateInfoText(panelObj.transform, "ASIGNAR NODO A VLAN:", new Vector2(-180, 85), font, 16, UIColors.textAccent, false);

            var nodeList = topology.GetAllNodes();
            var nodeNames = new List<string> { "Seleccionar..." };
            foreach (var node in nodeList)
                nodeNames.Add($"{node.Name} (VLAN {topology.VLAN.GetNodeVLAN(node)})");

            GameObject dropdownObj = UIPanelFactory.CreateDropdown(panelObj.transform, nodeNames, font, new Vector2(0, 55), new Vector2(250, 30));
            var dropdown = dropdownObj.GetComponent<UnityEngine.UI.Dropdown>();

            var vlanSelectObj = new GameObject("VLANSelect");
            vlanSelectObj.transform.SetParent(panelObj.transform, false);
            var vlanSelectRect = vlanSelectObj.AddComponent<RectTransform>();
            vlanSelectRect.anchorMin = new Vector2(0.5f, 0.5f);
            vlanSelectRect.anchorMax = new Vector2(0.5f, 0.5f);
            vlanSelectRect.anchoredPosition = new Vector2(80, 10);
            vlanSelectRect.sizeDelta = new Vector2(100, 30);
            var vlanSelect = vlanSelectObj.AddComponent<InputField>();
            vlanSelect.text = "10";
            vlanSelect.characterLimit = 4;
            vlanSelect.contentType = InputField.ContentType.IntegerNumber;
            CreateInputFieldText(vlanSelect, font, 16, UIColors.textPrimary);

            Button assignBtn = UIComp.CreateMenuButton(panelObj.transform, "AssignBtn", "ASIGNAR", new Vector2(150, 10), new Vector2(90, 35), font, 14);
            assignBtn.onClick.AddListener(() => {
                if (dropdown.value > 0 && dropdown.value <= nodeList.Count)
                {
                    var selectedNode = nodeList[dropdown.value - 1];
                    if (int.TryParse(vlanSelect.text, out int vlanId))
                    {
                        topology.VLAN.AssignToVLAN(selectedNode, vlanId);
                        UpdateVLANListDisplay(panelObj.transform, topology, font);
                    }
                }
            });

            GameObject listObj = new GameObject("VLANList");
            listObj.transform.SetParent(panelObj.transform, false);
            var listRect = listObj.AddComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0.5f, 0.5f);
            listRect.anchorMax = new Vector2(0.5f, 0.5f);
            listRect.anchoredPosition = new Vector2(0, -50);
            listRect.sizeDelta = new Vector2(500, 200);
            UpdateVLANListDisplay(listObj.transform, topology, font);

            Button closeBtn = UIComp.CreateMenuButton(panelObj.transform, "CloseVlanBtn", "CERRAR", new Vector2(0, -220), new Vector2(120, 45), font, 16);
            closeBtn.onClick.AddListener(() => {
                UnityEngine.Object.Destroy(panelObj);
                var bg = GameObject.Find("VLANPanelBG");
                if (bg != null) UnityEngine.Object.Destroy(bg);
            });
        }

        /// <summary>
        /// Crea el componente Text hijo de un InputField con la fuente, tamano y color especificados.
        /// </summary>
        /// <param name="field">InputField padre.</param>
        /// <param name="font">Fuente a utilizar.</param>
        /// <param name="fontSize">Tamano de la fuente.</param>
        /// <param name="color">Color del texto.</param>
        /// <returns>Componente Text creado.</returns>
        private static Text CreateInputFieldText(InputField field, Font font, int fontSize, Color color)
        {
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(field.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8, 3);
            textRect.offsetMax = new Vector2(-8, -3);
            var textComp = textObj.AddComponent<Text>();
            textComp.font = font;
            textComp.fontSize = fontSize;
            textComp.color = color;
            textComp.alignment = TextAnchor.MiddleLeft;
            textComp.raycastTarget = false;
            field.textComponent = textComp;
            return textComp;
        }

        /// <summary>
        /// Actualiza la visualizacion de la lista de VLANs configuradas en el panel.
        /// Destruye el contenido anterior y recrea el texto con las VLANs actuales.
        /// </summary>
        /// <param name="parent">Transform padre donde se muestra la lista.</param>
        /// <param name="topology">Topologia activa con el gestor VLAN.</param>
        /// <param name="font">Fuente a utilizar.</param>
        private static void UpdateVLANListDisplay(Transform parent, TopologyManager topology, Font font)
        {
            var existingList = parent.Find("VLANListContent");
            if (existingList != null) UnityEngine.Object.Destroy(existingList.gameObject);

            GameObject listContent = new GameObject("VLANListContent");
            listContent.transform.SetParent(parent, false);
            var listRect = listContent.AddComponent<RectTransform>();
            listRect.anchorMin = Vector2.zero;
            listRect.anchorMax = Vector2.one;
            listRect.offsetMin = new Vector2(10, 10);
            listRect.offsetMax = new Vector2(-10, -10);

            var vlans = topology.VLAN.GetAllVLANs();
            string content = "VLANs CONFIGURADAS:\n\n";
            foreach (int vlanId in vlans)
            {
                var nodes = topology.VLAN.GetNodesInVLAN(vlanId);
                content += $"VLAN {vlanId}: {nodes.Count} nodos\n";
                foreach (var node in nodes)
                    content += $"  - {node.Name}\n";
            }
            if (vlans.Count == 1)
                content += "(Solo VLAN 1 por defecto)";

            var textObj = new GameObject("ListText");
            textObj.transform.SetParent(listContent.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            var textComp = textObj.AddComponent<Text>();
            textComp.text = content;
            textComp.font = font;
            textComp.fontSize = 14;
            textComp.color = UIColors.textPrimary;
            textComp.alignment = TextAnchor.UpperLeft;
        }

        /// <summary>
        /// Crea el panel de configuracion ACL. Permite crear listas de control de acceso,
        /// agregar reglas PERMIT/DENY con IP origen y destino, y visualizar las reglas.
        /// </summary>
        /// <param name="canvas">Canvas raiz.</param>
        /// <param name="topology">Topologia activa con el gestor ACL.</param>
        public static void CreateACLPanel(Transform canvas, TopologyManager topology)
        {
            var existingPanel = GameObject.Find("ACLPanel");
            if (existingPanel != null) UnityEngine.Object.Destroy(existingPanel);

            Font font = UIPanelFactory.GetFont();
            GameObject bgObj = UIComp.CreateClickOutsideToClose(canvas, "ACLPanel");
            if (bgObj == null) return;

            GameObject panelObj = UIComp.CreateRoundedPanel(bgObj.transform, new Vector2(650, 580), 20,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "ACLPanel";

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;

            UIComp.CreateMenuTitle(panelObj.transform, "Configuracion ACL", 24, new Vector2(0, 240), font);
            UIComp.CreateInfoText(panelObj.transform, "NOMBRE ACL:", new Vector2(-200, 190), font, 14, UIColors.textSecondary, false);

            var nameObj = new GameObject("ACLNameInput");
            nameObj.transform.SetParent(panelObj.transform, false);
            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.5f, 0.5f);
            nameRect.anchorMax = new Vector2(0.5f, 0.5f);
            nameRect.anchoredPosition = new Vector2(50, 190);
            nameRect.sizeDelta = new Vector2(180, 30);
            var nameInput = nameObj.AddComponent<InputField>();
            nameInput.text = "MI_ACL";
            CreateInputFieldText(nameInput, font, 16, UIColors.textPrimary);

            UIComp.CreateInfoText(panelObj.transform, "ACCION:", new Vector2(-200, 150), font, 14, UIColors.textSecondary, false);

            GameObject actionDropdown = UIPanelFactory.CreateDropdown(panelObj.transform,
                new List<string> { "PERMIT", "DENY" }, font,
                new Vector2(80, 150), new Vector2(120, 30));
            var actionDD = actionDropdown.GetComponent<UnityEngine.UI.Dropdown>();

            UIComp.CreateInfoText(panelObj.transform, "IP ORIGEN:", new Vector2(-200, 110), font, 14, UIColors.textSecondary, false);

            var srcObj = new GameObject("SRCInput");
            srcObj.transform.SetParent(panelObj.transform, false);
            var srcRect = srcObj.AddComponent<RectTransform>();
            srcRect.anchorMin = new Vector2(0.5f, 0.5f);
            srcRect.anchorMax = new Vector2(0.5f, 0.5f);
            srcRect.anchoredPosition = new Vector2(50, 110);
            srcRect.sizeDelta = new Vector2(150, 30);
            var srcInput = srcObj.AddComponent<InputField>();
            srcInput.text = "any";
            CreateInputFieldText(srcInput, font, 16, UIColors.textPrimary);

            UIComp.CreateInfoText(panelObj.transform, "IP DESTINO:", new Vector2(-200, 70), font, 14, UIColors.textSecondary, false);

            var dstObj = new GameObject("DSTInput");
            dstObj.transform.SetParent(panelObj.transform, false);
            var dstRect = dstObj.AddComponent<RectTransform>();
            dstRect.anchorMin = new Vector2(0.5f, 0.5f);
            dstRect.anchorMax = new Vector2(0.5f, 0.5f);
            dstRect.anchoredPosition = new Vector2(50, 70);
            dstRect.sizeDelta = new Vector2(150, 30);
            var dstInput = dstObj.AddComponent<InputField>();
            dstInput.text = "any";
            CreateInputFieldText(dstInput, font, 16, UIColors.textPrimary);

            Button addRuleBtn = UIComp.CreateMenuButton(panelObj.transform, "AddRuleBtn", "AGREGAR REGLA", new Vector2(150, 30), new Vector2(130, 38), font, 14);
            addRuleBtn.onClick.AddListener(() => {
                string aclName = string.IsNullOrEmpty(nameInput.text) ? "MI_ACL" : nameInput.text;
                topology.CreateACL(aclName);
                var rule = new ACLRule
                {
                    Action = actionDD.value == 0 ? ACLAction.Permit : ACLAction.Deny,
                    SourceIP = srcInput.text,
                    DestIP = dstInput.text,
                    Description = $"Rule {topology.ACL.GetRules(aclName).Count + 1}"
                };
                topology.AddACLRule(aclName, rule);
                UpdateACLListDisplay(panelObj.transform, topology, font);
            });

            var aclListObj = new GameObject("ACLList");
            aclListObj.transform.SetParent(panelObj.transform, false);
            var aclListRect = aclListObj.AddComponent<RectTransform>();
            aclListRect.anchorMin = new Vector2(0.5f, 0.5f);
            aclListRect.anchorMax = new Vector2(0.5f, 0.5f);
            aclListRect.anchoredPosition = new Vector2(0, -80);
            aclListRect.sizeDelta = new Vector2(580, 220);
            UpdateACLListDisplay(aclListObj.transform, topology, font);

            Button closeBtn = UIComp.CreateMenuButton(panelObj.transform, "CloseACLBtn", "CERRAR", new Vector2(0, -235), new Vector2(120, 45), font, 16);
            closeBtn.onClick.AddListener(() => {
                UnityEngine.Object.Destroy(panelObj);
                var bg = GameObject.Find("ACLPanelBG");
                if (bg != null) UnityEngine.Object.Destroy(bg);
            });
        }

        /// <summary>
        /// Actualiza la visualizacion de la lista de ACLs y sus reglas en el panel.
        /// Destruye el contenido anterior y recrea el texto con las ACLs actuales.
        /// </summary>
        /// <param name="parent">Transform padre donde se muestra la lista.</param>
        /// <param name="topology">Topologia activa con el gestor ACL.</param>
        /// <param name="font">Fuente a utilizar.</param>
        private static void UpdateACLListDisplay(Transform parent, TopologyManager topology, Font font)
        {
            var existing = parent.Find("ACLListContent");
            if (existing != null) UnityEngine.Object.Destroy(existing.gameObject);

            GameObject content = new GameObject("ACLListContent");
            content.transform.SetParent(parent, false);
            var rect = content.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(10, 10);
            rect.offsetMax = new Vector2(-10, -10);

            string text = "ACLs CONFIGURADAS:\n\n";
            text += topology.ACL.GetACLSummary();
            text += "\n\nREGLAS:\n";
            var rules = topology.ACL.GetRules("MI_ACL");
            foreach (var rule in rules)
                text += $"  Seq {rule.Sequence}: {rule.Action} {rule.SourceIP} \u2192 {rule.DestIP}\n";

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(content.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            var comp = textObj.AddComponent<Text>();
            comp.text = text;
            comp.font = font;
            comp.fontSize = 13;
            comp.color = UIColors.textPrimary;
            comp.alignment = TextAnchor.UpperLeft;
        }

        /// <summary>
        /// Crea el panel de configuracion NAT. Permite configurar NAT estatica, dinamica
        /// y PAT, con campos para IP publica, IP interna, IP externa y puerto.
        /// </summary>
        /// <param name="canvas">Canvas raiz.</param>
        /// <param name="topology">Topologia activa con el gestor NAT.</param>
        public static void CreateNATPanel(Transform canvas, TopologyManager topology)
        {
            var existingPanel = GameObject.Find("NATPanel");
            if (existingPanel != null) UnityEngine.Object.Destroy(existingPanel);

            Font font = UIPanelFactory.GetFont();
            GameObject bgObj = UIComp.CreateClickOutsideToClose(canvas, "NATPanel");
            if (bgObj == null) return;

            GameObject panelObj = UIComp.CreateRoundedPanel(bgObj.transform, new Vector2(600, 580), 20,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "NATPanel";

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;

            UIComp.CreateMenuTitle(panelObj.transform, "Configuracion NAT", 24, new Vector2(0, 240), font);
            UIComp.CreateInfoText(panelObj.transform, "IP PUBLICA (Router):", new Vector2(-180, 190), font, 14, UIColors.textSecondary, false);

            var pubIpObj = new GameObject("PublicIPInput");
            pubIpObj.transform.SetParent(panelObj.transform, false);
            var pubIpRect = pubIpObj.AddComponent<RectTransform>();
            pubIpRect.anchorMin = new Vector2(0.5f, 0.5f);
            pubIpRect.anchorMax = new Vector2(0.5f, 0.5f);
            pubIpRect.anchoredPosition = new Vector2(50, 190);
            pubIpRect.sizeDelta = new Vector2(160, 30);
            var pubIpInput = pubIpObj.AddComponent<InputField>();
            pubIpInput.text = topology.NAT.GetRouterIP();
            CreateInputFieldText(pubIpInput, font, 16, UIColors.textPrimary);

            Button setPubBtn = UIComp.CreateMenuButton(panelObj.transform, "SetPubBtn", "SET", new Vector2(180, 190), new Vector2(60, 30), font, 12);
            setPubBtn.onClick.AddListener(() => { topology.NAT.SetPublicIP(pubIpInput.text); });

            UIComp.CreateInfoText(panelObj.transform, "TIPO NAT:", new Vector2(-180, 145), font, 14, UIColors.textSecondary, false);

            GameObject natTypeDD = UIPanelFactory.CreateDropdown(panelObj.transform,
                new List<string> { "ESTATICA", "DINAMICA", "PAT" }, font,
                new Vector2(30, 145), new Vector2(120, 30));
            var natType = natTypeDD.GetComponent<UnityEngine.UI.Dropdown>();

            UIComp.CreateInfoText(panelObj.transform, "IP INTERNA:", new Vector2(-180, 105), font, 14, UIColors.textSecondary, false);

            var intIpObj = new GameObject("IntIPInput");
            intIpObj.transform.SetParent(panelObj.transform, false);
            var intIpRect = intIpObj.AddComponent<RectTransform>();
            intIpRect.anchorMin = new Vector2(0.5f, 0.5f);
            intIpRect.anchorMax = new Vector2(0.5f, 0.5f);
            intIpRect.anchoredPosition = new Vector2(50, 105);
            intIpRect.sizeDelta = new Vector2(160, 30);
            var intIpInput = intIpObj.AddComponent<InputField>();
            intIpInput.text = "192.168.1.10";
            CreateInputFieldText(intIpInput, font, 16, UIColors.textPrimary);

            var extIpLabelObj = new GameObject("ExtIPLabel");
            extIpLabelObj.transform.SetParent(panelObj.transform, false);
            var extIpLabelRect = extIpLabelObj.AddComponent<RectTransform>();
            extIpLabelRect.anchorMin = new Vector2(0.5f, 0.5f);
            extIpLabelRect.anchorMax = new Vector2(0.5f, 0.5f);
            extIpLabelRect.anchoredPosition = new Vector2(-80, 60);
            extIpLabelRect.sizeDelta = new Vector2(100, 20);
            var extIpLabel = extIpLabelObj.AddComponent<Text>();
            extIpLabel.text = "IP EXTERNA:";
            extIpLabel.font = font;
            extIpLabel.fontSize = 14;
            extIpLabel.color = UIColors.textSecondary;

            var extIpObj = new GameObject("ExtIPInput");
            extIpObj.transform.SetParent(panelObj.transform, false);
            var extIpRect = extIpObj.AddComponent<RectTransform>();
            extIpRect.anchorMin = new Vector2(0.5f, 0.5f);
            extIpRect.anchorMax = new Vector2(0.5f, 0.5f);
            extIpRect.anchoredPosition = new Vector2(50, 60);
            extIpRect.sizeDelta = new Vector2(160, 30);
            var extIpInput = extIpObj.AddComponent<InputField>();
            extIpInput.text = "200.100.50.10";
            CreateInputFieldText(extIpInput, font, 16, UIColors.textPrimary);

            var portLabelObj = new GameObject("PortLabel");
            portLabelObj.transform.SetParent(panelObj.transform, false);
            var portLabelRect = portLabelObj.AddComponent<RectTransform>();
            portLabelRect.anchorMin = new Vector2(0.5f, 0.5f);
            portLabelRect.anchorMax = new Vector2(0.5f, 0.5f);
            portLabelRect.anchoredPosition = new Vector2(-80, 20);
            portLabelRect.sizeDelta = new Vector2(100, 20);
            var portLabel = portLabelObj.AddComponent<Text>();
            portLabel.text = "PUERTO:";
            portLabel.font = font;
            portLabel.fontSize = 14;
            portLabel.color = UIColors.textSecondary;

            var portObj = new GameObject("PortInput");
            portObj.transform.SetParent(panelObj.transform, false);
            var portRect = portObj.AddComponent<RectTransform>();
            portRect.anchorMin = new Vector2(0.5f, 0.5f);
            portRect.anchorMax = new Vector2(0.5f, 0.5f);
            portRect.anchoredPosition = new Vector2(50, 20);
            portRect.sizeDelta = new Vector2(100, 30);
            var portInput = portObj.AddComponent<InputField>();
            portInput.text = "80";
            portInput.characterLimit = 5;
            portInput.contentType = InputField.ContentType.IntegerNumber;
            CreateInputFieldText(portInput, font, 16, UIColors.textPrimary);

            Button addNatBtn = UIComp.CreateMenuButton(panelObj.transform, "AddNatBtn", "AGREGAR NAT", new Vector2(150, -10), new Vector2(120, 38), font, 14);
            addNatBtn.onClick.AddListener(() => {
                string intIP = intIpInput.text;
                string extIP = extIpInput.text;
                if (natType.value == 0)
                    topology.NAT.AddStaticNAT(intIP, extIP);
                else if (natType.value == 1)
                    topology.NAT.AddDynamicNAT(intIP);
                else if (natType.value == 2 && int.TryParse(portInput.text, out int port))
                    topology.NAT.AddPAT(intIP, port, "TCP");
                UpdateNATListDisplay(panelObj.transform, topology, font);
            });

            var natListObj = new GameObject("NATList");
            natListObj.transform.SetParent(panelObj.transform, false);
            var natListRect = natListObj.AddComponent<RectTransform>();
            natListRect.anchorMin = new Vector2(0.5f, 0.5f);
            natListRect.anchorMax = new Vector2(0.5f, 0.5f);
            natListRect.anchoredPosition = new Vector2(0, -100);
            natListRect.sizeDelta = new Vector2(520, 220);
            UpdateNATListDisplay(natListObj.transform, topology, font);

            Button closeBtn = UIComp.CreateMenuButton(panelObj.transform, "CloseNATBtn", "CERRAR", new Vector2(0, -245), new Vector2(120, 45), font, 16);
            closeBtn.onClick.AddListener(() => {
                UnityEngine.Object.Destroy(panelObj);
                var bg = GameObject.Find("NATPanelBG");
                if (bg != null) UnityEngine.Object.Destroy(bg);
            });
        }

        /// <summary>
        /// Actualiza la visualizacion de la tabla NAT en el panel.
        /// Destruye el contenido anterior y recrea el texto con las entradas NAT actuales.
        /// </summary>
        /// <param name="parent">Transform padre donde se muestra la tabla.</param>
        /// <param name="topology">Topologia activa con el gestor NAT.</param>
        /// <param name="font">Fuente a utilizar.</param>
        private static void UpdateNATListDisplay(Transform parent, TopologyManager topology, Font font)
        {
            var existing = parent.Find("NATListContent");
            if (existing != null) UnityEngine.Object.Destroy(existing.gameObject);

            GameObject content = new GameObject("NATListContent");
            content.transform.SetParent(parent, false);
            var rect = content.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(10, 10);
            rect.offsetMax = new Vector2(-10, -10);

            string text = "TABLA NAT:\n\n";
            text += topology.NAT.GetNATSummary();

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(content.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            var comp = textObj.AddComponent<Text>();
            comp.text = text;
            comp.font = font;
            comp.fontSize = 13;
            comp.color = UIColors.textPrimary;
            comp.alignment = TextAnchor.UpperLeft;
        }

        /// <summary>
        /// Crea un boton invisible sobre un campo de texto (IP o Mascara) para capturar clicks.
        /// Al hacer click, invoca el callback onSelect para cambiar la seleccion al campo clickeado.
        /// NO usa InputField - solo Image + Button para deteccion de clicks.
        /// </summary>
        /// <param name="fieldObj">GameObject del campo visual (tiene RectTransform + Image).</param>
        /// <param name="onSelect">Callback cuando el usuario hace click en este campo.</param>
        private static void CreateFieldSelectButton(GameObject fieldObj, System.Action onSelect)
        {
            GameObject overlay = new GameObject("FieldSelectBtn");
            overlay.transform.SetParent(fieldObj.transform, false);
            var overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            var overlayImg = overlay.AddComponent<Image>();
            overlayImg.color = Color.clear;
            overlayImg.raycastTarget = true;
            var overlayBtn = overlay.AddComponent<Button>();
            overlayBtn.targetGraphic = overlayImg;
            overlayBtn.transition = Selectable.Transition.None;
            overlayBtn.navigation = new Navigation { mode = Navigation.Mode.None };
            overlayBtn.onClick.AddListener(() => onSelect?.Invoke());
        }
    }
}
