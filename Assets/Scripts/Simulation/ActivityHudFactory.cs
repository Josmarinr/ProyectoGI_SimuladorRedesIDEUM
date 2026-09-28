using UnityEngine;
using UnityEngine.UI;
using SimRedes.Network;
using SimRedes.Tangible;
using SimRedes.UI;
using UIComp = SimRedes.UI.UIComponents;
using UIColors = SimRedes.UI.UIComponents.Colors;

namespace SimRedes.Simulation
{
    /// <summary>
    /// Fabrica del HUD de simulacion. Construye y actualiza el panel de topologia
    /// (TopologyInfoPanel) con sus indicadores, botones de accion (LIMPIAR, PING,
    /// VOLVER, CONECTAR, DESCONECTAR), botones de dispositivos y red avanzada
    /// (VLAN, ACL, NAT), ademas del spawn de dispositivos con anti-doble-clic.
    /// Extraido de <see cref="ActivityLoader"/> (tarea C5) sin cambios de comportamiento.
    /// </summary>
    public class ActivityHudFactory
    {
        private readonly ActivityLoader owner;
        private Font font;

        // Proteccion contra doble clic (EventSystem puede generar 2 eventos con Both input mode)
        private float lastAddTime = 0f;
        private SimRedes.Network.DeviceType lastAddType = SimRedes.Network.DeviceType.Unknown;
        private int spawnIndex = 0;
        private readonly Vector2[] spawnPositions = new Vector2[]
        {
            new Vector2(100, 400), new Vector2(300, 350), new Vector2(200, 500),
            new Vector2(150, 600), new Vector2(250, 600), new Vector2(300, 250),
            new Vector2(100, 250), new Vector2(200, 700), new Vector2(300, 700)
        };

