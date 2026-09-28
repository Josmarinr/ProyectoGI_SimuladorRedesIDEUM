using UnityEngine;
using SimRedes.Network;

namespace SimRedes.UI
{
    public class NodeInteractionController : MonoBehaviour
    {
        private LinkModeController linkMode;
        private PingModeController pingMode;
        private IPConfigController ipConfig;
        private TopologyManager topology;

        private void Awake()
        {
            linkMode = LinkModeController.Instance;
            pingMode = Object.FindAnyObjectByType<PingModeController>();
            ipConfig = Object.FindAnyObjectByType<IPConfigController>();
            topology = Object.FindAnyObjectByType<TopologyManager>();
        }

        public bool IsLinkModeActive() => linkMode != null && linkMode.IsLinkModeActive();
        public bool IsPingModeActive() => pingMode != null && pingMode.IsPingModeActive();
        public bool IsIPConfigPanelOpen() => ipConfig != null && ipConfig.IsIPConfigPanelOpen();
        public int GetCurrentIPConfigNodeDiscId() => ipConfig != null ? ipConfig.GetCurrentIPConfigNodeDiscId() : -1;
        public void CloseIPConfigPanelPublic() { if (ipConfig != null) ipConfig.CloseIPConfigPanel(); }

        public void HandleNodeClick(int discId)
        {
            var node = topology?.GetNode(discId);
            if (node == null) return;

            if (IsLinkModeActive())
            {
                CloseIPConfigPanelPublic();
                linkMode.HandleNodeLinkClick(discId);
            }
            else if (IsPingModeActive())
            {
                CloseIPConfigPanelPublic();
                pingMode.HandlePingNodeClick(node);
            }
            else if (IsIPConfigPanelOpen() && discId != GetCurrentIPConfigNodeDiscId())
            {
                CloseIPConfigPanelPublic();
                if (ipConfig != null) ipConfig.ShowIPConfigPanel(node, discId);
            }
            else if (!IsIPConfigPanelOpen())
            {
                if (ipConfig != null) ipConfig.ShowIPConfigPanel(node, discId);
            }
        }

        public void ShowIPConfigPanel(NetworkNode node, int discId)
        {
            if (ipConfig != null) ipConfig.ShowIPConfigPanel(node, discId);
        }
    }
}
