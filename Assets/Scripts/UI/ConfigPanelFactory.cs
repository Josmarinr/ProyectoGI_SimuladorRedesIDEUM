using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using SimRedes.Network;
using UIComp = SimRedes.UI.UIComponents;
using UIColors = SimRedes.UI.UIComponents.Colors;

namespace SimRedes.UI
{
    public static class ConfigPanelFactory
    {
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
            string networkInfo = "";
            Text validationText = null;

            float labelX = -110;
            float inputX = 70;
            float fieldY = panelHeight / 2 - 95;

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

            var ipField = ipFieldObj.AddComponent<InputField>();
            var ipFieldTextObj = new GameObject("Text");
            ipFieldTextObj.transform.SetParent(ipFieldObj.transform, false);
            var ipFieldTextRect = ipFieldTextObj.AddComponent<RectTransform>();
            ipFieldTextRect.anchorMin = Vector2.zero;
            ipFieldTextRect.anchorMax = Vector2.one;
            ipFieldTextRect.offsetMin = new Vector2(15, 2);
            ipFieldTextRect.offsetMax = new Vector2(-15, -2);
            var ipFieldText = ipFieldTextObj.AddComponent<Text>();
            ipFieldText.text = nodeIP;
            ipFieldText.color = UIColors.textPrimary;
            ipFieldText.fontSize = 20;
            ipFieldText.alignment = TextAnchor.MiddleCenter;
            ipFieldText.font = bigFont;
            ipField.textComponent = ipFieldText;
            ipField.text = nodeIP;
            UIComp.SetupNumericInput(ipField, (v) => { onIPChanged?.Invoke(v); });

            fieldY -= 65;

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

            var maskField = maskFieldObj.AddComponent<InputField>();
            var maskFieldTextObj = new GameObject("Text");
            maskFieldTextObj.transform.SetParent(maskFieldObj.transform, false);
            var maskFieldTextRect = maskFieldTextObj.AddComponent<RectTransform>();
            maskFieldTextRect.anchorMin = Vector2.zero;
            maskFieldTextRect.anchorMax = Vector2.one;
            maskFieldTextRect.offsetMin = new Vector2(15, 2);
            maskFieldTextRect.offsetMax = new Vector2(-15, -2);
            var maskFieldText = maskFieldTextObj.AddComponent<Text>();
            maskFieldText.text = nodeMask;
            maskFieldText.color = UIColors.textPrimary;
            maskFieldText.fontSize = 20;
            maskFieldText.alignment = TextAnchor.MiddleCenter;
            maskFieldText.font = bigFont;
            maskField.textComponent = maskFieldText;
            maskField.text = nodeMask;
            UIComp.SetupNumericInput(maskField, (v) => { onMaskChanged?.Invoke(v); });

            fieldY -= 65;

            var vsObj = new GameObject("ValidationStatus");
            vsObj.transform.SetParent(panelObj.transform, false);
            var vsRect = vsObj.AddComponent<RectTransform>();
            vsRect.anchorMin = new Vector2(0.5f, 0.5f);
            vsRect.anchorMax = new Vector2(0.5f, 0.5f);
            vsRect.anchoredPosition = new Vector2(0, fieldY + 20);
            vsRect.sizeDelta = new Vector2(400, 40);
            validationText = vsObj.AddComponent<Text>();
            validationText.text = networkInfo;
            validationText.color = UIColors.textSecondary;
            validationText.fontSize = 12;
            validationText.alignment = TextAnchor.MiddleCenter;
            validationText.font = smallFont;

            float keypadY = fieldY - 30;
            UIPanelFactory.CreateNumericKeypad(panelObj.transform, keypadY, font, bigFont, ipField, maskField);

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

            return validationText;
        }

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
            field.textComponent = textComp;
            return textComp;
        }

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
    }
}
