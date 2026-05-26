using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using SimRedes.Network;
using SimRedes.UI;

namespace SimRedes.Simulation
{
    public class StaticRoutingActivity : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] public Text infoText;
        [SerializeField] public Text routesText;
        [SerializeField] public Text feedbackText;

        private TopologyManager topologyManager;
        private NetworkNode selectedRouter;
        private Canvas canvasRef;

        private void Start()
        {
            topologyManager = Object.FindAnyObjectByType<TopologyManager>();
            if (topologyManager == null)
            {
                var go = new GameObject("TopologyManager");
                topologyManager = go.AddComponent<TopologyManager>();
            }

            canvasRef = Object.FindAnyObjectByType<Canvas>();
            UpdateUI();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.aKey.wasPressedThisFrame)
            {
                AddSampleRoute();
            }
            else if (keyboard != null && keyboard.tKey.wasPressedThisFrame)
            {
                TestRouting();
            }
        }

        public void AddRoute(string destNetwork, string mask, string nextHop, string outInterface = "G0/0")
        {
            if (selectedRouter == null)
            {
                var routers = topologyManager.GetAllNodes().FindAll(n => n.Type == SimRedes.Network.DeviceType.Router);
                if (routers.Count > 0)
                    selectedRouter = routers[0];
            }

            if (selectedRouter != null)
            {
                selectedRouter.RoutingTable.AddStaticRoute(destNetwork, mask, nextHop, outInterface);
                ShowFeedback($"Ruta añadida: {destNetwork}/{IPValidation.GetPrefixLength(mask)} -> {nextHop} via {outInterface}");
                UpdateRoutesDisplay();
            }
        }

        public void ShowAddRoutePanel()
        {
            if (canvasRef == null)
                canvasRef = Object.FindAnyObjectByType<Canvas>();

            if (canvasRef == null)
            {
                ShowFeedback("Error: No se encontró el Canvas");
                return;
            }

            var routers = topologyManager.GetAllNodes().FindAll(n => n.Type == SimRedes.Network.DeviceType.Router);
            if (routers.Count == 0)
            {
                ShowFeedback("Añade un router primero (tecla 1)");
                return;
            }

            if (selectedRouter == null)
                selectedRouter = routers[0];

            ConfigPanelFactory.CreateAddRoutePanel(
                canvasRef.transform,
                selectedRouter,
                (dest, mask, nextHop, iface) => AddRoute(dest, mask, nextHop, iface),
                () => ShowFeedback("Operación cancelada")
            );

            ShowFeedback($"Configurando ruta para Router {selectedRouter.DiscId}");
        }

        public void AddSampleRoute()
        {
            if (selectedRouter == null)
            {
                var routers2 = topologyManager.GetAllNodes().FindAll(n => n.Type == SimRedes.Network.DeviceType.Router);
                if (routers2.Count > 0)
                    selectedRouter = routers2[0];
            }

            if (selectedRouter != null)
            {
                string dest = $"10.{Random.Range(1, 255)}.0.0";
                string mask = "255.0.0.0";
                string nextHop = $"192.168.{selectedRouter.DiscId}.254";
                selectedRouter.RoutingTable.AddStaticRoute(dest, mask, nextHop, "G0/0");
                ShowFeedback($"Ruta estática añadida: {dest}/8 -> {nextHop}");
                UpdateRoutesDisplay();
            }
            else
            {
                ShowFeedback("Añade un router primero (tecla 1)");
            }
        }

        public void TestRouting()
        {
            if (selectedRouter == null)
            {
                ShowFeedback("Selecciona un router primero");
                return;
            }

            string testIP = $"10.{Random.Range(1, 255)}.{Random.Range(1, 255)}.1";
            bool found = RoutingSimulator.SimulatePacketForwarding(selectedRouter, testIP);

            if (found)
            {
                ShowFeedback($"Tráfico hacia {testIP} enrutado correctamente");
            }
            else
            {
                ShowFeedback($"No hay ruta hacia {testIP}");
            }
        }

        private void UpdateUI()
        {
            if (infoText != null)
            {
                infoText.text = "ENRUTAMIENTO ESTÁTICO\n\n" +
                    "Configura rutas estáticas en los\n" +
                    "routers para dirigir el tráfico.\n\n" +
                    "BOTONES:\n" +
                    "  AÑADIR RUTA = Ruta de ejemplo\n" +
                    "  MANUAL = Ruta personalizada\n" +
                    "  TEST = Probar enrutamiento\n\n" +
                    "Las rutas se muestran a la derecha.\n" +
                    "Presiona ESC para volver.";
            }

            UpdateRoutesDisplay();
        }

        private void UpdateRoutesDisplay()
        {
            if (routesText == null) return;

            string content = "=== RUTAS ESTÁTICAS ===\n\n";

            var routers = topologyManager.GetAllNodes().FindAll(n => n.Type == SimRedes.Network.DeviceType.Router);
            if (routers.Count == 0)
            {
                content += "No hay routers configurados.\n" +
                           "Usa la tecla 1 para añadir routers.";
            }
            else
            {
                foreach (var router in routers)
                {
                    if (selectedRouter == null) selectedRouter = router;

                    content += $"Router {router.DiscId} ({router.Name}):\n";
                    content += "-----------------------------\n";

                    var entries = router.RoutingTable.GetAllEntries();
                    if (entries.Count == 0)
                    {
                        content += "Sin rutas configuradas\n";
                    }
                    else
                    {
                        foreach (var entry in entries)
                        {
                            content += $"{entry.DestinationNetwork}/{entry.GetPrefixLength()} -> {entry.NextHop} via {entry.OutInterface} [Metrica: {entry.Metric}] ({entry.Protocol})\n";
                        }
                    }

                    content += "-----------------------------\n\n";
                }
            }

            content += "\nAÑADIR RUTA / TEST = Botones | ESC = Menú";

            routesText.text = content;
        }

        private void ShowFeedback(string message)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
                UnityEngine.Debug.Log("[StaticRouting] " + message);
            }
        }

    }
}