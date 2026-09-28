using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using SimRedes.Network;
using SimRedes.UI;

namespace SimRedes.Simulation
{
    /// <summary>
    /// Actividad de configuracion de rutas estaticas en routers. Permite anadir rutas manuales o de ejemplo y probar el enrutamiento.
    /// </summary>
    public class StaticRoutingActivity : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] public Text infoText;
        [SerializeField] public Text routesText;
        [SerializeField] public Text feedbackText;

        private TopologyManager topologyManager;
        private NetworkNode selectedRouter;
        private Canvas canvasRef;

        /// <summary>
        /// Inicializa la actividad: resuelve TopologyManager via
        /// <see cref="ActivityStartup.ResolveTopologyManager"/> (sin crear
        /// singletons fantasma), obtiene referencia al Canvas y actualiza la UI.
        /// </summary>
        private void Start()
        {
            topologyManager = ActivityStartup.ResolveTopologyManager();
            canvasRef = Object.FindAnyObjectByType<Canvas>();
            UpdateUI();
        }

        /// <summary>
        /// Escucha las teclas A (anadir ruta de ejemplo) y T (probar enrutamiento) mediante el nuevo Input System.
        /// </summary>
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

        /// <summary>
        /// Anade una ruta estatica al router seleccionado. Si no hay router seleccionado, toma el primero disponible.
        /// </summary>
        /// <param name="destNetwork">Red destino en formato IPv4 (ej: "10.0.0.0").</param>
        /// <param name="mask">Mascara de subred en formato IPv4 (ej: "255.0.0.0").</param>
        /// <param name="nextHop">Direccion IPv4 del siguiente salto.</param>
        /// <param name="outInterface">Nombre de la interfaz de salida (por defecto "G0/0").</param>
        public void AddRoute(string destNetwork, string mask, string nextHop, string outInterface = "G0/0")
        {
            if (selectedRouter == null)
            {
                var routers = GetRouters();
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

        /// <summary>
        /// Muestra el panel de configuracion para anadir una ruta personalizada mediante ConfigPanelFactory.
        /// </summary>
        public void ShowAddRoutePanel()
        {
            if (canvasRef == null)
                canvasRef = Object.FindAnyObjectByType<Canvas>();

            if (canvasRef == null)
            {
                ShowFeedback("Error: No se encontró el Canvas");
                return;
            }

            var routers = GetRouters();
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

        /// <summary>
        /// Anade una ruta estatica de ejemplo con red destino aleatoria 10.x.0.0/8 al router seleccionado.
        /// </summary>
        public void AddSampleRoute()
        {
            if (selectedRouter == null)
            {
                var routers2 = GetRouters();
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

        /// <summary>
        /// Prueba el enrutamiento simulando el reenvio de un paquete hacia una IP aleatoria 10.x.x.1.
        /// </summary>
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

        /// <summary>
        /// Actualiza el texto de informacion y la lista de rutas mostradas en pantalla.
        /// </summary>
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

        /// <summary>
        /// Construye y asigna el texto con todas las rutas estaticas de todos los routers en la topologia.
        /// </summary>
        private void UpdateRoutesDisplay()
        {
            if (routesText == null) return;

            string content = "=== RUTAS ESTÁTICAS ===\n\n";

            var routers = GetRouters();
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

        /// <summary>
        /// Retorna los routers actuales de la topologia. Si todavia no hay
        /// TopologyManager en la escena, retorna una lista vacia en vez de fallar.
        /// </summary>
        /// <returns>Lista con los nodos de tipo Router (vacia si no hay manager).</returns>
        private List<NetworkNode> GetRouters()
        {
            if (topologyManager == null) return new List<NetworkNode>();
            return topologyManager.GetAllNodes().FindAll(n => n.Type == SimRedes.Network.DeviceType.Router);
        }

        /// <summary>
        /// Muestra un mensaje de retroalimentacion en el texto feedbackText y lo registra en la consola.
        /// </summary>
        /// <param name="message">Mensaje a mostrar al usuario.</param>
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