        /// <summary>
        /// Crea una fabrica de HUD asociada al <see cref="ActivityLoader"/> dueño,
        /// que aporta la ruta de vuelta al menu y el canvas compartidos.
        /// </summary>
        /// <param name="owner">Fachada <see cref="ActivityLoader"/> que usa como contexto.</param>
        public ActivityHudFactory(ActivityLoader owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Crea el panel HUD de simulacion con informacion de topologia, botones de accion
        /// (LIMPIAR, PING, VOLVER, CONECTAR, DESCONECTAR) y botones de red avanzada (VLAN, ACL, NAT).
        /// </summary>
        /// <param name="ct">Transform del canvas raiz.</param>
        public void CreateSimulationHUDPanel(Transform ct)
        {
            var existingPanel = GameObject.Find("TopologyInfoPanel");
            if (existingPanel != null)
            {
                existingPanel.SetActive(false);
                // B4: liberar los Sprite del panel antes de destruirlo
                UIComp.SafeDestroyPanelSprites(existingPanel);
                UnityEngine.Object.Destroy(existingPanel);
            }

            var existingInfo = GameObject.Find("TopologyExamplePanel");
            if (existingInfo != null)
            {
                existingInfo.SetActive(false);
                UIComp.SafeDestroyPanelSprites(existingInfo);
                UnityEngine.Object.Destroy(existingInfo);
            }

            font = UIComp.GetFont();

            GameObject panelObj = UIComp.CreateRoundedPanel(ct, new Vector2(560, 760), 20,
                UIColors.surfaceElevated, UIColors.borderAccent);
            panelObj.name = "TopologyInfoPanel";

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);
            panelRect.anchoredPosition = new Vector2(-20, -20);

            UIComp.CreateMenuTitle(panelObj.transform, "Topologia", 36, new Vector2(0, 280), font);

            Button infoBtn = UIComp.CreateSmallInfoButton(panelObj.transform, new Vector2(220, 280), font);
            infoBtn.onClick.AddListener(() => UIPanelFactory.ToggleTopologyExamplePanel(ct, font));

            float infoY = 210f;
            float lineHeight = 42f;

            var topologyTypeText = UIComp.CreateInfoText(panelObj.transform, "Topologia: Sin topologia",
                new Vector2(0, infoY), font, 26, UIColors.textAccent, true);
            topologyTypeText.gameObject.name = "TopologyTypeText";

            infoY -= lineHeight;
            var linkText = UIComp.CreateInfoText(panelObj.transform, "Enlaces: 0",
                new Vector2(0, infoY), font, 24, UIColors.textPrimary, true);
            linkText.gameObject.name = "LinkCountText";

            infoY -= lineHeight + 12;
            var devicesTitle = UIComp.CreateInfoText(panelObj.transform, "Dispositivos:",
                new Vector2(0, infoY), font, 23, UIColors.textSecondary, false);
            UIComp.ApplyTitleStyle(devicesTitle);

            infoY -= 36;
            var routerText = UIComp.CreateInfoText(panelObj.transform, "  Routers: 0",
                new Vector2(0, infoY), font, 23, UIColors.textSecondary, false);
            routerText.gameObject.name = "RouterCountText";

            infoY -= 34;
            var switchText = UIComp.CreateInfoText(panelObj.transform, "  Switches: 0",
                new Vector2(0, infoY), font, 23, UIColors.textSecondary, false);
            switchText.gameObject.name = "SwitchCountText";

            infoY -= 34;
            var pcText = UIComp.CreateInfoText(panelObj.transform, "  PCs: 0",
                new Vector2(0, infoY), font, 23, UIColors.textSecondary, false);
            pcText.gameObject.name = "PCCountText";

            infoY -= lineHeight + 10;
            var pingResultText = UIComp.CreateInfoText(panelObj.transform, "Ping: Seleccionar origen",
                new Vector2(0, infoY), font, 24, UIColors.textSecondary, false);
            pingResultText.gameObject.name = "PingResultText";

            float btnY = -125f;
            float btnSpacing = 150f;

            Button clearBtn = UIComp.CreateMenuButton(panelObj.transform, "ClearBtn", "LIMPIAR",
                new Vector2(-btnSpacing, btnY), new Vector2(140, 58), font, 21);
            clearBtn.onClick.AddListener(() => {
                var cleanup = UnityEngine.Object.FindAnyObjectByType<SceneCleanupService>();
                if (cleanup != null)
                    cleanup.ClearSimulation(UnityEngine.Object.FindAnyObjectByType<TopologyManager>(),
                        UnityEngine.Object.FindAnyObjectByType<NodeVisualizer>(),
                        UnityEngine.Object.FindAnyObjectByType<DebugDiscSimulator>());
            });

            Button pingBtn = UIComp.CreateMenuButton(panelObj.transform, "PingBtn", "PING",
                new Vector2(0, btnY), new Vector2(140, 58), font, 21);
            var pingCtrl = UnityEngine.Object.FindAnyObjectByType<PingModeController>();
            if (pingCtrl != null)
            {
                pingCtrl.StorePingReferences(pingBtn, pingResultText);
                pingBtn.onClick.AddListener(() => pingCtrl.TogglePingMode());
            }

            Button backBtn = UIComp.CreateMenuButton(panelObj.transform, "BackBtn", "VOLVER",
                new Vector2(btnSpacing, btnY), new Vector2(140, 58), font, 21);
            backBtn.onClick.AddListener(() => owner.GoBackToMainMenu());

            float linkBtnY = -195f;
            float linkBtnSpacing = 155f;

            // Asegurar que LinkModeController existe (por si la limpieza anterior lo destruyo)
            var linkCtrl = LinkModeController.Instance;
            if (linkCtrl == null)
            {
                var gm = GameObject.Find("GameManager");
                if (gm != null)
                {
                    linkCtrl = gm.GetComponent<LinkModeController>();
                    if (linkCtrl == null)
                        linkCtrl = gm.AddComponent<LinkModeController>();
                }
            }

            Button connectBtn = UIComp.CreateMenuButton(panelObj.transform, "ConnectBtn", "CONECTAR",
                new Vector2(-linkBtnSpacing, linkBtnY), new Vector2(140, 58), font, 20);
            if (linkCtrl != null)
            {
                connectBtn.onClick.AddListener(() => linkCtrl.ToggleLinkMode("connect"));
            }

            Button disconnectBtn = UIComp.CreateMenuButton(panelObj.transform, "DisconnectBtn", "DESCONECTAR",
                new Vector2(linkBtnSpacing, linkBtnY), new Vector2(140, 58), font, 18);
            if (linkCtrl != null)
            {
                disconnectBtn.onClick.AddListener(() => linkCtrl.ToggleLinkMode("disconnect"));
                linkCtrl.StoreLinkButtons(connectBtn, disconnectBtn);
            }

            float devBtnY = -265f;
            float devSpacing = 120f;

            Button routerBtn = UIComp.CreateMenuButton(panelObj.transform, "RouterBtn", "ROUTER",
                new Vector2(-devSpacing, devBtnY), new Vector2(110, 48), font, 17);
            routerBtn.onClick.AddListener(() => AddDeviceAtSpawn(Network.DeviceType.Router));

            Button switchBtn = UIComp.CreateMenuButton(panelObj.transform, "SwitchBtn", "SWITCH",
                new Vector2(0, devBtnY), new Vector2(110, 48), font, 17);
            switchBtn.onClick.AddListener(() => AddDeviceAtSpawn(Network.DeviceType.Switch));

            Button pcBtn = UIComp.CreateMenuButton(panelObj.transform, "PCBtn", "PC",
                new Vector2(devSpacing, devBtnY), new Vector2(110, 48), font, 17);
            pcBtn.onClick.AddListener(() => AddDeviceAtSpawn(Network.DeviceType.PC));

            float advBtnY = -330f;
            float advSpacing = 130f;

            Button vlanBtn = UIComp.CreateMenuButton(panelObj.transform, "VLANBtn", "VLAN",
                new Vector2(-advSpacing, advBtnY), new Vector2(120, 48), font, 19);
            vlanBtn.onClick.AddListener(() => CreateNetworkAdvancedPanel("VLAN"));

            Button aclBtn = UIComp.CreateMenuButton(panelObj.transform, "ACLBtn", "ACL",
                new Vector2(0, advBtnY), new Vector2(120, 48), font, 19);
            aclBtn.onClick.AddListener(() => CreateNetworkAdvancedPanel("ACL"));

            Button natBtn = UIComp.CreateMenuButton(panelObj.transform, "NATBtn", "NAT",
                new Vector2(advSpacing, advBtnY), new Vector2(110, 42), font, 17);
            natBtn.onClick.AddListener(() => CreateNetworkAdvancedPanel("NAT"));

            // Create DevicesPanel and ScorePanel via controllers
            var devicePanelCtrl = UnityEngine.Object.FindAnyObjectByType<DevicePanelController>();
            if (devicePanelCtrl != null) devicePanelCtrl.RefreshDevicesPanel();

            UIPanelFactory.CreateScorePanel(ct);

            UnityEngine.Debug.Log("[ActivityLoader] Panel de topologia creado");
        }

