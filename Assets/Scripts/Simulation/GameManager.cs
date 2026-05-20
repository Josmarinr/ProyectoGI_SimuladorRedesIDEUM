using UnityEngine;
using SimRedes.Tangible;
using SimRedes.Network;
using SimRedes.Simulation;

namespace SimRedes
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Managers")]
        [SerializeField] private TangibleDiscManager tangibleManager;
        [SerializeField] private TopologyManager topologyManager;
        [SerializeField] private RoutingSimulator routingSimulator;

        [Header("UI")]
        [SerializeField] private GameObject mainCanvas;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            InitializeManagers();
        }

        private void InitializeManagers()
        {
            if (tangibleManager == null)
                tangibleManager = gameObject.AddComponent<TangibleDiscManager>();

            if (topologyManager == null)
                topologyManager = gameObject.AddComponent<TopologyManager>();

            if (routingSimulator == null)
                routingSimulator = new RoutingSimulator();

            UnityEngine.Debug.Log("[GameManager] Todos los sistemas inicializados");
        }

        public TopologyManager GetTopologyManager()
        {
            return topologyManager;
        }

        public RoutingSimulator GetRoutingSimulator()
        {
            return routingSimulator;
        }

        public void ResetSimulation()
        {
            tangibleManager?.ClearAllDiscs();
            topologyManager?.ClearTopology();
            routingSimulator?.ClearAllTables();
            UnityEngine.Debug.Log("[GameManager] Simulación reiniciada");
        }
    }
}