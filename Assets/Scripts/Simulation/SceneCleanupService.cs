using UnityEngine;
using SimRedes.Network;
using SimRedes.Tangible;
using SimRedes.UI;

namespace SimRedes.Simulation
{
    public class SceneCleanupService : MonoBehaviour
    {
        public static SceneCleanupService Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void ClearSimulation(TopologyManager topology, NodeVisualizer visualizer, DebugDiscSimulator discSim)
        {
            if (discSim != null) discSim.ForceClearAll();
            if (topology != null) topology.ClearTopology();
            if (visualizer != null)
            {
                if (visualizer.nodeContainer != null)
                    foreach (Transform child in visualizer.nodeContainer)
                        Object.Destroy(child.gameObject);
                if (visualizer.linkContainer != null)
                    foreach (Transform child in visualizer.linkContainer)
                        Object.Destroy(child.gameObject);
            }
        }

        public void GoBackToMainMenu(Transform canvasTransform, System.Action createMainMenuCallback)
        {
            var managers = Object.FindObjectsOfType<MonoBehaviour>();
            foreach (var m in managers)
            {
                if (m is TopologyManager || m is TangibleDiscManager ||
                    m is NodeVisualizer || m is PingVisualizer ||
                    m is SimulationControls || m is ConnectivityTestPanel ||
                    m is DebugDiscSimulator || m is ScoringSystem ||
                    m is BuildTopologyActivity || m is FindFaultActivity ||
                    m is RoutingTablesActivity || m is StaticRoutingActivity ||
                    m is DynamicRoutingActivity || m is BestRouteActivity)
                    Object.Destroy(m.gameObject);
            }

            string[] panelNames = { "TopologyInfoPanel", "DevicesPanel", "ScorePanel", "ScenarioInfoPanel",
                "IPConfigPanel",                 "IPConfigBackground", "StatusPanel", "NetworkAdvancedPanel",
                "ConnectivityPanel", "BuildTopologyInfoPanel", "ScenariosPanel",
                "BestRoutePanel", "RoutingTablesPanel", "StaticRoutingPanel",
                "DynamicRoutingPanel", "ActivitiesPanel", "MainMenuPanel", "InstructionsPanel",
                "ClickOutsideBG" };
            foreach (var name in panelNames)
            {
                var obj = GameObject.Find(name);
                if (obj != null) Object.Destroy(obj);
            }

            var canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
                if (scaler == null) scaler = canvas.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(4096, 2160);
                scaler.matchWidthOrHeight = 0.5f;
            }

            createMainMenuCallback?.Invoke();
        }

        public void ExitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