        /// <summary>
        /// Crea el panel de configuracion avanzada de red (VLAN, ACL o NAT) delegando en
        /// ConfigPanelFactory segun el tipo solicitado.
        /// </summary>
        /// <param name="type">Tipo de panel: "VLAN", "ACL" o "NAT".</param>
        public void CreateNetworkAdvancedPanel(string type)
        {
            var topo = UnityEngine.Object.FindAnyObjectByType<TopologyManager>();
            if (topo == null) return;

            if (owner.canvas == null) owner.canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (owner.canvas == null) return;

            switch (type)
            {
                case "VLAN": ConfigPanelFactory.CreateVLANPanel(owner.canvas.transform, topo); break;
                case "ACL": ConfigPanelFactory.CreateACLPanel(owner.canvas.transform, topo); break;
                case "NAT": ConfigPanelFactory.CreateNATPanel(owner.canvas.transform, topo); break;
            }
        }

        /// <summary>
        /// Agrega un dispositivo en la siguiente posicion predefinida.
        /// Alternativa a los atajos de teclado (1/2/3) para uso sin teclado fisico.
        /// </summary>
        /// <param name="type">Tipo de dispositivo a agregar.</param>
        public void AddDeviceAtSpawn(Network.DeviceType type)
        {
            // Verificar si es una llamada duplicada (EventSystem puede procesar el clic dos veces
            // con activeInputHandler=Both en el Editor). Skip si el ultimo dispositivo se creo
            // hace menos de 100ms con el mismo tipo.
            float now = Time.unscaledTime;
            if (now - lastAddTime < 0.1f && lastAddType == type)
            {
                UnityEngine.Debug.Log($"[ActivityLoader] Llamada duplicada ignorada: {type}");
                return;
            }
            lastAddTime = now;
            lastAddType = type;

            // Usar TopologyManager.Instance (singleton siempre fresco) en vez de campo cacheado
            var tm = TopologyManager.Instance;
            if (tm == null)
            {
                var gm = GameObject.Find("GameManager");
                if (gm == null) gm = new GameObject("GameManager");
                tm = gm.AddComponent<TopologyManager>();
                // Asegurar que DevicePanelController exista (se destruye con GameManager al limpiar)
                if (gm.GetComponent<DevicePanelController>() == null)
                    gm.AddComponent<DevicePanelController>();
            }
            else
            {
                // Si TopologyManager ya existe pero DevicePanelController no (ej: tras limpiar),
                // crearlo en el GameManager
                var gm = GameObject.Find("GameManager");
                if (gm != null && gm.GetComponent<DevicePanelController>() == null)
                    gm.AddComponent<DevicePanelController>();
            }
            Vector2 pos = spawnPositions[spawnIndex % spawnPositions.Length];
            spawnIndex++;
            int discId = spawnIndex + 200; // ID unico (sobre 100 usado por TangibleDiscManager)
            tm.AddNode(discId, type, pos);
            var devicePanel = UnityEngine.Object.FindAnyObjectByType<DevicePanelController>();
            if (devicePanel != null) devicePanel.RefreshDevicesPanel();
            UnityEngine.Debug.Log($"[ActivityLoader] Dispositivo agregado: {type} en {pos}, discId={discId}");
        }
    }
}
