using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using SimRedes.Network;

namespace SimRedes.Simulation
{
    /// <summary>
    /// Actividad que muestra las tablas de enrutamiento de todos los routers en la topologia actual.
    /// Permite refrescar las tablas y visualizar rutas por defecto y estaticas de ejemplo.
    /// </summary>
    public class RoutingTablesActivity : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private GameObject tablePanel;
        [SerializeField] public Text infoText;
        [SerializeField] public Text tableText;

        private TopologyManager topologyManager;
        private List<NetworkNode> routers = new List<NetworkNode>();

        /// <summary>
        /// Inicializa la actividad: resuelve TopologyManager via
        /// <see cref="ActivityStartup.ResolveTopologyManager"/> (sin crear
        /// singletons fantasma) y actualiza la UI inicial.
        /// </summary>
        private void Start()
        {
            topologyManager = ActivityStartup.ResolveTopologyManager();
            UpdateUI();
        }

        /// <summary>
        /// Escucha la tecla R para refrescar las tablas de enrutamiento si el texto de tabla esta visible.
        /// </summary>
        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame
                && tableText != null && tableText.gameObject.activeInHierarchy)
            {
                RefreshRoutingTables();
            }
        }

        /// <summary>
        /// Refresca la lista de routers desde TopologyManager, genera rutas de ejemplo para los que no tengan y actualiza la visualizacion.
        /// </summary>
        public void RefreshRoutingTables()
        {
            routers.Clear();

            // Null-safe: sin TopologyManager (aun no existe en escena) se trata como lista vacia
            var allNodes = topologyManager != null
                ? topologyManager.GetAllNodes()
                : new List<NetworkNode>();
            foreach (var node in allNodes)
            {
                if (node.Type == SimRedes.Network.DeviceType.Router)
                {
                    routers.Add(node);
                    GenerateSampleRoutes(node);
                }
            }

            UpdateTableDisplay();
            UnityEngine.Debug.Log("[RoutingTables] Tablas actualizadas. Routers: " + routers.Count);
        }

        /// <summary>
        /// Agrega rutas de ejemplo al router solo si su tabla de enrutamiento esta vacia (ruta por defecto, 10.0.0.0/8 y 172.16.0.0/12).
        /// </summary>
        /// <param name="router">Router al que se le agregaran las rutas de ejemplo.</param>
        private void GenerateSampleRoutes(NetworkNode router)
        {
            if (router.RoutingTable.GetAllEntries().Count == 0)
            {
                router.RoutingTable.AddStaticRoute("0.0.0.0", "0.0.0.0", "192.168.1.254", "G0/0");
                router.RoutingTable.AddStaticRoute("10.0.0.0", "255.0.0.0", "10.1.1.1", "G0/1");
                router.RoutingTable.AddStaticRoute("172.16.0.0", "255.240.0.0", "172.16.0.1", "G0/2");
            }
        }

        /// <summary>
        /// Actualiza el texto de informacion y la visualizacion de las tablas de enrutamiento.
        /// </summary>
        private void UpdateUI()
        {
            if (infoText != null)
            {
                infoText.text = "TABLAS DE ENRUTAMIENTO\n\n" +
                    "Esta actividad muestra las tablas de\n" +
                    "enrutamiento de los routers en la\n" +
                    "topolog\u00eda actual.\n\n" +
                    "Presiona ACTUALIZAR para refrescar\n" +
                    "las tablas de todos los routers.\n\n" +
                    "Coloca routers (tecla 1 o discos)\n" +
                    "para ver sus tablas de enrutamiento.";
            }

            UpdateTableDisplay();
        }

        /// <summary>
        /// Construye y asigna el texto formateado con todas las tablas de enrutamiento de los routers detectados.
        /// </summary>
        private void UpdateTableDisplay()
        {
            if (tableText == null) return;

            if (routers.Count == 0)
            {
                tableText.text = "No hay routers en la topología.\n\nColoca routers usando la tecla 1.";
                return;
            }

            string content = "=== TABLAS DE ENRUTAMIENTO ===\n\n";

            foreach (var router in routers)
            {
                content += $"Router {router.DiscId} ({router.Name}):\n";
                content += "------------------------------------------------------------\n";
                content += "Red Destino/Prefijo | Siguiente Salto | Interfaz | Métrica | Protocolo\n";
                content += "------------------------------------------------------------\n";

                var entries = router.RoutingTable.GetAllEntries();
                if (entries.Count == 0)
                {
                    content += "Sin rutas configuradas\n";
                }
                else
                {
                    foreach (var entry in entries)
                    {
                        content += $"{entry.DestinationNetwork}/{entry.GetPrefixLength(),-8} | {entry.NextHop,-14} | {entry.OutInterface,-7} | {entry.Metric,-7} | {entry.Protocol}\n";
                    }
                }

                content += "------------------------------------------------------------\n\n";
            }

            content += "\nACTUALIZAR = Refrescar tablas | ESC = Menú";

            tableText.text = content;
        }

        /// <summary>
        /// Retorna una copia de la lista interna de routers detectados.
        /// </summary>
        /// <returns>Nueva lista con los routers actuales.</returns>
        public List<NetworkNode> GetRouters()
        {
            return new List<NetworkNode>(routers);
        }
    }
}