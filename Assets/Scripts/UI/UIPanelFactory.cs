using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using SimRedes.Network;
using SimRedes.Simulation;
using UIComp = SimRedes.UI.UIComponents;
using UIColors = SimRedes.UI.UIComponents.Colors;

namespace SimRedes.UI
{
    public static class UIPanelFactory
    {
        public static Font GetFont(int size = 14)
        {
            Font font = Font.CreateDynamicFontFromOSFont("Arial", size);
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font;
        }

        public static Text CreateIPConfigPanel(Transform canvas, string nodeName, string nodeIP, string nodeMask,
            Action<string> onIPChanged, Action<string> onMaskChanged,
            Action onApply, Action onARP, Action onRouting, Action onCancel)
        {
            Font font = GetFont();
            Font bigFont = GetFont(18);
            Font smallFont = GetFont(12);

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
            CreateNumericKeypad(panelObj.transform, keypadY, font, bigFont, ipField, maskField);

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

        private static void CreateNumericKeypad(Transform parent, float startY, Font font, Font bigFont, InputField ipField, InputField maskField)
        {
            string[] keys = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", ".", "DEL" };
            float keySize = 54;
            float keySpacing = 62;
            float startX = -(keySpacing * 1.5f);

            for (int i = 0; i < keys.Length; i++)
            {
                int row = i / 3;
                int col = i % 3;
                float x = startX + (col * keySpacing);
                float y = startY - (row * keySpacing);

                var keyObj = new GameObject("Key_" + keys[i]);
                keyObj.transform.SetParent(parent, false);
                var keyRect = keyObj.AddComponent<RectTransform>();
                keyRect.anchorMin = new Vector2(0.5f, 0.5f);
                keyRect.anchorMax = new Vector2(0.5f, 0.5f);
                keyRect.anchoredPosition = new Vector2(x, y);
                keyRect.sizeDelta = new Vector2(keySize, keySize);

                var keyImg = keyObj.AddComponent<Image>();
                Texture2D keyTex = UIComp.CreateRoundedRectTexture((int)keySize, (int)keySize, 10, UIColors.buttonNormal, UIColors.borderAccent, 1f);
                keyImg.sprite = Sprite.Create(keyTex, new Rect(0, 0, (int)keySize, (int)keySize), new Vector2(0.5f, 0.5f), 100);
                keyImg.type = Image.Type.Sliced;

                var keyBtn = keyObj.AddComponent<Button>();
                keyBtn.targetGraphic = keyImg;
                keyBtn.colors = UIComp.GetButtonColors(UIColors.buttonNormal);

                var keyTextObj = new GameObject("Text");
                keyTextObj.transform.SetParent(keyObj.transform, false);
                var keyTextRect = keyTextObj.AddComponent<RectTransform>();
                keyTextRect.anchorMin = Vector2.zero;
                keyTextRect.anchorMax = Vector2.one;
                keyTextRect.offsetMin = Vector2.zero;
                keyTextRect.offsetMax = Vector2.zero;

                var keyText = keyTextObj.AddComponent<Text>();
                keyText.text = keys[i];
                keyText.color = UIColors.textPrimary;
                keyText.fontSize = keys[i] == "DEL" ? 10 : 20;
                keyText.alignment = TextAnchor.MiddleCenter;
                keyText.font = keys[i] == "DEL" ? font : bigFont;

                string captured = keys[i];
                keyBtn.onClick.AddListener(() =>
                {
                    InputField activeField = ipField.isFocused ? ipField : maskField;
                    if (captured == "DEL")
                    {
                        if (activeField.text.Length > 0)
                            activeField.text = activeField.text.Substring(0, activeField.text.Length - 1);
                    }
                    else if (captured == ".")
                    {
                        char lastChar = activeField.text.Length > 0 ? activeField.text[activeField.text.Length - 1] : ' ';
                        if (lastChar != '.' && lastChar != ' ')
                            activeField.text = activeField.text + captured;
                    }
                    else
                    {
                        if (activeField.text.Length < 18)
                            activeField.text = activeField.text + captured;
                    }
                });
            }
        }

        public static GameObject CreateScorePanel(Transform canvas)
        {
            Font font = GetFont(18);
            Font bigFont = GetFont(28);

            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(180, 90), 15,
                UIColors.surfacePanel, UIColors.borderAccent, "ScorePanel");

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0f);
            panelRect.pivot = new Vector2(1f, 0f);
            panelRect.anchoredPosition = new Vector2(-10, 10);

            UIComp.CreateInfoText(panelObj.transform, "Puntaje", new Vector2(0, 18), font, 18, UIColors.textSecondary, false);
            UIComp.CreateInfoText(panelObj.transform, "0", new Vector2(0, -5), bigFont, 28, UIColors.textAccent, true);

            return panelObj;
        }

        public static GameObject CreateDevicesPanel(Transform canvas, int nodeCount,
            Action<int> onItemClicked)
        {
            Font font = GetFont(14);

            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(220, 400), 20,
                UIColors.surfacePanel, UIColors.borderAccent, "DevicesPanel");

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(10, -10);

            UIComp.CreateMenuTitle(panelObj.transform, "Dispositivos", 18, new Vector2(0, 170), font);

