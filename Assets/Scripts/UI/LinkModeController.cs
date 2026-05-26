using UnityEngine;
using UnityEngine.UI;
using SimRedes.Network;

namespace SimRedes.UI
{
    public class LinkModeController : MonoBehaviour
    {
        private string currentLinkMode = null;
        private int linkModeFirstNode = -1;
        private Button connectBtn;
        private Button disconnectBtn;
        private TopologyManager topology;

        private readonly Color activeButtonColor = new Color(0.2f, 0.7f, 0.3f, 1f);
        private readonly Color normalButtonColor = UIComponents.Colors.buttonNormal;

        private void Start()
        {
            topology = Object.FindAnyObjectByType<TopologyManager>();
        }

        public void StoreLinkButtons(Button connect, Button disconnect)
        {
            connectBtn = connect;
            disconnectBtn = disconnect;
        }

        public void ToggleLinkMode(string mode)
        {
            if (topology == null) topology = Object.FindAnyObjectByType<TopologyManager>();
            if (topology == null) return;

            if (currentLinkMode == mode)
            {
                currentLinkMode = null;
                linkModeFirstNode = -1;
                UpdateLinkButtonColors();
                Debug.Log("[LinkMode] Modo desactivado");
                return;
            }

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

        public void HandleNodeLinkClick(int discId)
        {
            if (topology == null) topology = Object.FindAnyObjectByType<TopologyManager>();
            if (topology == null) return;

            if (currentLinkMode == "connect")
            {
                if (linkModeFirstNode == -1)
                {
                    linkModeFirstNode = discId;
                    Debug.Log($"[LinkMode] Primer nodo seleccionado: {discId}");
                }
                else if (linkModeFirstNode != discId)
                {
                    topology.AddLink(linkModeFirstNode, discId);
                    Debug.Log($"[LinkMode] Enlace creado entre {linkModeFirstNode} y {discId}");
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
                    Debug.Log($"[LinkMode] Primer nodo para desconectar: {discId}");
                }
                else
                {
                    topology.RemoveLink(linkModeFirstNode, discId);
                    Debug.Log($"[LinkMode] Enlace eliminado entre {linkModeFirstNode} y {discId}");
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

        private void UpdateLinkButtonColors()
        {
            if (connectBtn != null)
            {
                var img = connectBtn.GetComponent<Image>();
                if (img != null)
                {
                    img.color = currentLinkMode == "connect" ? activeButtonColor : normalButtonColor;
                }
            }
            if (disconnectBtn != null)
            {
                var img = disconnectBtn.GetComponent<Image>();
                if (img != null)
                {
                    img.color = currentLinkMode == "disconnect" ? activeButtonColor : normalButtonColor;
                }
            }
        }
    }
}
