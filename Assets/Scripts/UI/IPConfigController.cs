using UnityEngine;
using UnityEngine.UI;
using SimRedes.Network;
using UIColors = SimRedes.UI.UIComponents.Colors;

namespace SimRedes.UI
{
    public class IPConfigController : MonoBehaviour
    {
        private int currentIPConfigNodeDiscId = -1;
        private GameObject ipConfigBackground;

        private TopologyManager topology;
        private NodeVisualizer visualizer;

        private void Start()
        {
            topology = Object.FindAnyObjectByType<TopologyManager>();
            visualizer = Object.FindAnyObjectByType<NodeVisualizer>();
        }

        public bool IsIPConfigPanelOpen()
        {
            return currentIPConfigNodeDiscId != -1;
        }

        public int GetCurrentIPConfigNodeDiscId()
        {
            return currentIPConfigNodeDiscId;
        }

        public void ShowIPConfigPanel(NetworkNode node, int discId)
        {
            var existingPanel = GameObject.Find("IPConfigPanel");
            if (existingPanel != null)
            {
                // B4: liberar los Sprite del panel previo antes de destruirlo
                UIComponents.SafeDestroyPanelSprites(existingPanel);
                Destroy(existingPanel);
            }

            var canvasTransform = Object.FindAnyObjectByType<Canvas>()?.transform;
            if (canvasTransform == null) return;

            CloseIPConfigPanelPublic();

            GameObject bgObj = new GameObject("IPConfigBackground");
            bgObj.transform.SetParent(canvasTransform, false);
            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            var bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.3f);

            var bgBtn = bgObj.AddComponent<Button>();
            bgBtn.onClick.AddListener(() => {
                CloseIPConfigPanelPublic();
            });

            ipConfigBackground = bgObj;
            currentIPConfigNodeDiscId = discId;

            bool showAdvanced = node.Type == Network.DeviceType.Router;

            ConfigPanelFactory.CreateIPConfigPanel(
                canvasTransform,
                node.Name,
                node.IpAddress ?? "",
                node.SubnetMask ?? "",
                onIPChanged: (val) => {
                    node.IpAddress = val;
                    UpdateNodeVisualIP(discId, val);
                },
                onMaskChanged: (val) => {
                    node.SubnetMask = val;
                },
                onApply: () => {
                    Debug.Log($"[IPConfig] {node.Name}: IP={node.IpAddress}, Mask={node.SubnetMask}");
                    CloseIPConfigPanelPublic();
                },
                onARP: showAdvanced ? (System.Action)(() => {
                    ShowARPPanel(node);
                }) : null,
                onRouting: showAdvanced ? (System.Action)(() => {
                    ShowRoutingPanel(node);
                }) : null,
                onCancel: () => {
                    CloseIPConfigPanelPublic();
                }
            );

            Debug.Log($"[IPConfigController] Panel IP abierto para {node.Name}");
        }

        public void CloseIPConfigPanel()
        {
            CloseIPConfigPanelPublic();
        }

        public void CloseIPConfigPanelPublic()
        {
            if (ipConfigBackground != null)
            {
                // B4: liberar los Sprite del fondo/panel antes de destruirlos
                UIComponents.SafeDestroyPanelSprites(ipConfigBackground);
                Destroy(ipConfigBackground);
                ipConfigBackground = null;
            }
            var panel = GameObject.Find("IPConfigPanel");
            if (panel != null)
            {
                UIComponents.SafeDestroyPanelSprites(panel);
                Destroy(panel);
            }
            currentIPConfigNodeDiscId = -1;
        }