            return panelObj;
        }

        public static GameObject CreateConnectivityPanel(Transform canvas,
            Action onPingExecute,
            Func<List<(int discId, string name)>> getNodeList,
            Action onBack = null)
        {
            Font font = GetFont();
            Font bigFont = GetFont(16);

            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(550, 480), 25,
                UIColors.surfacePanel, UIColors.borderAccent, "ConnectivityPanel");

            UIComp.CreateMenuTitle(panelObj.transform, "Test de Conectividad", 28, new Vector2(0, 195), bigFont);

            var sourceObj = new GameObject("SourceNode");
            sourceObj.transform.SetParent(panelObj.transform, false);
            var sourceRect = sourceObj.AddComponent<RectTransform>();
            sourceRect.anchorMin = new Vector2(0.5f, 0.5f);
            sourceRect.anchorMax = new Vector2(0.5f, 0.5f);
            sourceRect.anchoredPosition = new Vector2(-120, 100);
            sourceRect.sizeDelta = new Vector2(150, 40);
            var sourceText = sourceObj.AddComponent<Text>();
            sourceText.text = "Origen: -";
            sourceText.color = UIColors.textPrimary;
            sourceText.fontSize = 14;
            sourceText.alignment = TextAnchor.MiddleLeft;
            sourceText.font = font;

            var arrowObj = new GameObject("Arrow");
            arrowObj.transform.SetParent(panelObj.transform, false);
            var arrowRect = arrowObj.AddComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(0.5f, 0.5f);
            arrowRect.anchorMax = new Vector2(0.5f, 0.5f);
            arrowRect.anchoredPosition = new Vector2(0, 100);
            arrowRect.sizeDelta = new Vector2(60, 30);
            var arrowText = arrowObj.AddComponent<Text>();
            arrowText.text = "→ → →";
            arrowText.color = UIColors.textAccent;
            arrowText.fontSize = 20;
            arrowText.alignment = TextAnchor.MiddleCenter;
            arrowText.font = font;

            var destObj = new GameObject("DestNode");
            destObj.transform.SetParent(panelObj.transform, false);
            var destRect = destObj.AddComponent<RectTransform>();
            destRect.anchorMin = new Vector2(0.5f, 0.5f);
            destRect.anchorMax = new Vector2(0.5f, 0.5f);
            destRect.anchoredPosition = new Vector2(120, 100);
            destRect.sizeDelta = new Vector2(150, 40);
            var destText = destObj.AddComponent<Text>();
            destText.text = "Destino: -";
            destText.color = UIColors.textPrimary;
            destText.fontSize = 14;
            destText.alignment = TextAnchor.MiddleLeft;
            destText.font = font;

            var resultObj = new GameObject("ResultDisplay");
            resultObj.transform.SetParent(panelObj.transform, false);
            var resultRect = resultObj.AddComponent<RectTransform>();
            resultRect.anchorMin = new Vector2(0.5f, 0.5f);
            resultRect.anchorMax = new Vector2(0.5f, 0.5f);
            resultRect.anchoredPosition = new Vector2(0, 30);
            resultRect.sizeDelta = new Vector2(60, 60);

            var resultImg = resultObj.AddComponent<Image>();
            Texture2D resultTex = UIComp.CreateRoundedRectTexture(60, 60, 15, new Color(0.5f, 0.5f, 0.5f, 0.3f), UIColors.borderAccent, 2f);
            resultImg.sprite = Sprite.Create(resultTex, new Rect(0, 0, 60, 60), new Vector2(0.5f, 0.5f), 100);
            resultImg.type = Image.Type.Sliced;

            var resultIconObj = new GameObject("ResultIcon");
            resultIconObj.transform.SetParent(resultObj.transform, false);
            var resultIconRect = resultIconObj.AddComponent<RectTransform>();
            resultIconRect.anchorMin = Vector2.zero;
            resultIconRect.anchorMax = Vector2.one;
            resultIconRect.offsetMin = Vector2.zero;
            resultIconRect.offsetMax = Vector2.zero;
            var resultIconInner = resultIconObj.AddComponent<Text>();
            resultIconInner.text = "?";
            resultIconInner.color = UIColors.textSecondary;
            resultIconInner.fontSize = 28;
            resultIconInner.alignment = TextAnchor.MiddleCenter;
            resultIconInner.font = font;

            var resultTextObj = new GameObject("ResultText");
            resultTextObj.transform.SetParent(panelObj.transform, false);
            var resultTextRect = resultTextObj.AddComponent<RectTransform>();
            resultTextRect.anchorMin = new Vector2(0.5f, 0.5f);
            resultTextRect.anchorMax = new Vector2(0.5f, 0.5f);
            resultTextRect.anchoredPosition = new Vector2(0, -20);
            resultTextRect.sizeDelta = new Vector2(480, 40);
            var resultText = resultTextObj.AddComponent<Text>();
            resultText.text = "Selecciona los nodos a testear";
            resultText.color = UIColors.textSecondary;
            resultText.fontSize = 14;
            resultText.alignment = TextAnchor.MiddleCenter;
            resultText.font = font;

            var statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(panelObj.transform, false);
            var statusRect = statusObj.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.5f, 0.5f);
            statusRect.anchorMax = new Vector2(0.5f, 0.5f);
            statusRect.anchoredPosition = new Vector2(0, -60);
            statusRect.sizeDelta = new Vector2(480, 30);
            var statusText = statusObj.AddComponent<Text>();
            statusText.text = "Esperando...";
            statusText.color = UIColors.textSecondary;
            statusText.fontSize = 13;
            statusText.alignment = TextAnchor.MiddleCenter;
            statusText.font = font;

            var hintObj = new GameObject("HintText");
            hintObj.transform.SetParent(panelObj.transform, false);
            var hintRect = hintObj.AddComponent<RectTransform>();
            hintRect.anchorMin = new Vector2(0.5f, 0.5f);
            hintRect.anchorMax = new Vector2(0.5f, 0.5f);
            hintRect.anchoredPosition = new Vector2(0, -95);
            hintRect.sizeDelta = new Vector2(480, 30);
            var hintText = hintObj.AddComponent<Text>();
            hintText.text = "Primero selecciona Origen, luego Destino";
            hintText.color = UIColors.textSecondary;
            hintText.fontSize = 12;
            hintText.alignment = TextAnchor.MiddleCenter;
            hintText.font = font;

            if (onBack != null)
            {
                Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "VOLVER", new Vector2(0, -140), new Vector2(120, 40), font, 14);
                backBtn.onClick.AddListener(() => onBack?.Invoke());
            }

            return panelObj;
        }

        public static GameObject CreateInstructionsPanel(Transform canvas,
            Action onBack)
        {
            Font font = GetFont();
            Font bigFont = GetFont(20);

            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(800, 650), 25,
                UIColors.surfacePanel, UIColors.borderAccent, "InstructionsPanel");

            UIComp.CreateMenuTitle(panelObj.transform, "Como Usar", 32, new Vector2(0, 280), bigFont);

            string leftText =
                "CONTROLES\n" +
                "─────────\n" +
                "1 = Anadir Router\n" +
                "2 = Anadir Switch\n" +
                "3 = Anadir PC\n" +
                "4 = Modo CONEXION\n" +
                "5 = Anadir Fallo\n" +
                "C = Limpiar todo\n" +
                "P = Ping test\n" +
                "R = Eliminar selec.\n" +
                "ESC = Volver";

            string rightText =
                "DISCOS (IDEUM)\n" +
                "─────────────\n" +
                "Los discos fisicos (PUCs)\n" +
                "se colocan en la mesa.\n\n" +
                "Acerca dos discos para\n" +
                "conectarlos (<300px).\n\n" +
                "Haz clic en un nodo para\n" +
                "configurar su IP.";
            UIComp.CreateInfoColumn(panelObj.transform, new Vector2(-170, 50), 300, 400, font, leftText);

            UIComp.CreateInfoColumn(panelObj.transform, new Vector2(170, 50), 300, 400, font, rightText);

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "VOLVER", new Vector2(0, -290), new Vector2(200, 50), font);
            backBtn.onClick.AddListener(() => onBack?.Invoke());

            return panelObj;
        }

        public static void ToggleTopologyExamplePanel(Transform canvas, Font font)
        {
            var existingPanel = GameObject.Find("TopologyExamplePanel");
            if (existingPanel != null)
            {
                UnityEngine.Object.Destroy(existingPanel);
                return;
            }

            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(700, 600), 20,
                UIColors.surfacePanel, UIColors.textAccent);
            panelObj.name = "TopologyExamplePanel";

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;

            UIComp.CreateMenuTitle(panelObj.transform, "Tipos de Topologia", 26, new Vector2(0, 250), font);

            string[] topologyTypes = {
                "ESTRELLA: Un router central conectado a varios nodos perifericos",
                "BUS: Todos los nodos conectados a un cable principal",
                "ANILLO: Cada nodo conectado al siguiente formando un ciclo",
                "ARBOL: Estructura jerarquica con routers y switches",
                "MALLA: Cada nodo conectado a todos los demas"
            };

            float startY = 180;
            for (int i = 0; i < topologyTypes.Length; i++)
            {
                var textObj = new GameObject("TopologyType_" + i);
                textObj.transform.SetParent(panelObj.transform, false);
                var textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0.5f, 0.5f);
                textRect.anchorMax = new Vector2(0.5f, 0.5f);
                textRect.anchoredPosition = new Vector2(0, startY - (i * 45));
                textRect.sizeDelta = new Vector2(550, 40);

                var text = textObj.AddComponent<Text>();
                text.text = topologyTypes[i];
                text.color = UIColors.textPrimary;
                text.fontSize = 14;
                text.alignment = TextAnchor.MiddleLeft;
                text.font = font;
            }

            var instructionsObj = new GameObject("Instructions");
            instructionsObj.transform.SetParent(panelObj.transform, false);
            var instructionsRect = instructionsObj.AddComponent<RectTransform>();
            instructionsRect.anchorMin = new Vector2(0.5f, 0.5f);
            instructionsRect.anchorMax = new Vector2(0.5f, 0.5f);
            instructionsRect.anchoredPosition = new Vector2(0, -150);
            instructionsRect.sizeDelta = new Vector2(600, 150);

            var instructions = instructionsObj.AddComponent<Text>();
            instructions.text = "COMO CONSTRUIR:\n\n" +
                "1. Coloca 2 Routers (tecla 1), 1 Switch (tecla 2) y 1 PC (tecla 3)\n" +
                "2. Coloca un Enlace (tecla 4) entre cada par de dispositivos\n" +
                "3. Verifica que la topologia se detecte automaticamente\n" +
                "4. Usa el boton PING para probar conectividad\n\n" +
                "TIP: Acercar dispositivos crea enlaces automaticamente";
            instructions.color = UIColors.textSecondary;
            instructions.fontSize = 12;
            instructions.alignment = TextAnchor.UpperLeft;
            instructions.font = font;

            var closeBtnObj = new GameObject("CloseBtn");
            closeBtnObj.transform.SetParent(panelObj.transform, false);
            var closeBtnRect = closeBtnObj.AddComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(1f, 1f);
            closeBtnRect.anchorMax = new Vector2(1f, 1f);
            closeBtnRect.anchoredPosition = new Vector2(-15, -15);
            closeBtnRect.sizeDelta = new Vector2(30, 30);

            var closeBtnImg = closeBtnObj.AddComponent<Image>();
            Texture2D closeTex = UIComp.CreateRoundedRectTexture(30, 30, 8, new Color(0.7f, 0.2f, 0.2f), UIColors.borderAccent, 1.5f);
            closeBtnImg.sprite = Sprite.Create(closeTex, new Rect(0, 0, 30, 30), new Vector2(0.5f, 0.5f), 100);
            closeBtnImg.type = Image.Type.Sliced;

            var closeBtn = closeBtnObj.AddComponent<Button>();
            closeBtn.colors = UIComp.GetButtonColors(new Color(0.7f, 0.2f, 0.2f));
            closeBtn.onClick.AddListener(() => UnityEngine.Object.Destroy(panelObj));

            var closeTextObj = new GameObject("Text");
            closeTextObj.transform.SetParent(closeBtnObj.transform, false);
            var closeTextRect = closeTextObj.AddComponent<RectTransform>();
            closeTextRect.anchorMin = Vector2.zero;
            closeTextRect.anchorMax = Vector2.one;
            closeTextRect.offsetMin = Vector2.zero;
            closeTextRect.offsetMax = Vector2.zero;

            var closeText = closeTextObj.AddComponent<Text>();
            closeText.text = "X";
            closeText.color = UIColors.textPrimary;
            closeText.fontSize = 14;
            closeText.fontStyle = FontStyle.Bold;
            closeText.alignment = TextAnchor.MiddleCenter;
            closeText.font = font;

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "CERRAR", new Vector2(0, -255), new Vector2(140, 40), font);
            backBtn.onClick.AddListener(() => UnityEngine.Object.Destroy(panelObj));
        }

        public static GameObject CreateBuildTopologyInfoPanel(Transform canvas)
        {
            Font font = GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(380, 400), 20,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "BuildTopologyInfoPanel";

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;

            UIComp.CreateMenuTitle(panelObj.transform, "Construir Topologia", 22, new Vector2(0, 155), font);

            float y = 110;
            UIComp.CreateInfoText(panelObj.transform, "Coloca discos en la mesa para crear", new Vector2(0, y), font, 13, UIColors.textSecondary, false);
            y -= 22;
            UIComp.CreateInfoText(panelObj.transform, "una topologia de red.", new Vector2(0, y), font, 13, UIColors.textSecondary, false);
            y -= 30;
            UIComp.CreateInfoText(panelObj.transform, "Teclas: 1=Router  2=Switch  3=PC", new Vector2(0, y), font, 12, UIColors.textAccent, false);
            y -= 22;
            UIComp.CreateInfoText(panelObj.transform, "Tecla 4 = Modo CONEXION", new Vector2(0, y), font, 12, UIColors.textAccent, false);
            y -= 26;

            string tipText =
                "Estrella: 1 central + perifericos\n" +
                "Bus: Linea de nodos\n" +
                "Anillo: Ciclo cerrado\n" +
                "Arbol: Routers + Switches\n" +
                "Malla: Todos conectados";

            var tipObj = new GameObject("TipText");
            tipObj.transform.SetParent(panelObj.transform, false);
            var tipRect = tipObj.AddComponent<RectTransform>();
            tipRect.anchorMin = new Vector2(0.5f, 0.5f);
            tipRect.anchorMax = new Vector2(0.5f, 0.5f);
            tipRect.anchoredPosition = new Vector2(0, y - 40);
            tipRect.sizeDelta = new Vector2(320, 130);
            var tipTextComp = tipObj.AddComponent<Text>();
            tipTextComp.text = tipText;
            tipTextComp.color = UIColors.textPrimary;
            tipTextComp.fontSize = 13;
            tipTextComp.alignment = TextAnchor.UpperLeft;
            tipTextComp.font = font;
            tipTextComp.lineSpacing = 1.3f;

            Button closeBtn = UIComp.CreateMenuButton(panelObj.transform, "CloseBtn", "CERRAR", new Vector2(0, -170), new Vector2(120, 40), font, 14);
            closeBtn.onClick.AddListener(() => UnityEngine.Object.Destroy(panelObj));

            return panelObj;
        }

        public static GameObject CreateMainMenu(Transform canvas,
            Action onStartSimulation, Action onActivities, Action onConnectivity,
            Action onInstructions, Action onExit)
        {
            Font font = GetFont();
            var navigatorObj = new GameObject("MenuNavigator");
            navigatorObj.AddComponent<MenuNavigator>();
            var menuObj = new GameObject("MainMenuManager");
            var menuManager = menuObj.AddComponent<MainMenuManager>();

            GameObject mainMenuPanel = UIComp.CreateMenuPanel(canvas, "MainMenuPanel", new Vector2(800, 600));
            UIComp.CreateMenuTitle(mainMenuPanel.transform, "Simulador de Redes", 40, new Vector2(0, 200), font);

            float buttonWidth = 250, buttonHeight = 60, startY = 100;

            Button startBtn = UIComp.CreateMenuButton(mainMenuPanel.transform, "Btn_Iniciar", "INICIAR SIMULACION", new Vector2(0, startY), new Vector2(buttonWidth, buttonHeight), font);
            startBtn.onClick.AddListener(() => onStartSimulation?.Invoke());

            Button activitiesBtn = UIComp.CreateMenuButton(mainMenuPanel.transform, "Btn_Actividades", "ACTIVIDADES", new Vector2(0, startY - 80), new Vector2(buttonWidth, buttonHeight), font);
            activitiesBtn.onClick.AddListener(() => onActivities?.Invoke());

            Button connectivityBtn = UIComp.CreateMenuButton(mainMenuPanel.transform, "Btn_Conectividad", "PRUEBAS Y CONEXIONES", new Vector2(0, startY - 160), new Vector2(buttonWidth, buttonHeight), font);
            connectivityBtn.onClick.AddListener(() => onConnectivity?.Invoke());

            Button instructionsBtn = UIComp.CreateMenuButton(mainMenuPanel.transform, "Btn_Instrucciones", "COMO USAR", new Vector2(0, startY - 240), new Vector2(buttonWidth, buttonHeight), font);
            instructionsBtn.onClick.AddListener(() => onInstructions?.Invoke());

            Button exitBtn = UIComp.CreateMenuButton(mainMenuPanel.transform, "Btn_Salir", "SALIR", new Vector2(0, startY - 320), new Vector2(buttonWidth, buttonHeight), font);
            exitBtn.onClick.AddListener(() => onExit?.Invoke());

            menuManager.mainMenuPanel = mainMenuPanel;
            var navigator = navigatorObj.GetComponent<MenuNavigator>();
            navigator.SetupPanel(mainMenuPanel, () => {
                UnityEngine.Debug.Log("[Menu] ESC en menu principal");
            });
            mainMenuPanel.SetActive(true);
            return mainMenuPanel;
        }

        public static GameObject CreateActivitiesPanel(Transform canvas, Action<int> onSelectActivity, Action onBack)
        {
            Font font = GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(700, 650), 25,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "ActivitiesPanel";
            UIComp.CreateMenuTitle(panelObj.transform, "Selecciona una Actividad", 32, new Vector2(0, 260), font);

            float startY = 200;
            string[] activities = { "Construir Topologia", "Encontrar Fallos", "Tablas de Enrutamiento", "Mejor Ruta", "Enrutamiento Estatico", "Enrutamiento Dinamico", "Escenarios" };

            for (int i = 0; i < activities.Length; i++)
            {
                int capturedIndex = i;
                float yPos = startY - (i * 60);
                Button btn = UIComp.CreateMenuButton(panelObj.transform, "Activity_" + i, activities[i], new Vector2(0, yPos), new Vector2(280, 50), font, 16);
                btn.onClick.AddListener(() => onSelectActivity?.Invoke(capturedIndex));
            }

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "VOLVER AL MENU", new Vector2(0, -290), new Vector2(200, 50), font);
            backBtn.onClick.AddListener(() => onBack?.Invoke());

            var navigator = UnityEngine.Object.FindObjectOfType<MenuNavigator>();
            if (navigator != null)
                navigator.SetupPanel(panelObj, () => onBack?.Invoke());

            return panelObj;
        }

        public static GameObject CreateBestRoutePanel(Transform canvas)
        {
            Font font = GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(520, 580), 25,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "BestRoutePanel";
            UIComp.CreateMenuTitle(panelObj.transform, "Mejor Ruta", 24, new Vector2(0, 250), font);

            var destObj = new GameObject("DestIPText");
            destObj.transform.SetParent(panelObj.transform, false);
            var destRect = destObj.AddComponent<RectTransform>();
            destRect.anchorMin = new Vector2(0.5f, 0.5f);
            destRect.anchorMax = new Vector2(0.5f, 0.5f);
            destRect.anchoredPosition = new Vector2(0, 190);
            destRect.sizeDelta = new Vector2(460, 30);
            var destText = destObj.AddComponent<Text>();
            destText.text = "Destino: --";
            destText.color = Color.white;
            destText.fontSize = 16;
            destText.alignment = TextAnchor.MiddleCenter;
            destText.font = font;

            var resultObj = new GameObject("ResultText");
            resultObj.transform.SetParent(panelObj.transform, false);
            var resultRect = resultObj.AddComponent<RectTransform>();
            resultRect.anchorMin = new Vector2(0.5f, 0.5f);
            resultRect.anchorMax = new Vector2(0.5f, 0.5f);
            resultRect.anchoredPosition = new Vector2(0, -200);
            resultRect.sizeDelta = new Vector2(460, 60);
            var resultText = resultObj.AddComponent<Text>();
            resultText.text = "Selecciona la mejor ruta para llegar al destino.";
            resultText.color = Color.white;
            resultText.fontSize = 13;
            resultText.alignment = TextAnchor.MiddleLeft;
            resultText.font = font;

            var scoreObj = new GameObject("ScoreText");
            scoreObj.transform.SetParent(panelObj.transform, false);
            var scoreRect = scoreObj.AddComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0.5f, 0.5f);
            scoreRect.anchorMax = new Vector2(0.5f, 0.5f);
            scoreRect.anchoredPosition = new Vector2(0, -135);
            scoreRect.sizeDelta = new Vector2(460, 30);
            var scoreText = scoreObj.AddComponent<Text>();
            scoreText.text = "Puntaje: 0 / 0";
            scoreText.color = new Color(0.8f, 0.8f, 0.3f);
            scoreText.fontSize = 14;
            scoreText.alignment = TextAnchor.MiddleCenter;
            scoreText.font = font;

            Button nextBtn = UIComp.CreateMenuButton(panelObj.transform, "NextBtn", "SIGUIENTE", new Vector2(0, -250), new Vector2(160, 40), font, 14);
            nextBtn.gameObject.SetActive(false);

            var activity = UnityEngine.Object.FindObjectOfType<SimRedes.Simulation.BestRouteActivity>();
            if (activity != null)
            {
                activity.destIPText = destText;
                activity.resultText = resultText;
                activity.scoreText = scoreText;
                activity.nextButton = nextBtn;
                nextBtn.onClick.AddListener(() => { activity.NextScenario(); });
            }
            return panelObj;
        }

        public static GameObject CreateRoutingTablesPanel(Transform canvas, Action onBack)
        {
            Font font = GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(650, 500), 25,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "RoutingTablesPanel";
            UIComp.CreateMenuTitle(panelObj.transform, "Tablas de Enrutamiento", 24, new Vector2(0, 200), font);

            var leftPanel = new GameObject("LeftPanel");
            leftPanel.transform.SetParent(panelObj.transform, false);
            var leftRect = leftPanel.AddComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0.5f, 0.5f);
            leftRect.anchorMax = new Vector2(0.5f, 0.5f);
            leftRect.anchoredPosition = new Vector2(-140, 20);
            leftRect.sizeDelta = new Vector2(280, 300);
            var leftText = leftPanel.AddComponent<Text>();
            leftText.text = "TABLAS DE ENRUTAMIENTO\n\n" +
                "Esta actividad muestra las\n" +
                "tablas de enrutamiento.\n\n" +
                "COMANDOS:\n  1 = Router\n  4 = Enlace\n  R = Actualizar\n  C = Limpiar\n  P = Ping";
            leftText.color = UIColors.textPrimary;
            leftText.fontSize = 14;
            leftText.alignment = TextAnchor.UpperLeft;
            leftText.font = font;
            leftText.lineSpacing = 1.1f;

            var rightPanel = new GameObject("RightPanel");
            rightPanel.transform.SetParent(panelObj.transform, false);
            var rightRect = rightPanel.AddComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(0.5f, 0.5f);
            rightRect.anchorMax = new Vector2(0.5f, 0.5f);
            rightRect.anchoredPosition = new Vector2(140, 20);
            rightRect.sizeDelta = new Vector2(280, 300);
            var rightText = rightPanel.AddComponent<Text>();
            rightText.text = "RUTAS:\n\nNo hay routers.\n\nUsa la tecla 1\npara anadir routers.";
            rightText.color = UIColors.textAccent;
            rightText.fontSize = 13;
            rightText.alignment = TextAnchor.UpperLeft;
            rightText.font = font;

            var routingAct = UnityEngine.Object.FindObjectOfType<SimRedes.Simulation.RoutingTablesActivity>();
            if (routingAct != null)
            {
                routingAct.tableText = rightText;
                routingAct.infoText = leftText;
            }

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "VOLVER", new Vector2(0, -200), new Vector2(140, 40), font, 14);
            backBtn.onClick.AddListener(() => onBack?.Invoke());

            return panelObj;
        }

        public static GameObject CreateStaticRoutingPanel(Transform canvas, Action onBack)
        {
            Font font = GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(650, 500), 25,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "StaticRoutingPanel";
            UIComp.CreateMenuTitle(panelObj.transform, "Enrutamiento Estatico", 24, new Vector2(0, 200), font);

            var leftPanel = new GameObject("LeftPanel");
            leftPanel.transform.SetParent(panelObj.transform, false);
            var leftRect = leftPanel.AddComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0.5f, 0.5f);
            leftRect.anchorMax = new Vector2(0.5f, 0.5f);
            leftRect.anchoredPosition = new Vector2(-140, 20);
            leftRect.sizeDelta = new Vector2(280, 300);
            var leftText = leftPanel.AddComponent<Text>();
            leftText.text = "ENRUTAMIENTO ESTATICO\n\n" +
                "Configura rutas manuales\nen los routers.\n\n" +
                "COMANDOS:\n  A = Anadir ruta\n  T = Test route\n  P = Ping";
            leftText.color = UIColors.textPrimary;
            leftText.fontSize = 14;
            leftText.alignment = TextAnchor.UpperLeft;
            leftText.font = font;
            leftText.lineSpacing = 1.1f;

            var rightPanel = new GameObject("RightPanel");
            rightPanel.transform.SetParent(panelObj.transform, false);
            var rightRect = rightPanel.AddComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(0.5f, 0.5f);
            rightRect.anchorMax = new Vector2(0.5f, 0.5f);
            rightRect.anchoredPosition = new Vector2(140, 20);
            rightRect.sizeDelta = new Vector2(280, 300);
            var rightText = rightPanel.AddComponent<Text>();
            rightText.text = "RUTAS CONFIGURADAS:\n\nip route 0.0.0.0 0.0.0.0 192.168.1.254\n\nip route 10.0.0.0 255.0.0.0 10.1.1.1";
            rightText.color = UIColors.textAccent;
            rightText.fontSize = 12;
            rightText.alignment = TextAnchor.UpperLeft;
            rightText.font = font;

            var staticAct = UnityEngine.Object.FindObjectOfType<SimRedes.Simulation.StaticRoutingActivity>();
            if (staticAct != null)
            {
                staticAct.routesText = rightText;
                staticAct.infoText = leftText;
                var feedbackObj = new GameObject("FeedbackText");
                feedbackObj.transform.SetParent(panelObj.transform, false);
                var feedbackRect = feedbackObj.AddComponent<RectTransform>();
                feedbackRect.anchorMin = new Vector2(0.5f, 0f);
                feedbackRect.anchorMax = new Vector2(0.5f, 0f);
                feedbackRect.pivot = new Vector2(0.5f, 0f);
                feedbackRect.anchoredPosition = new Vector2(0, 55);
                feedbackRect.sizeDelta = new Vector2(500, 30);
                var feedbackTextComp = feedbackObj.AddComponent<Text>();
                feedbackTextComp.text = "";
                feedbackTextComp.color = UIColors.textAccent;
                feedbackTextComp.fontSize = 14;
                feedbackTextComp.alignment = TextAnchor.MiddleCenter;
                feedbackTextComp.font = font;
                staticAct.feedbackText = feedbackTextComp;
            }

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "VOLVER", new Vector2(0, -200), new Vector2(140, 40), font, 14);
            backBtn.onClick.AddListener(() => onBack?.Invoke());
            return panelObj;
        }

        public static GameObject CreateDynamicRoutingPanel(Transform canvas,
            Action onSelectRIP, Action onSelectOSPF, Action<GameObject> onStart,
            Action<GameObject> onStop, Action<GameObject> onClearRoutes,
            Action<GameObject> onViewRoutes, Action onBack)
        {
            Font font = GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(750, 580), 25,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "DynamicRoutingPanel";
            UIComp.CreateMenuTitle(panelObj.transform, "Enrutamiento Dinamico", 24, new Vector2(0, 240), font);

            var leftPanel = new GameObject("LeftPanel");
            leftPanel.transform.SetParent(panelObj.transform, false);
            var leftRect = leftPanel.AddComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0.5f, 0.5f);
            leftRect.anchorMax = new Vector2(0.5f, 0.5f);
            leftRect.anchoredPosition = new Vector2(-180, 50);
            leftRect.sizeDelta = new Vector2(320, 350);
            var leftText = leftPanel.AddComponent<Text>();
            leftText.text = "PROTOCOLOS DISPONIBLES:\n\n" +
                "R = RIP (conteo de hops)\n" +
                "O = OSPF (costo por enlace)\n\n" +
                "CONTROLES:\n  S = Iniciar Advertisement\n  T = Ver convergencia\n  L = Limpiar rutas\n\n" +
                "Usa teclas 1-4 para\ncrear la topologia.";
            leftText.color = UIColors.textPrimary;
            leftText.fontSize = 14;
            leftText.alignment = TextAnchor.UpperLeft;
            leftText.font = font;
            leftText.lineSpacing = 1.2f;

            var rightPanel = new GameObject("RightPanel");
            rightPanel.transform.SetParent(panelObj.transform, false);
            var rightRect = rightPanel.AddComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(0.5f, 0.5f);
            rightRect.anchorMax = new Vector2(0.5f, 0.5f);
            rightRect.anchoredPosition = new Vector2(180, 50);
            rightRect.sizeDelta = new Vector2(320, 350);
            var rightText = rightPanel.AddComponent<Text>();
            rightText.text = "SIMULACION:\n\nProtocolo: Ninguno\n\nPresiona START para\niniciar el protocolo.";
            rightText.color = UIColors.textAccent;
            rightText.fontSize = 14;
            rightText.alignment = TextAnchor.UpperLeft;
            rightText.font = font;
            rightText.name = "StatusText";

            float btnY = -220, btnSpacing = 130;
            Button ripBtn = UIComp.CreateMenuButton(panelObj.transform, "RIPBtn", "RIP", new Vector2(-btnSpacing, btnY), new Vector2(100, 45), font, 16);
            ripBtn.onClick.AddListener(() => { UpdateStatusText(panelObj, "RIP"); onSelectRIP?.Invoke(); });

            Button ospfBtn = UIComp.CreateMenuButton(panelObj.transform, "OSBFBtn", "OSPF", new Vector2(0, btnY), new Vector2(100, 45), font, 16);
            ospfBtn.onClick.AddListener(() => { UpdateStatusText(panelObj, "OSPF"); onSelectOSPF?.Invoke(); });

            Button startBtn = UIComp.CreateMenuButton(panelObj.transform, "StartBtn", "START", new Vector2(btnSpacing, btnY), new Vector2(100, 45), font, 16);
            startBtn.onClick.AddListener(() => onStart?.Invoke(panelObj));

            btnY -= 60;
            Button stopBtn = UIComp.CreateMenuButton(panelObj.transform, "StopBtn", "STOP", new Vector2(-btnSpacing, btnY), new Vector2(100, 40), font, 14);
            stopBtn.onClick.AddListener(() => onStop?.Invoke(panelObj));

            Button clearBtn = UIComp.CreateMenuButton(panelObj.transform, "ClearBtn", "LIMPIAR", new Vector2(0, btnY), new Vector2(100, 40), font, 14);
            clearBtn.onClick.AddListener(() => onClearRoutes?.Invoke(panelObj));

            Button viewBtn = UIComp.CreateMenuButton(panelObj.transform, "ViewBtn", "VER RUTAS", new Vector2(btnSpacing, btnY), new Vector2(100, 40), font, 14);
            viewBtn.onClick.AddListener(() => onViewRoutes?.Invoke(panelObj));

            var dynAct = UnityEngine.Object.FindObjectOfType<SimRedes.Simulation.DynamicRoutingActivity>();
            if (dynAct != null)
            {
                dynAct.tablesText = rightText;
                dynAct.infoText = leftText;
                dynAct.protocolText = rightText;
                var feedbackObj = new GameObject("FeedbackText");
                feedbackObj.transform.SetParent(panelObj.transform, false);
                var feedbackRect = feedbackObj.AddComponent<RectTransform>();
                feedbackRect.anchorMin = new Vector2(0.5f, 0f);
                feedbackRect.anchorMax = new Vector2(0.5f, 0f);
                feedbackRect.pivot = new Vector2(0.5f, 0f);
                feedbackRect.anchoredPosition = new Vector2(0, 40);
                feedbackRect.sizeDelta = new Vector2(500, 30);
                var feedbackTextComp = feedbackObj.AddComponent<Text>();
                feedbackTextComp.text = "";
                feedbackTextComp.color = UIColors.textAccent;
                feedbackTextComp.fontSize = 14;
                feedbackTextComp.alignment = TextAnchor.MiddleCenter;
                feedbackTextComp.font = font;
                dynAct.feedbackText = feedbackTextComp;
            }

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "VOLVER", new Vector2(0, -250), new Vector2(140, 40), font, 14);
            backBtn.onClick.AddListener(() => onBack?.Invoke());
            return panelObj;
        }

        public static GameObject CreateScenariosPanel(Transform canvas,
            System.Collections.Generic.List<SimRedes.Simulation.PredefinedScenarios.NetworkScenario> scenarios,
            Action<int> onScenarioClick, Action onBack)
        {
            Font font = GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(800, 800), 25,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "ScenariosPanel";
            UIComp.CreateMenuTitle(panelObj.transform, "Escenarios Preconfigurados", 32, new Vector2(0, 320), font);

            float startY = 170;
            float itemSpacing = 115;
            int displayCount = Mathf.Min(scenarios.Count, 5);

            for (int i = 0; i < displayCount; i++)
            {
                int scenarioIndex = i;
                var scenario = scenarios[i];

                var itemBg = new GameObject($"ScenarioItem_{i}");
                itemBg.transform.SetParent(panelObj.transform, false);
                var itemRect = itemBg.AddComponent<RectTransform>();
                itemRect.anchorMin = new Vector2(0.5f, 1f);
                itemRect.anchorMax = new Vector2(0.5f, 1f);
                itemRect.pivot = new Vector2(0.5f, 1f);
                itemRect.anchoredPosition = new Vector2(0, -startY - (i * itemSpacing));
                itemRect.sizeDelta = new Vector2(700, 100);

                var itemImg = itemBg.AddComponent<Image>();
                Texture2D itemTex = UIComp.CreateRoundedRectTexture(700, 100, 15, UIColors.surfaceElevated, UIColors.borderAccent, 3f);
                itemImg.sprite = Sprite.Create(itemTex, new Rect(0, 0, 700, 100), new Vector2(0.5f, 0.5f), 100);
                itemImg.type = Image.Type.Sliced;

                string difficultyText = scenario.difficulty == SimRedes.Simulation.PredefinedScenarios.ScenarioDifficulty.Basico ? "BASICO" :
                    (scenario.difficulty == SimRedes.Simulation.PredefinedScenarios.ScenarioDifficulty.Intermedio ? "INTERMEDIO" : "AVANZADO");
                Color diffColor = scenario.difficulty == SimRedes.Simulation.PredefinedScenarios.ScenarioDifficulty.Basico ? Color.green :
                    (scenario.difficulty == SimRedes.Simulation.PredefinedScenarios.ScenarioDifficulty.Intermedio ? Color.yellow : Color.red);

                var nameObj = new GameObject("Name");
                nameObj.transform.SetParent(itemBg.transform, false);
                var nameRect = nameObj.AddComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0f, 0.5f);
                nameRect.anchorMax = new Vector2(0.5f, 0.5f);
                nameRect.pivot = new Vector2(0f, 0.5f);
                nameRect.anchoredPosition = new Vector2(20, 18);
                nameRect.offsetMin = Vector2.zero;
                nameRect.offsetMax = new Vector2(0, 25);
                var nameText = nameObj.AddComponent<Text>();
                nameText.text = scenario.name;
                nameText.font = font;
                nameText.fontSize = 18;
                nameText.color = UIColors.textPrimary;
                nameText.fontStyle = FontStyle.Bold;
                nameText.alignment = TextAnchor.MiddleLeft;
                nameText.resizeTextForBestFit = true;
                nameText.resizeTextMinSize = 14;

                var diffObj = new GameObject("Difficulty");
                diffObj.transform.SetParent(itemBg.transform, false);
                var diffRect = diffObj.AddComponent<RectTransform>();
                diffRect.anchorMin = new Vector2(0.5f, 0.5f);
                diffRect.anchorMax = new Vector2(1f, 0.5f);
                diffRect.pivot = new Vector2(0.5f, 0.5f);
                diffRect.anchoredPosition = new Vector2(0, 18);
                diffRect.offsetMin = Vector2.zero;
                diffRect.offsetMax = new Vector2(-20, 25);
                var diffText = diffObj.AddComponent<Text>();
                diffText.text = difficultyText;
                diffText.font = font;
                diffText.fontSize = 13;
                diffText.color = diffColor;
                diffText.fontStyle = FontStyle.Bold;
                diffText.alignment = TextAnchor.MiddleCenter;

                var descObj = new GameObject("Description");
                descObj.transform.SetParent(itemBg.transform, false);
                var descRect = descObj.AddComponent<RectTransform>();
                descRect.anchorMin = new Vector2(0f, 0.5f);
                descRect.anchorMax = new Vector2(1f, 0.5f);
                descRect.pivot = new Vector2(0.5f, 0.5f);
                descRect.anchoredPosition = new Vector2(0, -12);
                descRect.sizeDelta = new Vector2(660, 25);
                var descText = descObj.AddComponent<Text>();
                descText.text = scenario.description;
                descText.font = font;
                descText.fontSize = 14;
                descText.color = UIColors.textSecondary;
                descText.alignment = TextAnchor.MiddleCenter;

                var btn = itemBg.AddComponent<Button>();
                ColorBlock colors = new ColorBlock();
                colors.normalColor = UIColors.surfaceElevated;
                colors.highlightedColor = UIColors.buttonHover;
                colors.pressedColor = UIColors.buttonNormal;
                colors.colorMultiplier = 1f;
                btn.colors = colors;
                btn.onClick.AddListener(() => onScenarioClick?.Invoke(scenarioIndex));
            }

            var navigator = UnityEngine.Object.FindObjectOfType<MenuNavigator>();
            if (navigator != null)
                navigator.SetupPanel(panelObj, () => onBack?.Invoke());

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "VOLVER", new Vector2(0, -360), new Vector2(200, 55), font, 20);
            backBtn.onClick.AddListener(() => onBack?.Invoke());
            return panelObj;
        }

        public static GameObject CreateScenarioInfoPanel(Transform canvas,
            SimRedes.Simulation.PredefinedScenarios.NetworkScenario scenario,
            Action onStart, Action onCancel)
        {
            Font font = GetFont();
            GameObject panelObj = UIComp.CreateRoundedPanel(canvas, new Vector2(550, 500), 20,
                UIColors.surfacePanel, UIColors.borderAccent);
            panelObj.name = "ScenarioInfoPanel";

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;

            UIComp.CreateMenuTitle(panelObj.transform, scenario.name, 22, new Vector2(0, 200), font);

            float yPos = 155;
            float lineHeight = 30;

            UIComp.CreateInfoText(panelObj.transform, "OBJETIVOS:", new Vector2(0, yPos), font, 14, UIColors.textAccent, true);
            yPos -= lineHeight;

            foreach (var obj in scenario.objectives)
            {
                UIComp.CreateInfoText(panelObj.transform, "- " + obj, new Vector2(-20, yPos), font, 12, UIColors.textPrimary, false);
                yPos -= lineHeight - 5;
            }

            yPos -= 10;
            UIComp.CreateInfoText(panelObj.transform, "AYUDAS:", new Vector2(0, yPos), font, 14, UIColors.textSecondary, true);
            yPos -= lineHeight;

            foreach (var hint in scenario.hints)
            {
                UIComp.CreateInfoText(panelObj.transform, "* " + hint, new Vector2(-20, yPos), font, 11, UIColors.textSecondary, false);
                yPos -= lineHeight - 5;
            }

            Button startBtn = UIComp.CreateMenuButton(panelObj.transform, "StartScenarioBtn", "INICIAR", new Vector2(-80, -200), new Vector2(130, 45), font, 16);
            startBtn.onClick.AddListener(() => { onStart?.Invoke(); UnityEngine.Object.Destroy(panelObj); });

            Button cancelBtn = UIComp.CreateMenuButton(panelObj.transform, "CancelBtn", "CANCELAR", new Vector2(80, -200), new Vector2(130, 45), font, 16);
            cancelBtn.onClick.AddListener(() => { onCancel?.Invoke(); UnityEngine.Object.Destroy(panelObj); });

            return panelObj;
        }

        public static GameObject CreateARPPanel(Transform canvas, SimRedes.Network.NetworkNode node)
        {
            Font font = GetFont();
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

        public static GameObject CreateRoutingPanel(Transform canvas, SimRedes.Network.NetworkNode node,
            Action<int> onDeleteRoute, Action<int> onAddRoute, Action onClose)
        {
            Font font = GetFont();
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

        public static GameObject CreateAddRoutePanel(Transform canvas, SimRedes.Network.NetworkNode node,
            System.Action<string, string, string, string> onAddRoute, Action onCancel)
        {
            Font font = GetFont();
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
            var destInput = CreateConfigField(panelObj.transform, "192.168.0.0", inputX - 30, inputX + 90, fieldY, font);

            fieldY -= 65;
            UIComp.CreateInfoText(panelObj.transform, "Mask:", new Vector2(labelX, fieldY), font, 13, UIColors.textSecondary, false);
            var maskInput = CreateConfigField(panelObj.transform, "255.255.255.0", inputX - 30, inputX + 90, fieldY, font);

            fieldY -= 65;
            UIComp.CreateInfoText(panelObj.transform, "Next Hop:", new Vector2(labelX, fieldY), font, 13, UIColors.textSecondary, false);
            var nextHopInput = CreateConfigField(panelObj.transform, "192.168.1.1", inputX - 30, inputX + 90, fieldY, font);

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

        private static InputField CreateConfigField(Transform parent, string placeholder, float leftX, float rightX, float yPos, Font font)
        {
            var inputObj = new GameObject("ConfigField");
            inputObj.transform.SetParent(parent, false);
            var inputRect = inputObj.AddComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0.5f, 0.5f);
            inputRect.anchorMax = new Vector2(0.5f, 0.5f);
            inputRect.anchoredPosition = new Vector2((leftX + rightX) / 2, yPos);
            inputRect.sizeDelta = new Vector2(rightX - leftX, 35);

            var inputImg = inputObj.AddComponent<Image>();
            inputImg.color = new Color(0.2f, 0.25f, 0.3f, 0.9f);

            var inputField = inputObj.AddComponent<InputField>();
            inputField.targetGraphic = inputImg;

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(inputObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8, 3);
            textRect.offsetMax = new Vector2(-8, -3);
            var textComp = textObj.AddComponent<Text>();
            textComp.text = "";
            textComp.font = font;
            textComp.fontSize = 14;
            textComp.color = Color.white;
            textComp.alignment = TextAnchor.MiddleLeft;
            inputField.textComponent = textComp;

            var placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(inputObj.transform, false);
            var placeholderRect = placeholderObj.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = new Vector2(8, 3);
            placeholderRect.offsetMax = new Vector2(-8, -3);
            var placeholderComp = placeholderObj.AddComponent<Text>();
            placeholderComp.text = placeholder;
            placeholderComp.font = font;
            placeholderComp.fontSize = 14;
            placeholderComp.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            placeholderComp.alignment = TextAnchor.MiddleLeft;
            inputField.placeholder = placeholderComp;

            return inputField;
        }

        public static void CreateVLANPanel(Transform canvas, SimRedes.Network.TopologyManager topology)
        {
            var existingPanel = GameObject.Find("VLANPanel");
            if (existingPanel != null) UnityEngine.Object.Destroy(existingPanel);

            Font font = GetFont();
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
            vlanInput.textComponent.font = font;
            vlanInput.textComponent.fontSize = 16;
            vlanInput.textComponent.color = UIColors.textPrimary;

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
            var nodeNames = new System.Collections.Generic.List<string> { "Seleccionar..." };
            foreach (var node in nodeList)
                nodeNames.Add($"{node.Name} (VLAN {topology.VLAN.GetNodeVLAN(node)})");

            GameObject dropdownObj = CreateDropdown(panelObj.transform, nodeNames, font, new Vector2(0, 55), new Vector2(250, 30));
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
            vlanSelect.textComponent.font = font;
            vlanSelect.textComponent.fontSize = 16;
            vlanSelect.textComponent.color = UIColors.textPrimary;

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

        private static void UpdateVLANListDisplay(Transform parent, SimRedes.Network.TopologyManager topology, Font font)
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

        public static void CreateACLPanel(Transform canvas, SimRedes.Network.TopologyManager topology)
        {
            var existingPanel = GameObject.Find("ACLPanel");
            if (existingPanel != null) UnityEngine.Object.Destroy(existingPanel);

            Font font = GetFont();
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
            nameInput.textComponent.font = font;
            nameInput.textComponent.fontSize = 16;
            nameInput.textComponent.color = UIColors.textPrimary;

            UIComp.CreateInfoText(panelObj.transform, "ACCION:", new Vector2(-200, 150), font, 14, UIColors.textSecondary, false);

            GameObject actionDropdown = CreateDropdown(panelObj.transform,
                new System.Collections.Generic.List<string> { "PERMIT", "DENY" }, font,
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
            srcInput.textComponent.font = font;
            srcInput.textComponent.fontSize = 16;
            srcInput.textComponent.color = UIColors.textPrimary;

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
            dstInput.textComponent.font = font;
            dstInput.textComponent.fontSize = 16;
            dstInput.textComponent.color = UIColors.textPrimary;

            Button addRuleBtn = UIComp.CreateMenuButton(panelObj.transform, "AddRuleBtn", "AGREGAR REGLA", new Vector2(150, 30), new Vector2(130, 38), font, 14);
            addRuleBtn.onClick.AddListener(() => {
                string aclName = string.IsNullOrEmpty(nameInput.text) ? "MI_ACL" : nameInput.text;
                topology.ACL.CreateACL(aclName);
                var rule = new SimRedes.Network.ACLRule
                {
                    Action = actionDD.value == 0 ? SimRedes.Network.ACLAction.Permit : SimRedes.Network.ACLAction.Deny,
                    SourceIP = srcInput.text,
                    DestIP = dstInput.text,
                    Description = $"Rule {topology.ACL.GetRules(aclName).Count + 1}"
                };
                topology.ACL.AddRule(aclName, rule);
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

        private static void UpdateACLListDisplay(Transform parent, SimRedes.Network.TopologyManager topology, Font font)
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
                text += $"  Seq {rule.Sequence}: {rule.Action} {rule.SourceIP} → {rule.DestIP}\n";

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

        public static void CreateNATPanel(Transform canvas, SimRedes.Network.TopologyManager topology)
        {
            var existingPanel = GameObject.Find("NATPanel");
            if (existingPanel != null) UnityEngine.Object.Destroy(existingPanel);

            Font font = GetFont();
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
            pubIpInput.textComponent.font = font;
            pubIpInput.textComponent.fontSize = 16;
            pubIpInput.textComponent.color = UIColors.textPrimary;

            Button setPubBtn = UIComp.CreateMenuButton(panelObj.transform, "SetPubBtn", "SET", new Vector2(180, 190), new Vector2(60, 30), font, 12);
            setPubBtn.onClick.AddListener(() => { topology.NAT.SetPublicIP(pubIpInput.text); });

            UIComp.CreateInfoText(panelObj.transform, "TIPO NAT:", new Vector2(-180, 145), font, 14, UIColors.textSecondary, false);

            GameObject natTypeDD = CreateDropdown(panelObj.transform,
                new System.Collections.Generic.List<string> { "ESTATICA", "DINAMICA", "PAT" }, font,
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
            intIpInput.textComponent.font = font;
            intIpInput.textComponent.fontSize = 16;
            intIpInput.textComponent.color = UIColors.textPrimary;

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
            extIpInput.textComponent.font = font;
            extIpInput.textComponent.fontSize = 16;
            extIpInput.textComponent.color = UIColors.textPrimary;

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
            portInput.textComponent.font = font;
            portInput.textComponent.fontSize = 16;
            portInput.textComponent.color = UIColors.textPrimary;

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

        private static void UpdateNATListDisplay(Transform parent, SimRedes.Network.TopologyManager topology, Font font)
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

        private static GameObject CreateDropdown(Transform parent, System.Collections.Generic.List<string> options, Font font, Vector2 position, Vector2 size)
        {
            GameObject dropdownObj = new GameObject("Dropdown");
            dropdownObj.transform.SetParent(parent, false);
            var dropdownRect = dropdownObj.AddComponent<RectTransform>();
            dropdownRect.anchorMin = new Vector2(0.5f, 0.5f);
            dropdownRect.anchorMax = new Vector2(0.5f, 0.5f);
            dropdownRect.anchoredPosition = position;
            dropdownRect.sizeDelta = size;

            var dropdown = dropdownObj.AddComponent<UnityEngine.UI.Dropdown>();
            dropdown.options.Clear();
            foreach (string opt in options)
                dropdown.options.Add(new UnityEngine.UI.Dropdown.OptionData(opt));

            var template = new GameObject("Template");
            template.transform.SetParent(dropdownObj.transform, false);
            var templateRect = template.AddComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0f, 0f);
            templateRect.anchorMax = new Vector2(1f, 0f);
            templateRect.pivot = new Vector2(0.5f, 1f);
            templateRect.anchoredPosition = Vector2.zero;
            templateRect.sizeDelta = new Vector2(0, 150);
            template.SetActive(false);
            dropdown.template = templateRect;

            var caption = new GameObject("Label");
            caption.transform.SetParent(dropdownObj.transform, false);
            var captionRect = caption.AddComponent<RectTransform>();
            captionRect.anchorMin = Vector2.zero;
            captionRect.anchorMax = Vector2.one;
            captionRect.offsetMin = new Vector2(10, 0);
            captionRect.offsetMax = new Vector2(-25, 0);
            var captionText = caption.AddComponent<Text>();
            captionText.text = options[0];
            captionText.font = font;
            captionText.fontSize = 14;
            captionText.color = UIColors.textPrimary;
            captionText.alignment = TextAnchor.MiddleLeft;
            dropdown.captionText = captionText;

            var arrow = new GameObject("Arrow");
            arrow.transform.SetParent(dropdownObj.transform, false);
            var arrowRect = arrow.AddComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(1f, 0.5f);
            arrowRect.anchorMax = new Vector2(1f, 0.5f);
            arrowRect.pivot = new Vector2(0.5f, 0.5f);
            arrowRect.anchoredPosition = new Vector2(-12, 0);
            arrowRect.sizeDelta = new Vector2(15, 15);
            var arrowText = arrow.AddComponent<Text>();
            arrowText.text = "▼";
            arrowText.fontSize = 10;
            arrowText.color = UIColors.textSecondary;
            arrowText.alignment = TextAnchor.MiddleCenter;

            return dropdownObj;
        }

        public static void CreateStatusPanel(Transform canvasTransform)
        {
            var existingText = GameObject.Find("StatusPanel");
            if (existingText != null) UnityEngine.Object.Destroy(existingText);

            var panelObj = new GameObject("StatusPanel");
            panelObj.transform.SetParent(canvasTransform, false);

            var rect = panelObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(400, 150);

            var image = panelObj.AddComponent<Image>();
            image.color = new Color(0.02f, 0.02f, 0.05f, 0.92f);

            Texture2D panelTex = UIComp.CreateRoundedRectTexture(400, 150, 15,
                new Color(0.02f, 0.02f, 0.05f, 0.92f),
                new Color(0.2f, 0.6f, 1f, 0.6f), 2f);
            image.sprite = Sprite.Create(panelTex, new Rect(0, 0, 400, 150), new Vector2(0.5f, 0.5f), 100);
            image.type = Image.Type.Sliced;

            var textObj = new GameObject("StatusText");
            textObj.transform.SetParent(panelObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(15, 10);
            textRect.offsetMax = new Vector2(-15, -10);

            var text = textObj.AddComponent<Text>();
            text.text = "Simulador de Redes\nEsperando discos...";
            text.color = UIColors.textPrimary;
            text.fontSize = 20;
            text.alignment = TextAnchor.UpperLeft;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static void UpdateStatusText(GameObject panel, string protocol)
        {
            var statusText = panel.transform.Find("RightPanel")?.GetComponent<Text>();
            if (statusText != null)
            {
                statusText.text = $"SIMULACION:\n\nProtocolo: {protocol}\n\nPresiona START para\niniciar el protocolo.";
                statusText.color = UIColors.textAccent;
            }
        }
    }
}
