using UnityEngine;
using SimRedes.Network;
using SimRedes.UI;

namespace SimRedes.Tangible
{
    public class DebugDiscSimulator : MonoBehaviour
    {
        [Header("Simulation Settings")]
        [SerializeField] private bool enableSimulation = false;
        [SerializeField] private KeyCode addRouterKey = KeyCode.Alpha1;
        [SerializeField] private KeyCode addSwitchKey = KeyCode.Alpha2;
        [SerializeField] private KeyCode addPCKey = KeyCode.Alpha3;
        [SerializeField] private KeyCode addEnlaceKey = KeyCode.Alpha4;
        [SerializeField] private KeyCode addFalloKey = KeyCode.Alpha5;
        [SerializeField] private KeyCode addProtocoloKey = KeyCode.Alpha6;
        [SerializeField] private KeyCode clearKey = KeyCode.C;
        [SerializeField] private KeyCode pingTestKey = KeyCode.P;

        private int uniqueIdCounter = 1;
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

        private void Update()
        {
            if (!enableSimulation) return;

            if (Input.GetKeyDown(addRouterKey))
                SimulateDiscAt(1, GetNextPosition());

            else if (Input.GetKeyDown(addSwitchKey))
                SimulateDiscAt(2, GetNextPosition());

            else if (Input.GetKeyDown(addPCKey))
                SimulateDiscAt(3, GetNextPosition());

            else if (Input.GetKeyDown(addEnlaceKey))
            {
                var linkMode = FindObjectOfType<LinkModeController>();
                if (linkMode != null)
                    linkMode.ToggleLinkMode("connect");
                else
                    UnityEngine.Debug.Log("[DebugSim] LinkModeController no encontrado, no se puede activar modo conexion");
            }

            else if (Input.GetKeyDown(addFalloKey))
                SimulateDiscAt(5, GetNextPosition());

            else if (Input.GetKeyDown(addProtocoloKey))
                SimulateDiscAt(6, GetNextPosition());

            else if (Input.GetKeyDown(clearKey))
                ClearAllDiscs();

            else if (Input.GetKeyDown(pingTestKey))
                TestConnectivity();
        }

        private Vector2 GetNextPosition()
        {
            Vector2 pos = spawnPositions[positionIndex % spawnPositions.Length];
            positionIndex++;
            return pos;
        }

        private void SimulateDiscAt(int discId, Vector2 position)
        {
            var manager = FindObjectOfType<TangibleDiscManager>();
            if (manager != null)
            {
                manager.SimulateDiscPlaced(discId, position);
                UnityEngine.Debug.Log($"[DebugSim] Disco simulado: {discId} en {position}");
                UpdateStatusText(discId);
            }
        }

        private void UpdateStatusText(int discId)
        {
            var config = DiscConfiguration.GetConfiguration(discId);
            var topology = FindObjectOfType<Network.TopologyManager>();
            if (topology != null)
            {
                var statusTexts = FindObjectsOfType<UnityEngine.UI.Text>();
                foreach (var text in statusTexts)
                {
                    if (text.name == "StatusText")
                    {
                        text.text = $"Disco: {config.Label}\n{topology.GetTopologySummary()}";
                    }
                }
            }
        }

        private void ClearAllDiscs()
        {
            var manager = FindObjectOfType<TangibleDiscManager>();
            if (manager != null)
            {
                manager.ClearAllDiscs();
            }

            var topology = FindObjectOfType<Network.TopologyManager>();
            if (topology != null)
            {
                topology.ClearTopology();
            }

            var visualizer = FindObjectOfType<NodeVisualizer>();
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

        public void ForceClearAll()
        {
            ClearAllDiscs();
        }

        private void TestConnectivity()
        {
            var topology = FindObjectOfType<Network.TopologyManager>();
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