        private void UpdateNodeVisualIP(int discId, string ip)
        {
            if (topology == null) topology = Object.FindAnyObjectByType<TopologyManager>();
            if (topology == null) return;

            var node = topology.GetNode(discId);
            if (node == null) return;

            bool isValid = string.IsNullOrEmpty(ip) || IPValidation.IsValidIP(ip);

            if (visualizer == null) visualizer = Object.FindAnyObjectByType<NodeVisualizer>();
            if (visualizer != null)
            {
                string label;
                if (!isValid)
                {
                    label = $"{node.Name}\n[IP invalida]";
                }
                else if (!string.IsNullOrEmpty(ip))
                {
                    label = $"{node.Name}\n{ip}";
                }
                else
                {
                    label = node.Name;
                }
                visualizer.UpdateNodeLabel(discId, label);
            }
        }

        private void ShowARPPanel(NetworkNode node)
        {
            var existingPanel = GameObject.Find("ARPPanel");
            if (existingPanel != null)
            {
                // B4: liberar los Sprite del panel previo antes de destruirlo
                UIComponents.SafeDestroyPanelSprites(existingPanel);
                Destroy(existingPanel);
            }

            var t = Object.FindAnyObjectByType<Canvas>()?.transform;
            if (t == null) return;

            ConfigPanelFactory.CreateARPPanel(t, node);
        }

        private void ShowRoutingPanel(NetworkNode node)
        {
            var existingPanel = GameObject.Find("RoutingPanel");
            if (existingPanel != null)
            {
                // B4: liberar los Sprite del panel previo antes de destruirlo
                UIComponents.SafeDestroyPanelSprites(existingPanel);
                Destroy(existingPanel);
            }

            var canvas = Object.FindAnyObjectByType<Canvas>();
            var canvasTransform = canvas?.transform;
            if (canvasTransform == null) return;

            ConfigPanelFactory.CreateRoutingPanel(
                canvasTransform, node,
                onDeleteRoute: (idx) => DeleteRouteFromNode(node, idx),
                onAddRoute: (idx) => ShowAddRoutePanel(node, idx),
                onClose: null
            );
        }

        private void DeleteRouteFromNode(NetworkNode node, int index)
        {
            var routes = node.RoutingTable.GetAllEntries();
            if (index < routes.Count)
            {
                node.RoutingTable.Clear();
                for (int j = 0; j < routes.Count; j++)
                {
                    if (j != index)
                    {
                        var r = routes[j];
                        node.RoutingTable.AddStaticRoute(r.DestinationNetwork, r.SubnetMask, r.NextHop, r.OutInterface);
                    }
                }
            }
            ShowRoutingPanel(node);
        }

        private void ShowAddRoutePanel(NetworkNode node, int insertIndex)
        {
            var existingPanel = GameObject.Find("AddRoutePanel");
            if (existingPanel != null)
            {
                // B4: liberar los Sprite del panel previo antes de destruirlo
                UIComponents.SafeDestroyPanelSprites(existingPanel);
                Destroy(existingPanel);
            }

            var canvas = Object.FindAnyObjectByType<Canvas>();
            var canvasTransform = canvas?.transform;
            if (canvasTransform == null) return;

            ConfigPanelFactory.CreateAddRoutePanel(
                canvasTransform, node,
                onAddRoute: (dest, mask, nextHop, iface) => {
                    if (IPValidation.IsValidIP(dest) && IPValidation.IsValidSubnetMask(mask) && IPValidation.IsValidIP(nextHop))
                    {
                        node.RoutingTable.AddStaticRoute(dest, mask, nextHop, iface);
                        var p = GameObject.Find("AddRoutePanel");
                        if (p != null) Destroy(p);
                        ShowRoutingPanel(node);
                        Debug.Log($"[Routing] Ruta anadida: {dest}/{IPValidation.GetPrefixLength(mask)} -> {nextHop} via {iface}");
                    }
                    else
                    {
                        Debug.LogWarning("[Routing] Datos invalidos para ruta");
                    }
                },
                onCancel: () => {
                    var p = GameObject.Find("AddRoutePanel");
                    if (p != null) Destroy(p);
                    ShowRoutingPanel(node);
                }
            );
        }
    }
}
