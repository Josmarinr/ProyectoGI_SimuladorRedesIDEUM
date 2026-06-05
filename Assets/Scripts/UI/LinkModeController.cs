using UnityEngine;
using UnityEngine.UI;
using SimRedes.Network;
using SimRedes.UI;

namespace SimRedes.UI
{
    public class LinkModeController : MonoBehaviour
    {
        /// <summary>Instancia unica del LinkModeController. Usar esta en vez de FindAnyObjectByType.</summary>
        public static LinkModeController Instance { get; private set; }

        private string currentLinkMode = null;
        private int linkModeFirstNode = -1;
        private Button connectBtn;
        private Button disconnectBtn;

        private readonly Color activeButtonColor = new Color(0.2f, 0.7f, 0.3f, 1f);
        private readonly Color normalButtonColor = UIComponents.Colors.buttonNormal;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // Ya hay una instancia vieja (de sesion anterior con Destroy diferido).
                // Destruir la VIEJA y quedarse con esta (la nueva).
                var oldGameObj = Instance.gameObject;
                Instance = this;
                Destroy(oldGameObj);
            }
            else
            {
                Instance = this;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void StoreLinkButtons(Button connect, Button disconnect)
        {
            connectBtn = connect;
            disconnectBtn = disconnect;
        }

        private float lastToggleTime = 0f;

        public void ToggleLinkMode(string mode)
        {
            // Ignorar eventos duplicados dentro de 150ms (TouchScript + InputSystem = doble clic)
            if (Time.unscaledTime - lastToggleTime < 0.15f)
                return;
            lastToggleTime = Time.unscaledTime;

            if (TopologyManager.Instance == null) return;

            if (currentLinkMode == mode)
            {
                UnhighlightAll();
                currentLinkMode = null;
                linkModeFirstNode = -1;
                UpdateLinkButtonColors();
                Debug.Log("[LinkMode] Modo desactivado");
                return;
            }

            UnhighlightAll();
            currentLinkMode = mode;
            linkModeFirstNode = -1;
            UpdateLinkButtonColors();

            if (mode == "connect")
            {
                Debug.Log("[LinkMode] Modo CONECTAR activado - Selecciona el primer nodo");
            }
            else
            {
                Debug.Log("[LinkMode] Modo DESCONECTAR activado - Selecciona un enlace");
            }
        }

        public bool IsLinkModeActive()
        {
            return currentLinkMode != null;
        }

        public int GetLinkModeFirstNode()
        {
            return linkModeFirstNode;
        }

        public string GetActiveMode()
        {
            return currentLinkMode;
        }

        public void ClearFirstNode()
        {
            UnhighlightAll();
            linkModeFirstNode = -1;
        }

        private NodeVisualizer GetVisualizer()
        {
            return Object.FindAnyObjectByType<NodeVisualizer>();
        }

        private void HighlightFirstNode()
        {
            var vis = GetVisualizer();
            if (vis != null) vis.SelectNodeByDiscId(linkModeFirstNode);
        }

        private void UnhighlightAll()
        {
            var vis = GetVisualizer();
            if (vis != null) vis.SelectNodeByDiscId(-1);
        }

        public void HandleNodeLinkClick(int discId)
        {
            if (TopologyManager.Instance == null) return;

            if (currentLinkMode == "connect")
            {
                if (linkModeFirstNode == -1)
                {
                    linkModeFirstNode = discId;
                    HighlightFirstNode();
                    Debug.Log($"[LinkMode] Primer nodo seleccionado: {discId}");
                }
                else if (linkModeFirstNode != discId)
                {
                    UnhighlightAll();
                    TopologyManager.Instance.AddLink(linkModeFirstNode, discId);
                    Debug.Log($"[LinkMode] Enlace creado entre {linkModeFirstNode} y {discId}");
                    // Forzar redibujado de enlaces visuales
                    var visualizer = GetVisualizer();
                    if (visualizer != null) visualizer.DrawLinks();
                    linkModeFirstNode = -1;
                    currentLinkMode = null;
                    UpdateLinkButtonColors();
                }
            }
            else if (currentLinkMode == "disconnect")
            {
                if (linkModeFirstNode == -1)
                {
                    linkModeFirstNode = discId;
                    HighlightFirstNode();
                    Debug.Log($"[LinkMode] Primer nodo para desconectar: {discId}");
                }
                else if (linkModeFirstNode != discId)
                {
                    UnhighlightAll();
                    TopologyManager.Instance.RemoveLink(linkModeFirstNode, discId);
                    Debug.Log($"[LinkMode] Enlace eliminado entre {linkModeFirstNode} y {discId}");
                    // Forzar redibujado de enlaces visuales
                    var visualizer = GetVisualizer();
                    if (visualizer != null) visualizer.DrawLinks();
                    linkModeFirstNode = -1;
                    currentLinkMode = null;
                    UpdateLinkButtonColors();
                }
            }
        }

        public void ClearState()
        {
            linkModeFirstNode = -1;
        }

        private readonly Color disconnectActiveColor = new Color(0.8f, 0.2f, 0.2f, 1f);

        private void UpdateLinkButtonColors()
        {
            SetButtonColor(connectBtn, currentLinkMode == "connect", activeButtonColor);
            SetButtonColor(disconnectBtn, currentLinkMode == "disconnect", disconnectActiveColor);
        }

        private void SetButtonColor(Button btn, bool isActive, Color activeColor)
        {
            if (btn == null) return;
            // Desactivar transicion de color del Button para que no sobrescriba
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = Color.white;
            colors.disabledColor = Color.white;
            colors.colorMultiplier = 1f;
            btn.colors = colors;
            // Controlar el color via Image directamente
            var img = btn.GetComponent<Image>();
            if (img != null)
                img.color = isActive ? activeColor : normalButtonColor;
        }
    }
}
