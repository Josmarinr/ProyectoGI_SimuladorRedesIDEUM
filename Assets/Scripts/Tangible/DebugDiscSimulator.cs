using UnityEngine;
using UnityEngine.InputSystem;
using SimRedes.Network;
using SimRedes.UI;

namespace SimRedes.Tangible
{
    /// <summary>
    /// Simula la colocacion de discos fisicos mediante teclado para testing sin hardware IDEUM.
    /// Asigna teclas (1-5, C, P) para crear routers, switches, PCs, enlaces, fallos,
    /// limpiar la escena y probar conectividad.
    /// </summary>
    public class DebugDiscSimulator : MonoBehaviour
    {
        [Header("Simulation Settings")]
        [SerializeField] private bool enableSimulation = false;
        [SerializeField] private Key addRouterKey = Key.Digit1;
        [SerializeField] private Key addSwitchKey = Key.Digit2;
        [SerializeField] private Key addPCKey = Key.Digit3;
        [SerializeField] private Key addEnlaceKey = Key.Digit4;
        [SerializeField] private Key addFalloKey = Key.Digit5;
        [SerializeField] private Key addProtocoloKey = Key.Digit6;
        [SerializeField] private Key clearKey = Key.C;
        [SerializeField] private Key pingTestKey = Key.P;

        private const int BASE_DISC_ID = 100;

        private Vector2[] spawnPositions = new Vector2[]
        {
            new Vector2(100, 400),
            new Vector2(300, 350),
            new Vector2(200, 500),
            new Vector2(150, 600),
            new Vector2(250, 600),
            new Vector2(300, 250),
            new Vector2(100, 250),
            new Vector2(200, 700),
            new Vector2(300, 700)
        };
        private int positionIndex = 0;

        /// <summary>
        /// Escucha teclas de acceso rapido para simular discos, alternar modo conexion,
        /// limpiar escena y ejecutar pruebas de conectividad (solo si enableSimulation=true).
        /// </summary>
        private void Update()
        {
            if (!enableSimulation) return;

            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard[addRouterKey].wasPressedThisFrame)
                SimulateDiscAt(1, GetNextPosition());

            else if (keyboard[addSwitchKey].wasPressedThisFrame)
                SimulateDiscAt(2, GetNextPosition());

            else if (keyboard[addPCKey].wasPressedThisFrame)
                SimulateDiscAt(3, GetNextPosition());

            else if (keyboard[addEnlaceKey].wasPressedThisFrame)
            {
                var linkMode = Object.FindAnyObjectByType<LinkModeController>();
                if (linkMode != null)
                    linkMode.ToggleLinkMode("connect");
                else
                    UnityEngine.Debug.Log("[DebugSim] LinkModeController no encontrado, no se puede activar modo conexion");
            }

            else if (keyboard[addFalloKey].wasPressedThisFrame)
                SimulateDiscAt(5, GetNextPosition());

            else if (keyboard[addProtocoloKey].wasPressedThisFrame)
                SimulateDiscAt(6, GetNextPosition());

            else if (keyboard[clearKey].wasPressedThisFrame)
                ClearAllDiscs();

            else if (keyboard[pingTestKey].wasPressedThisFrame)
                TestConnectivity();
        }

        /// <summary>
        /// Retorna la siguiente posicion predefinida del arreglo spawnPositions,
        /// avanzando el indice ciclicamente.
        /// </summary>
        /// <returns>Vector2 con la siguiente posicion de aparicion.</returns>
        private Vector2 GetNextPosition()
        {
            Vector2 pos = spawnPositions[positionIndex % spawnPositions.Length];
            positionIndex++;
            return pos;
        }

        /// <summary>
        /// Coloca un disco simulado en la posicion dada a traves de TangibleDiscManager
        /// y actualiza el texto de estado en la UI.
        /// </summary>
        /// <param name="discId">Tipo de disco a simular (1-18).</param>
        /// <param name="position">Posicion en coordenadas Canvas.</param>
        private void SimulateDiscAt(int discId, Vector2 position)
        {
            var manager = Object.FindAnyObjectByType<TangibleDiscManager>();
            if (manager != null)
            {
                manager.SimulateDiscPlaced(discId, position);
                UnityEngine.Debug.Log($"[DebugSim] Disco simulado: {discId} en {position}");
                UpdateStatusText(discId);
            }
        }

        /// <summary>
        /// Busca un texto de UI llamado "StatusText" y lo actualiza con la etiqueta
        /// del disco recien colocado y el resumen de topologia.
        /// </summary>
        /// <param name="discId">Tipo de disco colocado para obtener su configuracion.</param>
        private void UpdateStatusText(int discId)
        {
            var config = DiscConfiguration.GetConfiguration(discId);
            var topology = Object.FindAnyObjectByType<Network.TopologyManager>();
            if (topology != null)
            {
                var statusTexts = FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None);
                foreach (var text in statusTexts)
                {
                    if (text.name == "StatusText")
                    {
                        text.text = $"Disco: {config.Label}\n{topology.GetTopologySummary()}";
                    }
                }
            }
        }

        /// <summary>
        /// Elimina todos los discos, nodos, enlaces y reinicia los indices de posicion.
        /// Operacion completa de limpieza de escena para testing.
        /// </summary>
        private void ClearAllDiscs()
        {
            var manager = Object.FindAnyObjectByType<TangibleDiscManager>();
            if (manager != null)
            {
                manager.ClearAllDiscs();
            }

            var topology = Object.FindAnyObjectByType<Network.TopologyManager>();
            if (topology != null)
            {
                topology.ClearTopology();
            }

            var visualizer = Object.FindAnyObjectByType<NodeVisualizer>();
            if (visualizer != null)
            {
                var nodeContainer = visualizer.nodeContainer;
                if (nodeContainer != null)
                {
                    foreach (Transform child in nodeContainer)
                    {
                        Destroy(child.gameObject);
                    }
                }

                var linkContainer = visualizer.linkContainer;
                if (linkContainer != null)
                {
                    foreach (Transform child in linkContainer)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }

            positionIndex = 0;
            UnityEngine.Debug.Log("[DebugSim] Todo limpiado");
        }

        /// <summary>
        /// Metodo publico para forzar la limpieza completa de la escena desde otros scripts.
        /// </summary>
        public void ForceClearAll()
        {
            ClearAllDiscs();
        }

        /// <summary>
        /// Prueba la conectividad entre los dos primeros nodos de la topologia
        /// y registra el resultado en la consola.
        /// </summary>
        private void TestConnectivity()
        {
            var topology = Object.FindAnyObjectByType<Network.TopologyManager>();
            if (topology != null)
            {
                var nodes = topology.GetAllNodes();
                if (nodes.Count >= 2)
                {
                    bool connected = topology.CheckConnectivity(nodes[0].DiscId, nodes[1].DiscId);
                    UnityEngine.Debug.Log($"[DebugSim] Conectividad entre {nodes[0].Name} y {nodes[1].Name}: {connected}");
                }
            }
        }
    }
}