using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimRedes.Network;

namespace SimRedes.UI
{
    public class ConnectivityTestPanel : MonoBehaviour
    {
        [Header("UI References")]
        public Text sourceNodeText;
        public Text destNodeText;
        public Text resultText;
        public Text resultIconText;
        public Button pingButton;
        public Text statusText;

        [Header("Colors")]
        public Color successColor = new Color(0.2f, 0.8f, 0.2f);
        public Color failColor = new Color(0.8f, 0.2f, 0.2f);
        public Color waitingColor = new Color(0.5f, 0.5f, 0.5f);

        private NetworkNode sourceNode;
        private NetworkNode destNode;
        private bool lastResult = false;
        private int pingCount = 0;
        private int successCount = 0;

        // B6: refresco acotado del panel (~4 veces por segundo). Sin esto,
        // Update buscaba TopologyManager, copiaba la lista de nodos y
        // reasignaba los textos en CADA frame generando churn de GC.
        private const float REFRESH_INTERVAL = 0.25f;
        private float refreshTimer = REFRESH_INTERVAL;

        public void Initialize(Text source, Text dest, Text result, Text icon, Button pingBtn, Text statusTxt)
        {
            sourceNodeText = source;
            destNodeText = dest;
            resultText = result;
            resultIconText = icon;
            pingButton = pingBtn;
            statusText = statusTxt;

            FindNodes();
            SetupButton();
            UpdateNodeDisplay();
            UpdateStatus();
        }

        void Start()
        {
            if (sourceNodeText == null)
            {
                FindNodes();
                SetupButton();
                UpdateNodeDisplay();
                UpdateStatus();
            }
        }

        void Update()
        {
            refreshTimer += Time.deltaTime;
            if (refreshTimer < REFRESH_INTERVAL) return;
            refreshTimer = 0f;

            FindNodes();
            UpdateNodeDisplay();
        }

        private void FindNodes()
        {
            var topology = Object.FindAnyObjectByType<TopologyManager>();
            if (topology == null) return;

            var nodes = topology.GetAllNodes();
            sourceNode = null;
            destNode = null;

            List<NetworkNode> pcNodes = new List<NetworkNode>();
            List<NetworkNode> routerNodes = new List<NetworkNode>();

            foreach (var node in nodes)
            {
                if (node.Type == SimRedes.Network.DeviceType.PC)
                    pcNodes.Add(node);
                else if (node.Type == SimRedes.Network.DeviceType.Router)
                    routerNodes.Add(node);
            }

            if (pcNodes.Count >= 2)
            {
                sourceNode = pcNodes[0];
                destNode = pcNodes[1];
            }
            else if (pcNodes.Count == 1 && routerNodes.Count >= 1)
            {
                sourceNode = pcNodes[0];
                destNode = routerNodes[0];
            }
            else if (routerNodes.Count >= 2)
            {
                sourceNode = routerNodes[0];
                destNode = routerNodes[1];
            }
            else if (nodes.Count >= 2)
            {
                sourceNode = nodes[0];
                destNode = nodes[1];
            }
        }

        private void SetupButton()
        {
            if (pingButton != null)
            {
                pingButton.onClick.RemoveAllListeners();
                pingButton.onClick.AddListener(ExecutePing);
            }
        }

        private void ExecutePing()
        {
            var topology = Object.FindAnyObjectByType<TopologyManager>();
            if (topology == null)
            {
                ShowResult(false, "No hay topología");
                return;
            }

            if (sourceNode == null || destNode == null)
            {
                ShowResult(false, "Se necesitan 2 nodos");
                return;
            }

            StartCoroutine(AnimatePing());
        }

        private IEnumerator AnimatePing()
        {
            ShowResultWaiting();
            yield return new WaitForSeconds(0.5f);

            var pingVis = Object.FindAnyObjectByType<PingVisualizer>();
            if (pingVis != null)
            {
                bool done = false;
                bool result = false;
                pingVis.AnimatePing(sourceNode.DiscId, destNode.DiscId, (success) =>
                {
                    result = success;
                    done = true;
                });
                yield return new WaitUntil(() => done);

                lastResult = result;
                pingCount++;
                if (result)
                {
                    successCount++;
                    ShowResult(true, $"Ping exitoso: {sourceNode.Name} -> {destNode.Name}");
                }
                else
                {
                    ShowResult(false, $"Ping fallido: {sourceNode.Name} -> {destNode.Name}");
                }
                UpdateStatus();
            }
            else
            {
                var topology = Object.FindAnyObjectByType<TopologyManager>();
                bool connected = topology.CheckConnectivity(sourceNode.DiscId, destNode.DiscId);
                lastResult = connected;
                pingCount++;

                if (connected)
                {
                    successCount++;
                    ShowResult(true, $"Ping exitoso: {sourceNode.Name} -> {destNode.Name}");
                }
                else
                {
                    ShowResult(false, $"Ping fallido: {sourceNode.Name} -> {destNode.Name}");
                }

                UpdateStatus();
            }
        }

        private void ShowResult(bool success, string message)
        {
            if (resultText != null)
            {
                resultText.text = message;
                resultText.color = success ? successColor : failColor;
            }

            if (resultIconText != null)
            {
                resultIconText.text = success ? "OK" : "X";
                resultIconText.color = success ? successColor : failColor;
            }

            UnityEngine.Debug.Log($"[Ping] {message}");
        }

        private void ShowResultWaiting()
        {
            if (resultText != null)
            {
                resultText.text = "Enviando ping...";
                resultText.color = waitingColor;
            }

            if (resultIconText != null)
            {
                resultIconText.text = "?";
                resultIconText.color = waitingColor;
            }
        }

        private void UpdateNodeDisplay()
        {
            if (sourceNodeText != null)
            {
                sourceNodeText.text = sourceNode != null
                    ? $"{GetNodeIcon(sourceNode.Type)} {sourceNode.Name}"
                    : $"{GetNodeIcon(SimRedes.Network.DeviceType.Unknown)} -";
            }

            if (destNodeText != null)
            {
                destNodeText.text = destNode != null
                    ? $"{GetNodeIcon(destNode.Type)} {destNode.Name}"
                    : $"{GetNodeIcon(SimRedes.Network.DeviceType.Unknown)} -";
            }
        }

        private void UpdateStatus()
        {
            if (statusText != null)
            {
                statusText.text = $"Pings: {pingCount} | Exitosos: {successCount} | Fallidos: {pingCount - successCount}";
            }
        }

        private string GetNodeIcon(SimRedes.Network.DeviceType type)
        {
            switch (type)
            {
                case SimRedes.Network.DeviceType.Router: return "📡";
                case SimRedes.Network.DeviceType.Switch: return "🔌";
                case SimRedes.Network.DeviceType.PC: return "💻";
                default: return "❓";
            }
        }

        public void ResetStats()
        {
            pingCount = 0;
            successCount = 0;
            lastResult = false;
            UpdateStatus();

            if (resultText != null)
            {
                resultText.text = "Presiona PING para probar conectividad";
                resultText.color = waitingColor;
            }

            if (resultIconText != null)
            {
                resultIconText.text = "?";
                resultIconText.color = waitingColor;
            }
        }
    }
}